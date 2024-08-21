// <copyright file="StringExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using MoreLinq;
    using NodaTime;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using Xunit;

    /// <summary>
    /// String extensions unit tests.
    /// </summary>
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\t")]
        [InlineData(" \t")]
        public void ToNullIfWhitespace_ReturnsNull_WhenStringIsEmtpyOrWhitespace(string input)
        {
            // Act
            var result = input.ToNullIfWhitespace();

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(" a")]
        [InlineData("a ")]
        [InlineData("foo")]
        [InlineData("\ta\t")]
        [InlineData("\\")]
        public void ToNullIfWhitespace_ReturnsOriginalString_WhenStringIsNotEmtpyOrWhitespace(string input)
        {
            // Act
            var result = input.ToNullIfWhitespace();

            // Assert
            Assert.Equal(input, result);
        }

        [Fact(Skip = "We are no longer using DBJ2 hash.")]
        public void Dbj2Hash_ReturnsLessThanTwoClashes_Per100kGuidInputs()
        {
            for (int x = 0; x < 100; ++x)
            {
                // Arrange
                var inputs = MoreEnumerable.Sequence(1, 10000).Select(i => Guid.NewGuid().ToString());
                const int AcceptableCollisionsPer10kHashes = 1;

                // Act
                var hashes = inputs.Select(s => s.Dbj2Hash());

                // Assert
                var collisions = hashes.GroupBy(h => h).Where(g => g.Count() > 1).Count();
                Assert.True(collisions <= AcceptableCollisionsPer10kHashes);
            }
        }

        [Fact]
        public void ToLocalDateFromIso8601y_ParsesStringsCorrectly_WhenStringInCorrectFormatWithSlashes()
        {
            // Arrange
            var input = "2020-12-25";

            // Act
            var output = input.ToLocalDateFromIso8601();

            // Assert
            Assert.Equal(new LocalDate(2020, 12, 25), output);
        }

        [Fact]
        public void GetTicksAtEndOfDayInZone_ThrowsException_WhenStringIncorrectISO8601Format()
        {
            // Arrange
            var input = "2020/12/25";
            var dateTimezone = Timezones.AET;

            // Act + Assert
            Assert.Throws<ErrorException>(() => input.ToLocalDateFromIso8601());
        }

        [Fact]
        public void ToLocalDateFromIso8601y_ThrowsException_WhenStringInCorrectddMMyyyyWithSlashes()
        {
            // Arrange
            var input = "25/12/2020";

            // Act + Assert
            Assert.Throws<ErrorException>(() => input.ToLocalDateFromIso8601());
        }

        [Fact]
        public void ToLocalDateFromddMMyyyy_ParsesStringsCorrectly_WhenStringInCorrectFormatWithSlashes()
        {
            // Arrange
            var input = "25/12/2020";

            // Act
            var output = input.ToLocalDateFromddMMyyyy();

            // Assert
            Assert.Equal(new LocalDate(2020, 12, 25), output);
        }

        [Fact]
        public void ToLocalDateFromddMMyyyy_ThrowsException_WhenStringInIsoFormat()
        {
            // Arrange
            var input = "2020-12-25";

            // Act + Assert
            Assert.Throws<ErrorException>(() => input.ToLocalDateFromddMMyyyy());
        }

        [Fact]
        public void ToLocalDateFromIso8601OrddMMyyyy_ParsesStringsCorrectly_WhenStringInIso8601Format()
        {
            // Arrange
            var input = "2020-12-25";

            // Act
            var output = input.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy();

            // Assert
            Assert.Equal(new LocalDate(2020, 12, 25), output);
        }

        [Fact]
        public void ToLocalDateFromIso8601OrddMMyyyy_ParsesStringsCorrectly_WhenStringInddMMyyyyFormatWithSlashes()
        {
            // Arrange
            var input = "25/12/2020";

            // Act
            var output = input.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy();

            // Assert
            Assert.Equal(new LocalDate(2020, 12, 25), output);
        }

        [Fact]
        public void ToLocalDateFromIso8601OrddMMyyyy_Throws_WhenStringInWrongFormat()
        {
            // Arrange
            var input = "12/25/2020";

            // Act + Assert
            Assert.Throws<ErrorException>(() => input.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy());
        }

        [Fact]
        public void ToLocalDateFromIso8601OrddMMyyyy_ThrowsAndShowsFieldWithError_WhenStringInWrongFormat()
        {
            // Arrange
            var dateString = "12/25/2020";

            // Act
            Action act = () => dateString.ToLocalDateFromIso8601OrddMMyyyyOrddMMyy(nameof(dateString));

            // Assert
            var exception = Assert.Throws<ErrorException>(act);
            exception.Message.Should().Contain("Date 'dateString' not in any of expected formats");
        }

        [Fact]
        public void TryParseAsLocalDate_Succeeds_WhenDateInddMMyyyyFormatWithSlashes()
        {
            // Arrange
            var input = "25/12/2020";

            // Act
            var result = input.TryParseAsLocalDate();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new LocalDate(2020, 12, 25), result.Value);
        }

        [Fact]
        public void TryParseAsLocalDate_Succeeds_WhenDateInIso860Format()
        {
            // Arrange
            var input = "2020-12-25";

            // Act
            var result = input.TryParseAsLocalDate();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new LocalDate(2020, 12, 25), result.Value);
        }

        [Fact]
        public void TryParseAsLocalDate_Fails_WhenDateInMMddyyyyFormatWithSlashes()
        {
            // Arrange
            var input = "12/25/2020";

            // Act
            var result = input.TryParseAsLocalDate();

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void TryParseAsLocalDate_Fails_WhenDateInddMMyyyyFormatWithDashes()
        {
            // Arrange
            var input = "25-12-2020";

            // Act
            var result = input.TryParseAsLocalDate();

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void TryParseAsLocalDate_Fails_WhenDateInyyyyMMddFormatWithSlashes()
        {
            // Arrange
            var input = "2020/12/25";

            // Act
            var result = input.TryParseAsLocalDate();

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void GetJsonProperty_GetsProperty_WhenMatchingStringPropertyExists()
        {
            // Arrange
            var json = @"{""foo"": { ""bar"": ""baz"" }, ""x"": null }";

            // Act
            var jtoken = json.GetJsonProperty("foo.bar");

            var fooToken = json.GetJsonProperty("foo");
            var fooHasValue = fooToken.HasValues;
            var barHasValue = jtoken.HasValues;
            var xToken = json.GetJsonProperty("x");
            var xHasValue = xToken.HasValues;

            // Assert
            jtoken.Should().NotBeNull("GetJsonProperty should find existing property.");
        }

        [Fact]
        public void GetJsonProperty_ReturnsNull_WhenMatchingStringPropertyDoesNotExist()
        {
            // Arrange
            var json = @"{""foo"": { ""bar"": ""baz"" } }";

            // Act
            var jtoken = json.GetJsonProperty("foo.baz");

            // Assert
            jtoken.Should().BeNull("GetJsonProperty should return null for nonexistant property.");
        }

        // StringExtentsions.IsNotNullOrWhitespace(this string value)
        [Fact]
        public void IsNotNullOrWhitespace_ReturnsTrue_ForStringWithNonWhitespaceCharacters()
        {
            Assert.True("a".IsNotNullOrWhitespace());
        }

        [Fact]
        public void IsNotNullOrWhitespace_ReturnsFalse_ForNullString()
        {
            string input = null;
            Assert.False(input.IsNotNullOrWhitespace());
        }

        [Fact]
        public void IsNotNullOrWhitespace_ReturnsFalse_ForEmptyString()
        {
            Assert.False(string.Empty.IsNotNullOrWhitespace());
        }

        [Fact]
        public void IsNotNullOrWhitespace_ReturnsFalse_ForWhitespaceString()
        {
            Assert.False(" ".IsNotNullOrWhitespace());
        }

        // StringExtentsions.IsNullOrWhitespace(this string value)
        [Fact]
        public void IsNullOrWhitespace_ReturnsFalse_ForStringWithNonWhitespaceCharacters()
        {
            Assert.False("a".IsNullOrWhitespace());
        }

        [Fact]
        public void IsNullOrWhitespace_ReturnsTrue_ForNullString()
        {
            string input = null;
            Assert.True(input.IsNullOrWhitespace());
        }

        [Fact]
        public void IsNullOrWhitespace_ReturnsTrue_ForEmptyString()
        {
            Assert.True(string.Empty.IsNullOrWhitespace());
        }

        [Fact]
        public void IsNullOrWhitespace_ReturnsTrue_ForWhitespaceString()
        {
            Assert.True(" ".IsNullOrWhitespace());
        }

        // StringExtentsions.IsNotNullOrEmpty(this string value)
        [Fact]
        public void IsNotNullOrEmpty_ReturnsTrue_ForStringWithNonWhitespaceCharacters()
        {
            Assert.True("a".IsNotNullOrEmpty());
        }

        [Fact]
        public void IsNotNullOrEmpty_ReturnsFalse_ForNullString()
        {
            string input = null;
            Assert.False(input.IsNotNullOrEmpty());
        }

        [Fact]
        public void IsNotNullOrEmpty_ReturnsFalse_ForEmptyString()
        {
            Assert.False(string.Empty.IsNotNullOrEmpty());
        }

        [Fact]
        public void IsNotNullOrEmpty_ReturnsTrue_ForWhitespaceString()
        {
            Assert.True(" ".IsNotNullOrEmpty());
        }

        // StringExtentsions.IsNullOrEmpty(this string value)
        [Fact]
        public void IsNullOrEmpty_ReturnsFalse_ForStringWithNonWhitespaceCharacters()
        {
            Assert.False("a".IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ReturnsTrue_ForNullString()
        {
            string input = null;
            Assert.True(input.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ReturnsTrue_ForEmptyString()
        {
            Assert.True(string.Empty.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_ReturnsFalse_ForWhitespaceString()
        {
            Assert.False(" ".IsNullOrEmpty());
        }

        [Theory]
        [InlineData("Welcome To    House", "Welcome To House")]
        [InlineData("Welcome   To    House", "Welcome To House")]
        [InlineData("    Welcome To    House   ", "Welcome To House")]
        [InlineData("    a    ", "a")]
        [InlineData("        a       a       a      ", "a a a")]
        public void NormalizeWhitespace_CorrectlyNormalizesWhitespace(string unnormalizedString, string expectedOutput)
        {
            Assert.Equal(expectedOutput, unnormalizedString.NormalizeWhitespace());
        }

        /// <summary>
        /// Test name masking.
        /// </summary>
        /// <param name="input">The name.</param>
        /// <param name="expectedOutput">The masked name.</param>
        [Theory]
        [InlineData("Leon Tayson", "Leon T****")]
        [InlineData("Jim Smith", "Jim S****")]
        [InlineData("Jim S", "Jim S****")]
        [InlineData("Madonna", "M****")]
        [InlineData("M", "M****")]
        [InlineData("*****", "*****")]
        [InlineData("#$%*( %^*& %^&*", "#$%*( %^*& %****")]
        [InlineData("    ", "")]
        [InlineData("  \t  ", "")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void IsName_MaskedProperly(string input, string expectedOutput)
        {
            input.ToMaskedName().Should().Be(expectedOutput);
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("yes", true)]
        [InlineData("false", false)]
        [InlineData("No", false)]
        [InlineData("Dummy", false)]
        [InlineData(null, false)]
        public void ToBoolean_CorrectlyConvertsStringsToBoolean(string input, bool result)
        {
            // Act + Assert
            Assert.Equal(input.ToBoolean(), result);
        }

        /// <summary>
        /// Test Glass's Guide capitalisation specs for vehicle descriptions.
        /// </summary>
        /// <param name="input">The description.</param>
        /// <param name="expectedOutput">The expected result after capitalising.</param>
        [Theory]
        [InlineData("GTS PERFORMANCE (APOLLO BLUE)", "GTS Performance (Apollo Blue)")]
        [InlineData("MULTI POINT F/INJ", "Multi Point F/Inj")]
        [InlineData("6 SP AUTO SEQUENTIAL", "6 SP Auto Sequential")]
        [InlineData("ASCENT SPORT + TR KIT HYBRID", "Ascent Sport + TR Kit Hybrid")]
        [InlineData("CVT AUTO 7 SP SEQUENTIAL", "CVT Auto 7 SP Sequential")]
        [InlineData("COMMUTER (12 SEATS)", "Commuter (12 Seats)")]
        [InlineData("LWB INTERIOR PACK - STL/PNL", "LWB Interior Pack - STL/PNL")]
        [InlineData("CAB SIX INJ DIR BI", "Cab Six Inj Dir Bi")]
        [InlineData("BMXGTSLWB", "BMXGTSLWB")]
        [InlineData(null, "")]
        public void GlassGuideVehicleCapitalisation_ShouldFollowSpecs(string input, string expectedOutput)
        {
            input.ToGlassGuideVehicleDescription().Should().Be(expectedOutput);
        }

        [Fact]
        public void ToZonedDateTimeFromExtendedISO8601_CorrectlyConvertsDateToZonedDateTime()
        {
            var timezone = DateTimeZoneProviders.Tzdb["Australia/Sydney"];
            string dateTime = "2024-04-07T02:11:14.0000000";
            var zonedDateTime = new LocalDateTime(2024, 04, 07, 2, 11, 14).InZoneLeniently(timezone);

            // Assert
            dateTime.ToZonedDateTimeFromExtendedISO8601(timezone).Should().Be(zonedDateTime);
        }

        [Fact]
        public void ToTicksFromExtendedISO8601InZone_CorrectlyConvertsDateToTicks()
        {
            var timezone = DateTimeZoneProviders.Tzdb["Australia/Sydney"];
            string dateTime = "2024-04-07T02:11:14.0000000";
            var zonedDateTime = new LocalDateTime(2024, 04, 07, 2, 11, 14).InZoneLeniently(timezone);
            var ticks = zonedDateTime.ToInstant().ToUnixTimeTicks();

            // Assert
            dateTime.ToTicksFromExtendedISO8601InZone(timezone).Should().Be(ticks);
        }
    }
}
