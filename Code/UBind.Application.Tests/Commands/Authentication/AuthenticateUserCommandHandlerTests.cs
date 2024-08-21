// <copyright file="AuthenticateUserCommandHandlerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Commands.Authentication;

using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using UBind.Application.Commands.Authentication;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Aggregates;
using UBind.Domain.Authentication;
using UBind.Domain.ReadModel.User;
using UBind.Domain.ReadModel;
using UBind.Domain.Repositories;
using UBind.Domain.Services;
using UBind.Domain;
using Xunit;
using NodaTime;
using UBind.Domain.Tests.Fakes;
using UBind.Domain.Aggregates.Organisation;
using UBind.Domain.Extensions;
using UBind.Domain.Aggregates.Person;
using FluentAssertions;
using UBind.Domain.Exceptions;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.ReadModel.Portal;

public class AuthenticateUserCommandHandlerTests
{
    private readonly Mock<IUserReadModelRepository> userReadModelRepositoryMock;
    private readonly Mock<IUserLoginEmailRepository> userLoginEmailRepositoryMock;
    private readonly Mock<IOrganisationService> organisationServiceMock;
    private readonly Mock<ILoginAttemptTrackingService> loginAttemptTrackingServiceMock;
    private readonly Mock<ICachingResolver> cachingResolverMock;
    private readonly Mock<IPasswordHashingService> passwordHashingServiceMock;
    private readonly Mock<IUserAggregateRepository> userAggregateRepositoryMock;
    private readonly Mock<ITenantRepository> tenantRepositoryMock;
    private readonly Mock<IOrganisationReadModelRepository> organisationReadModelRepositoryMock;
    private readonly Mock<IHttpContextPropertiesResolver> httpContextPropertiesResolverMock;
    private Guid? performingUserId = Guid.NewGuid();
    private TestClock clock = new TestClock();
    private Guid tenantId = Guid.NewGuid();
    private Guid provisionedOrganisationId = Guid.NewGuid();

