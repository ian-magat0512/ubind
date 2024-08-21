// <copyright file="Base64TextBinaryProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Binary
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Binary;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class Base64TextBinaryProviderTests
    {
        /// <summary>
        /// Unit test for file binary provider.
        /// </summary>
        [Fact]
        public async void Base64TextBinaryProvider_Should_Convert_Base64String_To_Binary()
        {
            // Arrange
            var json = @"{
					        ""binaryBase64Text"": {
                                    ""fileBinary"": {
                                         ""textFile"": {
                                          ""outputFilename"": ""testing.txt"",
						                  ""sourceData"": ""If you see this file in the quote documents after paying the quote, attach file to entity provider is working.""
                                          }
                                    }
                                }
                        }";

            var base64TextBinaryProviderBuilder = JsonConvert.DeserializeObject<Base64TextBinaryProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var base64TextBinaryProvider = base64TextBinaryProviderBuilder.Build(null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var content = (await base64TextBinaryProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            content.DataValue.Should().NotBeEmpty();
            var contentAsString = System.Text.Encoding.ASCII.GetString(content.DataValue);
            contentAsString.Should().Be("If you see this file in the quote documents after paying the quote, attach file to entity provider is working.");
        }

        [Fact]
        public async Task Base64TextBinaryProvider_Should_Throw_When_String_Not_In_Base64()
        {
            // Arrange
            var textProvider = new StaticBuilder<Data<string>>() { Value = "not valid base64 string because it contains a $ dollar sign" };
            var base64TextBinaryProviderBuilder = new Base64TextBinaryProviderConfigModel() { TextProvider = textProvider };
            var base64TextBinaryProvider = base64TextBinaryProviderBuilder.Build(null);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await base64TextBinaryProvider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.provider.invalid.input.data");
        }
    }
}
