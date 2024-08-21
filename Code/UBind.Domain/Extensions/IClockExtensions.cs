// <copyright file="IClockExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using NodaTime;

    /// <summary>
    /// Extension methods for <see cref="IClock"/>.
    /// </summary>
    public static class IClockExtensions
    {
        /// <summary>
        /// Shorter method for getting the current time.
        /// </summary>
        /// <param name="clock">An instance of <see cref="IClock"/>.</param>
        /// <returns>A new instance of <see cref="Instant"/> representing the current time.</returns>
        public static Instant Now(this IClock clock)
        {
            return clock.GetCurrentInstant();
        }

        /// <summary>
        /// Convenient method for getting the current date.
        /// </summary>
        /// <param name="clock">An instance of <see cref="IClock"/>.</param>
        /// <param name="timezone">The timezone to get the date in (defaults to AET).</param>
        /// <returns>The current date in the specified timezone, or AET timezone if no timezone is specified.</returns>
        public static LocalDate Today(this IClock clock, DateTimeZone timezone = null)
        {
            if (timezone == null)
            {
                timezone = Timezones.AET;
            }

            return clock.GetCurrentInstant().InZone(Timezones.AET).Date;
        }
    }
}
