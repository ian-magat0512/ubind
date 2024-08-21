// <copyright file="Policy.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable IDE0060 // Remove unused parameter

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using Humanizer;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Record of a policy created from an application.
    /// </summary>
    public class Policy : IPolicy
    {
        private List<Entities.PolicyTransaction> transactions;

        [System.Text.Json.Serialization.JsonConstructor]
        public Policy(
            Guid policyId,
            Guid? quoteId,
            Instant createdTimestamp,
            DateTimeZone timeZone,
            string policyNumber,
            CalculationResult calculationResult,
            QuoteDataSnapshot dataSnapshot,
            LocalDateTime inceptionDateTime,
            Instant inceptionTimestamp,
            LocalDateTime? cancellationEffectiveDateTime,
            Instant? cancellationEffectiveTimestamp,
            LocalDateTime? adjustmentEffectiveDateTime,
            Instant? adjustmentEffectiveTimestamp,
            LocalDateTime? expiryDateTime,
            Instant? expiryTimestamp,
            LocalDateTime latestPolicyPeriodStartDateTime,
            Instant latestPolicyPeriodStartTimestamp,
            bool isFromImport,
            bool areTimestampsAuthoritative,
            List<Entities.PolicyTransaction> transactions)
        {
            this.PolicyId = policyId;
            this.QuoteId = quoteId;
            this.CreatedTimestamp = createdTimestamp;
            this.TimeZone = timeZone;
            this.PolicyNumber = policyNumber;
            this.CalculationResult = calculationResult;
            this.InceptionDateTime = inceptionDateTime;
            this.InceptionTimestamp = inceptionTimestamp;
            this.CancellationEffectiveDateTime = cancellationEffectiveDateTime;
            this.CancellationEffectiveTimestamp = cancellationEffectiveTimestamp;
            this.AdjustmentEffectiveDateTime = adjustmentEffectiveDateTime;
            this.AdjustmentEffectiveTimestamp = adjustmentEffectiveTimestamp;
            this.ExpiryDateTime = expiryDateTime;
            this.ExpiryTimestamp = expiryTimestamp;
            this.LatestPolicyPeriodStartDateTime = latestPolicyPeriodStartDateTime;
            this.LatestPolicyPeriodStartTimestamp = latestPolicyPeriodStartTimestamp;
            this.DataSnapshot = dataSnapshot;
            this.IsFromImport = isFromImport;
            this.AreTimestampsAuthoritative = areTimestampsAuthoritative;
            this.transactions = transactions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Policy"/> class.
        /// </summary>
        /// <param name="event">A policy issuing event.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event within its aggregate.</param>
        /// <param name="quoteAggregate">The quote aggregate that this policy is a part of.</param>
        public Policy(QuoteAggregate.PolicyIssuedEvent @event, int eventSequenceNumber, QuoteAggregate quoteAggregate)
        {
            this.Aggregate = quoteAggregate;
            this.QuoteId = @event.QuoteId;

            // TODO: for now we set the PolicyId to the same as the QuoteAggregate ID,
            // although in future we may update the PolicyIssuedEvent to include a unique PolicyID.
            // We don't have that yet though.
            this.PolicyId = quoteAggregate.Id;
            this.HandleUpsert(@event);
            this.PolicyNumber = @event.PolicyNumber;
            this.InceptionDateTime = @event.InceptionDateTime;
            this.InceptionTimestamp = @event.InceptionTimestamp;
            this.LatestPolicyPeriodStartDateTime = @event.InceptionDateTime;
            this.LatestPolicyPeriodStartTimestamp = @event.InceptionTimestamp;
            this.ExpiryDateTime = @event.ExpiryDateTime;
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            this.CreatedTimestamp = @event.Timestamp;
            this.TimeZone = Timezones.GetTimeZoneByIdOrDefault(@event.TimeZoneId);
            var newBusinessTransaction =
                new Entities.NewBusinessPolicyTransaction(@event.NewBusinessTransactionId, @event, eventSequenceNumber);
            this.transactions = new List<Entities.PolicyTransaction>()
            {
                newBusinessTransaction,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Policy"/> class.
        /// </summary>
        /// <param name="event">The policy imported event.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created the transaction.</param>
        /// <param name="quoteAggregate">The quote aggregate that this policy is a part of.</param>
        public Policy(QuoteAggregate.PolicyImportedEvent @event, int eventSequenceNumber, QuoteAggregate quoteAggregate)
        {
            this.Aggregate = quoteAggregate;
            this.IsFromImport = true;
            this.QuoteId = @event.QuoteId;

            // QuoteId is same with PolicyId on this event.
            this.PolicyId = @event.QuoteId.Value;
            this.InceptionDateTime = @event.InceptionDateTime;
            this.InceptionTimestamp = @event.InceptionTimestamp;
            this.LatestPolicyPeriodStartDateTime = @event.InceptionDateTime;
            this.LatestPolicyPeriodStartTimestamp = @event.EffectiveTimestamp;
            this.ExpiryDateTime = @event.ExpiryDateTime;
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            this.CalculationResult = @event.PolicyData.QuoteDataSnapshot.CalculationResult.Data;
            this.DataSnapshot = @event.PolicyData.QuoteDataSnapshot;
            this.PolicyNumber = @event.PolicyNumber;
            this.CreatedTimestamp = @event.Timestamp;
            this.TimeZone = Timezones.GetTimeZoneByIdOrDefault(@event.TimeZoneId);
            var newBusinessTransaction =
                new Entities.NewBusinessPolicyTransaction(@event.NewBusinessTransactionId, @event, eventSequenceNumber);
            this.transactions = new List<Entities.PolicyTransaction>()
            {
                newBusinessTransaction,
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Policy"/> class.
        /// </summary>
        /// <param name="event">The policy issue event.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created the transaction.</param>
        /// <param name="quoteAggregate">The quote aggregate that this policy is a part of.</param>
        public Policy(QuoteAggregate.PolicyIssuedWithoutQuoteEvent @event, int eventSequenceNumber, QuoteAggregate quoteAggregate)
        {
            this.Aggregate = quoteAggregate;
            this.IsFromImport = false;
            this.QuoteId = @event.QuoteId;

            // QuoteId is same with PolicyId on this event.
            this.PolicyId = @event.QuoteId.Value;
            this.InceptionDateTime = @event.InceptionDateTime;
            this.InceptionTimestamp = @event.InceptionTimestamp;
            this.LatestPolicyPeriodStartDateTime = @event.InceptionDateTime;
            this.LatestPolicyPeriodStartTimestamp = @event.EffectiveTimestamp;
            this.ExpiryDateTime = @event.ExpiryDateTime;
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            this.CalculationResult = @event.PolicyData.QuoteDataSnapshot.CalculationResult.Data;
            this.DataSnapshot = @event.PolicyData.QuoteDataSnapshot;
            this.PolicyNumber = @event.PolicyNumber;
            this.CreatedTimestamp = @event.Timestamp;
            this.TimeZone = Timezones.GetTimeZoneByIdOrDefault(@event.TimeZoneId);
            var newBusinessTransaction =
                new Entities.NewBusinessPolicyTransaction(@event.NewBusinessTransactionId, @event, eventSequenceNumber);
            this.transactions = new List<Entities.PolicyTransaction>()
            {
                newBusinessTransaction,
            };
        }

        public Policy(QuoteAggregate.AggregateCreationFromPolicyEvent @event, int eventSequenceNumber, QuoteAggregate quoteAggregate)
        {
            this.Aggregate = quoteAggregate;
            this.IsFromImport = true;
            this.QuoteId = @event.QuoteId;

            // QuoteId is same with PolicyId on this event.
            this.PolicyId = @event.QuoteId.Value;
            this.LatestPolicyPeriodStartDateTime = @event.InceptionDateTime;
            this.LatestPolicyPeriodStartTimestamp = @event.InceptionTimestamp;
            this.InceptionDateTime = @event.InceptionDateTime;
            this.InceptionTimestamp = @event.InceptionTimestamp;
            this.ExpiryDateTime = @event.ExpiryDateTime;
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            this.CalculationResult = @event.DataSnapshot.CalculationResult.Data;
            this.DataSnapshot = @event.DataSnapshot;
            this.PolicyNumber = @event.PolicyNumber;
            this.CreatedTimestamp = @event.Timestamp;
            this.TimeZone = Timezones.GetTimeZoneByIdOrDefault(@event.TimeZoneId);
            this.transactions = new List<Entities.PolicyTransaction>();
        }

        /// <summary>
        /// Gets the aggregate.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public QuoteAggregate Aggregate { get; private set; }

        /// <summary>
        /// Gets or sets the latest projected read model for the policy.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public PolicyReadModel ReadModel { get; set; }

        /// <summary>
        /// Gets the policies tenant id.
        /// </summary>
        public Guid TenantId => this.Aggregate.TenantId;

        /// <summary>
        /// Gets the PolicyId of the quote that created of last updated the policy.
        /// </summary>
        public Guid PolicyId { get; private set; }

        /// <summary>
        /// Gets the ID of the quote that created of last updated the policy.
        /// </summary>
        public Guid? QuoteId { get; private set; }

        /// <summary>
        /// Gets the time the policy was created.
        /// </summary>
        public Instant CreatedTimestamp { get; private set; }

        public DateTimeZone TimeZone { get; }

        /// <summary>
        /// Gets the policy number assigned to this policy.
        /// </summary>
        public string PolicyNumber { get; private set; }

        /// <summary>
        /// Gets the calculation result used for the policy.
        /// </summary>
        public CalculationResult CalculationResult { get; private set; }

        /// <summary>
        /// Gets the date the policy is set to take effect.
        /// </summary>
        public LocalDateTime InceptionDateTime { get; private set; }

        /// <summary>
        /// Gets the timestamp for the first policy period start date,
        /// as calculated at the time of issuance.
        /// </summary>
        public Instant InceptionTimestamp { get; private set; }

        /// <summary>
        /// Gets the cancellation date.
        /// </summary>
        public LocalDateTime? CancellationEffectiveDateTime { get; private set; }

        /// <summary>
        /// Gets the cancellation timestamp
        /// as calculated at the time of cancellation transaction.
        /// </summary>
        public Instant? CancellationEffectiveTimestamp { get; private set; }

        /// <summary>
        /// Gets the cancellation date.
        /// </summary>
        public LocalDateTime? AdjustmentEffectiveDateTime { get; private set; }

        /// <summary>
        /// Gets the adjustment effective timestamp
        /// as calculated at the time of the adjustment transaction.
        /// </summary>
        public Instant? AdjustmentEffectiveTimestamp { get; private set; }

        /// <summary>
        /// Gets the date the policy is set to expire.
        /// </summary>
        public LocalDateTime? ExpiryDateTime { get; private set; }

        /// <summary>
        /// Gets the adjustment effective timestamp
        /// as calculated at the time of the adjustment transaction.
        /// </summary>
        public Instant? ExpiryTimestamp { get; private set; }

        /// <summary>
        /// Gets the date the policy coverage is set to start for the latest policy period.
        /// </summary>
        public LocalDateTime LatestPolicyPeriodStartDateTime { get; private set; }

        /// <summary>
        /// Gets the policy effective timestamp
        /// as calculated at the time of the newbusiness/renewal transaction.
        /// </summary>
        public Instant LatestPolicyPeriodStartTimestamp { get; private set; }

        /// <summary>
        /// Gets the data used to issue the policy.
        /// </summary>
        public QuoteDataSnapshot DataSnapshot { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy has been cancelled.
        /// </summary>
        public bool IsCancelled => this.CancellationEffectiveDateTime.HasValue;

        /// <summary>
        /// Gets a value indicating whether the policy has been adjusted.
        /// </summary>
        public bool IsAdjusted => this.AdjustmentEffectiveDateTime.HasValue;

        /// <summary>
        /// Gets a value indicating whether the policy is from import.
        /// </summary>
        public bool IsFromImport { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the policy is date-based.
        /// A date-based policy has an expiry date.
        /// </summary>
        public bool IsTermBased => this.ExpiryTimestamp != null;

        /// <summary>
        /// Gets the transactions that have occurred for the policy.
        /// </summary>
        public List<Entities.PolicyTransaction> Transactions => this.transactions;

        /// <summary>
        /// Gets a value indicating whether timestamp values should be used instead
        /// of date time values for official policy dates.
        /// When set to false, the time zone and daylight savings are taken into account
        /// when calculating the real policy expiry dates and other dates.
        /// </summary>
        public bool AreTimestampsAuthoritative { get; private set; }

        /// <summary>
        /// Creates a new quote for a mid-term adjustment.
        /// </summary>
        /// <param name="timestamp">The time the adjustment quote is being created.</param>
        /// <param name="quoteNumber">The quote number.</param>
        /// <param name="pastClaims">Past claims under the policy that need to be taken into account.</param>
        /// <param name="performingUserId">The userId who created adjustment quote.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="quoteExpirySettings">The quote expiry settings.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <returns>The adjustment quote that was created.</returns>
        public AdjustmentQuote CreateAdjustmentQuote(
            Instant timestamp,
            string quoteNumber,
            IEnumerable<IClaimReadModel> pastClaims,
            Guid? performingUserId,
            IQuoteWorkflow quoteWorkflow,
            IQuoteExpirySettings quoteExpirySettings,
            bool isMutual,
            Guid? productReleaseId,
            FormDataSchema formDataSchema = null,
            FormData? formData = null,
            string? initialQuoteState = null,
            List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
        {
            this.VerifyAdjustmentCreationIsPermitted(timestamp, isMutual);

            if (this.WouldBeExpired(timestamp))
            {
                throw new ErrorException(Errors.Policy.Adjustment.PolicyHasExpired(this.PolicyNumber, this.ExpiryTimestamp.Value, timestamp, isMutual));
            }

            if (this.WouldBeCancelled(timestamp))
            {
                throw new ErrorException(Errors.Policy.Adjustment.PolicyHasBeenCancelled(this.PolicyNumber, this.CancellationEffectiveTimestamp.Value, timestamp, isMutual));
            }

            var previousTransactionFormData = this.DataSnapshot.FormData.Data.Clone();
            previousTransactionFormData.AddRecentClaims(pastClaims);

            var @event = new QuoteAggregate.AdjustmentQuoteCreatedEvent(
                this.TenantId,
                this.Aggregate.Id,
                this.Aggregate.OrganisationId,
                quoteNumber,
                formData?.Json ?? previousTransactionFormData.Json,
                this.QuoteId,
                performingUserId,
                timestamp,
                productReleaseId,
                initialQuoteState,
                additionalProperties);
            this.Aggregate.ApplyNewEvent(@event);
            if (initialQuoteState == null)
            {
                var resultingState = quoteWorkflow.GetResultingState(QuoteAction.Actualise, QuoteStatus.Nascent.Humanize());
                var stateChangeEvent = new QuoteAggregate.QuoteStateChangedEvent(this.TenantId, @event.AggregateId, @event.QuoteId, QuoteAction.Actualise, performingUserId, QuoteStatus.Nascent.Humanize(), resultingState, timestamp);
                this.Aggregate.ApplyNewEvent(stateChangeEvent);
            }

            this.Aggregate.SetQuoteExpiryFromSettings(@event.QuoteId, performingUserId, timestamp, quoteExpirySettings);
            var adjustmentQuote = this.Aggregate.GetQuoteOrThrow(@event.QuoteId);
            var newFormData = adjustmentQuote.RemoveFieldValuesThatAreConfiguredToBeReset(formDataSchema);
            adjustmentQuote.UpdateFormData(newFormData, performingUserId, timestamp);
            return (AdjustmentQuote)this.Aggregate.GetQuoteOrThrow(@event.QuoteId);
        }

        /// <summary>
        /// Creates a new quote for a cancellation.
        /// </summary>
        /// <param name="timestamp">The time the adjustment quote is being created.</param>
        /// <param name="quoteNumber">The quote number.</param>
        /// <param name="performingUserId">The userId who created cancellation quote.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="quoteExpirySettings">The quote expiry settings.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <returns>The CancellationQuote that was created.</returns>
        public CancellationQuote CreateCancellationQuote(
            Instant timestamp,
            string quoteNumber,
            Guid? performingUserId,
            IQuoteWorkflow quoteWorkflow,
            IQuoteExpirySettings quoteExpirySettings,
            bool isMutual,
            Guid? productReleaseId,
            FormDataSchema formDataSchema = null,
            FormData? formData = null,
            string? initialQuoteState = null,
            List<AdditionalPropertyValueUpsertModel>? additionalPropertyUpserts = null)
        {
            this.VerifyCancellationCreationIsPermitted(timestamp, isMutual);

            // Only create cancellation if policy is active or pending.
            if (!this.IsAdjustable(timestamp))
            {
                throw new InvalidOperationException("Policy cannot currently be cancelled.");
            }

            FormData previousTransactionFormData = this.DataSnapshot.FormData.Data;
            var @event = new QuoteAggregate.CancellationQuoteCreatedEvent(
                this.TenantId,
                this.Aggregate.Id,
                this.Aggregate.OrganisationId,
                quoteNumber,
                this.QuoteId,
                performingUserId,
                timestamp,
                productReleaseId,
                formData?.Json ?? previousTransactionFormData.Json,
                initialQuoteState,
                additionalPropertyUpserts);
            this.Aggregate.ApplyNewEvent(@event);

            if (initialQuoteState == null)
            {
                var resultingState = quoteWorkflow.GetResultingState(QuoteAction.Actualise, QuoteStatus.Nascent.Humanize());
                var stateChangeEvent = new QuoteAggregate.QuoteStateChangedEvent(this.TenantId, @event.AggregateId, @event.QuoteId, QuoteAction.Actualise, performingUserId, QuoteStatus.Nascent.Humanize(), resultingState, timestamp);
                this.Aggregate.ApplyNewEvent(stateChangeEvent);
            }

            this.Aggregate.SetQuoteExpiryFromSettings(@event.QuoteId, performingUserId, timestamp, quoteExpirySettings);
            var cancelQuote = this.Aggregate.GetQuoteOrThrow(@event.QuoteId);
            var newFormData = cancelQuote.RemoveFieldValuesThatAreConfiguredToBeReset(formDataSchema);
            cancelQuote.UpdateFormData(newFormData, performingUserId, timestamp);
            return (CancellationQuote)this.Aggregate.GetQuoteOrThrow(@event.QuoteId);
        }

        /// <summary>
        /// Creates a new quote for a renewal.
        /// </summary>
        /// <param name="recentClaimsUnderPolicy">The list of claims to be added to html table in formdata.</param>
        /// <param name="timestamp">The time the quote is being created.</param>
        /// <param name="quoteNumber">The quote number.</param>
        /// <param name="performingUserId">The userId who created renewal quote.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="quoteExpirySettings">The quote expiry settings.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <returns>The renewal quote that was created.</returns>
        public RenewalQuote CreateRenewalQuote(
            IEnumerable<IClaimReadModel> recentClaimsUnderPolicy,
            Instant timestamp,
            string quoteNumber,
            Guid? performingUserId,
            IQuoteWorkflow quoteWorkflow,
            IQuoteExpirySettings quoteExpirySettings,
            IProductConfiguration productConfiguration,
            bool isMutual,
            Guid? productReleaseId,
            IFormDataSchema formDataSchema = null,
            FormData? formData = null,
            string? initialQuoteState = null,
            List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
        {
            var quote = this.Aggregate.GetLatestQuote();

            if (quote == null)
            {
                if (!this.IsFromImport)
                {
                    throw new ErrorException(Errors.Policy.Renewal.NoPolicyExists(isMutual));
                }
            }

            this.VerifyRenewalCreationIsPermitted(timestamp, isMutual);

            LocalDate newEffectiveDate = this.ExpiryDateTime.Value.Date;
            LocalDate newExpiryDate = newEffectiveDate.PlusYears(1);

            // TODO: Get form data inception/expiry date keys from product configuration.
            FormData previousTransactionFormData = this.DataSnapshot.FormData.Data.UpdateDates(
            newEffectiveDate, newExpiryDate, FormDataPaths.Default);
            previousTransactionFormData.AddRecentClaims(recentClaimsUnderPolicy);

            // Get latest form data
            // Update dates.
            // Get latest cutomer details
            var @event = new QuoteAggregate.RenewalQuoteCreatedEvent(
                this.TenantId,
                this.Aggregate.Id,
                this.Aggregate.OrganisationId,
                quoteNumber,
                formData?.Json ?? previousTransactionFormData.Json,
                performingUserId,
                timestamp,
                productReleaseId,
                initialQuoteState,
                additionalProperties);
            this.Aggregate.ApplyNewEvent(@event);
            if (initialQuoteState == null)
            {
                var resultingState = quoteWorkflow.GetResultingState(QuoteAction.Actualise, QuoteStatus.Nascent.Humanize());
                var stateChangeEvent = new QuoteAggregate.QuoteStateChangedEvent(this.TenantId, @event.AggregateId, @event.QuoteId, QuoteAction.Actualise, performingUserId, QuoteStatus.Nascent.Humanize(), resultingState, timestamp);
                this.Aggregate.ApplyNewEvent(stateChangeEvent);
            }

            this.Aggregate.SetQuoteExpiryFromSettings(@event.QuoteId, performingUserId, timestamp, quoteExpirySettings);
            var renewalQuote = this.Aggregate.GetQuoteOrThrow(@event.QuoteId);
            var newformData = renewalQuote.RemoveFieldValuesThatAreConfiguredToBeReset(formDataSchema);
            renewalQuote.UpdateFormData(newformData, performingUserId, timestamp);
            return (RenewalQuote)this.Aggregate.GetQuoteOrThrow(@event.QuoteId);
        }

        /// <summary>
        /// Mark the policy as cancelled.
        /// </summary>
        /// <param name="event">The policy cancelled event.</param>
        /// <param name="sequenceNumber">The sequence number for the event.</param>
        public void Apply(QuoteAggregate.PolicyCancelledEvent @event, int sequenceNumber)
        {
            // Note: this null checking was needed for the migration UB-11410 to proceed because some records dont have this.
            // this is triggered when getting an aggregate by id, but the production data have this as null,
            // maybe we can remove this null handling after the migration has happened.
            if (@event.DataSnapshot != null)
            {
                this.HandleUpsert(@event);
            }
            this.CancellationEffectiveDateTime = @event.EffectiveDateTime;
            this.CancellationEffectiveTimestamp = @event.EffectiveTimestamp;
            var cancellationTransaction =
                new Entities.CancellationPolicyTransaction(@event.CancellationTransactionId, @event, sequenceNumber);
            this.transactions.Add(cancellationTransaction);
        }

        /// <summary>
        /// Returns a value indicating whether the policy can be adjusted at a given time.
        /// </summary>
        /// <param name="time">The adjustment time.</param>
        /// <returns><c>true</c> if the policy can be adjusted, otherwise <c>false</c>.</returns>
        public bool IsAdjustable(Instant time)
        {
            var policyStatus = this.GetPolicyStatus(time);
            return policyStatus != PolicyStatus.Cancelled
                && policyStatus != PolicyStatus.Expired;
        }

        /// <summary>
        /// Returns a value indicating whether the policy would be expired at a given time.
        /// </summary>
        /// <param name="timestamp">Time to consider.</param>
        /// <returns><c>true</c> if the policy would be expired, otherwise <c>false</c>.</returns>
        public bool WouldBeExpired(Instant timestamp)
        {
            return this.GetPolicyStatus(timestamp) == PolicyStatus.Expired;
        }

        /// <summary>
        /// Returns a value indicating whether the policy would be cancelled at a given time.
        /// </summary>
        /// <param name="timestamp">Time to consider.</param>
        /// <returns><c>true</c> if the policy would be cancelled, otherwise <c>false</c>.</returns>
        public bool WouldBeCancelled(Instant timestamp)
        {
            return this.GetPolicyStatus(timestamp) == PolicyStatus.Cancelled;
        }

        /// <summary>
        /// Returns a value indicating whether the policy can be renewed at a given time.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns><c>true</c> if the policy can be renewed, otherwise <c>false</c>.</returns>
        public bool IsRenewable(Instant time)
        {
            // TODO: Need to implement rules about when a policy can be renewed.
            // E.g. upto X days before expiry, or upto Y days after expiry.
            // These should be set in product configuration.
            var policyStatus = this.GetPolicyStatus(time);
            return policyStatus == PolicyStatus.Active;
        }

        /// <summary>
        /// Update the policy number of the policy.
        /// </summary>
        /// <param name="policyNumber">The quote number.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UpdatePolicyNumber(string policyNumber, Guid? performingUserId, Instant timestamp)
        {
            var @event = new QuoteAggregate.PolicyNumberUpdatedEvent(this.TenantId, this.PolicyId, this.QuoteId, policyNumber, performingUserId, timestamp);
            this.Aggregate.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Apply a policy adjusted event.
        /// </summary>
        /// <param name="event">The policy adjusted event.</param>
        /// <param name="sequenceNumber">The sequence number of the event within its aggregate.</param>
        public void Apply(QuoteAggregate.PolicyAdjustedEvent @event, int sequenceNumber)
        {
            this.HandleUpsert(@event);
            this.AdjustmentEffectiveDateTime = @event.EffectiveDateTime;
            this.AdjustmentEffectiveTimestamp = @event.EffectiveTimestamp;
            this.ExpiryDateTime = @event.ExpiryDateTime;
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            var adjustmentTransaction = new Entities.AdjustmentPolicyTransaction(@event.AdjustmentTransactionId, @event, sequenceNumber);
            this.transactions.Add(adjustmentTransaction);
        }

        /// <summary>
        /// Apply a policy renewed event.
        /// </summary>
        /// <param name="event">The policy renewed event.</param>
        /// <param name="sequenceNumber">The sequence number of the event.</param>
        public void Apply(QuoteAggregate.PolicyRenewedEvent @event, int sequenceNumber)
        {
            this.HandleUpsert(@event);
            this.LatestPolicyPeriodStartDateTime = @event.EffectiveDateTime;
            this.LatestPolicyPeriodStartTimestamp = @event.EffectiveTimestamp;
            this.ExpiryDateTime = @event.ExpiryDateTime;
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            var renewalTransaction = new Entities.RenewalPolicyTransaction(@event.RenewalTransactionId, @event, sequenceNumber);
            this.transactions.Add(renewalTransaction);
        }

        /// <summary>
        /// Apply the policy form data updated event.
        /// </summary>
        /// <param name="event">The policy updated event.</param>
        /// <param name="sequenceNumber">The sequence number of the event.</param>
        public void Apply(QuoteAggregate.PolicyDataPatchedEvent @event, int sequenceNumber)
        {
            this.ApplyPatch(@event.PolicyDatPatch);

            foreach (var transaction in this.transactions)
            {
                transaction.ApplyPatch(@event.PolicyDatPatch);
            }
        }

        /// <summary>
        /// Handle policy number updated event.
        /// </summary>
        /// <param name="event">The policy number updated event.</param>
        public void Apply(QuoteAggregate.PolicyNumberUpdatedEvent @event, int sequenceNumber)
        {
            this.PolicyNumber = @event.PolicyNumber;
        }

        public void SetMissingPropertyDuringSnapshotCreation(QuoteAggregate aggregate)
        {
            this.Aggregate = aggregate;
        }

        internal void SetTransactions(QuoteAggregate.SetPolicyTransactionsEvent @event)
        {
            @event.PolicyTransactions?.ForEach(x => this.transactions.Add(x));
        }

        private void ApplyPatch(PolicyDataPatch patch)
        {
            if (patch.Type == DataPatchType.FormData)
            {
                this.DataSnapshot.FormData.Data.PatchFormModelProperty(patch.Path, patch.Value);
            }
            else if (patch.Type == DataPatchType.CalculationResult)
            {
                this.DataSnapshot.CalculationResult.Data.PatchProperty(patch.Path, patch.Value);
            }
        }

        private void HandleUpsert(IPolicyUpsertEvent @event)
        {
            this.QuoteId = @event.QuoteId;
            this.CalculationResult = @event.DataSnapshot.CalculationResult.Data;
            this.DataSnapshot = @event.DataSnapshot;
        }

        private void VerifyCancellationCreationIsPermitted(Instant timestamp, bool isMutual)
        {
            var quote = this.Aggregate.GetLatestQuote();
            if (quote == null)
            {
                // This is an imported policy.
                return;
            }

            quote.ThrowIfActiveWhenTryingToCancel(timestamp, isMutual);
        }

        private void VerifyAdjustmentCreationIsPermitted(Instant timestamp, bool isMutual)
        {
            var quote = this.Aggregate.GetLatestQuote();
            if (quote == null)
            {
                // This is an imported policy.
                return;
            }

            quote.ThrowIfActiveWhenTryingToAdjust(timestamp, isMutual);
        }

        private void VerifyRenewalCreationIsPermitted(Instant timestamp, bool isMutual)
        {
            var quote = this.Aggregate.GetLatestQuote();

            if (quote == null)
            {
                // This is an imported policy.
                return;
            }

            quote.ThrowIfActiveWhenTryingToRenew(timestamp, isMutual);
        }
    }
}
