// <copyright file="PathLookupConditionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.PathLookup;

using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers;
using UBind.Application.Automation;
using UBind.Application.Tests.Automations.Fakes;
using UBind.Domain;
using UBind.Domain.Exceptions;
using Xunit;
using UBind.Application.Automation.Providers.Conditions;

public class PathLookupConditionTests
{
    private JsonSerializerSettings settings;
    private IServiceProvider dependencyProvider;

    public PathLookupConditionTests()
    {
        this.dependencyProvider = new Mock<IServiceProvider>().Object;
        this.settings = AutomationDeserializationConfiguration.ModelSettings;
    }

    [Fact]
    public async Task PathLookupCondition_ShouldParseTheConditionValueToString_WhenTheConditionValueIsValid()
    {
        // Arrange
        var pathLookUp = @"{""condition"": true}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupCondition"": {
                        ""path"": ""/condition"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupCondition;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(true);
    }

    [Fact]
    public async Task PathLookupCondition_ShouldParseTheConditionValue_WhenTheConditionValueIsInValid()
    {
        // Arrange
        var pathLookUp = @"{""condition"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupCondition"": {
                        ""path"": ""/condition"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupCondition;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.Should().Be(null);
    }

    [Fact]
    public async Task PathLookupCondition_ShoulThrowError_WhenPathValueNotFound()
    {
        // Arrange
        var pathLookUp = @"{""conditions"": false}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupCondition"": {
                        ""path"": ""/condition"",
                        ""raiseErrorIfNotFound"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupCondition;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));

        // Assert
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueNotFound(condition.SchemaReferenceKey, null).Code);
    }

    [Fact]
    public async Task PathLookupCondition_ShoulReturnTheValueIfNotFound_WhenPathValueNotFound()
    {
        // Arrange
        var pathLookUp = @"{""conditions"": true}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupCondition"": {
                        ""path"": ""/condition"",
                        ""raiseErrorIfNotFound"": false,
                        ""valueIfNotFound"": false,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupCondition;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(false);
    }

    [Fact]
    public async Task PathLookupCondition_ShoulThrowError_WhenPathValueIfNull()
    {
        // Arrange
        var pathLookUp = @"{""condition"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupCondition"": {
                        ""path"": ""/condition"",
                        ""raiseErrorIfNull"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupCondition;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));

        // Assert
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueIfNull(condition.SchemaReferenceKey, null).Code);
    }

    [Fact]
    public async Task PathLookupCondition_ShoulReturnTheValueIfNull_WhenPathValueIfNull()
    {
        // Arrange
        var pathLookUp = @"{""condition"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupCondition"": {
                        ""path"": ""/condition"",
                        ""raiseErrorIfNull"": false,
                        ""valueIfNull"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupCondition;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(true);
    }

    [Fact]
    public async Task PathLookupCondition_ShouldThrowError_WhenValueTypeIsMismatch()
    {
        // Arrange
        var pathLookUp = @"{""condition"": ""abc""}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupCondition"": {
                        ""path"": ""/condition"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupCondition;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueInvalidType(condition.SchemaReferenceKey, "string", "bool", null).Code);
    }

    [Fact]
    public async Task PathLookupCondition_ShoulReturnTheValueIfTypeMismatch_WhenPathValueTypeMismatch()
    {
        // Arrange
        var pathLookUp = @"{""condition"": ""abc""}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupCondition"": {
                        ""path"": ""/condition"",
                        ""raiseErrorIfTypeMismatch"": false,
                        ""valueIfTypeMismatch"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<bool>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupCondition;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(true);
    }
}
