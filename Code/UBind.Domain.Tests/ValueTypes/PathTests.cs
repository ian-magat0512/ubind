// <copyright file="PathTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.ValueTypes
{
    using FluentAssertions;
    using UBind.Domain.ValueTypes;
    using Xunit;

    public class PathTests
    {
        [Fact]
        public void GetParent_ReturnsParent_WhenItHasATrailingDelimter()
        {
            // Arrange
            string testPath = "a/b/c/";

            // Act
            string result = new Path(testPath).GetParent();

            // Assert
            result.Should().Be("a/b/");
        }

        [Fact]
        public void GetParentFolderPath_ReturnsParent_WhenItDoesNotHaveATrailingDelimter()
        {
            // Arrange
            string testPath = "a/b/c";

            // Act
            string result = new Path(testPath).GetParent();

            // Assert
            result.Should().Be("a/b/");
        }

        [Fact]
        public void GetParentFolderPath_ReturnsEmptyString_WhenThereIsNoParent()
        {
            // Arrange
            string testPath = "a";

            // Act
            string result = new Path(testPath).GetParent();

            // Assert
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void GetLastSegment_GetLastSegment_WhenItHasATrailingDelimiter()
        {
            // Arrange
            string testPath = "a/b/c/";

            // Act
            string result = new Path(testPath).GetLastSegment();

            // Assert
            result.Should().Be("c");
        }

        [Fact]
        public void GetLastSegment_ReturnsEmptyString_WhenPathIsEmpty()
        {
            // Arrange
            string testPath = string.Empty;

            // Act
            string result = new Path(testPath).GetLastSegment();

            // Assert
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void GetLastSegmentName_GetLastSegmentName_WhenItDoesNotHaveATrailingDelimiter()
        {
            // Arrange
            string testPath = "a/b/c";

            // Act
            string result = new Path(testPath).GetLastSegment();

            // Assert
            result.Should().Be("c");
        }

        [Fact]
        public void Join_JoinsTwoPathsCorrectly_WhenFirstPartContainsTrailingDelimiter()
        {
            // Arrange
            string testPath = "a/b/c/";

            // Act
            string result = new Path(testPath).Join("anotherSegment");

            // Assert
            result.Should().Be("a/b/c/anotherSegment");
        }

        [Fact]
        public void Join_JoinsTwoPathsCorrectly_WhenFirstPartDoesNotContainTrailingDelimiter()
        {
            // Arrange
            string testPath = "a/b/c";

            // Act
            string result = new Path(testPath).Join("anotherSegment");

            // Assert
            result.Should().Be("a/b/c/anotherSegment");
        }

        [Fact]
        public void Join_JoinsTwoPathsCorrectly_WhenSecondPartContainsLeadingDelimiter()
        {
            // Arrange
            string testPath = "a/b/c";

            // Act
            string result = new Path(testPath).Join("/anotherSegment");

            // Assert
            result.Should().Be("a/b/c/anotherSegment");
        }

        [Fact]
        public void Join_JoinsTwoPathsCorrectly_WhenBothPartsContainsJoiningDelimiters()
        {
            // Arrange
            string testPath = "a/b/c/";

            // Act
            string result = new Path(testPath).Join("/anotherSegment");

            // Assert
            result.Should().Be("a/b/c/anotherSegment");
        }

        [Fact]
        public void Join_JoinsTwoPathsCorrectly_WhenOnePathStringIsNotNormalised()
        {
            // Arrange
            string testPath = "a/b/c/";

            // Act
            string result = new Path(testPath).Join("some\\other\\segments");

            // Assert
            result.Should().Be("a/b/c/some/other/segments");
        }

        [Fact]
        public void Join_JoinsTwoPathsCorrectly_WhenBothAreEmpty()
        {
            // Arrange
            string testPath = string.Empty;

            // Act
            string result = new Path(testPath).Join(string.Empty);

            // Assert
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void Join_JoinsTwoPathsCorrectly_WhenFirstIsEmpty()
        {
            // Arrange
            string testPath = string.Empty;

            // Act
            string result = new Path(testPath).Join("something");

            // Assert
            result.Should().Be("something");
        }

        [Fact]
        public void Join_JoinsTwoPathsCorrectly_WhenSecondIsEmpty()
        {
            // Arrange
            string testPath = "something";

            // Act
            string result = new Path(testPath).Join(string.Empty);

            // Assert
            result.Should().Be("something");
        }

        [Fact]
        public void Constructor_NormalisesFileSystemPaths_ByDefault()
        {
            // Arrange
            string testPath = "a\\b/c/";

            // Act
            string result = new Path(testPath);

            // Assert
            result.Should().Be("a/b/c/");
        }
    }
}
