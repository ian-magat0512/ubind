// <copyright file="DataTableDefinitionQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using UBind.Application.Helpers;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Filtering options when querying for Data Table Definition.
    /// </summary>
    public class DataTableDefinitionQueryOptionsModel : BaseQueryOptionsModel
    {
        /// <summary>
        /// Convert into a DataTableDefinitionFilters object.
        /// </summary>
        public EntityListFilters ToFilters(Guid tenantId, string defaultSortBy)
        {
            var filter = new EntityListFilters
            {
                TenantId = tenantId,
                EntityType = this.EntityType,
                EntityId = this.EntityId,
                SearchTerms = this.SearchTerms ?? Enumerable.Empty<string>(),
                Page = this.Page,
                PageSize = this.PageSize,
                Environment = EnvironmentHelper.ParseOptionalEnvironmentOrThrow(this.Environment),
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
