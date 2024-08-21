// <copyright file="RaiseEventActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Actions
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.SystemEvents;
    using UBind.Application.SystemEvents.Payload;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Processing;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Services;
    using Xunit;
    using Action = UBind.Application.Automation.Actions.Action;

    /// <summary>
    /// Unit tests for the different automation actions.
    /// </summary>
    [SystemEventTypeExtensionInitialize]

    public class RaiseEventActionTests : IAsyncLifetime
    {
        private IServiceProvider dependencyProvider = new Mock<IServiceProvider>().Object;
        private FakeSystemEventRepository systemEventRepository = new FakeSystemEventRepository();

        public async Task InitializeAsync()
        {
            var mockCachingResolver = new Mock<ICachingResolver>();
            var tempDependencyProvider = new Mock<IServiceProvider>();
            var mockAutomationEventTriggerService = new Mock<IAutomationEventTriggerService>();
            var mockBackgroundJobClient = new Mock<IJobClient>();
            var systemEventPersistenceService = new SystemEventPersistenceService(
                new Mock<IUBindDbContext>().Object,
                new Mock<IRelationshipRepository>().Object,
                new Mock<ITagRepository>().Object,
                this.systemEventRepository);
            var systemEventService = new SystemEventService(
                mockAutomationEventTriggerService.Object,
                mockBackgroundJobClient.Object,
                systemEventPersistenceService);
            var organisationService = new Mock<IOrganisationService>();
            organisationService
                .Setup(o => o.GetDefaultOrganisationForTenant(It.IsAny<Guid>()))
                .Returns(new OrganisationReadModelSummary
                {
                    TenantId = Guid.NewGuid(),
                    Id = Guid.NewGuid(),
                });
            var dataContext = await MockAutomationData.CreateWithHttpTrigger();
            var tenant = TenantFactory.Create(dataContext.ContextManager.Tenant.Id);
            var product = ProductFactory.Create(tenant.Id, dataContext.ContextManager.Tenant.Id);
            mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockCachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            tempDependencyProvider.Setup(x => x.GetService(typeof(ISystemEventService))).Returns(systemEventService);
            tempDependencyProvider.Setup(x => x.GetService(typeof(ISystemEventRepository))).Returns(this.systemEventRepository);
            tempDependencyProvider.Setup(x => x.GetService(typeof(IClock))).Returns(new TestClock());
            tempDependencyProvider.Setup(x => x.GetService(typeof(IOrganisationService))).Returns(organisationService.Object);
            tempDependencyProvider.AddLoggers();
            tempDependencyProvider.Setup(x => x.GetService(typeof(ICachingResolver))).Returns(mockCachingResolver.Object);
            this.dependencyProvider = tempDependencyProvider.Object;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task ExecuteAction_Persisted_ExecuteAction()
        {
            // Arrange
            var actionConfig = EventAutomationTestJson.GetBasic();
            var dataContext = await MockAutomationData.CreateWithHttpTrigger();
            var raiseEventAction = this.CreateRaiseEventActionUsingJson(actionConfig, this.dependencyProvider);
            var actionData = raiseEventAction.CreateActionData();
            dataContext.AddActionData(actionData);

            // Act
            await raiseEventAction.Execute(new ProviderContext(dataContext), actionData);

            // Assert
            var eventRecords = this.systemEventRepository.GetAll();
            eventRecords.Should().HaveCount(1);
            eventRecords.First().Id.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ExecuteAction_CreatesSystemEvent_WithPerformingUser()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var productContext = new ProductContext(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DeploymentEnvironment.Development);
            var organisationId = Guid.NewGuid();
            var systemEvent = SystemEvent.CreateWithPayload(
                productContext.TenantId,
                organisationId,
                productContext.ProductId,
                productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                default);
            systemEvent.AddRelationshipToEntity(RelationshipType.EventPerformingUser, EntityType.User, Guid.NewGuid());
            var dataContext = MockAutomationData.CreateFromSystemEvent(systemEvent, null, MockAutomationData.GetDefaultServiceProvider());
            dataContext.ContextManager.SetContextFromEventRelationships(systemEvent.Relationships);

            // Act
            var actionConfig = EventAutomationTestJson.GetBasic();
            var raiseEventAction = this.CreateRaiseEventActionUsingJson(actionConfig, this.dependencyProvider);
            var actionData = raiseEventAction.CreateActionData();
            dataContext.AddActionData(actionData);

            // Act
            await raiseEventAction.Execute(new ProviderContext(dataContext), actionData);

            // Assert
            var eventRecords = this.systemEventRepository.GetAll();
            eventRecords.Should().HaveCount(1);
            eventRecords.First().Relationships.Should().HaveCount(1);
            eventRecords.First().Relationships.FirstOrDefault(r => r.Type == RelationshipType.EventPerformingUser).Should().NotBeNull();
        }

        [Fact]
        public async Task ExecuteAction_CreatesSystemEvent_WithoutPerformingUser()
        {
            // Arrange
            var payload = new QuoteEventPayload(Guid.NewGuid(), Guid.NewGuid());
            var productContext = new ProductContext(
                Guid.NewGuid(),
                Guid.NewGuid(),
                DeploymentEnvironment.Development);
            var organisationId = Guid.NewGuid();
            var systemEvent = SystemEvent.CreateWithPayload(
                productContext.TenantId,
                organisationId,
                productContext.ProductId,
                productContext.Environment,
                SystemEventType.CustomerExpiredQuoteOpened,
                payload,
                default);
            var dataContext = MockAutomationData.CreateFromSystemEvent(systemEvent, null, MockAutomationData.GetDefaultServiceProvider());
            dataContext.ContextManager.SetContextFromEventRelationships(systemEvent.Relationships);

            // Act
            var actionConfig = EventAutomationTestJson.GetBasic();
            var raiseEventAction = this.CreateRaiseEventActionUsingJson(actionConfig, this.dependencyProvider);
            var actionData = raiseEventAction.CreateActionData();
            dataContext.AddActionData(actionData);

            // Act
            await raiseEventAction.Execute(new ProviderContext(dataContext), actionData);

            // Assert
            var eventRecords = this.systemEventRepository.GetAll();
            eventRecords.Should().HaveCount(1);
            eventRecords.First().Relationships.Should().HaveCount(0);
            dataContext.ContextManager.PerformingUser.Should().BeNull();
        }

        private RaiseEventAction CreateRaiseEventActionUsingJson(string actionConfig, IServiceProvider serviceProvider)
        {
            var actionModel = JsonConvert.DeserializeObject<IBuilder<Action>>(
                actionConfig, AutomationDeserializationConfiguration.ModelSettings);
            var action = actionModel!.Build(serviceProvider ?? this.dependencyProvider);
            return (RaiseEventAction)action;
        }

        private static class EventAutomationTestJson
        {
            private static string mainJsonAction = @"{
                        ""raiseEventAction"": {
                            ""name"": ""Raise Event Action Test"",
                            ""alias"": ""raiseEventActionTest"",
                            ""description"": ""A test of the raise event action"",
                            ""asynchronous"": false,
                           ";

            private static string runConditionJsonTrue = @"""runCondition"": {
                                        ""textStartsWithCondition"": {
                                            ""text"": ""pikachu"",
                                            ""startsWith"": ""pika""
                                        }
                                        }, ";

            private static string runConditionJsonFalse = @"""runCondition"": {
                                        ""textStartsWithCondition"": {
                                            ""text"": ""pikachu"",
                                            ""startsWith"": ""chu""
                                        }
                                        }, ";

            private static string afterRunErrorConditionFalse = @"""afterRunErrorConditions"": [
                                {
                                    ""condition"": {
                                        ""textEndsWithCondition"": {
                                            ""text"": ""pikachu"",
                                            ""endsWith"": ""pika""
                                        }
                                        },
                                        ""error"": {
                                        ""code"": ""some.error"",
                                        ""title"": ""An error has occurred"",
                                        ""message"": ""You need to fix the error"",
                                        ""httpStatusCode"": 400
                                        }
                                }
                            ],";

            private static string afterRunErrorConditionTrue = @"""afterRunErrorConditions"": [
                                {
                                    ""condition"": {
                                        ""textEndsWithCondition"": {
                                            ""text"": ""pikachu"",
                                            ""endsWith"": ""chu""
                                        }
                                        },
                                        ""error"": {
                                        ""code"": ""some.error"",
                                        ""title"": ""An error has occurred"",
                                        ""message"": ""You need to fix the error"",
                                        ""httpStatusCode"": 400
                                        }
                                }
                            ],";

            private static string beforeRunErrorConditionFalse = @"""beforeRunErrorConditions"": [
                                {
                                    ""condition"": {
                                        ""textEndsWithCondition"": {
                                            ""text"": ""pikachu"",
                                            ""endsWith"": ""pika""
                                        }
                                        },
                                        ""error"": {
                                        ""code"": ""some.error"",
                                        ""title"": ""An error has occurred"",
                                        ""message"": ""You need to fix the error"",
                                        ""httpStatusCode"": 400
                                        }
                                }
                            ],";

            private static string beforeRunErrorConditionTrue = @"""beforeRunErrorConditions"": [
                                {
                                    ""condition"": {
                                        ""textEndsWithCondition"": {
                                            ""text"": ""pikachu"",
                                            ""endsWith"": ""chu""
                                        }
                                        },
                                        ""error"": {
                                        ""code"": ""some.error"",
                                        ""title"": ""An error has occurred"",
                                        ""message"": ""You need to fix the error"",
                                        ""httpStatusCode"": 400
                                        }
                                }
                            ],";

            private static string eventJson = @"""customEventAlias"": ""MyTestEvent"",
                                        ""eventData"": [
                                            {
                                                ""propertyName"": ""item1"",
                                                ""value"": ""textValue""
                                            }
                                        ],
                                        ""eventTags"": [""somethingqwe""],
                                        ""eventPersistanceDuration"": ""P1Y4M2DT10H31M3.452S""";

            public static string GetBasic()
            {
                return mainJsonAction + eventJson + "}}";
            }

            public static string GetWithTrueRunCondition()
            {
                return mainJsonAction + runConditionJsonTrue + eventJson + "}}";
            }

            public static string GetWithFalseRunCondition()
            {
                return mainJsonAction + runConditionJsonFalse + eventJson + "}}";
            }

            public static string GetWithTrueRunConditionAndAfterRunErrorConditionFalse()
            {
                return mainJsonAction + runConditionJsonTrue + afterRunErrorConditionFalse + eventJson + "}}";
            }

            public static string GetWithTrueRunConditionAndAfterRunErrorConditionTrue()
            {
                return mainJsonAction + runConditionJsonTrue + afterRunErrorConditionTrue + eventJson + "}}";
            }

            public static string GetWithTrueRunConditionAndABeforeRunErrorConditionFalse()
            {
                return mainJsonAction + runConditionJsonTrue + beforeRunErrorConditionFalse + eventJson + "}}";
            }

            public static string GetWithTrueRunConditionAndBeforeRunErrorConditionTrue()
            {
                return mainJsonAction + runConditionJsonTrue + beforeRunErrorConditionTrue + eventJson + "}}";
            }
        }

        /// <summary>
        /// The sample product service to sample system events.
        /// </summary>
#pragma warning disable SA1402 // File may only contain a single type
        private class TestProductService : IObserver<SystemEvent>
#pragma warning restore SA1402 // File may only contain a single type
        {
            public TestProductService()
            {
                this.Value = string.Empty;
                this.Triggered = false;
            }

            /// <summary>
            /// Gets the value.
            /// </summary>
            public string Value { get; private set; }

            /// <summary>
            /// Gets a value indicating whether if the event is triggered.
            /// </summary>
            public bool Triggered { get; private set; }

            public Product CreateOrUpdateProduct(string productId, string tenantId, string name, bool disabled, bool deleted)
            {
                throw new NotImplementedException();
            }

            public Product CreateProduct(string productId, string tenantId, string name)
            {
                throw new NotImplementedException();
            }

            public ProductDeploymentSetting GetDeploymentSettings(string productId, string tenantId)
            {
                throw new NotImplementedException();
            }

            public Product GetProductByTenantId(string tenantId, string productId)
            {
                throw new NotImplementedException();
            }

            public Task InitializeProduct(string tenantId, string productId)
            {
                throw new NotImplementedException();
            }

            public Task SeedFilesAsync(string tenantId, string productId, string environment, FileModel file, string folder)
            {
                throw new NotImplementedException();
            }

            public Product UpdateDeploymentSettings(string productId, string tenantId, ProductDeploymentSetting deploymentSettings)
            {
                throw new NotImplementedException();
            }

            public Product UpdateProduct(string productId, string tenantId, string name, bool disabled, bool deleted, QuoteExpirySettings? productQuoteExpirySetting = null, ProductQuoteExpirySettingUpdateType updateType = ProductQuoteExpirySettingUpdateType.UpdateNone)
            {
                throw new NotImplementedException();
            }

            public Product UpdateQuoteExpirySettings(string productId, string tenantId, QuoteExpirySettings expirySettings, ProductQuoteExpirySettingUpdateType updateType)
            {
                throw new NotImplementedException();
            }

            public void OnNext(SystemEvent systemEvent)
            {
                this.Triggered = true;
                this.Value = systemEvent.PayloadJson;
            }

            public void OnError(Exception error)
            {
                this.Value = "error";
            }

            public void OnCompleted()
            {
                this.Value = "done";
            }
        }
    }
}
