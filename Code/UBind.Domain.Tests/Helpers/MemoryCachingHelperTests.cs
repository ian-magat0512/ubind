// <copyright file="MemoryCachingHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using System;
    using System.Threading;
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using Xunit;

    /// <summary>
    /// Tests the MemorrCachingHelper.
    /// </summary>
    public class MemoryCachingHelperTests
    {
        private readonly DateTimeOffset cacheDuration = DateTimeOffset.Now.AddSeconds(1);

        [Theory]
        [InlineData("test", "zz")]
        [InlineData("test2", "zz2")]
        [InlineData("test3", "zz3")]
        public void AddOrGet_CorrectData_IfUsedAtDifferentTimings(string key, string value)
        {
            // Arange

            // Act
            var result = MemoryCachingHelper.AddOrGet<string>(
                key,
                () =>
                {
                    return value;
                },
                this.cacheDuration);

            // Assert
            result.Should().Be(value);

            // Act
            var result2 = MemoryCachingHelper.AddOrGet<string>(
                key,
                () =>
                {
                    return "anothervalue";
                },
                this.cacheDuration);

            // Assert
            result2.Should().Be(value);

            Thread.Sleep(1010);

            // Act
            var result3 = MemoryCachingHelper.AddOrGet<string>(
                key,
                () =>
                {
                    return "anothervalue";
                },
                this.cacheDuration);

            // Assert
            result3.Should().Be("anothervalue");
        }
    }
}
