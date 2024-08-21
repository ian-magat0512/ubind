// <copyright file="TextFileProviderTests.cs" company="uBind">
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
    using UBind.Domain.Exceptions;
    using Xunit;

    public class TextFileProviderTests
    {
        /// <summary>
        /// Unit test for text file provider for generating text file.
        /// </summary>
        /// <returns>Return.</returns>
        [Fact]
        public async Task TextFileProvider_Should_Return_FileInfo()
        {
            // Arrange
            var json = @"{
                            ""sourceData"": ""The quick brown fox jumps over the lazy dog."",
							""outputFileName"": ""brownfox.txt""
                         }";

            var textFileProviderBuilder = JsonConvert.DeserializeObject<TextFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var textFileProvider = textFileProviderBuilder.Build(null);

            // Act
            var fileInfo = (await textFileProvider.Resolve(null)).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.DataValue.FileName.ToString().Should().Be("brownfox.txt");

            var content = Encoding.UTF8.GetString(fileInfo.DataValue.Content);
            content.Should().Be("The quick brown fox jumps over the lazy dog.");
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
        public async Task TextFileProvider_Throw_When_FileName_Is_InValid(string fileName, string expectedErrorMessage)
        {
            // Arrange
            // Some special characters are not allowed in json string like \ ' { }
            // We will test only the special characters that allowed in json but not allowed in file name.
            var json = @"{
                                ""sourceData"": ""The quick brown fox jumps over the lazy dog."",
						        ""outputFileName"": """ + fileName + @"""
                         }";
            var textFileProviderBuilder = JsonConvert.DeserializeObject<TextFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var textFileProvider = textFileProviderBuilder.Build(null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var providerContext = new ProviderContext(automationData);

            // Act
            Func<Task> func = async () => await textFileProvider.Resolve(providerContext);

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Text file must have a valid filename");

            exception.Which.Error.AdditionalDetails.Should().Contain($"Output Filename: {fileName}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Invalid Reason: {expectedErrorMessage}");
        }
    }
}
