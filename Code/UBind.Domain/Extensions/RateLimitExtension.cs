// <copyright file="RateLimitExtension.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;
    using Humanizer;
    using UBind.Domain.Enums;

    public static class RateLimitExtension
    {
        public static int RemainingPeriod(this DateTime timestamp, int period)
        {
            return CalculatePeriod(timestamp, period);
        }

        public static int RetryAfterFrom(this DateTime timestamp, int period)
        {
            return CalculatePeriod(timestamp, period);
        }

        public static TimeSpan ToTimeSpan(this int value, RateLimitPeriodType periodType)
        {
            switch (periodType)
            {
                case RateLimitPeriodType.Days:
                    return TimeSpan.FromDays(value);
                case RateLimitPeriodType.Hours:
                    return TimeSpan.FromHours(value);
                case RateLimitPeriodType.Minutes:
                    return TimeSpan.FromMinutes(value);
                case RateLimitPeriodType.Seconds:
                    return TimeSpan.FromSeconds(value);
                default:
                    throw new FormatException($"{value} can't be converted to TimeSpan, unknown type {periodType.Humanize()}");
            }
        }

        public static string ToMessage(this int value, string message)
        {
            return $"{value} {(value > 2 ? message.Pluralize(false) : message.Singularize(false))}";
        }

        private static int CalculatePeriod(DateTime timestamp, int period)
        {
            var diff = timestamp - DateTime.UtcNow;
            var seconds = period - Math.Abs(diff.TotalSeconds);

            return int.Parse($"{seconds:F0}");
        }
    }
}
