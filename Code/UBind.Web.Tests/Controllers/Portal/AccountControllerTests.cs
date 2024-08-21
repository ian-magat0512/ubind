// <copyright file="AccountControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.Portal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using NodaTime;
using UBind.Application.Authorisation;
using UBind.Application.Commands.Authentication;
using UBind.Application.ExtensionMethods;
using UBind.Application.Infrastructure;
using UBind.Application.Models.User;
using UBind.Application.Person;
using UBind.Application.Queries.Principal;
using UBind.Application.Queries.User;
using UBind.Application.Services;
using UBind.Application.User;
using UBind.Domain;
using UBind.Domain.Aggregates;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Authentication;
using UBind.Domain.Entities;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Permissions;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Portal;
using UBind.Domain.ReadModel.User;
using UBind.Domain.Redis;
using UBind.Domain.Repositories;
using UBind.Domain.Services;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Web.Controllers;
using UBind.Web.ResourceModels;
using Xunit;

public class AccountControllerTests
{
    private readonly AccountController sut;
    private readonly Guid tenantId = Guid.NewGuid();
    private Mock<IUserReadModelRepository> mockReadModelRepo;
    private Mock<Application.User.IUserService> mockUserService;
    private Mock<IPersonService> mockPersonService;
    private Mock<ITenantRepository> mockTenantRepo;
    private Mock<IAccessTokenService> mockAccessTokenService;
    private Mock<ITenantService> mockTenantService;
    private Mock<IOrganisationService> mockOrganisationService;
    private Mock<IAdditionalPropertyValueService> additionalPropertyValueService;
    private Mock<ICqrsMediator> mockMediator;
    private Mock<ICachingResolver> mockCachingResolver;
    private Mock<IClock> mockClock;
    private Mock<AuthorisationService> mockAuthorisationService;

    public AccountControllerTests()
    {
        this.mockReadModelRepo = new Mock<IUserReadModelRepository>();
        this.mockUserService = new Mock<Application.User.IUserService>();
        this.mockPersonService = new Mock<IPersonService>();
        this.mockTenantRepo = new Mock<ITenantRepository>();
        this.mockAccessTokenService = new Mock<IAccessTokenService>();
        this.mockTenantService = new Mock<ITenantService>();
        this.mockOrganisationService = new Mock<IOrganisationService>();
        this.additionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();
        this.mockMediator = new Mock<ICqrsMediator>();
        this.mockCachingResolver = new Mock<ICachingResolver>();
        this.mockAuthorisationService = new Mock<AuthorisationService>(
            MockBehavior.Strict,
            this.mockOrganisationService.Object,
            this.mockUserService.Object,
            null,
            null,
            null,
            null,
            null,
            this.mockMediator.Object,
            this.mockCachingResolver.Object,
            null,
            null,
            null,
            null,
            null);
        this.mockClock = new Mock<IClock>();

        this.sut = new AccountController(
            this.mockReadModelRepo.Object,
            this.mockUserService.Object,
            this.mockPersonService.Object,
            this.mockAccessTokenService.Object,
            this.additionalPropertyValueService.Object,
            this.mockMediator.Object,
            this.mockCachingResolver.Object,
            this.mockAuthorisationService.Object);

        var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
        var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
        var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
        Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
        Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
        PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
        PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);

        var user = new UserAuthenticationData(
            Guid.NewGuid(),
            Guid.NewGuid(),
            UserType.Master,
            Guid.NewGuid(),
            Guid.NewGuid(),
            new List<Permission>
            {
                Permission.ViewBackgroundJobs,
            });
        this.mockMediator.Setup(_ => _.Send(It.IsAny<GetPrincipalAuthenticationDataQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
    }

    private IFormFileCollection MockFormFileCollection
    {
        get
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(_ => _.Length).Returns(0);
            var coll = new FormFileCollection();
            coll.Add(fileMock.Object);

            return coll;
        }
    }

