// <copyright file="PolicyDataPatchScope.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Commands
{
    using System;

    /// <summary>
    /// For specifying the scope of the patch, i.e. which entities within the policy to patch.
    /// </summary>
    public class PolicyDataPatchScope
    {
        private PolicyDataPatchScope(PatchScopeType type, Guid? entityId = null, int? versionNumber = null)
        {
            this.Type = type;
            this.EntityId = entityId;
            this.VersionNumber = versionNumber;
        }

        /// <summary>
        /// Gets the type of the patch scope.
        /// </summary>
        public PatchScopeType Type { get; }

        /// <summary>
        /// Gets the ID of the entity the patch is scoped to, if applicable, otherwise null.
        /// </summary>
        public Guid? EntityId { get; }

        /// <summary>
        /// Gets the version of the entity the patch is scoped to, if applicable, otherwise null.
        /// </summary>
        public int? VersionNumber { get; }

        /// <summary>
        /// Create a patch scope for a full quote.
        /// </summary>
        /// <returns>A new instance of <see cref="PolicyDataPatchScope"/>.</returns>
        public static PolicyDataPatchScope CreateGlobalPatchScope()
        {
            return new PolicyDataPatchScope(PatchScopeType.Global);
        }

        /// <summary>
        /// Create a patch scope for a full quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>A new instance of <see cref="PolicyDataPatchScope"/>.</returns>
        public static PolicyDataPatchScope CreateFullQuotePatchScope(Guid quoteId)
        {
            return new PolicyDataPatchScope(PatchScopeType.QuoteFull, quoteId);
        }

        /// <summary>
        /// Create a patch scope for the latest quote data only (excluding versions).
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>A new instance of <see cref="PolicyDataPatchScope"/>.</returns>
        public static PolicyDataPatchScope CreateLatestQuotePatchScope(Guid quoteId)
        {
            return new PolicyDataPatchScope(PatchScopeType.QuoteLatest, quoteId);
        }

        /// <summary>
        /// Create a patch scope for a particular version of a quote.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="versionNumber">The number of the version to patch.</param>
        /// <returns>A new instance of <see cref="PolicyDataPatchScope"/>.</returns>
        public static PolicyDataPatchScope CreateQuoteVersionPatchScope(Guid quoteId, int versionNumber)
        {
            return new PolicyDataPatchScope(PatchScopeType.QuoteVersion, quoteId, versionNumber);
        }

        /// <summary>
        /// Create a patch scope for a policy transaction quote.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction to patch.</param>
        /// <returns>A new instance of <see cref="PolicyDataPatchScope"/>.</returns>
        public static PolicyDataPatchScope CreatePolicyTransactionPatchScope(Guid transactionId)
        {
            return new PolicyDataPatchScope(PatchScopeType.PolicyTransaction, transactionId);
        }

        /// <summary>
        /// Returns a value indicating whether the scope includes a given policy transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns><code>true</code> if the transaction is included, otherwise false.</returns>
        public bool Applicable(Entities.PolicyTransaction transaction)
        {
            return (this.Type == PatchScopeType.Global) ||
                (this.Type == PatchScopeType.PolicyTransaction && this.EntityId == transaction.Id);
        }

        /// <summary>
        /// Returns a value indicating whether the scope includes a given quote.
        /// </summary>
        /// <param name="quote">The transaction.</param>
        /// <returns><code>true</code> if the quote is included, otherwise false.</returns>
        public bool Applicable(Quote quote)
        {
            return (this.Type == PatchScopeType.Global) ||
                ((this.EntityId == quote.Id) &&
                    (this.Type == PatchScopeType.QuoteFull || this.Type == PatchScopeType.QuoteLatest));
        }

        /// <summary>
        /// Returns a value indicating whether the scope includes a given quote version transaction.
        /// </summary>
        /// <param name="quote">The quote.</param>
        /// <param name="quoteVersion">The quote version.</param>
        /// <returns><code>true</code> if the quote version is included, otherwise false.</returns>
        public bool Applicable(Quote quote, QuoteVersion quoteVersion)
        {
            return (this.Type == PatchScopeType.Global) ||
                (this.Type == PatchScopeType.QuoteFull && this.EntityId == quote.Id) ||
                (this.Type == PatchScopeType.QuoteVersion && this.EntityId == quote.Id
                    && this.VersionNumber == quoteVersion.Number);
        }
    }
}
