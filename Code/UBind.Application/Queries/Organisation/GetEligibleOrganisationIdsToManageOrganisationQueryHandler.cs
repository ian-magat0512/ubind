// <copyright file="GetEligibleOrganisationIdsToManageOrganisationQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetEligibleOrganisationIdsToManageOrganisationQueryHandler
        : IQueryHandler<GetEligibleOrganisationIdsToManageOrganisationQuery, List<Guid>>
    {
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;

        public GetEligibleOrganisationIdsToManageOrganisationQueryHandler(
            IOrganisationReadModelRepository organisationReadModelRepository)
        {
            this.organisationReadModelRepository = organisationReadModelRepository;
        }

        public async Task<List<Guid>> Handle(GetEligibleOrganisationIdsToManageOrganisationQuery query, CancellationToken cancellationToken)
        {
            var organisation = this.organisationReadModelRepository.Get(query.TenantId, query.OrganisationId);
            if (organisation == null)
            {
                throw new ErrorException(Domain.Errors.Organisation.NotFound(query.OrganisationId));
            }

            var filters = new OrganisationReadModelFilters()
            {
                TenantId = query.TenantId,
            };

            // List of all organisations in the tenancy
            var eligibleOrganisations = this.organisationReadModelRepository.GetIds(query.TenantId, filters);

            // Get all descendant organisations of the organisation
            var descendantOrganisations = await this.organisationReadModelRepository
                .GetIdsOfDescendantOrganisationsOfOrganisation(query.TenantId, query.OrganisationId);

            // Remove the organisation, all of its descendant, and its current managing organisation from the list of all eligible organisations
            eligibleOrganisations.RemoveAll(
                a => query.OrganisationId == a
                || descendantOrganisations.Contains(a)
                || (organisation.ManagingOrganisationId.HasValue && organisation.ManagingOrganisationId.Value == a));

            return eligibleOrganisations;
        }
    }
}
