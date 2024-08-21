// <copyright file="SequenceObfuscatorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Services
{
    using System.Collections.Generic;
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using UBind.Domain.NumberGenerators;
    using Xunit;

    public class SequenceObfuscatorTests
    {
        [Theory]
        [InlineData(10, 3)]
        [InlineData(10, 7)]
        [InlineData(10, 6)]
        [InlineData(10000, 339)]
        public void ObfuscateMethod_DoesNotRepeatNumbers_WithinGivenRange(int maxNumber, int coprime)
        {
            // Arrange
            var sut = new SequenceObfuscator(maxNumber, coprime);
            var results = new HashSet<long>();

            // Act
            for (var i = 0; i <= maxNumber; ++i)
            {
                var output = sut.Obfuscate(i);
                var isElementAdded = results.Add(output);

                // Assert
                isElementAdded.Should().BeTrue();
            }
        }

        [Fact]
        public void SixDigitSequenceObfuscator_DoesNotRepeatSequences_ForFull1MNumberRange()
        {
            // Arrange
            var sut = SequenceObfuscator.SixDigitSequenceObfuscator;
            var results = new HashSet<long>();

            // Act
            for (var i = 0; i < 1000000; ++i)
            {
                var output = sut.Obfuscate(i);
                var isElementAdded = results.Add(output);

                // Assert
                isElementAdded.Should().BeTrue();
                output.Should().BeGreaterThan(-1);
            }
        }

        [Fact]
        public void SixLetterSequenceObfuscator_DoesNotRepeatSequences_ForFull1MNumberRange()
        {
            // Arrange
            var sut = SequenceObfuscator.SixLetterSequenceObfuscator;
            var results = new HashSet<long>();

            // Act
            for (var i = 0; i < 1000000; ++i)
            {
                var output = sut.Obfuscate(i);
                var isElementAdded = results.Add(output);

                // Assert
                isElementAdded.Should().BeTrue();
                output.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public void TenDigitSequenceObfuscator_DoesNotRepeatSequences_ForFull1MNumberRange()
        {
            // Arrange
            var sut = SequenceObfuscator.TenDigitSequenceObfuscator;
            var results = new HashSet<long>();

            // Act
            for (var i = 0; i < 1000000; ++i)
            {
                var output = sut.Obfuscate(i);
                var isElementAdded = results.Add(output);

                // Assert
                isElementAdded.Should().BeTrue();
                output.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public void SixLetterSequenceObfuscator_ResultShouldBeLessThan_MaxNumber()
        {
            // Arrange
            var sut = SequenceObfuscator.SixLetterSequenceObfuscator;
            var maxNumberForSixLetterSequence = 308915775; // 308915774 is equivalent to ZZZZZZ in base 26, using A-Z
            var results = new HashSet<long>();

            // Act
            for (var i = 0; i < 1000000; ++i)
            {
                var output = sut.Obfuscate(i);
                var isElementAdded = results.Add(output);

                // Assert
                isElementAdded.Should().BeTrue();
                output.Should().BeLessThan(maxNumberForSixLetterSequence);
            }
        }

        [Fact]
        public void EightAlphanumericSequenceObfuscator_DoesNotRepeatSequences_ForFull1MNumberRange()
        {
            // Arrange
            var sut = SequenceObfuscator.EightAlphaNumericSequenceObfuscator;
            var results = new HashSet<long>();
            long maxNumberForEightAlphaNumericSequence = 2821109907455; // 36^8-1, using A-Z, 0-9, base 36, 8 characters

            // Act
            for (long i = 0; i < 1000000; ++i)
            {
                var output = sut.Obfuscate(i);
                var isElementAdded = results.Add(output);

                // Assert
                isElementAdded.Should().BeTrue();
                output.Should().BeGreaterThan(0);
                output.Should().BeLessThan(maxNumberForEightAlphaNumericSequence);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(11111)]
        [InlineData(2821109907455)]
        [InlineData(2821109907456)]
        public void EightAlphanumericSequenceObfuscator_Cycles_WhenNumberIsGreaterThanMaximum(long input)
        {
            // Arrange
            var sut = SequenceObfuscator.EightAlphaNumericSequenceObfuscator;
            long setSize = 2821109907456; // base 36, 36^8, using A-Z, 0-9
            long previousOutput = sut.Obfuscate(input);

            // Act
            long output = sut.Obfuscate(setSize + input);

            // Assert
            output.Should().Be(previousOutput);
        }
    }
}