    [Fact]
    public async Task UpdateUserPicture_Returns_UserDtoModelWithProfilePictureIdAsync()
    {
        // Arrange
        var context = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new Claim[]
                {
                new Claim(ClaimNames.TenantId, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "example name"),
                new Claim(ClaimTypes.NameIdentifier, "3CB23246-66A1-478F-B9C7-F4A8180D0F64"),
                }, "mock")),
            },
        };

        context.HttpContext.Request.Form = new FormCollection(It.IsAny<Dictionary<string, StringValues>>(), this.MockFormFileCollection);
        this.sut.ControllerContext = context;

        var userReadModel = new UserReadModel(
            It.IsAny<Guid>(),
            new PersonData(
                PersonAggregate.CreatePerson(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    NodaTime.SystemClock.Instance.GetCurrentInstant())),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            NodaTime.SystemClock.Instance.GetCurrentInstant(),
            It.IsAny<UserType>())
        {
            ProfilePictureId = Guid.Parse("855A3B7E-D7FC-4E09-A33E-C079530E18AA"),
        };

        var userModel = new Application.User.UserModel(
            new UserAggregate(Enumerable.Empty<IEvent<UserAggregate, Guid>>()),
            PersonAggregate.LoadFromEvents(Enumerable.Empty<IEvent<PersonAggregate, Guid>>()));

        this.mockUserService.Setup(_ => _.GetUser(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(userReadModel);

        // Act
        var result = (await this.sut.UpdateUserPicture() as OkObjectResult).Value as Web.ResourceModels.UserResourceModel;

        // Assert
        Assert.NotNull(result.ProfilePictureId);
    }

    [Fact]
    public async Task UpdateUserPicture_Should_Call_UserService_SaveProfilePictureForUser()
    {
        // Arrange
        var context = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new Claim[]
                {
                new Claim(ClaimNames.TenantId, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "example name"),
                new Claim(ClaimTypes.NameIdentifier, "3CB23246-66A1-478F-B9C7-F4A8180D0F64"),
                }, "mock")),
            },
        };

        context.HttpContext.Request.Form = new FormCollection(It.IsAny<Dictionary<string, StringValues>>(), this.MockFormFileCollection);
        this.sut.ControllerContext = context;

        var userReadModel = new UserReadModel(
            It.IsAny<Guid>(),
            new PersonData(
                PersonAggregate.CreatePerson(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    NodaTime.SystemClock.Instance.GetCurrentInstant())),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            NodaTime.SystemClock.Instance.GetCurrentInstant(),
            It.IsAny<UserType>())
        {
            ProfilePictureId = Guid.Parse("855A3B7E-D7FC-4E09-A33E-C079530E18AA"),
        };

        var userModel = new Application.User.UserModel(
            new UserAggregate(Enumerable.Empty<IEvent<UserAggregate, Guid>>()),
            PersonAggregate.LoadFromEvents(Enumerable.Empty<IEvent<PersonAggregate, Guid>>()));

        this.mockUserService.Setup(_ => _.GetUser(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(userReadModel);

        this.mockUserService.Setup(_ => _.SaveProfilePictureForUser(It.IsAny<Guid>(), It.IsAny<IFormFile>(), userReadModel))
        .ReturnsAsync(() =>
        {
            return userReadModel.ProfilePictureId.Value;
        })
        .Verifiable();

        // Act
        var result = (await this.sut.UpdateUserPicture() as OkObjectResult).Value as Web.ResourceModels.UserResourceModel;

        // Assert
        this.mockUserService.Verify(_ => _.SaveProfilePictureForUser(It.IsAny<Guid>(), It.IsAny<IFormFile>(), userReadModel), Times.Exactly(1), "should call the method that updates the profile picture at least once.");
    }

    [Fact]
    public async Task Login_Returns_ModelWithProfilePictureIdProperty()
    {
        // Arrange
        var mockTenant = new Tenant(Guid.NewGuid(), "Name", "Alias", null, default, default, NodaTime.SystemClock.Instance.GetCurrentInstant());
        this.mockCachingResolver.Setup(_ => _.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(mockTenant));
        this.mockTenantRepo.Setup(_ => _.GetTenantById(It.IsAny<Guid>())).Returns(mockTenant);
        var userModel = new UserReadModel(
            It.IsAny<Guid>(),
            new PersonData(
                PersonAggregate.CreatePerson(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    NodaTime.SystemClock.Instance.GetCurrentInstant())),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            NodaTime.SystemClock.Instance.GetCurrentInstant(),
            It.IsAny<UserType>())
        {
            ProfilePictureId = Guid.Parse("855A3B7E-D7FC-4E09-A33E-C079530E18AA"),
        };
        var organisationModel = new OrganisationReadModel(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            null, It.IsAny<bool>(),
            It.IsAny<bool>(),
            NodaTime.SystemClock.Instance.GetCurrentInstant());
        var portalModel = new PortalReadModel
        {
            Id = Guid.NewGuid(),
            Alias = "my-portal",
        };

        this.mockMediator
            .Setup(m => m.Send(It.IsAny<GetUsersMatchingEmailAddressIncludingPlusAddressingQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<UserModel>());
        this.mockMediator
            .Setup(s => s.Send(It.IsAny<AuthenticateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(userModel);
        this.mockAccessTokenService
            .Setup(_ => _.CreateAccessToken(It.IsAny<UserSessionModel>()))
            .Returns(Task.FromResult(new System.IdentityModel.Tokens.Jwt.JwtSecurityToken()));
        this.mockTenantService
            .Setup(_ => _.GetTenant(It.IsAny<Guid>()))
            .Returns(mockTenant);
        this.mockCachingResolver
            .Setup(_ => _.GetTenantOrThrow(It.IsAny<GuidOrAlias>()))
            .Returns(Task.FromResult(mockTenant));
        this.mockCachingResolver
            .Setup(s => s.GetPortalOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.FromResult(portalModel));
        this.mockCachingResolver
            .Setup(s => s.GetOrganisationOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .Returns(Task.FromResult(organisationModel));

        // Mock the call to GetEffectivePermissions
        this.mockUserService
            .Setup(u => u.GetEffectivePermissions(It.IsAny<UserReadModel>(), It.IsAny<OrganisationReadModel>()))
            .ReturnsAsync(new List<Permission>());

        // Act
        IActionResult actionResult = await this.sut.Login(new AuthenticationModel { Tenant = mockTenant.Id.ToString(), EmailAddress = "Email", PlaintextPassword = "Password" });
        UserAuthorisationModel result = (actionResult as OkObjectResult).Value as UserAuthorisationModel;

        // Assert
        Assert.NotNull(result?.ProfilePictureId);
    }

    /// <summary>
    /// The GetPermissionsOfTheLoggedInUser_ShouldReturnListOfPermissions_WhenUserTypeIsAgent.
    /// </summary>
    [Fact]
    public async Task GetPermissionsOfTheLoggedInUser_ShouldReturnListOfPermissions_WhenUserTypeIsAgent()
    {
        // Arrange
        var mockedUser = this.CreateClaimsPrincipal(UserType.Client);
        var mockedRole = new Role(
                    this.tenantId,
                    Guid.NewGuid(),
                    DefaultRole.Customer,
                    this.mockClock.Object.Now());

        var mockedPermissions = new List<Permission>(mockedRole.Permissions.ToList());
        var expectedPermissions = mockedPermissions.Select(a => new PermissionModel(a));
        this.mockMediator.Setup(a => a.Send(
            It.IsAny<GetUserByIdQuery>(),
            It.IsAny<CancellationToken>()).Result).Returns(
                new UserReadModel(
                    mockedUser.GetId().Value,
                    new PersonData(
                        PersonAggregate.CreatePerson(
                            this.tenantId,
                            It.IsAny<Guid>(),
                            It.IsAny<Guid>(),
                            NodaTime.SystemClock.Instance.GetCurrentInstant())),
                    mockedUser.GetCustomerId(),
                    It.IsAny<Guid>(),
                    NodaTime.SystemClock.Instance.GetCurrentInstant(),
                    It.IsAny<UserType>()));

        this.mockMediator.Setup(a => a.Send(
            It.IsAny<GetEffectivePermissionsForUserQuery>(),
            It.IsAny<CancellationToken>()).Result).Returns(
            mockedPermissions);

        var context = this.CreateControllerHttpContext();
        context.HttpContext.User = mockedUser;
        var accountController = new AccountController(
            new Mock<IUserReadModelRepository>().Object,
            this.mockUserService.Object,
            this.mockPersonService.Object,
            this.mockAccessTokenService.Object,
            new Mock<IAdditionalPropertyValueService>().Object,
            this.mockMediator.Object,
            new Mock<ICachingResolver>().Object,
            new Mock<IAuthorisationService>().Object)
        {
            ControllerContext = context,
        };

        // Act
        var response = await accountController.GetPermissionsOfTheLoggedInUser();
        var result = response as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);

        var permissions = result.Value as IEnumerable<PermissionModel>;

        permissions.Should().NotBeNullOrEmpty();
        permissions.Should().BeEquivalentTo(expectedPermissions);
    }

    /// <summary>
    /// The GetPermissionsOfTheLoggedInUser_ShouldReturnForbiddenStatusCode_WhenUserTypeIsCustomer.
    /// </summary>
    [Fact]
    public async Task GetPermissionsOfTheLoggedInUser_ShouldReturnForbiddenStatusCode_WhenUserTypeIsCustomer()
    {
        // Arrange
        var mockedUser = this.CreateClaimsPrincipal(UserType.Customer);
        var context = this.CreateControllerHttpContext();
        context.HttpContext.User = mockedUser;

        var accountController = this.sut;
        accountController.ControllerContext = context;

        // Act
        var response = await accountController.GetPermissionsOfTheLoggedInUser();
        var result = response as JsonResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(403);
    }

    /// <summary>
    /// The GetRolesOfTheLoggedInUser_ShouldReturnListOfRoles_WhenUserTypeIsAgent.
    /// </summary>
    [Fact]
    public async Task GetRolesOfTheLoggedInUser_ShouldReturnListOfRoles_WhenUserTypeIsAgent()
    {
        // Arrange
        var mockedUser = this.CreateClaimsPrincipal(UserType.Client);
        var mockedRole = new Role(
                    this.tenantId,
                    Guid.NewGuid(),
                    DefaultRole.UnderWriter,
                    this.mockClock.Object.Now());

        var mockedRoles = new List<Role>() { mockedRole };
        var expectedRoles = mockedRoles.Select(a => new RoleSummaryModel(a));
        this.mockMediator.Setup(a => a.Send(
            It.IsAny<EffectiveRolesForUserQuery>(),
            It.IsAny<CancellationToken>()).Result).Returns(
            mockedRoles);

        var context = this.CreateControllerHttpContext();
        context.HttpContext.User = mockedUser;
        var accountController = new AccountController(
            new Mock<IUserReadModelRepository>().Object,
            this.mockUserService.Object,
            this.mockPersonService.Object,
            this.mockAccessTokenService.Object,
            new Mock<IAdditionalPropertyValueService>().Object,
            this.mockMediator.Object,
            new Mock<ICachingResolver>().Object,
            new Mock<IAuthorisationService>().Object)
        {
            ControllerContext = context,
        };

        // Act
        var response = await accountController.GetRolesOfTheLoggedInUser();
        var result = response as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(200);

        var roles = result.Value as IEnumerable<RoleSummaryModel>;

        roles.Should().NotBeNullOrEmpty();
        roles.Should().BeEquivalentTo(expectedRoles);
    }

    /// <summary>
    /// The GetRolesOfTheLoggedInUser_ShouldReturnForbiddenStatusCode_WhenUserTypeIsCustomer.
    /// </summary>
    [Fact]
    public async Task GetRolesOfTheLoggedInUser_ShouldReturnForbiddenStatusCode_WhenUserTypeIsCustomer()
    {
        // Arrange
        var mockedUser = this.CreateClaimsPrincipal(UserType.Customer);
        var context = this.CreateControllerHttpContext();
        context.HttpContext.User = mockedUser;

        var accountController = this.sut;
        accountController.ControllerContext = context;

        // Act
        var response = await accountController.GetRolesOfTheLoggedInUser();
        var result = response as JsonResult;

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(403);
    }

    private ControllerContext CreateControllerHttpContext()
    {
        var context = new ControllerContext();
        var defaultContext = new DefaultHttpContext();
        context.HttpContext = defaultContext;
        return context;
    }

    private ClaimsPrincipal CreateClaimsPrincipal(UserType userType)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>()
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimNames.TenantId, this.tenantId.ToString()),
            new Claim(ClaimNames.OrganisationId, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, userType.Humanize()),
            new Claim(ClaimNames.CustomerId, Guid.NewGuid().ToString()),
        }));
    }
}
