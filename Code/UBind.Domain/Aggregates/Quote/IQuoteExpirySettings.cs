// <copyright file="IQuoteExpirySettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    /// <summary>
    /// For providing product-specific quote expiry settings.
    /// </summary>
    public interface IQuoteExpirySettings
    {
        /// <summary>
        /// Gets a value indicating whether check if it is enabled.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets a value indicating whether the value of expiry days to be appended to quotes.
        /// </summary>
        int ExpiryDays { get; }
    }
}
