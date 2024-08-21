// <copyright file="PathListObjectProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Object;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class PathListObjectProviderTests
    {
        private readonly Mock<IServiceProvider> dependencyProviderMock = new Mock<IServiceProvider>();

        [Theory]
        [InlineData("FooBar")]
        [InlineData("2fooBar")]
        [InlineData("foo bar")]
        [InlineData("foo-bar")]
        public async Task PathListObjectProvider_ShouldThrowException_WhenPropertyNameIsInvalid(string propertyName)
        {
            // Arrange
            var propertyNameProvider = new StaticProvider<Data<string>>(propertyName);
            var pathProvider = new StaticProvider<Data<string>>("/foo");
            var objectPathProperty = new ObjectPathProperty(
                propertyNameProvider, pathProvider, null, null, null, null, null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            List<ObjectPathProperty> pathPropertyList = new List<ObjectPathProperty>
            {
                objectPathProperty,
            };
            var pathListObjectProvider = new PathLookupListObjectProvider(pathPropertyList, null);

            // Act
            Func<Task> func = async () => await pathListObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.provider.invalid.property.key");
        }

        [Theory]
        [InlineData("/Foo")]
        [InlineData("Foo")]
        [InlineData("context.foo")]
        [InlineData("/context-bar/foo")]
        [InlineData("/foo/0/bar!")]
        public async Task PathListObjectProvider_ShouldThrowException_WhenPathIsInvalid(string path)
        {
            // Arrange
            var propertyNameProvider = new StaticProvider<Data<string>>("foo");
            var pathProvider = new StaticProvider<Data<string>>(path);
            var objectPathProperty = new ObjectPathProperty(
                propertyNameProvider, pathProvider, null, null, null, null, null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            List<ObjectPathProperty> pathPropertyList = new List<ObjectPathProperty>
            {
                objectPathProperty,
            };
            var pathListObjectProvider = new PathLookupListObjectProvider(pathPropertyList, null);

            // Act
            Func<Task> func = async () => await pathListObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.path.syntax.error");
        }

        [Theory]
        [InlineData("/trigger")]
        [InlineData("/triggers/0/triggerAlias")]
        [InlineData("/actions/0/actionAlias")]
        public async Task PathListObjectProvider_ShouldCreateObject_WhenPathIsValid(string path)
        {
            // Arrange
            var propertyNameProvider = new StaticProvider<Data<string>>("foo");
            var pathProvider = new StaticProvider<Data<string>>(path);
            var dataDictionary = new Dictionary<string, object>
            {
                { "trigger", "fooTrigger" },
            };

            var triggerList = new List<Dictionary<string, object>>();
            var actionList = new List<Dictionary<string, object>>();
            for (int i = 0; i < 3; i++)
            {
                var action = new Dictionary<string, object>
                {
                    { "actionAlias", $"action{i}" },
                };
                var trigger = new Dictionary<string, object>
                {
                    { "triggerAlias", $"trigger{i}" },
                };
                triggerList.Add(trigger);
                actionList.Add(action);
            }

            dataDictionary.Add("triggers", triggerList);
            dataDictionary.Add("actions", actionList);
            var jsonString = JsonConvert.SerializeObject(dataDictionary);
            var dataObjectProvider = new JsonTextToObjectProvider(new StaticProvider<Data<string>>(jsonString));
            var objectPathProperty = new ObjectPathProperty(
                propertyNameProvider, pathProvider, null, null, null, null, null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            List<ObjectPathProperty> pathPropertyList = new List<ObjectPathProperty>
            {
                objectPathProperty,
            };
            var pathListObjectProvider = new PathLookupListObjectProvider(pathPropertyList, dataObjectProvider);

            // Act
            var dictionary = (await pathListObjectProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            dictionary.Should().NotBeNull();
            DataObjectHelper.ContainsProperty(dictionary.DataValue, "foo").Should().BeTrue();
            DataObjectHelper.GetPropertyValue(dictionary.DataValue, "foo").Should().NotBeNull();
            DataObjectHelper.GetPropertyValue(dictionary.DataValue, "foo").ToString().Should().NotBeEmpty();
        }

        [Fact]
        public void PathListObjectProvider_Converter_ShouldCreateAnInstanceOfTheBuilderModelWithDataObject()
        {
            // Arrange
            var jsonString = @"{
                ""properties"": [
                    {
                        ""propertyName"": ""foo"",
                        ""path"": ""/foo"",
                        ""valueIfNotFound"": ""Hello World""
                    },
                    {
                        ""propertyName"": ""bar"",
                        ""path"": ""/bar"",
                        ""valueIfNotFound"": ""Kon'nichiwa sekai""
                    },
                    {
                        ""propertyName"": ""baz"",
                        ""path"": ""/baz"",
                        ""valueIfNotFound"": ""Privet mir""
                    },
                    {
                        ""propertyName"": ""bat"",
                        ""path"": ""/bat"",
                        ""valueIfNotFound"": ""marhaban bialealam""
                    }
                ],
                ""dataObject"": [
                    {
                        ""propertyName"": ""foo"",
                        ""value"": ""English""
                    },
                    {
                        ""propertyName"": ""bar"",
                        ""value"": ""Nihongo""
                    },
                    {
                        ""propertyName"": ""baz"",
                        ""value"": ""Russian""
                    },
                    {
                        ""propertyName"": ""bat"",
                        ""value"": ""Arabic""
                    }
                ]
            }";

            // Act
            var model = JsonConvert.DeserializeObject<PathLookupListObjectProviderConfigModel>(
                jsonString,
                AutomationDeserializationConfiguration.ModelSettings);
            var mockLogger = new Mock<ILogger<DynamicObjectProvider>>();
            this.dependencyProviderMock.Setup(s => s.GetService(typeof(ILogger<DynamicObjectProvider>)))
                .Returns(mockLogger.Object);

            var provider = model.Build(this.dependencyProviderMock.Object);

            // Assert
            model.Properties.Should().HaveCount(4);
            model.DataObject.Should().NotBeNull();
            provider.Should().NotBeNull();
        }
    }
}
