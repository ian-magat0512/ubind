// <copyright file="GetClaimPeriodicSummariesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.Claim;

using System;
using System.Collections.Generic;
using UBind.Application.Dashboard.Model;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

/// <summary>
/// Query for obtaining a list of claim summaries with the given filters.
/// </summary>
public class GetClaimPeriodicSummariesQuery : IQuery<List<ClaimPeriodicSummaryModel>>
{
    public GetClaimPeriodicSummariesQuery(Guid tenantId, EntityListFilters filters, IPeriodicSummaryQueryOptionsModel options)
    {
        this.TenantId = tenantId;
        this.Filters = filters;
        this.Options = options;
    }

    public Guid TenantId { get; }

    public EntityListFilters Filters { get; }

    public IPeriodicSummaryQueryOptionsModel Options { get; }
}