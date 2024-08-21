// <copyright file="PolicyReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Humanizer;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// For projecting read model summaries from the database.
    /// </summary>
    internal class PolicyReadModelDetails : PolicyReadModelSummary, IPolicyReadModelDetails
    {
        /// <inheritdoc/>
        public string OrganisationName { get; set; }

        /// <inheritdoc/>
        public string OwnerFullName { get; set; }

        /// <inheritdoc/>
        public IEnumerable<QuoteDocumentReadModel> Documents { get; set; }

        /// <inheritdoc/>
        public string LatestFormData => this.Transactions.FirstOrDefault()?.PolicyTransaction.PolicyData.FormData;

        /// <inheritdoc/>
        public IEnumerable<PolicyTransactionAndQuote> Transactions { get; set; }

        /// <inheritdoc/>
        public Guid? CustomerOwnerUserId { get; set; }

        /// <inheritdoc/>
        public Guid? QuoteOwnerUserId { get; set; }

        /// <inheritdoc/>
        public Guid? CustomerPersonId { get; set; }

        public Guid? DisplayTransactionReleaseId => this.Transactions
            .Where(ptq => ptq.PolicyTransaction is NewBusinessTransaction || ptq.PolicyTransaction is RenewalTransaction)
            .OrderByDescending(ptq => ptq.PolicyTransaction.CreatedTicksSinceEpoch)
            .FirstOrDefault()?.PolicyTransaction.ProductReleaseId;

        /// <inheritdoc/>
        public PolicyTransaction GetCurrentTransaction(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant asAtTimestamp)
        {
            var transactions = this.Transactions.Select(x => x.PolicyTransaction);
            return transactions
                    .OfType<PolicyTransaction>()
                    .OrderBy(t => t.EffectiveTimestamp)
                    .OrderBy(t => t.CreatedTimestamp)
                    .Where(t => areTimestampsAuthoritative
                        ? t.EffectiveTimestamp < asAtTimestamp
                        : t.EffectiveDateTime.InZoneLeniently(timeZone).ToInstant() < asAtTimestamp)
                    .LastOrDefault()
                ?? transactions
                    .OfType<PolicyTransaction>()
                    .OrderBy(t => t.EffectiveTimestamp)
                    .OrderBy(t => t.CreatedTimestamp)
                    .LastOrDefault();
        }

        /// <inheritdoc/>
        public PolicyTransaction GetDisplayTransaction(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant asAtTimestamp)
        {
            // Return the latest transaction excluding cancellation transaction whose effective time has already been reached
            // or the first transaction if none has been reached (i.e. new business transaction
            // for pending policy).
            var transactions = this.Transactions.Select(x => x.PolicyTransaction);
            return transactions
                    .OfType<PolicyTransaction>()
                    .OrderBy(t => t.EffectiveTimestamp)
                    .Where(t => areTimestampsAuthoritative
                        ? t.EffectiveTimestamp < asAtTimestamp
                        : t.EffectiveDateTime.InZoneLeniently(timeZone).ToInstant() < asAtTimestamp)
                    .Where(t => t.GetType() != typeof(CancellationTransaction))
                    .LastOrDefault()
                ?? transactions
                    .OfType<PolicyTransaction>()
                    .OrderBy(t => t.EffectiveTimestamp)
                    .Where(t => t.GetType() != typeof(CancellationTransaction))
                    .First();
        }

        /// <inheritdoc/>
        public string GetDetailStatus(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant asAtTimestamp)
        {
            var transactions = this.Transactions.Select(x => x.PolicyTransaction);
            var newBusinessTransaction = transactions.OrderBy(t => t.CreatedTimestamp).First();
            if (this.CancellationEffectiveDateTime != null && this.CancellationEffectiveTimestamp != null)
            {
                var hasPendingCancellation = transactions
                    .OfType<CancellationTransaction>()
                    .Where(t => areTimestampsAuthoritative
                        ? t.EffectiveTimestamp > asAtTimestamp
                        : t.EffectiveDateTime.InZoneLeniently(timeZone).ToInstant() > asAtTimestamp)
                    .Any();

                if (hasPendingCancellation
                    && (areTimestampsAuthoritative
                        ? newBusinessTransaction.EffectiveTimestamp < asAtTimestamp
                        : newBusinessTransaction.EffectiveDateTime.InZoneLeniently(timeZone).ToInstant() < asAtTimestamp))
                {
                    return PolicyStatus.Active.Humanize();
                }

                bool isFullTermCancellation = areTimestampsAuthoritative
                    ? this.CancellationEffectiveTimestamp == newBusinessTransaction.EffectiveTimestamp
                    : this.CancellationEffectiveDateTime.Value == newBusinessTransaction.EffectiveDateTime;

                bool hasCancellatationHappened = areTimestampsAuthoritative
                    ? this.CancellationEffectiveTimestamp <= asAtTimestamp
                    : this.CancellationEffectiveDateTime.Value.InZoneLeniently(timeZone).ToInstant() < asAtTimestamp;

                if (isFullTermCancellation || hasCancellatationHappened)
                {
                    return PolicyStatus.Cancelled.Humanize();
                }
            }

            var isNewBusinessTransaction = newBusinessTransaction is NewBusinessTransaction;
            if (isNewBusinessTransaction)
            {
                if (areTimestampsAuthoritative
                    ? newBusinessTransaction.EffectiveTimestamp > asAtTimestamp
                    : newBusinessTransaction.EffectiveDateTime.InZoneLeniently(timeZone).ToInstant() > asAtTimestamp)
                {
                    return PolicyStatus.Issued.Humanize();
                }
            }

            if (areTimestampsAuthoritative
                ? this.ExpiryTimestamp < asAtTimestamp
                : this.ExpiryDateTime.HasValue && this.ExpiryDateTime.Value.InZoneLeniently(timeZone).ToInstant() < asAtTimestamp)
            {
                return PolicyStatus.Expired.Humanize();
            }

            return PolicyStatus.Active.Humanize();
        }
    }
}
