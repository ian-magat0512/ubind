// <copyright file="IRedisConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Configuration;

public interface IRedisConfiguration
{
    /// <summary>
    /// Gets or sets the connection string to use to connect to the redis instances.
    /// Follows the StackExhange.Redis format: https://stackexchange.github.io/StackExchange.Redis/Configuration.
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the prefix for all redis keys related to ubind.
    /// Defaults to "ubind:" if not set.
    /// </summary>
    string Prefix { get; set; }

    /// <summary>
    /// Gets or sets the flag that enables/disables debugging logs.
    /// Defaults to false (disable).
    /// </summary>
    bool DebuggingLog { get; set; }
}
