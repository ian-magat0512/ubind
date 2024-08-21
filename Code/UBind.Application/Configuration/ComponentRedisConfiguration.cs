// <copyright file="ComponentRedisConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Configuration
{
    public class ComponentRedisConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string to use to connect to the redis instances.
        /// Follows the StackExhange.Redis format: https://stackexchange.github.io/StackExchange.Redis/Configuration.
        /// <remarks>It's not necessary to set this per component. It will use the connection string from the Redis
        /// configuration more generally if you don't, which is what you should do.</remarks>
        /// </summary>
        public string? ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the prefix for all redis keys related to the component.
        /// </summary>
        public string Prefix { get; set; }
    }
}
