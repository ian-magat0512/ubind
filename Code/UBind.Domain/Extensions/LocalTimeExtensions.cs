// <copyright file="LocalTimeExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System.Globalization;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// Extension methods for <see cref="LocalTime"/>.
    /// </summary>
    public static class LocalTimeExtensions
    {
        /// <summary>
        /// Get the ISO8601 representation of a date.
        /// </summary>
        /// <param name="time">A time.</param>
        /// <returns>A tsring containing the ISO8601 representation of the time.</returns>
        public static string ToIso8601(this LocalTime time)
        {
            return LocalTimePattern.ExtendedIso.Format(time);
        }

        /// <summary>
        /// Get the 12 hr format representation of a time.
        /// </summary>
        /// <param name="time">A time.</param>
        /// <returns>A string representing a date in the specified format.</returns>
        public static string To12HrFormat(this LocalTime time)
        {
            return LocalTimePattern.CreateWithInvariantCulture("hh:mm tt").Format(time);
        }

        /// <summary>
        /// Get the ISO8601 representation of a date.
        /// </summary>
        /// <param name="time">A time.</param>
        /// <param name="cultureInfo">The culture info that will be used to format the time.</param>
        /// <returns>A tsring containing the ISO8601 representation of the time.</returns>
        public static string To12HrFormat(this LocalTime time, CultureInfo cultureInfo)
        {
            var pattern = LocalTimePattern.Create("h:mm tt", cultureInfo);
            return pattern.Format(time);
        }
    }
}
