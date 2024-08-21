// <copyright file="LocalDateExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Extensions;
    using Xunit;

    public class LocalDateExtensionsTests
    {
        [Fact]
        public void ToIso8601_FormatsCorrect()
        {
            // Arrange
            var localDate = new LocalDate(2020, 12, 25);

            // Act
            var output = localDate.ToIso8601();

            // Assert
            Assert.Equal("2020-12-25", output);
        }

        [Fact]
        public void SecondsToDays_Calculate_CorrectDays()
        {
            // Arrange
            var threedaysInSeconds = 259200;

            // Act
            var output = LocalDateExtensions.SecondsToDays(threedaysInSeconds);

            // Assert
            output.Should().Be(3);
        }
    }
}
