// <copyright file="ClaimPeriodicSummaryQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBind.Application;
using UBind.Application.Dashboard;
using UBind.Application.Dashboard.Model;
using UBind.Application.Helpers;
using UBind.Domain;
using UBind.Domain.Enums;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Claim;

/// <summary>
/// Model used for the parameters of the GET request to the Claim Periodic Summary endpoint.
/// </summary>
public class ClaimPeriodicSummaryQueryOptionsModel : BasePeriodicSummaryQueryOptionsModel, IPeriodicSummaryQueryOptionsModel
{
    protected override HashSet<string> ValidIncludeProperties => ClaimPeriodicSummaryModel.IncludeProperties;

    /// <summary>
    /// Create filters from these options.
    /// </summary>
    /// <param name="contextTenantId">The context tenant Id.</param>
    /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
    /// <returns>Read model filters.</returns>
    public async Task<EntityListFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver)
    {
        await this.ConvertContextToGuid(contextTenantId, cachingResolver);
        var tenantId = this.TenantId ?? contextTenantId;
        var productIds = await this.GetValidProducts(contextTenantId, cachingResolver);
        this.Statuses = new List<string> { ClaimState.Complete, ClaimState.Declined };
        var filter = new EntityListFilters
        {
            ProductIds = productIds.Any() ? productIds : Enumerable.Empty<Guid>(),
            SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
            Sources = this.Sources,
            Statuses = this.Statuses ?? Enumerable.Empty<string>(),
            IncludeTestData = this.IncludeTestData,
            PolicyNumber = this.PolicyNumber,
            ProductId = this.ProductId,
            TenantId = this.TenantId,
            OrganisationIds = this.OrganisationId.HasValue ? new List<Guid> { this.OrganisationId.Value } : Enumerable.Empty<Guid>(),
            CustomerId = this.CustomerId,
            Page = this.Page,
            PageSize = this.PageSize,
            Environment = EnvironmentHelper.ParseOptionalEnvironmentOrThrow(this.Environment),
            OwnerUserId = this.OwnerUserId,
            EntityId = this.EntityId,
            EntityType = this.EntityType,
            PolicyId = this.PolicyId,
            SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : nameof(ClaimReadModel.CreatedTicksSinceEpoch),
            SortOrder = !string.IsNullOrEmpty(this.SortOrder.ToString())
                        ? (SortDirection)Enum.Parse(typeof(SortDirection), this.SortOrder.ToString())
                        : SortDirection.Descending,
        };

        filter.DateFilteringPropertyName = nameof(ClaimReadModel.CreatedTicksSinceEpoch);
        filter.DateIsAfterTicks = this.FromDateTime?.ToTicksFromExtendedISO8601InZone(this.Timezone);
        filter.DateIsBeforeTicks = this.ToDateTime?.ToTicksFromExtendedISO8601InZone(this.Timezone);
        return filter;
    }

    protected override long GetNumberOfExpectedPeriods()
    {
        return new ClaimSummaryGeneratorFactory().GetNumberOfExpectedPeriods(
                this.SamplePeriodLength.Value,
                this.FromDateTime,
                this.ToDateTime,
                this.TimeZoneId,
                this.CustomSamplePeriodMinutes);
    }
}