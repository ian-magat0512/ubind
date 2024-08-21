// <copyright file="CancellationQuote.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;

    /// <summary>
    /// Represents a quote of type cancellation within a QuoteAggregate.
    /// </summary>
    public class CancellationQuote : Quote
    {
        [System.Text.Json.Serialization.JsonConstructor]
        public CancellationQuote(
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
        /// Initializes a new instance of the <see cref="CancellationQuote"/> class.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="quoteAggregate">The quote aggregate.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event that created the quote.</param>
        /// <param name="quoteNumber">A quote number.</param>
        /// <param name="customerId">The ID of the customer, if any.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        /// <param name="formDataJson">Initial form data json.</param>
        public CancellationQuote(
            Guid quoteId,
            QuoteAggregate quoteAggregate,
            int eventSequenceNumber,
            string quoteNumber,
            Guid? customerId,
            Instant createdTimestamp,
            string formDataJson,
            Guid? productReleaseId,
            string? initialQuoteState = null)
            : base(quoteId, quoteAggregate, eventSequenceNumber, createdTimestamp, customerId, productReleaseId, formDataJson)
        {
            this.TimeZone = quoteAggregate.Policy.TimeZone;
            this.InitialQuoteState = initialQuoteState;
            this.SetQuoteNumber(quoteNumber);
        }

        /// <inheritdoc/>
        public override QuoteType Type => QuoteType.Cancellation;

        /// <summary>
        /// Cancels the policy for the quote.
        /// </summary>
        /// <param name="calculationResultId">The ID of the calculation result to use.</param>
        /// <param name="configuration">The product configuration.</param>
        /// <param name="timeOfDayScheme">A helper for setting policy limit times.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="progressQuoteState">A value indicating whether the policy operation should progress the quote state.</param>
        public void CancelPolicy(
            Guid calculationResultId,
            IProductConfiguration configuration,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            bool isMutual,
            bool progressQuoteState = true)
        {
            if (!this.ParentPolicy.IsAdjustable(timestamp))
            {
                throw new InvalidOperationException(TenantHelper.CheckAndChangeTextToMutual(
                    "Cannot adjust an expired or cancelled policy.", isMutual));
            }

            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.Policy, this.QuoteStatus) && progressQuoteState)
            {
                throw new InvalidOperationException(TenantHelper.CheckAndChangeTextToMutual(
                    $"Policy operation is not applicable in current quote state {this.QuoteStatus}", isMutual));
            }

            this.ThrowIfActiveTriggersAndNotBindable();

            var quoteDataSnapshot = this.GetQuoteDataSnapshotForPolicyTransaction(calculationResultId);
            var applicableFormData = this.LatestFormData.Data;
            var quoteDataRetriever = new StandardQuoteDataRetriever(
                configuration, applicableFormData, quoteDataSnapshot.CalculationResult?.Data);
            var cancellationEffectiveDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.CancellationEffectiveDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "cancellation effective date")));

            LocalDateTime cancellationEffectiveDateTime = cancellationEffectiveDate.At(timeOfDayScheme.GetEndTime());
            Instant cancellationEffectiveTimestamp = cancellationEffectiveDateTime.InZoneLeniently(this.TimeZone).ToInstant();

            if (this.ParentPolicy.LatestPolicyPeriodStartDateTime > cancellationEffectiveDateTime)
            {
                if (cancellationEffectiveDateTime.Date.Equals(this.ParentPolicy.LatestPolicyPeriodStartDateTime.Date))
                {
                    // if we're cancelling on the same date as the start date, but the cancellation time is before the
                    // start time, let's just adjust the cancellation time to match the start time, so we can proceed.
                    cancellationEffectiveDateTime = this.ParentPolicy.LatestPolicyPeriodStartDateTime;
                    cancellationEffectiveTimestamp = this.ParentPolicy.LatestPolicyPeriodStartTimestamp;
                }
                else
                {
                    throw new ErrorException(
                        Errors.Policy.Adjustment.PolicyCancellationEffectiveDateMustNotBeBeforePolicyPeriodStarted(
                            this.ParentPolicy.PolicyNumber,
                            this.ParentPolicy.LatestPolicyPeriodStartDateTime,
                            cancellationEffectiveDateTime,
                            isMutual));
                }
            }

            if (cancellationEffectiveDateTime > this.ParentPolicy.ExpiryDateTime)
            {
                throw new ErrorException(
                    Errors.Policy.Adjustment.PolicyCancellationEffectiveDateMustNotBeAfterPolicyPeriodEnded(
                        this.ParentPolicy.PolicyNumber,
                        this.ParentPolicy.LatestPolicyPeriodStartDateTime,
                        cancellationEffectiveDateTime,
                        isMutual));
            }

            var @event = new QuoteAggregate.PolicyCancelledEvent(
                this.TenantId,
                this.AggregateId,
                this.Id,
                this.QuoteNumber,
                quoteDataSnapshot,
                cancellationEffectiveDateTime,
                cancellationEffectiveTimestamp,
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
                        .Where(w => w.ResetForNewCancellationQuotes == true || w.ResetForNewQuotes == true)
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
                throw new ErrorException(Errors.Policy.Cancellation.ExpiredCancellationQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Cancellation.CancellationQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
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
                throw new ErrorException(Errors.Policy.Renewal.ExpiredCancellationQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Renewal.CancellationQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
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
                throw new ErrorException(Errors.Policy.Adjustment.ExpiredCancellationQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Adjustment.CancellationQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
        }
    }
}
