// <copyright file="QuoteVersionReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Json;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// For updating the quote version read model in response to events from the write model.
    /// </summary>
    public class QuoteVersionReadModelWriter : IQuoteVersionReadModelWriter
    {
        private readonly IWritableReadModelRepository<QuoteVersionReadModel> quoteVersionReadModelUpdateRepository;
        private readonly IQuoteVersionReadModelRepository quoteVersionReadModelRepository;
        private readonly IQuoteReadModelRepository quoteRepository;
        private readonly IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionReadModelWriter"/> class.
        /// </summary>
        /// <param name="quoteVersionWritableReadModelRepository">The quote version writable read model repository.</param>
        /// <param name="quoteVersionReadModelRepository">The quote version read model repository.</param>
        /// <param name="quoteReadModelRepository">The quote read model repository.</param>
        public QuoteVersionReadModelWriter(
            IWritableReadModelRepository<QuoteVersionReadModel> quoteVersionWritableReadModelRepository,
            IQuoteVersionReadModelRepository quoteVersionReadModelRepository,
            IQuoteReadModelRepository quoteRepository,
            IWritableReadModelRepository<NewQuoteReadModel> quoteReadModelRepository)
        {
            Debug.Assert(quoteVersionReadModelRepository != null, "quoteVersionReadModelRepository should not be null");
            this.quoteVersionReadModelUpdateRepository = quoteVersionWritableReadModelRepository
                ?? throw new ArgumentNullException(nameof(quoteVersionWritableReadModelRepository));
            this.quoteVersionReadModelRepository = quoteVersionReadModelRepository;
            this.quoteRepository = quoteRepository;
            this.quoteReadModelRepository = quoteReadModelRepository
                ?? throw new ArgumentNullException(nameof(quoteReadModelRepository));
        }

        public void Dispatch(
            QuoteAggregate aggregate,
            IEvent<QuoteAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteRollbackEvent @event, int sequenceNumber)
        {
            this.DeleteQuoteVersionsRolledBack(@event);
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyDataPatchedEvent @event, int sequenceNumber)
        {
            var versions = this.quoteVersionReadModelUpdateRepository
                .Where(@event.TenantId, v => v.AggregateId == @event.AggregateId);

            foreach (var version in versions)
            {
                version.ApplyPatch(@event.PolicyDatPatch);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyDeletedEvent @event, int sequenceNumber)
        {
            if (aggregate.Policy != null)
            {
                var policyId = aggregate.Policy.PolicyId;
                var quoteIds = this.quoteRepository.ListQuoteIdsFromPolicy(@event.TenantId, policyId, aggregate.Environment);
                quoteIds.ToList().ForEach(q => this.quoteVersionReadModelUpdateRepository.Delete(@event.TenantId, v => v.QuoteId == q));
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteVersionCreatedEvent @event, int sequenceNumber)
        {
            var quote = this.GetQuoteReadModel(@event.TenantId, @event.QuoteId);

            // set version id if has no value.
            var versionId = @event.VersionId == Guid.Empty ? Guid.NewGuid() : @event.VersionId;

            // dont create duplicates for replays.
            var quoteVersion = this.quoteVersionReadModelUpdateRepository.Where(
                quote.TenantId,
                x =>
                    x.QuoteVersionNumber == @event.VersionNumber
                    && x.QuoteId == @event.QuoteId
                    && x.QuoteVersionId == @event.VersionId)
                .FirstOrDefault();

            if (quoteVersion == null)
            {
                // TODO: Store data in event?
                quoteVersion = new QuoteVersionReadModel
                {
                    WorkflowStep = quote.WorkflowStep,
                    State = quote.QuoteState,
                    QuoteVersionId = versionId,
                    Id = versionId,
                    AggregateId = @event.AggregateId,
                    QuoteId = quote.Id,
                    QuoteVersionNumber = @event.VersionNumber,
                    CreatedTimestamp = @event.Timestamp,
                    LastModifiedTimestamp = quote.LastModifiedTimestamp,
                    QuoteNumber = quote.QuoteNumber,
                    LatestFormData = quote.LatestFormData,
                    CustomerEmail = quote.CustomerEmail,
                    CustomerAlternativeEmail = quote.CustomerAlternativeEmail,
                    CustomerFullName = quote.CustomerFullName,
                    CustomerId = quote.CustomerId,
                    CustomerPersonId = quote.CustomerPersonId,
                    CustomerPreferredName = quote.CustomerPreferredName,
                    CustomerHomePhone = quote.CustomerHomePhone,
                    CustomerMobilePhone = quote.CustomerMobilePhone,
                    CustomerWorkPhone = quote.CustomerWorkPhone,
                    OwnerUserId = quote.OwnerUserId,
                    OwnerPersonId = quote.OwnerPersonId,
                    OwnerFullName = quote.OwnerFullName,
                    CalculationResult = quote.LatestCalculationResult,
                    CalculationResultJson = quote.LatestCalculationResult?.Json,
                    Environment = quote.Environment,
                    Type = quote.Type,
                    ProductId = quote.ProductId,
                    TenantId = quote.TenantId,
                    OrganisationId = quote.OrganisationId,
                    IsTestData = quote.IsTestData,
                };
                this.quoteVersionReadModelUpdateRepository.Add(quoteVersion);
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteVersionDocumentGeneratedEvent @event, int sequenceNumber)
        {
            var quoteVersion = this.GetQuoteVersionReadModel(@event.TenantId, @event.VersionId);
            quoteVersion.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            IEnumerable<QuoteVersionReadModel> versions = this.quoteVersionReadModelUpdateRepository
                .Where(
                @event.TenantId,
                x =>
                    (x.QuoteId != default && x.QuoteId == @event.QuoteId) ||
                    x.AggregateId == @event.AggregateId);

            foreach (var version in versions)
            {
                version.TenantId = @event.TenantId;
                version.ProductId = @event.ProductId;
            }
        }

        public void Handle(
            QuoteAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<QuoteAggregate, IQuoteEventObserver> @event,
            int sequenceNumber)
        {
            var quoteVersion = this.quoteVersionReadModelRepository.GetVersionDetailsById(@event.TenantId, @event.EntityId);
            if (quoteVersion != null)
            {
                var quoteVersionUpdate = this.GetQuoteVersionReadModel(@event.TenantId, @event.EntityId);
                quoteVersionUpdate.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.QuoteTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            IEnumerable<QuoteVersionReadModel> versions = this.quoteVersionReadModelUpdateRepository.Where(
                @event.TenantId,
                x =>
                    (x.QuoteId != default && x.QuoteId == @event.QuoteId) || x.AggregateId == @event.AggregateId);

            foreach (var version in versions)
            {
                version.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
        {
            var versions = this.quoteVersionReadModelUpdateRepository
                .Where(@event.TenantId, x => x.AggregateId == @event.AggregateId);

            foreach (var version in versions)
            {
                version.OwnerUserId = @event.UserId;
                version.OwnerPersonId = @event.PersonId;
                version.OwnerFullName = @event.FullName;
                version.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.CustomerAssignedEvent @event, int sequenceNumber)
        {
            var versions = this.quoteVersionReadModelUpdateRepository
                .Where(@event.TenantId, x => x.AggregateId == @event.AggregateId);

            foreach (var version in versions)
            {
                version.CustomerId = @event.CustomerId;
                version.CustomerPersonId = @event.PersonId;
                version.CustomerFullName = @event.CustomerDetails.FullName;
                version.CustomerPreferredName = @event.CustomerDetails.PreferredName;
                version.CustomerEmail = @event.CustomerDetails.Email;
                version.CustomerAlternativeEmail = @event.CustomerDetails.AlternativeEmail;
                version.CustomerMobilePhone = @event.CustomerDetails.MobilePhone;
                version.CustomerMobilePhone = @event.CustomerDetails.HomePhone;
                version.CustomerWorkPhone = @event.CustomerDetails.WorkPhone;

                var formData = new FormData(version.LatestFormData);
                formData.PatchFormModelProperty(new JsonPath("contactEmail"), @event.CustomerDetails.Email);
                formData.PatchFormModelProperty(new JsonPath("contactName"), @event.CustomerDetails.FullName);
                formData.PatchFormModelProperty(new JsonPath("contactMobile"), @event.CustomerDetails.MobilePhoneNumber);
                formData.PatchFormModelProperty(new JsonPath("contactPhone"), @event.CustomerDetails.HomePhoneNumber);
                version.LatestFormData = formData.Json;

                version.CalculationResult.PatchProperty(new JsonPath("contactEmail"), @event.CustomerDetails.Email);
                version.CalculationResult.PatchProperty(new JsonPath("contactName"), @event.CustomerDetails.FullName);
                version.CalculationResult.PatchProperty(new JsonPath("contactMobile"), @event.CustomerDetails.MobilePhoneNumber);
                version.CalculationResult.PatchProperty(new JsonPath("contactPhone"), @event.CustomerDetails.HomePhoneNumber);
            }
        }

        private void DeleteQuoteVersionsRolledBack(QuoteAggregate.QuoteRollbackEvent @event)
        {
            var quote = this.GetQuoteReadModel(@event.TenantId, @event.QuoteId);
            IEnumerable<IQuoteVersionReadModelSummary> currentQuoteVersions =
                this.quoteVersionReadModelRepository.GetDetailVersionsOfQuote(quote.TenantId, @event.QuoteId);
            int newNumberOfQuoteVersions
                = @event.ReplayEvents.Count(e => e is QuoteAggregate.QuoteVersionCreatedEvent);
            int currentQuoteVersionIndex = 0;
            foreach (IQuoteVersionReadModelSummary currentQuoteVersion in currentQuoteVersions)
            {
                ++currentQuoteVersionIndex;
                if (currentQuoteVersionIndex > newNumberOfQuoteVersions)
                {
                    this.quoteVersionReadModelUpdateRepository
                        .Delete(quote.TenantId, q => q.QuoteVersionId == currentQuoteVersion.QuoteVersionId);
                }
            }
        }

        private QuoteVersionReadModel GetQuoteVersionReadModel(Guid tenantId, Guid quoteVersionId)
        {
            return this.quoteVersionReadModelUpdateRepository.Single(tenantId, qv => qv.QuoteVersionId == quoteVersionId);
        }

        private NewQuoteReadModel GetQuoteReadModel(Guid tenantId, Guid quoteId)
        {
            return this.quoteReadModelRepository.GetById(tenantId, quoteId);
        }
    }
}
