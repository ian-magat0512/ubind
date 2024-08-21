// <copyright file="DateTimeVariants.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Globalization;
    using NodaTime;
    using NodaTime.Text;
    using TimeZoneNames;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Helper class to create variants of a date time which are used in Automation Serialised Entities.
    /// </summary>
    public class DateTimeVariants
    {
        public DateTimeVariants(LocalDateTime localDateTime, DateTimeZone timeZone, Instant? timestamp = null)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(Locales.en_AU);
            var zonedDateTime = localDateTime.InZoneLeniently(timeZone);
            var offsetDateTime = zonedDateTime.ToOffsetDateTime().WithOffset(timeZone.GetUtcOffset(zonedDateTime.ToInstant()));
            this.DateTime = OffsetDateTimePattern.ExtendedIso.Format(offsetDateTime);
            this.Timestamp = timestamp ?? zonedDateTime.ToInstant();
            this.TicksSinceEpoch = this.Timestamp.ToUnixTimeTicks();
            this.Date = localDateTime.Date.ToDMMMYYYY(cultureInfo);
            this.Time = localDateTime.TimeOfDay.To12HrFormat(cultureInfo);

            var tzNames = TZNames.GetNamesForTimeZone(timeZone.Id, Locales.en_AU);
            this.TimeZoneName = zonedDateTime.IsDaylightSavingTime()
                ? tzNames.Daylight
                : tzNames.Standard;

            var tzAbbreviations = TZNames.GetAbbreviationsForTimeZone(timeZone.Id, Locales.en_AU);
            this.TimeZoneAbbreviation = zonedDateTime.IsDaylightSavingTime()
                ? tzAbbreviations.Daylight
                : tzAbbreviations.Standard;

            this.TimeZoneId = timeZone.Id;
        }

        public DateTimeVariants(Instant timestamp, DateTimeZone timeZone)
            : this(timestamp.InZone(timeZone).LocalDateTime, timeZone, timestamp)
        {
        }

        public DateTimeVariants(long ticksSinceEpoch, DateTimeZone timeZone)
            : this(Instant.FromUnixTimeTicks(ticksSinceEpoch), timeZone)
        {
        }

        public Instant Timestamp { get; }

        /// <summary>
        /// Gets an ISO-8601 formatted date time string.
        /// </summary>
        public string DateTime { get; }

        /// <summary>
        /// Gets the number of ticks since the epoch.
        /// </summary>
        public long? TicksSinceEpoch { get; }

        /// <summary>
        /// Gets the local date in the format "d MMM yyyy".
        /// </summary>
        public string Date { get; }

        /// <summary>
        /// Gets the local time in the format "h:mm tt" (12 hour format).
        /// </summary>
        public string Time { get; }

        /// <summary>
        /// Gets the time zone.
        /// </summary>
        public string TimeZoneName { get; }

        /// <summary>
        /// Gets the time zone alias.
        /// </summary>
        public string TimeZoneAbbreviation { get; }

        /// <summary>
        /// Gets the IANA time zone ID.
        /// </summary>
        public string TimeZoneId { get; }

        public static DateTimeVariants CreateFromDateTime(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Utc:
                    return new DateTimeVariants(Instant.FromDateTimeUtc(dateTime), DateTimeZone.Utc);
                case DateTimeKind.Local:
                case DateTimeKind.Unspecified:
                default:
                    var utcDateTime = dateTime.ToUniversalTime();
                    return new DateTimeVariants(Instant.FromDateTimeUtc(utcDateTime), DateTimeZone.Utc);
            }
        }

        public static DateTimeVariants CreateFromDateTimeOffset(DateTimeOffset dateTimeOffset)
        {
            return new DateTimeVariants(
                ZonedDateTime.FromDateTimeOffset(dateTimeOffset).ToInstant(),
                DateTimeZone.Utc);
        }
    }
}
