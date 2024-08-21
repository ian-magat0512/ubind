// <copyright file="AutomationServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Queries.Services;
    using UBind.Application.Queries.Tenant;
    using UBind.Application.Releases;
    using UBind.Application.Services;
    using UBind.Application.Services.Email;
    using UBind.Application.Services.Imports;
    using UBind.Application.SystemEvents;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using Automation = UBind.Application.Automation.Automation;
    using IClock = NodaTime.IClock;

    public class AutomationServiceTests
    {
        private readonly Mock<IServiceProvider> mockServiceProvider;
        private readonly Mock<ICachingResolver> mockCachingResolver;
        private readonly Mock<IAutomationConfigurationProvider> mockConfigurationProvider;
        private readonly Mock<ISystemEventService> mockSystemEventService;
        private readonly Mock<IAutomationPortalPageTriggerService> mockPortalPageTriggerService;
        private readonly Mock<IReleaseQueryService> mockReleaseQueryService;
        private Mock<IProductRepository> mockProductRepository;
        private Mock<ITenantRepository> mockTenantRepository;
        private Mock<IFeatureSettingRepository> mockFeatureSettingRepository;
        private Mock<IProductFeatureSettingRepository> mockProductFeatureSettingRepository;
        private ServiceProvider serviceProvider;
        private CachingResolver cachingResolver;
        private ICqrsMediator mediator;

        public AutomationServiceTests()
        {
            this.mockServiceProvider = new Mock<IServiceProvider>();
            this.mockCachingResolver = new Mock<ICachingResolver>();
            this.mockConfigurationProvider = new Mock<IAutomationConfigurationProvider>();
            this.mockSystemEventService = new Mock<ISystemEventService>();
            this.mockPortalPageTriggerService = new Mock<IAutomationPortalPageTriggerService>();
            this.mockTenantRepository = new Mock<ITenantRepository>();
            this.mockProductRepository = new Mock<IProductRepository>();
            this.mockFeatureSettingRepository = new Mock<IFeatureSettingRepository>();
            this.mockProductFeatureSettingRepository = new Mock<IProductFeatureSettingRepository>();
            this.mockReleaseQueryService = new Mock<IReleaseQueryService>();
            var services = new ServiceCollection();
            var internalUrlConfiguration = new InternalUrlConfiguration() { BaseApi = "https://localhost:44301" };
            services.AddTransient<IClock>(c => new TestClock());
            services.AddSingleton<IInternalUrlConfiguration>(x => internalUrlConfiguration);
            services.AddTransient<ICachingResolver>(c => this.cachingResolver);
            services.AddTransient<ITenantRepository>(c => this.mockTenantRepository.Object);
            services.AddTransient<IErrorNotificationService>(c => new Mock<IErrorNotificationService>().Object);
            services.AddSingleton<ICqrsMediator, CqrsMediator>();
            services.AddSingleton<ICqrsRequestContext>(_ => new CqrsRequestContext());
            services.AddLogging(loggingBuilder => loggingBuilder.AddDebug());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetTenantByIdQuery).GetTypeInfo().Assembly));
            this.serviceProvider = services.BuildServiceProvider();
            this.mediator = this.serviceProvider.GetService<ICqrsMediator>();
            this.cachingResolver = new CachingResolver(
                this.mediator,
                this.mockTenantRepository.Object,
                this.mockProductRepository.Object,
                this.mockFeatureSettingRepository.Object,
                this.mockProductFeatureSettingRepository.Object);
        }

        [Theory]
        [InlineData("ACL")]
        [InlineData("BASELINE-CONTROL")]
        [InlineData("BIND")]
        [InlineData("CHECKIN")]
        [InlineData("CHECKOUT")]
        [InlineData("CONNECT")]
        [InlineData("COPY")]
        [InlineData("DELETE")]
        [InlineData("GET")]
        [InlineData("HEAD")]
        [InlineData("LABEL")]
        [InlineData("LINK")]
        [InlineData("LOCK")]
        [InlineData("MERGE")]
        [InlineData("MKACTIVITY")]
        [InlineData("MKCALENDAR")]
        [InlineData("MKCOL")]
        [InlineData("MKREDIRECTREF")]
        [InlineData("MKWORKSPACE")]
        [InlineData("MOVE")]
        [InlineData("OPTIONS")]
        [InlineData("ORDERPATCH")]
        [InlineData("PATCH")]
        [InlineData("POST")]
        [InlineData("PRI")]
        [InlineData("PROPFIND")]
        [InlineData("PROPPATCH")]
        [InlineData("PUT")]
        [InlineData("REBIND")]
        [InlineData("REPORT")]
        [InlineData("SEARCH")]
        [InlineData("TRACE")]
        [InlineData("UNBIND")]
        [InlineData("UNCHECKOUT")]
        [InlineData("UNLINK")]
        [InlineData("UNLOCK")]
        [InlineData("UPDATE")]
        [InlineData("UPDATEREDIRECTREF")]
        [InlineData("VERSION-CONTROL")]
        public async Task TriggerHttpAutomation_ShouldExecuteProperly_WhenTriggerIsMatchedFromAutomationData(string httpMethod)
        {
            // Arrange
            var automationService = new AutomationService(
                this.mockConfigurationProvider.Object,
                this.mockSystemEventService.Object,
                this.serviceProvider,
                this.mockCachingResolver.Object,
                this.mockPortalPageTriggerService.Object,
                this.mockReleaseQueryService.Object);
            var triggerRequest = new TriggerRequest(
                "https://localhost/api/v1/tenant/test/product/test/environment/development/automation/testObjectPathLookupFile",
                httpMethod,
                string.Empty,
                new Dictionary<string, StringValues>
                {
                    { "Connection", "Keep-Alive" },
                    { "Accept", "*/*" },
                    { "Accept-Encoding", "gzip, deflate, br" },
                });
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var productId = Guid.NewGuid();
            var product = new Domain.Product.Product(TenantFactory.DefaultId, productId, "product_alias", productId.ToString(), SystemClock.Instance.GetCurrentInstant());
            var triggers = new List<Trigger>();
            var triggerRequestEndpoint = new TriggerRequestEndpoint("testObjectPathLookupFile", httpMethod, Enumerable.Empty<ErrorCondition>());
            var httpResponse = new HttpResponse(
                    new StaticProvider<Data<long>>(200),
                    Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>(),
                    new StaticProvider<Data<string>>("application/json"),
                    new TextContentProvider(new StaticProvider<Data<string>>("sample content")));
            var httpTrigger = new HttpTrigger("sample http trigger", "sample alias", "sample description", null, triggerRequestEndpoint, null, httpResponse);
            triggers.Add(httpTrigger);
            var automation = new Automation("periodic", "alias", "desc", null, triggers, Enumerable.Empty<UBind.Application.Automation.Actions.Action>(), null);
            var automations = new List<Automation>();
            automations.Add(automation);
            var automationConfiguration = new AutomationsConfiguration(automations);
            this.mockCachingResolver.Setup(cr => cr.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(product));
            this.mockConfigurationProvider.Setup(cp => cp.GetAutomationConfigurationOrThrow(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DeploymentEnvironment>(),
                It.IsAny<Guid>()))
                .Returns(Task.FromResult(automationConfiguration));
            var tenant = new Tenant(Guid.NewGuid(), "test tenant", "alias", null, default, default, SystemClock.Instance.Now());
            this.mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            var releaseContext = new ReleaseContext(
                tenant.Id,
                product.Id,
                DeploymentEnvironment.Development,
                Guid.NewGuid());

            // Act
            var result = await automationService.TriggerHttpAutomation(
                releaseContext,
                Guid.Empty,
                triggerRequest,
                token);

            var triggerData = result.Trigger as HttpTriggerData;

            // Assert
            result.Should().NotBeNull();
            triggerData.HttpResponse.Content.ToString().Should().Be("sample content");
            triggerData.HttpResponse.HttpStatusCode.Should().Be(200);
        }

        [Fact]
        public async Task TriggerHttpAutomation_ShouldExecuteTheTriggerWithTheClosestMatch_When2SimilarTriggersInSameAutomation()
        {
            // Arrange
            var automationService = new AutomationService(
                this.mockConfigurationProvider.Object,
                this.mockSystemEventService.Object,
                this.serviceProvider,
                this.mockCachingResolver.Object,
                this.mockPortalPageTriggerService.Object,
                this.mockReleaseQueryService.Object);
            var triggerRequest = new TriggerRequest(
                "https://localhost/api/v1/tenant/test/product/test/environment/development/automation/gizmo/99/fold",
                "GET",
                string.Empty,
                new Dictionary<string, StringValues>
                {
                    { "Connection", "Keep-Alive" },
                    { "Accept", "*/*" },
                    { "Accept-Encoding", "gzip, deflate, br" },
                });
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var productId = Guid.NewGuid();
            var product = new Domain.Product.Product(
                TenantFactory.DefaultId, productId, "product_alias", productId.ToString(), SystemClock.Instance.GetCurrentInstant());
            var triggers = new List<Trigger>();

            // Trigger 1
            var trigger1RequestEndpoint = new TriggerRequestEndpoint("gizmo/{gizmoId}/{action}", "GET", Enumerable.Empty<ErrorCondition>());
            var httpResponse1 = new HttpResponse(
                    new StaticProvider<Data<long>>(200),
                    Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>(),
                    new StaticProvider<Data<string>>("application/json"),
                    new TextContentProvider(new StaticProvider<Data<string>>("trigger 1 matched")));
            var httpTrigger1 = new HttpTrigger("HTTP Trigger 1", "httpTrigger1", null, null, trigger1RequestEndpoint, null, httpResponse1);
            triggers.Add(httpTrigger1);

            // Trigger 2
            var trigger2RequestEndpoint = new TriggerRequestEndpoint("gizmo/{gizmoId}/fold", "GET", Enumerable.Empty<ErrorCondition>());
            var httpResponse2 = new HttpResponse(
                    new StaticProvider<Data<long>>(200),
                    Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>(),
                    new StaticProvider<Data<string>>("application/json"),
                    new TextContentProvider(new StaticProvider<Data<string>>("trigger 2 matched")));
            var httpTrigger2 = new HttpTrigger("HTTP Trigger 2", "httpTrigger2", null, null, trigger2RequestEndpoint, null, httpResponse2);
            triggers.Add(httpTrigger2);

            var automation = new Automation(
                "Testing HTTP endpoint selection",
                "testHttpEndpoints",
                "desc",
                null,
                triggers,
                Enumerable.Empty<UBind.Application.Automation.Actions.Action>(),
                null);
            var automations = new List<Automation>();
            automations.Add(automation);
            var automationConfiguration = new AutomationsConfiguration(automations);
            this.mockCachingResolver.Setup(cr => cr.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(product));
            this.mockConfigurationProvider.Setup(cp => cp.GetAutomationConfigurationOrThrow(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DeploymentEnvironment>(),
                It.IsAny<Guid>()))
                .Returns(Task.FromResult(automationConfiguration));
            var tenant = new Tenant(Guid.NewGuid(), "test tenant", "alias", null, default, default, SystemClock.Instance.Now());
            this.mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            var releaseContext = new ReleaseContext(
                tenant.Id,
                product.Id,
                DeploymentEnvironment.Development,
                Guid.NewGuid());

            // Act
            var result = await automationService.TriggerHttpAutomation(
                releaseContext,
                Guid.Empty,
                triggerRequest,
                token);

            var triggerData = result.Trigger as HttpTriggerData;

            // Assert
            result.Should().NotBeNull();
            triggerData.HttpResponse.Content.ToString().Should().Be("trigger 2 matched");
            triggerData.HttpResponse.HttpStatusCode.Should().Be(200);
        }

        [Fact]
        public async Task TriggerHttpAutomation_ShouldExecuteTheTriggerWithTheClosestMatch_When2SimilarTriggersInDifferentAutomations()
        {
            // Arrange
            var automationService = new AutomationService(
                this.mockConfigurationProvider.Object,
                this.mockSystemEventService.Object,
                this.serviceProvider,
                this.mockCachingResolver.Object,
                this.mockPortalPageTriggerService.Object,
                this.mockReleaseQueryService.Object);
            var triggerRequest = new TriggerRequest(
                "https://localhost/api/v1/tenant/test/product/test/environment/development/automation/gizmo/99/fold",
                "GET",
                string.Empty,
                new Dictionary<string, StringValues>
                {
                    { "Connection", "Keep-Alive" },
                    { "Accept", "*/*" },
                    { "Accept-Encoding", "gzip, deflate, br" },
                });
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var productId = Guid.NewGuid();
            var product = new Domain.Product.Product(
                TenantFactory.DefaultId, productId, "product_alias", productId.ToString(), SystemClock.Instance.GetCurrentInstant());

            // Trigger 1
            var trigger1RequestEndpoint = new TriggerRequestEndpoint("gizmo/{gizmoId}/{action}", "GET", Enumerable.Empty<ErrorCondition>());
            var httpResponse1 = new HttpResponse(
                    new StaticProvider<Data<long>>(200),
                    Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>(),
                    new StaticProvider<Data<string>>("application/json"),
                    new TextContentProvider(new StaticProvider<Data<string>>("trigger 1 matched")));
            var httpTrigger1 = new HttpTrigger("HTTP Trigger 1", "httpTrigger1", null, null, trigger1RequestEndpoint, null, httpResponse1);

            // Trigger 2
            var trigger2RequestEndpoint = new TriggerRequestEndpoint("gizmo/{gizmoId}/fold", "GET", Enumerable.Empty<ErrorCondition>());
            var httpResponse2 = new HttpResponse(
                    new StaticProvider<Data<long>>(200),
                    Enumerable.Empty<IProvider<KeyValuePair<string, IEnumerable<string>>>>(),
                    new StaticProvider<Data<string>>("application/json"),
                    new TextContentProvider(new StaticProvider<Data<string>>("trigger 2 matched")));
            var httpTrigger2 = new HttpTrigger("HTTP Trigger 2", "httpTrigger2", null, null, trigger2RequestEndpoint, null, httpResponse2);

            var automation1 = new Automation(
                "Testing HTTP endpoint selection 1",
                "testHttpEndpoints1",
                "desc",
                null,
                new List<Trigger> { httpTrigger1 },
                Enumerable.Empty<UBind.Application.Automation.Actions.Action>(),
                null);
            var automation2 = new Automation(
                "Testing HTTP endpoint selection 2",
                "testHttpEndpoints2",
                "desc",
                null,
                new List<Trigger> { httpTrigger2 },
                Enumerable.Empty<UBind.Application.Automation.Actions.Action>(),
                null);

            var automations = new List<Automation>();
            automations.Add(automation1);
            automations.Add(automation2);
            var automationConfiguration = new AutomationsConfiguration(automations);
            this.mockCachingResolver.Setup(cr => cr.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(product));
            this.mockConfigurationProvider.Setup(cp => cp.GetAutomationConfigurationOrThrow(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<DeploymentEnvironment>(),
                It.IsAny<Guid>()))
                .Returns(Task.FromResult(automationConfiguration));
            var tenant = new Tenant(Guid.NewGuid(), "test tenant", "alias", null, default, default, SystemClock.Instance.Now());
            this.mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            var releaseContext = new ReleaseContext(
                tenant.Id,
                product.Id,
                DeploymentEnvironment.Development,
                Guid.NewGuid());

            // Act
            var result = await automationService.TriggerHttpAutomation(
                releaseContext,
                Guid.Empty,
                triggerRequest,
                token);

            var triggerData = result.Trigger as HttpTriggerData;

            // Assert
            result.Should().NotBeNull();
            triggerData.HttpResponse.Content.ToString().Should().Be("trigger 2 matched");
            triggerData.HttpResponse.HttpStatusCode.Should().Be(200);
        }

        [Fact]
        public async Task TriggerHttpAutomation_ShouldThrowErrorException_WhenHttpVerbFormatIsInvalidAsync()
        {
            // Arrange
            var automationService = new AutomationService(
                this.mockConfigurationProvider.Object,
                this.mockSystemEventService.Object,
                this.mockServiceProvider.Object,
                this.mockCachingResolver.Object,
                this.mockPortalPageTriggerService.Object,
                this.mockReleaseQueryService.Object);
            var triggerRequest = new TriggerRequest(
                "https://localhost/api/v1/tenant/test/product/test/environment/development/automation/testObjectPathLookupFile",
                "2GET--POST",
                string.Empty,
                new Dictionary<string, StringValues>
                {
                    { "Connection", "Keep-Alive" },
                    { "Accept", "*/*" },
                    { "Accept-Encoding", "gzip, deflate, br" },
                });
            CancellationTokenSource source = new CancellationTokenSource();
            CancellationToken token = source.Token;
            var releaseContext = new ReleaseContext(
                Guid.Empty,
                Guid.Empty,
                DeploymentEnvironment.Development,
                Guid.NewGuid());

            // Act
            Func<Task> func = async () => await automationService.TriggerHttpAutomation(
                releaseContext,
                Guid.Empty,
                triggerRequest,
                token);

            // Assert
            var result = await func.Should().ThrowAsync<ErrorException>();
            result.Which.Should().NotBeNull();
            result.Which.Error.Code.Should().Be("automation.action.http.verb.invalid");
        }
    }
}
