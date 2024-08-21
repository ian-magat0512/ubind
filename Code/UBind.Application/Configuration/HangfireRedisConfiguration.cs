// <copyright file="HangfireRedisConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Configuration
{
    public class HangfireRedisConfiguration : ComponentRedisConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum visible background jobs in the succeed list to prevent it from growing indefinitely.
        /// </summary>
        public int MaxSucceededListLength { get; set; }
    }
}