// <copyright file="DataObjectHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Helper
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Http;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class DataObjectHelperTests
    {
        [Fact]
        public async Task DataObjectHelper_IsArray_ShouldReturnTrueForListProviderValues()
        {
            // Arrange
            var genericList = new GenericDataList<object>(new List<string> { "hello", "world", "i'm", "pinnochio" });
            var listProvider = new StaticListProvider<object>(genericList, null);
            var providerContext = new ProviderContext(await MockAutomationData.CreateWithHttpTrigger());
            var listValue = (await listProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Act
            var isList = DataObjectHelper.IsArray(listValue);

            // Assert
            isList.Should().BeTrue();
        }

        [Fact]
        public async Task DataObjectHelper_IsArray_ShouldReturnTrueForHeaderValuesReturnedByHeaderProvider()
        {
            // Arrange
            var headerValues = new List<string> { "hi", "world", "im", "cinderella" };
            var headerValuesProviders = new List<IProvider<Data<string>>>();
            foreach (var value in headerValues)
            {
                headerValuesProviders.Add(new StaticProvider<Data<string>>(value));
            }

            var headerProvider = new HttpHeaderProvider(
                new StaticProvider<Data<string>>("intro"),
                headerValuesProviders);
            var providerContext = new ProviderContext(await MockAutomationData.CreateWithHttpTrigger());
            var header = (await headerProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            // Act
            var isList = DataObjectHelper.IsArray(header.Value);

            // Assert
            isList.Should().BeTrue();
        }

        [Fact]
        public void DataObjectHelper_IsArray_ShouldReturnTrueForHeaderValuesRetrievedFromTriggerData()
        {
            // Arrange
            var headers = new Dictionary<string, StringValues>()
            {
                { "fruits", new StringValues(new string[] { "apple", "pear", "banana" }) },
            };
            var triggerData = new TriggerRequest(
                "www.test.com",
                "GET",
                string.Empty,
                headers);

            // Act + Assert
            triggerData.Headers.TryGetValue("fruits", out StringValues fruits);
            DataObjectHelper.IsArray(fruits).Should().BeTrue();
        }

        [Fact]
        public async Task DataObjectHelper_IsArray_ShouldReturnFalseIfValueIsDictionary()
        {
            // Arrange
            var objectProperties = new Dictionary<IProvider<Data<string>>, IProvider<IData>>()
            {
                { new StaticProvider<Data<string>>("first"), (IProvider<IData>)new StaticProvider<Data<string>>("1st") },
                { new StaticProvider<Data<string>>("second"), (IProvider<IData>)new StaticProvider<Data<long>>(2) },
            };
            var providerContext = new ProviderContext(await MockAutomationData.CreateWithHttpTrigger());
            var dataProvider = new DynamicObjectProvider(
                objectProperties,
                new Mock<ILogger<DynamicObjectProvider>>().Object,
                providerContext.AutomationData.ServiceProvider);
            var dataObject = dataProvider.Resolve(providerContext);

            // Act + Assert
            DataObjectHelper.IsArray(dataObject).Should().BeFalse();
        }

        [Theory]
        [InlineData(12, false)]
        [InlineData(true, false)]
        [InlineData("item", false)]
        [InlineData(new string[] { "hi", "ho" }, true)]
        public void DataObjectHelper_IsArray_ShouldReturnCorrectEvaluation(object itemToEvaluate, bool isArray)
        {
            // Arrange
            var data = new Data<object>(itemToEvaluate);

            // Act + Assert
            DataObjectHelper.IsArray(data).Should().Be(isArray);
        }

        [Fact]
        public void DataObjectHelper_TryGetPropertyValue_ShouldReturnStringIfPropertyNameVary()
        {
            // Arrange
            AutomationSystemData automationSystemData = new AutomationSystemData(
                DeploymentEnvironment.Production,
                "testurl",
                NodaTime.DateTimeZone.Utc,
                new TestClock());

            // Act
            DataObjectHelper.TryGetPropertyValue(automationSystemData, "environmentName", out object value);
            DataObjectHelper.TryGetPropertyValue(automationSystemData, "EnvironmentName", out object value2);

            // Assert
            value.Should().BeOfType(typeof(string));
            value2.Should().BeOfType(typeof(string));
        }

        [Fact]
        public void DataObjectHelper_TryGetPropertyValueForJsonConverters_ShouldReturnStringFromEnum()
        {
            // Arrange
            AutomationSystemData automationSystemData = new AutomationSystemData(
                DeploymentEnvironment.Production,
                "testurl",
                NodaTime.DateTimeZone.Utc,
                new TestClock());

            // Act
            DataObjectHelper.TryGetPropertyValue(automationSystemData, "Environment", out object value);
            DataObjectHelper.TryGetPropertyValue(automationSystemData, "environment", out object value2);

            // Assert
            value.ToString().Should().Be("production");
            value2.ToString().Should().Be("production");
        }

        [Fact]
        public void DataObjectHelper_TryGetPropertyValueForJsonConverters_HeadersShouldReturnString()
        {
            // Arrange
            var response = new Response(
                123,
                "test",
                new Dictionary<string, StringValues>
                {
                    { "qwee", "ewq" },
                },
                "test",
                "testtt",
                new byte[] { 100, 100 });

            // Act
            DataObjectHelper.TryGetPropertyValue(response, "headers", out object value);

            // Assert
            value.ToString().Should().Be("{\r\n  \"qwee\": \"ewq\"\r\n}");
        }

        [Fact]
        public void DataObjectHelper_TryGetPropertyValueForJsonConverters_ContentByteShouldBeByte()
        {
            // Arrange
            var testByte = new byte[] { 100, 100 };
            var response = new Response(
                123,
                "test",
                new Dictionary<string, StringValues>
                {
                    { "qwee", "ewq" },
                },
                "test",
                "testtt",
                testByte);

            // Act
            DataObjectHelper.TryGetPropertyValue(response, "content", out object value);

            // Assert
            value.Should().Be(testByte);
        }

        [Fact]
        public void DataObjectHelper_TryGetPropertyValueForJsonConverters_ContentStringShouldBeString()
        {
            // Arrange
            var testContent = "string test";
            var response = new Response(
                123,
                "test",
                new Dictionary<string, StringValues>
                {
                    { "qwee", "ewq" },
                },
                "test",
                "testtt",
                testContent);

            // Act
            DataObjectHelper.TryGetPropertyValue(response, "content", out object value);

            // Assert
            value.Should().Be(testContent);
        }

        [Fact]
        public void DataObjectHelper_TryGetPropertyValueForJsonConverters_HttpMethodShouldBeString()
        {
            // Arrange
            var response = new Request(
                "url",
                "testMethod",
                new Dictionary<string, StringValues>
                {
                    { "qwee", "ewq" },
                },
                "testt",
                "characterSet",
                "testing string",
                null);

            // Act
            DataObjectHelper.TryGetPropertyValue(response, "httpverb", out object value);

            // Assert
            value.ToString().Should().Be("testMethod");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(123.45)]
        [InlineData(3.14159f)]
        public void DataObjectHelper_IsPrimitive_ShouldReturnTrueForSupportedPrimitiveTypes(object value)
        {
            // Arrange
            var data = new Data<object>(value).DataValue;

            // Act + Assert
            DataObjectHelper.IsPrimitive(data).Should().BeTrue();
        }
    }
}
