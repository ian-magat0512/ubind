// <copyright file="SearchPoliciesIndexQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Policy;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Services.Search;
using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Search;

/// <summary>
/// This query handler is to search the policy list base on the filters.
/// </summary>
public class SearchPoliciesIndexQueryHandler : IQueryHandler<SearchPoliciesIndexQuery, IEnumerable<IPolicySearchResultItemReadModel>>
{
    private readonly ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> searchableEntityService;
    private readonly ICachingResolver cachingResolver;

    public SearchPoliciesIndexQueryHandler(
           ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> searchableEntityService,
           ICachingResolver cachingResolver)
    {
        this.searchableEntityService = searchableEntityService;
        this.cachingResolver = cachingResolver;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IPolicySearchResultItemReadModel>> Handle(SearchPoliciesIndexQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tenant = await this.cachingResolver.GetTenantOrThrow(request.TenantId);
        var policies = this.searchableEntityService.Search(tenant, request.Environment, request.Filters);
        return policies;
    }
}
