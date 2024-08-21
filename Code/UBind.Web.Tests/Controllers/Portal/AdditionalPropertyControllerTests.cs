// <copyright file="AdditionalPropertyControllerTests.cs" company="uBind">
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
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using UBind.Application.Commands.AdditionalPropertyDefinition;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// unit tests for <see cref="AdditionalPropertyControllerTests"/> controller.
    /// </summary>
    public class AdditionalPropertyControllerTests : IClassFixture<AdditionalPropertyControllerFixture>
    {
        private readonly AdditionalPropertyControllerFixture fixture;

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid userId = new Guid("A1B2A3B4-3333-2222-1111-000000000000");

        public AdditionalPropertyControllerTests(AdditionalPropertyControllerFixture fixture)
        {
            var tenant = TenantFactory.Create();
            this.fixture = fixture;
            this.fixture.CachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));
        }

        [Fact]
        public async Task Create_ShouldReturnOk_WhenValidationSucceedsAsync()
        {
            // Assign
            var tenant = TenantFactory.Create();
            this.fixture.CachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));
            var additionalPropertyDefinitionCreateOrUpdateModel = AdditionalPropertyFakeData
                .CreateFakeAdditionalPropertyCreateModel();
            var additionalPropertyDefinitionModel = AdditionalPropertyFakeData
                .CreateFakeAdditionalPropertyModelOnCreate();
            var additionalPropertyReadModel = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(
                Guid.NewGuid());
            var additionalPropertyDefinition = AdditionalPropertyFakeData
                .CreateAdditionalPropertyDefinitionWithId(additionalPropertyReadModel.Id);
            this.fixture.AdditionalPropertyValidatorMock.Setup(vm => vm.GetValidatorByContextType(
               Domain.Enums.AdditionalPropertyDefinitionContextType.Tenant))
                                               .Returns(this.fixture.AdditionalPropertyContextValidatorMock.Object);
            this.fixture.MediatorMock.Setup(
                mo => mo.Send(It.IsAny<CreateAdditionalPropertyDefinitionCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(additionalPropertyReadModel));

            this.fixture.AdditionalPropertyModelResolver.Setup(vm => vm.ResolveToDefinitionModel(additionalPropertyDefinitionCreateOrUpdateModel))
                .Returns(Task.FromResult(additionalPropertyDefinitionModel));

            additionalPropertyDefinitionCreateOrUpdateModel.ContextType = AdditionalPropertyDefinitionContextType.Tenant;
            additionalPropertyDefinitionCreateOrUpdateModel.ContextId = Guid.NewGuid().ToString();
            additionalPropertyDefinitionCreateOrUpdateModel.Tenant = tenant.Id.ToString();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", additionalPropertyDefinitionCreateOrUpdateModel.Tenant.ToString()),
            }));

            this.fixture.Controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // Act
            var response = await this.fixture.Controller.Create(additionalPropertyDefinitionCreateOrUpdateModel);
            var okResult = response as OkObjectResult;
            var okResultActualObject = okResult.Value as AdditionalPropertyDefinitionModel;

            // Assert
            okResult.Value.Should().BeOfType(typeof(AdditionalPropertyDefinitionModel));
            okResultActualObject.Alias.Should().Be(additionalPropertyDefinitionCreateOrUpdateModel.Alias);
            okResultActualObject.ContextType.Should().Be(AdditionalPropertyDefinitionContextType.Tenant);
            okResultActualObject.EntityType.Should().Be(additionalPropertyDefinitionCreateOrUpdateModel.EntityType);
            okResultActualObject.Name.Should().Be(additionalPropertyDefinitionCreateOrUpdateModel.Name);
            okResultActualObject.IsRequired.Should().Be(additionalPropertyDefinitionCreateOrUpdateModel.IsRequired);
            okResultActualObject.IsUnique.Should().Be(additionalPropertyDefinitionCreateOrUpdateModel.IsUnique);
        }

        [Fact]
        public void Update_ShouldReturnOk_WhenValidationSucceeds()
        {
            // Act
            var tenant = AdditionalPropertyFakeData.CreateFakeTenant();
            var additionalPropertyDefinitionUpdateModel =
                AdditionalPropertyFakeData.CreateFakeAdditionalPropertyUpdateModel();
            var additionalPropertyDefinitionModel = AdditionalPropertyFakeData
                .CreateFakeAdditionalPropertyModelOnCreate();
            var testIdOfAdditionalPropertyDefinitionReadModel = Guid.NewGuid();
            AdditionalPropertyDefinitionReadModel additionalPropertyReadModel =
                AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(
                    testIdOfAdditionalPropertyDefinitionReadModel);
            var additionalPropertyDefinition = AdditionalPropertyFakeData.CreateAdditionalPropertyDefinitionWithId(
                testIdOfAdditionalPropertyDefinitionReadModel);

            this.fixture.AdditionalPropertyValidatorMock.Setup(vm => vm.GetValidatorByContextType(
                Domain.Enums.AdditionalPropertyDefinitionContextType.Tenant))
                .Returns(this.fixture.AdditionalPropertyContextValidatorMock.Object);
            this.fixture.AdditionalPropertyModelResolver
                .Setup(vm => vm.ResolveToDefinitionModel(additionalPropertyDefinitionUpdateModel))
                .Returns(Task.FromResult(additionalPropertyDefinitionModel));

            additionalPropertyDefinitionUpdateModel.EntityType = AdditionalPropertyEntityType.Quote;
            this.fixture.MediatorMock.Setup(
                mo => mo.Send(It.IsAny<UpdateAdditionalPropertyDefinitionCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(additionalPropertyReadModel));
            additionalPropertyDefinitionUpdateModel.ContextType = AdditionalPropertyDefinitionContextType.Tenant;

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", tenant.Id.ToString()),
            }));

            this.fixture.Controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // Act
            var response = this.fixture.Controller.Update(
                testIdOfAdditionalPropertyDefinitionReadModel,
                additionalPropertyDefinitionUpdateModel).
                Result;
            var okResult = response as OkObjectResult;
            var okResultActualObject = okResult.Value as AdditionalPropertyDefinitionModel;

            // Assert
            okResult.Value.Should().BeOfType(typeof(AdditionalPropertyDefinitionModel));
            okResultActualObject.Alias.Should().Be(additionalPropertyDefinitionUpdateModel.Alias);
            okResultActualObject.Name.Should().Be(additionalPropertyDefinitionUpdateModel.Name);
            okResultActualObject.IsRequired.Should().Be(additionalPropertyDefinitionUpdateModel.IsRequired);
            okResultActualObject.IsUnique.Should().Be(
                additionalPropertyDefinitionUpdateModel.IsUnique);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_WhenValidationSucceedsAsync()
        {
            // Assign
            var fakeId = Guid.NewGuid();
            var tenant = AdditionalPropertyFakeData.CreateFakeTenant();
            var readModel = AdditionalPropertyFakeData.CreateFakeAdditionalPropertyDefinitionReadModel(fakeId);
            var additionalPropertDefinition = AdditionalPropertyFakeData
                .CreateAdditionalPropertyDefinitionWithId(fakeId);
            readModel.IsDeleted = true;

            this.fixture.MediatorMock.Setup(
                mo => mo.Send(It.IsAny<DeleteAdditionalPropertyDefinitionCommand>(), It.IsAny<CancellationToken>())).Returns(
                Task.FromResult(Unit.Value));
            this.fixture.AdditionalPropertyValidatorMock.Setup(vm => vm.GetValidatorByContextType(
                Domain.Enums.AdditionalPropertyDefinitionContextType.Tenant))
                                               .Returns(this.fixture.AdditionalPropertyContextValidatorMock.Object);

            this.fixture.CachingResolverMock.Setup(t => t.GetTenantOrThrow(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(tenant));

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", tenant.Id.ToString()),
            }));

            this.fixture.Controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // Act
            IActionResult result = await this.fixture.Controller.Delete(fakeId, tenant.Id.ToString());
            var okResult = result as OkResult;

            // Assert
            okResult.StatusCode.Should().Be(200);
        }
    }
}
