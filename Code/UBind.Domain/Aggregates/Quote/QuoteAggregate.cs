// <copyright file="QuoteAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// Suppress IDE0060 because there are Apply Event method in which parameter are not in used.
// And we cannot remove the apply method otherwise it will throw an exception.
#pragma warning disable IDE0060 // Remove unused parameter

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Microsoft.AspNetCore.JsonPatch;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Aggregates.Quote.CustomerFormDataPatch;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Imports;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Services;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
        : AggregateRootEntity<QuoteAggregate, Guid>,
        IProductContext,
        IAdditionalPropertyValueEntityAggregate,
        IAdditionalProperties,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueInitializedEvent<QuoteAggregate, IQuoteEventObserver>>,
        IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueUpdatedEvent<QuoteAggregate, IQuoteEventObserver>>
    {
        private readonly IList<Quote> quotes = new List<Quote>();
        private readonly IDictionary<string, QuoteDocument> policyDocumentsByName
            = new Dictionary<string, QuoteDocument>();

        private QuoteCustomerAssociationInvitation quoteAssociationInvitation;

        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public QuoteAggregate(
            Guid tenantId,
            Guid id,
            Guid productId,
            Guid organisationId,
            DeploymentEnvironment environment,
            Policy? policy,
            Guid? customerId,
            Guid? ownerUserId,
            bool isTestData,
            bool isBeingReplayed,
            long createdTicksSinceEpoch,
            long lastModifiedTicksSinceEpoch,
            List<AdditionalPropertyValue> additionalPropertyValues,
            IList<Quote> quotes,
            IDictionary<string, QuoteDocument> policyDocumentsByName)
        {
            this.Id = id;
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.OrganisationId = organisationId;
            this.Environment = environment;
            this.Policy = policy;
            this.CustomerId = customerId;
            this.OwnerUserId = ownerUserId;
            this.IsTestData = isTestData;
            this.IsBeingReplayed = isBeingReplayed;
            this.CreatedTicksSinceEpoch = createdTicksSinceEpoch;
            this.LastModifiedTicksSinceEpoch = lastModifiedTicksSinceEpoch;
            this.AdditionalPropertyValues = additionalPropertyValues ?? new List<AdditionalPropertyValue>();
            this.quotes = quotes;
            this.policyDocumentsByName = policyDocumentsByName ?? new Dictionary<string, QuoteDocument>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteAggregate"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        private QuoteAggregate(
            IEnumerable<IEvent<QuoteAggregate, Guid>> events)
            : base(events)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="QuoteAggregate"/> class from a creation event.
        /// </summary>
        /// <param name="event">The event used to create this quote.</param>
        private QuoteAggregate(Event<QuoteAggregate, Guid> @event)
        {
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="QuoteAggregate"/> class from a creation event
        /// and sets the quote expiry from the passed expiry settings.
        /// </summary>
        /// <param name="event">The creation event.</param>
        /// <param name="quoteId">The Id of the quote.</param>
        /// <param name="performingUserId">The Id of the performing user, if any.</param>
        /// <param name="createdTimestamp">The creation timestamp.</param>
        /// <param name="quoteExpirySettings">The expiry settings to be set for this quote.</param>
        private QuoteAggregate(Event<QuoteAggregate, Guid> @event, Guid quoteId, Guid? performingUserId, Instant createdTimestamp, IQuoteExpirySettings quoteExpirySettings)
        {
            this.ApplyNewEvent(@event);

            // Functionality relying on initialized data (e.g. product context, etc.) must occur after initialization.
            this.SetQuoteExpiryFromSettings(quoteId, performingUserId, createdTimestamp, quoteExpirySettings);
        }

        private QuoteAggregate(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment env,
            Guid? personId,
            Guid? customerId,
            IPersonalDetails personalDetails,
            string policyNumber,
            string policyTitle,
            LocalDate inceptionDate,
            LocalDate? expiryDate,
            FormData formData,
            JObject calculationResult,
            bool isTestData,
            DateTimeZone timeZone,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            Guid? performingUserId,
            Instant timestamp,
            Guid? productReleaseId)
        {
            var policyIssued = new PolicyIssuedWithoutQuoteEvent(
                tenantId,
                Guid.NewGuid(),
                organisationId,
                productId,
                env,
                personId,
                customerId,
                personalDetails,
                policyNumber,
                policyTitle,
                inceptionDate,
                expiryDate,
                formData,
                calculationResult,
                isTestData,
                timeZone,
                timeOfDayScheme,
                performingUserId,
                timestamp,
                productReleaseId);
            this.ApplyNewEvent(policyIssued);
        }

        public override AggregateType AggregateType => AggregateType.Quote;

        /// <summary>
        /// Gets the ID of the tenant the quote is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the organisation the quote is for.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the ID of the product the quote is for.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; private set; }

        /// <summary>
        /// Gets the ID of the environment the quote belongs to.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the policy issued if any, otherwise null.
        /// </summary>
        public Policy? Policy { get; private set; }

        /// <summary>
        /// Gets the user ID of the current owner, if any, otherwise null.
        /// </summary>
        public Guid? OwnerUserId { get; private set; }

        /// <summary>
        /// Gets the ID of the customer the quote is assigned to, if any, otherwise default.
        /// </summary>
        public Guid? CustomerId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote is assigned to a customer.
        /// </summary>
        public bool HasCustomer => this.CustomerId.HasValue && this.CustomerId.Value != Guid.Empty;

        /// <summary>
        /// Gets the product context the quote belongs to.
        /// </summary>
        [JsonIgnore]
        public ProductContext ProductContext => new ProductContext(this.TenantId, this.ProductId, this.Environment);

        /// <summary>
        /// Gets the current collection of additional property values.
        /// </summary>
        public List<AdditionalPropertyValue> AdditionalPropertyValues { get; private set; }
            = new List<AdditionalPropertyValue>();

        public IList<Quote> Quotes => this.quotes;

        public IDictionary<string, QuoteDocument> PolicyDocumentsByName => this.policyDocumentsByName;

        /// <summary>
        /// Creates a new quote for new business.
        /// </summary>
        /// <returns>The current quote.</returns>
        public static Quote CreateNewBusinessQuote(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            IQuoteExpirySettings quoteExpirySettings,
            Guid? performingUserId,
            Instant createdTimestamp,
            Guid? productReleaseId,
            DateTimeZone? timeZone = null,
            bool areTimestampsAuthoritative = false,
            Guid? customerId = null,
            bool isTestData = false,
            JObject? formData = null,
            string? quoteNumber = null,
            string? initialQuoteState = null,
            List<AdditionalPropertyValueUpsertModel>? additionalProperties = null)
        {
            timeZone = timeZone ?? Timezones.AET;
            var quoteId = Guid.NewGuid();
            var initializedEvent = new QuoteInitializedEvent(
                tenantId,
                Guid.NewGuid(),
                quoteId,
                organisationId,
                productId,
                environment,
                QuoteType.NewBusiness,
                performingUserId,
                createdTimestamp,
                timeZone ?? Timezones.AET,
                areTimestampsAuthoritative,
                customerId,
                isTestData,
                productReleaseId,
                default,
                formData != null ? formData.ToString() : null,
                quoteNumber,
                initialQuoteState,
                additionalProperties);
            var aggregate = new QuoteAggregate(
                initializedEvent,
                quoteId,
                performingUserId,
                createdTimestamp,
                quoteExpirySettings);
            return aggregate.quotes[0];
        }

        public static QuoteAggregate CreateFromExistingPolicy(
           Guid tenantId,
           Guid organisationId,
           Guid productId,
           DeploymentEnvironment env,
           Guid aggregateId,
           Guid personId,
           Guid customerId,
           IPersonalDetails personalDetails,
           Guid formDataId,
           string formData,
           string calculationResult,
           string policyNumber,
           LocalDateTime inceptionDateTime,
           Instant inceptionTimestamp,
           LocalDateTime expiryDateTime,
           Instant expiryTimestamp,
           DateTimeZone timeZone,
           Guid? performingUserId,
           Instant timestamp,
           Guid? productReleaseId)
        {
            var fromPolicyEvent = new AggregateCreationFromPolicyEvent(
               tenantId,
               organisationId,
               productId,
               env,
               aggregateId,
               personId,
               customerId,
               personalDetails,
               formData,
               calculationResult,
               policyNumber,
               inceptionDateTime,
               inceptionTimestamp,
               expiryDateTime,
               expiryTimestamp,
               timeZone,
               performingUserId,
               timestamp,
               productReleaseId);
            return new QuoteAggregate(fromPolicyEvent);
        }

        /// <summary>
        /// Creates a new quote from imported policy.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="organisationId">The ID of the organisation the quote belongs to.</param>
        /// <param name="productId">The ID of the product the quote is for.</param>
        /// <param name="env">The environment the quote is being created in.</param>
        /// <param name="personId">The ID of the person.</param>
        /// <param name="customerId">The ID of the customer.</param>
        /// <param name="personalDetails">The personal details of the quote.</param>
        /// <param name="data">The policy import data object.</param>
        /// <param name="timeOfDayScheme">The scheme for specifying times of day policies start and end.</param>
        /// <param name="performingUserId">The userId who imported policy.</param>
        /// <param name="timestamp">The time the quote is being created.</param>
        /// <returns>A new instance of <see cref="QuoteAggregate"/> representing the imported application.</returns>
        public static QuoteAggregate CreateImportedPolicy(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment env,
            Guid personId,
            Guid customerId,
            IPersonalDetails personalDetails,
            PolicyImportData data,
            DateTimeZone timeZone,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            Guid? performingUserId,
            Instant timestamp,
            Guid? productReleaseId,
            UserReadModel? owner = null)
        {

            var importEvent = new PolicyImportedEvent(
                tenantId,
                Guid.NewGuid(),
                organisationId,
                productId,
                env,
                personId,
                customerId,
                personalDetails,
                timeZone,
                data,
                timeOfDayScheme,
                performingUserId,
                timestamp,
                productReleaseId,
                owner);
            return new QuoteAggregate(importEvent);
        }

        public static QuoteAggregate CreateImportedQuote(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment env,
            Guid customerId,
            IPersonalDetails personalDetails,
            QuoteImportData importData,
            DateTimeZone timeZone,
            IQuoteExpirySettings quoteExpirySettings,
            Guid? performingUserId,
            Instant timestamp,
            Guid? productReleaseId)
        {
            var quoteId = Guid.NewGuid();
            var quoteImportedEvent = new QuoteImportedEvent(
                tenantId,
                Guid.NewGuid(),
                quoteId,
                organisationId,
                productId,
                env,
                performingUserId,
                timestamp,
                timeZone,
                customerId,
                personalDetails,
                importData,
                productReleaseId);
            return new QuoteAggregate(quoteImportedEvent, quoteId, performingUserId, timestamp, quoteExpirySettings);
        }

        /// <summary>
        /// Creates a new quote from imported policy.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote belongs to.</param>
        /// <param name="organisationId">The ID of the organisation the quote belongs to.</param>
        /// <param name="productId">The ID of the product the quote is for.</param>
        /// <param name="env">The environment the quote is being created in.</param>
        /// <param name="personId">The ID of the person, optional.</param>
        /// <param name="customerId">The ID of the customer, optional.</param>
        /// <param name="personalDetails">The personal details of the quote, optional.</param>
        /// <param name="timeOfDayScheme">The scheme for specifying times of day policies start and end.</param>
        /// <param name="performingUserId">The userId who imported policy.</param>
        /// <param name="timestamp">The time the quote is being created.</param>
        /// <returns>A new instance of <see cref="QuoteAggregate"/> representing the imported application.</returns>
        public static QuoteAggregate CreatePolicy(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment env,
            Guid? personId,
            Guid? customerId,
            IPersonalDetails? personalDetails,
            string policyNumber,
            string policyTitle,
            LocalDate inceptionDate,
            LocalDate? expiryDate,
            FormData formData,
            JObject calculationResult,
            bool isTestData,
            DateTimeZone timeZone,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            Guid? performingUserId,
            Instant timestamp,
            Guid? productReleaseId)
        {
            return new QuoteAggregate(
                tenantId,
                organisationId,
                productId,
                env,
                personId,
                customerId,
                personalDetails,
                policyNumber,
                policyTitle,
                inceptionDate,
                expiryDate,
                formData,
                calculationResult,
                isTestData,
                timeZone,
                timeOfDayScheme,
                performingUserId,
                timestamp,
                productReleaseId);
        }

        /// <summary>
        /// Loads a quote from the event store.
        /// </summary>
        /// <param name="events">Existing events.</param>
        /// <returns>A new instance of <see cref="QuoteAggregate"/>, loaded from the event store.</returns>
        public static QuoteAggregate LoadFromEvents(
            IEnumerable<IEvent<QuoteAggregate, Guid>> events)
        {
            return new QuoteAggregate(events);
        }

        /// <summary>
        /// Get a quote by ID and throw if null.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>The quote, if found.</returns>
        /// <exception cref="ErrorException">Thrown if the quote is not found.</exception>
        public Quote GetQuoteOrThrow(Guid quoteId)
        {
            var quote = this.GetQuote(quoteId);
            if (quote == null)
            {
                EntityHelper.ThrowIfNotFound(quote, quoteId, "quote");
            }

            return quote;
        }

        /// <summary>
        /// Get a quote by ID.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <returns>The quote.</returns>
        public Quote? GetQuote(Guid quoteId)
        {
            return this.quotes.FirstOrDefault(quote => quote.Id == quoteId);
        }

        /// <summary>
        /// Gets quotes which have not been discarded.
        /// </summary>
        public IEnumerable<Quote> GetQuotes()
        {
            return this.quotes.Where(q => !q.IsDiscarded);
        }

        /// <summary>
        /// Get the latest quote created.
        /// </summary>
        /// <returns>The quote, if found, otherwise null.</returns>
        public Quote GetLatestQuote()
        {
            return this.quotes.LastOrDefault();
        }

        /// <summary>
        /// Creates a new quote as a clone from the most recent quote.
        /// </summary>
        /// <param name="quoteId">The quote identifier to base the replacement from.</param>
        /// <param name="quoteNumber">The quote number.</param>
        /// <param name="performingUserId">The userId who created clone for expired quote.</param>
        /// <param name="timestamp">The time the adjustment quote is being created.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="formDataSchema">The form data schema.</param>
        /// <returns> Quote id of the new quote.</returns>
        public Guid CreateCloneQuoteForExpiredQuote(
            Guid quoteId,
            string quoteNumber,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            FormDataSchema formDataSchema = null)
        {
            var quote = this.quotes.FirstOrDefault(x => x.Id == quoteId);

            if (quote == null)
            {
                throw new NotFoundException(Errors.General.NotFound("quote", quoteId));
            }

            if (quote.Type == QuoteType.NewBusiness)
            {
                throw new InvalidOperationException("You cannot create a new quote that has type New Business.");
            }

            FormData formData = quote.RemoveFieldValuesThatAreConfiguredToBeReset(formDataSchema);
            Guid newQuoteId = Guid.Empty;
            if (quote.Type == QuoteType.Renewal)
            {
                var renewalEvent = new RenewalQuoteCreatedEvent(
                    this.TenantId,
                    this.Id,
                    this.OrganisationId,
                    quoteNumber,
                    formData.Json,
                    performingUserId,
                    timestamp,
                    quote.ProductReleaseId);
                newQuoteId = renewalEvent.QuoteId;
                this.ApplyNewEvent(renewalEvent);
            }
            else if (quote.Type == QuoteType.Adjustment)
            {
                var adjustmentEvent = new AdjustmentQuoteCreatedEvent(
                      this.TenantId,
                      this.Id,
                      this.OrganisationId,
                      quoteNumber,
                      formData.Json,
                      this.Policy.QuoteId.Value,
                      performingUserId,
                      timestamp,
                      quote.ProductReleaseId);
                newQuoteId = adjustmentEvent.QuoteId;
                this.ApplyNewEvent(adjustmentEvent);
            }
            else if (quote.Type == QuoteType.Cancellation)
            {
                var cancellationEvent = new CancellationQuoteCreatedEvent(
                         this.TenantId,
                         this.Id,
                         this.OrganisationId,
                         quoteNumber,
                         this.Policy.QuoteId.Value,
                         performingUserId,
                         timestamp,
                         quote.ProductReleaseId,
                         formData?.Json);
                newQuoteId = cancellationEvent.QuoteId;
                this.ApplyNewEvent(cancellationEvent);
            }

            var resultingState = quoteWorkflow.GetResultingState(
                QuoteAction.Actualise, QuoteStatus.Nascent.Humanize());
            var stateChangeEvent = new QuoteStateChangedEvent(
                this.TenantId,
                this.Id,
                newQuoteId,
                QuoteAction.Actualise,
                default,
                QuoteStatus.Nascent.Humanize(),
                resultingState,
                timestamp);
            this.ApplyNewEvent(stateChangeEvent);

            return newQuoteId;
        }

        /// <summary>
        /// Binds the quote.
        /// </summary>
        /// <param name="performingUserId">The ID of the user binding the quote.</param>
        /// <param name="calculationResultId">The Id of the calculation result used in the binding process.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// /// <param name="quoteId">The Id of the quote.</param>
        public void RecordQuoteBinding(
            Guid? performingUserId,
            Guid calculationResultId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);
            IEnumerable<IEvent<QuoteAggregate, Guid>> events = quote.RecordQuoteBinding(
                performingUserId,
                calculationResultId,
                timestamp,
                quoteWorkflow);
            foreach (IEvent<QuoteAggregate, Guid> @event in events)
            {
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Update the customer details for the quote.
        /// </summary>
        /// <param name="customerDetails">The new customer details.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="createdTimestamp">A created timestamp.</param>
        /// <param name="quoteId">The Id of the quote.</param>
        public void UpdateCustomerDetails(
            IPersonalDetails customerDetails, Guid? performingUserId, Instant createdTimestamp, Guid quoteId)
        {
            var @event = new CustomerDetailsUpdatedEvent(
                this.TenantId, this.Id, quoteId, customerDetails, performingUserId, createdTimestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Creates a numbered version of the quote with the current data.
        /// </summary>
        /// <param name="performingUserId">The userId who created version.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteId">The Id of the quote.</param>
        public void CreateVersion(Guid? performingUserId, Instant timestamp, Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);

            var @event = new QuoteVersionCreatedEvent(this.TenantId, this.Id, Guid.NewGuid(), quote, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Marks the quote as a discarded quote.
        /// </summary>
        /// <param name="quoteId">The quote id to discard.</param>
        /// <param name="performingUserId">The userId who discard the quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void Discard(Guid quoteId, Guid? performingUserId, Instant timestamp)
        {
            var quote = this.GetQuoteOrThrow(quoteId);

            if (quoteId != quote.Id)
            {
                // Until we change the rule about there being one quote we must throw an exception here
                // if the quote id doesn't match the current quoteId.
                throw new InvalidOperationException(
                    "There is a current business rule that there can be only one active quote per aggregate "
                    + " and on this QuoteAggregate with id, " + this.Id + " that quote ID is "
                    + quote.Id + ". However a request was made to discard the "
                    + "quote with ID " + quoteId + " which is not the current quote. Something is amiss.");
            }

            var @event = new QuoteDiscardEvent(this.TenantId, this.Id, quoteId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Deletes the policy and all its associated records.
        /// </summary>
        /// <param name="performingUserId">The ID of the user who deleted the policy.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void DeletePolicyRecords(Guid? performingUserId, Instant timestamp)
        {
            var @event = new PolicyDeletedEvent(this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Rolls back this quote aggregate to an earlier state.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the quote event to roll back to.</param>
        /// <param name="performingUserId">The userId who roll back the quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void RollbackTo(int sequenceNumber, Guid? performingUserId, Instant timestamp)
        {
            var quote = this.GetQuoteBySequenceNumber(sequenceNumber);

            if (this.PersistedEventCount == 0)
            {
                throw new ErrorException(Errors.Maintenance.Rollback.NoPriorSequenceNumbers());
            }

            int currentSequenceNumber = this.PersistedEventCount - 1;
            if (sequenceNumber == currentSequenceNumber)
            {
                throw new ErrorException(Errors.Maintenance.Rollback.CannotRollbackToSameSequenceNumber(
                    sequenceNumber, currentSequenceNumber));
            }

            if (sequenceNumber >= this.PersistedEventCount)
            {
                throw new ErrorException(Errors.Maintenance.Rollback.TargetSequenceNumberNotFound(
                    sequenceNumber, currentSequenceNumber));
            }

            QuoteRollbackEvent @event = quote.RollbackToSequenceNumber(
                sequenceNumber,
                performingUserId,
                timestamp,
                this.GetRemainingEventsAfterRollbacks,
                this.GetEventsStrippedByRollbacks);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record an enquiry for the quote.
        /// </summary>
        /// <param name="performingUserId">The userId who makes Enquiry.</param>
        /// <param name="timestamp">The time of the submission.</param>
        /// <param name="quoteId">The id of the quote.</param>
        public void MakeEnquiry(Guid? performingUserId, Instant timestamp, Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);

            var @event = new EnquiryMadeEvent(
                this.TenantId, this.Id, quote, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Issue an invoice.
        /// </summary>
        /// <param name="invoiceNumber">The invoice number to be used for the invoice.</param>
        /// <param name="performingUserId">The userId who issued the invoice.</param>
        /// <param name="timestamp">The time and date of issuing the invoice.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="quoteId">The id of the quote.</param>
        /// <param name="fromBind">A value indicating whether the operation is being called as part of the binding process.</param>
        public void IssueInvoice(
            string invoiceNumber,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            Guid quoteId,
            bool fromBind = false)
        {
            var quote = this.GetQuoteOrThrow(quoteId);
            InvoiceIssuedEvent @event = quote.IssueInvoice(
                invoiceNumber, performingUserId, timestamp, quoteWorkflow, fromBind);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Issue a credit note.
        /// </summary>
        /// <param name="creditNoteNumber">The credit note number to be used for the credit note.</param>
        /// <param name="fromBind">A value indicating whether the operation is being called as part of the binding process.</param>
        /// <param name="performingUserId">The userId who issued the credit note.</param>
        /// <param name="timestamp">The time and date of issuing the credit note.</param>
        /// <param name="quoteWorkflow">The quote workflow.</param>
        /// <param name="quoteId">The quote Id.</param>
        public void IssueCreditNote(
            string creditNoteNumber,
            bool fromBind,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteWorkflow quoteWorkflow,
            Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);
            CreditNoteIssuedEvent @event = quote.IssueCreditNote(
                creditNoteNumber, fromBind, performingUserId, timestamp, quoteWorkflow);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Assign ownership of the quote to a given user.
        /// </summary>
        /// <param name="person">The person who is the new owner.</param>
        /// <param name="performingUserId">The userId who assign to new owner.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignToOwner(PersonAggregate person, Guid? performingUserId, Instant timestamp)
        {
            this.AssignToOwner(person.UserId.Value, person.Id, person.FullName, performingUserId, timestamp);
        }

        /// <summary>
        /// Assign ownership of the quote to a given user.
        /// </summary>
        /// <param name="personUserId">The ID of the persons user who is the new owner.</param>
        /// <param name="personId">The person Id who is the new owner.</param>
        /// <param name="personFullName">The persons full name who is the new owner.</param>
        /// <param name="performingUserId">The userId who assign to new owner.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignToOwner(Guid personUserId, Guid personId, string personFullName, Guid? performingUserId, Instant timestamp)
        {
            var @event = new OwnershipAssignedEvent(
                this.TenantId, this.Id, personUserId, personId, personFullName, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void UnassignOwnership(Guid? performingUserId, Instant timestamp)
        {
            var @event = new OwnershipUnassignedEvent(
                this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Assign the quote to a given customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="customerDetails">The customer details.</param>
        /// <param name="performingUserId">The userId who assigned quote to customer.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void RecordAssociationWithCustomer(
            CustomerAggregate customer, IPersonalDetails customerDetails, Guid? performingUserId, Instant timestamp)
        {
            this.RecordAssociationWithCustomer(customer.Id, customer.PrimaryPersonId, customerDetails, performingUserId, timestamp);
        }

        /// <summary>
        /// Assign the quote to a given customer.
        /// </summary>
        /// <param name="customerId">The customer Id.</param>
        /// <param name="customerPrimaryPersonId">The customers primary person Id.</param>
        /// <param name="customerDetails">The customer details.</param>
        /// <param name="performingUserId">The userId who assigned quote to customer.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void RecordAssociationWithCustomer(
            Guid customerId, Guid customerPrimaryPersonId, IPersonalDetails customerDetails, Guid? performingUserId, Instant timestamp)
        {
            var @event = new CustomerAssignedEvent(
                this.TenantId, this.Id, customerId, customerPrimaryPersonId, customerDetails, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Set the expiry time of a quote, based from the days to append from the created date.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="performingUserId">The userId who sets the quote expiry time.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteExpirySettings">The quote expiry settings.</param>
        public void SetQuoteExpiryFromSettings(
            Guid quoteId,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteExpirySettings quoteExpirySettings)
        {
            var quote = this.quotes.FirstOrDefault(x => x.Id == quoteId);

            if (quote == null)
            {
                throw new NotFoundException(Errors.General.NotFound("quote", quoteId));
            }

            if (quoteExpirySettings == null)
            {
                throw new DomainRuleViolationException("All quotes must have expiry settings.");
            }

            if (quoteExpirySettings.Enabled)
            {
                // add settings to created date to get new expiry date
                var expireDate = quote.CreatedTimestamp.Plus(Duration.FromDays(quoteExpirySettings.ExpiryDays));
                expireDate = expireDate
                    .ToLocalDateInAet()
                    .AtStartOfDayInZone(Timezones.AET)
                    .PlusHours(QuoteExpirySettings.DefaultExpiryHoursFromStartOfDay)
                    .ToInstant();
                var @event = new QuoteExpiryTimestampSetEvent(
                    this.TenantId, this.Id, quote.Id, expireDate, QuoteExpiryReason.Automatic, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Set the expiry time of a quote to a given time.
        /// </summary>
        /// <param name="quoteId">The ID of the quote to set the expiry time for.</param>
        /// <param name="newExpiryTime">The expiry time that will be set for the quote.</param>
        /// <param name="performingUserId">The userId who sets the expiry time.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void SetQuoteExpiryTime(
            Guid quoteId,
            Instant newExpiryTime,
            Guid? performingUserId,
            Instant timestamp)
        {
            var quote = this.quotes.FirstOrDefault(x => x.Id == quoteId);
            if (quote == null)
            {
                throw new NotFoundException(Errors.General.NotFound("quote", quoteId));
            }

            var @event = new QuoteExpiryTimestampSetEvent(
                this.TenantId, this.Id, quoteId, newExpiryTime, QuoteExpiryReason.Manual, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void RemoveQuoteExpiryTime(Guid quoteId, Guid? performingUserId, Instant timestamp)
        {
            var quote = this.quotes.FirstOrDefault(x => x.Id == quoteId);
            if (quote == null)
            {
                throw new NotFoundException(Errors.General.NotFound("quote", quoteId));
            }

            var @event = new QuoteExpiryTimestampSetEvent(
                this.TenantId, this.Id, quoteId, null, QuoteExpiryReason.Automatic, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Set a quote's expiry time based on a specific date.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="newExpiryDateTime">The expiry date that will be used.</param>
        /// <param name="performingUserId">The userId who sets the expiry date.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteExpirySettings">The quote expiry settings.</param>
        public void SetExpiryDate(
            Guid quoteId,
            Instant newExpiryDateTime,
            Guid? performingUserId,
            Instant timestamp,
            IQuoteExpirySettings quoteExpirySettings)
        {
            var quote = this.quotes.FirstOrDefault(x => x.Id == quoteId);
            if (quote == null)
            {
                throw new NotFoundException(Errors.General.NotFound("quote", quoteId));
            }

            if (quoteExpirySettings.Enabled)
            {
                var @event = new QuoteExpiryTimestampSetEvent(
                    this.TenantId, this.Id, quoteId, newExpiryDateTime, QuoteExpiryReason.Manual, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Record a payment for this quote.
        /// </summary>
        /// <param name="reference">A reference for the payment.</param>
        /// <param name="details">Details of the payment request and response.</param>
        /// <param name="performingUserId">The userId who records the payment.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteId">The Id of the quote.</param>
        public void RecordPaymentMade(
            string? reference, PaymentDetails details, Guid? performingUserId, Instant timestamp, Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);
            var @event = new PaymentMadeEvent(this.TenantId, this.Id, quote, details, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record a failed attempt to pay for this quote.
        /// </summary>
        /// <param name="errors">The errors that occurred.</param>
        /// <param name="performingUserId">The userId who records the payment.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteId">The quote Id.</param>
        public void RecordPaymentFailed(IEnumerable<string> errors, Guid? performingUserId, Instant timestamp, Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);
            var @event = new PaymentFailedEvent(this.TenantId, this.Id, quote, errors, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record a failure to create a funding proposal.
        /// </summary>
        /// <param name="errors">The errors encountered.</param>
        /// <param name="performingUserId">The userId who record funding proposal.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteId">The quote Id.</param>
        public void RecordFundingProposalCreationFailure(
            IEnumerable<string> errors, Guid? performingUserId, Instant timestamp, Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);
            var @event = new FundingProposalCreationFailedEvent(
                this.TenantId, this.Id, quote, errors, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record the successful acceptance of a funding proposal.
        /// </summary>
        /// <param name="internalFundingProposalId">The ID of the proposal being accepted.</param>
        /// <param name="performingUserId">The userId who record funding proposal.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteId">The Id of the quote.</param>
        public void RecordFundingProposalAccepted(
            Guid internalFundingProposalId, Guid? performingUserId, Instant timestamp, Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);
            FundingProposalAcceptedEvent @event = quote.RecordFundingProposalAccepted(
                internalFundingProposalId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record the failure to create a funding proposal.
        /// </summary>
        /// <param name="fundingProposal">The proposal that was to be accepted.</param>
        /// <param name="errors">The errors encountered.</param>
        /// <param name="performingUserId">The userId who record funding proposal.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void RecordFundingProposalAcceptanceFailed(
            FundingProposal fundingProposal, IEnumerable<string> errors, Guid? performingUserId, Instant timestamp)
        {
            var @event = new FundingProposalAcceptanceFailedEvent(
                this.TenantId, this.Id, fundingProposal, errors, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Attach a file.
        /// </summary>
        /// <param name="attachmentId">The quote file attachment ID.</param>
        /// <param name="fileContentId">The file content ID.</param>
        /// <param name="name">The file name.</param>
        /// <param name="type">The file type.</param>
        /// <param name="fileSize">The file size.</param>
        /// <param name="performingUserId">The userId who attached the file.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <param name="quoteId">The quote Id.</param>
        public void AttachFile(
            Guid attachmentId,
            Guid fileContentId,
            string name,
            string type,
            long fileSize,
            Guid? performingUserId,
            Instant timestamp,
            Guid quoteId)
        {
            var @event = new FileAttachedEvent(
                this.TenantId,
                this.Id,
                quoteId,
                attachmentId,
                name,
                type,
                fileContentId,
                fileSize,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Trigger the ApplyNewIdEvent that applies new id to this aggregate.
        /// </summary>
        /// <param name="tenantId">The tenant new id.</param>
        /// <param name="productId">The product new id.</param>
        /// <param name="quoteId">The quote id.</param>
        /// <param name="performingUserId">The ID of the user who performed the action.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void TriggerApplyNewIdEvent(Guid tenantId, Guid productId, Guid quoteId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ApplyNewIdEvent(tenantId, this.Id, quoteId, productId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Attach a generated quote document.
        /// </summary>
        /// <param name="document">The content of the document.</param>
        /// <param name="sourceEventSequenceNumber">The sequence number of the event in response to which the document was generated.</param>
        /// <param name="performingUserId">The userId who attached the quote document.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AttachQuoteDocument(
            QuoteDocument document, int sourceEventSequenceNumber, Guid? performingUserId, Instant timestamp)
        {
            // We need to attach the document to the quote that was current at the time the event was fired.
            // TODO: We need a timestamp for the event too, in case of discarded quotes (maybe).
            var quoteId = this.quotes.Where(q => q.EventSequenceNumber <= sourceEventSequenceNumber).Last().Id;
            var @event = new QuoteDocumentGeneratedEvent(this.TenantId, this.Id, quoteId, document, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Attach a generated quote document.
        /// </summary>
        /// <param name="document">The content of the document.</param>
        /// <param name="sourceEventSequenceNumber">The sequence number of the event in response to which the document was generated.</param>
        /// <param name="performingUserId">The UserId who attached generated quote document.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AttachQuoteVersionDocument(
            QuoteDocument document, int sourceEventSequenceNumber, Guid? performingUserId, Instant timestamp)
        {
            var quoteVersion = this.quotes
                .SelectMany(q => q.Versions)
                .SingleOrDefault(v => v.EventSequenceNumber == sourceEventSequenceNumber);
            var quoteOfQuoteVersion = this.quotes
                .Where(quote =>
                    quote.Versions.FirstOrDefault(version => version.EventSequenceNumber == sourceEventSequenceNumber) != null)
                .FirstOrDefault();

            if (quoteVersion == null)
            {
                throw new InvalidOperationException(
                    $"Attempt to associate document with quote version generated by event {sourceEventSequenceNumber}, but no such quote version could be found.");
            }

            var @event = new QuoteVersionDocumentGeneratedEvent(this.TenantId, this.Id, quoteOfQuoteVersion.Id, quoteVersion.VersionId, document, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Attach a generated policy document.
        /// </summary>
        /// <param name="document">The content of the document.</param>
        /// <param name="policyTransactionId">Policy Transaction Id.</param>
        /// <param name="performingUserId">The userId who attach policy document.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AttachPolicyDocument(
            QuoteDocument document, Guid policyTransactionId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new PolicyDocumentGeneratedEvent(
                this.TenantId, this.Id, policyTransactionId, document, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Gets an attached policy document by name.
        /// </summary>
        /// <param name="name">The name of the document.</param>
        /// <returns>The document, if one with the given name exists, otherwise null.</returns>
        public QuoteDocument GetPolicyDocument(string name)
        {
            if (this.policyDocumentsByName.TryGetValue(name, out QuoteDocument document))
            {
                return document;
            }

            return null;
        }

        /// <summary>
        /// Gets an attached quote document by name.
        /// </summary>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="name">The name of the document.</param>
        /// <returns>The document, if one with the given name exists, otherwise null.</returns>
        public QuoteDocument GetQuoteDocument(Guid quoteId, string name)
        {
            return this.quotes.Single(q => q.Id == quoteId).GetQuoteDocument(name);
        }

        /// <summary>
        /// Record workflow status.
        /// </summary>
        /// <param name="workflowStep">The workflow step.</param>
        /// <param name="performingUserId">The userId who record workflow step.</param>
        /// <param name="timestamp">The time it was created.</param>
        /// <param name="quoteId">The quote Id.</param>
        public void RecordWorkflowStep(string workflowStep, Guid? performingUserId, Instant timestamp, Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);

            // TODO: Checks on whether state change is permitted for user and current state.
            // TODO: Rename event?
            var @event = new WorkflowStepAssignedEvent(
                this.TenantId, this.Id, quote.Id, workflowStep, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Retrieves quote by sequence number.
        /// </summary>
        /// <param name="sourceEventSequenceNumber">The sequence number of the event in response to which the document was generated.</param>
        /// <returns>The quote.</returns>
        public Quote GetQuoteBySequenceNumber(int sourceEventSequenceNumber)
        {
            var quote = this.quotes.Where(q => q.EventSequenceNumber <= sourceEventSequenceNumber).LastOrDefault();
            return quote;
        }

        /// <summary>
        /// Gets the quote email generated by Id.
        /// </summary>
        /// <param name="id">The ID of the quote email generated.</param>
        /// <returns>The quote email read model.</returns>
        public QuoteEmailReadModel GetQuoteEmailGenerated(string id)
        {
            throw new NotImplementedException("TODO: Fix.");
        }

        /// <summary>
        /// Attempt to patch form data.
        /// </summary>
        /// <param name="command">The patch command specifying what to patch.</param>
        /// <param name="performingUserId">The userId who patched form data.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <returns>A result indicating success or any error.</returns>
        public Result PatchFormData(PolicyDataPatchCommand command, Guid? performingUserId, Instant timestamp)
        {
            // Obtain source
            var newData = command.GetNewValue(this);

            var dataHolders = new List<Entities.IPatchableDataHolder>();
            dataHolders.AddRange(this.quotes);
            dataHolders.AddRange(this.Policy?.Transactions ?? Enumerable.Empty<Entities.PolicyTransaction>());
            if (command.TargetFormDataPath != null)
            {
                var results = dataHolders.Select(dh => dh.SelectAndValidateFormDataPatchTargets(command));
                if (results.Any(r => r.IsFailure))
                {
                    return Result.Failure(results.First(r => r.IsFailure).Error);
                }

                var patchTargets = results.SelectMany(r => r.Value);
                if (!patchTargets.Any())
                {
                    return Result.Failure("Could not find any matching target to patch.");
                }

                var patch = new PolicyDataPatch(
                    DataPatchType.FormData, command.TargetFormDataPath, newData, patchTargets);
                var @event = new PolicyDataPatchedEvent(this.TenantId, this.Id, patch, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }

            if (command.TargetCalculationResultPath != null)
            {
                var results = dataHolders.Select(dh => dh.SelectAndValidateCalculationResultPatchTargets(command));
                if (results.Any(r => r.IsFailure))
                {
                    return Result.Failure(results.First(r => r.IsFailure).Error);
                }

                var patchTargets = results.SelectMany(r => r.Value);
                if (!patchTargets.Any())
                {
                    return Result.Failure("Could not find any matching target to patch.");
                }

                var patch = new PolicyDataPatch(
                    DataPatchType.CalculationResult, command.TargetCalculationResultPath, newData, patchTargets);
                var @event = new PolicyDataPatchedEvent(this.TenantId, this.Id, patch, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }

            return Result.Success();
        }

        /// <summary>
        /// Record organisation migration and only applicable for an aggregate with an empty organisation Id.
        /// </summary>
        /// <param name="organisationId">The Id of the organisation to persist in this aggregate.</param>
        /// <param name="performingUserId">The user Id who updates the aggregate.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        /// <returns>A result indicating success or any error.</returns>
        public Result<Guid, Error> RecordOrganisationMigration(
            Guid organisationId, Guid? quoteId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new QuoteTransferredToAnotherOrganisationEvent(
                this.TenantId, this.Id, organisationId, quoteId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);

            return Result.Success<Guid, Error>(@event.AggregateId);
        }

        /// <summary>
        /// Patch the form data for the quote using json patch document.
        /// </summary>
        /// <param name="personalDetails">The aggregate for people.</param>
        /// <param name="questionMetaData"> Represents the Question Metadata.</param>
        /// <param name="productConfiguration">Configuration for a product.</param>
        /// <param name="performingUserId">The userId who updates formdata.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        /// <param name="quoteId">The Id of the quote.</param>
        public void UpdateFormDataWithCustomerDetails(
            IPersonalDetails personalDetails,
            IEnumerable<IQuestionMetaData> questionMetaData,
            IProductConfiguration productConfiguration,
            Guid? performingUserId,
            Instant timestamp,
            Guid quoteId)
        {
            var quote = this.GetQuoteOrThrow(quoteId);

            if (quote.IsDiscarded)
            {
                throw new ErrorException(Errors.Quote.CannotUpdateWhenDiscarded());
            }

            var defaultQuoteDatumLocations = (DefaultQuoteDatumLocations)DefaultQuoteDatumLocations.Instance;
            var customerDataPatchGenerator = new CustomerFormModelDataPatchGenerator(
                productConfiguration.QuoteDataLocations, defaultQuoteDatumLocations, personalDetails);
            JsonPatchDocument customerFormDataPatch = customerDataPatchGenerator.Invoke(questionMetaData);

            if (quote.LatestFormData == null)
            {
                var formData = FormData.CreateEmpty();
                customerFormDataPatch.ApplyTo(formData.JObject);
                quote.UpdateFormData(formData, performingUserId, timestamp);
            }
            else
            {
                var @event = new FormDataPatchedEvent(
                    this.TenantId, this.Id, quote.Id, customerFormDataPatch, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Create a new customer association invitation.
        /// </summary>
        /// <param name="quoteId">The quote ID.</param>
        /// <param name="customerUserId">The customer ID as a user.</param>
        /// <param name="performingUserId">The userId who associates customer inivitation.</param>
        /// <param name="createdTimestamp">The quote and customer association created time.</param>
        /// <returns>The ID of the created invitation.</returns>
        public Guid RecordCustomerAssociationInvitationCreation(
            Guid quoteId, Guid customerUserId, Guid? performingUserId, Instant createdTimestamp)
        {
            var @event = new QuoteCustomerAssociationInvitationCreatedEvent(
                this.TenantId, this.Id, quoteId, customerUserId, performingUserId, createdTimestamp);
            this.ApplyNewEvent(@event);
            return @event.AggregateId;
        }

        /// <summary>
        /// Verifies the availability of the customer association invitation.
        /// </summary>
        /// <param name="associationInvitationId">The ID of the invitation to verify.</param>
        /// <param name="ownerId">The owner ID of the invitation to verify.</param>
        /// <param name="verificationTime">THe time the verification is being performed.</param>
        public void VerifyCustomerAssociationInvitation(
            Guid associationInvitationId, Guid ownerId, Instant verificationTime)
        {
            if (this.quoteAssociationInvitation == null || this.quoteAssociationInvitation.Id != associationInvitationId)
            {
                throw new ErrorException(Errors.Quote.AssociationInvitation.NotFound(associationInvitationId));
            }
            else if (this.quoteAssociationInvitation.OwnerId != ownerId)
            {
                throw new ErrorException(Errors.Quote.AssociationInvitation.CustomerUserNotFound(associationInvitationId));
            }
            else if (this.quoteAssociationInvitation.IsExpired(verificationTime))
            {
                throw new ErrorException(Errors.Quote.AssociationInvitation.Expired(associationInvitationId));
            }
        }

        public void TransferToAnotherOrganisation(
            Guid organisationId, Guid quoteId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new QuoteTransferredToAnotherOrganisationEvent(
               this.TenantId, this.Id, organisationId, quoteId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void MigrateQuotesAndPolicyTransactionsToNewProductRelease(
            Guid orignalProductReleaseId, Guid newProductReleaseId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent(
               this.TenantId, this.Id, orignalProductReleaseId, newProductReleaseId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void MigrateUnassociatedEntitiesToProductRelease(
            Guid productId, Guid newProductReleaseId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new MigrateUnassociatedEntitiesToProductReleaseEvent(
               this.TenantId, productId, this.Id, newProductReleaseId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueInitializedEvent<QuoteAggregate, IQuoteEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.Add(this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <inheritdoc/>
        public void AddAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefinitionId,
            string value,
            AdditionalPropertyDefinitionType propertyType,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            var initializedEvent = new AdditionalPropertyValueInitializedEvent<QuoteAggregate, IQuoteEventObserver>(
                tenantId,
                this.Id,
                Guid.NewGuid(), // the additional property value id is generated here
                additionalPropertyDefinitionId,
                entityId,
                value,
                propertyType,
                performingUserId,
                createdTimestamp);
            this.ApplyNewEvent(initializedEvent);
        }

        /// <inheritdoc/>
        public void UpdateAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            AdditionalPropertyDefinitionType type,
            Guid additionalPropertyDefinitionId,
            Guid additionalPropertyValueId,
            string value,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            var updateEvent = new AdditionalPropertyValueUpdatedEvent<QuoteAggregate, IQuoteEventObserver>(
                tenantId,
                this.Id,
                value,
                performingUserId,
                createdTimestamp,
                type,
                additionalPropertyDefinitionId,
                additionalPropertyValueId,
                entityId);
            this.ApplyNewEvent(updateEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueUpdatedEvent<QuoteAggregate, IQuoteEventObserver> aggregateEvent, int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.AddOrUpdate(this.AdditionalPropertyValues, aggregateEvent);
        }

        public override QuoteAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<QuoteAggregate, Guid>> events, int finalVersion)
        {
            this.ApplyEvents(events, finalVersion);
            foreach (var quote in this.quotes)
            {
                quote.SetMissingPropertyDuringSnapshotCreation(this);
            }

            this.Policy?.SetMissingPropertyDuringSnapshotCreation(this);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(QuoteInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.OrganisationId = @event.OrganisationId;
            this.TenantId = @event.TenantId;
            this.ProductId = @event.ProductId;
            this.IsTestData = @event.IsTestData;
            this.Environment = @event.Environment;
            this.CreatedTimestamp = @event.Timestamp;
            this.CustomerId = @event.CustomerId;
            DateTimeZone timeZone = Timezones.GetTimeZoneByIdOrDefault(@event.TimeZoneId);
            var quote = new NewBusinessQuote(
                @event.QuoteId,
                this,
                sequenceNumber,
                @event.Timestamp,
                timeZone,
                @event.AreTimestampsAuthoritative,
                @event.CustomerId,
                @event.ProductReleaseId,
                @event.QuoteNumber,
                @event.InitialQuoteState);
            this.quotes.Add(quote);

            if (@event.FormDataJson != null)
            {
                quote.SeedWithFormData(@event.FormDataJson);
            }
        }

        private void Apply(QuoteMigratedEvent @event, int sequenceNumber)
        {
            throw new NotImplementedException("TODO: Re-implement quote import!");
        }

        private void Apply(QuoteImportedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.TenantId = @event.TenantId;
            this.ProductId = @event.ProductId;
            this.OrganisationId = @event.OrganisationId;
            this.Environment = @event.Environment;
            this.CreatedTimestamp = @event.Timestamp;
            this.CustomerId = @event.CustomerId;
            DateTimeZone timeZone = Timezones.GetTimeZoneByIdOrDefault(@event.TimeZoneId);
            var quote = new NewBusinessQuote(
                @event.QuoteId,
                this,
                sequenceNumber,
                @event.Timestamp,
                timeZone,
                @event.AreTimestampsAuthoritative,
                @event.CustomerId,
                @event.ProductReleaseId,
                @event.QuoteNumber,
                @event.QuoteState);

            quote.SeedWithFormData(@event.FormDataJson);
            quote.SeedWithCalculationResult(@event.CalculationResultId, @event.CalculationResult, @event.Timestamp);
            quote.SetToActualised();
            this.quotes.Add(quote);
        }

        private void Apply(FormDataUpdatedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
        }

        private void Apply(FormDataPatchedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
        }

        private void Apply(CalculationResultCreatedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
        }

        private void Apply(CustomerDetailsUpdatedEvent @event, int sequenceNumber)
        {
            var customerDetailsUpdate = new QuoteDataUpdate<IPersonalDetails>(
                @event.CustomerDetailsUpdateId, @event.CustomerDetails, @event.Timestamp);
            this.GetQuoteBySequenceNumber(sequenceNumber).RecordCustomerDetailsUpdate(customerDetailsUpdate);
        }

        private void Apply(QuoteNumberAssignedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).SetQuoteNumber(@event.QuoteNumber);
        }

        private void Apply(QuoteTitleAssignedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
        }

        private void Apply(QuoteVersionCreatedEvent @event, int sequenceNumber)
        {
            // TODO: Work out which data we actually need to store in quote.
            this.quotes.Single(q => q.Id == @event.QuoteId).RecordVersioning(@event, sequenceNumber);
        }

        private void Apply(QuoteBoundEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        private void Apply(QuoteDiscardEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).RecordDiscarding();
        }

        private void Apply(PolicyDeletedEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        private void Apply(QuoteSubmittedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).RecordSubmission(@event);
        }

        private void Apply(QuoteRollbackEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        private void Apply(QuoteSavedEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        private void Apply(EnquiryMadeEvent @event, int sequenceNumber)
        {
            // Nop.
        }

        private void Apply(PolicyIssuedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
            this.Policy = new Policy(@event, sequenceNumber, this);
        }

        private void Apply(PolicyImportedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.OrganisationId = @event.OrganisationId;
            this.TenantId = @event.TenantId;
            this.ProductId = @event.ProductId;
            this.IsTestData = @event.IsTestData;
            this.Environment = @event.Environment;
            this.CreatedTimestamp = @event.Timestamp;
            this.CustomerId = @event.CustomerId;
            this.Policy = new Policy(@event, sequenceNumber, this);
        }

        private void Apply(PolicyIssuedWithoutQuoteEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.OrganisationId = @event.OrganisationId;
            this.TenantId = @event.TenantId;
            this.ProductId = @event.ProductId;
            this.IsTestData = @event.IsTestData;
            this.Environment = @event.Environment;
            this.CreatedTimestamp = @event.Timestamp;
            this.CustomerId = @event.CustomerId;
            this.Policy = new Policy(@event, sequenceNumber, this);
        }

        private void Apply(AggregateCreationFromPolicyEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.OrganisationId = @event.OrganisationId;
            this.TenantId = @event.TenantId;
            this.ProductId = @event.ProductId;
            this.IsTestData = @event.IsTestData;
            this.Environment = @event.Environment;
            this.CreatedTimestamp = @event.Timestamp;
            this.CustomerId = @event.CustomerId;
            this.Policy = new Policy(@event, sequenceNumber, this);
        }

        [Obsolete("Policy state is calculated dynamically by aggregates, and should not be set.")]
        private void Apply(PolicyStateChangedEvent @event, int sequenceNumber)
        {
            // no-op. This must stay here so that runtime exceptions do not occur. Do not remove it.
        }

        private void Apply(SetPolicyTransactionsEvent @event, int sequenceNumber)
        {
            if (this.Policy != null)
            {
                this.Policy.SetTransactions(@event);
            }
        }

        private void Apply(PolicyAdjustedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
            this.Policy.Apply(@event, sequenceNumber);
        }

        private void Apply(PolicyRenewedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber)?.Apply(@event, sequenceNumber);
            this.Policy.Apply(@event, sequenceNumber);
        }

        private void Apply(PolicyCancelledEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
            this.Policy.Apply(@event, sequenceNumber);
        }

        private void Apply(PolicyNumberUpdatedEvent @event, int sequenceNumber)
        {
            this.Policy.Apply(@event, sequenceNumber);
        }

        private void Apply(PolicyDataPatchedEvent @event, int sequenceNumber)
        {
            foreach (var quote in this.quotes)
            {
                quote.Apply(@event, sequenceNumber);
            }

            this.Policy.Apply(@event, sequenceNumber);
        }

        private void Apply(InvoiceIssuedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
        }

        private void Apply(CreditNoteIssuedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
        }

        private void Apply(OwnershipAssignedEvent @event, int sequenceNumber)
        {
            this.OwnerUserId = @event.UserId;
        }

        private void Apply(OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            this.OwnerUserId = null;
        }

        private void Apply(CustomerAssignedEvent @event, int sequenceNumber)
        {
            this.CustomerId = @event.CustomerId;
            this.quoteAssociationInvitation = null;
        }

        private void Apply(QuoteExpiryTimestampSetEvent @event, int sequenceNumber)
        {
            this.quotes.FirstOrDefault(x => x.Id == @event.QuoteId)?.Apply(@event, sequenceNumber);
        }

        private void Apply(PaymentMadeEvent @event, int sequenceNumber)
        {
            var result = PaymentAttemptResult.CreateSuccessResponse(
                @event.PaymentDetails, @event.DataSnapshotIds, @event.Timestamp);
            this.GetQuoteBySequenceNumber(sequenceNumber).RecordPaymentAtttemptResult(result);
        }

        private void Apply(PaymentFailedEvent @event, int sequenceNumber)
        {
            var result = PaymentAttemptResult.CreateFailureResponse(
                @event.Errors, @event.DataSnapshotIds, @event.Timestamp);
            this.GetQuoteBySequenceNumber(sequenceNumber).RecordPaymentAtttemptResult(result);
        }

        private void Apply(FundingProposalCreatedEvent @event, int sequenceNumber)
        {
            var result = FundingProposalCreationResult.CreateSuccessResponse(
                @event.FundingProposal, @event.DataSnapshotIds, @event.Timestamp);
            this.GetQuoteBySequenceNumber(sequenceNumber).RecordFundingProposalCreationResult(result);
        }

        private void Apply(FundingProposalCreationFailedEvent @event, int sequenceNumber)
        {
            var result = FundingProposalCreationResult.CreateFailureResponse(
                @event.Errors, @event.DataSnapshotIds, @event.Timestamp);
            this.GetQuoteBySequenceNumber(sequenceNumber).RecordFundingProposalCreationResult(result);
        }

        private void Apply(FundingProposalAcceptedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
        }

        private void Apply(FundingProposalAcceptanceFailedEvent @event, int sequenceNumber)
        {
            var result = FundingProposalAcceptanceResult.CreateFailureResponse(
                @event.FundingProposal, @event.Errors, @event.Timestamp);
            this.GetQuoteBySequenceNumber(sequenceNumber).RecordFundingProposalAcceptanceResult(result);
        }

        private void Apply(FileAttachedEvent @event, int sequenceNumber)
        {
            var quoteFileAttachment = new QuoteFileAttachment(
                @event.TenantId,
                @event.AttachmentId,
                @event.QuoteId,
                @event.FileContentId,
                @event.Name,
                @event.Type,
                @event.FileSize,
                @event.Timestamp);

            var quote = this.quotes.Single(q => q.Id == @event.QuoteId);
            quote.AttachFile(quoteFileAttachment);
        }

        private void Apply(QuoteDocumentGeneratedEvent @event, int sequenceNumber)
        {
            var quote = this.quotes.FirstOrDefault(q => q.Id == @event.QuoteId);

            if (quote != null)
            {
                quote.UpsertDocument(@event.Document);
            }
        }

        private void Apply(QuoteVersionDocumentGeneratedEvent @event, int sequenceNumber)
        {
            // Nop.

            // Quote documents are not accessed on the command side any more.
            // Previously they were used by the SavedFileProvider for integrations,
            // but that is no longer used.
            // If it required in the future or resurrected for use in the service bus, then
            // quote version documents will need to be associated with the correct quote version here,
            // and possibly also the quote.
        }

        private void Apply(PolicyDocumentGeneratedEvent @event, int sequenceNumber)
        {
            this.policyDocumentsByName[@event.Document.Name] = @event.Document;
        }

        private void Apply(QuoteEmailGeneratedEvent @event, int sequenceNumber)
        {
            // TODO: Check to see if you can update the quote email read model based on the given event
        }

        private void Apply(PolicyEmailGeneratedEvent @event, int sequenceNumber)
        {
            // TODO: Check to see if you can update the policy email read model based on the given event
        }

        private void Apply(QuoteEmailSentEvent @event, int sequenceNumber)
        {
            // TODO: Check to see if you can update teh quote email sent read models based on the given event.
        }

        private void Apply(WorkflowStepAssignedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).UpdateWorkflowStep(@event.WorkflowStep);
        }

        private void Apply(QuoteActualisedEvent @event, int sequenceNumber)
        {
            this.GetQuoteBySequenceNumber(sequenceNumber).Apply(@event, sequenceNumber);
        }

        private void Apply(QuoteCustomerAssociationInvitationCreatedEvent @event, int sequenceNumber)
        {
            // Replace any existing quote association invitation
            this.quoteAssociationInvitation = new QuoteCustomerAssociationInvitation(
                @event.AggregateId, @event.CustomerUserId, @event.Timestamp);
        }

        private void Apply(ApplyNewIdEvent applyNewIdEvent, int sequenceNumber)
        {
            this.TenantId = applyNewIdEvent.TenantId;
            this.ProductId = applyNewIdEvent.ProductId;
        }

        private void Apply(QuoteTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(MigrateQuotesAndPolicyTransactionsToNewProductReleaseEvent @event, int sequenceNumber)
        {
            var quotes = this.quotes.Where(q => q.ProductReleaseId == @event.OrginalProductReleaseId);

            foreach (var quote in quotes)
            {
                quote.Apply(@event, sequenceNumber);
            }

            var policyTransactions = this.Policy?.Transactions
                .Where(t => t.ProductReleaseId == @event.OrginalProductReleaseId) ?? Enumerable.Empty<Entities.PolicyTransaction>();

            foreach (var transaction in policyTransactions)
            {
                transaction.Apply(@event);
            }
        }

        private void Apply(MigrateUnassociatedEntitiesToProductReleaseEvent @event, int sequenceNumber)
        {
            var quotes = this.quotes.Where(q => q.ProductReleaseId == null);

            foreach (var quote in quotes)
            {
                quote.Apply(@event, sequenceNumber);
            }

            var policyTransactions = this.Policy?.Transactions
                .Where(t => t.ProductReleaseId == null) ?? Enumerable.Empty<Entities.PolicyTransaction>();

            foreach (var transaction in policyTransactions)
            {
                transaction.Apply(@event);
            }
        }
    }
}
