// <copyright file="PortalQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Portal
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Filters;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;

    public class PortalQueryOptionsModel : QueryOptionsModel
    {
        public string Portal { get; set; }

        public PortalUserType? UserType { get; set; }

        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <param name="defaultSortBy">The default sortBy per entity.</param>
        /// <returns>Read model filters.</returns>
        public override async Task<EntityListFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver, string defaultSortBy)
        {
            await this.ConvertContextToGuid(contextTenantId, cachingResolver);
            Guid? portalId = new GuidOrAlias(this.Portal).Guid;

            if (portalId == null)
            {
                var portal = await cachingResolver.GetPortalOrNull(this.TenantId ?? contextTenantId, new GuidOrAlias(this.Portal));
                portalId = portal?.Id;
            }

            var filter = new PortalListFilters
            {
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                Sources = this.Sources,
                Statuses = this.Statuses ?? Enumerable.Empty<string>(),
                IncludeTestData = this.IncludeTestData,
                PolicyNumber = this.PolicyNumber,
                ProductId = this.ProductId,
                TenantId = this.TenantId,
                OrganisationIds = this.OrganisationId != null ? new List<Guid> { this.OrganisationId.Value } : null,
                Page = this.Page,
                PageSize = this.PageSize,
                Environment = EnvironmentHelper.ParseOptionalEnvironmentOrThrow(this.Environment),
                OwnerUserId = this.OwnerUserId,
                PortalAlias = this.PortalAlias,
                PortalId = portalId,
                UserType = this.UserType,
                SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : defaultSortBy,
                SortOrder = !string.IsNullOrEmpty(this.SortOrder.ToString())
                            ? (SortDirection)Enum.Parse(typeof(SortDirection), this.SortOrder.ToString())
                            : SortDirection.Ascending,
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
