// <copyright file="RoleQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model representing the filtering options when querying for role.
    /// </summary>
    public class RoleQueryOptionsModel : BaseQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets the role names which should be matched when filtering.
        /// </summary>
        public string[] Names { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only return roles which are assignable.
        /// If set to true:
        /// - The customer role will not be included
        /// - If the organisation is not the default, the Tenant Admin role will not be included
        /// - If the performing user is not a Tenant Admin, the Tenant Admin role will not be included
        /// - If the performing user is not a Tenant Admin or Organisation Admin, the Organisation Admin role
        /// will not be included.
        /// </summary>
        public bool? Assignable { get; set; }

        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id or alias.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <returns>Read model filters.</returns>
        public async Task<RoleReadModelFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver)
        {
            await this.ConvertContextToGuid(contextTenantId, cachingResolver);
            var filter = new RoleReadModelFilters()
            {
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                TenantId = this.TenantId,
                OrganisationIds = this.OrganisationId != null ? new List<Guid> { this.OrganisationId.Value } : null,
                Page = this.Page,
                PageSize = this.PageSize,
                Names = this.Names ?? Array.Empty<string>(),
                SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : nameof(Role.CreatedTicksSinceEpoch),
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
