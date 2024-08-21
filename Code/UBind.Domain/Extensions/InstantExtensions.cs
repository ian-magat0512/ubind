// <copyright file="InstantExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System.Globalization;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Extension methods for <see cref="NodaTime.Instant"/>.
    /// </summary>
    public static class InstantExtensions
    {
        /// <summary>
        /// Get the ISO8601 representation of an instant.
        /// </summary>
        /// <param name="instant">An instant.</param>
        /// <returns>A string containing the ISO8601 representation of the instant.</returns>
        public static string ToExtendedIso8601String(this Instant instant)
        {
            return InstantPattern.ExtendedIso.Format(instant);
        }

        /// <summary>
        /// Get the ISO8601 representation of the instant in UTC.
        /// </summary>
        /// <param name="instant">An instant.</param>
        /// <returns>A string containing the ISO8601 representation of the instant in UTC.</returns>
        public static string ToIso8601DateInUtc(this Instant instant)
        {
            return instant.InUtc().ToDateTimeOffset().ToString("yyyy-MM-ddTHH:mm:ss\\.fffffffzzz");
        }

        /// <summary>
        /// Get the local date in the Australian Eastern timezone for a given instant in time.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <returns>The date in the Australian Eastern timezone at this moment in time.</returns>
        public static LocalDate ToLocalDateInAet(this Instant instant)
        {
            return instant.InZone(Timezones.AET).Date;
        }

        /// <summary>
        /// Get the local date in the Australian Eastern timezone for a given instant in time.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <returns>The date in the Australian Eastern timezone at this moment in time.</returns>
        public static LocalDateTime ToLocalDateTimeInAet(this Instant instant)
        {
            return instant.InZone(Timezones.AET).LocalDateTime;
        }

        /// <summary>
        /// Get the local date for a given timezone and instant in time.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <param name="timezone">The time zone.</param>
        /// <returns>The date in the given timezone at this moment in time.</returns>
        public static LocalDateTime ToLocalDateTimeInZone(this Instant instant, DateTimeZone timezone)
        {
            return instant.InZone(timezone).LocalDateTime;
        }

        /// <summary>
        /// Gets a string representation of the date in AET, for a given instant, using the short format for Autralia.
        /// </summary>
        /// <param name="instant">An instant representing the point in time to get the date of.</param>
        /// <returns>A string with the date.</returns>
        public static string ToLocalMediumDateStringAet(this Instant instant) =>
            instant.ToLocalMediumDateString(Timezones.AET, Locales.en_AU);

        /// <summary>
        /// Gets a string representation of the date in a given zone, for a given instant, using the default short format for a given locale.
        /// </summary>
        /// <param name="instant">An instant representing the point in time to get the date of.</param>
        /// <param name="timeZone">The time zone to calculate the date in.</param>
        /// <param name="localeCode">The locale to determine format for.</param>
        /// <returns>A string with the date.</returns>
        public static string ToLocalMediumDateString(this Instant instant, DateTimeZone timeZone, string localeCode)
        {
            return instant.InZone(timeZone).Date.ToString(FormatStrings.MediumDate[localeCode], new CultureInfo(localeCode));
        }

        /// <summary>
        /// Gets a string representation of the time in AET, for a given instant, using the short format for Autralia.
        /// </summary>
        /// <param name="instant">An instant representing the point in time to get the date of.</param>
        /// <returns>A string with the date.</returns>
        public static string ToLocalShortTimeStringAet(this Instant instant) =>
            instant.ToLocalShortTimeString(Timezones.AET, Locales.en_AU);

        /// <summary>
        /// Gets a string representation of the time in a given zone, for a given instant, using the default short format for a given locale.
        /// </summary>
        /// <param name="instant">An instant representing the point in time to get the date of.</param>
        /// <param name="timeZone">The time zone to calculate the date in.</param>
        /// <param name="localeCode">The locale to determine format for.</param>
        /// <returns>A string with the date.</returns>
        public static string ToLocalShortTimeString(this Instant instant, DateTimeZone timeZone, string localeCode)
        {
            return instant.InZone(timeZone).TimeOfDay.ToString("t", new CultureInfo(localeCode));
        }

        /// <summary>
        /// Get the instant date in the Australian Eastern timezone at midnight.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <returns>The instant time in the Australian Eastern timezone in instant midnight time.</returns>
        public static Instant ToInstantAetEndOfDay(this Instant instant)
        {
            return instant.ToLocalDateInAet().PlusDays(1).AtStartOfDayInZone(Timezones.AET).ToInstant();
        }

        public static Instant ToUtcAtEndOfDay(this Instant instant)
        {
            DateTimeZone zone = DateTimeZone.Utc;
            return instant.InZone(zone).LocalDateTime.Date.GetInstantAtEndOfDayInZone(zone);
        }

        public static Instant ToUtcAtStartOfDay(this Instant instant)
        {
            DateTimeZone zone = DateTimeZone.Utc;
            return instant.InZone(zone).LocalDateTime.Date.AtMidnight().InZoneLeniently(zone).ToInstant();
        }

        /// <summary>
        /// Get the ISO8601 representation of the date in the Australian Eastern timezone for a given instant in time.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <returns>The ISO8601 representation of the date in the Australian Eastern timezone at this moment in time.</returns>
        /// <remarks>Do not use this for normal operations. Dates should be timezone specific and should not be defaulted to AET.</remarks>
        public static string ToIso8601DateInAet(this Instant instant)
        {
            return instant.ToLocalDateInAet().ToIso8601();
        }

        /// <summary>
        /// Gets a string containing the date and time in AET represented by an instant.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <returns>A string containing the date in AET represented by an instant.</returns>
        public static string ToRfc5322DateStringInAet(this Instant instant)
        {
            var rfc5322DatePattern = ZonedDateTimePattern.CreateWithInvariantCulture("dd MMM yyyy", DateTimeZoneProviders.Tzdb);

            if (instant == default(Instant))
            {
                return string.Empty;
            }

            return rfc5322DatePattern.Format(instant.InZone(Timezones.AET));
        }

        /// <summary>
        /// Gets a string containing the date and time in AET represented by an instant.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <returns>A string containing the time in AET represented by an instant.</returns>
        public static string To12HourClockTimeInAet(this Instant instant)
        {
            var twelveHourClockPattern = ZonedDateTimePattern.CreateWithInvariantCulture("h:mm tt", DateTimeZoneProviders.Tzdb);

            if (instant == default(Instant))
            {
                return string.Empty;
            }

            return twelveHourClockPattern.Format(instant.InZone(Timezones.AET));
        }

        /// <summary>
        /// Get the local time in the Australian Eastern timezone for a given instant in time.
        /// </summary>
        /// <param name="instant">An instant.</param>
        /// <returns>The time in the Australian Eastern timezone at this instant.</returns>
        public static LocalTime ToLocalTimeInAet(this Instant instant)
        {
            return instant.InZone(Timezones.AET).TimeOfDay;
        }

        /// <summary>
        /// Get the local time in the Australian Eastern timezone for a given instant in time.
        /// </summary>
        /// <param name="instant">An instant.</param>
        /// <param name="period">The period to append.</param>
        /// <returns>The time in the Australian Eastern timezone at this instant.</returns>
        public static Instant AddPeriodInAet(this Instant instant, Period period)
        {
            var zonedDateTime = instant.InZone(Timezones.AET);
            var localDateTime = zonedDateTime.LocalDateTime.Plus(period);
            var resultingInstant = localDateTime.InZoneStrictly(Timezones.AET).ToInstant();
            return resultingInstant;
        }

        /// <summary>
        /// Get the ISO8601 representation of the time in the Australian Eastern timezone for a given instant in time.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <returns>The ISO8601 representation of the date in the Australian Eastern timezone at this moment in time.</returns>
        public static string ToIso8601TimeInAet(this Instant instant)
        {
            return instant.ToLocalTimeInAet().ToIso8601();
        }

        /// <summary>
        /// Get the datetime in the Australian Eastern timezone for a given instant in time.
        /// </summary>
        /// <param name="instant">The instant.</param>
        /// <returns>The datetime in the Australian Eastern timezone at this moment in time.</returns>
        public static string ToIso8601DateTimeInAet(this Instant instant)
        {
            return instant.InZone(Timezones.AET).ToOffsetDateTime().ToString();
        }

        public static Instant FromExtendedIso8601String(this string dateTimeString)
        {
            var pattern = InstantPattern.ExtendedIso; // Use ISO 8601 pattern
            var parseResult = pattern.Parse(dateTimeString);

            if (parseResult.Success)
            {
                return parseResult.Value;
            }
            else
            {
                // Handle the error appropriately, e.g., log, throw an exception, etc.
                throw new FormatException($"Failed to parse the date time string: {dateTimeString}");
            }
        }

        /// <summary>
        /// Returns a string representation of the instant in the format "dd/MM/yyyy".
        /// </summary>
        /// <param name="timestamp">The timestamp</param>
        /// <returns>The formatted string</returns>
        public static string ToDdMmYyyyString(this Instant timestamp)
        {
            return timestamp.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        }

        public static ZonedDateTime ToStartOfDayInZone(this Instant instant, DateTimeZone zone)
        {
            return instant.InZone(zone).Date.AtStartOfDayInZone(zone);
        }

        public static ZonedDateTime ToStartOfMonthInZone(this Instant instant, DateTimeZone zone)
        {
            return instant.InZone(zone).ToStartOfMonth(zone);
        }

        public static ZonedDateTime ToStartOfQuarterInZone(this Instant instant, DateTimeZone zone)
        {
            return instant.InZone(zone).ToStartOfQuarter(zone);
        }

        public static ZonedDateTime ToStartOfYearInZone(this Instant instant, DateTimeZone zone)
        {
            return instant.InZone(zone).ToStartOfYear(zone);
        }

        /// <summary>
        /// Gets the difference in days between two instants.
        /// </summary>
        /// <param name="start">The instant.</param>
        /// <param name="end">Another instant to compute the difference with.</param>
        /// <returns>The difference.</returns>
        public static double GetDaysDifference(this Instant start, Instant end)
        {
            var duration = end - start;
            var ticks = duration.TotalTicks;
            var days = ticks / NodaConstants.TicksPerDay;
            return days;
        }
    }
}
