// <copyright file="OrganisationLinkedIdentityReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Organisation
{
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.Repositories;

    public class OrganisationLinkedIdentityReadModelWriter : IOrganisationLinkedIdentityReadModelWriter
    {
        private readonly IUBindDbContext dbContext;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodRepository;

        public OrganisationLinkedIdentityReadModelWriter(
            IUBindDbContext dbContext,
            IAuthenticationMethodReadModelRepository authenticationMethodRepository)
        {
            this.dbContext = dbContext;
            this.authenticationMethodRepository = authenticationMethodRepository;
        }

        public void Dispatch(
            Organisation aggregate,
            IEvent<Organisation, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(Organisation aggregate, Organisation.OrganisationIdentityLinkedEvent @event, int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                var existingRecord = this.dbContext.OrganisationLinkedIdentities
                    .SingleOrDefault(li => li.TenantId == @event.TenantId
                        && li.OrganisationId == @event.AggregateId
                        && li.AuthenticationMethodId == @event.AuthenticationMethodId
                        && li.UniqueId == @event.UniqueId);
                if (existingRecord != null)
                {
                    this.dbContext.OrganisationLinkedIdentities.Remove(existingRecord);
                }
            }

            var authenticationMethod = this.authenticationMethodRepository.Get(@event.TenantId, @event.AuthenticationMethodId);
            var readModel = new OrganisationLinkedIdentityReadModel
            {
                TenantId = @event.TenantId,
                OrganisationId = @event.AggregateId,
                AuthenticationMethodId = @event.AuthenticationMethodId,
                AuthenticationMethodName = authenticationMethod.Name,
                AuthenticationMethodTypeName = authenticationMethod.TypeName,
                UniqueId = @event.UniqueId,
            };
            this.dbContext.OrganisationLinkedIdentities.Add(readModel);
        }

        public void Handle(Organisation aggregate, Organisation.OrganisationIdentityUnlinkedEvent @event, int sequenceNumber)
        {
            var readModel = this.dbContext.OrganisationLinkedIdentities.Find(@event.AggregateId, @event.AuthenticationMethodId);
            if (readModel != null)
            {
                this.dbContext.OrganisationLinkedIdentities.Remove(readModel);
            }
        }

        public void Handle(Organisation aggregate, Organisation.OrganisationLinkedIdentityUpdatedEvent @event, int sequenceNumber)
        {
            var readModel = this.Get(@event.TenantId, @event.AggregateId, @event.AuthenticationMethodId);
            if (readModel == null)
            {
                throw new InvalidOperationException("trying to update a LinkedIdentity that does not exist");
            }

            readModel.UniqueId = @event.UniqueId;
        }

        private OrganisationLinkedIdentityReadModel Get(Guid tenantId, Guid organisationId, Guid authenticationMethodId)
        {
            return this.dbContext.OrganisationLinkedIdentities.Find(organisationId, authenticationMethodId);
        }
    }
}
