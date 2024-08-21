// <copyright file="QuoteStatus.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents the statuses a quote can be in.
    /// </summary>
    public enum QuoteStatus
    {
        /// <summary>
        /// The quote exists, but has not been assigned a number yet.
        /// </summary>
        Nascent = 0,

        /// <summary>
        /// The quote has a quote number, but has not been submitted, nor has a policy been issued yet.
        /// </summary>
        Incomplete,

        /// <summary>
        /// The quote cannot be updated any more.
        /// </summary>
        Complete,

        /// <summary>
        /// The quote is discarded/abandoned and cannot be viewed or updated.
        /// </summary>
        Discarded,
    }
}
