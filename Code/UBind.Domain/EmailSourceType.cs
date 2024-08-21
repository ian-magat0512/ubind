// <copyright file="EmailSourceType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Email types.
    /// </summary>
    public enum EmailSourceType
    {
        /// <summary>
        /// Email For user.
        /// </summary>
        User = 0,

        /// <summary>
        /// Email For quote.
        /// </summary>
        Quote = 1,

        /// <summary>
        /// Email for policy.
        /// </summary>
        Policy = 2,

        /// <summary>
        /// Email For Renewal Invitation.
        /// </summary>
        RenewalInvitation = 3,
    }
}
