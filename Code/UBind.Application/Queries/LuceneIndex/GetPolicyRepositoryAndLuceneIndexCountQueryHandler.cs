// <copyright file="GetPolicyRepositoryAndLuceneIndexCountQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.LuceneIndex
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Services.Search;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search;

    public class GetPolicyRepositoryAndLuceneIndexCountQueryHandler : IQueryHandler<GetPolicyRepositoryAndLuceneIndexCountQuery, IEnumerable<IEntityRepositoryAndLuceneIndexCountModel>>
    {
        private readonly ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> searchableEntityService;
        private readonly ITenantRepository tenantRepository;

        public GetPolicyRepositoryAndLuceneIndexCountQueryHandler(
            ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> searchableEntityService,
            ITenantRepository tenantRepository)
        {
            this.searchableEntityService = searchableEntityService;
            this.tenantRepository = tenantRepository;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IEntityRepositoryAndLuceneIndexCountModel>> Handle(GetPolicyRepositoryAndLuceneIndexCountQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenants = new List<Tenant>();
            var repositoryAndLuceneIndexCountModel = new List<IEntityRepositoryAndLuceneIndexCountModel>();

            if (query.TenantId.HasValue)
            {
                var tenant = this.tenantRepository.GetTenantById(query.TenantId.Value);
                tenants.Add(tenant);
            }
            else
            {
                tenants = this.tenantRepository.GetTenants(includeMasterTenant: false).ToList();
            }

            foreach (var tenant in tenants)
            {
                var resultRepositoryAndLuceneIndexCounts = this.searchableEntityService.GetRepositoryAndLuceneIndexCount(
                    tenant,
                    query.Environment,
                    query.FromTimestamp,
                    query.ToTimestamp);

                repositoryAndLuceneIndexCountModel.Add(resultRepositoryAndLuceneIndexCounts);
            }

            return Task.FromResult(repositoryAndLuceneIndexCountModel.AsEnumerable());
        }
    }
}
