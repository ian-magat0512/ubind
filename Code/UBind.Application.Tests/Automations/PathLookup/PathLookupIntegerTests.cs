// <copyright file="PathLookupIntegerTests.cs" company="uBind">
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
using UBind.Application.Automation.Providers.Integer;

public class PathLookupIntegerTests
{
    private JsonSerializerSettings settings;
    private IServiceProvider dependencyProvider;

    public PathLookupIntegerTests()
    {
        this.dependencyProvider = new Mock<IServiceProvider>().Object;
        this.settings = AutomationDeserializationConfiguration.ModelSettings;
    }

    [Fact]
    public async Task PathLookupInteger_ShouldParseTheIntegerValueToString_WhenTheIntegerValueIsValid()
    {
        // Arrange
        var pathLookUp = @"{""integer"": 123}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupInteger"": {
                        ""path"": ""/integer"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupIntegerProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(123);
    }

    [Fact]
    public async Task PathLookupInteger_ShouldParseTheIntegerValue_WhenTheIntegerValueIsInValid()
    {
        // Arrange
        var pathLookUp = @"{""integer"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupInteger"": {
                        ""path"": ""/integer"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupIntegerProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.Should().Be(null);
    }

    [Fact]
    public async Task PathLookupInteger_ShoulThrowError_WhenPathValueNotFound()
    {
        // Arrange
        var pathLookUp = @"{""integers"": 123}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupInteger"": {
                        ""path"": ""/integer"",
                        ""raiseErrorIfNotFound"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupIntegerProvider;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));

        // Assert
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueNotFound(condition.SchemaReferenceKey, null).Code);
    }

    [Fact]
    public async Task PathLookupInteger_ShoulReturnTheValueIfNotFound_WhenPathValueNotFound()
    {
        // Arrange
        var pathLookUp = @"{""integers"": 123}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupInteger"": {
                        ""path"": ""/integer"",
                        ""raiseErrorIfNotFound"": false,
                        ""valueIfNotFound"": 123,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupIntegerProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(123);
    }

    [Fact]
    public async Task PathLookupInteger_ShoulThrowError_WhenPathValueIfNull()
    {
        // Arrange
        var pathLookUp = @"{""integer"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupInteger"": {
                        ""path"": ""/integer"",
                        ""raiseErrorIfNull"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupIntegerProvider;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));

        // Assert
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueIfNull(condition.SchemaReferenceKey, null).Code);
    }

    [Fact]
    public async Task PathLookupInteger_ShoulReturnTheValueIfNull_WhenPathValueIfNull()
    {
        // Arrange
        var pathLookUp = @"{""integer"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupInteger"": {
                        ""path"": ""/integer"",
                        ""raiseErrorIfNull"": false,
                        ""valueIfNull"": 123,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupIntegerProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(123);
    }

    [Fact]
    public async Task PathLookupInteger_ShouldThrowError_WhenValueTypeIsMismatch()
    {
        // Arrange
        var pathLookUp = @"{""integer"": ""abc""}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupInteger"": {
                        ""path"": ""/integer"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupIntegerProvider;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueInvalidType(condition.SchemaReferenceKey, "string", "integer", null).Code);
    }

    [Fact]
    public async Task PathLookupInteger_ShoulReturnTheValueIfTypeMismatch_WhenPathValueTypeMismatch()
    {
        // Arrange
        var pathLookUp = @"{""integer"": ""abc""}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupInteger"": {
                        ""path"": ""/integer"",
                        ""raiseErrorIfTypeMismatch"": false,
                        ""valueIfTypeMismatch"": 123,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<long>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupIntegerProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(123);
    }
}
