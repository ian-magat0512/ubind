// <copyright file="JTokenExtensionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;
    using Xunit;

    /// <summary>
    /// Tests for JSON token extension.
    /// </summary>
    public class JTokenExtensionTests
    {
        /// <summary>
        /// Tests when token holds non empty data.
        /// </summary>
        /// <param name="json">The JSON payload.</param>
        [Theory]
        [InlineData(@"{ ""foo"": ""bar"" }")] // Non-empty string values are not empty.
        [InlineData(@"{ ""foo"": { ""bar"": ""baz"" } }")] // Non-empty objects are not empty.")]
        [InlineData(@"{ ""foo"": [ ""bar"" ] }")] // Non-empty arrays are not empty.")]
        [InlineData(@"{ ""foo"": [ """" ] }")] // Non-empty arrays are not empty, even when only item is empty string.")]
        public void IsNullOrEmpty_ReturnsFalse_WhenTokenHoldsNonEmptyData(string json)
        {
            // Arrange
            var jObject = JObject.Parse(json);
            var jtoken = jObject.SelectToken("foo");

            // Act
            var result = jtoken.IsNullOrEmpty();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests when token does not hold non empty data.
        /// </summary>
        /// <param name="json">The JSON payload.</param>
        [Theory]
        [InlineData(@"{ ""foo"": """" }")] // Empty string values are empty.")]
        [InlineData(@"{ ""foo"": { } }")] // Empty objects are empty.")]
        [InlineData(@"{ ""foo"": [ ] }")] // Empty arrays are empty.")]
        [InlineData(@"{ ""foo"": null }")] // Null values are null.")]
        public void IsNullOrEmpty_ReturnsTrue_WhenTokenDoesNotHoldNonEmptyData(string json)
        {
            // Arrange
            var jObject = JObject.Parse(json);
            var jtoken = jObject.SelectToken("foo");

            // Act
            var result = jtoken.IsNullOrEmpty();

            // Assert
            Assert.True(result);
        }
    }
}
