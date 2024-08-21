// <copyright file="QuoteEmailGeneratedEventType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Quote Email Generated Event Type.
    /// </summary>
    public enum QuoteEmailGeneratedEventType
    {
        /// <summary>
        /// Quote Email Generated Event Type for policy.
        /// </summary>
        Policy = 0,

        /// <summary>
        /// Quote Email Generated Event Type for quote.
        /// </summary>
        Quote = 1,
    }
}
