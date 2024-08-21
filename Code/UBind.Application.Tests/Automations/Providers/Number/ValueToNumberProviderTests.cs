// <copyright file="ValueToNumberProviderTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Providers.Number;

using Moq;
using Newtonsoft.Json;
using System;
using UBind.Application.Automation;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers;
using UBind.Application.Tests.Automations.Fakes;
using Xunit;
using UBind.Application.Automation.Providers.Number;
using FluentAssertions;
using UBind.Domain.Exceptions;

public class ValueToNumberProviderTests
{
    private JsonSerializerSettings settings;
    private IServiceProvider dependencyProvider;

    public ValueToNumberProviderTests()
    {
        this.dependencyProvider = new Mock<IServiceProvider>().Object;
        this.settings = AutomationDeserializationConfiguration.ModelSettings;
    }

    [Theory]
    [InlineData("123123", 123123.0)]
    [InlineData("1231.23", 1231.23)]
    [InlineData("1,231", 1231.0)]
    [InlineData("1,231.23", 1231.23)]
    public async Task ValueToNumber_ShouldParseTheTextValue_Succeed(string value, decimal output)
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToNumber"": """ + value + @"""
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToNumberProvider)model!.Build(this.dependencyProvider);

        // Assert
        var result = await condition.Resolve(new ProviderContext(automationData));
        result.GetValueOrThrowIfFailed().DataValue.Should().Be(output);
    }

    [Fact]
    public async Task ValueToNumber_ShouldParseTheIntegerValue_Succeed()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToNumber"": 123123
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToNumberProvider)model!.Build(this.dependencyProvider);

        // Assert
        var result = await condition.Resolve(new ProviderContext(automationData));
        result.GetValueOrThrowIfFailed().DataValue.Should().Be(123123.0m);
    }

    [Fact]
    public async Task ValueToNumber_ShouldParseTheNumberValue_Succeed()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToNumber"": 123123.12
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToNumberProvider)model!.Build(this.dependencyProvider);

        // Assert
        var result = await condition.Resolve(new ProviderContext(automationData));
        result.GetValueOrThrowIfFailed().DataValue.Should().Be(123123.12m);
    }

    [Fact]
    public async Task ValueToNumber_ShouldParseTheInvalidValueList_ThrowException()
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToNumber"": [12312, 12312]
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToNumberProvider)model!.Build(this.dependencyProvider);

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("provider.parameter.invalid");
    }

    [Theory]
    [InlineData("12312321abc")]
    [InlineData("abcdefg")]
    [InlineData("12312.asdas")]
    [InlineData("123,123,123,asdasd")]
    public async Task ValueToNumber_ShouldParseTheInvalidTextValue_ThrowException(string value)
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToNumber"": """ + value + @"""
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToNumberProvider)model!.Build(this.dependencyProvider);

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("provider.parameter.invalid");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ValueToNumber_ShouldParseTheInvalidValueboolean_ThrowException(bool value)
    {
        // Arrange
        var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
        var json = @"{
                ""valueToNumber"":  " + value.ToString().ToLower() + @"
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
        var condition = (ValueToNumberProvider)model!.Build(this.dependencyProvider);

        // Assert
        Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
        (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("provider.parameter.invalid");
    }
}
