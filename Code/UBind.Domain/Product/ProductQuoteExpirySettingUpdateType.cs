// <copyright file="ProductQuoteExpirySettingUpdateType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    /// <summary>
    /// Product quote expiry settings update type.
    /// </summary>
    public enum ProductQuoteExpirySettingUpdateType
    {
        /// <summary>
        /// Update no quotes.
        /// </summary>
        UpdateNone,

        /// <summary>
        /// Update quotes that do not have an expiry date.
        /// </summary>
        UpdateAllWithoutExpiryOnly,

        /// <summary>
        /// Update quotes any quotes whose expiry dates have not been set manually.
        /// </summary>
        UpdateAllExceptExplicitSet,

        /// <summary>
        /// Update all quotes.
        /// </summary>
        UpdateAllQuotes,
    }
}
