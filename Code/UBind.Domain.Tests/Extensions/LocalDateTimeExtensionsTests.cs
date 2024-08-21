// <copyright file="LocalDateTimeExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain;
    using Xunit;

    public class LocalDateTimeExtensionsTests
    {
        [Theory]
        [InlineData(2023, 12, 29, 10, 50, 56, "UTC", "America/New_York", "December 29, 2023 05:50:56 AM")]
        [InlineData(2023, 3, 12, 10, 50, 56, "Asia/Singapore", "UTC", "March 12, 2023 02:50:56 AM")]
        [InlineData(2023, 11, 7, 2, 33, 06, "Asia/Tokyo", "Asia/Shanghai", "November 07, 2023 01:33:06 AM")]
        public void ToTargetTimeZone_ShouldReturnTheLocalDateTimeInTargetTimeZone(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            string timeZoneId,
            string targeTimeZoneId,
            string expectedOutput)
        {
            // Arrange
            var dateTime = new LocalDateTime(year, month, day, hour, minute, second);

            // Act
            var targetDateTime = dateTime.ToTargetTimeZone(
                Timezones.GetTimeZoneByIdOrThrow(timeZoneId),
                Timezones.GetTimeZoneByIdOrThrow(targeTimeZoneId));

            // Assert
            targetDateTime.ToString("MMMM dd, yyyy hh:mm:ss tt", null).Should().Be(expectedOutput);
        }
    }
}
