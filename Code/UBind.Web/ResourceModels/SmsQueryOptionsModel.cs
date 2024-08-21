// <copyright file="SmsQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Helpers;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;

    public class SmsQueryOptionsModel : QueryOptionsModel
    {
        /// <summary>
        /// Gets or sets the filter for any tags provided that can be related to any entity.
        /// </summary>
        [FromQuery(Name = "tags")]
        public string[] Tags { get; set; }

        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver to resolve tenant/product/organisation.</param>
        /// <param name="defaultSortBy">The default sortBy per entity.</param>
        /// <returns>Read model filters.</returns>
        public override async Task<EntityListFilters> ToFilters(Guid contextTenantId, ICachingResolver cachingResolver, string defaultSortBy)
        {
            if (!string.IsNullOrEmpty(this.SortBy))
            {
                this.SortBy = $"{nameof(Sms)}.{this.SortBy}";
            }

            await this.ConvertContextToGuid(contextTenantId, cachingResolver);
            var filters = new EntityListFilters
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
                Tags = this.Tags,
                EntityId = this.EntityId ?? Guid.Empty,
                EntityType = this.EntityType,
                SortBy = !string.IsNullOrEmpty(this.SortBy) ? this.SortBy : defaultSortBy,
                SortOrder = !string.IsNullOrEmpty(this.SortOrder.ToString())
                            ? (SortDirection)Enum.Parse(typeof(SortDirection), this.SortOrder.ToString())
                            : SortDirection.Descending,
            };

            if (!string.IsNullOrEmpty(this.AfterDateTime) && !string.IsNullOrEmpty(this.DateFilteringPropertyName))
            {
                filters.DateFilteringPropertyName = this.DateFilteringPropertyName;
                filters.WithDateIsAfterFilter(this.AfterDateTime.ToLocalDateFromIso8601());
            }

            if (!string.IsNullOrEmpty(this.BeforeDateTime) && !string.IsNullOrEmpty(this.DateFilteringPropertyName))
            {
                filters.DateFilteringPropertyName = this.DateFilteringPropertyName;
                filters.WithDateIsBeforeFilter(this.BeforeDateTime.ToLocalDateFromIso8601());
            }

            return filters;
        }
    }
}
