// <copyright file="AutomationConfigurationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Conditions;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    /// <summary>
    /// Unit test for automations configuration model creation and instantiaton.
    /// </summary>
    public class AutomationConfigurationTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationConfigurationTests"/> class.
        /// </summary>
        public AutomationConfigurationTests()
        {
            this.dependencyProvider = MockAutomationData.GetDefaultServiceProvider();
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        /// <summary>
        /// Unit test for testing <see cref="PropertyDiscriminatorConverter{T}"/> class in automations configuration parsing.
        /// </summary>
        [Fact]
        public void JsonConverter_ShouldCreateEmailTrigger_FromValidJson()
        {
            // Arrange
            var json = @"{
                    ""emailTrigger"": {
                        ""name"": ""Remote Email Trigger Test"",
                        ""alias"": ""remoteEmailTriggerTest"",
                        ""description"": ""A test of the local email trigger"",

                        ""emailAccount"": {
                            ""protocol"": ""POP3"",
                            ""encryptionMethod"": ""SSL"",
                            ""hostname"": ""mail.abcinsurance.com.au"",
                            ""port"": 995,
                            ""username"": ""contact @abcinsurance.com.au"",
                            ""password"": ""9#J8&s@fDO"",
                            ""pollingIntervalSeconds"": 60
                        }
                    }
                }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<Trigger>>(
                json, this.settings);
            var trigger = model.Build(this.dependencyProvider) as EmailTrigger;

            // Assert
            model.Should().NotBeNull();
            trigger.Should().NotBeNull();
            trigger.Name.Should().Be("Remote Email Trigger Test");
            trigger.EmailAccount.Protocol.Should().Be("POP3");
        }

        [Fact]
        public async Task JsonConverterTest_ShouldCreateAndAutomationCondition_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                ""orCondition"": [
                    {
                        ""textStartsWithCondition"": {
                            ""text"":""baz"",
                            ""startsWith"": ""b""
                        }
                    },
                    {
                        ""textIsEqualToCondition"":{
                            ""text"": ""sample1"",
                            ""isEqualTo"": ""sample2""
                        }
                    }
             ]}";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as OrCondition;

            // Assert
            var textIsEqual = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            textIsEqual.DataValue.Should().BeTrue();
        }

        [Fact]
        public async Task JsonConverter_ShouldCreateHttpTrigger_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                            ""httpTrigger"": {
                                ""name"": ""HTTP Trigger Test"",
                                ""alias"": ""httpTriggerTest"",
                                ""description"": ""A test of the http trigger"",
                                ""endpoint"": {
                                    ""path"": ""searchAddress"",
                                    ""httpVerb"": ""POST"",
                                    ""requestValidationErrorConditions"": [
                                        {
                                            ""condition"": {
                                                ""textStartsWithCondition"": {
                                                    ""text"": {
                                                        ""objectPathLookupText"": {
                                                            ""path"": ""/key1"",
                                                            ""dataObject"": [
                                                                {
                                                                    ""propertyName"": ""key1"",
                                                                    ""value"": ""value1""
                                                                },
                                                                {
                                                                    ""propertyName"": ""key2"",
                                                                    ""value"": ""value2""
                                                                }
                                                            ]
                                                        }
                                                    },
                                                    ""startsWith"": ""val""
                                                }
                                            },
                                            ""error"": {
                                                ""code"": ""contentType.incorrect"",
                                                ""title"": ""Incorrect content-type used in request"",
                                                ""message"": ""The content-type of the request must be 'application/json'."",
                                                ""httpStatusCode"": 415
                                            }
                                        },
                                        {
                                            ""condition"": {
                                                ""textMatchesRegexPatternCondition"": {
                                                    ""text"": ""alpha"",
                                                    ""regexPattern"": ""/A-Za-z0-9]+/g""
                                                }
                                            },
                                            ""error"": {
                                                ""code"": ""referrer.not.permitted"",
                                                ""title"": ""Referrer not permitted"",
                                                ""message"": ""This API endpoint is only accessible when the referrer is a valid uBind domain."",
                                                ""httpStatusCode"": 403
                                            }
                                        }
                                    ]
                                },
                                ""httpResponse"": {
                                    ""httpStatusCode"": 200,
                                    ""headers"": [
                                        {
                                            ""name"": ""Authorization"",
                                            ""value"": {
                                                ""liquidText"": {
                                                    ""liquidTemplate"": ""access token {{key2}}"",
                                                    ""dataObject"": [
                                                        {
                                                            ""propertyName"": ""key1"",
                                                            ""value"": ""foo""
                                                        },
                                                        {
                                                            ""propertyName"": ""key2"",
                                                            ""value"": ""baz""
                                                        }
                                                    ]
                                                }
                                            }
                                        },
                                        {
                                            ""sample1"": ""sample2""
                                        }
                                    ],
                                    ""contentType"": ""text/json"",
                                    ""content"": ""test this""
                                }
                            }
                        }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<Trigger>>(json, this.settings);
            var trigger = model.Build(this.dependencyProvider) as HttpTrigger;
            var endpoint = trigger.Endpoint.Path;
            var conditionProvider = trigger.Endpoint.RequestValidationErrorConditions.FirstOrDefault();
            var condition = (await conditionProvider.Condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var response = trigger.HttpResponse;
            var responseHeader1 = (await response.Headers.FirstOrDefault()?.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var responseHeader2 = (await response.Headers.LastOrDefault()?.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            model.Should().NotBeNull();
            trigger.Name.Should().Be("HTTP Trigger Test");
            endpoint.Should().Be("searchAddress");
            trigger.Endpoint.HttpVerb.Should().Be("POST");
            condition.DataValue.Should().BeTrue();
            responseHeader1.Value.First().ToString().Should().Be("access token baz");
            responseHeader2.Value.First().ToString().Should().Be("sample2");
        }

        [Fact]
        public async Task JsonConverter_ShouldInstantiateAValidTextIsEqualToWithCondition_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                ""textIsEqualToCondition"": {
                        ""text"": {
                            ""objectPathLookupText"": {
                                  ""path"": ""/key4"",
                                  ""dataObject"": [
                                        {
                                            ""propertyName"": ""key1"",
                                            ""value"": ""value1""
                                        },
                                        {
                                            ""propertyName"": ""key2"",
                                            ""value"": ""value2""
                                        },
                                        {   ""propertyName"": ""key4"",
                                            ""value"": {
                                                ""liquidText"": {
                                                    ""liquidTemplate"": ""Did i get {{actionPath}}?"",
                                                    ""dataObject"": {
                                                        ""objectPathLookupObject"": ""/trigger/httpRequest""
                                                    }
                                                }
                                            }
                                        }
        
                                    ]
                                }
                          },
                          ""isEqualTo"": ""Did i get addressMatch?""
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings)
                as TextIsEqualToConditionConfigModel;
            var condition = model.Build(this.dependencyProvider);
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.DataValue.Should().BeTrue();
            condition.Should().NotBeNull();
            model.Text.Should().NotBeNull();
        }

        [Fact]
        public async Task JsonConverterTest_ShouldCreateHttpAction_FromValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                ""httpRequestAction"": {
                    ""name"": ""HTTP Request Action Test"",
                    ""alias"": ""httpRequestActionTest"",
                    ""description"": ""A test of the HTTP request action"",
                    ""asynchronous"": true,
                    ""httpRequest"": {
                        ""httpVerb"": ""POST"",
                        ""url"": ""https:/www.something.com/path/endpoint"",
                        ""headers"": [
                            {
                                ""username"": ""jonathan""
                            },
                            {
                                ""password"": [""test"", ""test1""]
                            },
                            {
                                ""name"": ""Content-Type"",
                                ""value"": ""test""
                            },
                            {
                                ""name"": ""X-Forwarded-For"",
                                ""value"": [""1.1.1.1"", ""2.2.2.2""]
                            }
                        ],
                        ""contentType"": ""multipart/mixed"",
                        ""content"": [
                            {
                                ""contentType"": ""text/plain"",
                                ""content"": ""first content""
                            },
                            {
                                ""contentType"": ""text/plain"",
                                ""content"": {
                                    ""liquidText"": {
                                        ""liquidTemplate"": ""second content in liquid provider {{key3}}"",
                                        ""dataObject"": [
                                            {
                                                ""propertyName"": ""key1"",
                                                ""value"": ""value1""
                                            },
                                            {
                                                ""propertyName"": ""key2"",
                                                ""value"": ""value2""
                                            },
                                            {   ""propertyName"": ""key3"",
                                                ""value"": {
                                                    ""liquidText"": {
                                                        ""liquidTemplate"": ""`Did i get {{actionPath}}?`"",
                                                        ""dataObject"": {
                                                            ""objectPathLookupObject"": ""/trigger/httpRequest""
                                                        }
                                                    }
                                                }
                                            },
                                            {
                                                ""propertyName"": ""key4"",
                                                ""value"": 400
                                            }
                                        ]
                                    }
                                }
                            }
                        ]
                    }
                }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<Application.Automation.Actions.Action>>(json, this.settings);
            var action = model.Build(this.dependencyProvider) as HttpRequestAction;
            var httpRequestUrl = (await action.HttpRequest.Url.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var httpVerb = (await action.HttpRequest.HttpVerb.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var firstHeader = (await action.HttpRequest.Headers.FirstOrDefault()?.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var secondHeader = (await action.HttpRequest.Headers.Skip(1).FirstOrDefault()?.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var thirdHeader = (await action.HttpRequest.Headers.Skip(2).FirstOrDefault()?.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var lastHeader = (await action.HttpRequest.Headers.LastOrDefault()?.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var resolveHttpContent = (await action.HttpRequest.Content.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            var httpContent = resolveHttpContent.GetValueFromGeneric() as IEnumerable<ContentPart>;
            var initialContent = httpContent.FirstOrDefault().Content.ToString();
            var secondContent = httpContent.LastOrDefault().Content.ToString();

            // Assert
            model.Should().NotBeNull();
            action.Name.Should().Be("HTTP Request Action Test");
            httpRequestUrl.DataValue.Should().Be("https:/www.something.com/path/endpoint");
            httpVerb.DataValue.Should().Be("POST");
            firstHeader.Value.First().Should().Be("jonathan");
            secondHeader.Value.LastOrDefault().Should().Be("test1");
            thirdHeader.Value.First().Should().Be("test");
            thirdHeader.Key.Should().Be("Content-Type");
            lastHeader.Value.Last().Should().Be("2.2.2.2");
            initialContent.Should().Be("first content");
            secondContent.Should().Be("second content in liquid provider `Did i get addressMatch?`");
        }

        [Fact]
        public async Task JsonConverterTest_ShouldCreateDynamicObjectProvider_WithDifferentTypeValuesForEachProperty()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var objectPathLookupJson = @"{
                ""objectPathLookupText"": {
                                  ""path"": ""/key4"",
                                  ""dataObject"": [
                                        {
                                            ""propertyName"": ""key1"",
                                            ""value"": ""value1""
                                        },
                                        {
                                            ""propertyName"": ""key2"",
                                            ""value"": ""value2""
                                        },
                                        {
                                            ""propertyName"": ""key3"",
                                            ""value"": 200
                                        },
                                        {   ""propertyName"": ""key4"",
                                            ""value"": {
                                                ""objectPathLookupText"": ""/trigger/httpRequest/actionPath""
                                            }
                                        }
        
                                    ]
                                }
            }";

            // Act
            var objectPathLookupModel = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(objectPathLookupJson, this.settings);
            var objectPathLookupProvider = objectPathLookupModel.Build(this.dependencyProvider);
            var resolvedObject = (await objectPathLookupProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            resolvedObject.DataValue.Should().BeEquivalentTo("addressMatch");
        }

        [Fact]
        public async Task JsonConverterTest_ShouldCreateJsonTextProvider_WithValidJson()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var objectPathLookupJson = @"{
                ""objectToJsonText"": {
                    ""jsonObject"": {
                        ""objectPathLookupText"": ""/trigger/httpRequest/content""
                    }
                }
            }";

            // Act
            var jsonTextModel = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(objectPathLookupJson, this.settings);
            var jsonTextProvider = jsonTextModel.Build(this.dependencyProvider);
            var resolvedText = (await jsonTextProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            resolvedText.DataValue.GetType().Should().Be(typeof(string));
        }

        [Fact]
        public async Task JsonConverterTest_ShouldCreateDynamicObjectProvider_WithDifferentTypesOfIDataValues()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var objectPathLookupJson = @"{
                ""objectPathLookupText"": {
                                  ""path"": ""/key4"",
                                  ""dataObject"": [
                                        {
                                            ""propertyName"": ""key1"",
                                            ""value"": ""value1""
                                        },
                                        {
                                            ""propertyName"": ""key2"",
                                            ""value"": 1000
                                        },
                                        {
                                            ""propertyName"": ""key3"",
                                            ""value"": true
                                        },
                                        {   ""propertyName"": ""key4"",
                                            ""value"": {
                                                ""objectPathLookupText"": ""/trigger/httpRequest/actionPath""
                                            }
                                        },
                                        {
                                            ""propertyName"": ""key5"",
                                            ""value"": [2,3,4,5,""hi"", [{ ""propertyName"": ""next"", ""value"": 6 }]]
                                        }
        
                                    ]
                                }
            }";

            var objectPathLookupConditionJson = @"{
                ""objectPathLookupCondition"": {
                                  ""path"": ""/key3"",
                                  ""dataObject"": [
                                        {
                                            ""propertyName"": ""key1"",
                                            ""value"": ""value1""
                                        },
                                        {
                                            ""propertyName"": ""key2"",
                                            ""value"": 1000
                                        },
                                        {
                                            ""propertyName"": ""key3"",
                                            ""value"": true
                                        },
                                        {   ""propertyName"": ""key4"",
                                            ""value"": {
                                                ""objectPathLookupText"": ""/trigger/httpRequest/actionPath""
                                            }
                                        }
        
                                    ]
                                }
            }";

            // Act
            var objectPathLookupTextModel = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(objectPathLookupJson, this.settings);
            var objectPathLookupConditionModel = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(objectPathLookupConditionJson, this.settings);

            // Assert
            var objectPathLookupTextProvider = objectPathLookupTextModel.Build(this.dependencyProvider);
            var objectPathLookupCondition = objectPathLookupConditionModel.Build(this.dependencyProvider);
            var resolvedText = (await objectPathLookupTextProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            resolvedText.DataValue.GetType().Should().Be(typeof(string));
            resolvedText.DataValue.Should().Be("addressMatch");

            var isPermitted = (await objectPathLookupCondition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            isPermitted.DataValue.GetType().Should().Be(typeof(bool));
            isPermitted.DataValue.Should().BeTrue();
        }

        [InlineData(@"{""objectPathLookupCondition"": ""/trigger/httpRequest/content/isTrue"" }", false)]
        [InlineData(@"{""objectPathLookupCondition"": { ""path"": ""/trigger/httpRequest/content/test"",""valueIfNotFound"": {""textIsEqualToCondition"": {""text"": ""fo"", ""isEqualTo"": ""fo""}}}}", true)]
        [InlineData(@"{""objectPathLookupCondition"": {""path"": ""/failsafe"",""dataObject"": [{""propertyName"": ""test"",""value"": false},{""propertyName"": ""failsafe"",""value"":true}],""valueIfNotFound"": true}}", true)]
        [Theory]
        public async Task JsonConverterTest_ShouldCreateObjectPathLookupCondition_WithValidJson(string configJson, bool shouldResolve)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger(withTriggerContent: true);

            // Act
            var conditionModel = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(configJson, this.settings);

            // Assert
            var conditionProvider = conditionModel.Build(this.dependencyProvider);
            conditionProvider.Should().NotBeNull();
            if (shouldResolve)
            {
                var value = (await conditionProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

                value.DataValue.Should().BeTrue();
            }
        }

        [Fact]
        public void JsonConverterTest_ShouldCreateRazorTextProvider_WithValidJson()
        {
            // Arrange
            var configJson = @"{
                ""razorText"": {
                    ""razorTemplate"": ""test"",
                    ""dataObject"": {
                        ""jsonTextToObject"": ""{ 'dont': 'remove' }""
                    }
                }
}";

            // Act
            var razorTextModel = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(configJson, this.settings);
            var razorTextProvider = razorTextModel.Build(this.dependencyProvider);

            // Assert
            razorTextProvider.Should().NotBeNull();
        }
    }
}
