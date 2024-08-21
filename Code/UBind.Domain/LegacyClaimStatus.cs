// <copyright file="LegacyClaimStatus.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;

    /// <summary>
    /// Old statuses for claims - only used in old events.
    /// </summary>
    [Flags]
    public enum LegacyClaimStatus
    {
        /// <summary>
        /// Default value.
        /// </summary>
        None = 0x0,

        /// <summary>
        /// Indicates whether a claim is new.
        /// </summary>
        New = 0x1,

        /// <summary>
        /// Indicates whether a claim is being processed.
        /// </summary>
        Processing = 0x2,

        /// <summary>
        /// Indicates whether a claim has been accepted.
        /// </summary>
        Accepted = 0x4,

        /// <summary>
        /// Indicates whether a claim has been rejected.
        /// </summary>
        Rejected = 0x8,

        /// <summary>
        /// Indicates whether a claim has been cancelled
        /// </summary>
        Cancelled = 0x10,

        /// <summary>
        /// Indicates whether a claim has a LegacyClaimStatus of New or Processing.
        /// </summary>
        Active = New | Processing,

        /// <summary>
        /// Indicates whether a claim has a LegacyClaimStatus of Accepted or Rejected.
        /// </summary>
        Completed = Accepted | Rejected,

        /// <summary>
        /// Indicates whether a claim has a LegacyClaimStatus of Completed or Cancelled.
        /// </summary>
        CompletedOrCancelled = Completed | Cancelled,
    }
}
