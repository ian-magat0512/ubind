// <copyright file="OrganisationReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;
    using OrganisationReadModel = UBind.Domain.ReadModel.OrganisationReadModel;

    /// <inheritdoc/>
    public class OrganisationReadModelWriter : IOrganisationReadModelWriter
    {
        private readonly IWritableReadModelRepository<OrganisationReadModel> organisationRepository;
        private readonly PropertyTypeEvaluatorService propertyTypeEvaluatorService;

        public OrganisationReadModelWriter(
            IWritableReadModelRepository<OrganisationReadModel> organisationRepository,
            PropertyTypeEvaluatorService propertyTypeEvaluatorService)
        {
            this.organisationRepository = organisationRepository;
            this.propertyTypeEvaluatorService = propertyTypeEvaluatorService;
        }

        public void Dispatch(
            OrganisationAggregate aggregate,
            IEvent<OrganisationAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationInitializedEvent @event,
            int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                // delete the old read model
                this.organisationRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var readModel = new OrganisationReadModel(
                @event.TenantId,
                @event.AggregateId,
                @event.Alias,
                @event.Name,
                @event.ManagingOrganisationId,
                @event.IsActive,
                @event.IsDeleted,
                @event.Timestamp);
            this.organisationRepository.Add(readModel);
            aggregate.LatestProjectedReadModel = readModel;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationNameUpdatedEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.Name = @event.Value;
            organisation.LastModifiedTicksSinceEpoch = @event.Timestamp.ToUnixTimeTicks();
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationAliasUpdatedEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.Alias = @event.Value;
            organisation.LastModifiedTicksSinceEpoch = @event.Timestamp.ToUnixTimeTicks();
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationDeletedEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.IsDeleted = true;
            organisation.IsActive = false;
            organisation.LastModifiedTicksSinceEpoch = @event.Timestamp.ToUnixTimeTicks();
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationActivatedEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.IsActive = true;
            organisation.LastModifiedTicksSinceEpoch = @event.Timestamp.ToUnixTimeTicks();
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationDisabledEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.IsActive = false;
            organisation.LastModifiedTicksSinceEpoch = @event.Timestamp.ToUnixTimeTicks();
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationTenantUpdatedEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.TenantId = @event.TenantId;
            organisation.LastModifiedTicksSinceEpoch = @event.Timestamp.ToUnixTimeTicks();
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.ApplyNewIdEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.TenantId = @event.TenantId;
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.SetOrganisationDefaultPortalEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.DefaultPortalId = @event.PortalId;
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.ManagingOrganisationUpdatedEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.ManagingOrganisationId = @event.ManagingOrganisationId;
            aggregate.LatestProjectedReadModel = organisation;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            AdditionalPropertyValueInitializedEvent<OrganisationAggregate, IOrganisationEventObserver> @event,
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

            var organisation = this.GetOrganisationById(@event.TenantId, @event.EntityId);
            if (organisation != null)
            {
                organisation.LastModifiedTicksSinceEpoch = @event.Timestamp.ToUnixTimeTicks();
                aggregate.LatestProjectedReadModel = organisation;
            }
        }

        public void Handle(
            OrganisationAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<OrganisationAggregate, IOrganisationEventObserver> @event,
            int sequenceNumber)
        {
            this.propertyTypeEvaluatorService.UpdateAdditionalPropertyValue(
                (TGuid<Tenant>)@event.TenantId,
                @event.EntityId,
                @event.AdditionalPropertyDefinitionType,
                (TGuid<AdditionalPropertyDefinition>)@event.AdditionalPropertyDefinitionId,
                (TGuid<AdditionalPropertyValue>)@event.AdditionalPropertyValueId,
                @event.Value);

            var organisation = this.GetOrganisationById(@event.TenantId, @event.EntityId);
            if (organisation != null)
            {
                organisation.LastModifiedTicksSinceEpoch = @event.Timestamp.ToUnixTimeTicks();
                aggregate.LatestProjectedReadModel = organisation;
            }
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.SetDefaultOrganisationEvent @event,
            int sequenceNumber)
        {
            var organisation = this.GetOrganisationById(@event.TenantId, @event.AggregateId);
            organisation.IsDefault = @event.IsDefault;
            aggregate.LatestProjectedReadModel = organisation;
        }

        private OrganisationReadModel GetOrganisationById(Guid tenantId, Guid organisationId)
        {
            return this.organisationRepository.GetById(tenantId, organisationId);
        }
    }
}
