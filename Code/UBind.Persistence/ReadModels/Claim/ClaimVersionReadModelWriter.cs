// <copyright file="ClaimVersionReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Claim
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Repositories;
    using UBind.Persistence;

    /// <summary>
    /// For updating the claim version read model in response to events from the write model.
    /// </summary>
    public class ClaimVersionReadModelWriter : IClaimVersionReadModelWriter
    {
        private readonly IWritableReadModelRepository<ClaimReadModel> claimReadModelUpdateRepository;
        private readonly IWritableReadModelRepository<ClaimVersionReadModel> claimVersionReadModelUpdateRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimVersionReadModelWriter"/> class.
        /// </summary>
        /// <param name="claimReadModelUpdateRepository">The claim writable read model repository.</param>
        /// <param name="claimVersionReadModelUpdateRepository">The claim vesion read model repository.</param>
        public ClaimVersionReadModelWriter(
            IWritableReadModelRepository<ClaimReadModel> claimReadModelUpdateRepository,
            IWritableReadModelRepository<ClaimVersionReadModel> claimVersionReadModelUpdateRepository)
        {
            this.claimReadModelUpdateRepository = claimReadModelUpdateRepository
                ?? throw new ArgumentNullException(nameof(claimReadModelUpdateRepository));
            this.claimVersionReadModelUpdateRepository = claimVersionReadModelUpdateRepository
                ?? throw new ArgumentNullException(nameof(claimVersionReadModelUpdateRepository));
        }

        public void Dispatch(
            ClaimAggregate aggregate,
            IEvent<ClaimAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimVersionFileAttachedEvent @event, int sequenceNumber)
        {
            var claimVersion = this.GetClaimVersionReadModel(@event.TenantId, @event.VersionId);
            claimVersion.LastModifiedTimestamp = @event.Timestamp;
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimNumberUpdatedEvent @event, int sequenceNumber)
        {
            var versions = this.GetClaimVersionsByClaimId(@event.TenantId, @event.AggregateId);
            foreach (var version in versions)
            {
                version.ClaimNumber = @event.ClaimNumber;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimVersionCreatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimReadModel(@event.TenantId, @event.AggregateId);
            var claimVersion = new ClaimVersionReadModel
            {
                Id = @event.VersionId,
                AggregateId = @event.AggregateId,
                ClaimId = claim.Id,
                ClaimVersionNumber = @event.VersionNumber,
                CreatedTimestamp = @event.Timestamp,
                LastModifiedTimestamp = claim.LastModifiedTimestamp,
                LatestFormData = claim.LatestFormData,
                ClaimReferenceNumber = claim.ClaimReference,
                CalculationResult = claim.LatestCalculationResult,
                SerializedCalculationResult = claim.SerializedLatestCalcuationResult,
                Environment = claim.Environment,
                CustomerId = claim.CustomerId,
                OwnerUserId = claim.OwnerUserId,
                TenantId = claim.TenantId,
                ProductId = claim.ProductId,
                OrganisationId = claim.OrganisationId,
                IsTestData = claim.IsTestData,
                PolicyId = claim.PolicyId,
                ClaimNumber = claim.ClaimNumber,
            };

            this.claimVersionReadModelUpdateRepository.Add(claimVersion);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ApplyNewIdEvent @event, int sequenceNumber)
        {
            var claimVersions = this.GetClaimVersionsByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var claimVersion in claimVersions)
            {
                claimVersion.TenantId = @event.TenantId;
                claimVersion.ProductId = @event.ProductNewId;
            }
        }

        public void Handle(
            ClaimAggregate aggregate,
            AdditionalPropertyValueUpdatedEvent<ClaimAggregate, IClaimEventObserver> @event,
            int sequenceNumber)
        {
            var claimVersion = this.claimVersionReadModelUpdateRepository
                .GetById(@event.TenantId, @event.EntityId);

            if (claimVersion != null)
            {
                claimVersion.LastModifiedTimestamp = @event.Timestamp;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var claimVersions = this.claimVersionReadModelUpdateRepository
                .Where(@event.TenantId, x => x.AggregateId == @event.AggregateId);

            foreach (var claimVersion in claimVersions)
            {
                claimVersion.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.OwnershipAssignedEvent @event, int sequenceNumber)
        {
            var claimVersions = this.GetClaimVersionsByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var claimVersion in claimVersions)
            {
                claimVersion.OwnerUserId = @event.UserId;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            var claimVersions = this.claimVersionReadModelUpdateRepository
                .Where(@event.TenantId, x => x.AggregateId == @event.AggregateId);
            foreach (var claimVersion in claimVersions)
            {
                claimVersion.OwnerUserId = null;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.CustomerAssignedEvent @event, int sequenceNumber)
        {
            var claimVersions = this.GetClaimVersionsByAggregateId(@event.TenantId, @event.AggregateId);
            foreach (var claimVersion in claimVersions)
            {
                claimVersion.CustomerId = @event.CustomerId;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var claimVersions = this.GetClaimVersionsByClaimId(@event.TenantId, @event.AggregateId);
            foreach (var claimVersion in claimVersions)
            {
                claimVersion.OrganisationId = @event.OrganisationId;
            }
        }

        private IEnumerable<ClaimVersionReadModel> GetClaimVersionsByAggregateId(Guid tenantId, Guid aggregateId)
            => this.claimVersionReadModelUpdateRepository.Where(tenantId, x => x.AggregateId == aggregateId);

        private ClaimVersionReadModel GetClaimVersionReadModel(Guid tenantId, Guid claimVersionId)
            => this.claimVersionReadModelUpdateRepository.Single(tenantId, cv => cv.Id == claimVersionId);

        private ClaimReadModel GetClaimReadModel(Guid tenantId, Guid claimId)
            => this.claimReadModelUpdateRepository.Single(tenantId, cv => cv.Id.Equals(claimId));

        private IEnumerable<ClaimVersionReadModel> GetClaimVersionsByClaimId(Guid tenantId, Guid claimId)
            => this.claimVersionReadModelUpdateRepository.Where(tenantId, c => c.ClaimId == claimId);
    }
}
