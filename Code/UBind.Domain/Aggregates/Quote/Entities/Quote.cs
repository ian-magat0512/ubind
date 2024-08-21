// <copyright file="Quote.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable IDE0060 // Remove unused parameter

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Aggregates.Quote.Entities;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Represents a quote within a QuoteAggregate.
    /// </summary>
    public abstract class Quote : IPatchableDataHolder
    {
        private readonly List<QuoteVersion> quoteVersions = new List<QuoteVersion>();
        private readonly IDictionary<string, QuoteDocument> quoteDocumentsByName = new Dictionary<string, QuoteDocument>();
        private readonly List<QuoteFileAttachment> quoteFileAttachments = new List<QuoteFileAttachment>();

        [System.Text.Json.Serialization.JsonConstructor]
        public Quote(
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
        {
            this.Id = id;
            this.AggregateId = policyId;
            this.ProductReleaseId = productReleaseId;
            this.EventSequenceNumber = eventSequenceNumber;
            this.HadCustomerOnCreation = hadCustomerOnCreation;
            this.IsDiscarded = isDiscarded;
            this.IsSubmitted = isSubmitted;
            this.TransactionCompleted = transactionCompleted;
            this.IsPaidFor = isPaidFor;
            this.IsFunded = isFunded;
            this.IsActualised = isActualised;
            this.ExpiryReason = expiryReason;
            this.AreTimestampsAuthoritative = areTimestampsAuthoritative;
            this.CreatedTimestamp = createdTimestamp;
            this.LatestFormData = latestFormData;
            this.LatestCalculationResult = latestCalculationResult;
            this.LatestQuoteStateChange = latestQuoteStateChange;
            this.PreviousQuoteStateChange = previousQuoteStateChange;
            this.TimeZone = timeZone;
            this.ImportQuoteState = importQuoteState;
            this.QuoteNumber = quoteNumber;
            this.QuoteTitle = quoteTitle;
            this.InitialQuoteState = initialQuoteState;
            this.WorkflowStep = workflowStep;
            this.Invoice = invoice;
            this.quoteVersions = versions ?? new List<QuoteVersion>();
            this.LatestCustomerDetails = latestCustomerDetails;
            this.quoteFileAttachments = quoteFileAttachments ?? new List<QuoteFileAttachment>();
            this.LatestFundingProposalCreationResult = latestFundingProposalCreationResult;
            this.AcceptedProposal = acceptedProposal;
            this.quoteDocumentsByName = quoteDocumentsByName ?? new Dictionary<string, QuoteDocument>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Quote"/> class.
        /// </summary>
        /// <param name="id">The aggregate Id.</param>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="eventSequenceNumber">The eventSequenceNumber.</param>
        /// <param name="createdTimestamp">The createdTimestamp.</param>
        /// <param name="customerId">The ID of the customer, if any.</param>
        /// <param name="customerDetails">The customerDetails.</param>
        /// <param name="formDataJson">The formDataJson.</param>
        public Quote(
            Guid id,
            QuoteAggregate aggregate,
            int eventSequenceNumber,
            Instant createdTimestamp,
            Guid? customerId,
            Guid? productReleaseId,
            string formDataJson = null,
            IPersonalDetails customerDetails = null)
        {
            this.Id = id;
            this.AggregateId = aggregate.Id;
            this.Aggregate = aggregate;
            this.EventSequenceNumber = eventSequenceNumber;
            this.TenantId = aggregate.TenantId;
            this.ParentPolicy = aggregate.Policy;
            this.CreatedTimestamp = createdTimestamp;
            this.ProductReleaseId = productReleaseId;
            if (customerId.HasValue && customerId.GetValueOrDefault() != default)
            {
                this.HadCustomerOnCreation = true;
            }

            if (customerDetails != null)
            {
                this.LatestCustomerDetails = new QuoteDataUpdate<IPersonalDetails>(this.Id, customerDetails, createdTimestamp);
            }

            if (formDataJson != null)
            {
                var formDataUpdate = new QuoteDataUpdate<FormData>(
                    this.Id, new FormData(formDataJson), createdTimestamp);
                this.LatestFormData = formDataUpdate;
            }
        }

        /// <summary>
        /// Gets the aggregate.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public QuoteAggregate Aggregate { get; private set; }

        /// <summary>
        /// Gets the latest projected read model for the quote.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public NewQuoteReadModel ReadModel { get; private set; }

        /// <summary>
        /// Gets the time the quote was created.
        /// </summary>
        public Instant CreatedTimestamp { get; private set; }

        public DateTimeZone TimeZone { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether timestamp values should be used instead
        /// of date time values for official policy dates.
        /// When set to false, the time zone and daylight savings are taken into account
        /// when calculating the real policy expiry dates and other dates.
        /// </summary>
        public bool AreTimestampsAuthoritative { get; protected set; }

        /// <summary>
        /// Gets the ID of the quote.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the ID of the policy to which the quote belongs.
        /// </summary>
        public Guid PolicyId => this.AggregateId;

        /// <summary>
        /// Gets the sequence number of the event that created the quote.
        /// </summary>
        public int EventSequenceNumber { get; }

        /// <summary>
        /// Gets or sets the state of the imported quote.
        /// </summary>
        /// <remarks>Property can only have a value if the quote is from import.</remarks>
        public string ImportQuoteState { get; protected set; }

        /// <summary>
        /// Gets the type of the quote.
        /// </summary>
        public abstract QuoteType Type { get; }

        /// <summary>
        /// Gets or sets the quote number.
        /// </summary>
        public string QuoteNumber { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether this quote had a customer associated with it at the time the quote
        /// was created.
        /// </summary>
        public bool HadCustomerOnCreation { get; private set; }

        /// <summary>
        /// Gets the quote title.
        /// </summary>
        public string QuoteTitle { get; private set; }

        /// <summary>
        /// Gets or sets the initial quote state of the created quote.
        /// </summary>
        public string? InitialQuoteState { get; protected set; }

        /// <summary>
        /// Gets the latest quote state change, or null, if no quote state change operation exists.
        /// </summary>
        public QuoteStateChange LatestQuoteStateChange { get; private set; }

        public QuoteStateChange PreviousQuoteStateChange { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote is currently updateable.
        /// </summary>
        public bool IsUpdatable => !this.IsDiscarded;

        /// <summary>
        /// Gets a value indicating whether the quote has been discarded.
        /// </summary>
        public bool IsDiscarded { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote has been submitted.
        /// </summary>
        public bool IsSubmitted { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether a policy has been completed.
        /// </summary>
        public bool TransactionCompleted { get; protected set; }

        /// <summary>
        /// Gets the invoice issued if any, otherwise null.
        /// </summary>
        public Invoice Invoice { get; private set; }

        /// <summary>
        /// Gets the invoice number if an invoice has been issued, otherwise null.
        /// </summary>
        public string InvoiceNumber => this.Invoice?.InvoiceNumber;

        /// <summary>
        /// Gets a value indicating whether an invoice has been issued for this quote.
        /// </summary>
        public bool InvoiceIssued => this.Invoice != null;

        /// <summary>
        /// Gets the credit note issued, if any, otherwise null.
        /// </summary>
        public ObsoleteCreditNote CreditNote { get; private set; }

        /// <summary>
        /// Gets the credit note number if a credit note has been issued, otherwise null.
        /// </summary>
        public string CreditNoteNumber => this.CreditNote?.CreditNoteNumber;

        /// <summary>
        /// Gets a value indicating whether a credit note has been issued for this quote.
        /// </summary>
        public bool CreditNoteIssued => this.CreditNote != null;

        /// <summary>
        /// Gets the versions persisted for this quote.
        /// </summary>
        public List<QuoteVersion> Versions => this.quoteVersions;

        /// <summary>
        /// Gets the latest version number if one exists, otherwise zero.
        /// </summary>
        public int VersionNumber => this.quoteVersions.Select(v => v.Number).LastOrDefault();

        /// <summary>
        /// Gets the latest version number if one exists, otherwise zero.
        /// </summary>
        public Guid VersionId => this.quoteVersions.Select(v => v.VersionId).LastOrDefault();

        /// <summary>
        /// Gets the latest form data, or null if no form data exists.
        /// </summary>
        public QuoteDataUpdate<FormData> LatestFormData { get; private set; }

        /// <summary>
        /// Gets the latest customer details, or null if no customer details exist.
        /// </summary>
        public QuoteDataUpdate<IPersonalDetails>? LatestCustomerDetails { get; private set; }

        /// <summary>
        /// Gets the latest form data, or null if no form data exists.
        /// </summary>
        public QuoteDataUpdate<ReadWriteModel.CalculationResult> LatestCalculationResult { get; private set; }

        /// <summary>
        /// Gets the result of the latest payment attempt, if any, otherwise null.
        /// </summary>
        public PaymentAttemptResult LatestPaymentAttemptResult { get; private set; }

        /// <summary>
        /// Gets the quote work flow step.
        /// </summary>
        public string WorkflowStep { get; private set; }

        /// <summary>
        /// Gets the quote file attachments.
        /// </summary>
        public List<QuoteFileAttachment> QuoteFileAttachments => this.quoteFileAttachments;

        /// <summary>
        /// Gets the latest funding proposal creation result if one exists, otherwise null.
        /// </summary>
        public FundingProposalCreationResult LatestFundingProposalCreationResult { get; private set; }

        /// <summary>
        /// Gets the accepted funding proposal, if any, otherwise null.
        /// </summary>
        public FundingProposal AcceptedProposal { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote has been paid for.
        /// </summary>
        public bool IsPaidFor { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote has been funded.
        /// </summary>
        public bool IsFunded { get; private set; }

        /// <summary>
        /// Gets the quote submission details if any, otherwise null.
        /// </summary>
        public QuoteSubmission Submission { get; private set; }

        /// <summary>
        /// Gets the time the quote expired.
        /// </summary>
        public Instant? ExpiryTimestamp { get; private set; }

        /// <summary>
        /// Gets the expiry reason of the quote.
        /// </summary>
        public QuoteExpiryReason ExpiryReason { get; private set; }

        /// <summary>
        /// Gets the documents associated with the quote.
        /// </summary>
        public IEnumerable<QuoteDocument> Documents => this.quoteDocumentsByName.Values;

        /// <summary>
        /// Gets a value indicating whether the quote is actualised.
        /// </summary>
        public bool IsActualised { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote is test data only and not to be considered a real quote.
        /// </summary>
        public bool IsTestData => this.Aggregate.IsTestData;

        /// <summary>
        /// Gets the customer id.
        /// </summary>
        public Guid? CustomerId => this.Aggregate.CustomerId;

        /// <summary>
        /// Gets a value indicating whether the quote is assigned to a customer.
        /// </summary>
        public bool HasCustomer => this.Aggregate.HasCustomer;

        /// <summary>
        /// Gets the latest quote status based on latest quote state change, or nascent, if no quote state change has
        /// been performed.
        /// Note that this status will never return "Expired" if the quote has expired. If you want to check if the
        /// quote has expired, please call isExpired(Instant now) instead.
        /// </summary>
        public string QuoteStatus => this.LatestQuoteStateChange?.ResultingState ?? this.ImportQuoteState ?? this.InitialQuoteState ?? StandardQuoteStates.Nascent;

        public Guid? ProductReleaseId { get; private set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ProductContext ProductContext => new ProductContext(this.TenantId, this.Aggregate.ProductId, this.Aggregate.Environment);

        public IDictionary<string, QuoteDocument> QuoteDocumentsByName => this.quoteDocumentsByName;

        /// <summary>
        /// Gets the parent policy.
        /// </summary>
        protected Policy ParentPolicy { get; private set; }

        /// <summary>
        /// Gets the aggregateId.
        /// </summary>
        protected Guid AggregateId { get; private set; }

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        protected Guid TenantId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote is bindable.
        /// </summary>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>true if the quote can be bound.</returns>
        public bool IsBindable(IQuoteWorkflow quoteWorkflow) =>
            quoteWorkflow.IsActionPermittedByState(QuoteAction.Bind, this.QuoteStatus);

        /// <summary>
        /// Gets a value indicating whether the quote can be paid for.
        /// </summary>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>true if the quote can be paid for.</returns>
        public bool IsPaymentAdmissible(IQuoteWorkflow quoteWorkflow) =>
            quoteWorkflow.IsActionPermittedByState(QuoteAction.Payment, this.QuoteStatus);

        /// <summary>
        /// Gets a value indicating whether the quote's funding proposal can be accepted.
        /// </summary>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>true if the quote's funding proposal can be accepted.</returns>
        public bool IsFundingAcceptanceAdmissible(IQuoteWorkflow quoteWorkflow) =>
            quoteWorkflow.IsActionPermittedByState(QuoteAction.Fund, this.QuoteStatus);

        /// <summary>
        /// Gets a value indicating whether a settlement is required based on the configured workflow for the quote.
        /// </summary>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>true if a settlement is required based on the configured workflow for the quote.</returns>
        public bool IsSettlementRequired(IQuoteWorkflow quoteWorkflow) =>
            quoteWorkflow.IsSettlementRequired;

        /// <summary>
        /// Gets a value indicating whether optional settlement methods are supported.
        /// </summary>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>true if optional settlement methods are supported.</returns>
        public bool IsSettlementSupported(IQuoteWorkflow quoteWorkflow) =>
            quoteWorkflow.IsSettlementSupported;

        /// <summary>
        /// Gets a value showing the threshold for calculation premium before a settlement can be made mandatory, if available.
        /// </summary>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>a value showing the threshold for calculation premium before a settlement can be made mandatory, if available.</returns>
        public decimal? PremiumThresholdRequiringSettlement(IQuoteWorkflow quoteWorkflow) =>
            quoteWorkflow.PremiumThresholdRequiringSettlement;

        /// <summary>
        /// Gets the binding options selected to be used for the binding process.
        /// </summary>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>The bind options.</returns>
        public BindOptions GetBindingOptions(IQuoteWorkflow quoteWorkflow) =>
            quoteWorkflow.BindOptions;

        public bool IsExpired(Instant timestamp) =>
            this.ExpiryTimestamp.HasValue && this.ExpiryTimestamp.Value < timestamp;

        public void SetMissingPropertyDuringSnapshotCreation(QuoteAggregate quoteAggregate)
        {
            this.Aggregate = quoteAggregate;
            this.TenantId = quoteAggregate.TenantId;
            this.AggregateId = quoteAggregate.Id;
            this.ParentPolicy = quoteAggregate.Policy;
        }

        /// <summary>
        /// Assign a quote number to the quote.
        /// </summary>
        /// <param name="quoteNumber">The quote number.</param>
        public void SetQuoteNumber(string quoteNumber)
        {
            if (this.QuoteNumber != null)
            {
                throw new ErrorException(Errors.Quote.QuoteNumberAlreadyAssigned(this.QuoteNumber, quoteNumber));
            }

            this.QuoteNumber = quoteNumber;
        }

        public void SetFormData(string formDataJson, Instant timestamp)
        {
            if (formDataJson != null)
            {
                var formDataUpdate = new QuoteDataUpdate<FormData>(
                    this.Id, new FormData(formDataJson), timestamp);
                this.LatestFormData = formDataUpdate;
            }
        }

        /// <summary>
        /// Updates the form data for the quote.
        /// </summary>
        /// <param name="formData">The latest form data for the application.</param>
        /// <param name="performingUserId">The userId who updates form data.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        public Guid UpdateFormData(FormData formData, Guid? performingUserId, Instant timestamp)
        {
            // retain always the additionalFormData fields.
            // these are custom fields for the form to retrieve value from.
            if (this.LatestFormData?.Data?.JObject["additionalFormData"]?.ToString() != null &&
                this.LatestFormData?.Data?.JObject["additionalFormData"]?.ToString() != "{}")
            {
                formData.PatchProperty(
                    new JsonPath("additionalFormData"),
                    this.LatestFormData.Data.JObject["additionalFormData"]);
            }

            var @event = new QuoteAggregate.FormDataUpdatedEvent(
                this.TenantId, this.AggregateId, this.Id, formData.Json, performingUserId, timestamp);
            if (this.IsExpired(timestamp))
            {
                throw new ErrorException(Errors.Quote.CannotUpdateWhenExpired(this.Id));
            }

            if (this.IsDiscarded)
            {
                throw new ErrorException(Errors.Quote.CannotUpdateWhenDiscarded());
            }

            this.Aggregate.ApplyNewEvent(@event);
            return @event.FormUpdateId;
        }

        /// <summary>
        /// Saves the form data for the application.
        /// </summary>
        /// <param name="performingUserId">The userId who saved the quote. </param>
        /// <param name="timestamp">The time the save was recorded.</param>
        public void SaveQuote(Guid? performingUserId, Instant timestamp)
        {
            var @event = new QuoteAggregate.QuoteSavedEvent(this.TenantId, this.AggregateId, this, performingUserId, timestamp);
            this.Aggregate.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Actualise the quote.
        /// </summary>
        /// <param name="performingUserId">The userId who created version.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void Actualise(Guid? performingUserId, Instant timestamp, IQuoteWorkflow quoteWorkflow)
        {
            if (!this.IsActualised)
            {
                if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.Actualise, this.QuoteStatus))
                {
                    var operation = quoteWorkflow.GetOperation(QuoteAction.Actualise);
                    throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                        operation.Action.Humanize(),
                        this.QuoteStatus,
                        operation.ResultingState,
                        operation.RequiredStates));
                }

                var resultingState = quoteWorkflow.GetResultingState(QuoteAction.Actualise, this.QuoteStatus);

                var quoteStateChangedEvent = new QuoteAggregate.QuoteStateChangedEvent(
                    this.TenantId,
                    this.AggregateId,
                    this.Id,
                    QuoteAction.Actualise,
                    performingUserId,
                    this.QuoteStatus,
                    resultingState,
                    timestamp);
                this.Aggregate.ApplyNewEvent(quoteStateChangedEvent);

                var quoteActualisedEvent = new QuoteAggregate.QuoteActualisedEvent(this.TenantId, this.AggregateId, this.Id, performingUserId, timestamp);
                this.Aggregate.ApplyNewEvent(quoteActualisedEvent);
            }
        }

        public void SetToActualised()
        {
            this.IsActualised = true;
        }

        /// <summary>
        /// Update the quote's workflow step.
        /// </summary>
        /// <param name="newStep">The new step.</param>
        public void UpdateWorkflowStep(string newStep)
        {
            this.WorkflowStep = newStep;
        }

        /// <summary>
        /// Apply a form update event.
        /// </summary>
        /// <param name="event">The form update event.</param>
        public void Apply(QuoteAggregate.FormDataUpdatedEvent @event, int sequenceNumber)
        {
            var formDataUpdate = new QuoteDataUpdate<FormData>(@event.FormUpdateId, new FormData(@event.FormData), @event.Timestamp);
            this.LatestFormData = formDataUpdate;
        }

        /// <summary>
        /// Seeds a quote with form data.
        /// </summary>
        /// <param name="formDataJson">Form data to seed the quote with.</param>
        public void SeedWithFormData(string formDataJson)
        {
            var formDataUpdate = new QuoteDataUpdate<FormData>(this.Id, new FormData(formDataJson), this.CreatedTimestamp);
            this.LatestFormData = formDataUpdate;
        }

        /// <summary>
        /// Apply a form data patched event.
        /// </summary>
        /// <param name="event">The form update event.</param>
        public void Apply(QuoteAggregate.FormDataPatchedEvent @event, int sequenceNumber)
        {
            JsonPatchDocument jsonPatchDocument = @event.FormDataPatch;
            ExpandoObject formDataExpadoObject = JsonConvert.DeserializeObject<ExpandoObject>(
                this.LatestFormData.Data.Json, new ExpandoObjectConverter());
            jsonPatchDocument.ApplyTo(formDataExpadoObject);
            var formData = JsonConvert.SerializeObject(formDataExpadoObject);
            var formDataUpdate = new QuoteDataUpdate<FormData>(@event.FormUpdateId, new FormData(formData), @event.Timestamp);
            this.LatestFormData = formDataUpdate;
        }

        /// <summary>
        /// Apply the policy form data updated event.
        /// </summary>
        /// <param name="event">The policy updated event.</param>
        public void Apply(QuoteAggregate.PolicyDataPatchedEvent @event, int sequenceNumber)
        {
            this.ApplyPatch(@event.PolicyDatPatch);
        }

        /// <summary>
        /// Apply a calculation result creation event.
        /// </summary>
        /// <param name="event">The calculation result creation event.</param>
        public void Apply(QuoteAggregate.CalculationResultCreatedEvent @event, int sequenceNumber)
        {
            var calculationResultUpdate = new QuoteDataUpdate<ReadWriteModel.CalculationResult>(
                @event.CalculationResultId, @event.CalculationResult, @event.Timestamp);
            this.LatestCalculationResult = calculationResultUpdate;
        }

        /// <summary>
        /// Seed quote with a calculation result.
        /// </summary>
        /// <param name="calculationResultId">The ID of the calculation result.</param>
        /// <param name="calculationResult">The calculation result.</param>
        /// <param name="timestamp">The timestamp of which the calculation result is created.</param>
        /// <remarks>Used when quote is imported with a calculation result.</remarks>
        public void SeedWithCalculationResult(Guid calculationResultId, CalculationResult calculationResult, Instant timestamp)
        {
            var calculationResultUpdate = new QuoteDataUpdate<CalculationResult>(
                calculationResultId, calculationResult, timestamp);
            this.LatestCalculationResult = calculationResultUpdate;
        }

        /// <summary>
        /// Record the creation of a new calculation result.
        /// </summary>
        /// <param name="calculationResultData">The calculation result json.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <param name="performingUserId">The userId who triggered calculation.</param>
        public void RecordCalculationResult(
            CalculationResult calculationResult,
            CachingJObjectWrapper calculationResultData,
            Instant timestamp,
            IFormDataSchema formDataSchema,
            bool isMutual,
            Guid? performingUserId)
        {
            this.ThrowIfSubmittedOrPolicyIssued(nameof(this.RecordCalculationResult), isMutual);
            this.ThrowIfCalculationInvalidatesApprovedQuote(
               calculationResult,
               formDataSchema);

            var calculationResultCreatedEvent = new QuoteAggregate.CalculationResultCreatedEvent(
                this.TenantId, this.AggregateId, this.Id, calculationResult, performingUserId, timestamp);
            this.Aggregate.ApplyNewEvent(calculationResultCreatedEvent);
        }

        /// <summary>
        /// Record the successful creation of a funding proposal.
        /// </summary>
        /// <param name="fundingProposal">The funding proposal.</param>
        /// <param name="performingUserId">The user ID.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteId">The quote Id.</param>
        public void RecordFundingProposalCreated(
            FundingProposal fundingProposal, Guid? performingUserId, Instant timestamp, Guid quoteId)
        {
            var @event = new QuoteAggregate.FundingProposalCreatedEvent(
                this.TenantId, this.Id, this, fundingProposal, performingUserId, timestamp);
            this.Aggregate.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record a calculation result for the quote.
        /// </summary>
        /// <param name="customerDetailsUpdate">The customer details udpate.</param>
        public void RecordCustomerDetailsUpdate(QuoteDataUpdate<IPersonalDetails> customerDetailsUpdate)
        {
            this.LatestCustomerDetails = customerDetailsUpdate;
        }

        /// <summary>
        /// Set the title of the quote.
        /// </summary>
        /// <param name="quoteTitle">The quote title.</param>
        /// <param name="performingUserId">The performing user.</param>
        /// <param name="timestamp">The time stamp now.</param>
        public void SetQuoteTitle(string quoteTitle, Guid performingUserId, Instant timestamp)
        {
            var @event = new QuoteAggregate.QuoteTitleAssignedEvent(this.TenantId, this.AggregateId, this.Id, quoteTitle, performingUserId, timestamp);
            this.Aggregate.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Binds the quote.
        /// </summary>
        /// <param name="performingUserId">The ID of the user binding the quote.</param>
        /// <param name="calculationResultId">The ID of the calculation result used in the binding process.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>The resulting state changed event.</returns>
        public IEnumerable<IEvent<QuoteAggregate, Guid>> RecordQuoteBinding(
            Guid? performingUserId,
            Guid calculationResultId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            var latestPayablePrice = this.LatestCalculationResult.Data.PayablePrice.TotalPayable;
            bool isPremiumGreaterThanThreshold = quoteWorkflow.PremiumThresholdRequiringSettlement.HasValue
                && latestPayablePrice > quoteWorkflow.PremiumThresholdRequiringSettlement.Value;
            if (quoteWorkflow.IsSettlementRequired && !isPremiumGreaterThanThreshold
                && (!this.IsPaidFor && !this.IsFunded) && (latestPayablePrice > 0))
            {
                throw new InvalidOperationException($"Binding error. Binding requires settlement that was not processed successfully");
            }

            var resultingState = quoteWorkflow.GetResultingState(QuoteAction.Bind, this.QuoteStatus);
            return new IEvent<QuoteAggregate, Guid>[]
            {
                new QuoteAggregate.QuoteBoundEvent(this.TenantId, this.AggregateId, this.Id, performingUserId, timestamp),
                new QuoteAggregate.QuoteStateChangedEvent(this.TenantId, this.AggregateId, this.Id, QuoteAction.Bind, performingUserId, this.QuoteStatus, resultingState, timestamp),
            };
        }

        /// <summary>
        /// Apply a quote bound event.
        /// </summary>
        /// <param name="event">The quote bound event.</param>
        public void Apply(QuoteAggregate.QuoteBoundEvent @event, int sequenceNumber)
        {
            // NOP
        }

        /// <summary>
        /// Apply a quote title event.
        /// </summary>
        /// <param name="event">The quote title event.</param>
        public void Apply(QuoteAggregate.QuoteTitleAssignedEvent @event, int sequenceNumber)
        {
            this.QuoteTitle = @event.QuoteTitle;
        }

        /// <summary>
        /// Record the creation of a new quote version.
        /// </summary>
        /// <param name="event">The versioned event.</param>
        /// <param name="sequenceNumber">The sequence number of the event.</param>
        public void RecordVersioning(QuoteAggregate.QuoteVersionCreatedEvent @event, int sequenceNumber)
        {
            var newVersion = new QuoteVersion(
                @event.VersionId,
                @event.VersionNumber,
                this.GetDataSnapshot(),
                sequenceNumber,
                @event.Timestamp);
            this.quoteVersions.Add(newVersion);
        }

        /// <summary>
        /// Discard the quote.
        /// </summary>
        public void RecordDiscarding()
        {
            this.IsDiscarded = true;
        }

        /// <summary>
        /// Record quote submission.
        /// </summary>
        /// <param name="event">The quote submitted event.</param>
        public void RecordSubmission(QuoteAggregate.QuoteSubmittedEvent @event)
        {
            if (this.IsSubmitted)
            {
                throw new ErrorException(Errors.Quote.AlreadySubmitted(this.Id));
            }

            if (this.LatestFormData == null)
            {
                throw new ErrorException(Errors.Quote.SubmissionRequiresFormData(this.Id));
            }

            this.IsSubmitted = true;
            this.Submission = new QuoteSubmission(@event.DataSnapshotIds, @event.Timestamp);
        }

        /// <summary>
        /// Assign a quote number to this quote.
        /// </summary>
        /// <param name="quoteNumber">The quote number.</param>
        /// <param name="performingUserId">The preforming user's ID.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignQuoteNumber(
            string quoteNumber,
            Guid? performingUserId,
            Instant timestamp)
        {
            if (this.QuoteNumber != null)
            {
                throw new ErrorException(Errors.Quote.QuoteNumberAlreadyAssigned(this.QuoteNumber));
            }

            var @event = new QuoteAggregate.QuoteNumberAssignedEvent(
                    this.TenantId, this.AggregateId, this.Id, quoteNumber, performingUserId, timestamp);
            this.Aggregate.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Rollback this quote to a prior sequence number.
        /// </summary>
        /// <param name="rollbackToSequenceNumber">The quote number.</param>
        /// <param name="performingUserId">The userId who rollback sequence number.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="replayEventsCallback">A function that returns events left after a rollback.</param>
        /// <param name="strippedEventsCallback">A function that returns events being ignored due to rollbacks.</param>
        /// <returns>A quote rollback event.</returns>
        public QuoteAggregate.QuoteRollbackEvent RollbackToSequenceNumber(
            int rollbackToSequenceNumber,
            Guid? performingUserId,
            Instant timestamp,
            Func<IEnumerable<IEvent<QuoteAggregate, Guid>>> replayEventsCallback,
            Func<IEnumerable<IEvent<QuoteAggregate, Guid>>> strippedEventsCallback)
        {
            return new QuoteAggregate.QuoteRollbackEvent(
                this.TenantId,
                this.AggregateId,
                this.Id,
                rollbackToSequenceNumber,
                performingUserId,
                timestamp,
                replayEventsCallback,
                strippedEventsCallback);
        }

        /// <summary>
        /// Record that the quote has been submitted.
        /// </summary>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">The time of the submission.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        public void Submit(
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            if (this.IsSubmitted)
            {
                throw new ErrorException(Errors.Quote.AlreadySubmitted(this.Id));
            }

            if (this.LatestFormData == null)
            {
                throw new ErrorException(Errors.Quote.SubmissionRequiresFormData(this.Id));
            }

            var quoteState = this.QuoteStatus;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.Submit, quoteState))
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.Submit);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            var resultingState = quoteWorkflow.GetResultingState(QuoteAction.Submit, quoteState);

            var events = new IEvent<QuoteAggregate, Guid>[]
            {
                new QuoteAggregate.QuoteSubmittedEvent(this.TenantId, this.AggregateId, this, performingUserId, timestamp),
                new QuoteAggregate.QuoteStateChangedEvent(this.TenantId, this.AggregateId, this.Id, QuoteAction.Submit, performingUserId, quoteState, resultingState, timestamp),
            };

            foreach (IEvent<QuoteAggregate, Guid> @event in events)
            {
                this.Aggregate.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Issue an invoice.
        /// </summary>
        /// <param name="invoiceNumber">The invoice number to be used for the invoice.</param>
        /// <param name="performingUserId">The userId who issue the invoice.</param>
        /// <param name="timestamp">The time and date of issuing the invoice.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="fromBind">A value indicating whether the operation is being called as part of the binding process.</param>
        /// <returns>An invoice issued event.</returns>
        public QuoteAggregate.InvoiceIssuedEvent IssueInvoice(
            string invoiceNumber,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            bool fromBind = false)
        {
            if (this.Invoice != null)
            {
                throw new ErrorException(Errors.Invoice.AlreadyIssued());
            }

            if (this.LatestFormData == null)
            {
                throw new ErrorException(Errors.Invoice.RequiresFormData());
            }

            var quoteState = this.QuoteStatus;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.Invoice, quoteState) && !fromBind)
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.Invoice);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            return new QuoteAggregate.InvoiceIssuedEvent(
                this.TenantId,
                this.AggregateId,
                this,
                invoiceNumber,
                performingUserId,
                timestamp);
        }

        /// <summary>
        /// Handle invoice issued event.
        /// </summary>
        /// <param name="event">The invoice isseud event.</param>
        public void Apply(QuoteAggregate.InvoiceIssuedEvent @event, int sequenceNumber)
        {
            this.Invoice = new Invoice(@event.InvoiceNumber, @event.DataSnapshotIds, @event.Timestamp);
        }

        /// <summary>
        /// Issue a credit note for the quote.
        /// </summary>
        /// <param name="creditNoteNumber">The reference number to be used for the credit note.</param>
        /// <param name="fromBind">A value indicating whether the operation is being called as part of the binding process.</param>
        /// <param name="performingUserId">The userId who created the note.</param>
        /// <param name="timestamp">The time and date of issuing the credit note.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>A credit note issued event.</returns>
        public QuoteAggregate.CreditNoteIssuedEvent IssueCreditNote(
            string creditNoteNumber,
            bool fromBind,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            if (this.CreditNote != null)
            {
                throw new ErrorException(Errors.Quote.CreditNote.AlreadyIssued(this.Id));
            }

            if (this.LatestFormData == null)
            {
                throw new ErrorException(Errors.Quote.CreditNote.RequiresFormData());
            }

            var quoteState = this.QuoteStatus;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.CreditNote, quoteState) && !fromBind)
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.CreditNote);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            return new QuoteAggregate.CreditNoteIssuedEvent(
                this.TenantId,
                this.AggregateId,
                this,
                creditNoteNumber,
                performingUserId,
                timestamp);
        }

        /// <summary>
        /// Handles credit note issued event.
        /// </summary>
        /// <param name="event">The credit note issued event.</param>
        public void Apply(QuoteAggregate.CreditNoteIssuedEvent @event, int sequenceNumber)
        {
            this.CreditNote = new ObsoleteCreditNote(@event.CreditNoteNumber, @event.DataSnapshotIds, @event.Timestamp);
        }

        /// <summary>
        /// Handles quote expiry event.
        /// </summary>
        /// <param name="event">The quote expiry set issued event.</param>
        public void Apply(QuoteAggregate.QuoteExpiryTimestampSetEvent @event, int sequenceNumber)
        {
            this.ExpiryTimestamp = @event.ExpiryTimestamp;
            this.ExpiryReason = @event.Reason;
        }

        /// <summary>
        /// Record a payment attempt result.
        /// </summary>
        /// <param name="result">The payment attempt result.</param>
        public void RecordPaymentAtttemptResult(PaymentAttemptResult result)
        {
            this.LatestPaymentAttemptResult = result;
            if (result.IsSuccess)
            {
                this.IsPaidFor = true;
            }
        }

        /// <summary>
        /// Record a funding proposal creation result.
        /// </summary>
        /// <param name="result">The payment attempt result.</param>
        public void RecordFundingProposalCreationResult(FundingProposalCreationResult result)
        {
            this.LatestFundingProposalCreationResult = result;
        }

        /// <summary>
        /// Record a funding proposal acceptance result.
        /// </summary>
        /// <param name="result">The payment attempt result.</param>
        public void RecordFundingProposalAcceptanceResult(FundingProposalAcceptanceResult result)
        {
            if (result.Success)
            {
                this.AcceptedProposal = result.FundingProposal;
                this.IsFunded = true;
            }
        }

        /// <summary>
        /// Record the successful acceptance of a funding proposal.
        /// </summary>
        /// <param name="internalFundingProposalId">The internal ID of the proposal being accepted.</param>
        /// <param name="performingUserId">The userId who record funding proposal.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <returns>A funding proposal accepted event.</returns>
        public QuoteAggregate.FundingProposalAcceptedEvent RecordFundingProposalAccepted(Guid internalFundingProposalId, Guid? performingUserId, Instant timestamp)
        {
            var fundingProposalCreationResult = this.LatestFundingProposalCreationResult;
            if (fundingProposalCreationResult == null || !fundingProposalCreationResult.IsSuccess)
            {
                throw new ErrorException(Errors.General.NotFound("funding proposal", internalFundingProposalId));
            }

            if (fundingProposalCreationResult.Proposal.InternalId != internalFundingProposalId)
            {
                // there's a mismatch here. This could be because the latest funding proposal hasn't been saved yet, so
                // the formsApp should just retry this request.
                throw new ErrorException(
                    Errors.Payment.Funding.FundingProposalMismatch(
                        internalFundingProposalId, fundingProposalCreationResult.Proposal.InternalId));
            }

            return new QuoteAggregate.FundingProposalAcceptedEvent(this.TenantId, this.AggregateId, this.Id, fundingProposalCreationResult.Proposal, performingUserId, timestamp);
        }

        /// <summary>
        /// Adds or updates a quote document.
        /// </summary>
        /// <param name="quoteDocument">The quote document.</param>
        public void UpsertDocument(QuoteDocument quoteDocument)
        {
            this.quoteDocumentsByName[quoteDocument.Name] = quoteDocument;
        }

        /// <summary>
        /// Attach a file to the quote.
        /// </summary>
        /// <param name="fileAttachment">The quote file attachment.</param>
        public void AttachFile(QuoteFileAttachment fileAttachment)
        {
            this.quoteFileAttachments.Add(fileAttachment);
        }

        /// <summary>
        /// Gets the IDs of the latest quote data updates.
        /// </summary>
        /// <returns>The IDs of the latest quote data updates.</returns>
        public QuoteDataSnapshotIds GetLatestDataSnapshotIds()
        {
            return new QuoteDataSnapshotIds(
                this.LatestFormData.Id,
                this.LatestCalculationResult?.Id,
                this.LatestCustomerDetails?.Id);
        }

        /// <summary>
        /// Gets an attached quote document by name.
        /// </summary>
        /// <param name="name">The name of the document.</param>
        /// <returns>The document, if one with the given name exists, otherwise null.</returns>
        public QuoteDocument GetQuoteDocument(string name)
        {
            if (this.quoteDocumentsByName.TryGetValue(name, out QuoteDocument document))
            {
                return document;
            }

            return null;
        }

        /// <summary>
        /// Approves a quote that is currently for review.
        /// </summary>
        /// <param name="performingUserId">The ID of the user that approved the quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>The resulting state change event.</returns>
        public QuoteAggregate.QuoteStateChangedEvent ApproveReviewedQuote(
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            if (this.LatestCalculationResult == null)
            {
                throw new ErrorException(Errors.Quote.CannotApproveQuoteWithoutCalculation(this.Id));
            }

            var quoteState = this.QuoteStatus;
            var latestCalculationResult = this.LatestCalculationResult.Data;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.ReviewApproval, quoteState))
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.ReviewApproval);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            if (!latestCalculationResult.IsBindable)
            {
                throw new ErrorException(
                    Errors.Quote.CannotBeApprovedWhenNotBindable(
                        latestCalculationResult.CalculationResultState, ReadWriteModel.CalculationResult.BindableState));
            }

            if (latestCalculationResult.HasSoftReferralTriggers
                || latestCalculationResult.HasEndorsementTriggers
                || latestCalculationResult.HasHardReferralTriggers
                || latestCalculationResult.HasDeclinedReferralTriggers)
            {
                throw new ErrorException(Errors.Quote.CannotBeApprovedWithTriggers());
            }

            var @event = new QuoteAggregate.QuoteStateChangedEvent(
                this.TenantId,
                this.AggregateId,
                this.Id,
                QuoteAction.ReviewApproval,
                performingUserId,
                quoteState,
                quoteWorkflow.GetResultingState(QuoteAction.ReviewApproval, quoteState),
                timestamp);
            this.Aggregate.ApplyNewEvent(@event);
            return @event;
        }

        /// <summary>
        /// Automatically approves an incomplete quote without referrals.
        /// </summary>
        /// <param name="performingUserId">The ID of the user that approved the quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <remarks>If previous strata is Review, use <see cref="ApproveReviewedQuote(Guid?, Instant, IQuoteWorkflow)"/> method.</remarks>
        /// <returns>The resulting state change event.</returns>
        public QuoteAggregate.QuoteStateChangedEvent AutoApproveQuote(
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            if (this.LatestCalculationResult == null)
            {
                throw new ErrorException(Errors.Quote.CannotApproveQuoteWithoutCalculation(this.Id));
            }

            var quoteState = this.QuoteStatus;
            var latestCalculationResult = this.LatestCalculationResult.Data;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.AutoApproval, quoteState))
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.AutoApproval);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            if (!latestCalculationResult.IsBindable)
            {
                throw new ErrorException(Errors.Quote.CannotBeApprovedWhenNotBindable(
                    latestCalculationResult.CalculationResultState, ReadWriteModel.CalculationResult.BindableState));
            }

            if (latestCalculationResult.Triggers.Any())
            {
                throw new ErrorException(Errors.Quote.CannotBeApprovedWithTriggers());
            }

            var resultingState = quoteWorkflow.GetResultingState(QuoteAction.AutoApproval, quoteState);
            var @event = new QuoteAggregate.QuoteStateChangedEvent(
                this.TenantId,
                this.AggregateId,
                this.Id,
                QuoteAction.AutoApproval,
                performingUserId,
                quoteState,
                resultingState,
                timestamp);
            this.Aggregate.ApplyNewEvent(@event);
            return @event;
        }

        /// <summary>
        /// Declines a quote.
        /// </summary>
        /// <param name="performingUserId">The ID of the user declining the quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        public void DeclineQuote(
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            bool isMutual)
        {
            this.ThrowIfSubmittedOrPolicyIssued("Decline", isMutual);
            var quoteState = this.QuoteStatus;
            var resultingState = quoteWorkflow.GetResultingState(QuoteAction.Decline, quoteState);
            var @event = new QuoteAggregate.QuoteStateChangedEvent(
                    this.TenantId, this.AggregateId, this.Id, QuoteAction.Decline, performingUserId, quoteState, resultingState, timestamp);

            this.Aggregate.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Refers a quote to an approver for endorsement.
        /// </summary>
        /// <param name="performingUserId">The ID of the user referring a quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>The resulting state change event.</returns>
        public QuoteAggregate.QuoteStateChangedEvent ReferEndorsementQuote(
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            this.ThrowIfQuoteIsDeclined();
            var quoteState = this.QuoteStatus;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.EndorsementReferral, quoteState))
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.EndorsementReferral);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            if (this.LatestCalculationResult == null)
            {
                throw new ErrorException(Errors.Quote.CannotReferQuoteWithoutCalculation(this.Id));
            }

            var latestCalculationResult = this.LatestCalculationResult.Data;
            if (latestCalculationResult.HasEndorsementTriggers || latestCalculationResult.HasSoftReferralTriggers || latestCalculationResult.HasHardReferralTriggers)
            {
                var @event = new QuoteAggregate.QuoteStateChangedEvent(
                    this.TenantId,
                    this.AggregateId,
                    this.Id,
                    QuoteAction.EndorsementReferral,
                    performingUserId,
                    quoteState,
                    quoteWorkflow.GetResultingState(QuoteAction.EndorsementReferral, quoteState),
                    timestamp);
                this.Aggregate.ApplyNewEvent(@event);
                return @event;
            }
            else
            {
                throw new ErrorException(Errors.Quote.CannotBeReferredForEndorsementWithoutTriggers());
            }
        }

        /// <summary>
        /// Approves a quote that is for endorsement.
        /// </summary>
        /// <param name="performingUserId">The ID of the user releasing the quote from referral.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>The resulting state change event.</returns>
        public QuoteAggregate.QuoteStateChangedEvent ApproveEndorsementQuote(
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            var quoteState = this.QuoteStatus;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.EndorsementApproval, quoteState))
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.EndorsementApproval);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            var @event = new QuoteAggregate.QuoteStateChangedEvent(
                this.TenantId,
                this.AggregateId,
                this.Id,
                QuoteAction.EndorsementApproval,
                performingUserId,
                quoteState,
                quoteWorkflow.GetResultingState(QuoteAction.EndorsementApproval, quoteState),
                timestamp);
            this.Aggregate.ApplyNewEvent(@event);
            return @event;
        }

        /// <summary>
        /// Returns a quote to its previous state for completion.
        /// </summary>
        /// <param name="performingUserId">The ID of the user returning the quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>The resulting state change event.</returns>
        public QuoteAggregate.QuoteStateChangedEvent ReturnQuote(
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            var quoteState = this.QuoteStatus;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.Return, quoteState))
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.Return);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            var @event = new QuoteAggregate.QuoteStateChangedEvent(
                this.TenantId,
                this.AggregateId,
                this.Id,
                QuoteAction.Return,
                performingUserId,
                quoteState,
                quoteWorkflow.GetResultingState(QuoteAction.Return, quoteState),
                timestamp);
            this.Aggregate.ApplyNewEvent(@event);
            return @event;
        }

        /// <summary>
        /// Refers a quote for approval.
        /// </summary>
        /// <param name="performingUserId">The ID of the user that reviewed the quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <returns>The resulting state change event.</returns>
        public QuoteAggregate.QuoteStateChangedEvent ReferQuoteForReview(
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow)
        {
            this.ThrowIfQuoteIsDeclined();
            var quoteState = this.QuoteStatus;
            if (!quoteWorkflow.IsActionPermittedByState(QuoteAction.ReviewReferral, quoteState))
            {
                var operation = quoteWorkflow.GetOperation(QuoteAction.ReviewReferral);
                throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                    operation.Action.Humanize(),
                    quoteState,
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            if (this.LatestCalculationResult == null)
            {
                throw new ErrorException(Errors.Quote.CannotReferQuoteWithoutCalculation(this.Id));
            }

            var latestCalculationResult = this.LatestCalculationResult.Data;
            if (!latestCalculationResult.HasReviewCalculationTriggers)
            {
                throw new InvalidOperationException($"Cannnot review a quote that has no review triggers");
            }

            var @event = new QuoteAggregate.QuoteStateChangedEvent(
                this.TenantId,
                this.AggregateId,
                this.Id,
                QuoteAction.ReviewReferral,
                performingUserId,
                quoteState,
                quoteWorkflow.GetResultingState(QuoteAction.ReviewReferral, quoteState),
                timestamp);
            this.Aggregate.ApplyNewEvent(@event);
            return @event;
        }

        public void ProgressQuoteState(
            IQuoteWorkflow quoteWorkflow, QuoteAction quoteActions, Guid? performingUserId, Instant timestamp)
        {
            var resultingState = quoteWorkflow.GetResultingState(quoteActions, this.QuoteStatus);
            var stateChangeEvent = new QuoteAggregate.QuoteStateChangedEvent(
                this.TenantId, this.AggregateId, this.Id, quoteActions, performingUserId, this.QuoteStatus, resultingState, timestamp);
            this.Aggregate.ApplyNewEvent(stateChangeEvent);
        }

        public void ThrowIfActiveTriggersAndNotBindable()
        {
            var calculationData = this.LatestCalculationResult.Data;
            if (!calculationData.IsBindable && calculationData.Triggers.Any())
            {
                throw new InvalidCalculationTriggerException(calculationData.Triggers.First().ErrorMessage);
            }
        }

        /// <summary>
        /// Handle state change events.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Apply(QuoteAggregate.QuoteStateChangedEvent @event, int sequenceNumber)
        {
            this.PreviousQuoteStateChange = this.LatestQuoteStateChange;
            this.LatestQuoteStateChange
                = new QuoteStateChange(
                    @event.OperationName,
                    @event.UserId,
                    @event.OriginalState,
                    @event.ResultingState,
                    @event.Timestamp);
            if (!this.IsActualised)
            {
                if (@event.OperationName == QuoteAction.Actualise.Humanize()
                    || @event.OperationName == QuoteAction.AutoApproval.Humanize()
                    || @event.OperationName == QuoteAction.ReviewReferral.Humanize()
                    || @event.OperationName == QuoteAction.EndorsementReferral.Humanize()
                    || @event.OperationName == QuoteAction.Bind.Humanize()
                    || @event.OperationName == QuoteAction.Policy.Humanize())
                {
                    this.IsActualised = true;
                }
            }
        }

        /// <summary>
        /// Handle policy issued event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Apply(QuoteAggregate.PolicyIssuedEvent @event, int sequenceNumber)
        {
            this.TransactionCompleted = true;
        }

        /// <summary>
        /// Handle policy adjusted event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Apply(QuoteAggregate.PolicyAdjustedEvent @event, int sequenceNumber)
        {
            this.TransactionCompleted = true;
        }

        /// <summary>
        /// Handle policy renewed event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Apply(QuoteAggregate.PolicyRenewedEvent @event, int sequenceNumber)
        {
            this.TransactionCompleted = true;
        }

        /// <summary>
        /// Handle policy cancelled event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Apply(QuoteAggregate.PolicyCancelledEvent @event, int sequenceNumber)
        {
            this.TransactionCompleted = true;
        }

        /// <summary>
        /// Handle funding proposal accepted event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Apply(QuoteAggregate.FundingProposalAcceptedEvent @event, int sequenceNumber)
        {
            var result = FundingProposalAcceptanceResult.CreateSuccessResponse(@event.FundingProposal, @event.Timestamp);
            this.RecordFundingProposalAcceptanceResult(result);
        }

        /// <summary>
        /// Handle quote actualised event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Apply(QuoteAggregate.QuoteActualisedEvent @event, int sequenceNumber)
        {
            this.IsActualised = true;
        }

        public void Apply(QuoteAggregate.MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent @event, int sequenceNumber)
        {
            this.ProductReleaseId = @event.NewProductReleaseId;
        }

        public void Apply(QuoteAggregate.MigrateUnassociatedEntitiesToProductReleaseEvent @event, int sequenceNumber)
        {
            this.ProductReleaseId = @event.NewProductReleaseId;
        }

        /// <inheritdoc/>
        public Result<IEnumerable<DataPatchTargetEntity>> SelectAndValidateCalculationResultPatchTargets(PolicyDataPatchCommand command)
        {
            if (command.TargetCalculationResultPath == null)
            {
                return Result.Success(Enumerable.Empty<DataPatchTargetEntity>());
            }

            var patchTargets = new List<DataPatchTargetEntity>();
            var targetCalculationResults = new List<ReadWriteModel.CalculationResult>();
            if (command.Scope.Applicable(this))
            {
                var latestCalculationResult = this.LatestCalculationResult?.Data;
                if (latestCalculationResult != null)
                {
                    // Ignore quotes without calculation results.
                    patchTargets.Add(new QuotDataPatchTarget(this.Id));
                    targetCalculationResults.Add(latestCalculationResult);
                }
            }

            foreach (var quoteVersion in this.quoteVersions)
            {
                if (command.Scope.Applicable(this, quoteVersion))
                {
                    var versionCalculationResult = quoteVersion.DataSnapshot.CalculationResult.Data;
                    if (versionCalculationResult != null)
                    {
                        // Ignore if calculation result does not exist.
                        patchTargets.Add(new QuoteVersionDataPatchTarget(this.Id, quoteVersion.Number));
                        targetCalculationResults.Add(versionCalculationResult);
                    }
                }
            }

            foreach (var calculationResult in targetCalculationResults)
            {
                var canPatchresult = calculationResult.CanPatchProperty(command.TargetCalculationResultPath, command.Rules);
                if (!canPatchresult.IsSuccess)
                {
                    var errorMessage = string.Format(
                        "Could not patch quote {0} calculation result property: {1} (quote or quote version): {2}",
                        this.Id,
                        command.TargetCalculationResultPath.Value,
                        canPatchresult.Error);
                    return Result.Failure<IEnumerable<DataPatchTargetEntity>>(errorMessage);
                }
            }

            return Result.Success<IEnumerable<DataPatchTargetEntity>>(patchTargets);
        }

        /// <inheritdoc/>
        public Result<IEnumerable<DataPatchTargetEntity>> SelectAndValidateFormDataPatchTargets(PolicyDataPatchCommand command)
        {
            if (command.TargetFormDataPath == null)
            {
                return Result.Success(Enumerable.Empty<DataPatchTargetEntity>());
            }

            var patchTargets = new List<DataPatchTargetEntity>();
            var targetFormDatas = new List<FormData>();
            if (command.Scope.Applicable(this))
            {
                var latestFormData = this.LatestFormData?.Data;
                if (latestFormData == null)
                {
                    return Result.Failure<IEnumerable<DataPatchTargetEntity>>(
                        $"Could not find form data for quote {this.Id}.");
                }

                patchTargets.Add(new QuotDataPatchTarget(this.Id));
                targetFormDatas.Add(latestFormData);
            }

            foreach (var quoteVersion in this.quoteVersions)
            {
                if (command.Scope.Applicable(this, quoteVersion))
                {
                    var versionFormData = quoteVersion.DataSnapshot.FormData.Data;
                    if (versionFormData == null)
                    {
                        return Result.Failure<IEnumerable<DataPatchTargetEntity>>(
                            $"Could not find form data for quote / version {this.Id} / {quoteVersion.Number}.");
                    }

                    patchTargets.Add(new QuoteVersionDataPatchTarget(this.Id, quoteVersion.Number));
                    targetFormDatas.Add(versionFormData);
                }
            }

            foreach (var formData in targetFormDatas)
            {
                var canPatchResult = formData.CanPatchFormModelProperty(command.TargetFormDataPath, command.Rules);
                if (canPatchResult.IsFailure)
                {
                    var errorMessage = string.Format(
                        "Could not patch quote {0} form data property: {1} (quote or quote version): {2}",
                        this.Id,
                        command.TargetFormDataPath.Value,
                        canPatchResult.Error);
                    return Result.Failure<IEnumerable<DataPatchTargetEntity>>(errorMessage);
                }
            }

            return Result.Success<IEnumerable<DataPatchTargetEntity>>(patchTargets);
        }

        /// <summary>
        /// Patch form data in quote and versions.
        /// </summary>
        /// <param name="patch">The patch to apply.</param>
        public void ApplyPatch(PolicyDataPatch patch)
        {
            if (patch.IsApplicable(this))
            {
                if (patch.Type == DataPatchType.FormData)
                {
                    this.LatestFormData.Data.PatchFormModelProperty(patch.Path, patch.Value);
                }
                else if (patch.Type == DataPatchType.CalculationResult)
                {
                    this.LatestCalculationResult.Data.PatchProperty(patch.Path, patch.Value);
                }
            }

            foreach (var version in this.quoteVersions)
            {
                if (patch.IsApplicable(this, version))
                {
                    if (patch.Type == DataPatchType.FormData)
                    {
                        version.DataSnapshot.FormData.Data.PatchFormModelProperty(patch.Path, patch.Value);
                    }
                    else if (patch.Type == DataPatchType.CalculationResult)
                    {
                        version.DataSnapshot.CalculationResult.Data.PatchProperty(patch.Path, patch.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve the latest quote version.
        /// </summary>
        /// <returns>The version id.</returns>
        public QuoteVersion GetLatestVersion()
        {
            if (this.Versions == null)
            {
                return null;
            }

            return this.Versions.OrderBy(x => x.EventSequenceNumber).LastOrDefault();
        }

        /// <summary>
        /// Gets a value indicating whether the quote can be edited.
        /// If the quote is complete, submitted, discarded, or expired then it cannot be edited.
        /// </summary>
        public bool IsEditable(Instant now)
        {
            return
                !this.QuoteStatus.EqualsIgnoreCase(StandardQuoteStates.Complete)
                && !this.IsSubmitted
                && !this.IsDiscarded
                && !this.IsExpired(now);
        }

        /// <summary>
        /// Remove the field values that are configured to be reset.
        /// </summary>
        /// <param name="formDataSchema">The form data schema.</param>
        public virtual UBind.Domain.Aggregates.Quote.FormData RemoveFieldValuesThatAreConfiguredToBeReset(IFormDataSchema formDataSchema)
        {
            var questionMetaData = formDataSchema?.GetQuestionMetaData();

            if (questionMetaData == null)
            {
                return this.LatestFormData.Data;
            }

            questionMetaData
                .Where(w => w.ResetForNewQuotes == true)
                .ToList()
                .ForEach(property => this.LatestFormData.Data.RemoveFormDataProperty(property.Key));

            return this.LatestFormData.Data;
        }

        public virtual void ThrowIfActiveWhenTryingToCancel(Instant now, bool isMutual)
        {
            throw new NotImplementedException("When trying to determine if the policy can be cancelled, could not deal with a conflict because the quote type is not recognised.");
        }

        public virtual void ThrowIfActiveWhenTryingToRenew(Instant now, bool isMutual)
        {
            throw new NotImplementedException("When trying to determine if the policy can be renewed, could not deal with a conflict because the quote type is not recognised.");
        }

        public virtual void ThrowIfActiveWhenTryingToAdjust(Instant now, bool isMutual)
        {
            throw new NotImplementedException("When trying to determine if the policy can be adjusted, could not deal with a conflict because the quote type is not recognised.");
        }

        public bool IsQuoteActive()
        {
            return !this.TransactionCompleted && !this.IsDiscarded && this.QuoteStatus != StandardQuoteStates.Declined;
        }

        public void ThrowIfGivenCalculationInvalidatesApprovedQuote(
            IFormDataSchema formDataSchema)
        {
            this.ThrowIfCalculationInvalidatesApprovedQuote(
                this.LatestCalculationResult?.Data,
                formDataSchema);
        }

        public void ThrowIfGivenCalculationIdNotMatchingTheLatest(Guid? calculationId)
        {
            if (calculationId.HasValue && calculationId.Value != this.LatestCalculationResult?.Id)
            {
                throw new ErrorException(Errors.Operations.Bind.CalculationIdPassedIsNotTheLatestForQuote());
            }
        }

        public void SetReadModel(NewQuoteReadModel quote)
        {
            if (quote != null)
            {
                this.ReadModel = quote;
            }
        }

        protected QuoteDataSnapshot GetQuoteDataSnapshotForPolicyTransaction(Guid calculationResultId)
        {
            // TODO: Currently we are using the specified calculation result, and taking date information
            // from the related form update, we are using the latest form update in the policy too.
            // This is not good, since it may be different, however we want to capture post-calculation data
            // too, so this is not simple to resolve at present.
            // We probably need to store both.
            return new QuoteDataSnapshot(this.LatestFormData, this.LatestCalculationResult, this.LatestCustomerDetails);
        }

        /// <summary>
        /// Usually there is a time of day scheme in place, which has the following rule:
        /// If the effective date of the new business policy or adjustment transaction is
        /// today's date, then we can make that policy transaction active immediately, which means we set
        /// the effective time of day to the current time. This method will adjust the time of day
        /// according to those rules.
        /// </summary>
        protected LocalDateTime GetEffectiveDateTimeForEffectiveDateUsingTimeOfDayScheme(
            LocalDate effectiveDate,
            Instant now,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
        {
            ZonedDateTime nowZonedDateTime = now.InZone(this.TimeZone);
            var todaysDate = nowZonedDateTime.Date;
            LocalDateTime effectiveDateTime = effectiveDate.At(timeOfDayScheme.GetEndTime());
            if (effectiveDate.Equals(todaysDate))
            {
                var nowLocalDateTime = nowZonedDateTime.LocalDateTime;
                if ((nowLocalDateTime > effectiveDateTime && !timeOfDayScheme.DoesAllowInceptionTimeInThePast)
                    || (effectiveDateTime > nowLocalDateTime && timeOfDayScheme.DoesAllowImmediateCoverage))
                {
                    effectiveDateTime = nowLocalDateTime;
                }
            }

            return effectiveDateTime;
        }

        private QuoteDataSnapshot GetDataSnapshot()
        {
            return new QuoteDataSnapshot(
                this.LatestFormData,
                this.LatestCalculationResult,
                this.LatestCustomerDetails);
        }

        private void ThrowIfSubmittedOrPolicyIssued(string failedOperation, bool isMutual)
        {
            var humanizedOperation = failedOperation.Humanize(LetterCasing.LowerCase);
            if (this.IsSubmitted)
            {
                throw new ErrorException(Errors.Quote.CannotPerformOperationOnSubmittedQuote(humanizedOperation));
            }

            if (this.TransactionCompleted)
            {
                throw new ErrorException(Errors.Quote.CannotPerformOperationOnIssuedPolicy(humanizedOperation, isMutual));
            }
        }

        private void ThrowIfQuoteIsDeclined()
        {
            var latestCalculationResult = this.LatestCalculationResult.Data;
            if (latestCalculationResult.HasDeclinedReferralTriggers)
            {
                throw new ErrorException(Errors.Quote.CannotReferWhenDeclined());
            }
        }

        /// <summary>
        /// Throws an exception if the quote is approved, but the new calculation returns new referral triggers, or
        /// changes questions answer which are not allowed to be changed once the quote is approved.
        /// </summary>
        private void ThrowIfCalculationInvalidatesApprovedQuote(
            CalculationResult newCalculationResult,
            IFormDataSchema formDataSchema)
        {
            if (!this.QuoteStatus.Equals(StandardQuoteStates.Approved, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!newCalculationResult.Triggers.Any())
            {
                return;
            }

            if (this.LatestCalculationResult?.Data != null)
            {
                var approvedTriggerNames = this.LatestCalculationResult.Data.Triggers.Select(t => t.Name);
                var hasNewTriggers = newCalculationResult.Triggers
                    .Select(t => t.Name)
                    .Any(n => !approvedTriggerNames.Contains(n));
                if (hasNewTriggers)
                {
                    throw new ErrorException(Errors.Quote.NewReferralTriggeredAfterQuoteApproved());
                }
            }
        }
    }
}
