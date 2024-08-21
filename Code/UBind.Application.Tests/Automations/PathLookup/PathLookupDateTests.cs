// <copyright file="PathLookupDateTests.cs" company="uBind">
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
using UBind.Application.Automation.Providers.Date;
using NodaTime;
using UBind.Domain.Extensions;

public class PathLookupDateTests
{
    private JsonSerializerSettings settings;
    private IServiceProvider dependencyProvider;

    public PathLookupDateTests()
    {
        this.dependencyProvider = new Mock<IServiceProvider>().Object;
        this.settings = AutomationDeserializationConfiguration.ModelSettings;
    }

    [Fact]
    public async Task PathLookupDate_ShouldParseTheDateValueToString_WhenTheDateValueIsValid()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateWithHttpTrigger();

        var json = @"{
                    ""objectPathLookupDate"": {
                        ""path"": ""/createdDateTime"",
                        ""dataObject"": {
                                ""entityObject"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                         },
                                        ""includeOptionalProperties"": []
                                 }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupDateProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        var date = automationData.ContextManager.Tenant.CreatedDateTime.ToLocalDateFromIso8601OrDateTimeIso8601();
        result.DataValue.Should().Be(date);
    }

    [Fact]
    public async Task PathLookupDate_ShouldParseTheDateValue_WhenTheDateValueIsInValid()
    {
        // Arrange
        var pathLookUp = @"{""date"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupDate"": {
                        ""path"": ""/date"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupDateProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.Should().Be(null);
    }

    [Fact]
    public async Task PathLookupDate_ShoulThrowError_WhenPathValueNotFound()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateWithHttpTrigger();

        var json = @"{
                    ""objectPathLookupDate"": {
                        ""path"": ""/createdDateTimes"",
                        ""dataObject"": {
                                ""entityObject"": {
                                    ""entity"": {
                                        ""contextEntity"": ""/tenant""
                                         },
                                        ""includeOptionalProperties"": []
                                 }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupDateProvider;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));

        // Assert
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueNotFound(condition.SchemaReferenceKey, null).Code);
    }

    [Fact]
    public async Task PathLookupDate_ShoulReturnTheValueIfNotFound_WhenPathValueNotFound()
    {
        // Arrange
        var pathLookUp = @"{""dates"": ""2022-01-02""}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupDate"": {
                        ""path"": ""/date"",
                        ""raiseErrorIfNotFound"": false,    
                        ""valueIfNotFound"": ""2022-01-02"",    
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupDateProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(new LocalDate(2022, 01, 02));
    }

    [Fact]
    public async Task PathLookupDate_ShoulThrowError_WhenPathValueIfNull()
    {
        // Arrange
        var pathLookUp = @"{""date"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupDate"": {
                        ""path"": ""/date"",
                        ""raiseErrorIfNull"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupDateProvider;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));

        // Assert
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueIfNull(condition.SchemaReferenceKey, null).Code);
    }

    [Fact]
    public async Task PathLookupDate_ShoulReturnTheValueIfNull_WhenPathValueIfNull()
    {
        // Arrange
        var pathLookUp = @"{""date"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupDate"": {
                        ""path"": ""/date"",
                        ""raiseErrorIfNull"": false,
                        ""valueIfNull"": ""2022-01-02"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupDateProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(new LocalDate(2022, 01, 02));
    }

    [Fact]
    public async Task PathLookupDate_ShouldThrowError_WhenValueTypeIsMismatch()
    {
        // Arrange
        var pathLookUp = @"{""date"": ""abc""}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupDate"": {
                        ""path"": ""/date"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupDateProvider;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueInvalidType(condition.SchemaReferenceKey, "string", "date", null).Code);
    }

    [Fact]
    public async Task PathLookupDate_ShoulReturnTheValueIfTypeMismatch_WhenPathValueTypeMismatch()
    {
        // Arrange
        var pathLookUp = @"{""date"": ""abc""}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupDate"": {
                        ""path"": ""/date"",
                        ""raiseErrorIfTypeMismatch"": false,
                        ""valueIfTypeMismatch"": ""2022-01-02"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<LocalDate>>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupDateProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(new LocalDate(2022, 01, 02));
    }
}
