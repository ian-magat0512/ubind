// <copyright file="PathLookupValueTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.PathLookup;

using Moq;
using Newtonsoft.Json;
using UBind.Application.Automation.Providers;
using UBind.Application.Automation;
using UBind.Application.Tests.Automations.Fakes;
using Xunit;
using FluentAssertions;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers.Value;
using UBind.Application.Automation.Object;
using UBind.Application.Automation.PathLookup;
using UBind.Application.Automation.Providers.List;
using UBind.Application.Automation.Providers.Object;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;

public class PathLookupValueTests
{
    private JsonSerializerSettings settings;
    private IServiceProvider dependencyProvider;

    public PathLookupValueTests()
    {
        this.dependencyProvider = new Mock<IServiceProvider>().Object;
        this.settings = AutomationDeserializationConfiguration.ModelSettings;
    }

    [Fact]
    public async Task PathLookupValue_ShouldParseTheBooleanValue_WhenTheBooleanValueIsValid()
    {
        // Arrange
        var pathLookUp = @"{""condition"": false}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupValue"": {
                        ""path"": ""/condition"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupValueProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed() as Data<bool>;
        result.DataValue.Should().Be(false);
    }

    [Fact]
    public async Task PathLookupValue_ShouldParseTheStringValue_WhenTheStringValueIsValid()
    {
        // Arrange
        var pathLookUp = @"{""string"": ""mystring""}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupValue"": {
                        ""path"": ""/string"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupValueProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed() as Data<string>;
        result.DataValue.Should().Be("mystring");
    }

    [Fact]
    public async Task PathLookupValue_ShouldParseTheIntegerValue_WhenTheIntegerValueIsValid()
    {
        // Arrange
        var pathLookUp = @"{""integer"": 123}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupValue"": {
                        ""path"": ""/integer"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupValueProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed() as Data<long>;
        result.DataValue.Should().Be(123);
    }

    [Fact]
    public async Task PathLookupValue_ShouldParseTheNumberValue_WhenTheNumberValueIsValid()
    {
        // Arrange
        var pathLookUp = @"{""number"": 12.2}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupValue"": {
                        ""path"": ""/number"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupValueProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed() as Data<decimal>;
        result.DataValue.Should().Be(12.2m);
    }

    [Fact]
    public async Task PathLookupValue_ShouldParseTheListValue_WhenTheListValueIsValid()
    {
        // Arrange
        IServiceProvider serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
        List<object> listValues = new List<object> { "item1", "item2", "item3" };
        var automationData = await MockAutomationData.CreateWithHttpTrigger();
        var dataObjectDictionary = new List<ObjectPropertyConfigModel>()
            {
                new ObjectPropertyConfigModel
                {
                    PropertyName = new StaticBuilder<Data<string>>() { Value = "listValues" },
                    Value = (IBuilder<IProvider<IData>>)new StaticBuilder<Data<List<object>>>() { Value = listValues },
                },
            };
        var dataObjectProvider = new DynamicObjectProviderConfigModel() { Properties = dataObjectDictionary };
        var pathProvider = new StaticBuilder<Data<string>> { Value = "/listValues" };
        var objectPathLookupProvider = new ObjectPathLookupProviderConfigModel() { Path = pathProvider, DataObject = dataObjectProvider };
        var pathLookupBuilder = new PathLookupValueProviderConfigModel() { PathLookup = objectPathLookupProvider };
        var pathLookupProvider = pathLookupBuilder.Build(serviceProvider);

        // Act
        var result = (await pathLookupProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        var listResult = new GenericDataList<object>(result.GetValueFromGeneric() as IEnumerable<object>);
        var data = listResult.ToList();
        data.Should().NotBeNull();
        data.Count.Should().Be(listValues.Count);
        data[0].Should().Be(listValues[0]);
        data[1].Should().Be(listValues[1]);
        data[2].Should().Be(listValues[2]);
    }

    [Fact]
    public async Task PathLookupValue_ShouldThrowError_WhenPathValueNotFound()
    {
        // Arrange
        var pathLookUp = @"{""conditions"": false}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupValue"": {
                        ""path"": ""/condition"",
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupValueProvider;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueNotFound(condition.SchemaReferenceKey, null).Code);
    }

    [Fact]
    public async Task PathLookupValue_ShouldParseValueIfNotFound_WhenPathValueNotFound()
    {
        // Arrange
        var pathLookUp = @"{""conditions"": false}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupValue"": {
                        ""path"": ""/condition"",
                        ""raiseErrorIfNotFound"": false,
                        ""valueIfNotFound"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupValueProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed() as Data<bool>;
        result.DataValue.Should().Be(true);
    }

    [Fact]
    public async Task PathLookupValue_ShouldThrowError_WhenValueIfNull()
    {
        // Arrange
        var pathLookUp = @"{""value"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupValue"": {
                        ""path"": ""/value"",
                        ""raiseErrorIfNull"": true,
                        ""dataObject"": {
                                ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content""                                         
                            }
                        }
                    }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupValueProvider;

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which
            .Error.Code.Should().Be(Errors.Automation.PathQueryValueIfNull(condition.SchemaReferenceKey, null).Code);
    }

    [Fact]
    public async Task PathLookupValue_ShouldParseTheValueIfNull_WhenValueIfNull()
    {
        // Arrange
        var pathLookUp = @"{""value"": null}";
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent(contentList: pathLookUp);

        var json = @"{
                    ""objectPathLookupValue"": {
                        ""path"": ""/value"",
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
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = model.Build(this.dependencyProvider) as PathLookupValueProvider;

        // Assert
        var result = (await condition.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed() as Data<long>;
        result.DataValue.Should().Be(123);
    }
}
