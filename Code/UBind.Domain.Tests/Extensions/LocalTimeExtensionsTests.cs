// <copyright file="LocalTimeExtensionsTests.cs" company="uBind">
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

    public class LocalTimeExtensionsTests
    {
        [Fact]
        public void ToIso8601_FormatsTimeCorrect()
        {
            // Arrange
            var localTime = new LocalTime(12, 34, 56, 789);

            // Act
            var output = localTime.ToIso8601();

            var @short = localTime.ToString();

            // Assert
            output.Should().Be("12:34:56.789");
        }
    }
}
