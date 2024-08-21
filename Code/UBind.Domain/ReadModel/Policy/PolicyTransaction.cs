// <copyright file="PolicyTransaction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Policy
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using Humanizer;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Enums;

    /// <summary>
    /// Base class for policy transactions.
    /// </summary>
    public class PolicyTransaction : EntityReadModel<Guid>, IFormDataHolder, IReadModel<Guid>
    {
        /// <summary>
        /// Initializes the static properties.
        /// </summary>
        static PolicyTransaction()
        {
            SupportsAdditionalProperties = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransaction"/> class.
        /// </summary>
        /// <param name="transactionId">A unique Id for the transaction.</param>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="effectiveDateTime">The effective date of the transaction.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created the transaction.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        /// <param name="quoteId">The ID of the quote related to the policy transaction.</param>
        /// <param name="quoteNumber">The quote number of the quote related to the policy transaction.</param>
        /// <param name="policyData">The policy data.</param>
        protected PolicyTransaction(
            Guid tenantId,
            Guid transactionId,
            Guid policyId,
            int eventSequenceNumber,
            LocalDateTime effectiveDateTime,
            Instant effectiveTimestamp,
            LocalDateTime? expiryDateTime,
            Instant? expiryTimestamp,
            Instant createdTimestamp,
            Guid? quoteId,
            string quoteNumber,
            PolicyTransactionData policyData,
            Guid? productReleaseId,
            TransactionType transactionType)
            : base(tenantId, transactionId, createdTimestamp)
        {
            this.PolicyId = policyId;
            this.EventSequenceNumber = eventSequenceNumber;
            this.EffectiveDateTime = effectiveDateTime;
            this.EffectiveTimestamp = effectiveTimestamp;
            this.ExpiryDateTime = expiryDateTime;
            this.ExpiryTimestamp = expiryTimestamp;
            this.QuoteId = quoteId;
            this.QuoteNumber = quoteNumber;
            this.PolicyData = policyData;
            this.ProductReleaseId = productReleaseId;
            this.Type = transactionType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransaction"/> class.
        /// </summary>
        /// <remarks>
        /// Protected parameterless constructor to allow EF use lazy loading.</remarks>
        protected PolicyTransaction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransaction"/> class.
        /// </summary>
        /// <param name="transactionId">The transaction Id, which is typically the same as the quote ID.</param>
        /// <param name="createdTimestamp">The created time.</param>
        protected PolicyTransaction(Guid tenantId, Guid transactionId, Instant createdTimestamp)
            : base(tenantId, transactionId, createdTimestamp)
        {
        }

        /// <summary>
        /// Gets the ID of the quote related to the policy transaction.
        /// </summary>
        public Guid? QuoteId { get; private set; }

        /// <summary>
        /// Gets the quote number of the quote that resulted in this transaction.
        /// </summary>
        public string QuoteNumber { get; private set; }

        /// <summary>
        /// Gets the data for the policy created or adjusted by this transaction.
        /// </summary>
        public PolicyTransactionData PolicyData { get; private set; }

        /// <summary>
        /// Gets the ID of the policy the transaction is for.
        /// </summary>
        public Guid PolicyId { get; private set; }

        /// <summary>
        /// Gets the sequence number of the event that created the transaction.
        /// </summary>
        public int EventSequenceNumber { get; }

        /// <summary>
        /// Gets the effective date time for this policy transaction.
        /// This means different things for different policy transaction types:
        /// New Business: The inception date and policy period start date.
        /// Renewal: The policy period start date.
        /// Adjustment: The adjustment effective date.
        /// Cancellation: The cancellation effective date.
        /// For policies which are not date based, this should be set to the createdDateTime.
        /// </summary>
        [NotMapped]
        public LocalDateTime EffectiveDateTime
        {
            get => LocalDateTime.FromDateTime(this.EffectiveDateTimeColumn);
            private set => this.EffectiveDateTimeColumn = value.ToDateTimeUnspecified();
        }

        [Column("EffectiveDateTime", TypeName = "datetime2")]
        public DateTime EffectiveDateTimeColumn { get; private set; }

        /// <summary>
        /// Gets the effective timestamp, pre-calculated at the time the transaction was created.
        /// </summary>
        public Instant EffectiveTimestamp
        {
            get => Instant.FromUnixTimeTicks(this.EffectiveTicksSinceEpoch);
            private set => this.EffectiveTicksSinceEpoch = value.ToUnixTimeTicks();
        }

        public long EffectiveTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or the expiry date time of the policy.
        /// Only valid for new business, renewal and adjustment transactions.
        /// For policies which are not date based, this would be null.
        /// </summary>
        [NotMapped]
        public LocalDateTime? ExpiryDateTime
        {
            get => this.ExpiryDateTimeColumn.HasValue
                ? LocalDateTime.FromDateTime(this.ExpiryDateTimeColumn.Value)
                : (LocalDateTime?)null;
            private set => this.ExpiryDateTimeColumn = value?.ToDateTimeUnspecified();
        }

        [Column("ExpiryDateTime", TypeName = "datetime2")]
        public DateTime? ExpiryDateTimeColumn { get; private set; }

        /// <summary>
        /// Gets the expiry timestamp, pre-calculated at the time the transaction was created.
        /// </summary>
        public Instant? ExpiryTimestamp
        {
            get => this.ExpiryTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.ExpiryTicksSinceEpoch.Value)
                : (Instant?)null;
            private set => this.ExpiryTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        public long? ExpiryTicksSinceEpoch { get; private set; }

        /// <summary>
        /// Gets or sets the environment where the quote is created.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the Id of the quote customer.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the Id of quote owner.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the Id of product the quote belongs to.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the Id of organisation the quote belongs to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        public Guid? ProductReleaseId { get; set; }

        public decimal? TotalPayable { get; set; }

        /// <summary>
        /// Gets or sets the type of the policy transaction.
        /// </summary>
        public TransactionType Type { get; set; }

        /// <summary>
        /// Gets a string containing the status of the transaction.
        /// </summary>
        /// <param name="time">The time to get the status at.</param>
        /// <returns>A string containing the status of the transaction.</returns>
        public string GetTransactionStatus(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant time = default)
        {
            if (time == default)
            {
                time = SystemClock.Instance.GetCurrentInstant();
            }

            Instant effectiveTimestamp = areTimestampsAuthoritative
                ? this.EffectiveTimestamp
                : timeZone.AtLeniently(this.EffectiveDateTime).ToInstant();
            Instant? expiryTimestamp = areTimestampsAuthoritative
                ? this.ExpiryTimestamp
                : this.ExpiryDateTime.HasValue
                    ? timeZone.AtLeniently(this.ExpiryDateTime.Value).ToInstant()
                    : (Instant?)null;

            if ((this is NewBusinessTransaction
                || this is RenewalTransaction
                || this is AdjustmentTransaction)
                    && effectiveTimestamp < time
                    && expiryTimestamp > time)
            {
                return PolicyTransactionStatus.Active.Humanize();
            }
            else if (expiryTimestamp <= time || (this is CancellationTransaction && effectiveTimestamp <= time))
            {
                return PolicyTransactionStatus.Complete.Humanize();
            }
            else if (effectiveTimestamp > time)
            {
                return PolicyTransactionStatus.Pending.Humanize();
            }
            else
            {
                return PolicyTransactionStatus.Void.Humanize();
            }
        }

        /// <inheritdoc/>
        public void ApplyPatch(PolicyDataPatch patch)
        {
            if (patch.IsApplicable(this))
            {
                if (patch.Type == DataPatchType.FormData)
                {
                    this.PolicyData.PatchFormDataPropertyValue(patch.Path, patch.Value);
                }
                else if (patch.Type == DataPatchType.CalculationResult)
                {
                    this.PolicyData.PatchCalculationResultPropertyValue(patch.Path, patch.Value);
                }
            }
        }

        /// <summary>
        /// Gets a short summary of the event type for this policy transaction, e.g. "Purchased", "Adjusted", "Renewed", "Cancelled".
        /// </summary>
        /// <returns>A short summary representing the event type.</returns>
        public virtual string GetEventTypeSummary() => string.Empty;
    }
}
