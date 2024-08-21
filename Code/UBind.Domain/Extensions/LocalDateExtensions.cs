// <copyright file="LocalDateExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;
    using System.Globalization;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// Extension methods for <see cref="LocalDate"/>.
    /// </summary>
    public static class LocalDateExtensions
    {
        /// <summary>
        /// Get the ISO8601 representation of a date.
        /// </summary>
        /// <param name="date">A date.</param>
        /// <returns>A string containing the ISO8601 representation of the date.</returns>
        public static string ToIso8601(this LocalDate date)
        {
            return LocalDatePattern.Iso.Format(date);
        }

        /// <summary>
        /// Get a string representing a date in the format dd/mm/yyyy.
        /// </summary>
        /// <param name="date">The date to represent.</param>
        /// <returns>A string representing a date in the format dd/mm/yyyy.</returns>
        public static string ToMMDDYYYWithSlashes(this LocalDate date)
        {
            return LocalDatePattern.CreateWithInvariantCulture("dd/MM/yyyy").Format(date);
        }

        /// <summary>
        /// Get a string representing a date in the specified format.
        /// </summary>
        /// <param name="date">The date to represent.</param>
        /// <returns>A string representing a date in the specified format.</returns>
        public static string ToDMMMYYYY(this LocalDate date)
        {
            return LocalDatePattern.CreateWithInvariantCulture("d MMM yyyy").Format(date);
        }

        /// <summary>
        /// Get a string representing a date in the specified format.
        /// </summary>
        /// <param name="date">The date to represent.</param>
        /// <param name="cultureInfo">The culture info that will be used to format the date.</param>
        /// <returns>A string representing a date in the specified format.</returns>
        public static string ToDMMMYYYY(this LocalDate date, CultureInfo cultureInfo)
        {
            var pattern = LocalDatePattern.Create("d MMM yyyy", cultureInfo);
            return pattern.Format(date);
        }

        /// <summary>
        /// Get a string representing a date in the specified format.
        /// </summary>
        /// <param name="date">The date to represent.</param>
        /// <returns>A string representing a date in the specified format.</returns>
        public static string ToMMMMYYYY(this LocalDate date)
        {
            return LocalDatePattern.CreateWithInvariantCulture("MMMM yyyy").Format(date);
        }

        /// <summary>
        /// Get a string representing a date in the specified format.
        /// </summary>
        /// <param name="date">The date to represent.</param>
        /// <returns>A string representing a date in the specified format.</returns>
        public static string ToMMYYYY(this LocalDate date)
        {
            return LocalDatePattern.CreateWithInvariantCulture("MM yyyy").Format(date);
        }

        /// <summary>
        /// Gets the number of ticks since the epoch at the start of the given local date in a given timezone.
        /// </summary>
        /// <param name="localDate">A string representing the date in ISO8601 format.</param>
        /// <param name="timezone">The timezone to calculate the time in.</param>
        /// <returns>The number of ticks since the epoch for the specified moment in time.</returns>
        public static long GetTicksAtStartOfDayInZone(this LocalDate localDate, DateTimeZone timezone)
        {
            return localDate
                    .GetInstantAtStartOfDayInZone(timezone)
                    .ToUnixTimeTicks();
        }

        /// <summary>
        /// Gets the number of ticks since the epoch at the end of the given local date in a given timezone.
        /// </summary>
        /// <param name="localDate">A string representing the date in ISO8601 format.</param>
        /// <param name="timezone">The timezone to calculate the time in.</param>
        /// <returns>The number of ticks since the epoch for the specified moment in time.</returns>
        public static long GetTicksAtEndOfDayInZone(this LocalDate localDate, DateTimeZone timezone)
        {
            return localDate
                    .GetInstantAtEndOfDayInZone(timezone)
                    .ToUnixTimeTicks();
        }

        /// <summary>
        /// Gets the instant instance at the end of the given date in a given timezone.
        /// </summary>
        /// <param name="localDate">The local date.</param>
        /// <param name="timezone">The timezone to calculate the time in.</param>
        /// <returns>The instant instance for the specified moment in time.</returns>
        public static Instant GetInstantAtEndOfDayInZone(this LocalDate localDate, DateTimeZone timezone)
        {
            return localDate
                    .PlusDays(1)
                    .AtStartOfDayInZone(timezone)
                    .Minus(Duration.FromTicks(1))
                    .ToInstant();
        }

        /// <summary>
        /// Gets the instant instance at the start of the given date in a given timezone.
        /// </summary>
        /// <param name="localDate">the local date..</param>
        /// <param name="timezone">The timezone to calculate the time in.</param>
        /// <returns>The instant instance for the specified moment in time.</returns>
        public static Instant GetInstantAtStartOfDayInZone(this LocalDate localDate, DateTimeZone timezone)
        {
            return localDate
                    .AtStartOfDayInZone(timezone)
                    .ToInstant();
        }

        /// <summary>
        /// Get week number of the specified date.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>A number representing the week number of year.</returns>
        public static int ToWeekOfYear(this DateTime dateTime)
        {
            var day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(dateTime);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                dateTime = dateTime.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        /// <summary>
        /// Get number of days from seconds.
        /// </summary>
        /// <param name="seconds">The number of seconds.</param>
        /// <returns>A number representing number of days.</returns>
        public static int SecondsToDays(int seconds)
        {
            return seconds / 24 / 60 / 60;
        }

        public static Instant GetInstantAt4pmAet(this LocalDate localDate)
        {
            return new LocalDateTime(localDate.Year, localDate.Month, localDate.Day, 16, 0)
                .InZoneLeniently(Timezones.AET)
                .ToInstant();
        }
    }
}
