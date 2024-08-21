// <copyright file="SearchPoliciesIndexQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Policy;

using System;
using System.Collections.Generic;
using UBind.Domain;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Search;

/// <summary>
/// This query handler is to search the policy list base on the filters.
/// </summary>
public class SearchPoliciesIndexQuery : IQuery<IEnumerable<IPolicySearchResultItemReadModel>>
{
    public SearchPoliciesIndexQuery(Guid tenantId, DeploymentEnvironment environment, PolicyReadModelFilters filters)
    {
        this.TenantId = tenantId;
        this.Environment = environment;
        this.Filters = filters;
    }

    public Guid TenantId { get; }

    public DeploymentEnvironment Environment { get; }

    public PolicyReadModelFilters Filters { get; }
}
