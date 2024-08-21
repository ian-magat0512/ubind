// <copyright file="RemoveOperationTests.cs" company="uBind">
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

    public class RemoveOperationTests
    {
        [Fact]
        public async Task RemoveOperation_Should_Remove_The_Value()
        {
            // Arrange
            var operation = @"{
                                ""remove"": {
                                    ""path"": ""/suburb""
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
            patchedObject.Should().NotContainKey("suburb");
        }

        [Fact]
        public async Task RemoveOperation_Should_Remove_Everything_When_Path_Is_Root()
        {
            // Arrange
            var operation = @"{
                                ""remove"": {
                                    ""path"": """"
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
            patchedObject.Should().NotContainKey("street");
            patchedObject.Should().NotContainKey("suburb");
        }

        [Fact]
        public async Task RemoveOperation_Should_Remove_Sub_Properties_When_Path_Is_To_Object()
        {
            // Arrange
            var operation = @"{
                                ""remove"": {
                                    ""path"": ""/street""
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
            patchedObject.Should().NotContainKey("street");
            patchedObject.Should().ContainKey("suburb");
        }

        [Fact]
        public async Task RemoveOperation_Should_End_Operation_When_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""remove"": {
                                    ""path"": ""/street1"",
                                    ""whenPropertyNotFound"": ""end""
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
        public async Task RemoveOperation_Should_Continue_Operation_When_Property_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""add"": {
                                    ""path"": ""/house"",
                                    ""value"": ""12""
                                 }
                              },
                              {
                                ""remove"": {
                                    ""path"": ""/street1"",
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
        public async Task RemoveOperation_Should_Throw_Exception_When_Property_Does_Not_Exists()
        {
            // Arrange
            var operation = @"{
                                ""remove"": {
                                    ""path"": ""/street1"",
                                    ""whenPropertyNotFound"": ""raiseError""
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
            exception.Which.Error.AdditionalDetails.Should().Contain($"Operation: Remove");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Error Message: Property not found");
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
