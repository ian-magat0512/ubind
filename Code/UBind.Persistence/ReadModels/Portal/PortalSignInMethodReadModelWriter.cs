// <copyright file="PortalSignInMethodReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Portal
{
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.Portal.PortalAggregate;

    public class PortalSignInMethodReadModelWriter : IPortalSignInMethodReadModelWriter
    {
        private readonly IWritableReadModelRepository<PortalSignInMethodReadModel> portalSignInMethodReadModelRepository;
        private readonly IUBindDbContext dbContext;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;

        public PortalSignInMethodReadModelWriter(
            IWritableReadModelRepository<PortalSignInMethodReadModel> portalSignInMethodReadModelRepository,
            IUBindDbContext dbContext,
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository)
        {
            this.portalSignInMethodReadModelRepository = portalSignInMethodReadModelRepository;
            this.dbContext = dbContext;
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
        }

        public void Dispatch(
            PortalAggregate aggregate,
            IEvent<PortalAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(PortalAggregate aggregate, PortalSignInMethodEnabledEvent @event, int sequenceNumber)
        {
            var method = this.GetMethod(@event.TenantId, @event.AggregateId, @event.AuthenticationMethodId);
            if (method == null)
            {
                var authenticationMethod = this.authenticationMethodReadModelRepository.Get(@event.TenantId, @event.AuthenticationMethodId);
                method = new PortalSignInMethodReadModel
                {
                    Id = @event.AuthenticationMethodId,
                    TenantId = @event.TenantId,
                    PortalId = @event.AggregateId,
                    IsEnabled = true,
                    SortOrder = aggregate.SignInMethods
                        .Single(s => s.AuthenticationMethodId == @event.AuthenticationMethodId).SortOrder,
                    Name = authenticationMethod.Name,
                    TypeName = authenticationMethod.TypeName,
                };
                this.portalSignInMethodReadModelRepository.Add(method);
            }
            else
            {
                method.IsEnabled = true;
            }
        }

        public void Handle(PortalAggregate aggregate, PortalSignInMethodDisabledEvent @event, int sequenceNumber)
        {
            var method = this.GetMethod(@event.TenantId, @event.AggregateId, @event.AuthenticationMethodId);
            if (method == null)
            {
                var authenticationMethod = this.authenticationMethodReadModelRepository.Get(@event.TenantId, @event.AuthenticationMethodId);
                method = new PortalSignInMethodReadModel
                {
                    Id = @event.AuthenticationMethodId,
                    TenantId = @event.TenantId,
                    PortalId = @event.AggregateId,
                    IsEnabled = false,
                    SortOrder = aggregate.SignInMethods
                        .Single(s => s.AuthenticationMethodId == @event.AuthenticationMethodId).SortOrder,
                    Name = authenticationMethod.Name,
                    TypeName = authenticationMethod.TypeName,
                };
                this.portalSignInMethodReadModelRepository.Add(method);
            }
            else
            {
                method.IsEnabled = false;
            }
        }

        public void Handle(PortalAggregate aggregate, PortalSignInMethodSortOrderChangedEvent @event, int sequenceNumber)
        {
            var readModels = this.GetMethodsForPortal(@event.TenantId, @event.AggregateId);
            foreach (var method in aggregate.SignInMethods)
            {
                var readModel = readModels.Single(x => x.AuthenticationMethodId == method.AuthenticationMethodId);
                readModel.SortOrder = method.SortOrder;
            }
        }

        private List<PortalSignInMethodReadModel> GetMethodsForPortal(Guid tenantId, Guid portalId)
        {
            return this.dbContext.PortalSignInMethods
                .Where(x => x.TenantId == tenantId && x.PortalId == portalId)
                .ToList();
        }

        private PortalSignInMethodReadModel? GetMethod(Guid tenantId, Guid portalId, Guid authenticationMethodId)
        {
            var result = this.portalSignInMethodReadModelRepository.SingleMaybe(
                tenantId, x => x.PortalId == portalId && x.Id == authenticationMethodId);

            return result.GetValueOrDefault();
        }
    }
}
