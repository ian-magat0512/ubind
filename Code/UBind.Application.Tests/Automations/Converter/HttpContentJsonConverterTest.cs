// <copyright file="HttpContentJsonConverterTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Converter
{
    using System.Collections.Generic;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Http;
    using Xunit;

    public class HttpContentJsonConverterTest
    {
        [Theory]
        [InlineData("{ 'hello': 'world' }")]
        [InlineData("[ { 'hello': 'world' }, { 'sample': 'test' }]")]
        public void HttpContentJsonConverter_ConvertsJsonContentOfAnyType_ShouldReturnValidJson(string stringyfiedContent)
        {
            // Arrange
            JToken content = JToken.Parse(stringyfiedContent);
            var request = new Request(
                "http://www.test.com",
                System.Net.Http.HttpMethod.Get.ToString(),
                new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(),
                "application/json",
                string.Empty,
                content);

            // Act
            var serializedData = JsonConvert.SerializeObject(request);

            // Assert
            var deserializedData = JObject.Parse(serializedData);
            deserializedData.Should().NotBeNull();
            deserializedData["content"].Should().NotBeNull();
            deserializedData["content"].Value<string>().Should().Be(content.ToString());
        }
    }
}
