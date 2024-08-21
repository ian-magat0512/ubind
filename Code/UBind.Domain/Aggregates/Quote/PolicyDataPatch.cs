// <copyright file="PolicyDataPatch.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Json;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Model that contains the information about policy data correction.
    /// </summary>
    public class PolicyDataPatch
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDataPatch"/> class.
        /// </summary>
        /// <param name="type">The type of data that is to be patched.</param>
        /// <param name="path">The path of the data modification.</param>
        /// <param name="value">The value of the data modification.</param>
        /// <param name="targets">The targets for the patch.</param>
        public PolicyDataPatch(DataPatchType type, JsonPath path, JToken value, IEnumerable<DataPatchTargetEntity> targets)
        {
            this.Type = type;
            this.Path = path;
            this.Value = value;
            this.Targets.AddRange(targets);
        }

        /// <summary>
        /// Gets the type of data the patch is for.
        /// </summary>
        [JsonProperty]
        public DataPatchType Type { get; private set; }

        /// <summary>
        /// Gets specification of which entities should be patched.
        /// </summary>
        [JsonProperty]
        public List<DataPatchTargetEntity> Targets { get; private set; } = new List<DataPatchTargetEntity>();

        /// <summary>
        /// Gets the path of the data modification.
        /// </summary>
        [JsonProperty]
        public JsonPath Path { get; private set; }

        /// <summary>
        /// Gets the path of the data modification.
        /// </summary>
        [JsonProperty]
        public JToken Value { get; private set; }

        /// <summary>
        /// Add a target to the patch.
        /// </summary>
        /// <param name="target">The target to add.</param>
        public void AddTarget(DataPatchTargetEntity target)
        {
            this.Targets.Add(target);
        }

        /// <summary>
        /// Determine whether the patch is applicable to a given quote.
        /// </summary>
        /// <param name="quote">The quote.</param>
        /// <returns><code>true</code> if the patch is applicable to the quote, otherwise false.</returns>
        public bool IsApplicable(Quote quote)
        {
            return this.Targets
                .OfType<QuotDataPatchTarget>()
                .Any(target => target.QuoteId == quote.Id);
        }

        /// <summary>
        /// Determine whether the patch is applicable to a given quote.
        /// </summary>
        /// <param name="policy">The policy.</param>
        /// <returns><code>true</code> if the patch is applicable to the quote, otherwise false.</returns>
        public bool IsApplicable(Policy policy)
        {
            return this.Targets
                .OfType<QuotDataPatchTarget>()
                .Any(target => target.QuoteId == policy.QuoteId);
        }

        /// <summary>
        /// Determine whether the patch is applicable to a given policyReadModel.
        /// </summary>
        /// <param name="policyReadModel">The policy read model.</param>
        /// <returns><code>true</code> if the patch is applicable to the policy read model, otherwise false.</returns>
        public bool IsApplicable(PolicyReadModel policyReadModel)
        {
            return this.Targets
                .OfType<QuotDataPatchTarget>()
                .Any(target => target.QuoteId == policyReadModel.QuoteId);
        }

        /// <summary>
        /// Determine whether the patch is applicable to a given policyReadModel.
        /// </summary>
        /// <param name="quoteVersionReadModel">The quote version read model.</param>
        /// <returns><code>true</code> if the patch is applicable to the quote version read model, otherwise false.</returns>
        public bool IsApplicable(QuoteVersionReadModel quoteVersionReadModel)
        {
            return this.Targets
                .OfType<QuoteVersionDataPatchTarget>()
                .Any(target => target.QuoteId == quoteVersionReadModel.QuoteId &&
                    target.VersionNumber == quoteVersionReadModel.QuoteVersionNumber);
        }

        /// <summary>
        /// Determine whether the patch is applicable to a given quote.
        /// </summary>
        /// <param name="quote">The quote the version belongs to.</param>
        /// <param name="quoteVersion">The quote.</param>
        /// <returns><code>true</code> if the patch is applicable to the quote, otherwise false.</returns>
        public bool IsApplicable(Quote quote, QuoteVersion quoteVersion)
        {
            return this.Targets
                .OfType<QuoteVersionDataPatchTarget>()
                .Any(target => target.QuoteId == quote.Id && target.VersionNumber == quoteVersion.Number);
        }

        /// <summary>
        /// Determine whether the patch is applicable to a given policy transaction.
        /// </summary>
        /// <param name="policyTransaction">The quote.</param>
        /// <returns><code>true</code> if the patch is applicable to the quote, otherwise false.</returns>
        public bool IsApplicable(Entities.PolicyTransaction policyTransaction)
        {
            return this.Targets
                .OfType<PolicyTransactionDataPatchTarget>()
                .Any(target => target.TransactionId == policyTransaction.Id);
        }

        /// <summary>
        /// Determine whether the patch is applicable to a given policy transaction.
        /// </summary>
        /// <param name="policyTransaction">The quote.</param>
        /// <returns><code>true</code> if the patch is applicable to the quote, otherwise false.</returns>
        public bool IsApplicable(ReadModel.Policy.PolicyTransaction policyTransaction)
        {
            return this.Targets
                .OfType<PolicyTransactionDataPatchTarget>()
                .Any(target => target.TransactionId == policyTransaction.Id);
        }
    }
}
