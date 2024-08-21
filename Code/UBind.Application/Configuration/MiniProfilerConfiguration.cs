// <copyright file="MiniProfilerConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Configuration
{
    public class MiniProfilerConfiguration
    {
        public bool Enabled { get; set; } = true;

        public string Storage { get; set; } = "Redis";

        public ComponentRedisConfiguration? Redis { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only profile requests from IP addresses (and ranges) that are
        /// whitelisted in appsettings.json.
        /// Defaults to true.
        /// </summary>
        public bool OnlyProfileWhitelistedIpAddresses { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to profile Entity Framework 6 database queries.
        /// </summary>
        public bool ProfileEntityFramework6 { get; set; }

        public int CacheDurationMinutes { get; set; } = 1440;

        public int ResultListMaxLength { get; set; } = 5000;
    }
}
