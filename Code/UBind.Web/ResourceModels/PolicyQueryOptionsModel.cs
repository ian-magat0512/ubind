// <copyright file="PolicyQueryOptionsModel.cs" company="uBind">
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
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// Resource model for binding query filters to a QueryOptions instance.
    /// </summary>
    public class PolicyQueryOptionsModel : BaseQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets product Ids filters for filtering result sets.
        /// </summary>
        [FromQuery(Name = "productIds")]
        public IEnumerable<Guid> ProductIds { get; set; }

        /// <summary>
        /// Gets or sets the type of quotes for filtering result sets.
        /// </summary>
        [FromQuery(Name = "quoteTypes")]
        public IEnumerable<string> QuoteTypes { get; set; }

        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <returns>The filtered organisation read models.</returns>r
        public async Task<PolicyReadModelFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver)
        {
            await this.ConvertContextToGuid(contextTenantId, cachingResolver);

            var filter = new PolicyReadModelFilters()
            {
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                Sources = this.Sources,
                ProductIds = this.ProductIds ?? Enumerable.Empty<Guid>(),
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
                CustomerId = this.CustomerId,
                IncludeProductFeatureSetting = this.IncludeProductFeatureSetting,
                QuoteTypes = this.QuoteTypes ?? Enumerable.Empty<string>(),
                SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : nameof(PolicyReadModel.CreatedTicksSinceEpoch),
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
