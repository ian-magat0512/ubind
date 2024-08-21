// <copyright file="PortalReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Portal
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.Portal.PortalAggregate;

    public class PortalReadModelWriter : IPortalReadModelWriter
    {
        private readonly IWritableReadModelRepository<PortalReadModel> portalReadModelRepository;

        public PortalReadModelWriter(IWritableReadModelRepository<PortalReadModel> portalReadModelRepository)
        {
            this.portalReadModelRepository = portalReadModelRepository;
        }

        public void Dispatch(
            PortalAggregate aggregate,
            IEvent<PortalAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(PortalAggregate aggregate, PortalCreatedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.portalReadModelRepository.DeleteById(@event.TenantId, @event.AggregateId);
            }

            var portal = new PortalReadModel
            {
                TenantId = @event.TenantId,
                Id = @event.AggregateId,
                CreatedTimestamp = @event.Timestamp,
                LastModifiedTimestamp = @event.Timestamp,
                Name = @event.Name,
                Alias = @event.Alias,
                Title = @event.Title,
                UserType = @event.UserType,
                OrganisationId = @event.OrganisationId,
            };

            this.portalReadModelRepository.Add(portal);
            aggregate.ReadModel = portal;
        }

        public void Handle(PortalAggregate aggregate, PortalUpdatedEvent @event, int sequenceNumber)
        {
            var portal = this.portalReadModelRepository.GetById(@event.TenantId, @event.AggregateId);
            portal.Name = @event.Name;
            portal.Alias = @event.Alias;
            portal.Title = @event.Title;
            portal.UserType = @event.UserType ?? portal.UserType;
            aggregate.ReadModel = portal;
        }

        public void Handle(PortalAggregate aggregate, SetPortalLocationEvent @event, int sequenceNumber)
        {
            var portal = this.portalReadModelRepository.GetById(@event.TenantId, @event.AggregateId);
            switch (@event.Environment)
            {
                case DeploymentEnvironment.Production:
                    portal.ProductionUrl = @event.Url;
                    break;
                case DeploymentEnvironment.Staging:
                    portal.StagingUrl = @event.Url;
                    break;
                case DeploymentEnvironment.Development:
                    portal.DevelopmentUrl = @event.Url;
                    break;
                default:
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(@event.Environment, typeof(DeploymentEnvironment)));
            }

            aggregate.ReadModel = portal;
        }

        public void Handle(PortalAggregate aggregate, UpdatePortalStylesEvent @event, int sequenceNumber)
        {
            var portal = this.portalReadModelRepository.GetById(@event.TenantId, @event.AggregateId);
            portal.StyleSheetUrl = @event.StylesheetUrl;
            portal.Styles = @event.Styles;
            aggregate.ReadModel = portal;
        }

        public void Handle(PortalAggregate aggregate, DisablePortalEvent @event, int sequenceNumber)
        {
            var portal = this.portalReadModelRepository.GetById(@event.TenantId, @event.AggregateId);
            portal.Disabled = true;
            portal.IsDefault = false;
            aggregate.ReadModel = portal;
        }

        public void Handle(PortalAggregate aggregate, EnablePortalEvent @event, int sequenceNumber)
        {
            var portal = this.portalReadModelRepository.GetById(@event.TenantId, @event.AggregateId);
            portal.Disabled = false;
            aggregate.ReadModel = portal;
        }

        public void Handle(PortalAggregate aggregate, DeletePortalEvent @event, int sequenceNumber)
        {
            var portal = this.portalReadModelRepository.GetById(@event.TenantId, @event.AggregateId);
            portal.Deleted = true;
            aggregate.ReadModel = portal;
        }

        public void Handle(PortalAggregate aggregate, SetDefaultPortalEvent @event, int sequenceNumber)
        {
            var portal = this.portalReadModelRepository.GetById(@event.TenantId, @event.AggregateId);
            portal.IsDefault = @event.IsDefault;
            aggregate.ReadModel = portal;
        }
    }
}
