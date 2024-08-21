// <copyright file="AutomationConfigurationValidatorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class AutomationConfigurationValidatorTests
    {
        private readonly IServiceProvider mockDependencyProvider;

        public AutomationConfigurationValidatorTests()
        {
            this.mockDependencyProvider = MockAutomationData.GetDefaultServiceProvider();
        }

        [Fact]
        public void Validator_ShouldThrowError_WhenAutomationHasNonDistinctAlias()
        {
            // Arrange
            var configString = @"{
                    ""schemaVersion"": ""1.0.0"",
                    ""automations"": [
                        {
                            ""name"": ""FirstAutomation"",
                            ""alias"": ""firstAutomationAlias"",
                            ""description"": ""test config for unit test"",
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
                                            ""content"" : ""This is a test""
                                        }
                                    }
                                }
                            ],
                        },
                        {
                            ""name"": ""SecondAutomation"",
                            ""alias"": ""firstAutomationAlias"",
                            ""description"": ""test config for unit test"",
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
                                            ""content"" : ""This is a test""
                                        }
                                    }
                                }
                            ],
                        }
                    ]
            }";
            var configModel = AutomationConfigurationParser.Parse(configString);
            var validator = new AutomationConfigurationValidator(this.mockDependencyProvider);

            // Act
            Action act = () => validator.Validate(configModel);

            // Assert
            act.Should().Throw<ErrorException>()
                .And.Error.Code.Should().Be("automation.configuration.should.have.distinct.automation.alias");
        }

        [Fact]
        public void Validator_ShouldThrowError_WhenAutomationHasNonDistinctTriggerAlias()
        {
            // Arrange
            var configString = @"{
                    ""schemaVersion"": ""1.0.0"",
                    ""automations"": [
                        {
                            ""name"": ""FirstAutomation"",
                            ""alias"": ""firstAutomationAlias"",
                            ""description"": ""test config for unit test"",
                            ""triggers"": [
                                {
                                    ""httpTrigger"": {
                                        ""name"": ""First HTTP Trigger"",
                                        ""alias"": ""fooTrigger"",
                                        ""description"": ""automation should be triggered by request to foo endpoint"",
                                        ""endpoint"": {
                                            ""path"": ""first"",
                                            ""httpVerb"": ""POST""
                                        },
                                        ""httpResponse"": {
                                            ""httpStatusCode"" : 200,
                                            ""contentType"" : ""application/json"",
                                            ""content"" : ""This is a test""
                                        }
                                    }
                                },
                                {
                                    ""httpTrigger"": {
                                        ""name"": ""Second HTTP Trigger"",
                                        ""alias"": ""fooTrigger"",
                                        ""description"": ""automation should be triggered by request to foo endpoint"",
                                        ""endpoint"": {
                                            ""path"": ""second"",
                                            ""httpVerb"": ""POST""
                                        },
                                        ""httpResponse"": {
                                            ""httpStatusCode"" : 200,
                                            ""contentType"" : ""application/json"",
                                            ""content"" : ""This is a test""
                                        }
                                    }
                                }
                            ]
                        }
                    ]
            }";
            var configModel = AutomationConfigurationParser.Parse(configString);
            var validator = new AutomationConfigurationValidator(this.mockDependencyProvider);

            // Act
            Action act = () => validator.Validate(configModel);

            // Assert
            act.Should().Throw<ErrorException>()
                .And.Error.Code.Should().Be("automation.configuration.should.have.distinct.automation.trigger.alias");
        }

        [Fact]
        public void Validator_ShouldThrowError_WhenAutomationHasNonDistinctActions()
        {
            // Arrange
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
                                                        ""objectPathLookupObject"" : ""actions.fooAction.httpResponse.content""
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            ],
                            ""actions"": [
                                {
                                    ""groupAction"": {
                                        ""name"": ""Foo Group Action"",
                                        ""alias"": ""foogroup"",
                                        ""description"": ""running foo group action"",
                                        ""asynchronous"": false,
                                        ""actions"": [
                                            {
                                                ""httpRequestAction"": {
                                                    ""name"": ""Foo Http Action"",
                                                    ""alias"": ""fooAction"",
                                                    ""description"": ""running foo action"",
                                                    ""asynchronous"": false,
                                                    ""httpRequest"": {
                                                        ""httpVerb"": ""GET"",
                                                        ""url"": ""https://foo.com""
                                                    }
                                                }
                                            },
                                            {
                                                ""httpRequestAction"": {
                                                    ""name"": ""Foo Http Action"",
                                                    ""alias"": ""fooAction"",
                                                    ""description"": ""running foo action"",
                                                    ""asynchronous"": false,
                                                    ""httpRequest"": {
                                                        ""httpVerb"": ""GET"",
                                                        ""url"": ""https://foo.com""
                                                    }
                                                }
                                            }
                                        ]
                                    }
                                }                                
                            ]
                        },
                    ]
                }";
            var configModel = AutomationConfigurationParser.Parse(configString);
            var validator = new AutomationConfigurationValidator(this.mockDependencyProvider);

            // Act
            Action act = () => validator.Validate(configModel);

            // Assert
            act.Should().Throw<ErrorException>()
                .And.Error.Code.Should().Be("automation.configuration.should.have.distinct.automation.action.alias");
        }
    }
}
