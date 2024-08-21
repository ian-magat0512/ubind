// <copyright file="CopyOperationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Object.PatchObject
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object.PatchObject;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class CopyOperationTests
    {
        [Fact]
        public async Task CopyOperation_Should_Copy_The_Value()
        {
            // Arrange
            var operation = @"{
                                ""copy"": {
                                    ""from"": ""/suburb"",
                                    ""to"": ""/newSubUrb""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["newSubUrb"].ToString().Should().Be("Sydney");
        }

        [Fact]
        public async Task CopyOperation_Should_Copy_The_Object_To_Root()
        {
            // Arrange
            var operation = @"{
                                ""copy"": {
                                    ""from"": ""/street"",
                                    ""to"": """"
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

            // Act
            var resolvePatchedObject = await patchObjectProvider
               .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["newName"].ToString().Should().Be("test1");
        }

        [Fact]
        public async Task CopyOperation_Should_Copy_The_Object_To_A_Sub_Object()
        {
            // Arrange
            var operation = @"{
                                ""copy"": {
                                    ""from"": ""/person"",
                                    ""to"": ""/street""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

            // Act
            var resolvePatchedObject = (await patchObjectProvider
               .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()))).GetValueOrThrowIfFailed();
            JObject patchedObject = (JObject)resolvePatchedObject.DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["street"]["firstName"].ToString().Should().Be("Bob");
        }

        [Fact]
        public async Task CopyOperation_Should_Replace_The_Existing_Value()
        {
            // Arrange
            var operation = @"{
                                ""copy"": {
                                    ""from"": ""/suburb"",
                                    ""to"": ""/street"",
                                    ""whenDestinationPropertyAlreadyExists"": ""replace""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

            // Act
            var resolvePatchedObject = await patchObjectProvider
                .Resolve(new ProviderContext(await MockAutomationData.CreateWithHttpTrigger()));
            JObject patchedObject = (JObject)resolvePatchedObject.GetValueOrThrowIfFailed().DataValue;

            // Assert
            patchedObject.Should().NotBeNull();
            patchedObject["street"].ToString().Should().Be("Sydney");
        }

        [Fact]
        public async Task CopyOperation_Should_Continue_Operation_When_Source_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""copy"": {
                                    ""from"": ""/suburb_not_existing"",
                                    ""to"": ""/street"",
                                    ""whenSourcePropertyNotFound"": ""continue""
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
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

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
        public async Task CopyOperation_Should_Throw_Exception_When_Source_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""copy"": {
                                    ""from"": ""/suburb_not_existing"",
                                    ""to"": ""/street/suburb"",
                                    ""whenSourcePropertyNotFound"": ""raiseError""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await patchObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be($"automation.providers.patchObject.operaton.failed");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Operation: Copy");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Error Message: Source property not found");
        }

        [Fact]
        public async Task CopyOperation_Should_End_Operation_When_Source_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""copy"": {
                                    ""from"": ""/suburb_not_existing"",
                                    ""to"": ""/street"",
                                    ""whenSourcePropertyNotFound"": ""end""
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
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

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
        public async Task CopyOperation_Should_Continue_Operation_When_Destination_Property_Already_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""copy"": {
                                    ""from"": ""/suburb"",
                                    ""to"": ""/street"",
                                    ""whenDestinationPropertyAlreadyExists"": ""continue""
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
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

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
        public async Task CopyOperation_Should_End_Operation_When_Destination_Property_Already_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""copy"": {
                                    ""from"": ""/suburb"",
                                    ""to"": ""/street"",
                                    ""whenDestinationPropertyAlreadyExists"": ""end""
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
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

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
        public async Task CopyOperation_Should_Throw_Exception_When_Destination_Property_Already_Exists()
        {
            // Arrange
            var operation = @"{
                                ""copy"": {
                                    ""from"": ""/suburb"",
                                    ""to"": ""/street"",
                                    ""whenDestinationPropertyAlreadyExists"": ""raiseError""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await patchObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be($"automation.providers.patchObject.operaton.failed");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Operation: Copy");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Error Message: Destination property already exists");
        }

        [Fact]
        public async Task CopyOperation_Should_End_Operation_When_Destination_Parent_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""copy"": {
                                    ""from"": ""/suburb"",
                                    ""to"": ""/street_not_existing/newName"",
                                    ""whenDestinationParentPropertyNotFound"": ""end""
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
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

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
        public async Task CopyOperation_Should_Continue_Operation_When_Destination_Parent_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""copy"": {
                                    ""from"": ""/suburb"",
                                    ""to"": ""/street_not_existing/newName"",
                                    ""whenDestinationParentPropertyNotFound"": ""continue""
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
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);

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
        public async Task CopyOperation_Should_Throw_Exception_When_Destination_Parent_Property_Does_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""copy"": {
                                    ""from"": ""/suburb"",
                                    ""to"": ""/street_not_existing/newName"",
                                    ""whenDestinationParentPropertyNotFound"": ""raiseError""
                                 }
                              }";
            var json = this.GetAutomationJson(operation);
            var patchObjectProviderBuilder = JsonConvert.DeserializeObject<PatchObjectProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var patchObjectProvider = patchObjectProviderBuilder.Build(null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await patchObjectProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be($"automation.providers.patchObject.operaton.failed");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Operation: Copy");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Error Message: Destination parent property not found");
        }

        private string GetAutomationJson(string operation)
        {
            return @"{ 
                            ""object"": {
                                ""jsonObject"": ""{\""street\"": { \""newName\"": \""test1\"", \""oldName\"": \""test2\"" },\""suburb\"":\""Sydney\"",\""state\"":\""NSW\"",\""postcode\"":2000,\""person\"": { \""firstName\"": \""Bob\"", \""lastName\"": \""Smith\"" }}""
                             },
                             ""operations"": 
                             ["
                                + operation +
                             @"]
                       }";
        }
    }
}
