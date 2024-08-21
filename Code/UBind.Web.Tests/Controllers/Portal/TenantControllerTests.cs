// <copyright file="TenantControllerTests.cs" company="uBind">
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
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Moq;
    using UBind.Application;
    using UBind.Application.Commands.Tenant;
    using UBind.Application.Infrastructure;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="TenantControllerTests" />.
    /// </summary>
    public class TenantControllerTests
    {
        private readonly NodaTime.IClock clock = NodaTime.SystemClock.Instance;
        private readonly Mock<IAdditionalPropertyValueService> additionalPropertyValueServiceMock;
        private Mock<ICqrsMediator> mockMediator = new Mock<ICqrsMediator>();

        public TenantControllerTests()
        {
            this.additionalPropertyValueServiceMock = new Mock<IAdditionalPropertyValueService>();
        }

        /// <summary>
        /// Test for creating new tenant.
        /// </summary>
        [Fact]
        public async void CreateTenant_Succeeds_When_Model_Is_Valid()
        {
            // Arrange
            var mockTenantRepository = new Mock<ITenantRepository>();
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var mockTenantService = new Mock<ITenantService>();
            var mockcachingResolver = new Mock<ICachingResolver>();
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId))
              .Returns(
                    new System.Security.Claims.Claim(ClaimNames.TenantId, Tenant.MasterTenantId.ToString()));

            var context = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            var tenantController = new TenantController(
                this.mockMediator.Object,
                mockcachingResolver.Object,
                contextAccessor.Object,
                this.additionalPropertyValueServiceMock.Object)
            {
                ControllerContext = context,
            };

            var tenant = new Tenant(Guid.NewGuid(), "foo", "foo", null, default, default, this.clock.Now());
            var model = new TenantModel(tenant);

            this.mockMediator.Setup(m => m.Send(It.IsAny<CreateTenantCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant.Id);
            this.mockMediator.Setup(m => m.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tenant);

            // Act
            var response = await tenantController.CreateTenant(model);

            // Assert
            var result = response as OkObjectResult;
            result.Value.Should().NotBeNull();
            result.Value.Should().BeOfType<TenantModel>();

            // verify all setup is called.
            this.mockMediator.Verify(m => m.Send(It.IsAny<CreateTenantCommand>(), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async void RedirectToCustomDomainAsync_RedirectsToCustomDomain_WhenTenantHasCustomDomain()
        {
            // Arrange
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockTenantService = new Mock<ITenantService>();
            var contextAccessor = new Mock<HttpContextAccessor>();
            var mockTenantAndProductResolver = new Mock<ICachingResolver>();
            var mockRoleService = new Mock<IRoleService>();
            var userTenantId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var customDomain = "domain.com";
            var additionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();

            var httpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, userTenantId.ToString()));
            var tenant = new Tenant(tenantId, tenantId.ToString(), tenantId.ToString(), customDomain, default, default, this.clock.Now());
            this.mockMediator.Setup(e => e.Send(It.IsAny<GetTenantByCustomDomainQuery>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(tenant));
            var context = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));

            var tenantController = new TenantController(
                this.mockMediator.Object,
                mockTenantAndProductResolver.Object,
                contextAccessor.Object,
                additionalPropertyValueService.Object)
            {
                ControllerContext = context,
            };

            // Act
            var response = await tenantController.RedirectToCustomDomainAsync();

            // Assert
            var result = response as RedirectResult;
            result.Permanent.Should().Be(true);
        }

        [Fact]
        public async void RedirectToCustomDomainAsync_ReturnsOk_WhenTenantHasNoCustomDomain()
        {
            // Arrange
            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockTenantService = new Mock<ITenantService>();
            var mockDomainResolver = new Mock<IHttpContextAccessor>();
            var mockTenantAndProductResolver = new Mock<ICachingResolver>();
            var mockRoleService = new Mock<IRoleService>();
            var userTenantId = "foo";
            var additionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();

            var httpContext = new Mock<Microsoft.AspNetCore.Http.HttpContext>();
            httpContext.Setup(m => m.User.FindFirst(ClaimNames.TenantId)).Returns(new System.Security.Claims.Claim(ClaimNames.TenantId, userTenantId));
            var context = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
            var tenantController = new TenantController(
                this.mockMediator.Object,
                mockTenantAndProductResolver.Object,
                mockDomainResolver.Object,
                additionalPropertyValueService.Object)
            {
                ControllerContext = context,
            };

            // Act
            var response = await tenantController.RedirectToCustomDomainAsync();

            // Assert
            var result = response as OkResult;
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }
}
