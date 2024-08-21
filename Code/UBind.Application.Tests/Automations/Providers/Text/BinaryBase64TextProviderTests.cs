// <copyright file="BinaryBase64TextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Domain.Extensions;
    using Xunit;

    public class BinaryBase64TextProviderTests
    {
        [Fact]
        public async Task BinaryBase64TextProvider_Should_Return_TextValue_In_Base64_Format()
        {
            // Arrange
            var json = @"{
                            ""fileBinary"": {
                                ""textFile"": {
                                    ""outputFilename"": ""testing.txt"",
						            ""sourceData"": ""If you see this file in the quote documents after paying the quote, attach file to entity provider is working.""
                                }
                            }
                        }";

            var binaryBase64TextProviderBuilder = JsonConvert.DeserializeObject<BinaryBase64TextProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var binaryBase64TextProvider = binaryBase64TextProviderBuilder.Build(null);

            // Act
            var content = (await binaryBase64TextProvider.Resolve(null)).GetValueOrThrowIfFailed();

            // Assert
            content.DataValue.Should().NotBeEmpty();
            var data = Convert.FromBase64String(content);
            var contentAsString = Encoding.ASCII.GetString(data);
            contentAsString.Should().Be("If you see this file in the quote documents after paying the quote, attach file to entity provider is working.");
        }
    }
}
