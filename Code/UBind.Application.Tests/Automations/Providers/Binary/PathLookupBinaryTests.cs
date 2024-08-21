// <copyright file="PathLookupBinaryTests.cs" company="uBind">
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
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Binary;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class PathLookupBinaryTests
    {
        [Fact]
        public async Task PathLookupBinary_Returns_ByteArray_WhenFound()
        {
            // Arrange
            var json = @"{
                            ""path"": ""/actions/testHttpRequestAction/httpResponse/content""
                        }";

            var pathLookupBinaryProviderBuilder = JsonConvert.DeserializeObject<PathLookupBinaryProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var provider = pathLookupBinaryProviderBuilder.Build(null);
            var testString = "This is some test binary data";
            byte[] data = Encoding.UTF8.GetBytes(testString);
            AutomationData automationData = await MockAutomationData.CreateWithHttpActionBinaryData(data, "testHttpRequestAction");

            // Act
            var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.DataValue.Should().NotBeNull();
            Encoding.UTF8.GetString(result.DataValue).Should().Be(testString);
        }

        [Fact]
        public async Task PathLookupBinary_Returns_DefaultByteArray_WhenNotFound()
        {
            // Arrange
            var json = @"{
                            ""path"": ""/actions/aBogusHttpAction/httpResponse/content"",                        
                            ""valueIfNotFound"": {
                                ""objectPathLookupBinary"": ""/actions/testHttpRequestAction/httpResponse/content""
                            }
                        }";

            var pathLookupBinaryProviderBuilder = JsonConvert.DeserializeObject<PathLookupBinaryProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var provider = pathLookupBinaryProviderBuilder.Build(null);
            var testString = "This is some test binary data";
            byte[] data = Encoding.UTF8.GetBytes(testString);
            AutomationData automationData = await MockAutomationData.CreateWithHttpActionBinaryData(data, "testHttpRequestAction");

            // Act
            var result = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            result.DataValue.Should().NotBeNull();
            Encoding.UTF8.GetString(result).Should().Be(testString);
        }

        [Fact]
        public async Task PathLookupBinary_Throws_WhenTypeIsNotByteArray()
        {
            // Arrange
            var json = @"{
                            ""path"": ""/actions/aBogusHttpAction/httpResponse/content"",                        
                            ""valueIfNotFound"": {
                                ""objectPathLookupBinary"": ""/actions/testHttpRequestAction/httpResponse/content""
                            }
                        }";

            var pathLookupBinaryProviderBuilder = JsonConvert.DeserializeObject<PathLookupBinaryProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var provider = pathLookupBinaryProviderBuilder.Build(null);
            var testString = "This is some test binary data";
            AutomationData automationData = await MockAutomationData.CreateWithHttpActionStringData(testString, "testHttpRequestAction");

            // Act
            Func<Task> func = async () => await provider.Resolve(new ProviderContext(automationData));

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.providers.invalid.value.type.obtained");
        }
    }
}
