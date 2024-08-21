// <copyright file="TimeZoneResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System;
    using System.Collections.Concurrent;
    using Hangfire;

    /// <summary>
    /// This class is needed because we need to translate IANA timezone to windows timezone.
    /// If the timezone has no equivalent windows timezone, we will use custom timezone instead.
    /// </summary>
    public class TimeZoneResolver : ITimeZoneResolver
    {
        private static ConcurrentDictionary<string, string> customTimeZone = new ConcurrentDictionary<string, string>();

        public static void AddCustomTimeZone(TimeZoneInfo timeZone)
        {
            if (!customTimeZone.ContainsKey(timeZone.StandardName))
            {
                customTimeZone.TryAdd(timeZone.StandardName, timeZone.ToSerializedString());
            }
        }

        public TimeZoneInfo GetTimeZoneById(string timeZoneId)
        {
            var timeZone = default(TimeZoneInfo);
            try
            {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch
            {
                // do nothing. it means the timeZoneId is a custom time zone.
            }

            if (timeZone == default)
            {
                if (customTimeZone.ContainsKey(timeZoneId))
                {
                    timeZone = TimeZoneInfo.FromSerializedString(customTimeZone[timeZoneId]);
                }
            }

            return timeZone;
        }
    }
}
