// <copyright file="QuoteReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// For projecting read model summaries from the database.
    /// </summary>
    internal class QuoteReadModelSummary : EntityReadModel<Guid>, IQuoteReadModelSummary
    {
        /// <summary>
        /// Gets or sets Quote Id.
        /// </summary>
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets or sets Quote Title.
        /// </summary>
        public string QuoteTitle { get; set; }

        /// <summary>
        /// Gets or sets Policy Id.
        /// </summary>
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets Policy Id.
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the organisation the quote belongs to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        public string OrganisationAlias { get; set; }

        /// <summary>
        /// Gets or sets quote number.
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quotes is discarded or not.
        /// </summary>
        public bool IsDiscarded { get; set; }

        /// <summary>
        /// Gets or sets invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets invoice time.
        /// </summary>
        public Instant InvoiceTimestamp => Instant.FromUnixTimeTicks(this.InvoiceTicksSinceEpoch);

        /// <summary>
        /// Gets quote submission time.
        /// </summary>
        public Instant SubmissionTimestamp => Instant.FromUnixTimeTicks(this.SumbmissionTicksSinceEpoch);

        /// <summary>
        /// Gets policy inception time.
        /// </summary>
        public Instant? PolicyInceptionTimestamp => this.PolicyInceptionTicksSinceEpoch.HasValue
               ? Instant.FromUnixTimeTicks(this.PolicyInceptionTicksSinceEpoch.Value)
               : null;

        /// <summary>
        /// Gets or sets a value indicating whether gets invoice.
        /// </summary>
        public bool IsInvoiced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is paidfor.
        /// </summary>
        public bool IsPaidFor { get; set; }

        /// <summary>
        /// Gets or sets policy number.
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it has been submitted.
        /// </summary>
        public bool IsSubmitted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it is a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets product ID.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <inheritdoc/>
        public string ProductAlias { get; set; }

        /// <summary>
        /// Gets or sets product name.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the User ID of the owner.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets customer ID.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets customer full name.
        /// </summary>
        public string CustomerFullName { get; set; }

        /// <summary>
        /// Gets or sets customer preferredName.
        /// </summary>
        public string CustomerPreferredName { get; set; }

        /// <inheritdoc/>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets created time.
        /// </summary>
        public long SumbmissionTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets invoice time.
        /// </summary>
        public long InvoiceTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets created time.
        /// </summary>
        public long? PolicyInceptionTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets latest calculation result json.
        /// </summary>
        public string SerializedLatestCalculationResult { get; set; }

        /// <summary>
        /// Gets or sets latest calculation result json.
        /// </summary>
        public string LatestFormData { get; set; }

        /// <summary>
        /// Gets or sets policy issue time.
        /// </summary>
        public long PolicyIssuedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets policy Inception date.
        /// </summary>
        public DateTime PolicyInceptionDateTime { get; set; }

        /// <summary>
        /// Gets or sets policy cancellation date.
        /// </summary>
        public DateTime PolicyCancellationEffectiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets policy cancellation time.
        /// </summary>
        public Instant CancellationEffectiveTimestamp { get; set; }

        /// <summary>
        /// Gets or sets policy expiry date.
        /// </summary>
        public DateTime PolicyExpiryDateTime { get; set; }

        /// <summary>
        /// Gets policy expiry date.
        /// </summary>
        public LocalDate PolicyExpiryDate => LocalDate.FromDateTime(this.PolicyExpiryDateTime);

        /// <summary>
        /// Gets policy cancellation date.
        /// </summary>
        public LocalDate CancellationEffectiveDate => LocalDate.FromDateTime(this.PolicyCancellationEffectiveDateTime);

        /// <summary>
        /// Gets policy inception date.
        /// </summary>
        public LocalDate PolicyInceptionDate => LocalDate.FromDateTime(this.PolicyInceptionDateTime);

        /// <summary>
        /// Gets or sets policy expiry time in ticks since the epoch for persistence in EF.
        /// </summary>
        public long? PolicyExpiryTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets policy expiry time in ticks since the epoch for persistence in EF.
        /// </summary>
        public long PolicyCancellationEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the quote's status.
        /// </summary>
        public string QuoteState { get; set; }

        /// <summary>
        /// Gets a value indicating whether a policy has been issued for the quote.
        /// </summary>
        public bool PolicyIssued => this.PolicyNumber != null;

        /// <summary>
        /// Gets or sets the effective cancellation time in ticks, if cancelled, otherwise default.
        /// </summary>
        public long CancellationEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets policy transaction effective time in ticks since the epoch for persistence in EF.
        /// </summary>
        public long? PolicyTransactionEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the policy issue date.
        /// </summary>
        public Instant PolicyIssuedTimestamp => Instant
            .FromUnixTimeTicks(this.PolicyIssuedTicksSinceEpoch);

        /// <summary>
        /// Gets the latest calculation result.
        /// </summary>
        public CalculationResultReadModel LatestCalculationResult => new CalculationResultReadModel(this.SerializedLatestCalculationResult);

        /// <inheritdoc/>
        public LocalDate PolicyEffectiveDate { get; }

        /// <inheritdoc/>
        public Instant PolicyEffectiveTimestamp { get; }

        /// <inheritdoc/>
        public Instant PolicyEffectiveEndTimestamp { get; }

        /// <inheritdoc/>
        public string PaymentGateway { get; set; }

        /// <inheritdoc/>
        public string PaymentResponseJson { get; set; }

        /// <inheritdoc/>
        public Instant? ExpiryTimestamp
        {
            get => this.ExpiryTicksSinceEpoch.HasValue
               ? Instant.FromUnixTimeTicks(this.ExpiryTicksSinceEpoch.Value)
               : (Instant?)null;

            set => this.ExpiryTicksSinceEpoch = value.HasValue
                ? value.Value.ToUnixTimeTicks()
                : (long?)null;
        }

        /// <summary>
        /// Gets or sets in ticks since epoch to allow persisting via EF.
        /// </summary>
        public long? ExpiryTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the last modified time by user in ticks since epoch to allow persisting via EF.
        /// </summary>
        public long? LastModifiedByUserTicksSinceEpoch { get; set; }

        public DateTimeZone TimeZone => this.TimeZoneId != null
            ? Timezones.GetTimeZoneByIdOrThrow(this.TimeZoneId)
            : Timezones.AET;

        public string TimeZoneId { get; private set; }

        /// <inheritdoc/>
        public QuoteType QuoteType { get; set; }

        public LocalDate PolicyCancellationEffectiveDate => LocalDate.FromDateTime(this.PolicyCancellationEffectiveDateTime);

        public Instant PolicyCancellationEffectiveTimestamp => Instant.FromUnixTimeTicks(this.CancellationEffectiveTicksSinceEpoch);

        /// <summary>
        /// Gets or sets policy expiry timestamp.
        /// </summary>
        public Instant? PolicyExpiryTimestamp
        {
            get => this.PolicyExpiryTicksSinceEpoch.HasValue
               ? Instant.FromUnixTimeTicks(this.PolicyExpiryTicksSinceEpoch.Value)
               : null;

            set => this.PolicyExpiryTicksSinceEpoch = value.HasValue
                ? value.Value.ToUnixTimeTicks()
                : null;
        }

        /// <summary>
        /// Gets or sets policy transaction effective timestamp.
        /// </summary>
        public Instant? PolicyTransactionEffectiveTimestamp
        {
            get => this.PolicyTransactionEffectiveTicksSinceEpoch.HasValue
               ? Instant.FromUnixTimeTicks(this.PolicyTransactionEffectiveTicksSinceEpoch.Value)
               : null;

            set => this.PolicyTransactionEffectiveTicksSinceEpoch = value.HasValue
                ? value.Value.ToUnixTimeTicks()
                : null;
        }

        public Guid? ProductReleaseId { get; set; }

        public int? ProductReleaseMajorNumber { get; set; }

        public int? ProductReleaseMinorNumber { get; set; }
    }
}
