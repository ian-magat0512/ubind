// <copyright file="UserAuthorisationServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Authorisation
{
    using System;
    using System.Net;
    using System.Security.Claims;
    using FluentAssertions;
    using Humanizer;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using UBind.Application.Authorisation;
    using UBind.Application.Infrastructure;
    using UBind.Application.Queries.User;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Services;
    using Xunit;

    public class UserAuthorisationServiceTests
    {

        /// <summary>
        /// Defines the currentTenant.
        /// </summary>
        private readonly Guid currentTenantId = Guid.NewGuid();

        /// <summary>
        /// Defines the currentOrganisation.
        /// </summary>
        private readonly Guid currentOrganisationId = Guid.NewGuid();

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid userId = new Guid("A1B2A3B4-3333-2222-1111-000000000000");

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid customerId = new Guid("FA449334-3333-2222-1111-000000000000");

        private IAuthorisationService authorisationService;
        private IUserAuthorisationService userAuthorisationService;
        private Mock<IOrganisationService> organisationService = new Mock<IOrganisationService>();
        private Mock<User.IUserService> userService = new Mock<User.IUserService>();
        private Mock<ICustomerService> customerService = new Mock<ICustomerService>();
        private Mock<IRoleService> roleService = new Mock<IRoleService>();
        private Mock<ICqrsMediator> mediator = new Mock<ICqrsMediator>();
        private Mock<IPolicyReadModelRepository> policyReadModelRepository = new Mock<IPolicyReadModelRepository>();
        private Mock<IClaimReadModelRepository> claimReadModelRepository = new Mock<IClaimReadModelRepository>();
        private Mock<IQuoteReadModelRepository> quoteReadModelRepository = new Mock<IQuoteReadModelRepository>();
        private Mock<ICachingResolver> cachingResolver = new Mock<ICachingResolver>();
        private Mock<IQuoteAggregateResolverService> quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        private Mock<IDkimSettingRepository> dkimSettingRepository = new Mock<IDkimSettingRepository>();
        private Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolver = new Mock<IHttpContextPropertiesResolver>();
        private Mock<IReportReadModelRepository> reportReadModelRepository = new Mock<IReportReadModelRepository>();
        private Mock<IUserSessionService> userSessionServiceMock = new Mock<IUserSessionService>();

        public UserAuthorisationServiceTests()
        {
            var orgReadModelSummary = new Mock<IOrganisationReadModelSummary>();
            orgReadModelSummary.Setup(o => o.TenantId).Returns(this.currentTenantId);
            this.organisationService.Setup(o => o.GetOrganisationSummaryForTenantIdAndOrganisationId(
                It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(orgReadModelSummary.Object);

            this.authorisationService = this.GetAuthorisationService();
            this.userAuthorisationService = this.GetUserAuthorisationService();
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);
        }

        [Fact]
        public async Task ThrowIfUserCannotViewUser_Throws_WhenUserDoesntHavePermission()
        {
            // Arrange
            var claimsPrincipal = this.GetClaimsPrincipal();
            string[] permissions = new string[]
            {
                Permission.ManageTenantAdminUsers.ToString(),
                Permission.ViewUsers.ToString(),
            };
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(true);
            this.authorisationService = this.GetAuthorisationService();
            this.userAuthorisationService = this.GetUserAuthorisationService();

            // Act
            Func<Task> act = async () => await this.userAuthorisationService.ThrowIfUserCannotView(
                this.currentTenantId, Guid.NewGuid(), claimsPrincipal);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.Code.Should().Be("authorisation.permission.required.to.view.user");
        }

        [Fact]
        public async Task ThrowIfUserCannotCreateUsers_Throws_WhenUserDoesntHaveManageUsersPermission()
        {
            // Arrange
            string[] permissions = new string[]
            {
                Permission.ManageOrganisations.ToString(),
                Permission.ManageTenants.ToString(),
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, null, null, Guid.NewGuid(), null, permissions);

            // Act
            Func<Task> act = async () => await this.userAuthorisationService.ThrowIfUserCannotCreate(
                this.currentTenantId, Guid.NewGuid(), claimsPrincipal);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotCreateUsers_Throws_WhenUserInDifferentOrgAndPerformingUserIsNotInDefaultOrg()
        {
            // Arrange
            string[] permissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, null, null, Guid.NewGuid(), null, permissions);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await this.userAuthorisationService.ThrowIfUserCannotCreate(
                this.currentTenantId, Guid.NewGuid(), claimsPrincipal);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ThrowIfUserCannotModifyUser_Throws_WhenUserHasManageClientAdminsPermissionAndPerformingUserDoesnt()
        {
            // Arrange
            string[] permissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, null, null, null, null, permissions);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var role = new Role(default, default, DefaultRole.TenantAdmin, default);
            var personData = new Domain.Aggregates.Person.PersonData();
            personData.TenantId = this.currentTenantId;
            personData.OrganisationId = this.currentOrganisationId;
            var user = new UserReadModel(
                Guid.NewGuid(),
                personData,
                null,
                null,
                default,
                UserType.Client);
            user.Roles.Add(role);
            this.mediator.Setup(s => s.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            // Act
            Func<Task> act = async () => await this.userAuthorisationService.ThrowIfUserCannotModify(
                this.currentTenantId, Guid.NewGuid(), claimsPrincipal);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.Code.Equals("user.authorisation.cannot.modify.elevated.user");
        }

        [Fact]
        public async Task ThrowIfUserCannotModifyUser_Throws_WhenUserHasManageOrgAdminsPermissionAndPerformingUserDoesnt()
        {
            // Arrange
            string[] permissions = new string[]
            {
                Permission.ManageUsers.ToString(),
            };
            var claimsPrincipal = this.GetClaimsPrincipal(null, null, null, null, null, permissions);
            this.organisationService.Setup(o => o.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(false);
            var role = new Role(default, default, DefaultRole.OrganisationAdmin, default);
            var personData = new Domain.Aggregates.Person.PersonData();
            personData.TenantId = this.currentTenantId;
            personData.OrganisationId = this.currentOrganisationId;
            var user = new UserReadModel(
                Guid.NewGuid(),
                personData,
                null,
                null,
                default,
                UserType.Client);
            user.Roles.Add(role);
            this.mediator.Setup(s => s.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(user));

            // Act
            Func<Task> act = async () => await this.userAuthorisationService.ThrowIfUserCannotModify(
                this.currentTenantId, Guid.NewGuid(), claimsPrincipal);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.And.Error.Code.Equals("user.authorisation.cannot.modify.elevated.user");
        }

        private ClaimsPrincipal GetClaimsPrincipal(
            Guid? userId = null,
            UserType? userType = null,
            Guid? tenantId = null,
            Guid? organisationId = null,
            Guid? customerId = null,
            string[] permissions = null)
        {
            userId = userId ?? this.userId;
            userType = userType ?? UserType.Client;
            tenantId = tenantId ?? this.currentTenantId;
            organisationId = organisationId ?? this.currentOrganisationId;
            customerId = customerId ?? this.customerId;
            permissions = permissions ?? Array.Empty<string>();
            return new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, userType.Humanize()),
                new Claim("Tenant", tenantId.ToString()),
                new Claim("OrganisationId", organisationId.ToString()),
                new Claim("CustomerId", customerId.ToString()),
                new Claim(
                    ClaimNames.Permissions,
                    JsonConvert.SerializeObject(
                        permissions,
                        new StringEnumConverter(new CamelCaseNamingStrategy()))),
            }));
        }

        private AuthorisationService GetAuthorisationService()
        {
            var authorisationService = new AuthorisationService(
            this.organisationService.Object,
            this.userService.Object,
            this.customerService.Object,
            this.roleService.Object,
            this.policyReadModelRepository.Object,
            this.claimReadModelRepository.Object,
            this.quoteReadModelRepository.Object,
            this.mediator.Object,
            this.cachingResolver.Object,
            this.quoteAggregateResolverService.Object,
            this.dkimSettingRepository.Object,
            this.httpContextPropertiesResolver.Object,
            this.reportReadModelRepository.Object,
            this.userSessionServiceMock.Object);
            return authorisationService;
        }

        private UserAuthorisationService GetUserAuthorisationService()
        {
            var authorisationService = new UserAuthorisationService(
                this.mediator.Object);
            return authorisationService;
        }
    }
}
