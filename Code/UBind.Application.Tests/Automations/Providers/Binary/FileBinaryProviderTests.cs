// <copyright file="FileBinaryProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Binary
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Binary;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class FileBinaryProviderTests
    {
        /// <summary>
        /// Unit test for file binary provider.
        /// </summary>
        /// <returns>Returms.</returns>
        [Fact]
        public async Task FileBinaryProvider_Should_Return_Content_Of_Generated_File()
        {
            // Arrange
            var json = @"{
					        ""textFile"": {
                                ""outputFilename"": ""testing.txt"",
						        ""sourceData"": ""If you see this file in the quote documents after paying the quote, attach file to entity provider is working.""
                            }
                        }";

            var fileBinaryProviderBuilder = JsonConvert.DeserializeObject<FileBinaryProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var fileBinaryProvider = fileBinaryProviderBuilder.Build(null);

            // Act
            var content = (await fileBinaryProvider.Resolve(null)).GetValueOrThrowIfFailed();

            // Assert
            content.DataValue.Should().NotBeEmpty();
            var contentAsString = System.Text.Encoding.ASCII.GetString(content.DataValue);
            contentAsString.Should().Be("If you see this file in the quote documents after paying the quote, attach file to entity provider is working.");
        }

        [Fact]
        public async Task FileBinaryProvider_Should_Return_Content_Of_File_From_Entity()
        {
            // Arrange
            var entityId = Guid.NewGuid();
            var json = @"{
                            ""entityFile"": {
					            ""outputFilename"": ""brown_fox.txt"",
                                ""filename"": ""brown_fox.txt"",
								""entity"" : {
                                        ""dynamicEntity"": {
                                                ""entityType"": ""quote"",
                                                ""entityId"": """ + entityId.ToString() + @"""
                                         }
                                 }
                            }
                        }";

            var fileBinaryProviderBuilder = JsonConvert.DeserializeObject<FileBinaryProviderConfigModel>(json, AutomationDeserializationConfiguration.ModelSettings);
            var mockServiceProvider = MockAutomationData.GetServiceProviderForEntityProviders(entityId, true);
            var fileBinaryProvider = fileBinaryProviderBuilder.Build(mockServiceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var content = (await fileBinaryProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            content.DataValue.Should().NotBeEmpty();
            var fileContent = Encoding.UTF8.GetString(content.DataValue);
            fileContent.Should().Be("This is a test file");
        }
    }
}
