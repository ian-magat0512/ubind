// <copyright file="GetDefaultOrganisationForTenantQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query Handler for getting default organisation for a tenant.
    /// </summary>
    public class GetDefaultOrganisationForTenantQueryHandler : IQueryHandler<GetDefaultOrganisationForTenantQuery, IOrganisationReadModelSummary>
    {
        private readonly ICachingResolver cachingResolver;

        public GetDefaultOrganisationForTenantQueryHandler(
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> Handle(GetDefaultOrganisationForTenantQuery request, CancellationToken cancellationToken)
        {
            var tenant = await this.cachingResolver.GetTenantOrThrow(request.TenantId);
            var defaultOrganisationId = tenant.Details.DefaultOrganisationId;
            var organisationReadModel = await this.cachingResolver.GetOrganisationOrThrow(request.TenantId, defaultOrganisationId);
            return this.GetSummary(organisationReadModel, defaultOrganisationId);
        }

        private IOrganisationReadModelSummary GetSummary(
            OrganisationReadModel organisation, Guid tenantDefaultOrganisationId)
        {
            return new OrganisationReadModelSummary
            {
                TenantId = organisation.TenantId,
                Id = organisation.Id,
                Alias = organisation.Alias,
                Name = organisation.Name,
                IsActive = organisation.IsActive,
                IsDeleted = organisation.IsDeleted,
                IsDefault = organisation.Id == tenantDefaultOrganisationId,
                CreatedTimestamp = organisation.CreatedTimestamp,
                LastModifiedTimestamp = organisation.LastModifiedTimestamp,
            };
        }
    }
}
