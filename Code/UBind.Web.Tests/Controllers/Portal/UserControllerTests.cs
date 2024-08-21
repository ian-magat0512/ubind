// <copyright file="UserControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using FluentFTP.Helpers;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using UBind.Application.Authorisation;
    using UBind.Application.Commands.User;
    using UBind.Application.Infrastructure;
    using UBind.Application.Person;
    using UBind.Application.Queries.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Entities;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    public class UserControllerTests
    {
        private Mock<Application.User.IUserService> mockUserService;
        private Mock<IUserProfilePictureRepository> mockUserProfilePictureRepo;
        private Mock<IOrganisationReadModelRepository> mockOrganisationReadModelRepo;
        private Mock<IOrganisationService> mockOrganisationService;
        private Mock<ICqrsMediator> mockMediator;
        private Mock<IAuthorisationService> mockAuthorisationService;
        private Mock<IUserAuthorisationService> mockUserAuthorisationService;
        private Mock<ICachingResolver> mockcachingResolver;
        private Mock<IPersonService> mockPersonService;
        private Mock<IAdditionalPropertyValueService> additionalPropertyValueService;

        private UserController sut;

        public UserControllerTests()
        {
            this.mockMediator = new Mock<ICqrsMediator>();
            this.mockUserService = new Mock<Application.User.IUserService>();
            this.mockPersonService = new Mock<IPersonService>();
            this.mockUserProfilePictureRepo = new Mock<IUserProfilePictureRepository>();
            this.mockOrganisationReadModelRepo = new Mock<IOrganisationReadModelRepository>();
            this.mockOrganisationService = new Mock<IOrganisationService>();
            this.mockMediator = new Mock<ICqrsMediator>();
            this.mockAuthorisationService = new Mock<IAuthorisationService>();
            this.mockUserAuthorisationService = new Mock<IUserAuthorisationService>();
            this.mockcachingResolver = new Mock<ICachingResolver>();
            this.additionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();
            this.sut = new UserController(
                this.mockUserService.Object,
                this.mockPersonService.Object,
                this.mockUserProfilePictureRepo.Object,
                this.mockOrganisationService.Object,
                this.mockAuthorisationService.Object,
                this.mockMediator.Object,
                this.mockcachingResolver.Object,
                this.additionalPropertyValueService.Object,
                this.mockUserAuthorisationService.Object);
        }

        private IFormFileCollection MockFormFileCollection
        {
            get
            {
                var s_bytes = Encoding.UTF8.GetBytes("test string");
                var fileMock = new FormFile(
                    baseStream: new MemoryStream(s_bytes),
                    baseStreamOffset: 0,
                    length: s_bytes.Length,
                    name: "Data",
                    fileName: "test file name");
                var coll = new FormFileCollection();
                coll.Add(fileMock);

                return coll;
            }
        }

        [Fact]
        public async Task UpdateUserPicture_Returns_UserDtoModelWithProfilePictureIdAsync()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string[] permissions = new string[] { Permission.ManageUsers.ToString() };
            var claims = new List<Claim>
                {
                    new Claim(ClaimNames.TenantId, tenantId.ToString()),
                    new Claim(ClaimTypes.Name, "example name"),
                    new Claim(ClaimTypes.NameIdentifier, "3CB23246-66A1-478F-B9C7-F4A8180D0F64"),
                };
            foreach (var permission in permissions)
            {
                claims.Add(new Claim(ClaimNames.Permissions, permission.ToString().Camelize()));
            }

            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        claims, "mock")),
                },
            };

            context.HttpContext.Request.Form = new FormCollection(It.IsAny<Dictionary<string, StringValues>>(), this.MockFormFileCollection);
            this.sut.ControllerContext = context;

            var userReadModel = new UserReadModel(
                It.IsAny<Guid>(),
                new PersonData(PersonAggregate.CreatePerson(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    NodaTime.SystemClock.Instance.GetCurrentInstant())),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                NodaTime.SystemClock.Instance.GetCurrentInstant(),
                It.IsAny<UserType>())
            {
                UserType = UserType.Master.Humanize(),
            };

            var userModel = new Application.User.UserModel(
                new UserAggregate(Enumerable.Empty<IEvent<UserAggregate, Guid>>()),
                PersonAggregate.LoadFromEvents(Enumerable.Empty<IEvent<PersonAggregate, Guid>>()));

            this.mockMediator.Setup(s => s.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(userReadModel));
            this.mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(TenantFactory.Create(tenantId)));

            // Act
            var result = (await this.sut.UpdateUserPicture(It.IsAny<Guid>(), It.IsAny<string>()) as OkObjectResult).Value as Web.ResourceModels.UserResourceModel;

            // Assert
            Assert.NotNull(result.ProfilePictureId);
        }

        [Fact]
        public async Task UpdateUserPicture_Should_Call_UserService_SaveProfilePictureForUserAsync()
        {
            // Arrange
            Guid tenantId = Guid.NewGuid();
            string[] permissions = new string[] { Permission.ManageUsers.ToString() };
            var claims = new List<Claim>
                {
                    new Claim(ClaimNames.TenantId, tenantId.ToString()),
                    new Claim(ClaimTypes.Name, "example name"),
                    new Claim(ClaimTypes.NameIdentifier, "3CB23246-66A1-478F-B9C7-F4A8180D0F64"),
                };
            foreach (var permission in permissions)
            {
                claims.Add(new Claim(ClaimNames.Permissions, permission.ToString().Camelize()));
            }

            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        claims, "mock")),
                },
            };

            context.HttpContext.Request.Form = new FormCollection(It.IsAny<Dictionary<string, StringValues>>(), this.MockFormFileCollection);
            this.sut.ControllerContext = context;

            var userReadModel = new UserReadModel(
                It.IsAny<Guid>(),
                new PersonData(PersonAggregate.CreatePerson(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    NodaTime.SystemClock.Instance.GetCurrentInstant())),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                NodaTime.SystemClock.Instance.GetCurrentInstant(),
                It.IsAny<UserType>())
            {
                UserType = UserType.Master.Humanize(),
            };

            var userModel = new Application.User.UserModel(
                new UserAggregate(Enumerable.Empty<IEvent<UserAggregate, Guid>>()),
                PersonAggregate.LoadFromEvents(Enumerable.Empty<IEvent<PersonAggregate, Guid>>()));

            this.mockMediator.Setup(s => s.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(userReadModel));
            this.mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(TenantFactory.Create(tenantId)));

            this.mockUserService.Setup(_ => _.SaveProfilePictureForUser(It.IsAny<Guid>(), It.IsAny<IFormFile>(), userReadModel))
            .Verifiable();

            // Act
            var result = (await this.sut.UpdateUserPicture(It.IsAny<Guid>(), It.IsAny<string>()) as OkObjectResult).Value as Web.ResourceModels.UserResourceModel;

            // Assert
            this.mockUserService.Verify(_ => _.SaveProfilePictureForUser(It.IsAny<Guid>(), It.IsAny<IFormFile>(), userReadModel), Times.Exactly(1), "should call the method that updates the profile picture at least once.");
        }

        [Theory]
        [InlineData("3CB23246-66A1-478F-B9C7-F4A8180D0F64", UserType.Client)]
        [InlineData("fba5a5f0-17ca-4f29-9f52-2c4aabb26c4d", UserType.Master)] // master tenant
        public async Task CreateUser_SendsCommand_WithCorrectUserType(string tenantIdOfUserToCreate, UserType expectedUserType)
        {
            // Arrange
            var tenantOfUserToCreate = TenantFactory.Create(new GuidOrAlias(tenantIdOfUserToCreate).Guid, "some-tenant");

            // The performingUser is a master user
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRole = defaultRoleNameRegistry.GetDefaultRoleForRoleName("Master Admin", RoleType.Master);
            var defaultRolePermissions = defaultRolePermissionsRegistry.GetPermissionsForDefaultRole(defaultRole).ToList();
            var claims = new List<Claim>
            {
                new Claim(ClaimNames.TenantId, Tenant.MasterTenantId.ToString()),
                new Claim(ClaimTypes.Name, "example name"),
                new Claim(ClaimTypes.NameIdentifier, "3CB23246-66A1-478F-B9C7-F4A8180D0F64"),
            };
            foreach (var permission in defaultRolePermissions)
            {
                claims.Add(new Claim(ClaimNames.Permissions, permission.ToString().Camelize()));
            }
            var context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        claims, "mock")),
                },
            };
            this.sut.ControllerContext = context;
            var signUpModel = new SignupModel
            {
                FullName = "Gwen Chana",
                FirstName = "Gwen",
                LastName = "Chana",
                Email = "gwenchana@ubind.io",
                Tenant = tenantOfUserToCreate.Details.Alias,
                OrganisationId = Guid.NewGuid(),
                Environment = DeploymentEnvironment.Staging,
                InitialRoles = new List<Guid> { Guid.NewGuid() }.ToArray(),
            };
            var userReadModel = new UserReadModel(
                It.IsAny<Guid>(),
                new PersonData(PersonAggregate.CreatePerson(
                    tenantOfUserToCreate.Id,
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    NodaTime.SystemClock.Instance.GetCurrentInstant())),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                NodaTime.SystemClock.Instance.GetCurrentInstant(),
                It.IsAny<UserType>());
            var userModel = new Application.User.UserModel(
                new UserAggregate(Enumerable.Empty<IEvent<UserAggregate, Guid>>()),
                PersonAggregate.LoadFromEvents(Enumerable.Empty<IEvent<PersonAggregate, Guid>>()));
            this.mockMediator.Setup(s => s.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(userReadModel));
            this.mockcachingResolver.Setup(x => x.GetTenantOrThrow(new GuidOrAlias(tenantOfUserToCreate.Details.Alias)))
                .Returns(Task.FromResult(tenantOfUserToCreate));
            this.mockcachingResolver.Setup(x => x.GetTenantOrThrow(new GuidOrAlias(Tenant.MasterTenantId)))
                .Returns(Task.FromResult(TenantFactory.Create(Tenant.MasterTenantId, Tenant.MasterTenantAlias)));

            // Act
            var result = (await this.sut.CreateUser(signUpModel) as OkObjectResult).Value as Web.ResourceModels.UserResourceModel;

            // Assert
            this.mockMediator.Verify(
                 m => m.Send(It.Is<CreateUserCommand>(r => r.UserData.UserType == expectedUserType), It.IsAny<CancellationToken>()),
                 Times.Once());
        }
    }
}
