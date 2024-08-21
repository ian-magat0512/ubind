// <copyright file="AutomationIntegrationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Flurl.Http.Testing;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.SystemEvents;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Integration tests for automations.
    /// </summary>
    [SystemEventTypeExtensionInitialize]
    public class AutomationIntegrationTests
    {
        private readonly IServiceProvider dependencyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationIntegrationTests"/> class.
        /// </summary>
        public AutomationIntegrationTests()
        {
            var mock = new Mock<IServiceProvider>();
            var mockcachingResolver = new Mock<ICachingResolver>();
            var jobClientMock = new Mock<IJobClient>();
            var organisationService = new Mock<IOrganisationService>();
            organisationService
                .Setup(o => o.GetDefaultOrganisationForTenant(It.IsAny<Guid>()))
                .Returns(new OrganisationReadModelSummary
                {
                    TenantId = Guid.NewGuid(),
                    Id = Guid.NewGuid(),
                });
            MockAutomationData dataContext = MockAutomationData.CreateWithEventTrigger();
            var mockMediator = new Mock<ICqrsMediator>();
            var tenant = TenantFactory.Create(dataContext.ContextManager.Tenant.Id);
            var product = ProductFactory.Create(tenant.Id, dataContext.ContextManager.Product.Id);
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mock.Setup(x => x.GetService(typeof(ISystemEventService))).Returns(new Mock<ISystemEventService>().Object);
            mock.Setup(x => x.GetService(typeof(ISystemEventRepository))).Returns(new Mock<ISystemEventRepository>().Object);
            mock.Setup(x => x.GetService(typeof(IOrganisationService))).Returns(organisationService.Object);
            mock.Setup(x => x.GetService(typeof(NodaTime.IClock))).Returns(NodaTime.SystemClock.Instance);
            mock.Setup(x => x.GetService(typeof(IActionRunner)))
                .Returns(new ActionRunner(jobClientMock.Object, mockcachingResolver.Object, mockMediator.Object));
            mock.Setup(x => x.GetService(typeof(ICachingResolver))).Returns(mockcachingResolver.Object);
            this.dependencyProvider = mock.Object;
        }

        [Fact]
        public async Task AutomationTest_ShouldReturnResponse_RenderedBasedOnTemplate()
        {
            var dataContext = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var configString = @"{
                    ""schemaVersion"": ""1.0.0"",
                    ""automations"": [
                        {
                            ""name"": ""Test"",
                            ""alias"": ""testString"",
                            ""description"": ""test config for automation matching"",
                            ""triggers"": [
                                {
                                    ""httpTrigger"": {
                                        ""name"": ""Foo HTTP Trigger"",
                                        ""alias"": ""fooTrigger"",
                                        ""description"": ""automation should be triggered by request to foo endpoint"",
                                        ""endpoint"": {
                                            ""path"": ""addressMatch"",
                                            ""httpVerb"": ""POST""
                                        },
                                        ""httpResponse"": {
                                            ""httpStatusCode"" : 200,
                                            ""contentType"" : ""application/json"",
                                            ""content"" : {
                                                ""liquidText"": {
                                                    ""liquidTemplate"": ""Ash's pokemon is called {{name}}"",
                                                    ""dataObject"" : {
                                                        ""jsonObject"": {
                                                            ""objectPathLookupText"": ""/actions/fooAction/httpResponse/content""
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            ],
                            ""actions"": [
                                {
                                    ""httpRequestAction"": {
                                        ""name"": ""Foo Http Action"",
                                        ""alias"": ""fooAction"",
                                        ""description"": ""running foo action"",
                                        ""asynchronous"": false,
                                        ""httpRequest"": {
                                            ""httpVerb"": ""GET"",
                                            ""url"": ""https://test.com/pikachu"",
                                            ""headers"": [
                                                {
                                                    ""something"": ""value""
                                                }
                                            ]
                                        }
                                    }
                                }
                            ]
                        }
                    ]
            }";

            var automationConfigurationModel = AutomationConfigurationParser.Parse(configString);
            var automationConfiguration = automationConfigurationModel.Build(this.dependencyProvider);
            var (matchingAutomation, trigger) = await automationConfiguration.GetClosestMatchingHttpTrigger(dataContext);
            var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { name = "pikachu" }, 200);

            // Act
            using (httpTest)
            {
                await matchingAutomation.Execute(dataContext);
            }

            // Assert
            var updateDataContextHttpTrigger = (HttpTriggerData)dataContext.Trigger!;
            var actionData = dataContext.Actions.First().Value as HttpRequestActionData;
            var updateDataContextResponse = updateDataContextHttpTrigger.HttpResponse;
            var updatedDataContextResponseContent = updateDataContextResponse.Content;
            var responseCode = updateDataContextResponse.HttpStatusCode;
            var responseContentType = updateDataContextResponse.ContentType;

            matchingAutomation.Should().NotBeNull();
            matchingAutomation.Alias.Should().Be("testString");
            responseCode.Should().Be(200);
            responseContentType.Should().Be("application/json");

            updatedDataContextResponseContent.Should().NotBeNull();
            string contentString = updatedDataContextResponseContent as string ?? string.Empty;
            contentString.Should().Be("Ash's pokemon is called pikachu");
            actionData?.HttpResponse.Should().NotBeNull();
            dataContext.Actions.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutomationTest_ShouldReturnResponse_ParseRaiseEventAction()
        {
            string eventJson = @"{
                ""type"": ""eventTrigger"",
                ""eventType"": ""custom"",
                ""customEventAlias"": ""MyTestEvent"",
                ""triggerAlias"": ""aliasName"",
                ""eventData"": {
                    ""address"": ""215 gen"",
                    ""name"":""jeo""
                }
            }";
            MockAutomationData dataContext = MockAutomationData.CreateWithEventTrigger(json: eventJson);
            var configString = @"{
                ""schemaVersion"": ""1.0.0"",
                ""automations"": [
                    {
                        ""name"": ""Test"",
                        ""alias"": ""testString"",
                        ""description"": ""test config for automation matching"",
                        ""triggers"": [
                            {
                                ""eventTrigger"": {
                                    ""name"": ""Foo Event Trigger"",
                                    ""alias"": ""eventTriggerTest"",
                                    ""eventType"": ""custom"",
                                    ""description"": ""A test of the event trigger"",
                                    ""customEventAlias"": ""MyTestEvent"",
                                    ""asynchronous"": false
                                }
                            }
                        ],
                        ""actions"": [
                            {
                                ""raiseEventAction"": {
                                    ""name"": ""Event Action Test"",
                                    ""alias"": ""eventActionTest"",
                                    ""description"": ""A test of the event action"",
                                    ""asynchronous"": false,
                                    ""customEventAlias"": ""MyTestEvent"",
                                    ""eventData"": {
                                            ""objectPathLookupObject"": ""/trigger/eventData""
                                    },
                                    ""eventTags"": [""somethingqwe""],
                                    ""eventPersistanceDuration"": ""P1Y4M2DT10H31M3.452S""
                                }
                            },
                            {
                                ""raiseEventAction"": {
                                    ""name"": ""Event Action Test2"",
                                    ""alias"": ""eventActionTest2"",
                                    ""customEventAlias"": ""MyTestEvent2"",
                                    ""description"": ""A test of the event action"",
                                    ""asynchronous"": false,
                                    ""eventTags"": [""somethingqwe""],
                                    ""eventPersistanceDuration"": {  
                                                    ""periodTypeValueDuration"": {
                                                        ""value"": 1,
                                                        ""periodType"": ""month""
                                                    }
                                            },
                                }
                            },
                            {
                                ""raiseEventAction"": {
                                    ""name"": ""Event Action Test3"",
                                    ""alias"": ""eventActionTest3"",
                                    ""customEventAlias"": ""MyTestEvent3"",
                                    ""description"": ""A test of the event action"",
                                    ""asynchronous"": false,
                                    ""eventTags"": [""somethingqwe""]
                                }
                            }
                        ]
                    }
                ]
            }";

            var automationConfigurationModel = AutomationConfigurationParser.Parse(configString);
            var automationConfiguration = automationConfigurationModel.Build(this.dependencyProvider);

            // Act
            var automation = automationConfiguration.Automations.FirstOrDefault();
            if (automation != null)
            {
                await automation.Execute(dataContext);
            }

            // Assert
            var updateDataContextHttpTrigger = dataContext.Trigger as EventTriggerData;
            var actionData = dataContext.Actions.First().Value as RaiseEventActionData;
            var eventTriggerAlias = updateDataContextHttpTrigger?.TriggerAlias;
            actionData?.State.Should().Be(ActionState.Completed);
            eventTriggerAlias.Should().Be("eventTriggerTest");
        }

        [Fact]
        public async Task AutomationTest_ShouldReturnError_WhenRaisedErrorsAreUnhandled()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var configString = @"{
                ""schemaVersion"": ""1.0.0"",
                ""automations"": [
                    {
                            ""name"": ""test"",
                            ""alias"": ""testAutomation"",
                            ""triggers"": [
                                {
                                    ""httpTrigger"": {
                                        ""name"": ""testTrigger"",
                                        ""alias"": ""httpTriggerTest"",
                                        ""endpoint"": {
                                            ""path"": ""addressMatch"",
                                            ""httpVerb"":  ""POST""
                                        },
                                        ""httpResponse"":{
                                            ""contentType"": ""text/json"",
                                            ""content"": ""end""
                                        }
                                    }
                                }
                            ],
                            ""actions"": [
                                {
                                    ""httpRequestAction"": {
                                        ""name"": ""action test"",
                                        ""alias"": ""actionTest"",
                                        ""beforeRunErrorConditions"": [
                                            {
                                            ""condition"": true,
                                                ""error"": {
                                                    ""code"": ""test.error.raised"",
                                                    ""title"": ""Test Error Raised"",
                                                    ""message"": ""This error was raised from an unhandled beforeRunErrorCondition"",
                                                    ""httpStatusCode"": 400
                                                }
                                        }
                                        ],
                                        ""httpRequest"": {
                                            ""url"": ""www.test-uri.com""
                                        }
                                    }
                                }
                            ]
                        }
                    ]
                }";
            var automationsConfigModel = AutomationConfigurationParser.Parse(configString);
            var automations = automationsConfigModel.Build(this.dependencyProvider);

            // Act
            var (matchingAutomation, trigger) = await automations.GetClosestMatchingHttpTrigger(automationData);
            Func<Task> act = async () => await matchingAutomation.Execute(automationData);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            var actionData = automationData.Actions.FirstOrDefault().Value;
            exception.Which.Error.Title.Should().Be(automationData?.Error?.Title);
            exception.Which.Error.Message.Should().Be(automationData?.Error?.Message);
            exception.Which.Error.Should().NotBeNull();
            exception.Which.Error.Message.Should().Be("This error was raised from an unhandled beforeRunErrorCondition");
            exception.Which.Error.Title.Should().Be("Test Error Raised");
        }

        [Fact]
        public async Task AutomationTest_ShouldSucceed_WhenRaisedErrorWasHandled()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);
            var configString = @"{
                ""schemaVersion"": ""1.0.0"",
                ""automations"": [
                    {
                            ""name"": ""test"",
                            ""alias"": ""testAutomation"",
                            ""triggers"": [
                                {
                                    ""httpTrigger"": {
                                        ""name"": ""testTrigger"",
                                        ""alias"": ""httpTriggerTest"",
                                        ""endpoint"": {
                                            ""path"": ""addressMatch"",
                                            ""httpVerb"":  ""POST""
                                        },
                                        ""httpResponse"":{
                                            ""contentType"": ""text/json"",
                                            ""content"": ""end""
                                        }
                                    }
                                }
                            ],
                            ""actions"": [
                                {
                                    ""httpRequestAction"": {
                                        ""name"": ""action test"",
                                        ""alias"": ""actionTest"",
                                        ""beforeRunErrorConditions"": [
                                            {
                                            ""condition"": true,
                                                ""error"": {
                                                    ""code"": ""test.error.raised"",
                                                    ""title"": ""Test Error Raised"",
                                                    ""message"": ""This error was raised from an unhandled beforeRunErrorCondition"",
                                                    ""httpStatusCode"": 400
                                                }
                                            }
                                        ],
                                        ""onErrorActions"": [
                                            {
                                                ""raiseEventAction"": {
                                                    ""name"": ""Event Action Test3"",
                                                    ""alias"": ""eventActionTest3"",
                                                    ""customEventAlias"": ""MyTestEvent3"",
                                                    ""description"": ""A test of the event action"",
                                                    ""asynchronous"": false,
                                                    ""eventTags"": [""somethingqwe""]
                                                }
                                            }
                                        ],
                                        ""httpRequest"": {
                                            ""url"": ""www.test-uri.com""
                                        }
                                    }
                                }
                            ]
                        }
                    ]
                }";
            var automationsConfigModel = AutomationConfigurationParser.Parse(configString);
            var automations = automationsConfigModel.Build(this.dependencyProvider);

            // Act
            var (matchingAutomation, trigger) = await automations.GetClosestMatchingHttpTrigger(automationData);
            await matchingAutomation.Execute(automationData);

            // Assert
            var actionData = automationData.Actions.FirstOrDefault().Value;
            actionData.Finished.Should().BeTrue();
            actionData.Succeeded.Should().BeTrue();
            actionData.Error.Should().BeNull();
            actionData.OnErrorActions.Should().HaveCount(1);
            actionData.OnErrorActions.First().Value.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task AutomationTest_ShouldNotExecute_WhenRunConditionInAutomationIsPresentAndReturnsFalse()
        {
            var dataContext = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var configString = @"{
                    ""schemaVersion"": ""1.0.0"",
                    ""automations"": [
                        {
                            ""name"": ""Test"",
                            ""alias"": ""testString"",
                            ""description"": ""test config for automation matching"",
                            ""runCondition"": false,
                            ""triggers"": [
                                {
                                    ""httpTrigger"": {
                                        ""name"": ""Foo HTTP Trigger"",
                                        ""alias"": ""fooTrigger"",
                                        ""description"": ""automation should be triggered by request to foo endpoint"",
                                        ""endpoint"": {
                                            ""path"": ""addressMatch"",
                                            ""httpVerb"": ""POST""
                                        },
                                        ""httpResponse"": {
                                            ""httpStatusCode"" : 200,
                                            ""contentType"" : ""application/json"",
                                            ""content"" : {
                                                ""liquidText"": {
                                                    ""liquidTemplate"": ""Ash's pokemon is called {{name}}"",
                                                    ""dataObject"" : {
                                                        ""jsonObject"": {
                                                            ""objectPathLookupText"": {
                                                                ""path"": ""/actions/fooAction/httpResponse/content"",
                                                                ""valueIfNotFound"": ""{ 'name': 'charizard' }""
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            ],
                            ""actions"": [
                                {
                                    ""httpRequestAction"": {
                                        ""name"": ""Foo Http Action"",
                                        ""alias"": ""fooAction"",
                                        ""description"": ""running foo action"",
                                        ""asynchronous"": false,
                                        ""httpRequest"": {
                                            ""httpVerb"": ""GET"",
                                            ""url"": ""https://test.com/pikachu"",
                                            ""headers"": [
                                                {
                                                    ""something"": ""value""
                                                }
                                            ]
                                        }
                                    }
                                }
                            ]
                        }
                    ]
            }";

            var automationConfigurationModel = AutomationConfigurationParser.Parse(configString);
            var automationConfiguration = automationConfigurationModel.Build(this.dependencyProvider);
            var (matchingAutomation, trigger) = await automationConfiguration.GetClosestMatchingHttpTrigger(dataContext);
            var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { name = "pikachu" }, 200);

            // Act
            using (httpTest)
            {
                await matchingAutomation.Execute(dataContext);
            }

            object? automationAlias;
            var automationAliasSet = dataContext.Automation.TryGetValue("automation", out automationAlias);

            // Assert
            automationAliasSet.Should().BeTrue();
            dataContext.Trigger.Should().NotBeNull();
            automationAlias.Should().Be("testString");
            dataContext?.Trigger?.TriggerAlias.Should().Be("fooTrigger");
            dataContext?.Actions.Should().HaveCount(0);

            // trigger response should still be generated.
            var updateDataContextHttpTrigger = dataContext?.Trigger as HttpTriggerData;
            var triggerDataContextResponse = updateDataContextHttpTrigger?.HttpResponse;
            var triggerResponseCode = triggerDataContextResponse?.HttpStatusCode;
            var triggerResponseContentType = triggerDataContextResponse?.ContentType;
            var triggerDataContextResponseContent = triggerDataContextResponse?.Content as string ?? string.Empty;
            triggerDataContextResponseContent.Should().Be("Ash's pokemon is called charizard");
            triggerResponseCode.Should().Be(200);
            triggerResponseContentType.Should().Be("application/json");
        }

        [Fact]
        public async Task AutomationTest_ShouldExecute_WhenRunConditionInAutomationIsPresentAndReturnsTrue()
        {
            var dataContext = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var configString = @"{
                    ""schemaVersion"": ""1.0.0"",
                    ""automations"": [
                        {
                            ""name"": ""Test"",
                            ""alias"": ""testString"",
                            ""description"": ""test config for automation matching"",
                            ""runCondition"": true,
                            ""triggers"": [
                                {
                                    ""httpTrigger"": {
                                        ""name"": ""Foo HTTP Trigger"",
                                        ""alias"": ""fooTrigger"",
                                        ""description"": ""automation should be triggered by request to foo endpoint"",
                                        ""runCondition"": true,
                                        ""endpoint"": {
                                            ""path"": ""addressMatch"",
                                            ""httpVerb"": ""POST""
                                        },
                                        ""httpResponse"": {
                                            ""httpStatusCode"" : 200,
                                            ""contentType"" : ""application/json"",
                                            ""content"" : {
                                                ""liquidText"": {
                                                    ""liquidTemplate"": ""Ash's pokemon is called {{name}}"",
                                                    ""dataObject"" : {
                                                        ""jsonObject"": {
                                                            ""objectPathLookupText"": {
                                                                ""path"": ""/actions/fooAction/httpResponse/content"",
                                                                ""valueIfNotFound"": ""charizard""
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            ],
                            ""actions"": [
                                {
                                    ""httpRequestAction"": {
                                        ""name"": ""Foo Http Action"",
                                        ""alias"": ""fooAction"",
                                        ""description"": ""running foo action"",
                                        ""asynchronous"": false,
                                        ""httpRequest"": {
                                            ""httpVerb"": ""GET"",
                                            ""url"": ""https://test.com/pikachu"",
                                            ""headers"": [
                                                {
                                                    ""something"": ""value""
                                                }
                                            ]
                                        }
                                    }
                                }
                            ]
                        }
                    ]
            }";

            var automationConfigurationModel = AutomationConfigurationParser.Parse(configString);
            var automationConfiguration = automationConfigurationModel.Build(this.dependencyProvider);
            var (matchingAutomation, trigger) = await automationConfiguration.GetClosestMatchingHttpTrigger(dataContext);
            var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { name = "pikachu" }, 200);

            // Act
            using (httpTest)
            {
                await matchingAutomation.Execute(dataContext);
            }

            var actionData = dataContext.Actions.First().Value;
            var triggerData = dataContext.Trigger as HttpTriggerData;

            dataContext.Trigger.Should().NotBeNull();
            dataContext?.Trigger?.TriggerAlias.Should().Be("fooTrigger");
            dataContext?.Actions.Should().HaveCount(1);
            actionData.Ran.Should().BeTrue();
            triggerData?.HttpResponse.Should().NotBeNull();
        }

        [Fact]
        public async Task AutomationTest_ShouldNotExecute_WhenRunConditionInTriggerIsPresentAndReturnsFalse()
        {
            string eventJson = @"{
                ""type"": ""eventTrigger"",
                ""eventType"": ""custom"",
                ""customEventAlias"": ""MyTestEvent"",
                ""triggerAlias"": ""eventLeon"",
                ""eventData"": {
                    ""address"": ""Malolos City"",
                    ""name"":""leon""
                }
            }";
            MockAutomationData dataContext = MockAutomationData.CreateWithEventTrigger(json: eventJson);
            var configString = @"{
                ""schemaVersion"": ""1.0.0"",
                ""automations"": [
                    {
                        ""name"": ""Test"",
                        ""alias"": ""testString"",
                        ""description"": ""test config for automation matching"",
                        ""triggers"": [
                            {
                                ""eventTrigger"": {
                                    ""name"": ""Foo Event Trigger"",
                                    ""alias"": ""eventLeon"",
                                    ""eventType"": ""custom"",
                                    ""description"": ""A test of the event trigger"",
                                    ""customEventAlias"": ""MyTestEvent"",
                                    ""asynchronous"": false,
                                    ""runCondition"": false,
                                }
                            }
                        ],
                        ""actions"": [
                            {
                                ""raiseEventAction"": {
                                    ""name"": ""Event Action Test"",
                                    ""alias"": ""eventActionTest"",
                                    ""description"": ""A test of the event action"",
                                    ""asynchronous"": false,
                                    ""customEventAlias"": ""MyTestEvent"",
                                    ""eventData"": {
                                            ""objectPathLookupObject"": ""/trigger/eventData""
                                    },
                                    ""eventTags"": [""somethingqwe""],
                                    ""eventPersistanceDuration"": ""P1Y4M2DT10H31M3.452S""
                                }
                            },
                            {
                                ""raiseEventAction"": {
                                    ""name"": ""Event Action Test2"",
                                    ""alias"": ""eventActionTest2"",
                                    ""customEventAlias"": ""MyTestEvent2"",
                                    ""description"": ""A test of the event action"",
                                    ""asynchronous"": false,
                                    ""eventTags"": [""somethingqwe""],
                                    ""eventPersistanceDuration"": {  
                                                    ""periodTypeValueDuration"": {
                                                        ""value"": 1,
                                                        ""periodType"": ""month""
                                                    }
                                            },
                                }
                            },
                            {
                                ""raiseEventAction"": {
                                    ""name"": ""Event Action Test3"",
                                    ""alias"": ""eventActionTest3"",
                                    ""customEventAlias"": ""MyTestEvent3"",
                                    ""description"": ""A test of the event action"",
                                    ""asynchronous"": false,
                                    ""eventTags"": [""somethingqwe""]
                                }
                            }
                        ]
                    }
                ]
            }"
            ;

            var automationConfigurationModel = AutomationConfigurationParser.Parse(configString);
            var automationConfiguration = automationConfigurationModel.Build(this.dependencyProvider);
            var automation = automationConfiguration.Automations.First();

            // Act
            await automation.Execute(dataContext);

            // Assert
            object? automationAlias;
            var automationAliasSet = dataContext.Automation.TryGetValue("automation", out automationAlias);
            automationAliasSet.Should().BeTrue();
            dataContext.Trigger.Should().NotBeNull();
            automationAlias.Should().Be("testString");
            dataContext?.Trigger?.TriggerAlias.Should().Be("eventLeon");

            // Ensure that no actions are executed when run condition is false
            dataContext?.Actions.Should().HaveCount(0);
        }
    }
}
