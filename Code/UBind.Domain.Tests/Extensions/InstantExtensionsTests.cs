// <copyright file="InstantExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using FluentAssertions;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Domain.Extensions;
    using Xunit;

    public class InstantExtensionsTests
    {
        [Fact]
        public void ToIso8601String_CreatesStringInCorrectFormat()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 15, 37, 30);

            // Act
            var output = instant.ToExtendedIso8601String();

            // Assert
            output.Should().Be("2020-08-16T15:37:30Z");
        }

        [Fact]
        public void ToLocalDateInAet_ReturnsCorrectDate_WhenAetDateMatchesUatDate()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            var expectedDate = new LocalDate(2020, 8, 16);

            // Act
            var localDateInAet = instant.ToLocalDateInAet();

            // Assert
            localDateInAet.Should().Be(expectedDate);
        }

        [Fact]
        public void ToLocalDateInAet_ReturnsCorrectDate_WhenAetDateIsAheadOfUtcDate()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 14, 01, 01);
            var expectedDate = new LocalDate(2020, 8, 17);

            // Act
            var localDateInAet = instant.ToLocalDateInAet();

            // Assert
            localDateInAet.Should().Be(expectedDate);
        }

        [Fact]
        public void ToLocalMediumDateStringAet_UsesCorrectTimezoneAndLocale()
        {
            // Arrange
            var instant = Instant.FromUtc(2000, 12, 25, 16, 0);

            // Act
            var dateString = instant.ToLocalMediumDateStringAet();

            // Assert
            dateString.Should().Be("26 Dec 2000");
        }

        [Theory]
        [InlineData("en-AU", "26 Dec 2000")]
        [InlineData("en-GB", "26 Dec 2000")]
        [InlineData("en-US", "Dec 26, 2000")]
        [InlineData("en-NZ", "26/12/2000")]
        public void ToLocalMediumDateString_CorrectlyUsesMediumDateString_ForLocalesWithProvidedFormats(string locale, string expectedOutput)
        {
            // Arrange
            var instant = Instant.FromUtc(2000, 12, 25, 16, 0);

            // Act
            var dateString = instant.ToLocalMediumDateString(Timezones.AET, locale);

            // Assert
            dateString.Should().Be(expectedOutput);
        }

        [Theory]
        [InlineData("fr-FR", "26/12/2000")]
        [InlineData("ja-JP", "2000/12/26")]
        public void ToLocalMediumDateString_FallsBackOnCorrectShortDateFormat_ForLocalesWithNoProvidedFormats(string locale, string expectedOutput)
        {
            // Arrange
            var instant = Instant.FromUtc(2000, 12, 25, 16, 0);

            // Act
            var dateString = instant.ToLocalMediumDateString(Timezones.AET, locale);

            // Assert
            dateString.Should().Be(expectedOutput);
        }

        [Fact]
        public void ToLocalShortTimeStringAet_CorrectlyFormatsTime()
        {
            // Arrange
            var instant = Instant.FromUtc(2000, 12, 25, 16, 0);

            // Act
            var dateString = instant.ToLocalShortTimeStringAet();

            // Assert
            dateString.ToUpper().Should().Be("3:00 AM"); // Note that this is how .NET specifies short date for en-AU, although lower case "am" would be better.
        }

        [Theory]
        [InlineData("en-AU", "3:00 AM")] // Note that this is how .NET specifies short date for en-AU, although lower case "am" would be better.
        [InlineData("en-GB", "03:00")]
        [InlineData("en-US", "3:00 AM")]
        [InlineData("en-NZ", "3:00 am")]
        [InlineData("fr-FR", "03:00")]
        [InlineData("ja-JP", "3:00")]
        public void ToLocalShortTimeString_CorrectlyFormatsDates_ForDifferentLocales(string locale, string expectedOutput)
        {
            // Arrange
            var instant = Instant.FromUtc(2000, 12, 25, 16, 0);

            // Act
            var dateString = instant.ToLocalShortTimeString(Timezones.AET, locale);

            // Assert
            dateString.Should().BeEquivalentTo(expectedOutput); // Culture settings can differ between OS and versions etc., so only using an equivalency assertion here.
        }

        [Fact]
        public void ToInstantAetEndOfDay_ReturnsCorrectTime_WhenAetDateMatchesUatDate()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            var expectedInstant = Instant.FromUtc(2020, 8, 16, 14, 0);

            // Act
            var output = instant.ToInstantAetEndOfDay();

            // Assert
            output.Should().Be(expectedInstant);
        }

        [Fact]
        public void ToInstantAetEndOfDay_ReturnsCorrectTime_WhenAetDateIsAheadOfUtcDate()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 14, 01, 01);
            var expectedInstant = Instant.FromUtc(2020, 8, 17, 14, 0);

            // Act
            var output = instant.ToInstantAetEndOfDay();

            // Assert
            output.Should().Be(expectedInstant);
        }

        [Fact]
        public void ToIso8601DateInAet_ReturnsCorrectlyFormattedDate_WhenAetDateMatchesUatDate()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);

            // Act
            var output = instant.ToIso8601DateInAet();

            // Assert
            output.Should().Be("2020-08-16");
        }

        [Fact]
        public void ToIso8601DateInAet_ReturnsCorrectlyFormattedDate__WhenAetDateIsAheadOfUtcDate()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 14, 01, 01);

            // Act
            var output = instant.ToIso8601DateInAet();

            // Assert
            output.Should().Be("2020-08-17");
        }

        [Fact]
        public void ToRfc5322DateStringInAet_ReturnsCorrectlyFormattedDate_WhenAetDateMatchesUatDate()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);

            // Act
            var output = instant.ToRfc5322DateStringInAet();

            // Assert
            output.Should().Be("16 Aug 2020");
        }

        [Fact]
        public void ToRfc5322DateStringInAet_ReturnsCorrectlyFormattedDate__WhenAetDateIsAheadOfUtcDate()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 14, 01, 01);

            // Act
            var output = instant.ToRfc5322DateStringInAet();

            // Assert
            output.Should().Be("17 Aug 2020");
        }

        [Fact]
        public void To12HourClockTimeInAet_ReturnsCorrectlyFormattedTime()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);

            // Act
            var output = instant.To12HourClockTimeInAet();

            // Assert
            output.Should().Be("11:59 PM");
        }

        [Fact]
        public void ToLocalTimeInAet_ReturnsCorrectlyFormattedTime()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            var expectedTime = new LocalTime(23, 59, 59);

            // Act
            var output = instant.ToLocalTimeInAet();

            // Assert
            output.Should().Be(expectedTime);
        }

        [Fact]
        public void ToIso8601TimeInAet_ReturnsCorrectlyFormattedTime()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);

            // Act
            var output = instant.ToIso8601TimeInAet();

            // Assert
            output.Should().Be("23:59:59");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_AddOneYear()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            Period period = Period.FromYears(1);

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.ToLocalDateInAet(), output.ToLocalDateInAet());

            // Assert
            periodInBetween.Years.Should().Be(1);
            output.ToString().Should().Be("2021-08-16T13:59:59Z");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_Add2Months()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            Period period = Period.FromMonths(2);

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.ToLocalDateInAet(), output.ToLocalDateInAet());

            // Assert
            periodInBetween.Months.Should().Be(2);
            output.ToString().Should().Be("2020-10-16T12:59:59Z");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_Add4weeks()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            Period period = Period.FromWeeks(4);

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.ToLocalDateInAet(), output.ToLocalDateInAet());

            // Assert
            periodInBetween.Days.Should().Be(28);
            output.ToString().Should().Be("2020-09-13T13:59:59Z");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_Add2Days()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            Period period = Period.FromDays(2);

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.ToLocalDateInAet(), output.ToLocalDateInAet());

            // Assert
            periodInBetween.Days.Should().Be(2);
            output.ToString().Should().Be("2020-08-18T13:59:59Z");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_Add2Hours()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            Period period = Period.FromHours(2);

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.InZone(Timezones.AET).LocalDateTime, output.InZone(Timezones.AET).LocalDateTime);

            // Assert
            periodInBetween.Hours.Should().Be(2);
            output.ToString().Should().Be("2020-08-16T15:59:59Z");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_Add5Minutes()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            Period period = Period.FromMinutes(5);

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.InZone(Timezones.AET).LocalDateTime, output.InZone(Timezones.AET).LocalDateTime);

            // Assert
            periodInBetween.Minutes.Should().Be(5);
            output.ToString().Should().Be("2020-08-16T14:04:59Z");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_Add50Seconds()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            Period period = Period.FromSeconds(50);

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.InZone(Timezones.AET).LocalDateTime, output.InZone(Timezones.AET).LocalDateTime);

            // Assert
            periodInBetween.Seconds.Should().Be(50);
            output.ToString().Should().Be("2020-08-16T14:00:49Z");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_Add50Milliseconds()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            Period period = Period.FromMilliseconds(50);

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.InZone(Timezones.AET).LocalDateTime, output.InZone(Timezones.AET).LocalDateTime);

            // Assert
            periodInBetween.Milliseconds.Should().Be(50);
            output.ToString().Should().Be("2020-08-16T13:59:59Z");
        }

        [Fact]
        public void AddPeriodInAet_CorrectResult_AddParsedPeriod()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            var oo = PeriodPattern.NormalizingIso.Parse("P1Y2WT2S");
            Period period = oo.Value;

            // Act
            var output = instant.AddPeriodInAet(period);
            var periodInBetween = Period.Between(instant.InZone(Timezones.AET).LocalDateTime, output.InZone(Timezones.AET).LocalDateTime);

            // Assert
            periodInBetween.Years.Should().Be(1);
            periodInBetween.Days.Should().Be(14);
            periodInBetween.Seconds.Should().Be(2);
            output.ToString().Should().Be("2021-08-30T14:00:01Z");
        }

        [Fact]
        public void ToUtcAtEndOfDay_CorrectResult_ShouldReturnValueAtEndOfDay()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);

            // Act
            var output = instant.ToUtcAtEndOfDay();

            // Assert
            output.ToString().Should().Be("2020-08-16T23:59:59Z");
        }

        [Fact]
        public void ToUtcAtStartOfDay_CorrectResult_ShouldReturnValueAtStartOfDay()
        {
            // Arrange
            var instant = Instant.FromUtc(2020, 8, 16, 13, 59, 59);

            // Act
            var output = instant.ToUtcAtStartOfDay();

            // Assert
            output.ToString().Should().Be("2020-08-16T00:00:00Z");
        }

        [Fact]
        public void GetDaysDifference_ReturnsCorrectResult()
        {
            // Arrange
            var instant1 = Instant.FromUtc(2020, 8, 16, 13, 59, 59);
            var instant2 = Instant.FromUtc(2020, 8, 18, 13, 59, 59);

            // Act
            var daysDifference = instant1.GetDaysDifference(instant2);

            // Assert
            daysDifference.Should().Be(2.0);
        }
    }
}
