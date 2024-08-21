// <copyright file="ZonedDateTimeExtensionsTests.cs" company="uBind">
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

    public class ZonedDateTimeExtensionsTests
    {
        [Fact]
        public void ToIso8601WithUTCOffset_formatsCorrect()
        {
            // Arrange
            var localDateTime = new LocalDate(2022, 01, 01);
            var timezone = Timezones.AET;
            var zonedDateTime = localDateTime.AtStartOfDayInZone(timezone);

            // Act
            var output = zonedDateTime.ToIso8601WithUTCOffset();

            // Assert
            output.Should().Be("2022-01-01T00:00:00.0000000+11:00");
        }

        [Fact]
        public void To24HrFormat_formatsCorrect()
        {
            // Arrange
            var localDateTime = new LocalDateTime(2022, 01, 01, 15, 0, 0);
            var timezone = Timezones.AET;
            var zonedDateTime = localDateTime.InZoneLeniently(timezone);

            // Act
            var output = zonedDateTime.To24HrFormat();

            // Assert
            output.Should().Be("15:00:00 PM");
        }

        [Fact]
        public void ToDMMMYYYY_formatsCorrect()
        {
            // Arrange
            var localDateTime = new LocalDate(2022, 01, 01);
            var timezone = Timezones.AET;
            var zonedDateTime = localDateTime.AtStartOfDayInZone(timezone);

            // Act
            var output = zonedDateTime.ToDMMMYYYY();

            // Assert
            output.Should().Be("1 Jan 2022");
        }

        [Fact]
        public void ToMMMMYYYY_formatsCorrect()
        {
            // Arrange
            var localDateTime = new LocalDate(2022, 01, 01);
            var timezone = Timezones.AET;
            var zonedDateTime = localDateTime.AtStartOfDayInZone(timezone);

            // Act
            var output = zonedDateTime.ToMMMMYYYY();

            // Assert
            output.Should().Be("January 2022");
        }

        [Fact]
        public void ToDMMMMYYYY_formatsCorrect()
        {
            // Arrange
            var localDateTime = new LocalDate(2022, 01, 01);
            var timezone = Timezones.AET;
            var zonedDateTime = localDateTime.AtStartOfDayInZone(timezone);

            // Act
            var output = zonedDateTime.ToDMMMMYYYY();

            // Assert
            output.Should().Be("1 January 2022");
        }

        [Fact]
        public void ToYYYYMMDD_formatsCorrect()
        {
            // Arrange
            var localDateTime = new LocalDate(2022, 01, 01);
            var timezone = Timezones.AET;
            var zonedDateTime = localDateTime.AtStartOfDayInZone(timezone);

            // Act
            var output = zonedDateTime.ToYYYYMMDD();

            // Assert
            output.Should().Be("2022-01-01");
        }

        [Fact]
        public void ToQQYYYY_formatsCorrect()
        {
            // Arrange
            var localDateTime = new LocalDate(2022, 01, 01);
            var timezone = Timezones.AET;
            var zonedDateTime = localDateTime.AtStartOfDayInZone(timezone);

            // Act
            var output = zonedDateTime.ToQQYYYY();

            // Assert
            output.Should().Be("Q1 2022");
        }

        [Fact]
        public void GetQuarter_returns_correctQuarter()
        {
            // Arrange
            var localDateTime = new LocalDate(2022, 01, 01);
            var timezone = Timezones.AET;
            var zonedDateTime = localDateTime.AtStartOfDayInZone(timezone);

            // Act
            var output = zonedDateTime.GetQuarter();

            // Assert
            output.Should().Be(1);
        }

        [Theory]
        [InlineData("Australia/Melbourne", "2022-01-01T00:00:00.0000000", null, "2022-01-01T00:00:00.0000000+11:00")]
        [InlineData("Australia/Melbourne", "2022-01-01T00:00:00.0000000", 10, "2022-01-11T00:00:00.0000000+11:00")]
        [InlineData("Australia/Melbourne", "2022-01-01T00:00:00.0000000", 31, "2022-02-01T00:00:00.0000000+11:00")]
        public void ToStartOfMonth_returns_correctZonedDateTime(
            string timeZoneId,
            string date,
            int? periodFromDays,
            string expectedDate)
        {
            // Arrange
            var timezone = Timezones.GetTimeZoneByIdOrNull(timeZoneId);
            var zonedDateTime = date.ToLocalDateTimeFromExtendedIso8601().InZoneLeniently(timezone);
            Period period = null;
            if (periodFromDays != null)
            {
                period = Period.FromDays(periodFromDays.Value);
            }

            // Act
            var output = zonedDateTime.ToStartOfMonth(timezone, period);

            // Assert
            output.ToIso8601WithUTCOffset().Should().Be(expectedDate);
        }

        [Theory]
        [InlineData("Australia/Melbourne", "2022-01-01T00:00:00.0000000", null, "2022-01-01T00:00:00.0000000+11:00")]
        [InlineData("Australia/Melbourne", "2022-01-01T00:00:00.0000000", 10, "2022-01-11T00:00:00.0000000+11:00")]
        [InlineData("Australia/Melbourne", "2022-01-01T00:00:00.0000000", 90, "2022-04-01T00:00:00.0000000+11:00")]
        public void ToStartOfQuarter_returns_correctZonedDateTime(
            string timeZoneId,
            string date,
            int? periodFromDays,
            string expectedDate)
        {
            // Arrange
            var timezone = Timezones.GetTimeZoneByIdOrNull(timeZoneId);
            var zonedDateTime = date.ToLocalDateTimeFromExtendedIso8601().InZoneLeniently(timezone);
            Period period = null;
            if (periodFromDays != null)
            {
                period = Period.FromDays(periodFromDays.Value);
            }

            // Act
            var output = zonedDateTime.ToStartOfQuarter(timezone, period);

            // Assert
            output.ToIso8601WithUTCOffset().Should().Be(expectedDate);
        }

        [Theory]
        [InlineData("Australia/Melbourne", "2022-01-01T00:00:00.0000000", null, "2022-01-01T00:00:00.0000000+11:00")]
        [InlineData("Australia/Melbourne", "2022-01-05T00:00:00.0000000", 10, "2022-01-11T00:00:00.0000000+11:00")]
        [InlineData("Australia/Melbourne", "2022-01-01T00:00:00.0000000", 365, "2023-01-01T00:00:00.0000000+11:00")]
        public void ToStartOfYear_returns_correctZonedDateTime(
            string timeZoneId,
            string date,
            int? periodFromDays,
            string expectedDate)
        {
            // Arrange
            var timezone = Timezones.GetTimeZoneByIdOrNull(timeZoneId);
            var zonedDateTime = date.ToLocalDateTimeFromExtendedIso8601().InZoneLeniently(timezone);
            Period period = null;
            if (periodFromDays != null)
            {
                period = Period.FromDays(periodFromDays.Value);
            }

            // Act
            var output = zonedDateTime.ToStartOfYear(timezone, period);

            // Assert
            output.ToIso8601WithUTCOffset().Should().Be(expectedDate);
        }

        [Fact]
        public void AddPeriod_returns_correctZonedDateTime()
        {
            // Arrange
            var timezone = Timezones.AET;

            // 7/4/2024 2:11:14 am is ambiguous in time zone Australia/Sydney / Australia/Melbourne
            var zonedDateTime = new LocalDateTime(2024, 04, 07, 2, 11, 14).InZoneLeniently(timezone);
            var expectedZonedDateTime = new LocalDateTime(2024, 05, 07, 2, 11, 14).InZoneLeniently(timezone);

            // Act
            var output = zonedDateTime.AddPeriod(timezone, Period.FromMonths(1));

            // Assert
            output.Should().Be(expectedZonedDateTime);
        }
    }
}
