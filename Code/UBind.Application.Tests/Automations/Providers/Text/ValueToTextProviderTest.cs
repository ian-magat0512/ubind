// <copyright file="ValueToTextProviderTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Providers.Text;

using Moq;
using Newtonsoft.Json;
using UBind.Application.Automation;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers.Text;
using UBind.Application.Automation.Providers;
using UBind.Application.Tests.Automations.Fakes;
using Xunit;
using FluentAssertions;
using UBind.Domain.Exceptions;

public class ValueToTextProviderTest
{
    private JsonSerializerSettings settings;
    private IServiceProvider dependencyProvider;

    public ValueToTextProviderTest()
    {
        this.dependencyProvider = new Mock<IServiceProvider>().Object;
        this.settings = AutomationDeserializationConfiguration.ModelSettings;
    }

    [Fact]
    public async Task ValueToText_ShouldParseTheBooleanValue_Succeed()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToText"": true
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToTextProvider)model!.Build(this.dependencyProvider);

        // Assert
        var result = await condition.Resolve(new ProviderContext(automationData));
        result.GetValueOrThrowIfFailed().DataValue.Should().Be("true");
    }

    [Fact]
    public async Task ValueToText_ShouldParseTheIntegerValue_Succeed()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToText"": 12312312
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToTextProvider)model!.Build(this.dependencyProvider);

        // Assert
        var result = await condition.Resolve(new ProviderContext(automationData));
        result.GetValueOrThrowIfFailed().DataValue.Should().Be("12312312");
    }

    [Fact]
    public async Task ValueToText_ShouldParseTheNumberValue_Succeed()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToText"": 12312312.12
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToTextProvider)model!.Build(this.dependencyProvider);

        // Assert
        var result = await condition.Resolve(new ProviderContext(automationData));
        result.GetValueOrThrowIfFailed().DataValue.Should().Be("12312312.12");
    }

    [Fact]
    public async Task ValueToText_ShouldParseTheTextValue_Succeed()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToText"": ""as is""
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToTextProvider)model!.Build(this.dependencyProvider);

        // Assert
        var result = await condition.Resolve(new ProviderContext(automationData));
        result.GetValueOrThrowIfFailed().DataValue.Should().Be("as is");
    }

    [Fact]
    public async Task ValueToText_ShouldParseTheNullValue_Succeed()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToText"": null
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToTextProvider)model!.Build(this.dependencyProvider);

        // Assert
        var result = await condition.Resolve(new ProviderContext(automationData));
        result.GetValueOrThrowIfFailed().DataValue.Should().Be("null");
    }

    [Fact]
    public async Task ValueToText_ShouldNotParseTheNotSupportValueList_ThrowException()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToText"":  [ ""some"", ""list"" ]
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToTextProvider)model!.Build(this.dependencyProvider);

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("provider.parameter.invalid");
    }
}
