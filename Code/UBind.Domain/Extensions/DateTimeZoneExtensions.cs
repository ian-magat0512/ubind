// <copyright file="DateTimeZoneExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NodaTime;
    using NodaTime.TimeZones;
    using TimeZoneConverter;

    public static class DateTimeZoneExtensions
    {
        private static readonly Lazy<IDictionary<string, string>> Map = new Lazy<IDictionary<string, string>>(LoadTimeZoneMap, true);

        public static TimeZoneInfo ToTimeZoneInfo(this DateTimeZone timeZone)
        {
            string id = Map.Value.FirstOrDefault(c => c.Value.EqualsIgnoreCase(timeZone.Id)).Key;
            if (string.IsNullOrEmpty(id))
            {
                id = TZConvert.IanaToWindows(timeZone.Id);
            }

            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(id);
            if (timeZoneInfo == null)
            {
                throw new TimeZoneNotFoundException(string.Format("Could not locate time zone with identifier {0}", timeZone.Id));
            }

            return timeZoneInfo;
        }

        private static IDictionary<string, string> LoadTimeZoneMap()
        {
            return TzdbDateTimeZoneSource.Default.WindowsMapping.PrimaryMapping;
        }
    }
}
