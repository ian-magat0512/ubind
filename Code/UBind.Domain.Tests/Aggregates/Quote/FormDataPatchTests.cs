// <copyright file="FormDataPatchTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Quote
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Json;
    using Xunit;

    /// <summary>
    /// Tests for form data patch.
    /// </summary>
    public class FormDataPatchTests
    {
        /// <summary>
        /// Tests form data patch when the value is a string.
        /// </summary>
        [Fact]
        public void FormDataPatch_RoundTripsSuccessfully_WhenValueIsString()
        {
            // Arrange
            var sut = new PolicyDataPatch(
                DataPatchType.FormData,
                new JsonPath("foo.bar"),
                "myValue",
                new List<QuotDataPatchTarget>());

            // Act
            var json = JsonConvert.SerializeObject(sut);
            var result = JsonConvert.DeserializeObject<PolicyDataPatch>(json);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Value.HasValues);
        }

        /// <summary>
        /// Tests form data patch when the value is an object.
        /// </summary>
        [Fact]
        public void FormDataPatch_RoundTripsSuccessfully_WhenValueIsObject()
        {
            // Arrange
            var sut = new PolicyDataPatch(
                DataPatchType.FormData,
                new JsonPath("foo.bar"),
                JToken.Parse(@"{ ""foo"" : ""bar"" }"),
                new List<QuotDataPatchTarget>());

            // Act
            var json = JsonConvert.SerializeObject(sut);
            var result = JsonConvert.DeserializeObject<PolicyDataPatch>(json);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Value.HasValues);
        }
    }
}
