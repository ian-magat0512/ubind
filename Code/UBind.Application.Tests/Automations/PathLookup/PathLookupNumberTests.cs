// <copyright file="PathLookupNumberTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.PathLookup
{
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Number;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class PathLookupNumberTests
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public PathLookupNumberTests()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Fact]
        public async Task PathLookupNumber_ShouldParseTheNumberValueToString_WhenTheNumberValueIsValid()
        {
            // Arrange
            var pathLookUp = @"{""number"": 12.2}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                    ""objectPathLookupNumber"": {
                        ""path"": ""/number"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as PathLookupNumberProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(12.2m);
        }

        [Fact]
        public async Task PathLookupNumber_ShouldParseTheNumberValue_WhenTheNumberValueIsInValid()
        {
            // Arrange
            var pathLookUp = @"{""number"": null}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                    ""objectPathLookupNumber"": {
                        ""path"": ""/number"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as PathLookupNumberProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.Should().Be(null);
        }

        [Fact]
        public async Task PathLookupNumber_ShoulThrowError_WhenPathValueNotFound()
        {
            // Arrange
            var pathLookUp = @"{""numbers"": 12.12}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                    ""objectPathLookupNumber"": {
                        ""path"": ""/number"",
                        ""raiseErrorIfNotFound"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as PathLookupNumberProvider;

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
            (await func.Should().ThrowAsync<ErrorException>()).Which
                .Error.Code.Should().Be(Errors.Automation.PathQueryValueNotFound(condition.SchemaReferenceKey, null).Code);
        }

        [Fact]
        public async Task PathLookupNumber_ShoulReturnTheValueIfNotFound_WhenPathValueNotFound()
        {
            // Arrange
            var pathLookUp = @"{""numbers"": 123.12}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                    ""objectPathLookupNumber"": {
                        ""path"": ""/number"",
                        ""raiseErrorIfNotFound"": false,
                        ""valueIfNotFound"": 12.12,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as PathLookupNumberProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(12.12m);
        }

        [Fact]
        public async Task PathLookupNumber_ShoulThrowError_WhenPathValueIfNull()
        {
            // Arrange
            var pathLookUp = @"{""number"": null}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                    ""objectPathLookupNumber"": {
                        ""path"": ""/number"",
                        ""raiseErrorIfNull"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as PathLookupNumberProvider;

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which
                .Error.Code.Should().Be(Errors.Automation.PathQueryValueIfNull(condition.SchemaReferenceKey, null).Code);
        }

        [Fact]
        public async Task PathLookupNumber_ShoulReturnTheValueIfNull_WhenPathValueIfNull()
        {
            // Arrange
            var pathLookUp = @"{""number"": null}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                    ""objectPathLookupNumber"": {
                        ""path"": ""/number"",
                        ""raiseErrorIfNull"": false,
                        ""valueIfNull"": 12.12,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as PathLookupNumberProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(12.12m);
        }

        [Fact]
        public async Task PathLookupNumber_ShouldThrowError_WhenValueTypeIsMismatch()
        {
            // Arrange
            var pathLookUp = @"{""number"": ""abc""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                    ""objectPathLookupNumber"": {
                        ""path"": ""/number"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as PathLookupNumberProvider;

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
            (await func.Should().ThrowAsync<ErrorException>()).Which
                .Error.Code.Should().Be(Errors.Automation.PathQueryValueInvalidType(condition.SchemaReferenceKey, "string", "decimal", null).Code);
        }

        [Fact]
        public async Task PathLookupNumber_ShoulReturnTheValueIfTypeMismatch_WhenPathValueTypeMismatch()
        {
            // Arrange
            var pathLookUp = @"{""number"": ""abc""}";
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

            var json = @"{
                    ""objectPathLookupNumber"": {
                        ""path"": ""/number"",
                        ""raiseErrorIfTypeMismatch"": false,
                        ""valueIfTypeMismatch"": 12.12,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<decimal>>>>(json, this.settings);
            var condition = model.Build(this.dependencyProvider) as PathLookupNumberProvider;

            // Assert
            var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
            result.DataValue.Should().Be(12.12m);
        }
    }
}
