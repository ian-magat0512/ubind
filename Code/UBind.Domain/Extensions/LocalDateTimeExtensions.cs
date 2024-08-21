// <copyright file="LocalDateTimeExtensions.cs" company="uBind">
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
    public static class LocalDateTimeExtensions
    {
        /// <summary>
        /// Get the ISO8601 representation of a date.
        /// </summary>
        /// <param name="date">A date.</param>
        /// <returns>A string containing the ISO8601 representation of the date.</returns>
        public static string ToIso8601(this LocalDateTime date)
        {
            return LocalDateTimePattern.GeneralIso.Format(date);
        }

        /// <summary>
        /// Get the extended ISO8601 representation of a date.
        /// </summary>
        /// <param name="date">A date.</param>
        /// <returns>A string containing the ISO8601 representation of the date, providing up to 7 decimal places of sub-second accuracy.</returns>
        public static string ToExtendedIso8601(this LocalDateTime date)
        {
            return LocalDateTimePattern.ExtendedIso.Format(date);
        }

        /// <summary>
        /// Gets the short date format of a date.
        /// </summary>
        /// <param name="date">A date.</param>
        /// <returns>A string value of a shortdate.</returns>
        public static string ToShortDateFormat(this LocalDateTime date)
        {
            return LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd").Format(date);
        }

        public static LocalDateTime ToTargetTimeZone(
        this LocalDateTime dateTime,
        DateTimeZone timeZone,
        DateTimeZone targetTimeZone)
        {
            var zonedDateTime = dateTime.InZoneLeniently(timeZone);
            return zonedDateTime.WithZone(targetTimeZone).LocalDateTime;
        }
    }
}
