// <copyright file="ZonedDateTimeExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// Extension methods for <see cref="LocalDate"/>.
    /// </summary>
    public static class ZonedDateTimeExtensions
    {
        /// <summary>
        /// Get the ISO8601 representation of a dateTime with UTC.
        /// </summary>
        /// <param name="dateTime">A dateTime.</param>
        /// <returns>A string containing the ISO8601 representation of the dateTime, providing up to 7 decimal places of sub-second accuracy.</returns>
        public static string ToIso8601WithUTCOffset(this ZonedDateTime dateTime)
        {
            var pattern = ZonedDateTimePattern.CreateWithInvariantCulture("uuuu-MM-ddTHH:mm:ss.fffffffo<+HH:mm>", DateTimeZoneProviders.Tzdb);
            return pattern.Format(dateTime);
        }

        /// <summary>
        /// Get the 12 hr format representation of a time from a ZonedDateTime.
        /// </summary>
        /// <returns>A string representing a dateTime in the specified format.</returns>
        public static string To24HrFormat(this ZonedDateTime dateTime)
        {
            return ZonedDateTimePattern.CreateWithInvariantCulture("HH:mm:ss tt", DateTimeZoneProviders.Tzdb).Format(dateTime);
        }

        /// <summary>
        /// Get a string representing a dateTime in the specified format.
        /// </summary>
        /// <param name="dateTime">The dateTime to represent.</param>
        /// <returns>A string representing a dateTime in the specified format.</returns>
        public static string ToDMMMYYYY(this ZonedDateTime dateTime)
        {
            return ZonedDateTimePattern.CreateWithInvariantCulture("d MMM yyyy", DateTimeZoneProviders.Tzdb).Format(dateTime);
        }

        /// <summary>
        /// Get a string representing a dateTime in the specified format.
        /// </summary>
        /// <param name="dateTime">The dateTime to represent.</param>
        /// <returns>A string representing a dateTime in the specified format.</returns>
        public static string ToMMMMYYYY(this ZonedDateTime dateTime)
        {
            return ZonedDateTimePattern.CreateWithInvariantCulture("MMMM yyyy", DateTimeZoneProviders.Tzdb).Format(dateTime);
        }

        /// <summary>
        /// Get a string representing a dateTime in the specified format.
        /// </summary>
        /// <param name="dateTime">The dateTime to represent.</param>
        /// <returns>A string representing a dateTime in the specified format.</returns>
        public static string ToDMMMMYYYY(this ZonedDateTime dateTime)
        {
            return ZonedDateTimePattern.CreateWithInvariantCulture("d MMMM yyyy", DateTimeZoneProviders.Tzdb).Format(dateTime);
        }

        /// <summary>
        /// Get a string representing a dateTime in the specified format.
        /// </summary>
        /// <param name="dateTime">The dateTime to represent.</param>
        /// <returns>A string representing a dateTime in yyyy-MM-dd format.</returns>
        public static string ToYYYYMMDD(this ZonedDateTime dateTime)
        {
            return ZonedDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd", DateTimeZoneProviders.Tzdb).Format(dateTime);
        }

        /// <summary>
        /// Get a string representing a dateTime in the specified format.
        /// </summary>
        /// <param name="dateTime">The dateTime to represent.</param>
        /// <returns>A string representing a dateTime in the specified format.</returns>
        public static string ToQQYYYY(this ZonedDateTime dateTime)
        {
            return $"Q{dateTime.GetQuarter()} {dateTime.Year}";
        }

        /// <summary>
        /// Gets the quarter the month belongs to.
        /// </summary>
        /// <param name="dateTime">The dateTime.</param>
        /// <returns>Quarter value.</returns>
        public static int GetQuarter(this ZonedDateTime dateTime)
        {
            return (dateTime.Month + 2) / 3;
        }

        public static ZonedDateTime ToStartOfMonth(this ZonedDateTime dateTime, DateTimeZone timeZone, Period addedPeriod = null)
        {
            var localDate = new LocalDate(dateTime.Year, dateTime.Month, 1);
            return localDate.WithPeriod(timeZone, addedPeriod);
        }

        public static ZonedDateTime ToStartOfQuarter(this ZonedDateTime dateTime, DateTimeZone timeZone, Period addedPeriod = null)
        {
            int month = ((dateTime.GetQuarter() - 1) * 3) + 1;
            var localDate = new LocalDate(dateTime.Year, month, 1);
            return localDate.WithPeriod(timeZone, addedPeriod);
        }

        public static ZonedDateTime ToStartOfYear(this ZonedDateTime dateTime, DateTimeZone timeZone, Period addedPeriod = null)
        {
            var localDate = new LocalDate(dateTime.Year, 1, 1);
            return localDate.WithPeriod(timeZone, addedPeriod);
        }

        /// <summary>
        /// Adds the period in a given time zone.
        /// NOTE: This method is lenient and will not
        /// throw an exception if the resulting date time is ambiguous
        /// such as during daylight savings time.
        /// </summary>
        public static ZonedDateTime AddPeriod(this ZonedDateTime dateTime, DateTimeZone timeZone, Period period)
        {
            LocalDateTime localDateTime = dateTime.LocalDateTime + period;
            return localDateTime.InZoneLeniently(timeZone);
        }

        public static ZonedDateTime AtStartOfDayInZone(this ZonedDateTime dateTime, DateTimeZone timeZone)
        {
            return dateTime.LocalDateTime.Date.AtStartOfDayInZone(timeZone);
        }

        public static ZonedDateTime AtEndOfDayInZone(this ZonedDateTime dateTime, DateTimeZone timeZone)
        {
            return dateTime.LocalDateTime.Date.PlusDays(1).AtStartOfDayInZone(timeZone).Minus(Duration.FromTicks(1));
        }

        private static ZonedDateTime WithPeriod(this LocalDate localDate, DateTimeZone timeZone, Period addedPeriod = null)
        {
            if (addedPeriod != null)
            {
                localDate = localDate.Plus(addedPeriod);
            }

            return localDate.AtStartOfDayInZone(timeZone);
        }
    }
}
