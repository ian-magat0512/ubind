// <copyright file="OrganisationQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Organisation
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Model representing the filtering options when querying for organisation.
    /// </summary>
    public class OrganisationQueryOptionsModel : BaseQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets the organisation names which should be matched when filtering.
        /// </summary>
        public string[] Names { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the organisation aliases which should be matched when filtering.
        /// </summary>
        public string[] Aliases { get; set; } = Array.Empty<string>();

        public Guid? ManagingOrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the id of the organisation for which all eligible organizations' that can manage it should be filtered.
        /// </summary>
        public Guid? EligibleToManageOrganisationId { get; set; }

        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <returns>The filtered organisation read models.</returns>r
        public async Task<OrganisationReadModelFilters> ToFilters(
            Guid contextTenantId,
            ICachingResolver cachingResolver)
        {
            await this.ConvertContextToGuid(contextTenantId, cachingResolver);
            var filter = new OrganisationReadModelFilters()
            {
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                Statuses = this.Statuses ?? Enumerable.Empty<string>(),
                TenantId = this.TenantId,
                Page = this.Page,
                PageSize = this.PageSize,
                Names = this.Names,
                Aliases = this.Aliases,
                ManagingOrganisationId = this.ManagingOrganisationId,
                SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : nameof(OrganisationReadModel.Name),
                SortOrder = !string.IsNullOrEmpty(this.SortOrder.ToString())
                            ? (SortDirection)Enum.Parse(typeof(SortDirection), this.SortOrder.ToString())
                            : SortDirection.Ascending,
                OrganisationIds = this.OrganisationId != null ? new[] { this.OrganisationId.Value } : Enumerable.Empty<Guid>(),
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
