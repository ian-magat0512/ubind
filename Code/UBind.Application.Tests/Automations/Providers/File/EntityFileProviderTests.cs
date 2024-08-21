// <copyright file="EntityFileProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using Xunit;

    public class EntityFileProviderTests
    {
        [Theory]
        [InlineData("quote")]
        [InlineData("quoteVersion")]
        [InlineData("policyTransaction")]
        [InlineData("claim")]
        [InlineData("claimVersion")]
        public async Task EntityFileProvider_Should_Return_FileInfo(string type)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
					            ""outputFilename"": ""brown_fox.txt"",
                                ""filename"": ""brown_fox.txt"",
								""entity"" : {
                                        ""dynamicEntity"": {
                                                ""entityType"": """ + type + @""",
                                                ""entityId"": """ + entityId.ToString() + @"""
                                         }
                                 }
                        }";

            var entityFileProviderBuilder = JsonConvert.DeserializeObject<EntityFileProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = MockAutomationData.GetServiceProviderForEntityProviders(entityId, true);
            var entityFileProvider = entityFileProviderBuilder.Build(mockServiceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var fileInfo = (await entityFileProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.DataValue.FileName.ToString().Should().Be("brown_fox.txt");

            var fileContent = Encoding.UTF8.GetString(fileInfo.DataValue.Content);
            fileContent.Should().Be("This is a test file");
        }

        [Theory]
        [InlineData("", "Filename is blank.")]
        [InlineData("morethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharacters", "Filename is too long (must be 255 characters or less)")]
        [InlineData("filename<.txt", "Filename contains an invalid character(s): <")]
        [InlineData("filename>.txt", "Filename contains an invalid character(s): >")]
        [InlineData("file:name.txt", "Filename contains an invalid character(s): :")]
        [InlineData("filename/asd.txt", "Filename contains an invalid character(s): /")]
        [InlineData("filen|amezz.txt", "Filename contains an invalid character(s): |")]
        [InlineData("file?namerr.txt", "Filename contains an invalid character(s): ?")]
        [InlineData("*filenamenff.txt", "Filename contains an invalid character(s): *")]
        public async Task EntityFileProvider_Throw_When_FileName_Is_InValid(string filename, string expectedErrorMessage)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
					            ""outputFilename"": """ + filename + @""",
                                ""filename"": ""brown_fox.txt"",
								""entity"" : {
                                        ""dynamicEntity"": {
                                                ""entityType"": ""quote"",
                                                ""entityId"": """ + entityId.ToString() + @"""
                                         }
                                 }
                        }";

            var entityFileProviderBuilder = JsonConvert.DeserializeObject<EntityFileProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = MockAutomationData.GetServiceProviderForEntityProviders(entityId, false);
            var entityFileProvider = entityFileProviderBuilder.Build(mockServiceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            Func<Task> func = async () => await entityFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Product file must have a valid filename");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity Type: quote");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity ID: {entityId}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Filename: brown_fox.txt");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Invalid Reason: {expectedErrorMessage}");
        }

        [Theory]
        [InlineData("user")]
        [InlineData("tenant")]
        [InlineData("organisation")]
        [InlineData("customer")]
        [InlineData("emailMessage")]
        [InlineData("product")]
        public async Task EntityFileProvider_Throw_When_Entity_Does_Not_Support_FileAttachment(string type)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
					            ""outputFilename"": ""brown_fox.txt"",
                                ""filename"": ""brown_fox.txt"",
								""entity"" : {
                                        ""dynamicEntity"": {
                                                ""entityType"": """ + type + @""",
                                                ""entityId"": """ + entityId.ToString() + @"""
                                         }
                                 }
                        }";

            var entityFileProviderBuilder = JsonConvert.DeserializeObject<EntityFileProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = MockAutomationData.GetServiceProviderForEntityProviders(entityId, false);
            var entityFileProvider = entityFileProviderBuilder.Build(mockServiceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            Func<Task> func = async () => await entityFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = (await func.Should().ThrowAsync<ErrorException>()).And;
            var expectedError = Errors.Automation.Provider.Entity.AttachmentNotSupported(type.ToUpperFirstChar(), null);

            exception.Error.Title.Should().Be(expectedError.Title);
            exception.Error.Message.Should().Be(expectedError.Message);
            exception.Error.AdditionalDetails.Should().Contain($"Entity Type: {type}");
            exception.Error.AdditionalDetails.Should().Contain($"Entity ID: {entityId}");
            exception.Error.AdditionalDetails.Should().Contain($"Output Filename: brown_fox.txt");
            exception.Error.AdditionalDetails.Should().Contain($"Filename: brown_fox.txt");
        }

        [Theory]
        [InlineData("quote")]
        [InlineData("quoteVersion")]
        [InlineData("policy")]
        [InlineData("policyTransaction")]
        [InlineData("claim")]
        [InlineData("claimVersion")]
        public async Task EntityFileProvider_Throw_When_File_Not_Found_In_Entity(string type)
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
					            ""outputFilename"": ""brown_fox.txt"",
                                ""filename"": ""brown_fox.txt"",
								""entity"" : {
                                        ""dynamicEntity"": {
                                                ""entityType"": """ + type + @""",
                                                ""entityId"": """ + entityId.ToString() + @"""
                                         }
                                 }
                        }";

            var entityFileProviderBuilder = JsonConvert.DeserializeObject<EntityFileProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = MockAutomationData.GetServiceProviderForEntityProviders(entityId, false);
            var entityFileProvider = entityFileProviderBuilder.Build(mockServiceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            Func<Task> func = async () => await entityFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            var expectedError = Errors.Automation.Provider.FileNotFound(null);

            exception.Which.Error.Title.Should().Be(expectedError.Title);
            exception.Which.Error.Message.Should().Be(expectedError.Message);
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity Type: {type}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Entity ID: {entityId}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Output Filename: brown_fox.txt");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Filename: brown_fox.txt");
        }
    }
}
