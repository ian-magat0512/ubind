// <copyright file="NewBusinessQuote.cs" company="uBind">
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
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;

    /// <summary>
    ///  Represents a quote of type new business within a QuoteAggregate.
    /// </summary>
    public class NewBusinessQuote : Quote
    {
        [System.Text.Json.Serialization.JsonConstructor]
        public NewBusinessQuote(
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
        /// Initializes a new instance of the <see cref="NewBusinessQuote"/> class.
        /// </summary>
        /// <param name="id">The aggregate id.</param>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="eventSequenceNumber">The eventSequenceNumber.</param>
        /// <param name="createdTimestamp">The quote createdTimestamp.</param>
        /// <param name="customerId">The customer ID, if any.</param>
        public NewBusinessQuote(
            Guid id,
            QuoteAggregate aggregate,
            int eventSequenceNumber,
            Instant createdTimestamp,
            DateTimeZone timeZone,
            bool areTimestampsAuthoritative,
            Guid? customerId,
            Guid? productReleaseId,
            string? quoteNumber = null,
            string? initialQuoteState = null)
            : base(id, aggregate, eventSequenceNumber, createdTimestamp, customerId, productReleaseId)
        {
            this.AreTimestampsAuthoritative = areTimestampsAuthoritative;
            this.TimeZone = timeZone;
            this.QuoteNumber = quoteNumber;
            this.InitialQuoteState = initialQuoteState;
        }

        public NewBusinessQuote(
            Guid id,
            QuoteAggregate aggregate,
            int eventSequenceNumber,
            Instant createdTimestamp,
            DateTimeZone timeZone,
            bool areTimestampsAuthoritative,
            Guid? customerId,
            string importQuoteState,
            Guid? productReleaseId)
            : base(id, aggregate, eventSequenceNumber, createdTimestamp, customerId, productReleaseId)
        {
            this.AreTimestampsAuthoritative = areTimestampsAuthoritative;
            this.TimeZone = timeZone;
            this.ImportQuoteState = importQuoteState;
        }

        /// <inheritdoc/>
        public override QuoteType Type => QuoteType.NewBusiness;

        /// <summary>
        /// Gets a value indicating whether policy is issued.
        /// </summary>
        public bool PolicyIssued
        {
            get { return this.TransactionCompleted; }
        }

        /// <summary>
        /// Issue a policy for the quote.
        /// </summary>
        /// <param name="calculationResultId">The ID of the calculation result to use.</param>
        /// <param name="policyNumberProvider">A method for providing policy numbers.</param>
        /// <param name="configuration">The product configuration.</param>
        /// <param name="timeOfDayScheme">A helper for setting policy limit times.</param>
        /// <param name="performingUserId">The user processing the policy issuance.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="progressQuoteState">A value indicating whether the policy operation should progress the quote state.</param>
        public void IssuePolicy(
            Guid calculationResultId,
            Func<string> policyNumberProvider,
            IProductConfiguration configuration,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            bool progressQuoteState = true)
        {
            if (this.PolicyIssued)
            {
                throw new ErrorException(Errors.Quote.CannotIssueMultiplePolicies());
            }

            if (this.LatestFormData == null)
            {
                throw new ErrorException(Errors.Quote.CannotIssuePolicyWithoutFormData());
            }

            if (this.LatestCalculationResult?.Data == null)
            {
                throw new ErrorException(Errors.Quote.CannotIssuePolicyWithoutCalculationResult());
            }

            var quoteDataSnapshot = this.GetQuoteDataSnapshotForPolicyTransaction(calculationResultId);
            bool isBindable = quoteDataSnapshot?.CalculationResult?.Data?.IsBindable ?? false;
            bool hasActiveTrigger = quoteDataSnapshot?.CalculationResult?.Data?.Triggers.Any() ?? false;
            if (this.QuoteStatus == StandardQuoteStates.Incomplete)
            {
                if (hasActiveTrigger)
                {
                    throw new ErrorException(Errors.Quote.CannotIssuePolicyForIncompleteQuoteWithActiveTriggers());
                }

                if (!isBindable)
                {
                    throw new ErrorException(Errors.Quote.CannotIssuePolicyForIncompleteQuoteWithNonBindingCalculation(this.QuoteStatus));
                }
            }

            // allow creation of policy with an incomplete quote with binding calculation and no triggers.
            var incompleteQuoteWithBindingCalculation = this.QuoteStatus == StandardQuoteStates.Incomplete && isBindable && !hasActiveTrigger;
            if (!incompleteQuoteWithBindingCalculation
                && !quoteWorkflow.IsActionPermittedByState(QuoteAction.Policy, this.QuoteStatus) && progressQuoteState)
            {
                throw new ErrorException(Errors.Quote.CannotIssuePolicyForInvalidQuoteState(this.QuoteStatus));
            }

            this.ThrowIfActiveTriggersAndNotBindable();
            var applicableFormData = this.LatestFormData.Data;
            var quoteDataRetriever = new StandardQuoteDataRetriever(
                configuration, applicableFormData, quoteDataSnapshot.CalculationResult?.Data);
            var effectiveDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy inception date")));
            var expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate)
                .GetValueOrThrow(new ErrorException(Errors.General.NotFound("property", "policy expiry date")));

            LocalDateTime effectiveDateTime = this.GetEffectiveDateTimeForEffectiveDateUsingTimeOfDayScheme(
                effectiveDate, timestamp, timeOfDayScheme);
            Instant effectiveTimestamp = effectiveDateTime.InZoneLeniently(this.TimeZone).ToInstant();
            LocalDateTime expiryDateTime = expiryDate.At(timeOfDayScheme.GetEndTime());
            Instant expiryTimestamp = expiryDateTime.InZoneLeniently(this.TimeZone).ToInstant();

            var @event = new QuoteAggregate.PolicyIssuedEvent(
                this.Aggregate,
                this.Id,
                this.QuoteNumber,
                policyNumberProvider.Invoke(),
                quoteDataSnapshot.CalculationResult.Data,
                effectiveDateTime,
                effectiveTimestamp,
                expiryDateTime,
                expiryTimestamp,
                this.TimeZone,
                quoteDataSnapshot,
                performingUserId,
                timestamp,
                this.ProductReleaseId);
            this.Aggregate.ApplyNewEvent(@event);

            if (progressQuoteState)
            {
                this.ProgressQuoteState(quoteWorkflow, QuoteAction.Policy, performingUserId, timestamp);
                this.RecordPolicyIssued();
            }
        }

        /// <summary>
        /// Record that a policy has been completed for this quote.
        /// </summary>
        public void RecordPolicyIssued()
        {
            this.TransactionCompleted = true;
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
                        .Where(w => w.ResetForNewPurchaseQuotes == true || w.ResetForNewQuotes == true)
                        .ToList()
                        .ForEach(property => this.LatestFormData.Data.RemoveFormDataProperty(property.Key));

            return this.LatestFormData.Data;
        }

        /// <inheritdoc/>
        public override void ThrowIfActiveWhenTryingToCancel(Instant now, bool isMutual)
        {
        }

        /// <inheritdoc/>
        public override void ThrowIfActiveWhenTryingToRenew(Instant now, bool isMutual)
        {
        }

        /// <inheritdoc/>
        public override void ThrowIfActiveWhenTryingToAdjust(Instant now, bool isMutual)
        {
        }
    }
}
