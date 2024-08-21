// <copyright file="ResultAssertionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using System;
    using CSharpFunctionalExtensions;
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using Xunit;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// Defines the <see cref="ResultAssertionsTests" />.
    /// </summary>
    public class ResultAssertionsTests
    {
        [Fact]
        public void Succeed_assertion_throws_when_result_fails()
        {
            // Arrange
            Error error = new Error("test.error.code", "Test error title", "Test error message");

            // Act
            var failedResult = Result.Failure<Void, Error>(error);

            // Assert
            failedResult.Invoking(result => result.Should().Succeed())
                .Should().Throw<Exception>()
                .WithMessage("Test error title: Test error message. Code: test.error.code.");
        }

        [Fact]
        public void Succeed_assertion_passes_through_when_result_succeeds()
        {
            // Arrange

            // Act
            var successfulResult = Result.Success<Void, Error>(default);

            // Assert
            successfulResult.Invoking(result => result.Should().Succeed())
                .Should().NotThrow();
        }

        [Fact]
        public void Fail_assertion_throws_when_result_succeeds()
        {
            // Arrange

            // Act
            var successfulResult = Result.Success<Void, Error>(default);

            // Assert
            successfulResult.Invoking(result => result.Should().Fail())
                .Should().Throw<Exception>();
        }

        [Fact]
        public void Fail_assertion_passes_through_when_result_fails()
        {
            // Arrange
            Error error = new Error("test.error.code", "Test error title", "Test error message");

            // Act
            var failedResult = Result.Failure<Void, Error>(error);

            // Assert
            failedResult.Invoking(result => result.Should().Fail())
                .Should().NotThrow();
        }
    }
}
