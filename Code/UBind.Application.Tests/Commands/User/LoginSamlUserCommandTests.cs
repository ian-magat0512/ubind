// <copyright file="LoginSamlUserCommandTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.User
{
    using System.Data.Entity;
    using System.Transactions;
    using ComponentSpace.Saml2;
    using ComponentSpace.Saml2.Assertions;
    using FluentAssertions;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NodaTime;
    using UBind.Application.Commands.User;
    using UBind.Application.Models.User;
    using UBind.Application.Queries.Portal;
    using UBind.Application.Services;
    using UBind.Application.Tests.Fakes;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Redis;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence;
    using Xunit;
    using IUserService = UBind.Application.User.IUserService;
    using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;
    using UserService = UBind.Application.User.UserService;

    public class LoginSamlUserCommandTests
    {
        private readonly Mock<ICachingResolver> cachingResolverMock = new Mock<ICachingResolver>();
        private readonly Mock<IUserReadModelRepository> userReadModelRepositoryMock
            = new Mock<IUserReadModelRepository>();
        private readonly Mock<IUserAggregateRepository> userAggregateRepositoryMock
            = new Mock<IUserAggregateRepository>();
        private readonly Mock<IOrganisationReadModelRepository> organisationReadModelRepositoryMock
            = new Mock<IOrganisationReadModelRepository>();
        private readonly Mock<IOrganisationAggregateRepository> organisationAggregateRepositoryMock
            = new Mock<IOrganisationAggregateRepository>();
        private readonly Mock<ICqrsMediator> mediatorMock = new Mock<ICqrsMediator>();
        private readonly Mock<ILogger<LoginSamlUserCommandHandler>> loggerMock
            = new Mock<ILogger<LoginSamlUserCommandHandler>>();
        private readonly Mock<IOrganisationService> organisationServiceMock = new Mock<IOrganisationService>();
        private readonly Mock<IAccessTokenService> accessTokenServiceMock = new Mock<IAccessTokenService>();
        private readonly Mock<IRoleRepository> roleRepositoryMock = new Mock<IRoleRepository>();
        private readonly Mock<IUserLoginEmailRepository> userLoginEmailRepositoryMock
            = new Mock<IUserLoginEmailRepository>();
        private readonly Mock<IAuthenticationMethodReadModelRepository> authenticationMethodReadModelRepositoryMock
            = new Mock<IAuthenticationMethodReadModelRepository>();
        private readonly IUserService userService;
        private readonly IClock clock;

        private SamlAuthenticationMethodReadModel samlAuthenticationMethod1;
        private SamlAuthenticationMethodReadModel samlAuthenticationMethod2;
        private Guid managingOrganisationId = Guid.NewGuid();
        private Guid defaultAgentRoleId;
        private Guid tenantId = Guid.NewGuid();
        private Guid defaultOrganisationId = Guid.NewGuid();
        private Guid provisionedOrganisationId = Guid.NewGuid();
        private Guid someOtherOrganisationId = Guid.NewGuid();
        private Guid customerRoleId;
        private Guid brokerRoleId;
        private Mock<ISpSsoResult> ssoResultMock;
        private LoginSamlUserCommandHandler handler;
        private SamlAttribute multiRoleAttribute;
        private SamlAttribute delimitedRoleAttribute;
        private OrganisationReadModel provisionedOrganisation;
        private OrganisationReadModel managingOrganisation;
        private OrganisationReadModel someOtherOrganisation;
        private UserReadModel user;
        private UserAggregate userAggregate;
        private SamlSessionData samlSessionData;
        private OrganisationAggregate organisationAggregate;

        public LoginSamlUserCommandTests()
        {
            this.clock = new TestClock();
            var now = this.clock.Now();

            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);

            var tenant = TenantFactory.Create(this.tenantId, "test-tenant");
            var customerRole = new Role(
                this.tenantId,
                this.managingOrganisationId,
                DefaultRole.Customer,
                now);
            this.customerRoleId = customerRole.Id;
            this.roleRepositoryMock.Setup(s => s.GetCustomerRoleForTenant(It.IsAny<Guid>()))
                .Returns(customerRole);
            this.roleRepositoryMock.Setup(s => s.GetRoleById(It.IsAny<Guid>(), this.customerRoleId))
                .Returns(customerRole);
            var brokerRole = new Role(
                this.tenantId,
                this.managingOrganisationId,
                RoleType.Client,
                "Broker",
                "Broker",
                now,
                false);
            this.brokerRoleId = brokerRole.Id;
            this.roleRepositoryMock.Setup(s => s.GetRoleById(It.IsAny<Guid>(), this.brokerRoleId))
                .Returns(brokerRole);
            var defaultAgentRole = new Role(
                this.tenantId,
                this.managingOrganisationId,
                RoleType.Client,
                "Broker",
                "Broker",
                now,
                false);
            this.defaultAgentRoleId = defaultAgentRole.Id;
            this.roleRepositoryMock.Setup(s => s.GetRoleById(It.IsAny<Guid>(), this.defaultAgentRoleId))
                .Returns(defaultAgentRole);

            this.samlAuthenticationMethod1 = new SamlAuthenticationMethodReadModel
            {
                // Common to all authentication methods
                OrganisationId = this.managingOrganisationId,
                Name = "SAML1",
                CanCustomersSignIn = true,
                CanAgentsSignIn = true,
                IncludeSignInButtonOnPortalLoginPage = true,
                SignInButtonBackgroundColor = "green",
                SignInButtonIconUrl = null,
                SignInButtonLabel = "Sign in with SAML1",
                Disabled = false,

                // Saml specific
                IdentityProviderEntityIdentifier = "https://saml1.example.com",
                IdentityProviderSingleSignOnServiceUrl = "https://saml1.example.com/sso",
                IdentityProviderSingleLogoutServiceUrl = "https://saml1.example.com/slo",
                MustSignAuthenticationRequests = true,
                ShouldLinkExistingCustomerWithSameEmailAddress = true,
                CanCustomerAccountsBeAutoProvisioned = true,
                CanCustomerDetailsBeAutoUpdated = true,
                ShouldLinkExistingAgentWithSameEmailAddress = true,
                CanAgentAccountsBeAutoProvisioned = true,
                CanAgentDetailsBeAutoUpdated = true,
                CanUsersOfManagedOrganisationsSignIn = true,
                ShouldLinkExistingOrganisationWithSameAlias = true,
                CanOrganisationsBeAutoProvisioned = true,
                CanOrganisationDetailsBeAutoUpdated = true,
                NameIdFormat = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress",
                UseNameIdAsUniqueIdentifier = true,
                UseNameIdAsEmailAddress = true,
                UniqueIdentifierAttributeName = "emailAddress",
                FirstNameAttributeName = "firstName",
                LastNameAttributeName = "lastName",
                EmailAddressAttributeName = "emailAddress",
                PhoneNumberAttributeName = "phoneNumber",
                MobileNumberAttributeName = "mobileNumber",
                UserTypeAttributeName = "userType",
                OrganisationUniqueIdentifierAttributeName = "organisationUniqueIdentifier",
                OrganisationNameAttributeName = "organisationName",
                OrganisationAliasAttributeName = "organisationAlias",
                RoleAttributeName = "role",
                RoleAttributeValueDelimiter = ";",
                DefaultAgentRoleId = this.defaultAgentRoleId,
                RoleMap = new Dictionary<string, Guid>
                {
                    { "admin", Guid.NewGuid() },
                    { "broker", this.brokerRoleId },
                    { "claims handler", Guid.NewGuid() },
                    { "assessor", Guid.NewGuid() },
                    { "underwriter", Guid.NewGuid() },
                },
                AreRolesManagedExclusivelyByThisIdentityProvider = false,
            };

            this.samlAuthenticationMethod2 = new SamlAuthenticationMethodReadModel
            {
                // Common to all authentication methods
                OrganisationId = this.managingOrganisationId,
                Name = "SAML2",
                CanCustomersSignIn = true,
                CanAgentsSignIn = true,
                IncludeSignInButtonOnPortalLoginPage = true,
                SignInButtonBackgroundColor = "purple",
                SignInButtonIconUrl = null,
                SignInButtonLabel = "Sign in with SAML2",
                Disabled = false,

                // Saml specific
                IdentityProviderEntityIdentifier = "https://saml2.example.com",
                IdentityProviderSingleSignOnServiceUrl = "https://saml2.example.com/sso",
                IdentityProviderSingleLogoutServiceUrl = "https://saml2.example.com/slo",
                MustSignAuthenticationRequests = true,
                ShouldLinkExistingCustomerWithSameEmailAddress = true,
                CanCustomerAccountsBeAutoProvisioned = true,
                CanCustomerDetailsBeAutoUpdated = true,
                ShouldLinkExistingAgentWithSameEmailAddress = true,
                CanAgentAccountsBeAutoProvisioned = true,
                CanAgentDetailsBeAutoUpdated = true,
                CanUsersOfManagedOrganisationsSignIn = true,
                ShouldLinkExistingOrganisationWithSameAlias = true,
                CanOrganisationsBeAutoProvisioned = true,
                CanOrganisationDetailsBeAutoUpdated = true,
                NameIdFormat = "urn:oasis:names:tc:SAML:1.1:nameid-format:emailAddress",
                UseNameIdAsUniqueIdentifier = true,
                UseNameIdAsEmailAddress = true,
                UniqueIdentifierAttributeName = "emailAddress",
                FirstNameAttributeName = "firstName",
                LastNameAttributeName = "lastName",
                EmailAddressAttributeName = "emailAddress",
                PhoneNumberAttributeName = "phoneNumber",
                MobileNumberAttributeName = "mobileNumber",
                UserTypeAttributeName = "userType",
                OrganisationUniqueIdentifierAttributeName = "organisationUniqueIdentifier",
                OrganisationNameAttributeName = "organisationName",
                OrganisationAliasAttributeName = "organisationAlias",
                RoleAttributeName = "role",
                RoleAttributeValueDelimiter = ";",
                DefaultAgentRoleId = null,
                RoleMap = new Dictionary<string, Guid>
                {
                    { "admin", Guid.NewGuid() },
                    { "broker", this.brokerRoleId },
                    { "claims handler", Guid.NewGuid() },
                    { "assessor", Guid.NewGuid() },
                    { "underwriter", Guid.NewGuid() },
                },
                AreRolesManagedExclusivelyByThisIdentityProvider = true,
            };

            this.ssoResultMock = new Mock<ISpSsoResult>();
            this.ssoResultMock.SetupGet(x => x.UserID).Returns("xTestUserId");

            this.ssoResultMock.SetupGet(x => x.Attributes).Returns(
                new List<SamlAttribute>
                {
                    new SamlAttribute("emailAddress", "johndoe@abcbrokers.com"),
                    new SamlAttribute("firstName", "John"),
                    new SamlAttribute("lastName", "Doe"),
                    new SamlAttribute("phoneNumber", "0123456789"),
                    new SamlAttribute("mobileNumber", "0123456789"),
                    new SamlAttribute("userType", "agent"),
                    new SamlAttribute("organisationUniqueIdentifier", "123456789"),
                    new SamlAttribute("organisationName", "ABC Brokers"),
                    new SamlAttribute("organisationAlias", "abcbrokers"),
                });

            // we'll create these and individual tests can add them as they please.
            this.multiRoleAttribute = new SamlAttribute("role", "claims handler");
            this.multiRoleAttribute.AttributeValues.Add(new AttributeValue("assessor"));
            this.delimitedRoleAttribute = new SamlAttribute("role", "broker;underwriter");

            this.user = new UserReadModel(
                Guid.NewGuid(),
                new Domain.Aggregates.Person.PersonData(),
                null,
                Guid.NewGuid(),
                now,
                UserType.Client);
            this.user.OrganisationId = this.provisionedOrganisationId;
            this.user.Roles.Add(brokerRole);
            this.userReadModelRepositoryMock.Setup(s => s.GetUser(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(this.user);
            var personAggregate = PersonAggregate.CreatePerson(
                this.tenantId,
                this.user.OrganisationId,
                null,
                now);
            this.userAggregate = UserAggregate.CreateUser(
                this.tenantId,
                this.user.Id,
                this.user.UserType.ToEnumOrThrow<UserType>(),
                personAggregate,
                null,
                this.user.PortalId,
                now,
                this.user.Roles.Select(r => r.Id).ToArray());
            this.userAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(this.userAggregate)
                .Callback<Guid, Guid>((tenantId, userId) =>
                {
                    System.Diagnostics.Debug.WriteLine("GetById called on userAggregateRepositoryMock");
                });

            this.userService = this.GetUserService(this.tenantId, this.user.Id);

            this.mediatorMock.Setup(s => s.Send(It.IsAny<GetDefaultPortalIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());
            this.mediatorMock.Setup(s => s.Send(It.IsAny<GetPortalUrlQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://app.ubind.io/portal/test/");

            this.handler = new LoginSamlUserCommandHandler(
                this.cachingResolverMock.Object,
                this.userReadModelRepositoryMock.Object,
                this.userAggregateRepositoryMock.Object,
                this.organisationReadModelRepositoryMock.Object,
                this.organisationAggregateRepositoryMock.Object,
                this.mediatorMock.Object,
                this.loggerMock.Object,
                this.organisationServiceMock.Object,
                this.accessTokenServiceMock.Object,
                this.roleRepositoryMock.Object,
                this.userLoginEmailRepositoryMock.Object,
                this.clock,
                this.authenticationMethodReadModelRepositoryMock.Object,
                this.userService);

            this.provisionedOrganisation = new OrganisationReadModel(
                this.tenantId,
                this.provisionedOrganisationId,
                "provisioned-organisation",
                "Provisioned Organisation",
                this.managingOrganisationId,
                true,
                false,
                now);
            this.managingOrganisation = new OrganisationReadModel(
                this.tenantId,
                this.managingOrganisationId,
                "managing-organisation",
                "Managing Organisation",
                null,
                true,
                false,
                now);
            this.someOtherOrganisation = new OrganisationReadModel(
                this.tenantId,
                this.someOtherOrganisationId,
                "some-other-organisation",
                "Some Other Organisation",
                null,
                true,
                false,
                now);
            this.cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), this.provisionedOrganisationId))
                .Returns(Task.FromResult(this.provisionedOrganisation));
            this.cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), this.managingOrganisationId))
                .Returns(Task.FromResult(this.managingOrganisation));
            this.cachingResolverMock.Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), this.someOtherOrganisationId))
                .Returns(Task.FromResult(this.someOtherOrganisation));
            this.cachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<Guid?>()))
                .ReturnsAsync(tenant);

            var portal = new PortalReadModel()
            {
                Name = "Test Portal",
                Id = Guid.NewGuid(),
                Alias = "test-portal",
                TenantId = this.tenantId,
                Title = "Test Portal",
                UserType = PortalUserType.Agent,
                OrganisationId = this.provisionedOrganisationId,
                IsDefault = true,
            };
            this.cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(portal);

            this.samlSessionData = new SamlSessionData
            {
                Issuer = "Test Issuer",
                SessionIndex = "1234",
                NameId = "PersonNameId",
            };

            IReadOnlyList<Permission> permissionList = new List<Permission>();
            this.mediatorMock.Setup(m => m.Send(It.IsAny<IQuery<IReadOnlyList<Permission>>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(permissionList));

            this.organisationAggregate = OrganisationAggregate.CreateNewOrganisation(
                this.tenantId, "provisioned-orgainsation", "Provisioned Organisation", null, null, this.clock.Now());
            this.organisationAggregateRepositoryMock.Setup(s => s.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(this.organisationAggregate);
        }

        [Fact]
        public async Task CustomerSignIn_GetsCustomerRoleAssigned()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);
            this.samlAuthenticationMethod1.CanAgentsSignIn = false;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.user.UserType = UserType.Customer.Humanize();
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns(this.provisionedOrganisation);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            var user = result.User;
            user.Roles.Should().HaveCount(1);
            user.Roles.First().Id.Should().Be(this.customerRoleId);
        }

        [Fact]
        public async Task SigningIn_GeneratesError_WhenUsersOrganisationIsDisabled()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.provisionedOrganisation.IsActive = false;
            this.userReadModelRepositoryMock.Setup(s => s.GetLinkedUser(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(this.user);
            this.organisationServiceMock.Setup(s => s.ValidateOrganisationIsActive(
                It.IsAny<OrganisationReadModel>(), It.IsAny<Guid>()))
                .Callback<OrganisationReadModel, Guid>((organisation, organisationId) =>
                {
                    if (!organisation.IsActive)
                    {
                        throw new ErrorException(
                            new Error("login.organisation.with.alias.disabled", "Organisation is disabled", "Test message"));
                    }
                });
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns(this.provisionedOrganisation);

            // Act
            Func<Task<UserLoginResult>> act = () => this.handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("login.organisation.with.alias.disabled");
        }

        /// <summary>
        /// This test validates that if there is no organisation attribute name specified in the SAML configuration,
        /// the same organisation whichthe authentication method is defined against, is used as the assumed
        /// organisation of the new user.
        /// </summary>
        [Fact]
        public async Task AutoAccountProvisioning_AssumesAuthenticationMethodOrganisation_IfNoOrganisationAttributeNameSpecified()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);
            var attributeToRemove = this.ssoResultMock.Object.Attributes.FirstOrDefault(a => a.Name == "organisationUniqueIdentifier");
            this.ssoResultMock.Object.Attributes.Remove(attributeToRemove);
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            this.samlAuthenticationMethod1.OrganisationUniqueIdentifierAttributeName = null;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);

            // Act
            var result = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            result.User.OrganisationId.Should().Be(this.samlAuthenticationMethod1.OrganisationId);
        }

        [Fact]
        public async Task FirstTimeSignIn_LinksUserToExistingAccount_WhenConfiguredToDoSo()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.samlAuthenticationMethod1.ShouldLinkExistingAgentWithSameEmailAddress = true;
            this.userLoginEmailRepositoryMock.Setup(s => s.GetUserLoginByEmail(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new UserLoginEmail(
                    this.user.TenantId,
                    this.user.Id,
                    this.user.CreatedTimestamp,
                    this.user.OrganisationId,
                    this.user.LoginEmail));
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns(this.provisionedOrganisation);

            // Act
            var userLoginResult = await this.handler.Handle(command, CancellationToken.None);

            // Assert
            this.mediatorMock.Verify(
                m => m.Send(It.Is<ICommand<UserModel>>(r => r is CreateUserCommand), It.IsAny<CancellationToken>()),
                Times.Never());
        }

        [Fact]
        public async Task FirstTimeSignIn_LinkingUserToExistingtAccounFails_WhenOrganisationUniqueIdMismatch()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);

            // make sure the user that's found belongs to another org than the one asserted in the SAML response.
            this.user.OrganisationId = this.someOtherOrganisationId;
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.samlAuthenticationMethod1.ShouldLinkExistingAgentWithSameEmailAddress = true;
            this.userLoginEmailRepositoryMock.Setup(s => s.GetUserLoginByEmail(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new UserLoginEmail(
                    this.user.TenantId,
                    this.user.Id,
                    this.user.CreatedTimestamp,
                    this.user.OrganisationId,
                    this.user.LoginEmail));
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns(this.provisionedOrganisation);

            // Act
            Func<Task<UserLoginResult>> act = () => this.handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("saml.users.organisation.has.linked.identity.to.a.different.organisation");
        }

        [Fact]
        public async Task LinkedUserSignIn_Fails_WhenOrganisationUniqueIdMismatch()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);

            // make sure the user that's found belongs to another org than the one asserted in the SAML response.
            this.user.OrganisationId = this.someOtherOrganisationId;
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.samlAuthenticationMethod1.ShouldLinkExistingAgentWithSameEmailAddress = true;
            this.userReadModelRepositoryMock.Setup(s => s.GetLinkedUser(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(this.user);
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns(this.provisionedOrganisation);

            // Act
            Func<Task<UserLoginResult>> act = () => this.handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("saml.users.organisation.has.linked.identity.to.a.different.organisation");
        }

        [Fact]
        public async Task SignInSucceds_WhenUnlinkedOrganisationExistsAndAutoLinkingEnabled()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);

            // make sure the user that's found belongs to another org than the one asserted in the SAML response.
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            this.samlAuthenticationMethod1.CanAgentsSignIn = true;
            this.samlAuthenticationMethod1.ShouldLinkExistingAgentWithSameEmailAddress = true;
            this.samlAuthenticationMethod1.CanOrganisationsBeAutoProvisioned = false;
            this.samlAuthenticationMethod1.ShouldLinkExistingOrganisationWithSameAlias = true;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.userReadModelRepositoryMock.Setup(s => s.GetLinkedUser(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns((UserReadModel?)null);
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns((OrganisationReadModel?)null);
            this.organisationReadModelRepositoryMock.Setup(s => s.GetByAlias(It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(this.provisionedOrganisation);
            this.userLoginEmailRepositoryMock.Setup(s => s.GetUserLoginByEmail(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new UserLoginEmail(
                    this.user.TenantId,
                    this.user.Id,
                    this.user.CreatedTimestamp,
                    this.user.OrganisationId,
                    this.user.LoginEmail));

            // Act
            Func<Task<UserLoginResult>> act = () => this.handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task FirstTimeSignIn_GeneratesError_WhenAutoProvisioningIsDisabledAndNoAccountWithSameEmailExists()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            this.samlAuthenticationMethod1.CanAgentAccountsBeAutoProvisioned = false;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.samlAuthenticationMethod1.ShouldLinkExistingAgentWithSameEmailAddress = true;
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns(this.provisionedOrganisation);

            // Act
            Func<Task<UserLoginResult>> act = () => this.handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("saml.agent.account.auto.provisioning.is.disabled");
        }

        [Fact]
        public async Task LinkingToASecondIdentity_GeneratesError_WhenTheFirstHasExclusiveRoleManagement()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            this.samlAuthenticationMethod1.CanAgentAccountsBeAutoProvisioned = false;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.samlAuthenticationMethod1.ShouldLinkExistingAgentWithSameEmailAddress = true;
            this.userLoginEmailRepositoryMock.Setup(s => s.GetUserLoginByEmail(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(new UserLoginEmail(
                    this.user.TenantId,
                    this.user.Id,
                    this.user.CreatedTimestamp,
                    this.user.OrganisationId,
                    this.user.LoginEmail));
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns(this.provisionedOrganisation);
            this.user.LinkedIdentities.Add(new UserLinkedIdentityReadModel
            {
                TenantId = tenant.Id,
                UserId = this.user.Id,
                AuthenticationMethodId = this.samlAuthenticationMethod2.Id,
                AuthenticationMethodName = this.samlAuthenticationMethod2.Name,
                AuthenticationMethodTypeName = this.samlAuthenticationMethod2.TypeName,
                UniqueId = "saml2-user-uniqueId",
            });
            this.authenticationMethodReadModelRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), this.samlAuthenticationMethod1.Id))
                .Returns(this.samlAuthenticationMethod1);
            this.authenticationMethodReadModelRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), this.samlAuthenticationMethod2.Id))
                .Returns(this.samlAuthenticationMethod2);

            // Act
            Func<Task<UserLoginResult>> act = () => this.handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("saml.user.has.a.linked.identity.with.exclusive.role.management");
        }

        [Fact]
        public async Task AssigningRoles_DoesNotGenerateError_WhenRoleAssertionsAreUnmapped()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);
            this.samlAuthenticationMethod1.CanCustomersSignIn = false;
            this.samlAuthenticationMethod1.CanAgentAccountsBeAutoProvisioned = true;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod2,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.organisationReadModelRepositoryMock.Setup(
                s => s.GetLinkedOrganisation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), null))
                .Returns(this.provisionedOrganisation);
            this.ssoResultMock.Object.Attributes.Add(new SamlAttribute("role", "coffee maker"));
            this.ssoResultMock.Object.Attributes.Add(new SamlAttribute("role", "broker"));

            // Act
            Func<Task<UserLoginResult>> act = () => this.handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task SigningIn_GeneratesError_WhenAuthenticationMethodIsDisabled()
        {
            // Arrange
            var now = this.clock.Now();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                this.defaultOrganisationId,
                Guid.NewGuid(),
                now);
            this.samlAuthenticationMethod1.Disabled = true;
            var command = new LoginSamlUserCommand(
                tenant,
                this.samlAuthenticationMethod1,
                this.ssoResultMock.Object,
                this.samlSessionData);
            this.userReadModelRepositoryMock.Setup(s => s.GetLinkedUser(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(this.user);

            // Act
            Func<Task<UserLoginResult>> act = () => this.handler.Handle(command, CancellationToken.None);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("authentication.method.disabled");
        }

        private IUserService GetUserService(Guid tenantId, Guid userId)
        {
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockPersonRepository = new Mock<IPersonAggregateRepository>();
            mockPersonRepository.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(PersonAggregate.CreatePerson(tenantId, Guid.NewGuid(), Guid.NewGuid(), Instant.MaxValue));
            var personRepository = mockPersonRepository.Object;
            mockTenantRepository.Setup(x => x.GetTenantById(tenantId)).Returns(new Tenant(tenantId));
            var tenantRepository = mockTenantRepository.Object;
            var mockUbindDbContext = new Mock<IUBindDbContext>();
            mockUbindDbContext.SetupProperty(p => p.TransactionStack, new Stack<TransactionScope>());
            mockUbindDbContext.Setup(s => s.GetDbSet<EventRecordWithGuidId>())
                           .Returns(new Mock<IDbSet<EventRecordWithGuidId>>().Object);
            mockUbindDbContext.Setup(s => s.GetContextAggregates<UserAggregate>())
                           .Returns(new HashSet<UserAggregate>());
            var clock = new TestClock(true);
            var userReadModelUpdateRepositoryFake = new FakeWritableReadModelRepository<UserReadModel>();
            var userLoginEmailUpdateRepositoryFake = new FakeWritableReadModelRepository<UserLoginEmail>();
            var propertyTypeEvaluatorService = new PropertyTypeEvaluatorService(
                Mock.Of<IReadOnlyDictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor>>());
            var userReadModelWriter = new UserReadModelWriter(
                userReadModelUpdateRepositoryFake,
                userLoginEmailUpdateRepositoryFake,
                Mock.Of<IUserLoginEmailRepository>(),
                this.roleRepositoryMock.Object,
                propertyTypeEvaluatorService);
            var userAggregateRepository = new UserAggregateRepository(
                mockUbindDbContext.Object,
                new Mock<IEventRecordRepository>().Object,
                userReadModelWriter,
                new Mock<IAggregateSnapshotService<UserAggregate>>().Object,
                clock,
                Mock.Of<ILogger<UserAggregateRepository>>(),
                new Mock<IServiceProvider>().AddLoggers().Object);
            this.userAggregateRepositoryMock.Setup(s => s.ApplyChangesToDbContext(It.IsAny<UserAggregate>()))
                .Callback<UserAggregate>(async a => await userAggregateRepository.ApplyChangesToDbContext(a));
            var userService = new UserService(
                this.userAggregateRepositoryMock.Object,
                new Mock<ICustomerAggregateRepository>().Object,
                personRepository,
                new Mock<IUserReadModelRepository>().Object,
                this.roleRepositoryMock.Object,
                new Mock<IUserProfilePictureRepository>().Object,
                new Mock<IOrganisationReadModelRepository>().Object,
                new Mock<IUserLoginEmailRepository>().Object,
                new Mock<IPasswordHashingService>().Object,
                new Mock<ICustomerService>().Object,
                new Mock<IHttpContextPropertiesResolver>().Object,
                new Mock<IQuoteAggregateResolverService>().Object,
                new Mock<IUserActivationInvitationService>().Object,
                new Mock<ICqrsMediator>().Object,
                new Mock<IAdditionalPropertyValueService>().Object,
                new Mock<IPasswordComplexityValidator>().Object,
                new Mock<IClock>().Object,
                new Mock<IUBindDbContext>().Object,
                this.cachingResolverMock.Object,
                new Mock<IAuthenticationMethodReadModelRepository>().Object,
                new Mock<IUserSessionDeletionService>().Object,
                Mock.Of<IUserSystemEventEmitter>());

            return userService;
        }
    }
}
