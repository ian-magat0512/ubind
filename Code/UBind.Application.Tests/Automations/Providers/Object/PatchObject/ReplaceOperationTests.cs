// <copyright file="ReplaceOperationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Object.PatchObject
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Automation.Providers.Object.PatchObject;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class ReplaceOperationTests
    {
        private readonly Mock<IServiceProvider> dependencyProviderMock = new Mock<IServiceProvider>();

        [Fact]
        public async Task ReplaceOperation_Should_Replace_The_Value()
        {
            // Arrange
            var operation = @"{
                                ""replace"": {
                                    ""path"": ""/postcode"",
                                    ""value"": ""1212""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["postcode"].ToString().Should().Be("1212");
        }

        [Fact]
        public async Task ReplaceOperation_Should_Replace_Root_With_An_Object()
        {
            // Arrange
            var operation = @"{
                                ""replace"": {
                                    ""path"": """",
                                    ""value"": [
                                         {
                                            ""propertyName"": ""myTestProperty"",
                                            ""value"": 42
                                         }
                                    ]
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            this.dependencyProviderMock.Setup(s => s.GetService(typeof(ILogger<DynamicObjectProvider>)))
                .Returns(new Mock<ILogger<DynamicObjectProvider>>().Object);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
               .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["postcode"].Should().BeNull();
            patchedObject["myTestProperty"].Should().NotBeNull();
            patchedObject["myTestProperty"].ToString().Should().Be("42");
        }

        [Fact]
        public async Task ReplaceOperation_Should_Replace_An_Object_With_Another()
        {
            // Arrange
            var operation = @"{
                                ""replace"": {
                                    ""path"": ""/street"",
                                    ""value"": [
                                         {
                                            ""propertyName"": ""myTestProperty"",
                                            ""value"": 42
                                         }
                                    ]
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            this.dependencyProviderMock.Setup(s => s.GetService(typeof(ILogger<DynamicObjectProvider>)))
                .Returns(new Mock<ILogger<DynamicObjectProvider>>().Object);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
               .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["street"].Should().NotBeNull();
            patchedObject["street"]["newName"].Should().BeNull();
            patchedObject["street"]["myTestProperty"].Should().NotBeNull();
            patchedObject["street"]["myTestProperty"].ToString().Should().Be("42");
        }

        [Fact]
        public async Task ReplaceOperation_Should_Add_When_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""replace"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12"",
                                    ""whenPropertyNotFound"": ""add""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["house"].ToString().Should().Be("12");
        }

        [Fact]
        public async Task ReplaceOperation_Should_Continue_Operation_When_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""replace"": {
                                    ""path"": ""/street/newName"",
                                    ""value"": ""12"",
                                    ""whenPropertyNotFound"": ""continue""
                                 }
                              },
                              {
                                ""add"": {
                                    ""path"": ""/floor"",
                                    ""value"": ""12""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["house"].ToString().Should().Be("12");
            patchedObject["floor"].ToString().Should().Be("12");
        }

        [Fact]
        public async Task ReplaceOperation_Should_Throw_Exception_When_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""replace"": {
                                    ""path"": ""/postcodeXXX"",
                                    ""value"": ""12"",
                                    ""whenPropertyNotFound"": ""raiseError""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await patchObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be($"automation.providers.patchObject.operaton.failed");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Operation: Replace");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Error Message: Property not found");
        }

        [Fact]
        public async Task ReplaceOperation_Should_End_Operation_When_Parent_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""replace"": {
                                    ""path"": ""/street1/newName"",
                                    ""value"": ""12"",
                                    ""whenParentPropertyNotFound"": ""end""
                                 }
                              },
                              {
                                ""replace"": {
                                    ""path"": ""/house"",
                                    ""value"": ""1212""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["house"].ToString().Should().Be("12");
            patchedObject["postcode"].Value<long>().Should().Be(2000);
        }

        [Fact]
        public async Task ReplaceOperation_Should_Continue_Operation_When_Parent_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""replace"": {
                                    ""path"": ""/street1/newName"",
                                    ""value"": ""12"",
                                    ""whenParentPropertyNotFound"": ""continue""
                                 }
                              },
                              {
                                ""add"": {
                                    ""path"": ""/floor"",
                                    ""value"": ""12""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["house"].ToString().Should().Be("12");
            patchedObject["floor"].ToString().Should().Be("12");
        }

        [Fact]
        public async Task ReplaceOperation_Should_Throw_Exception_When_Parent_Property_Does_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""replace"": {
                                    ""path"": ""/street1/newName"",
                                    ""value"": ""12"",
                                    ""whenParentPropertyNotFound"": ""raiseError""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(
                json,
                AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await patchObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be($"automation.providers.patchObject.operaton.failed");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Operation: Replace");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Error Message: Parent property not found");
        }

        private string GetAutomationJson(string operation)
        {
            return @"{ 
                            ""object"": {
                                ""jsonObject"": ""{\""street\"": { \""newName\"": \""test1\"", \""oldName\"": \""test2\"" },\""suburb\"":\""Sydney\"",\""state\"":\""NSW\"",\""postcode\"":2000}""
                             },
                             ""operations"": 
                             ["
                                + operation +
                             @"]
                       }";
        }
    }
}
