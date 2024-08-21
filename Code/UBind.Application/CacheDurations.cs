// <copyright file="CacheDurations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    /// <summary>
    /// Constants for Cache Durations.
    /// </summary>
    public static class CacheDurations
    {
        /// <summary>
        /// One Day Cache Duration.
        /// </summary>
        public const string OneDayCache = "public,max-age=86400";

        /// <summary>
        /// No Cache.
        /// </summary>
        public const string NoCache = "public,no-cache";

        /// <summary>
        /// Max duration by max int value.
        /// </summary>
        public const string MaxDuration = "public,max-age=2147483647";
    }
}
