// <copyright file="PolicyStatus.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;

    /// <summary>
    /// Represents a generic status.
    /// </summary>
    [Flags]
    public enum PolicyStatus
    {
        /// <summary>
        /// Default value.
        /// </summary>
        None = 0,

        /// <summary>
        /// The policy's inception date is in the future.
        /// </summary>
        Issued = 1 << 0,

        /// <summary>
        /// For backward compatibility only, because some users might still have Pending in their cache.
        /// You should use PolicyStatus.Issued in your code instead.
        /// </summary>
        Pending = Issued,

        /// <summary>
        /// The policy's inception date has arrived, but its expiry date has not.
        /// </summary>
        Active = 1 << 1,

        /// <summary>
        /// The policy's expiry date has been passed.
        /// </summary>
        Expired = 1 << 2,

        /// <summary>
        /// The policy has been cancelled.
        /// </summary>
        Cancelled = 1 << 3,

        /// <summary>
        /// The policy has been adjusted.
        /// </summary>
        Adjusted = 1 << 4,

        /// <summary>
        /// Indicates whether the policy has a PolicyStatus of Issued or Active.
        /// </summary>
        IssuedOrActive = Issued | Active,

        /// <summary>
        /// Indicates whether the policy has a PolicyStatus of Expired or Cancelled.
        /// </summary>
        ExpiredOrCancelled = Expired | Cancelled,

        /// <summary>
        /// Any status.
        /// </summary>
        Any = IssuedOrActive | ExpiredOrCancelled | Adjusted,
    }
}
