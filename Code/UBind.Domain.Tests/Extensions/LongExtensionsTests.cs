// <copyright file="LongExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using Xunit;

    public class LongExtensionsTests
    {
        [Theory]
        [InlineData(0, "0")]
        [InlineData(10, "A")]
        [InlineData(35, "Z")]
        [InlineData(308915775, "53X4XR")]
        [InlineData(2821109907455, "ZZZZZZZZ")]
        public void ToBase36_ReturnsCorrectOutput(long input, string expectedOutput)
        {
            input.ToBase36().Should().Be(expectedOutput);
        }
    }
}
