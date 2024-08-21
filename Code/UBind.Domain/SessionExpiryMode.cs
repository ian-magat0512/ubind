// <copyright file="SessionExpiryMode.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

/// <summary>
/// Modes for how user sessions can expire.
/// </summary>
public enum SessionExpiryMode
{
    /// <summary>
    /// The session will expire after a fixed period of time, e.g. 30 days.
    /// </summary>
    FixedPeriod = 0,

    /// <summary>
    /// The session will expire after a period of inactivity, e.g. 15 minutes.
    /// </summary>
    InactivityPeriod = 1,
}
