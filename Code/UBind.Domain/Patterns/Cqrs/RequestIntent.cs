// <copyright file="RequestIntent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    /// <summary>
    /// Represents whether the current request intends only to read, or intends to read and write.
    /// The request intent determines which database instance the query is directed to. If it's
    /// ReadOnly, it may be routed to a read only replica, for performance and load balancing reasons.
    /// </summary>
    public enum RequestIntent
    {
        ReadOnly = 1,
        ReadWrite = 2,
    }
}
