// <copyright file="JsonTextToObjectProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Object
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class JsonTextToObjectProviderTests
    {
        [Theory]
        [InlineData(@"{""foo"": ""fooz"",  ""baz"": true, ""quarks"": 12}", "fooz")]
        [InlineData(@"{""foo"": ""my\foos\foofoo"", ""baz"": true, ""quarks"": 20 }", "my\foos\foofoo")]
        public async Task JsonTextToObjectProvider_ShouldCreateDataObject_FromNonEscapedJsonString(string jsonString, string foozValue)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.BuildJsonTextToObjectProvider(jsonString);

            // Act
            var dataObject = (await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            var fooValue = DataObjectHelper.GetPropertyValue(dataObject.DataValue, "foo");
            var bazValue = DataObjectHelper.GetPropertyValue(dataObject.DataValue, "baz");

            DataObjectHelper.CountProperties(dataObject.DataValue).Should().Be(3);
            fooValue.ToString().Should().Be(foozValue);
            bazValue.ToString().Should().Be("True");
        }

        [Theory]
        [InlineData("{\r\n   \"foo\": \"fooz\",\r\n   \"baz\": true,\r\n   \"quarks\": 12\r\n}", "fooz")]
        [InlineData("\"{\\r\\n  \\\"foo\\\": \\\"fooz\\\",\\r\\n  \\\"baz\\\": \\\"True\\\",\\r\\n  \\\"quarks\\\": \\\"quazars\\\"\\r\\n}\"", "fooz")]
        public async Task JsonTextToObjectProvider_ShouldCreateDataObject_FromEscapedJsonString(string jsonString, string foo)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.BuildJsonTextToObjectProvider(jsonString);

            // Act
            var dataObject = (await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            var fooValue = DataObjectHelper.GetPropertyValue(dataObject.DataValue, "foo");
            var bazValue = DataObjectHelper.GetPropertyValue(dataObject.DataValue, "baz");

            DataObjectHelper.CountProperties(dataObject.DataValue).Should().Be(3);
            fooValue.ToString().Should().Be(foo);
            bazValue.ToString().Should().Be("True");
        }

        [Theory]
        [InlineData(@"""test""")]
        [InlineData("{\"foo\"}")]
        [InlineData("\"{\\r\\n \\\"foo\\\"\\r\\n}\"")]
        public async Task JsonTextToObjectProvider_ShouldRaiseAnExceptiont_FromInvalidJsonTextToObject(string jsonString)
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.BuildJsonTextToObjectProvider(jsonString);

            // Act & Assert
            Func<Task> action = async () => await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData));
            await action.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task JsonTextToObjectProvider_ShouldNotThrowException_WhenJsonStringProvidedIsEmptyObject()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonString = "{}";
            var jsonTextToObjectProvider = this.BuildJsonTextToObjectProvider(jsonString);

            // Act + Assert
            jsonTextToObjectProvider.Resolve(new ProviderContext(automationData)).Should().NotBeNull();
        }

        [Fact]
        public async Task JsonTextToObjectProvider_ShouldRaiseException_WhenStringIsEmpty()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonString = string.Empty;
            var jsonTextToObjectProvider = this.BuildJsonTextToObjectProvider(jsonString);

            // Act + Assert
            Func<Task> action = async () => await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData));
            await action.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task JsonTextToObjectProvider_ShouldCreateJsonTextToObject_FromValidJsonTextString()
        {
            // Arrange
            var jsonString = @"{ ""fooProperty"": ""fooz"", ""barProperty"": ""bar"" }";
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.CreateJsonTextToObjectProvider(jsonString);

            // Act
            var jsonTextToObject = (await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            DataObjectHelper.CountProperties(jsonTextToObject.DataValue).Should().NotBe(0);
            DataObjectHelper.GetPropertyValue(jsonTextToObject.DataValue, "fooProperty").ToString().Should().Be("fooz");
            DataObjectHelper.GetPropertyValue(jsonTextToObject.DataValue, "barProperty").ToString().Should().Be("bar");
        }

        [Fact]
        public async Task JsonTextToObjectProvider_ShouldCreatJsonTextToOBject_FromJsonStringWithEscapedCharacters()
        {
            // Arrange
            var jsonString = @"{ ""fooProperty"": [ ""fooz"", ""foor"", ""foob""], ""barProperty"": ""bar"" }";
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.CreateJsonTextToObjectProvider(jsonString);

            // Act
            var jsonTextToObject = (await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            DataObjectHelper.CountProperties(jsonTextToObject.DataValue).Should().NotBe(0);
            DataObjectHelper.GetPropertyValue(jsonTextToObject.DataValue, "barProperty").ToString().Should().Be("bar");
        }

        [Fact]
        public async Task JsonTextToObjectProvider_ShouldCreateJsonTextToObject_FromJsonStringWithObjectProperties()
        {
            // Arrange
            var jsonString = @"{
                ""fooProperty"": ""fooz"",
                ""barProperty"": ""bar""
            }";
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.CreateJsonTextToObjectProvider(jsonString);

            // Act
            var jsonTextToObject = (await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            DataObjectHelper.CountProperties(jsonTextToObject.DataValue).Should().NotBe(0);
            DataObjectHelper.GetPropertyValue(jsonTextToObject.DataValue, "fooProperty").ToString().Should().Be("fooz");
            DataObjectHelper.GetPropertyValue(jsonTextToObject.DataValue, "barProperty").ToString().Should().Be("bar");
        }

        [Fact]
        public async Task JsonTextToObjectProvider_ShouldRaiseAnException_WHenProvidedValueIsNotAJsonString()
        {
            // Arrange
            var jsonString = "test: 'i'";
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.CreateJsonTextToObjectProvider(jsonString);

            // Act + Assert
            Func<Task> action = async () => await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData));
            await action.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task JsonTextToObjectProvider_ShouldRaiseException_WhenStringProvidedIsEmpty()
        {
            // Arrange
            var jsonString = string.Empty;
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.CreateJsonTextToObjectProvider(jsonString);

            // Act + Assert
            Func<Task> action = async () => await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData));
            await action.Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task JsonTextToObjectProvider_ShouldRaiseException_WhenStringProvidedIsOfAJArrayOnly()
        {
            // Arrange
            var jsonString = @"{[ { ""foo1"" : ""fooVal1"", ""foo2"": ""fooVal2"" }, { ""bar1"": ""barVal1"" } ] }";
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var jsonTextToObjectProvider = this.CreateJsonTextToObjectProvider(jsonString);

            // Act + Assert
            Func<Task> action = async () => await jsonTextToObjectProvider.Resolve(new ProviderContext(automationData));
            await action.Should().ThrowAsync<ErrorException>();
        }

        private IObjectProvider CreateJsonTextToObjectProvider(string jsonString = null)
        {
            var fixedTextProviderModel = new StaticBuilder<Data<string>>() { Value = jsonString };
            var jsonObjectProviderModel = new JsonTextToObjectProviderConfigModel() { TextProvider = fixedTextProviderModel };
            var jsonObjectProvider = jsonObjectProviderModel.Build(null);
            return jsonObjectProvider;
        }

        private IObjectProvider BuildJsonTextToObjectProvider(string jsonString)
        {
            var textProviderModel = new StaticBuilder<Data<string>>() { Value = jsonString };
            var providerModel = new JsonTextToObjectProviderConfigModel() { TextProvider = textProviderModel };
            var objectProvider = providerModel.Build(null);
            return objectProvider;
        }
    }
}