    public AuthenticateUserCommandHandlerTests()
    {
        this.userReadModelRepositoryMock = new Mock<IUserReadModelRepository>();
        this.userLoginEmailRepositoryMock = new Mock<IUserLoginEmailRepository>();
        this.organisationServiceMock = new Mock<IOrganisationService>();
        this.loginAttemptTrackingServiceMock = new Mock<ILoginAttemptTrackingService>();
        this.cachingResolverMock = new Mock<ICachingResolver>();
        this.passwordHashingServiceMock = new Mock<IPasswordHashingService>();
        this.userAggregateRepositoryMock = new Mock<IUserAggregateRepository>();
        this.tenantRepositoryMock = new Mock<ITenantRepository>();
        this.organisationReadModelRepositoryMock = new Mock<IOrganisationReadModelRepository>();
        this.httpContextPropertiesResolverMock = new Mock<IHttpContextPropertiesResolver>();
        this.httpContextPropertiesResolverMock.Setup(s => s.ClientIpAddress)
            .Returns(new System.Net.IPAddress(0x2444));
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsUserReadModel()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var user = this.CreateUser(organisationId);
        var tenant = TenantFactory.Create(Guid.NewGuid());
        var organisation = Organisation.CreateNewOrganisation(
            tenant.Id,
            tenant.Details.Alias,
            tenant.Details.Name,
            null,
            this.performingUserId,
            this.clock.GetCurrentInstant());
        var userLoginEmail = new UserLoginEmail(
            tenant.Id,
            user.Id,
            user.CreatedTimestamp,
            user.OrganisationId,
            user.LoginEmail);
        userLoginEmail.SaltedHashedPassword = "hashed_password";

        var portal = this.CreateTestPortal();

        var organisationReadModel = new OrganisationReadModel(
            tenant.Id, organisationId, organisation.Alias, organisation.Name, null, true, false, this.clock.GetCurrentInstant());

        this.organisationServiceMock
            .Setup(x => x.ValidateOrganisationBelongsToTenantAndIsActive(It.IsAny<Guid>(), It.IsAny<Guid>()));
        this.organisationServiceMock
            .Setup(x => x.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(false);

        this.loginAttemptTrackingServiceMock
            .Setup(x => x.IsLoginAttemptEmailBlocked(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(false);

        this.cachingResolverMock
            .Setup(x => x.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(organisationReadModel);
        this.cachingResolverMock.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).ReturnsAsync(tenant);

        this.cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(portal);

        this.passwordHashingServiceMock.Setup(x => x.SaltAndHash(It.IsAny<string>())).Returns("hashed_password");
        this.userLoginEmailRepositoryMock
            .Setup(x => x.GetUserLoginsByEmail(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new List<UserLoginEmail> { userLoginEmail });

        this.passwordHashingServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        this.userReadModelRepositoryMock.Setup(x => x.GetUserWithRoles(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(user);

        this.organisationServiceMock
            .Setup(x => x.ValidateOrganisationIsActive(It.IsAny<OrganisationReadModel>(), It.IsAny<Guid>()));
        var userAggregate = this.CreateUserAggregate(tenant.Id, this.performingUserId.GetValueOrDefault());
        this.userAggregateRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(userAggregate);

        var handler = this.CreateHandler();
        var command = new AuthenticateUserCommand(tenant.Id, Guid.NewGuid(), "leon@ubind.io", "password");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(user);
    }

    [Fact]
    public async Task Handle_InactiveOrganisation_ThrowsException()
    {
        // Arrange
        var organisationId = Guid.NewGuid();
        var user = this.CreateUser(organisationId);
        var tenant = TenantFactory.Create(Guid.NewGuid());
        var organisation = Organisation.CreateNewOrganisation(
                tenant.Id,
                tenant.Details.Alias,
                tenant.Details.Name,
                null,
                this.performingUserId,
                this.clock.GetCurrentInstant());

        var organisationReadModel = new OrganisationReadModel(
            tenant.Id, organisationId, organisation.Alias, organisation.Name, null, true, false, this.clock.GetCurrentInstant());

        // Set organisation as disabled
        organisationReadModel.IsActive = false;

        this.organisationServiceMock
            .Setup(x => x.ValidateOrganisationBelongsToTenantAndIsActive(It.IsAny<Guid>(), It.IsAny<Guid>()));
        this.organisationServiceMock
            .Setup(x => x.IsOrganisationDefaultForTenant(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(false);
        this.loginAttemptTrackingServiceMock
            .Setup(x => x.IsLoginAttemptEmailBlocked(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(false);

        this.cachingResolverMock
            .Setup(x => x.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(organisationReadModel);
        this.cachingResolverMock.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).ReturnsAsync(tenant);

        this.passwordHashingServiceMock.Setup(x => x.SaltAndHash(It.IsAny<string>())).Returns("hashed_password");
        this.userLoginEmailRepositoryMock
            .Setup(x => x.GetUserLoginByEmail(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new UserLoginEmail(
                tenant.Id,
                user.Id,
                user.CreatedTimestamp,
                user.OrganisationId,
                user.LoginEmail));
        this.passwordHashingServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        this.userReadModelRepositoryMock
            .Setup(x => x.GetUserWithRoles(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(user);

        var userAggregate = this.CreateUserAggregate(tenant.Id, this.performingUserId.GetValueOrDefault());
        this.userAggregateRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(userAggregate);

        this.tenantRepositoryMock.Setup(x => x.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
        this.organisationReadModelRepositoryMock
            .Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(organisationReadModel);
        var organisationService = new OrganisationService(
            Mock.Of<IOrganisationAggregateRepository>(),
            this.organisationReadModelRepositoryMock.Object,
            this.tenantRepositoryMock.Object,
            Mock.Of<IAdditionalPropertyValueService>(),
            Mock.Of<IHttpContextPropertiesResolver>(),
            Mock.Of<ICqrsMediator>(),
            this.cachingResolverMock.Object,
            Mock.Of<IClock>(),
            Mock.Of<IProductRepository>(),
            Mock.Of<IProductFeatureSettingRepository>(),
            Mock.Of<IProductOrganisationSettingRepository>());
        var handler = this.CreateHandler(organisationService);
        var command = new AuthenticateUserCommand(tenant.Id, Guid.NewGuid(), "leon@ubind.io", "password");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ErrorException>(() => handler.Handle(command, CancellationToken.None));
        ex.Error.Code.Should().Be(Errors.Organisation.Login.Disabled(organisation.Name).Code);
    }

    [Fact]
    public async Task Handle_WithMultipleEmailAddressMatches_PrefersMatchInRequestedOrganisation()
    {
        // Arrange
        var tenant = TenantFactory.Create(Guid.NewGuid());
        var requestedOrganisation = Organisation.CreateNewOrganisation(
            tenant.Id,
            tenant.Details.Alias,
            tenant.Details.Name,
            null,
            this.performingUserId,
            this.clock.GetCurrentInstant());
        var anotherOrganisation = Organisation.CreateNewOrganisation(
            tenant.Id,
            tenant.Details.Alias,
            tenant.Details.Name,
            null,
            this.performingUserId,
            this.clock.GetCurrentInstant());
        var requestedUser = this.CreateUser(requestedOrganisation.Id);
        var anotherUser = this.CreateUser(anotherOrganisation.Id);
        var requestedUserLoginEmail = new UserLoginEmail(
            tenant.Id,
            requestedUser.Id,
            requestedUser.CreatedTimestamp,
            requestedUser.OrganisationId,
            requestedUser.LoginEmail);
        requestedUserLoginEmail.SaltedHashedPassword = "hashed_password";
        var anotherUserLoginEmail = new UserLoginEmail(
            tenant.Id,
            anotherUser.Id,
            anotherUser.CreatedTimestamp,
            anotherUser.OrganisationId,
            anotherUser.LoginEmail);
        anotherUserLoginEmail.SaltedHashedPassword = "hashed_password2";

        var requestedOrganisationReadModel = new OrganisationReadModel(
            tenant.Id, requestedOrganisation.Id, requestedOrganisation.Alias, requestedOrganisation.Name, null, true, false, this.clock.GetCurrentInstant());
        var anotherOrganisationReadModel = new OrganisationReadModel(
            tenant.Id, anotherOrganisation.Id, anotherOrganisation.Alias, anotherOrganisation.Name, null, true, false, this.clock.GetCurrentInstant());

        var portal = this.CreateTestPortal();

        this.organisationServiceMock
            .Setup(x => x.ValidateOrganisationBelongsToTenantAndIsActive(It.IsAny<Guid>(), It.IsAny<Guid>()));

        this.loginAttemptTrackingServiceMock
            .Setup(x => x.IsLoginAttemptEmailBlocked(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(false);

        this.cachingResolverMock
            .Setup(x => x.GetOrganisationOrThrow(It.IsAny<Guid>(), requestedOrganisation.Id))
            .ReturnsAsync(requestedOrganisationReadModel);
        this.cachingResolverMock
            .Setup(x => x.GetOrganisationOrThrow(It.IsAny<Guid>(), anotherOrganisation.Id))
            .ReturnsAsync(anotherOrganisationReadModel);
        this.cachingResolverMock.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).ReturnsAsync(tenant);

        this.cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(portal);
        this.passwordHashingServiceMock.Setup(x => x.SaltAndHash(It.IsAny<string>())).Returns("hashed_password");
        this.userLoginEmailRepositoryMock
            .Setup(x => x.GetUserLoginsByEmail(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new List<UserLoginEmail> { requestedUserLoginEmail, anotherUserLoginEmail });

        this.passwordHashingServiceMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        this.userReadModelRepositoryMock.Setup(x => x.GetUserWithRoles(It.IsAny<Guid>(), requestedUser.Id)).Returns(requestedUser);
        this.userReadModelRepositoryMock.Setup(x => x.GetUserWithRoles(It.IsAny<Guid>(), anotherUser.Id)).Returns(anotherUser);

        this.organisationServiceMock
            .Setup(x => x.ValidateOrganisationIsActive(It.IsAny<OrganisationReadModel>(), It.IsAny<Guid>()));
        var userAggregate = this.CreateUserAggregate(tenant.Id, this.performingUserId.GetValueOrDefault());
        this.userAggregateRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(userAggregate);

        var handler = this.CreateHandler();
        var command = new AuthenticateUserCommand(tenant.Id, requestedOrganisation.Id, requestedUser.LoginEmail, "password");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(requestedUser);
    }

    [Fact]
    public async Task Handle_AcceptsMatchOutsideRequestedOrganisation_WhenRequestedOrganisationDoesntMatch()
    {
        // Arrange
        var tenant = TenantFactory.Create(Guid.NewGuid());
        var requestedOrganisation = Organisation.CreateNewOrganisation(
            tenant.Id,
            tenant.Details.Alias,
            tenant.Details.Name,
            null,
            this.performingUserId,
            this.clock.GetCurrentInstant());
        var anotherOrganisation = Organisation.CreateNewOrganisation(
            tenant.Id,
            tenant.Details.Alias,
            tenant.Details.Name,
            null,
            this.performingUserId,
            this.clock.GetCurrentInstant());
        var user1 = this.CreateUser(anotherOrganisation.Id);
        var user2 = this.CreateUser(anotherOrganisation.Id);
        var requestedUserLoginEmail = new UserLoginEmail(
            tenant.Id,
            user1.Id,
            user1.CreatedTimestamp,
            user1.OrganisationId,
            user1.LoginEmail);
        requestedUserLoginEmail.SaltedHashedPassword = "hashed_password";
        var anotherUserLoginEmail = new UserLoginEmail(
            tenant.Id,
            user2.Id,
            user2.CreatedTimestamp,
            user2.OrganisationId,
            user2.LoginEmail);
        anotherUserLoginEmail.SaltedHashedPassword = "hashed_password2";

        var portal = this.CreateTestPortal();

        var requestedOrganisationReadModel = new OrganisationReadModel(
            tenant.Id, requestedOrganisation.Id, requestedOrganisation.Alias, requestedOrganisation.Name, null, true, false, this.clock.GetCurrentInstant());
        var anotherOrganisationReadModel = new OrganisationReadModel(
            tenant.Id, anotherOrganisation.Id, anotherOrganisation.Alias, anotherOrganisation.Name, null, true, false, this.clock.GetCurrentInstant());

        this.organisationServiceMock
            .Setup(x => x.ValidateOrganisationBelongsToTenantAndIsActive(It.IsAny<Guid>(), It.IsAny<Guid>()));

        this.loginAttemptTrackingServiceMock
            .Setup(x => x.IsLoginAttemptEmailBlocked(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(false);

        this.cachingResolverMock
            .Setup(x => x.GetOrganisationOrThrow(It.IsAny<Guid>(), requestedOrganisation.Id))
            .ReturnsAsync(requestedOrganisationReadModel);
        this.cachingResolverMock
            .Setup(x => x.GetOrganisationOrThrow(It.IsAny<Guid>(), anotherOrganisation.Id))
            .ReturnsAsync(anotherOrganisationReadModel);
        this.cachingResolverMock.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).ReturnsAsync(tenant);

        this.cachingResolverMock.Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(portal);

        this.passwordHashingServiceMock.Setup(x => x.SaltAndHash(It.IsAny<string>())).Returns("hashed_password");
        this.userLoginEmailRepositoryMock
            .Setup(x => x.GetUserLoginsByEmail(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(new List<UserLoginEmail> { requestedUserLoginEmail, anotherUserLoginEmail });

        this.passwordHashingServiceMock.Setup(x => x.Verify("wrong_password", It.IsAny<string>())).Returns(false);
        this.passwordHashingServiceMock.Setup(x => x.Verify("right_password", It.IsAny<string>())).Returns(true);

        this.userReadModelRepositoryMock.Setup(x => x.GetUserWithRoles(It.IsAny<Guid>(), user1.Id)).Returns(user1);
        this.userReadModelRepositoryMock.Setup(x => x.GetUserWithRoles(It.IsAny<Guid>(), user2.Id)).Returns(user2);

        this.organisationServiceMock
            .Setup(x => x.ValidateOrganisationIsActive(It.IsAny<OrganisationReadModel>(), It.IsAny<Guid>()));
        var userAggregate = this.CreateUserAggregate(tenant.Id, this.performingUserId.GetValueOrDefault());
        this.userAggregateRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(userAggregate);

        var handler = this.CreateHandler();
        var command = new AuthenticateUserCommand(tenant.Id, requestedOrganisation.Id, user1.LoginEmail, "right_password");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    private UserReadModel CreateUser(Guid organisationId)
    {
        var user = new UserReadModel(
            Guid.NewGuid(),
            new PersonData(),
            null,
            Guid.NewGuid(),
            this.clock.Now(),
            UserType.Client);
        user.OrganisationId = organisationId;
        return user;
    }

    private UserAggregate CreateUserAggregate(Guid tenantId, Guid performingUserId)
    {
        var timestamp = SystemClock.Instance.GetCurrentInstant();
        var tenant = new Tenant(
            tenantId, performingUserId.ToString(), performingUserId.ToString(), null, default, default, timestamp);
        var person = PersonAggregate.CreatePerson(
            tenant.Id, tenant.Details.DefaultOrganisationId, performingUserId, timestamp);
        var user = UserAggregate.CreateUser(tenant.Id, performingUserId, UserType.Client, person, performingUserId, null, timestamp);
        return user;
    }

    private AuthenticateUserCommandHandler CreateHandler(OrganisationService? organisationService = null)
    {
        return new AuthenticateUserCommandHandler(
            this.passwordHashingServiceMock.Object,
            this.userLoginEmailRepositoryMock.Object,
            this.userAggregateRepositoryMock.Object,
            this.userReadModelRepositoryMock.Object,
            this.loginAttemptTrackingServiceMock.Object,
            organisationService ?? this.organisationServiceMock.Object,
            Mock.Of<IUserSystemEventEmitter>(),
            this.cachingResolverMock.Object,
            Mock.Of<IClock>(),
            this.httpContextPropertiesResolverMock.Object);
    }

    private PortalReadModel CreateTestPortal()
    {
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

        return portal;
    }
}
