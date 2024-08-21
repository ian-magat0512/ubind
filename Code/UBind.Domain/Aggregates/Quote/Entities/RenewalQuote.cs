// <copyright file="RenewalQuote.cs" company="uBind">
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
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;

    /// <summary>
    /// Represents Renewal Quote.
    /// </summary>
    public class RenewalQuote : Quote
    {
        [System.Text.Json.Serialization.JsonConstructor]
        public RenewalQuote(
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
        /// Initializes a new instance of the <see cref="RenewalQuote"/> class.
        /// </summary>
        /// <param name="quoteId">The quoteId.</param>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="eventSequenceNumber">The event Sequence Number.</param>
        /// <param name="quoteNumber">The quote Number.</param>
        /// <param name="formDataJson">Thye form Data Json.</param>
        /// <param name="createdTimestamp">The createdTimestamp.</param>
        /// <param name="customerId">The ID of the customer, if any.</param>
        public RenewalQuote(
            Guid quoteId,
            QuoteAggregate aggregate,
            int eventSequenceNumber,
            string quoteNumber,
            string formDataJson,
            Instant createdTimestamp,
            Guid? customerId,
            Guid? productReleaseId,
            string? initialQuoteState)
            : base(quoteId, aggregate, eventSequenceNumber, createdTimestamp, customerId, productReleaseId)
        {
            this.TimeZone = aggregate.Policy.TimeZone;
            this.SetFormData(formDataJson, createdTimestamp);
            this.InitialQuoteState = initialQuoteState;
            this.SetQuoteNumber(quoteNumber);
        }

        /// <inheritdoc/>
        public override QuoteType Type => QuoteType.Renewal;

        public static void ValidateRenewalCanProceed(
            LocalDateTime effectiveDateTime,
            Instant effectiveTimestamp,
            LocalDateTime expiryDateTime,
            Instant expiryTimestamp,
            Policy parentPolicy,
            bool isMutual)
        {
            if (effectiveDateTime != parentPolicy.ExpiryDateTime)
            {
                if (effectiveDateTime.Date.Equals(parentPolicy.ExpiryDateTime.Value.Date))
                {
                    // if the renewal date is correct but the time is different, let's just adjust the
                    // time to match the policy expiry time.
                    effectiveDateTime = parentPolicy.ExpiryDateTime.Value;
                    effectiveTimestamp = parentPolicy.ExpiryTimestamp.Value;
                }
                else
                {
                    throw new ErrorException(Errors.Policy.Renewal.RenewalEffectiveDateTimeMustMatchExpiry(
                    parentPolicy.PolicyNumber,
                    parentPolicy.ExpiryDateTime.Value,
                    parentPolicy.ExpiryTimestamp.Value,
                    effectiveDateTime,
                    effectiveTimestamp,
                    isMutual));
                }
            }

            if (expiryDateTime < parentPolicy.ExpiryDateTime)
            {
                throw new ErrorException(Errors.Policy.Renewal.RenewalEndDateTimeMustNotBeBeforePreviousPolicyPeriodEndDate(
                    parentPolicy.PolicyNumber,
                    parentPolicy.ExpiryDateTime.Value,
                    parentPolicy.ExpiryTimestamp.Value,
                    expiryDateTime,
                    expiryTimestamp,
                    isMutual));
            }
        }

        /// <summary>
        /// Renew policy for the quote.
        /// </summary>
        /// <param name="calculationResultId">The ID of the calculation result to use.</param>
        /// <param name="configuration">The product configuration.</param>
        /// <param name="timeOfDayScheme">A helper for setting policy limit times.</param>
        /// <param name="performingUserId">The user processing the policy issuance.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="progressQuoteState">A value indicating whether the policy operation should progress the quote state.</param>
        public void RenewPolicy(
            Guid calculationResultId,
            IProductConfiguration configuration,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            bool isMutual,
            bool progressQuoteState = true)
        {
            if (this.Type != QuoteType.Renewal)
            {
                throw new InvalidOperationException(TenantHelper.CheckAndChangeTextToMutual("Policies can only be renewed via a renewal quote.", isMutual));
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
            var effectiveDateMaybe = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.EffectiveDate);
            if (!effectiveDateMaybe.HasValue)
            {
                // let's see if they gave an inception date instead
                effectiveDateMaybe = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate)
                    .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy adjustment effective date")));
            }

            var effectiveDate = effectiveDateMaybe.Value;
            LocalDateTime expiryDateTime = expiryDate.At(timeOfDayScheme.GetEndTime());
            Instant expiryTimestamp = expiryDateTime.InZoneLeniently(this.TimeZone).ToInstant();
            LocalDateTime effectiveDateTime = effectiveDate.At(timeOfDayScheme.GetEndTime());
            Instant effectiveTimestamp = effectiveDateTime.InZoneLeniently(this.TimeZone).ToInstant();

            if (effectiveDateTime != this.ParentPolicy.ExpiryDateTime)
            {
                if (effectiveDateTime.Date.Equals(this.ParentPolicy.ExpiryDateTime.Value.Date))
                {
                    // if the renewal date is correct but the time is different, let's just adjust the
                    // time to match the policy expiry time.
                    effectiveDateTime = this.ParentPolicy.ExpiryDateTime.Value;
                    effectiveTimestamp = this.ParentPolicy.ExpiryTimestamp.Value;
                }
            }

            RenewalQuote.ValidateRenewalCanProceed(
                effectiveDateTime,
                effectiveTimestamp,
                expiryDateTime,
                expiryTimestamp,
                this.ParentPolicy,
                isMutual);

            var @event = new QuoteAggregate.PolicyRenewedEvent(
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
                        .Where(w => w.ResetForNewRenewalQuotes == true || w.ResetForNewQuotes == true)
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
                throw new ErrorException(Errors.Policy.Cancellation.ExpiredRenewalQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Cancellation.RenewalQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
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
                throw new ErrorException(Errors.Policy.Renewal.ExpiredRenewalQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Renewal.RenewalQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
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
                throw new ErrorException(Errors.Policy.Adjustment.ExpiredRenewalQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
            }

            throw new ErrorException(Errors.Policy.Adjustment.RenewalQuoteExists(this.Id, this.CreatedTimestamp, this.Aggregate.Policy.PolicyNumber, isMutual));
        }
    }
}
