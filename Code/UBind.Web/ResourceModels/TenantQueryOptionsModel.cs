// <copyright file="TenantQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model representing the filtering options when querying for Tenant.
    /// </summary>
    public class TenantQueryOptionsModel : QueryOptionsModel
    {
        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <param name="defaultSortBy">The default sortBy per entity.</param>
        /// <returns>Read model filters.</returns>
        public override async Task<EntityListFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver, string defaultSortBy)
        {
            var filter = await base.ToFilters(contextTenantId, cachingResolver, defaultSortBy);
            if (string.IsNullOrEmpty(this.SortBy))
            {
                filter.SortBy = defaultSortBy;
                filter.SortOrder = SortDirection.Ascending;
            }

            return filter;
        }
    }
}
