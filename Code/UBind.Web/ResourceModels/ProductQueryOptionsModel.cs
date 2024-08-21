// <copyright file="ProductQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Product;

    /// <summary>
    /// Model representing the filtering options when querying for Product.
    /// </summary>
    public class ProductQueryOptionsModel : BaseQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets a list of feature settings that would need to be enabled for the product to be included in
        /// the set of returned products.
        /// </summary>
        public IEnumerable<ProductFeatureSettingItem> HasFeatureSettingsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a list of product component types that would need to be available in the given environment
        /// for the product to be included in the set of returned products.
        /// </summary>
        public IEnumerable<WebFormAppType> HasComponentTypes { get; set; }

        public async Task<ProductReadModelFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver, string defaultSortBy)
        {
            await this.ConvertContextToGuid(contextTenantId, cachingResolver);
            var filter = new ProductReadModelFilters
            {
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                Sources = this.Sources,
                Statuses = this.Statuses ?? Enumerable.Empty<string>(),
                IncludeTestData = this.IncludeTestData,
                PolicyNumber = this.PolicyNumber,
                ProductId = this.ProductId,
                TenantId = this.TenantId,
                Page = this.Page,
                PageSize = this.PageSize,
                Environment = EnvironmentHelper.ParseOptionalEnvironmentOrThrow(this.Environment),
                OwnerUserId = this.OwnerUserId,
                HasFeatureSettingsEnabled = this.HasFeatureSettingsEnabled,
                HasComponentTypes = this.HasComponentTypes,
                PortalAlias = this.PortalAlias,
                OrganisationIds = this.OrganisationId != null ? new List<Guid> { this.OrganisationId.Value } : null,
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
