// <copyright file="AddOperationTests.cs" company="uBind">
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

    public class AddOperationTests
    {
        private readonly Mock<IServiceProvider> dependencyProviderMock = new Mock<IServiceProvider>();

        [Fact]
        public async Task AddOperation_Should_Add_The_Value()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed();

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["house"].ToString().Should().Be("12");
        }

        [Fact]
        public async Task AddOperation_Should_Add_The_Value_To_A_Sub_Object()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/street"",
                                    ""value"": [
                                         {
                                            ""propertyName"": ""myTestProperty"",
                                            ""value"": 42
                                         },
                                         {
                                            ""propertyName"": ""myTestProperty2"",
                                            ""value"": 43
                                         }
                                    ]
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            this.dependencyProviderMock.Setup(s => s.GetService(typeof(ILogger<DynamicObjectProvider>)))
                .Returns(new Mock<ILogger<DynamicObjectProvider>>().Object);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["street"]["myTestProperty"].Should().NotBeNull();
            patchedObject["street"]["myTestProperty"].ToString().Should().Be("42");
        }

        [Fact]
        public async Task AddOperation_Should_Add_The_Value_To_Root()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
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
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            this.dependencyProviderMock.Setup(s => s.GetService(typeof(ILogger<DynamicObjectProvider>)))
                .Returns(new Mock<ILogger<DynamicObjectProvider>>().Object);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["myTestProperty"].Should().NotBeNull();
            patchedObject["myTestProperty"].ToString().Should().Be("42");
        }

        [Fact]
        public async Task AddOperation_Should_Add_The_Value_To_Root_With_Slash()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/"",
                                    ""value"": [
                                         {
                                            ""propertyName"": ""myTestProperty"",
                                            ""value"": 42
                                         }
                                    ]
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            this.dependencyProviderMock.Setup(s => s.GetService(typeof(ILogger<DynamicObjectProvider>))).Returns(new Mock<ILogger<DynamicObjectProvider>>().Object);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["myTestProperty"].Should().NotBeNull();
            patchedObject["myTestProperty"].ToString().Should().Be("42");
        }

        [Fact]
        public async Task AddOperation_Should_Replace_The_Existing_Value()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/postcode"",
                                    ""value"": ""12"",
                                    ""whenPropertyAlreadyExists"": ""replace""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["postcode"].ToString().Should().Be("12");
        }

        [Fact]
        public async Task AddOperation_Should_Continue_Operation_When_Value_Already_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""add"": {
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
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
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
        public async Task AddOperation_Should_Throw_Exception_When_Value_Already_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/postcode"",
                                    ""value"": ""12"",
                                    ""whenPropertyAlreadyExists"": ""raiseError""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await patchObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be($"automation.providers.patchObject.operaton.failed");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Operation: Add");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Error Message: Property already exists");
        }

        [Fact]
        public async Task AddOperation_Should_End_Operation_When_Parent_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""add"": {
                                    ""path"": ""/street1/newName"",
                                    ""value"": ""12"",
                                    ""whenParentPropertyNotFound"": ""end""
                                 }
                              },
                              {
                                ""add"": {
                                    ""path"": ""/postcode"",
                                    ""value"": ""12""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
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
        public async Task AddOperation_Should_Continue_Operation_When_Parent_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""add"": {
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
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
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
        public async Task AddOperation_Should_Throw_Exception_When_Parent_Property_Does_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/street1/newName"",
                                    ""value"": ""12"",
                                    ""whenParentPropertyNotFound"": ""raiseError""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await patchObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be($"automation.providers.patchObject.operaton.failed");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Operation: Add");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Error Message: Parent property not found");
        }

        [Fact]
        public async Task AddOperation_Should_AddObject_When_Property_DoesNotExist_AndObjectToAddIsStronglyTyped()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/system"",
                                    ""value"": {
                                        ""objectPathLookupObject"": ""/system""
                                    }
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(automationData));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["system"].Children<JProperty>().Should().NotBeNull();
            patchedObject["system"]["environment"].ToString().Should().Be("development");
        }

        [Fact]
        public async Task AddOperation_Should_MergeTheTwoObjects_When_Property_AlreadyExists_AndObjectToAddIsStronglyTyped()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/street"",
                                    ""value"": {
                                        ""objectPathLookupObject"": ""/system""
                                    }
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(this.dependencyProviderMock.Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(automationData));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["street"].Children<JProperty>().Should().NotBeNull();
            patchedObject["street"]["environment"].ToString().Should().Be("development");
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
