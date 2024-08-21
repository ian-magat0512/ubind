// <copyright file="GetOrganisationSummaryByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Gets an org.
    /// </summary>
    public class GetOrganisationSummaryByIdQueryHandler : IQueryHandler<GetOrganisationSummaryByIdQuery, IOrganisationReadModelSummary>
    {
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly ITenantRepository tenantRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrganisationSummaryByIdQueryHandler"/> class.
        /// </summary>
        /// <param name="organisationReadModelRepository">The org read model repo.</param>
        /// <param name="tenantRepository">The tenant repo.</param>
        public GetOrganisationSummaryByIdQueryHandler(
            IOrganisationReadModelRepository organisationReadModelRepository,
            ITenantRepository tenantRepository)
        {
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.tenantRepository = tenantRepository;
        }

        /// <inheritdoc/>
        public async Task<IOrganisationReadModelSummary> Handle(GetOrganisationSummaryByIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var organisation = this.organisationReadModelRepository.Get(request.TenantId, request.OrganisationId);
            var tenant = this.tenantRepository.GetTenantById(request.TenantId);
            return await Task.FromResult(this.GetSummary(organisation, tenant.Details.DefaultOrganisationId));
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
                ManagingOrganisationId = organisation.ManagingOrganisationId,
                IsActive = organisation.IsActive,
                IsDeleted = organisation.IsDeleted,
                IsDefault = organisation.Id == tenantDefaultOrganisationId,
                CreatedTimestamp = organisation.CreatedTimestamp,
                LastModifiedTimestamp = organisation.LastModifiedTimestamp,
            };
        }
    }
}
