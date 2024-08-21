// <copyright file="ClaimAttachmentReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Claim
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.Repositories;

    /// <summary>
    /// For updating the claim attachment read model in response to events from the write model.
    /// </summary>
    public class ClaimAttachmentReadModelWriter : IClaimAttachmentReadModelWriter
    {
        private readonly IWritableReadModelRepository<ClaimAttachmentReadModel> readModelRepository;
        private readonly IWritableReadModelRepository<ClaimReadModel> claimRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimAttachmentReadModelWriter"/> class.
        /// </summary>
        /// <param name="readModelRepository">The repository for claim attachment read models.</param>
        /// <param name="claimRepository">The repository for claim read models.</param>
        public ClaimAttachmentReadModelWriter(
            IWritableReadModelRepository<ClaimAttachmentReadModel> readModelRepository,
            IWritableReadModelRepository<ClaimReadModel> claimRepository)
        {
            this.readModelRepository = readModelRepository;
            this.claimRepository = claimRepository;
        }

        public void Dispatch(
            ClaimAggregate aggregate,
            IEvent<ClaimAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimFileAttachedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            Expression<Func<ClaimAttachmentReadModel, bool>> isExists = (d) =>
                d.TenantId == @event.TenantId &&
                d.ClaimOrClaimVersionId == @event.AggregateId &&
                d.OwnerType == DocumentOwnerType.Claim &&
                d.Name == @event.Attachment.Name;

            var readModel = ClaimAttachmentReadModel
                .CreateClaimAttachmentReadModel(@event.AggregateId, @event.Attachment);
            readModel.Environment = claim.Environment;
            readModel.TenantId = claim.TenantId;
            readModel.OrganisationId = claim.OrganisationId;
            readModel.IsTestData = claim.IsTestData;
            readModel.CustomerId = claim.CustomerId;
            this.readModelRepository.AddOrUpdate(@event.TenantId, readModel, isExists);
        }

        /// <summary>
        /// handle Claim attachment created event.
        /// </summary>
        /// <param name="event">The claim version created event.</param>
        /// <param name="sequenceNumber">The sequece number.</param>
        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimVersionFileAttachedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            Expression<Func<ClaimAttachmentReadModel, bool>> isExists = (d) =>
                d.TenantId == @event.TenantId &&
                d.ClaimOrClaimVersionId == @event.AggregateId &&
                d.OwnerType == DocumentOwnerType.ClaimVersion &&
                d.Name == @event.Attachment.Name;

            var readModel = ClaimAttachmentReadModel
                .CreateClaimVersionAttachmentReadModel(@event.AggregateId, @event.VersionId, @event.Attachment);
            readModel.Environment = claim.Environment;
            readModel.TenantId = claim.TenantId;
            readModel.OrganisationId = claim.OrganisationId;
            readModel.IsTestData = claim.IsTestData;
            readModel.CustomerId = claim.CustomerId;
            this.readModelRepository.AddOrUpdate(@event.TenantId, readModel, isExists);
        }

        /// <summary>
        /// handle Claim attachment created event.
        /// </summary>
        /// <param name="event">The claim version created event.</param>
        /// <param name="sequenceNumber">The sequece number.</param>
        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimVersionCreatedEvent @event, int sequenceNumber)
        {
            var claim = this.GetClaimById(@event.TenantId, @event.AggregateId);
            foreach (var attachment in @event.Attachments)
            {
                Expression<Func<ClaimAttachmentReadModel, bool>> isExists = (d) =>
                    d.TenantId == @event.TenantId &&
                    d.ClaimOrClaimVersionId == @event.AggregateId &&
                    d.OwnerType == DocumentOwnerType.ClaimVersion &&
                    d.Name == attachment.Name;

                var readModel = ClaimAttachmentReadModel
                    .CreateClaimVersionAttachmentReadModel(@event.AggregateId, @event.VersionId, attachment);
                readModel.Environment = claim.Environment;
                readModel.TenantId = claim.TenantId;
                readModel.OrganisationId = claim.OrganisationId;
                readModel.IsTestData = claim.IsTestData;
                readModel.CustomerId = claim.CustomerId;
                this.readModelRepository.AddOrUpdate(@event.TenantId, readModel, isExists);
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimOrganisationMigratedEvent @event, int sequenceNumber)
        {
            var attachments = this.readModelRepository
                .Where(@event.TenantId, x => x.ClaimId == @event.ClaimId);

            foreach (var attachment in attachments)
            {
                attachment.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(ClaimAggregate aggregate, ClaimAggregate.ClaimTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            var claims = this.readModelRepository
                .Where(@event.TenantId, c => c.ClaimId == @event.AggregateId);

            foreach (var claim in claims)
            {
                claim.OrganisationId = @event.OrganisationId;
            }
        }

        public void Handle(ClaimAggregate.CustomerAssignedEvent @event, int sequenceNumber)
        {
            var attachments = this.readModelRepository
                .Where(@event.TenantId, x => x.ClaimId == @event.AggregateId && x.CustomerId == @event.CustomerId);

            foreach (var attachment in attachments)
            {
                attachment.CustomerId = @event.CustomerId;
            }
        }

        private ClaimReadModel GetClaimById(Guid tenantId, Guid claimId)
        {
            return this.claimRepository.GetById(tenantId, claimId);
        }
    }
}
