// <copyright file="AdjustmentQuote.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Linq;
    using Humanizer;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;

    /// <summary>
    /// Represents a quote of type adjustment within a QuoteAggregate.
    /// </summary>
    public class AdjustmentQuote : Quote
    {
        [System.Text.Json.Serialization.JsonConstructor]
        public AdjustmentQuote(
            Guid id,
            Guid policyId,
            Guid? productReleaseId,
            Instant createdTimestamp,
            int eventSequenceNumber,
            bool hadCustomerOnCreation,
            bool isDiscarded,
            bool isSubmitted,
            bool transactionCompleted,
            bool isPaidFor,
            bool isFunded,
            bool isActualised,
            QuoteExpiryReason expiryReason,
            bool areTimestampsAuthoritative,
            QuoteDataUpdate<FormData> latestFormData,
            QuoteDataUpdate<CalculationResult> latestCalculationResult,
            QuoteStateChange latestQuoteStateChange,
            QuoteStateChange previousQuoteStateChange,
            DateTimeZone timeZone,
            string importQuoteState,
            string quoteNumber,
            string quoteTitle,
            string initialQuoteState,
            string workflowStep,
            Invoice invoice,
            List<QuoteVersion> versions,
            QuoteDataUpdate<IPersonalDetails> latestCustomerDetails,
            List<QuoteFileAttachment> quoteFileAttachments,
            FundingProposalCreationResult latestFundingProposalCreationResult,
            FundingProposal acceptedProposal,
            IDictionary<string, QuoteDocument> quoteDocumentsByName)
            : base(
                  id,
                  policyId,
                  productReleaseId,
                  createdTimestamp,
                  eventSequenceNumber,
                  hadCustomerOnCreation,
                  isDiscarded,
                  isSubmitted,
                  transactionCompleted,
                  isPaidFor,
                  isFunded,
                  isActualised,
                  expiryReason,
                  areTimestampsAuthoritative,
                  latestFormData,
                  latestCalculationResult,
                  latestQuoteStateChange,
                  previousQuoteStateChange,
                  timeZone,
                  importQuoteState,
                  quoteNumber,
                  quoteTitle,
                  initialQuoteState,
                  workflowStep,
                  invoice,
                  versions,
                  latestCustomerDetails,
                  quoteFileAttachments,
                  latestFundingProposalCreationResult,
                  acceptedProposal,
                  quoteDocumentsByName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdjustmentQuote"/> class.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created the quote.</param>
        /// <param name="quoteNumber">A quote number.</param>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        /// <param name="formDataJson">Initial form data json.</param>
        public AdjustmentQuote(
            Guid quoteId,
            QuoteAggregate aggregate,
            int eventSequenceNumber,
            string quoteNumber,
            Guid? customerId,
            Instant createdTimestamp,
            string formDataJson,
            string? initialQuoteState,
            Guid? productReleaseId)
            : base(quoteId, aggregate, eventSequenceNumber, createdTimestamp, customerId, productReleaseId, formDataJson)
        {
            this.TimeZone = aggregate.Policy.TimeZone;
            this.SetQuoteNumber(quoteNumber);
            this.InitialQuoteState = initialQuoteState;
        }

        /// <inheritdoc/>
        public override QuoteType Type => QuoteType.Adjustment;

        /// <summary>
        /// Issue an adjustment policy for the quote.
        /// </summary>
        /// <param name="calculationResultId">The ID of the calculation result to use.</param>
        /// <param name="configuration">The product configuration.</param>
        /// <param name="timeOfDayScheme">A helper for setting policy limit times.</param>
        /// <param name="performingUserId">The user processing the policy issuance.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="progressQuoteState">A value indicating whether the policy operation should progress the quote state.</param>
        public void AdjustPolicy(
            Guid calculationResultId,
            IProductConfiguration configuration,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            bool isMutual,
            bool progressQuoteState = true)
        {
            if (this.ParentPolicy.WouldBeExpired(timestamp))
            {
                throw new ErrorException(Errors.Policy.Adjustment.PolicyHasExpired(
                    this.ParentPolicy.PolicyNumber, this.ParentPolicy.ExpiryTimestamp.Value, timestamp, isMutual));
            }

            if (this.ParentPolicy.WouldBeCancelled(timestamp))
            {
                throw new ErrorException(Errors.Policy.Adjustment.PolicyHasBeenCancelled(
                    this.ParentPolicy.PolicyNumber, this.ParentPolicy.CancellationEffectiveTimestamp.Value, timestamp, isMutual));
            }

            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.Policy, this.QuoteStatus) && progressQuoteState)
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.Policy);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    this.QuoteStatus,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            this.ThrowIfActiveTriggersAndNotBindable();

            var quoteDataSnapshot = this.GetQuoteDataSnapshotForPolicyTransaction(calculationResultId);
            var applicableFormData = this.LatestFormData.Data;
            var quoteDataRetriever = new StandardQuoteDataRetriever(
                configuration, applicableFormData, quoteDataSnapshot.CalculationResult?.Data);
            var expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy expiry date")));
            var effectiveDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.EffectiveDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy adjustment effective date")));

            LocalDateTime effectiveDateTime = this.GetEffectiveDateTimeForEffectiveDateUsingTimeOfDayScheme(
                effectiveDate, timestamp, timeOfDayScheme);
            Instant effectiveTimestamp = effectiveDateTime.InZoneLeniently(this.TimeZone).ToInstant();
            LocalDateTime expiryDateTime = expiryDate.At(timeOfDayScheme.GetEndTime());
            Instant expiryTimestamp = expiryDateTime.InZoneLeniently(this.TimeZone).ToInstant();
            if (expiryDateTime < this.ParentPolicy.LatestPolicyPeriodStartDateTime)
            {
                throw new ErrorException(Errors.Policy.Adjustment.PolicyAdjustmentExpiryDateMustNotBeBeforePolicyPeriodStarted(
                    this.ParentPolicy.PolicyNumber,
                    this.ParentPolicy.LatestPolicyPeriodStartDateTime,
                    expiryDateTime,
                    isMutual));
            }

            if (this.ParentPolicy.LatestPolicyPeriodStartDateTime > effectiveDateTime)
            {
                if (effectiveDateTime.Date.Equals(this.ParentPolicy.LatestPolicyPeriodStartDateTime.Date))
                {
                    // if we're adjusting on the same date as the start date, but somehow the adjustment time is
                    // before the start time, let's just make the adjustment time the same as the start time so we can proceed
                    effectiveDateTime = this.ParentPolicy.LatestPolicyPeriodStartDateTime;
                    effectiveTimestamp = this.ParentPolicy.LatestPolicyPeriodStartTimestamp;
                }
                else
                {
                    throw new ErrorException(Errors.Policy.Adjustment.PolicyAdjustmentEffectiveDateMustNotBeBeforePolicyPeriodStarted(
                        this.ParentPolicy.PolicyNumber,
                        this.ParentPolicy.LatestPolicyPeriodStartDateTime,
                        effectiveDateTime,
                        isMutual));
                }
            }

            if (effectiveDateTime > expiryDateTime)
            {
                throw new ErrorException(
                    Errors.Policy.Adjustment.PolicyAdjustmentEffectiveDateMustNotBeAfterPolicyPeriodEnded(
                        this.ParentPolicy.PolicyNumber,
                        this.ParentPolicy.LatestPolicyPeriodStartDateTime,
                        effectiveDateTime,
                        isMutual));
            }

            var @event = new QuoteAggregate.PolicyAdjustedEvent(
                this.TenantId,
                this.AggregateId,
                this.Id,
                this.QuoteNumber,
                effectiveDateTime,
                effectiveTimestamp,
                expiryDateTime,
                expiryTimestamp,
                quoteDataSnapshot,
                performingUserId,
                timestamp,
                this.ProductReleaseId);
            this.Aggregate.ApplyNewEvent(@event);

            if (progressQuoteState)
            {
                this.ProgressQuoteState(quoteWorkflow, QuoteAction.Policy, performingUserId, timestamp);
            }
        }

        /// <inheritdoc/>
        public override UBind.Domain.Aggregates.Quote.FormData RemoveFieldValuesThatAreConfiguredToBeReset(IFormDataSchema formDataSchema)
        {
            var questionMetaData = formDataSchema?.GetQuestionMetaData();

            if (questionMetaData == null)
            {
                return this.LatestFormData.Data;
            }

            questionMetaData
                        .Where(w => w.ResetForNewAdjustmentQuotes == true || w.ResetForNewQuotes == true)
                        .ToList()
                        .ForEach(property => this.LatestFormData.Data.RemoveFormDataProperty(property.Key));

            return this.LatestFormData.Data;
        }

        /// <inheritdoc/>
        public override void ThrowIfActiveWhenTryingToCancel(Instant now, bool isMutual)
        {
            if (!this.IsQuoteActive())
            {
                return;
            }

            if (this.IsExpired(now))
            {
                throw new ErrorException(Errors.Policy.Cancellation.ExpiredAdjustmentQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Cancellation.AdjustmentQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
        }

        /// <inheritdoc/>
        public override void ThrowIfActiveWhenTryingToRenew(Instant now, bool isMutual)
        {
            if (!this.IsQuoteActive())
            {
                return;
            }

            if (this.IsExpired(now))
            {
                throw new ErrorException(Errors.Policy.Renewal.ExpiredAdjustmentQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Renewal.AdjustmentQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
        }

        /// <inheritdoc/>
        public override void ThrowIfActiveWhenTryingToAdjust(Instant now, bool isMutual)
        {
            if (!this.IsQuoteActive())
            {
                return;
            }

            if (this.IsExpired(now))
            {
                throw new ErrorException(Errors.Policy.Adjustment.ExpiredAdjustmentQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Adjustment.AdjustmentQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
        }
    }
}
