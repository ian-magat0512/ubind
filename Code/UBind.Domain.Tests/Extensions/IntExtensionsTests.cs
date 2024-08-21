// <copyright file="IntExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using UBind.Domain.Extensions;
    using Xunit;

    public class IntExtensionsTests
    {
        [Theory]
        [InlineData(0, "A")]
        [InlineData(1, "B")]
        [InlineData(2, "C")]
        [InlineData(25, "Z")]
        [InlineData(26, "BA")]
        [InlineData(27, "BB")]
        [InlineData(51, "BZ")]
        [InlineData(52, "CA")]
        [InlineData(53, "CB")]
        [InlineData(308915773, "ZZZZZX")]
        [InlineData(308915774, "ZZZZZY")]
        [InlineData(308915775, "ZZZZZZ")]
        public void ToBase26_ReturnsCorrectOutput(int input, string expectedOutput)
        {
            Assert.Equal(expectedOutput, input.ToBase26());
        }
    }
}
