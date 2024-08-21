// <copyright file="PolicyReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// For projecting read model summaries from the database.
    /// </summary>
    public class PolicyReadModelSummary : EntityReadModel<Guid>, IPolicyReadModelSummary
    {
        private CalculationResultReadModel calculationResult;

        /// <summary>
        /// Gets policy Id.
        /// </summary>
        public Guid PolicyId => this.Id;

        /// <summary>
        /// Gets or sets Aggregate Id.
        /// </summary>
        public Guid AggregateId { get; set; }

        /// <summary>
        /// Gets or sets Policy Title.
        /// </summary>
        public string PolicyTitle { get; set; }

        /// <summary>
        /// Gets or sets Policy State.
        /// </summary>
        public string PolicyState { get; set; }

        /// <summary>
        /// Gets or sets Quote Id.
        /// </summary>
        public Guid? QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the organisation the quote belongs to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets quote number.
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets invoice time.
        /// </summary>
        public Instant InvoiceTimestamp { get; set; }

        /// <summary>
        /// Gets or sets quote submission time.
        /// </summary>
        public Instant SubmissionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the product feature setting.
        /// </summary>
        public ProductFeatureSetting ProductFeatureSetting { get; set; }

        /// <summary>
        /// Gets or sets policy inception time.
        /// </summary>
        public Instant PolicyInceptionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote has been invoice.
        /// </summary>
        public bool IsInvoiced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is paid for.
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

        /// <summary>
        /// Gets or sets environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets policy issue time.
        /// </summary>
        public long IssuedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets policy Inception date.
        /// </summary>
        public LocalDateTime InceptionDateTime
        {
            get => LocalDateTime.FromDateTime(this.InceptionDateTimeColumn);
            set => this.InceptionDateTimeColumn = value.ToDateTimeUnspecified();
        }

        public DateTime InceptionDateTimeColumn { get; set; }

        /// <inheritdoc/>
        public Instant InceptionTimestamp
        {
            get => Instant.FromUnixTimeTicks(this.InceptionTicksSinceEpoch);
            set => value.ToUnixTimeTicks();
        }

        public long InceptionTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the local date time of the start of the current policy period.
        /// </summary>
        public LocalDateTime LatestPolicyPeriodStartDateTime
        {
            get => LocalDateTime.FromDateTime(this.LatestPolicyPeriodStartDateTimeColumn);
            set => this.LatestPolicyPeriodStartDateTimeColumn = value.ToDateTimeUnspecified();
        }

        public DateTime LatestPolicyPeriodStartDateTimeColumn { get; set; }

        /// <inheritdoc/>
        public Instant LatestPolicyPeriodStartTimestamp
        {
            get => Instant.FromUnixTimeTicks(this.LatestPolicyPeriodStartTicksSinceEpoch);
            set => value.ToUnixTimeTicks();
        }

        public long LatestPolicyPeriodStartTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets policy cancellation date.
        /// </summary>
        public LocalDateTime? CancellationEffectiveDateTime
        {
            get => this.CancellationEffectiveDateTimeColumn.HasValue
                ? LocalDateTime.FromDateTime(this.CancellationEffectiveDateTimeColumn.Value)
                : (LocalDateTime?)null;
            set => this.CancellationEffectiveDateTimeColumn = value?.ToDateTimeUnspecified();
        }

        public DateTime? CancellationEffectiveDateTimeColumn { get; set; }

        /// <inheritdoc/>
        public Instant? CancellationEffectiveTimestamp
        {
            get => this.CancellationEffectiveTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.CancellationEffectiveTicksSinceEpoch.Value)
                : (Instant?)null;
            set => value?.ToUnixTimeTicks();
        }

        public long? CancellationEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets policy adjustment date time.
        /// </summary>
        public LocalDateTime? AdjustmentEffectiveDateTime
        {
            get => this.AdjustmentEffectiveDateTimeColumn.HasValue
                ? LocalDateTime.FromDateTime(this.AdjustmentEffectiveDateTimeColumn.Value)
                : (LocalDateTime?)null;
            set => this.AdjustmentEffectiveDateTimeColumn = value?.ToDateTimeUnspecified();
        }

        public DateTime? AdjustmentEffectiveDateTimeColumn { get; set; }

        /// <inheritdoc/>
        public Instant? AdjustmentEffectiveTimestamp
        {
            get => this.AdjustmentEffectiveTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.AdjustmentEffectiveTicksSinceEpoch.Value)
                : (Instant?)null;
            set => value?.ToUnixTimeTicks();
        }

        public long? AdjustmentEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets policy expiry date.
        /// </summary>
        public LocalDateTime? ExpiryDateTime
        {
            get => this.ExpiryDateTimeColumn.HasValue
                ? LocalDateTime.FromDateTime(this.ExpiryDateTimeColumn.Value)
                : (LocalDateTime?)null;
            set => this.ExpiryDateTimeColumn = value?.ToDateTimeUnspecified();
        }

        public DateTime? ExpiryDateTimeColumn { get; set; }

        /// <inheritdoc/>
        public Instant? ExpiryTimestamp
        {
            get => this.ExpiryTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.ExpiryTicksSinceEpoch.Value)
                : (Instant?)null;
            set => value?.ToUnixTimeTicks();
        }

        public long? ExpiryTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the quote's status.
        /// </summary>
        public string QuoteState { get; set; }

        /// <summary>
        /// Gets the policy issue date.
        /// </summary>
        public Instant IssuedTimestamp => Instant.FromUnixTimeTicks(this.IssuedTicksSinceEpoch);

        /// <summary>
        /// Gets or sets the policy's latest renewal effective time.
        /// </summary>
        public long? LatestRenewalEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the policy's effective latest renewal time.
        /// </summary>
        public Instant? LatestRenewalEffectiveTimestamp => this.LatestRenewalEffectiveTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.LatestRenewalEffectiveTicksSinceEpoch.Value)
                : (Instant?)null;

        /// <summary>
        /// Gets or sets a string containing the serialized calculation result used for the policy.
        /// </summary>
        public string SerializedCalculationResult { get; set; }

        /// <summary>
        /// Gets the calculation result for the policy.
        /// </summary>
        [NotMapped]
        public CalculationResultReadModel CalculationResult
        {
            get
            {
                if (this.calculationResult == null)
                {
                    this.calculationResult = new CalculationResultReadModel(this.SerializedCalculationResult);
                }

                return this.calculationResult;
            }
        }

        /// <inheritdoc/>
        public bool IsAdjusted { get; }

        /// <inheritdoc/>
        public QuoteType QuoteType { get; set; }

        /// <inheritdoc/>
        public bool IsTermBased => this.ExpiryTimestamp != null;

        public DateTimeZone TimeZone => this.TimeZoneId != null
            ? Timezones.GetTimeZoneByIdOrThrow(this.TimeZoneId)
            : Timezones.AET;

        public string TimeZoneId { get; set; }

        public bool AreTimestampsAuthoritative { get; set; }
    }
}
