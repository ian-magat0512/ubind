// <copyright file="AutomationPeriodicTriggerSchedulerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.Storage;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Releases;
    using UBind.Application.Services.Email;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Repositories;
    using Xunit;

    public class AutomationPeriodicTriggerSchedulerTests
    {
        [Fact]
        public async Task Scheduler_Should_Register_Jobs_For_PeriodicTriggers()
        {
            // Arrange
            var json = @"{
                        ""name"": ""Periodic Trigger Test 2"",
                        ""alias"": ""periodicTriggerTest2"",
                        ""description"": ""A test of the periodic trigger with time zone name/ID"",
                        ""timeZoneId"": ""Australia/Sydney"",
                        ""month"": {
                            ""every"": 1
                        },
                        ""day"": {
                            ""dayOfTheWeekOccurrenceWithinMonth"": {
                                ""dayOfTheWeek"": ""Monday"",
                                ""occurrence"": 2
                            }
                        },
                        ""hour"": {
                            ""every"": 1
                        },
                        ""minute"": {
                            ""every"": 3
                        }
                    }";

            var periodicTriggerConfigModel = JsonConvert.DeserializeObject<PeriodicTriggerConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = new Mock<IServiceProvider>();
            var periodicTrigger = (PeriodicTrigger)periodicTriggerConfigModel.Build(mockServiceProvider.Object);
            var triggers = new List<UBind.Application.Automation.Triggers.Trigger>();
            triggers.Add(periodicTrigger);
            var automation = new Automation("periodic", "alias", "desc", null, triggers, Enumerable.Empty<UBind.Application.Automation.Actions.Action>(), null);
            var productId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();

            var mockConfigProvider = new Mock<IAutomationConfigurationProvider>();
            var mockProductRepository = new Mock<IProductRepository>();
            var product = new Product(tenantId, productId, "product name", "alias", SystemClock.Instance.Now());
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false))
                .Returns(product);

            var mockTenantRepository = new Mock<ITenantRepository>();
            var tenant = new Tenant(tenantId, "test tenant", "alias", null, default, default, SystemClock.Instance.Now());
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>()))
                .Returns(tenant);
            var automations = new List<Automation>();
            automations.Add(automation);
            var automationConfig = new AutomationsConfiguration(automations);
            mockConfigProvider.Setup(c => c.GetAutomationConfiguration(It.IsAny<AutomationsConfigurationModel>()))
                .Returns(automationConfig);

            var mockRecurringJob = new Mock<IRecurringJobManager>();
            var mockStorageConnection = new Mock<IStorageConnection>();
            var mockReleaseQuery = new Mock<IReleaseQueryService>();
            var releaseContext = new ReleaseContext(tenant.Id, product.Id, DeploymentEnvironment.Development, Guid.NewGuid());
            mockReleaseQuery
                .Setup(s => s.GetDefaultReleaseContextOrNull(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DeploymentEnvironment>()))
                .Returns(releaseContext);
            var mockFieldBinder = new Mock<IFieldSerializationBinder>();
            var productFeature = new ProductFeatureSetting(tenantId, productId, SystemClock.Instance.Now());

            var release = FakeReleaseBuilder.CreateForProduct(tenant.Id, product.Id).WithAutomationsJson(json).BuildRelease();
            var cachedRelease = new ActiveDeployedRelease(release, DeploymentEnvironment.Development, mockFieldBinder.Object);
            var mockIServiceScopeFactory = new Mock<IServiceScopeFactory>();
            var mockServiceScope = new Mock<IServiceScope>();
            mockServiceProvider.Setup(m => m.GetService(typeof(IReleaseQueryService))).Returns(mockReleaseQuery.Object);
            mockIServiceScopeFactory.Setup(p => p.CreateScope()).Returns(mockServiceScope.Object);
            mockServiceScope.Setup(p => p.ServiceProvider).Returns(mockServiceProvider.Object);
            mockReleaseQuery.Setup(c => c.GetReleaseWithoutCachingOrAssets(It.IsAny<ReleaseContext>())).Returns(cachedRelease);

            var errorNotificationServiceMock = new Mock<IErrorNotificationService>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();

            var scheduler = new AutomationPeriodicTriggerScheduler(
                mockProductRepository.Object,
                mockTenantRepository.Object,
                mockRecurringJob.Object,
                null,
                mockReleaseQuery.Object,
                mockConfigProvider.Object,
                errorNotificationServiceMock.Object,
                loggerFactoryMock.Object);

            // Act
            await scheduler.RegisterPeriodicTriggerJobs(tenantId, productId, DeploymentEnvironment.Development);

            // Assert
            mockRecurringJob.Verify(c => c.AddOrUpdate(It.IsAny<string>(), It.IsAny<Job>(), periodicTrigger.GetCronSchedule(), It.IsAny<RecurringJobOptions>()));
        }
    }
}
