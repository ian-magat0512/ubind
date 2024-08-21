// <copyright file="RawJsonConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using UBind.Domain.JsonConverters;
    using Xunit;

    public class RawJsonConverterTests
    {
        [Fact]
        public void Deserialize_DoesNotQuoteJson()
        {
            // Arrange
            var input = new TestObject
            {
                Json = @"{""foo"": 1, ""Bar"": ""X""}",
            };
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
            };
            var expectedOutputRegexp = @"json""\s*:\s*\{";

            // Act
            var output = JsonConvert.SerializeObject(input, settings);

            // Assert
            Assert.True(new Regex(expectedOutputRegexp).Match(output).Success);
        }

        private class TestObject
        {
            [JsonConverter(typeof(RawJsonConverter))]
            public string Json { get; set; }
        }
    }
}
