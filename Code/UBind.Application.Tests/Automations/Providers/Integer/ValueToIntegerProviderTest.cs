// <copyright file="ValueToIntegerProviderTest.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Providers.Integer
{
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Integer;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class ValueToIntegerProviderTest
    {
        private JsonSerializerSettings settings;
        private IServiceProvider dependencyProvider;

        public ValueToIntegerProviderTest()
        {
            this.dependencyProvider = new Mock<IServiceProvider>().Object;
            this.settings = AutomationDeserializationConfiguration.ModelSettings;
        }

        [Theory]
        [InlineData("12312321", 12312321)]
        [InlineData("678,678,678", 678678678)]
        [InlineData("123123.0000", 123123)]
        [InlineData("456.0", 456)]
        [InlineData("678,678,678.0", 678678678)]
        public async Task ValueToInteger_ShouldParseTheTextValue_Succeed(string value, long output)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                ""valueToInteger"":  """ + value + @"""
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
            var condition = (ValueToIntegerProvider)model!.Build(this.dependencyProvider);

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(output);
        }

        [Fact]
        public async Task ValueToInteger_ShouldParseTheNumberValue_Succeed()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                ""valueToInteger"":  123123.0
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
            var condition = (ValueToIntegerProvider)model!.Build(this.dependencyProvider);

            // Assert
            var result = await condition.Resolve(new ProviderContext(automationData));
            result.GetValueOrThrowIfFailed().DataValue.Should().Be(123123);
        }

        [Fact]
        public async Task ValueToInteger_ShouldParseTheNumberWithDecimalValue_ThrowException()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                ""valueToInteger"":  123123.12
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
            var condition = (ValueToIntegerProvider)model!.Build(this.dependencyProvider);

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("provider.parameter.invalid");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ValueToInteger_ShouldParseTheInvalidValueboolean_ThrowException(bool value)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                ""valueToInteger"": " + value.ToString().ToLower() + @"
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
            var condition = (ValueToIntegerProvider)model!.Build(this.dependencyProvider);

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("provider.parameter.invalid");
        }

        [Fact]
        public async Task ValueToInteger_ShouldParseTheInvalidValueList_ThrowException()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                ""valueToInteger"":  [12312, 12312]
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
            var condition = (ValueToIntegerProvider)model!.Build(this.dependencyProvider);

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("provider.parameter.invalid");
        }

        [Theory]
        [InlineData("12312321abc")]
        [InlineData("abcdefg")]
        [InlineData("12312.asdas")]
        [InlineData("123,123,123,asdasd")]
        public async Task ValueToInteger_ShouldParseTheInvalidTextValue_ThrowException(string value)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateDataWithHttpTriggerAndContent();
            var json = @"{
                ""valueToInteger"": """ + value + @"""
            }";

            // Act
            var model = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(json, this.settings);
            var condition = (ValueToIntegerProvider)model!.Build(this.dependencyProvider);

            // Assert
            Func<Task> func = async () => await condition.Resolve(new ProviderContext(automationData));
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("provider.parameter.invalid");
        }
    }
}
