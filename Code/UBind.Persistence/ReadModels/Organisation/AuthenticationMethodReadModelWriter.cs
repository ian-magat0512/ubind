// <copyright file="AuthenticationMethodReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Organisation
{
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Organisation.AuthenticationMethod;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.Repositories;
    using OrganisationAggregate = UBind.Domain.Aggregates.Organisation.Organisation;

    public class AuthenticationMethodReadModelWriter : IAuthenticationMethodReadModelWriter
    {
        private readonly IWritableReadModelRepository<AuthenticationMethodReadModelSummary> repository;

        public AuthenticationMethodReadModelWriter(IWritableReadModelRepository<AuthenticationMethodReadModelSummary> repository)
        {
            this.repository = repository;
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
            OrganisationAggregate.OrganisationAuthenticationMethodAddedEvent @event,
            int sequenceNumber)
        {
            if (aggregate.IsBeingReplayed)
            {
                this.repository.DeleteById(@event.TenantId, @event.AuthenticationMethod.Id);
            }

            AuthenticationMethodReadModelSummary readModel;
            switch (@event.AuthenticationMethod)
            {
                case LocalAccount:
                    readModel = new LocalAccountAuthenticationMethodReadModel(@event);
                    break;
                case Saml:
                    readModel = new SamlAuthenticationMethodReadModel(@event);
                    break;
                default:
                    throw new ErrorException(Domain.Errors.General.Unexpected(
                        "When trying to instantiate an AuthenticalReadModel, we came across an unknown type "
                        + $"{@event.AuthenticationMethod.GetType().Name}."));
            }

            this.repository.Add(readModel);
            aggregate.LatestProjectedAuthenticationMethodReadModel = readModel;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationAuthenticationMethodUpdatedEvent @event,
            int sequenceNumber)
        {
            var readModel = this.GetAuthenticationMethodById(@event.TenantId, @event.AuthenticationMethod.Id);
            if (readModel == null)
            {
                throw new ArgumentException("When trying to update the authentication method, its read model was not "
                    + "found. You can try regenerating it by replaying the aggregate events for the organisation.");
            }

            readModel.Update(@event);
            aggregate.LatestProjectedAuthenticationMethodReadModel = readModel;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationAuthenticationMethodDeletedEvent @event,
            int sequenceNumber)
        {
            this.repository.DeleteById(@event.TenantId, @event.AuthenticationMethodId);
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationAuthenticationMethodDisabledEvent @event,
            int sequenceNumber)
        {
            var readModel = this.GetAuthenticationMethodById(@event.TenantId, @event.AuthenticationMethodId);
            if (readModel == null)
            {
                throw new ArgumentException("When trying to disable the authentication method, its read model was not "
                    + "found. You can try regenerating it by replaying the aggregate events for the organisation.");
            }

            readModel.Disabled = true;
            aggregate.LatestProjectedAuthenticationMethodReadModel = readModel;
        }

        public void Handle(
            OrganisationAggregate aggregate,
            OrganisationAggregate.OrganisationAuthenticationMethodEnabledEvent @event,
            int sequenceNumber)
        {
            var readModel = this.GetAuthenticationMethodById(@event.TenantId, @event.AuthenticationMethodId);
            if (readModel == null)
            {
                throw new ArgumentException("When trying to enable the authentication method, its read model was not "
                    + "found. You can try regenerating it by replaying the aggregate events for the organisation.");
            }

            readModel.Disabled = false;
            aggregate.LatestProjectedAuthenticationMethodReadModel = readModel;
        }

        private AuthenticationMethodReadModelSummary GetAuthenticationMethodById(Guid tenantId, Guid authentictionMethodId)
        {
            return this.repository.GetById(tenantId, authentictionMethodId);
        }
    }
}
