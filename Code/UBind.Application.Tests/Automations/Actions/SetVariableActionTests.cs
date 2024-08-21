// <copyright file="SetVariableActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using Action = UBind.Application.Automation.Actions.Action;

    [SystemEventTypeExtensionInitialize]
    public class SetVariableActionTests
    {
        [Theory]
        [InlineData("test", "\"value\"", "value")]
        [InlineData("test", "1", "1")]
        [InlineData("test", "true", "true")]
        [InlineData("test", "[ { \"propertyName\": \"item\", \"value\": \"textValue\" }]", "{\"item\":\"textValue\"}")]
        [InlineData("test", "[[ { \"propertyName\": \"item\", \"value\": \"textValue\" }], [ { \"propertyName\": \"item2\", \"value\": \"textValue2\" }]]", "[{\"item\":\"textValue\"},{\"item2\":\"textValue2\"}]")]
        [InlineData("test", "[1,2,3]", "[1,2,3]")]
        public async Task SetVariableAction_ShouldSuceed_WithValue(string propertyName, string jsonToken, string expectedValue)
        {
            // Arrange
            var dataProviderBuilder = JsonConvert.DeserializeObject<IBuilder<IProvider<IData>>>(
                jsonToken, AutomationDeserializationConfiguration.ModelSettings);
            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            var dataProvider = dataProviderBuilder.Build(serviceProvider);
            var propertyNameProvider = new StaticProvider<Data<string>>(propertyName);
            var setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyNameProvider,
                dataProvider,
                new TestClock(),
                null,
                null,
                serviceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await setVariableAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            if (propertyName != null)
            {
                automationData.Variables.ContainsKey(propertyName).Should().BeTrue();
                var data = automationData.Variables.GetValue(propertyName);
                if (data.Type == JTokenType.String)
                {
                    data.ToString().Should().Be(expectedValue);
                }
            }
            else
            {
                var data = automationData.Variables.GetValue(propertyName);
                var dataToString = JsonConvert.SerializeObject(data);
                dataToString.Should().Be(expectedValue);
            }
        }

        [Fact]
        public async Task SetVariableAction_ShouldSuceed_WithBinaryValue()
        {
            // Arrange
            var expectedValue = Encoding.UTF8.GetBytes("This is some binary content");
            var binaryProvider = new StaticProvider<Data<byte[]>>(expectedValue);
            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            var propertyNameProvider = new StaticProvider<Data<string>>("myBinaryData");
            var setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyNameProvider,
                binaryProvider,
                new TestClock(),
                null,
                null,
                serviceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await setVariableAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            automationData.Variables.ContainsKey("myBinaryData").Should().BeTrue();
            JToken? data = automationData.Variables.GetValue("myBinaryData");
            data.Should().NotBeNull();
            data.Type.Should().Be(JTokenType.Bytes);
            var byteArray = data.ToObject<byte[]>();
            byteArray.Should().Equal(expectedValue);
        }

        [Fact]
        public async Task SetVariableAction_ShouldSuceed_WithFileValue()
        {
            // Arrange
            var fileContent = Encoding.UTF8.GetBytes("This is the file contents");
            var expectedValue = new FileInfo("myfile.txt", fileContent);
            var fileProvider = new StaticProvider<Data<FileInfo>>(expectedValue);
            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            var propertyNameProvider = new StaticProvider<Data<string>>("myFile");
            var setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyNameProvider,
                fileProvider,
                new TestClock(),
                null,
                null,
                serviceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await setVariableAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            automationData.Variables.ContainsKey("myFile").Should().BeTrue();
            JToken? data = automationData.Variables.GetValue("myFile");
            var fileInfo = data.ToObject<FileInfo>();
            fileInfo.FileName.Should().Be(fileInfo.FileName);
            fileInfo.Content.Should().Equal(expectedValue.Content);
        }

        [Fact]
        public async Task SetVariableAction_ShouldSuceed_WithProperties()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            var dummyData = new Dictionary<IProvider<Data<string>>, IProvider<IData>>()
            {
                { new StaticProvider<Data<string>>("ContactName"), (IProvider<IData>)new StaticProvider<Data<string>>("Jane Doe") },
                { new StaticProvider<Data<string>>("ContactAddress"), (IProvider<IData>)new StaticProvider<Data<long>>(100) },
                { new StaticProvider<Data<string>>("ContactTown"), (IProvider<IData>)new StaticProvider<Data<bool>>(false) },
            };
            var propertyProvider = new DynamicObjectProvider(
                dummyData,
                new Mock<ILogger<DynamicObjectProvider>>().Object,
                serviceProvider);
            var setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                null,
                null,
                new TestClock(),
                null,
                propertyProvider,
                serviceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await setVariableAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            automationData.Variables.ContainsKey("ContactName").Should().BeTrue();
            automationData.Variables.ContainsKey("ContactAddress").Should().BeTrue();
            automationData.Variables.ContainsKey("ContactTown").Should().BeTrue();
        }

        [Fact]
        public async Task SetVariableAction_ShouldSuceed_CreatePathIfDoesntExists()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            var pathProvider = new StaticProvider<Data<string>>("something/else");
            var propertyName = new StaticProvider<Data<string>>("name");
            var valueProvider = (IProvider<IData>)new StaticProvider<Data<string>>("Peter");
            var setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyName,
                valueProvider,
                new TestClock(),
                pathProvider,
                null,
                serviceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await setVariableAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            automationData.Variables.ToString().Should().Be("{\r\n  \"something\": {\r\n    \"else\": {\r\n      \"name\": \"Peter\"\r\n    }\r\n  }\r\n}");
        }

        [Fact]
        public async Task SetVariableAction_ShouldError_IfUpdatingPathThatIsPrimitiveType()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
            var pathProvider = new StaticProvider<Data<string>>("something/else");
            var propertyName = new StaticProvider<Data<string>>("name");
            var valueProvider = (IProvider<IData>)new StaticProvider<Data<string>>("Peter");
            var setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyName,
                valueProvider,
                new TestClock(),
                pathProvider,
                null,
                serviceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await setVariableAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            automationData.Variables.ToString().Should().Be("{\r\n  \"something\": {\r\n    \"else\": {\r\n      \"name\": \"Peter\"\r\n    }\r\n  }\r\n}");

            // Arrange
            pathProvider = new StaticProvider<Data<string>>("something/else/name");
            propertyName = new StaticProvider<Data<string>>("name");
            valueProvider = (IProvider<IData>)new StaticProvider<Data<string>>("Peter");
            setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyName,
                valueProvider,
                new TestClock(),
                pathProvider,
                null,
                serviceProvider);
            actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            var result = await setVariableAction.Execute(new ProviderContext(automationData), actionData);
            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("automation.action.set.variable.failed");
        }

        [Fact]
        public async Task SetVariableAction_ShouldSuceed_InUpdatingValueIfKeyExists()
        {
            var expectedValue = "new value";
            var propertyName = "propertyName";
            var dataProvider = new StaticProvider<IData>(new Data<string>(expectedValue));
            var propertyNameProvider = new StaticProvider<Data<string>>(propertyName);
            var setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyNameProvider,
                dataProvider,
                new TestClock(),
                null,
                null,
                new Mock<IServiceProvider>().Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            automationData.AddOrUpdateVariableByPath("previous value", null, propertyName);
            var actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await setVariableAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            automationData.Variables.ContainsKey(propertyName).Should().BeTrue();
            var data = automationData.Variables.GetValue(propertyName);
            data.ToString().Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("$sample1")]
        [InlineData("Sample2")]
        [InlineData("sa-mple3")]
        public async Task SetVariableAction_ShouldThrowError_WhenVariableNameIsNotValid(string invalidPropertyName)
        {
            var dataProvider = new StaticProvider<IData>(new Data<string>("value"));
            var propertyNameProvider = new StaticProvider<Data<string>>(invalidPropertyName);
            var setVariableAction = new SetVariableAction(
                "Set variable",
                "setPropertyValue",
                string.Empty,
                false,
                null,
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<ErrorCondition>(),
                Enumerable.Empty<Action>(),
                propertyNameProvider,
                dataProvider,
                new TestClock(),
                null,
                null,
                new Mock<IServiceProvider>().Object);
            var automationData = MockAutomationData.CreateWithEventTrigger();
            var actionData = setVariableAction.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            var result = await setVariableAction.Execute(new ProviderContext(automationData), actionData);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Title.Should().Be("Object property key must have a valid property name");
        }
    }
}
