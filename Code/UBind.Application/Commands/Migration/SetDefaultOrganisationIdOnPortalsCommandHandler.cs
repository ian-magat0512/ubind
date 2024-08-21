// <copyright file="SetDefaultOrganisationIdOnPortalsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Sets the tenant's default organanisation id on portals.
    /// This is needed because we are adding organisation ID to portals
    /// and so we need to set an initial value. Since all portals were only
    /// against the tenant previously, it's fine to set the organisation ID
    /// to the default.
    /// </summary>
    public class SetDefaultOrganisationIdOnPortalsCommandHandler
        : ICommandHandler<SetDefaultOrganisationIdOnPortalsCommand, Unit>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IPortalRepository portalRepository;

        public SetDefaultOrganisationIdOnPortalsCommandHandler(
            ITenantRepository tenantRepository,
            IPortalRepository portalRepository)
        {
            this.tenantRepository = tenantRepository;
            this.portalRepository = portalRepository;
        }

        public Task<Unit> Handle(SetDefaultOrganisationIdOnPortalsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = this.tenantRepository.GetTenants();
            foreach (var tenant in tenants)
            {
                this.portalRepository.SetOrganisationIdOnPortalsForTenantWhenEmpty(
                    tenant.Id, tenant.Details.DefaultOrganisationId);
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
