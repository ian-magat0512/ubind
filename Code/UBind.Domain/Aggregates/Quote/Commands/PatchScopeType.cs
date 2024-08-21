// <copyright file="PatchScopeType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Commands
{
    /// <summary>
    /// The type of a patch scope.
    /// </summary>
    public enum PatchScopeType
    {
        /// <summary>
        /// Patch all quotes, quote versions and policy transactions.
        /// </summary>
        Global = 0,

        /// <summary>
        /// Patch a given quote, and all quote versions.
        /// </summary>
        QuoteFull = 1,

        /// <summary>
        /// Patch the latest data in the quote, but leave quote versions.
        /// </summary>
        QuoteLatest = 2,

        /// <summary>
        /// Patch a particular quote version only.
        /// </summary>
        QuoteVersion = 3,

        /// <summary>
        /// Patch a particular policy transaction.
        /// </summary>
        PolicyTransaction = 4,
    }
}
