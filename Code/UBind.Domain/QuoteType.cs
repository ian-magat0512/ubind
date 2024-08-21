// <copyright file="QuoteType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Represents the different types of quotes.
    /// </summary>
    public enum QuoteType
    {
        /// <summary>
        /// The quote is new.
        /// </summary>
        NewBusiness = 0,

        /// <summary>
        /// The quote is for a renewal of an existing policy.
        /// </summary>
        Renewal = 1,

        /// <summary>
        /// The quote is for an adjustment of an existing policy.
        /// </summary>
        Adjustment = 2,

        /// <summary>
        /// The quote is for a cancellation of an existing policy.
        /// </summary>
        Cancellation = 3,
    }
}
