// <copyright file="QuoteQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
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
    using UBind.Domain.Search;

    /// <summary>
    /// Resource model for binding query filters to a QueryOptions instance.
    /// </summary>
    public class QuoteQueryOptionsModel : BaseQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets quote type filters for filtering result sets.
        /// </summary>
        [FromQuery(Name = "quoteTypes")]
        public IEnumerable<string> QuoteTypes { get; set; }

        /// <summary>
        /// Gets or sets product Ids filters for filtering result sets.
        /// </summary>
        [FromQuery(Name = "productIds")]
        public IEnumerable<Guid> ProductIds { get; set; }

        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <returns>Read model filters.</returns>
        public async Task<QuoteReadModelFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver)
        {
            await this.ConvertContextToGuid(contextTenantId, cachingResolver);
            var filter = new QuoteReadModelFilters
            {
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                Sources = this.Sources,
                Statuses = this.Statuses ?? Enumerable.Empty<string>(),
                QuoteTypes = this.QuoteTypes ?? Enumerable.Empty<string>(),
                ProductIds = this.ProductIds ?? Enumerable.Empty<Guid>(),
                IncludeTestData = this.IncludeTestData,
                PolicyNumber = this.PolicyNumber,
                ProductId = this.ProductId,
                TenantId = this.TenantId,
                OrganisationIds = this.OrganisationId != null ? new List<Guid> { this.OrganisationId.Value } : null,
                Page = this.Page,
                PageSize = this.PageSize,
                Environment = EnvironmentHelper.ParseOptionalEnvironmentOrThrow(this.Environment),
                OwnerUserId = this.OwnerUserId,
                IsDiscarded = false,
                CustomerId = this.CustomerId,
                SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : nameof(IQuoteSearchResultItemReadModel.LastModifiedTicksSinceEpoch),
                SortOrder = !string.IsNullOrEmpty(this.SortOrder.ToString())
                            ? (SortDirection)Enum.Parse(typeof(SortDirection), this.SortOrder.ToString())
                            : SortDirection.Descending,
                ExcludedStatuses = this.ExcludedStatuses ?? Enumerable.Empty<string>(),
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
