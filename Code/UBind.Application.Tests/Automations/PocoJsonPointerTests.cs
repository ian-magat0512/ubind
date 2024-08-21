// <copyright file="PocoJsonPointerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain.Exceptions;
    using Xunit;

    public class PocoJsonPointerTests
    {
        [Theory]
        [InlineData("0", "/foo/1", "/foo/1")]
        [InlineData("1/0", "/foo/1", "/foo/0")]
        [InlineData("2/highly/nested/objects", "/foo/1", "/highly/nested/objects")]
        [InlineData("0/objects", "/highly/nested", "/highly/nested/objects")]
        [InlineData("1/nested/objects", "/highly/nested", "/highly/nested/objects")]
        [InlineData("2/foo/0", "/highly/nested", "/foo/0")]
        public void PocoJsonPointer_ConvertsRelativeJsonPointerToAbsolute(string relative, string context, string absolute)
        {
            // Arrange
            var relativePointer = new PocoJsonPointer(relative, "pocoJsonPointer");
            var contextPointer = new PocoJsonPointer(context, "pocoJsonPointer");

            // Act
            var absolutePointer = relativePointer.ToAbsolute(contextPointer);

            // Assert
            absolutePointer.ToString().Should().Be(absolute);
        }

        [Fact]
        public void PocoJsonPointer_EvaluatesRelativeJsonPointer_WithHashSyntaxForArrayIndex()
        {
            // Arrange
            string json = @"
                {
                    ""foo"": [""bar"", ""baz""],
                    ""highly"": {
                        ""nested"": {
                            ""objects"": true
                        }
                    }
                }";
            var jObject = JObject.Parse(json);
            var contextJsonPointer = new PocoJsonPointer("/foo/1", "pocoJsonPointer"); // "baz"
            var relativeJsonPointer = new PocoJsonPointer("0#", "pocoJsonPointer");

            // Act
            var result = relativeJsonPointer.Evaluate(jObject, contextJsonPointer).GetValueOrThrowIfFailed();

            // Assert
            result.Should().Be(1);
        }

        [Fact]
        public void PocoJsonPointer_EvaluatesRelativeJsonPointer_WithHashSyntaxForPropertyName()
        {
            // Arrange
            string json = @"
                {
                    ""foo"": [""bar"", ""baz""],
                    ""highly"": {
                        ""nested"": {
                            ""objects"": true
                        }
                    }
                }";
            var jObject = JObject.Parse(json);
            var contextJsonPointer = new PocoJsonPointer("/foo/1", "pocoJsonPointer"); // "baz"
            var relativeJsonPointer = new PocoJsonPointer("1#", "pocoJsonPointer");

            // Act
            var result = relativeJsonPointer.Evaluate(jObject, contextJsonPointer).GetValueOrThrowIfFailed();

            // Assert
            result.Should().Be("foo");
        }

        [Fact]
        public void PocoJsonPointer_EvaluateJsonPointer_ShouldThrowExceptionIfObjectIsNullAndPathIsNotRoot()
        {
            string json = @"
                    {
                        ""foo"": null
                    }";
            var jObject = JObject.Parse(json);
            var jsonPointer = new PocoJsonPointer("/foo/bar/baz", "pocoJsonPointer");

            // Act with Retry
            var ex = Assert.Throws<ErrorException>(() => jsonPointer.Evaluate(jObject).GetValueOrThrowIfFailed());
            ex = Assert.Throws<ErrorException>(() => jsonPointer.Evaluate(jObject).GetValueOrThrowIfFailed());

            // Assert
            ex.Error.Code.Should().Be("automation.providers.path.not.found");
        }

        [Fact]
        public void PocoJsonPointer_EvaluateJsonPointer_ShouldReturnEmptyObject()
        {
            string json = @"
                {
                    ""foo"": {}
                }";

            var jObject = JObject.Parse(json);
            var jsonPointer = new PocoJsonPointer("/foo/");

            // Act
            var result = jsonPointer.Evaluate(jObject).GetValueOrThrowIfFailed();

            // Assert
            result.Should().NotBeNull();
            var resultToken = result as JToken;
            resultToken.Type.Should().Be(JTokenType.Object);
        }
    }
}
