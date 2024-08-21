// <copyright file="JsonPathTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Json
{
    using System.Linq;
    using UBind.Domain.Json;
    using Xunit;

    /// <summary>
    /// Tests for JSON path.
    /// </summary>
    public class JsonPathTests
    {
        /// <summary>
        /// Test when parameter is an invalid json path.
        /// </summary>
        /// <param name="path">The JSON path.</param>
        [Theory]
        [InlineData("foo bar")]
        [InlineData("[]")]
        [InlineData("[ ]")]
        [InlineData(".")]
        [InlineData("..")]
        [InlineData("[")]
        public void Constructor_Throws_WhenParameterIsInvalidJsonPath(string path)
        {
            // Act + Assert
            var exception = Assert.Throws<JsonPathFormatException>(
                () => new JsonPath(path));
        }

        /// <summary>
        /// Test when parameter is a valid json path.
        /// </summary>
        /// <param name="path">The JSON path.</param>
        [Theory]
        [InlineData("")]
        [InlineData("foo")]
        [InlineData("foo.bar")]
        public void Constructor_Succeeds_WhenParameterIsValidJsonPath(string path)
        {
            // Act
            var sut = new JsonPath(path);

            // Assert
            Assert.NotNull(sut);
        }

        /// <summary>
        /// Test whether the function can identify ancestors for simple paths.
        /// </summary>
        /// <param name="path">The JSON path.</param>
        [Theory]
        [InlineData("foo")]
        [InlineData("foo.bar")]
        [InlineData("foo.bar.baz")]
        [InlineData("1.2.3")]
        public void CanIdentifyAncestors_ReturnsTrue_ForSimplePaths(string path)
        {
            // Arrange
            var sut = new JsonPath(path);

            // Act
            var result = sut.CanIdentifyAncestors;

            // Assert
            Assert.True(sut.CanIdentifyAncestors);
        }

        /// <summary>
        /// Test whether the function returns expected ancestors for simple paths.
        /// </summary>
        /// <param name="path">The JSON path.</param>
        /// <param name="expectedAncestorList">The expected ancestor path list.</param>
        [Theory]
        [InlineData("foo.bar", "foo")]
        [InlineData("foo.bar.baz", "foo,bar")]
        [InlineData("1.2.3", "1,2")]
        public void Ancestors_ReturnsExpectedAncestors_ForSimplePaths(string path, string expectedAncestorList)
        {
            // Arrange
            var expectedAncestors = expectedAncestorList.Split(',');
            var sut = new JsonPath(path);

            // Act
            var ancestors = sut.Ancestors;

            // Assert
            var index = 0;
            foreach (var expectedAncestor in expectedAncestors)
            {
                Assert.Equal(expectedAncestor, ancestors.Skip(index).First());
                ++index;
            }
        }

        /// <summary>
        /// Test whether it returns an empty collection when path is a single property name.
        /// </summary>
        [Fact]
        public void Ancestors_ReturnsEmptyCollection_WhenPathIsSinglePropertyName()
        {
            // Arrange
            var sut = new JsonPath("foo");

            // Act
            var ancestors = sut.Ancestors;

            // Assert
            Assert.False(ancestors.Any());
        }

        /// <summary>
        /// Test whether the function can identify ancestors for unsupported paths.
        /// </summary>
        /// <param name="path">The JSON path.</param>
        [Theory]
        [InlineData("")]
        [InlineData("$")]
        [InlineData("$..author")]
        [InlineData("$.store.*")]
        [InlineData("$.store..price")]
        [InlineData("$.store.book[0].title")]
        public void CanIdentifyAncestors_ReturnsFalse_ForUnsupportedPaths(string path)
        {
            // Arrange
            var sut = new JsonPath(path);

            // Act
            var result = sut.CanIdentifyAncestors;

            // Assert
            Assert.False(result);
        }
    }
}
