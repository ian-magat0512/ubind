// <copyright file="GetIdsOfOrganisationsManagedByQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetIdsOfOrganisationsManagedByQueryHandler : IQueryHandler<GetIdsOfOrganisationsManagedByQuery, List<Guid>>
    {
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;

        public GetIdsOfOrganisationsManagedByQueryHandler(
            IOrganisationReadModelRepository organisationReadModelRepository)
        {
            this.organisationReadModelRepository = organisationReadModelRepository;
        }

        public Task<List<Guid>> Handle(GetIdsOfOrganisationsManagedByQuery query, CancellationToken cancellationToken)
        {
            var filters = new OrganisationReadModelFilters();
            if (query.ManagingOrganisationId.HasValue)
            {
                filters.ManagingOrganisationId = query.ManagingOrganisationId;
            }

            return Task.FromResult(this.organisationReadModelRepository.GetIds(query.TenantId, filters));
        }
    }
}
