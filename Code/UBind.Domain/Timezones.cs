// <copyright file="Timezones.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Timezones by name, for convenience.
    /// </summary>
    public static class Timezones
    {
        /// <summary>
        /// List of all timezone providers supported in ubind application.
        /// </summary>
        private static readonly IDictionary<string, DateTimeZone> SupportedDateTimeZones = new Dictionary<string, DateTimeZone>
        {
            { "NSW", DateTimeZoneProviders.Tzdb["Australia/Sydney"] },
            { "VIC", DateTimeZoneProviders.Tzdb["Australia/Melbourne"] },
            { "SA", DateTimeZoneProviders.Tzdb["Australia/Adelaide"] },
            { "QLD", DateTimeZoneProviders.Tzdb["Australia/Brisbane"] },
            { "NT", DateTimeZoneProviders.Tzdb["Australia/Darwin"] },
            { "TAS", DateTimeZoneProviders.Tzdb["Australia/Hobart"] },
            { "WA", DateTimeZoneProviders.Tzdb["Australia/Perth"] },
            { "ACT", DateTimeZoneProviders.Tzdb["Australia/Sydney"] },
        };

        /// <summary>
        /// List of all timezones supported by ubind application.
        /// </summary>
        private static readonly IDictionary<(string state, bool isDaylightSaving), TimeZoneName> TimezoneNames = new Dictionary<(string state, bool isDaylightSaving), TimeZoneName>
        {
            { (state: "NSW", isDaylightSaving: false), new TimeZoneName() { Name = "Australian Eastern Standard Time", Alias = "AEST" } },
            { (state: "VIC", isDaylightSaving: false), new TimeZoneName() { Name = "Australian Eastern Standard Time", Alias = "AEST" } },
            { (state: "WA", isDaylightSaving: false), new TimeZoneName() { Name = "Australian Western Standard Time", Alias = "AWST" } },
            { (state: "SA", isDaylightSaving: false), new TimeZoneName() { Name = "Australian Central Standard Time", Alias = "ACST" } },
            { (state: "TAS", isDaylightSaving: false), new TimeZoneName() { Name = "Australian Eastern Standard Time", Alias = "AEST" } },
            { (state: "QLD", isDaylightSaving: false), new TimeZoneName() { Name = "Australian Eastern Standard Time", Alias = "AEST" } },
            { (state: "ACT", isDaylightSaving: false), new TimeZoneName() { Name = "Australian Eastern Standard Time", Alias = "AEST" } },
            { (state: "NT", isDaylightSaving: false), new TimeZoneName() { Name = "Australian Central Standard Time", Alias = "ACST" } },
            { (state: "NSW", isDaylightSaving: true), new TimeZoneName() { Name = "Australian Eastern Daylight Time", Alias = "AEDT" } },
            { (state: "VIC", isDaylightSaving: true), new TimeZoneName() { Name = "Australian Eastern Daylight Time", Alias = "AEDT" } },
            { (state: "WA", isDaylightSaving: true), new TimeZoneName() { Name = "Australian Western Standard Time", Alias = "AWST" } },
            { (state: "SA", isDaylightSaving: true), new TimeZoneName() { Name = "Australian Central Daylight Time", Alias = "ACDT" } },
            { (state: "TAS", isDaylightSaving: true), new TimeZoneName() { Name = "Australian Eastern Daylight Time", Alias = "AEDT" } },
            { (state: "QLD", isDaylightSaving: true), new TimeZoneName() { Name = "Australian Eastern Standard Time", Alias = "AEST" } },
            { (state: "ACT", isDaylightSaving: true), new TimeZoneName() { Name = "Australian Eastern Daylight Time", Alias = "AEDT" } },
            { (state: "NT", isDaylightSaving: true), new TimeZoneName() { Name = "Australian Central Standard Time", Alias = "ACST" } },
        };

        /// <summary>
        /// Gets the timezone for Australian Eastern Time (i.e. AEST in winter, and AEDT in summer).
        /// </summary>
        public static DateTimeZone AET => DateTimeZoneProviders.Tzdb["Australia/Melbourne"];

        /// <summary>
        /// Get TimeZone by state.
        /// </summary>
        /// <param name="state">The state. e.g. (VIC, NSW, etc).</param>
        /// <returns>The DateTimeZone of the given state.</returns>
        public static DateTimeZone GetTimeZoneByState(string state) => SupportedDateTimeZones[state];

        /// <summary>
        /// Get TimeZone by state.
        /// </summary>
        /// <param name="state">The state. e.g. (VIC, NSW, etc).</param>
        /// <param name="isDaylightSavingTime">The flag for daylight savings time.</param>
        /// <returns>Thetimezone name.</returns>
        public static TimeZoneName GetTimeZoneNameByState(string state, bool isDaylightSavingTime) => TimezoneNames[(state, isDaylightSavingTime)];

        public static DateTimeZone GetTimeZoneByIdOrThrow(string timeZoneId)
        {
            DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            if (zone == null)
            {
                throw new ErrorException(Errors.General.UnknownTimeZoneId(timeZoneId));
            }

            return zone;
        }

        /// <summary>
        /// Gets the timezone by id, but if null returns AET.
        /// </summary>
        public static DateTimeZone GetTimeZoneByIdOrDefault(string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                return AET;
            }

            DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            return zone ?? AET;
        }

        /// <summary>
        /// Gets the timezone by id or null if it's not supported by provider.
        /// </summary>
        public static DateTimeZone GetTimeZoneByIdOrNull(string timeZoneId)
        {
            if (string.IsNullOrEmpty(timeZoneId))
            {
                return null;
            }

            DateTimeZone zone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            return zone;
        }
    }
}
