// <copyright file="QueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Resource model for binding query filters to a QueryOptions instance.
    /// </summary>
    public class QueryOptionsModel : BaseQueryOptionsModel, IQueryOptionsModel
    {
        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <param name="defaultSortBy">The default sortBy per entity.</param>
        /// <returns>Read model filters.</returns>
        public virtual async Task<EntityListFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver, string defaultSortBy)
        {
            await this.ConvertContextToGuid(contextTenantId, cachingResolver);
            var filter = new EntityListFilters
            {
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                Sources = this.Sources,
                Statuses = this.Statuses ?? Enumerable.Empty<string>(),
                IncludeTestData = this.IncludeTestData,
                PolicyNumber = this.PolicyNumber,
                ProductId = this.ProductId,
                TenantId = this.TenantId,
                OrganisationIds = this.OrganisationId != null ? new List<Guid> { this.OrganisationId.Value } : null,
                CustomerId = this.CustomerId,
                Page = this.Page,
                PageSize = this.PageSize,
                Environment = EnvironmentHelper.ParseOptionalEnvironmentOrThrow(this.Environment),
                OwnerUserId = this.OwnerUserId,
                EntityId = this.EntityId,
                EntityType = this.EntityType,
                PolicyId = this.PolicyId,
                SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : defaultSortBy,
                SortOrder = !string.IsNullOrEmpty(this.SortOrder.ToString())
                            ? (SortDirection)Enum.Parse(typeof(SortDirection), this.SortOrder.ToString())
                            : SortDirection.Descending,
            };

            if (!string.IsNullOrEmpty(this.AfterDateTime) && !string.IsNullOrEmpty(this.DateFilteringPropertyName))
            {
                filter.DateFilteringPropertyName = this.DateFilteringPropertyName;
                filter.WithDateIsAfterFilter(this.AfterDateTime.ToLocalDateFromIso8601());
            }

            if (!string.IsNullOrEmpty(this.BeforeDateTime) && !string.IsNullOrEmpty(this.DateFilteringPropertyName))
            {
                filter.DateFilteringPropertyName = this.DateFilteringPropertyName;
                filter.WithDateIsBeforeFilter(this.BeforeDateTime.ToLocalDateFromIso8601());
            }

            return filter;
        }
    }
}
