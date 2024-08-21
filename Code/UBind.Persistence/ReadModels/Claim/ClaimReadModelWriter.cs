// <copyright file="ClaimReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Claim
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Reponsible for updating the user read model.
    /// </summary>
    public class ClaimReadModelWriter : IClaimReadModelWriter
    {
        private readonly IWritableReadModelRepository<ClaimReadModel> repository;
        private readonly IWritableReadModelRepository<ClaimVersionReadModel> writableClaimVersionReadModelRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimReadModelWriter"/> class.
        /// </summary>
        /// <param name="repository">The read model repository.</param>
        /// <param name="policyReadModelRepository">The repository for policy read models.</param>
        /// <param name="propertyTypeEvaluatorService">Service that selects which eav table the values will be
        /// persisted.</param>
        public ClaimReadModelWriter(
            IWritableReadModelRepository<ClaimReadModel> repository,
            IWritableReadModelRepository<ClaimVersionReadModel> writableClaimVersionReadModelRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            PropertyTypeEvaluatorService propertyTypeEvaluatorService)
        {
            this.repository = repository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.writableClaimVersionReadModelRepository = writableClaimVersionReadModelRepository;
            this.propertyTypeEvaluatorService = propertyTypeEvaluatorService;
        }

        public void Dispatch(
            ClaimAggregate aggregate,
            IEvent<ClaimAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Dispatch(
            PersonAggregate aggregate,
            IEvent<PersonAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Dispatch(
            QuoteAggregate aggregate,
            IEvent<QuoteAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimInitializedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.repository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var policy = this.policyReadModelRepository.GetPolicyByNumber(
                @event.TenantId, @event.ProductId, @event.Environment, @event.PolicyNumber);
            var isTestData = policy?.IsTestData ?? @event.IsTestData;
            var claim = new ClaimReadModel(
                @event.TenantId,
                @event.ProductId,
                @event.Environment,
                @event.OrganisationId,
                @event.AggregateId,
                @event.PolicyId,
                @event.ReferenceNumber,
                null, // Claim number is not assigned yet.
                @event.Amount,
                ClaimState.Nascent,
                @event.Description,
                @event.IncidentDate?.AtStartOfDayInZone(aggregate.Claim.TimeZone).ToInstant(),
                @event.PolicyNumber,
                @event.CustomerId,
                policy?.OwnerUserId ?? default,
                @event.PersonId,
                @event.CustomerFullName,
                @event.CustomerPreferredName,
                @event.Timestamp,
                Timezones.GetTimeZoneByIdOrThrow(@event.TimeZoneId),
                isTestData);
            this.repository.Add(claim);
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimImportedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.repository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var policy = this.policyReadModelRepository.GetPolicyByNumber(
                @event.TenantId, @event.ProductId, @event.Environment, @event.PolicyNumber);
            var claim = new ClaimReadModel(
                @event.TenantId,
                @event.ProductId,
                @event.Environment,
                @event.OrganisationId,
                @event.AggregateId,
                @event.PolicyId,
                @event.ReferenceNumber,
                @event.ClaimNumber,
                @event.Amount,
                @event.Status ?? ClaimState.Incomplete,
                @event.Description,
                @event.IncidentDate?.AtStartOfDayInZone(aggregate.Claim.TimeZone).ToInstant(),
                @event.PolicyNumber,
                @event.CustomerId,
                policy?.OwnerUserId ?? default,
                @event.PersonId,
                @event.CustomerFullName,
                @event.CustomerPreferredName,
                @event.Timestamp,
                Timezones.GetTimeZoneByIdOrThrow(@event.TimeZoneId),
                policy?.IsTestData ?? true);
            this.repository.Add(claim);
            aggregate.ReadModel = claim;
        }

        /// <summary>
        /// Handle a claim update imported event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimUpdateImportedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.Amount = @event.Amount;
            claim.Description = @event.Description;
            claim.IncidentTimestamp = @event.IncidentDate?.AtStartOfDayInZone(aggregate.Claim.TimeZone).ToInstant();
            claim.LastModifiedTimestamp = @event.Timestamp;
            if (@event.Status != null)
            {
                claim.Status = @event.Status;
            }

            if (@event.ReferenceNumber != null)
            {
                claim.ClaimReference = @event.ReferenceNumber;
            }

            aggregate.ReadModel = claim;
        }

        /// <summary>
        /// Handle a claim amount updated event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimAmountUpdatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.Amount = @event.Amount;
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        /// <summary>
        /// Handle a claim desciption updated event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimDescriptionUpdatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.Description = @event.Description;
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimIncidentDateUpdatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.IncidentTimestamp = @event.IncidentTimestamp;
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.AssociateClaimWithPolicyEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.PolicyNumber = @event.PolicyNumber;
            claim.PolicyId = @event.PolicyId;
            claim.CustomerId = @event.CustomerId;
            claim.PersonId = @event.CustomerPersonId;
            claim.CustomerPreferredName = @event.CustomerPreferredName;
            claim.CustomerFullName = @event.CustomerFullName;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.DisassociateClaimWithPolicyEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.PolicyNumber = null;
            claim.PolicyId = null;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimDeletedEvent @event, int sequenceNumber)
        {
            this.repository.DeleteById(@event.TenantId, @event.AggregateId);
            this.writableClaimVersionReadModelRepository.Delete(@event.TenantId, v => v.ClaimId == @event.AggregateId);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimStatusUpdatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.Status = ClaimState.FromClaimStatus(@event.ClaimStatus);
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimFileAttachedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        /// <summary>
        /// Handle a claim form data updated event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimFormDataUpdatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.LatestFormData = @event.FormData;
            claim.LatestCalculationResultFormDataId = @event.FormUpdateId;
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        /// <summary>
        ///  Handle a claim calculation result updated event.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimCalculationResultCreatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.LatestCalculationResultId = @event.CalculationResultId;
            claim.LatestCalculationResultFormDataId = @event.CalculationResult.FormDataId;
            claim.LatestCalculationResult = new ClaimCalculationResultReadModel(@event.CalculationResult.Json);
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.FullNameUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var claim in this.GetClaimsByPersonId(@event.TenantId, @event.AggregateId))
            {
                claim.CustomerFullName = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PreferredNameUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var claim in this.GetClaimsByPersonId(@event.TenantId, @event.AggregateId))
            {
                claim.CustomerPreferredName = @event.Value;
            }
        }

        public void Handle(PersonAggregate aggregate, PersonAggregate.PersonUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var claim in this.GetClaimsByPersonId(@event.TenantId, @event.AggregateId))
            {
                claim.CustomerFullName = @event.PersonData.FullName;
                claim.CustomerPreferredName = @event.PersonData.PreferredName;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimStateChangedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.Status = @event.ResultingState;
            switch (claim.Status)
            {
                case nameof(ClaimState.Assessment):
                case nameof(ClaimState.Review):
                case nameof(ClaimState.Approved):
                    if (!claim.LodgedTimestamp.HasValue)
                    {
                        claim.LodgedTimestamp = @event.Timestamp;
                    }

                    break;
                case nameof(ClaimState.Complete):
                    if (!claim.LodgedTimestamp.HasValue)
                    {
                        claim.LodgedTimestamp = @event.Timestamp;
                    }

                    claim.SettledTimestamp = @event.Timestamp;
                    break;
                case nameof(ClaimState.Declined):
                    claim.DeclinedTimestamp = @event.Timestamp;
                    break;
                default:
                    break;
            }

            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimNumberUpdatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.ClaimNumber = @event.ClaimNumber;
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        public void Handle(QuoteAggregate aggregate, QuoteAggregate.PolicyNumberUpdatedEvent @event, int sequenceNumber)
        {
            foreach (var claim in this.GetClaimsByPolicyId(@event.TenantId, @event.AggregateId))
            {
                claim.PolicyNumber = @event.PolicyNumber;
                claim.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimWorkflowStepAssignedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.WorkflowStep = @event.WorkflowStep;
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimActualisedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.IsActualised = true;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.TenantId = @event.TenantId;
            claim.ProductId = @event.ProductNewId;
            claim.LastModifiedTimestamp = @event.Timestamp;
            aggregate.ReadModel = claim;
        }

        public void Handle(
            ClaimAggregate aggregate,
            AdditionalPropertyValueInitializedEvent<ClaimAggregate, IClaimEventObserver> @event,
            int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.propertyTypeEvaluatorService.DeleteAdditionalPropertyValue(
                    (TGuid<Tenant>)@event.TenantId,
                    @event.AdditionalPropertyDefinitionType,
                    (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId);
            }

            this.propertyTypeEvaluatorService.CreateNewAdditionalPropertyValueByPropertyType(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);
        }

        public void Handle(
            ClaimAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<ClaimAggregate, IClaimEventObserver> @event,
            int sequenceNumber)
        {
            this.propertyTypeEvaluatorService.UpdateAdditionalPropertyValue(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);

            var claim = this.repository
                .GetById(@event.TenantId, @event.EntityId);

            if (claim != null)
            {
                claim.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.ClaimId);
            if (claim != null)
            {
                claim.OrganisationId = @event.OrganisationId;
                claim.LastModifiedTimestamp = @event.Timestamp;
                aggregate.ReadModel = claim;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var claims = this.GetClaimsByPersonId(@event.TenantId, @event.PersonId);
            foreach (var claim in claims)
            {
                claim.OrganisationId = @event.OrganisationId;
                claim.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.OwnerUserId = @event.UserId;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.OwnerUserId = default;
            aggregate.ReadModel = claim;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.CustomerAssignedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            claim.PersonId = @event.PersonId;
            claim.CustomerId = @event.CustomerId;
            claim.CustomerFullName = @event.CustomerDetails.FullName;
            claim.CustomerPreferredName = @event.CustomerDetails.PreferredName;
            aggregate.ReadModel = claim;
        }

        private ClaimReadModel GetClaimById(Guid tenantId, Guid claimId)
            => this.repository.Single(tenantId, c => c.TenantId == tenantId && c.Id == claimId);

        private IEnumerable<ClaimReadModel> GetClaimsByPersonId(Guid tenantId, Guid personId)
            => this.repository.Where(tenantId, c => c.TenantId == tenantId && c.PersonId == personId);

        private IEnumerable<ClaimReadModel> GetClaimsByPolicyId(Guid tenantId, Guid policyId)
            => this.repository.Where(tenantId, c => c.TenantId == tenantId && c.PolicyId == policyId);
    }
}
