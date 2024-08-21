// <copyright file="BaseQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using UBind.Application;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using Core = System.Text.Json.Serialization;

    /// <summary>
    /// Resource model for binding query filters to a QueryOptions instance.
    /// </summary>
    public class BaseQueryOptionsModel : IQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets search keywords for filtering result sets.
        /// </summary>
        [FromQuery(Name = "search")]
        public IEnumerable<string> SearchTerms { get; set; }

        /// <summary>
        /// Gets or sets the name of the date field which DateIsAfterTicks and DateIsBeforeTicks relates to.
        /// </summary>
        [FromQuery(Name = "dateFilteringPropertyName")]
        public string DateFilteringPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the date after which the object should have the filter by date.
        /// </summary>
        [RegularExpression(@"\d\d\d\d-\d\d-\d\d", ErrorMessage = "AfterDateTime must be in format YYYY-MM-DD")]
        public string AfterDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date before which the object should have the filter by date.
        /// </summary>
        [RegularExpression(@"\d\d\d\d-\d\d-\d\d", ErrorMessage = "BeforeDateTime must be in format YYYY-MM-DD")]
        public string BeforeDateTime { get; set; }

        /// <summary>
        /// Gets or sets status filters for filtering result sets.
        /// </summary>
        [FromQuery(Name = "status")]
        public IEnumerable<string> Statuses { get; set; }

        /// <summary>
        /// Gets or sets statuses that will be excluded in filtering result sets.
        /// </summary>
        [FromQuery(Name = "excludedStatuses")]
        public IEnumerable<string> ExcludedStatuses { get; set; }

        /// <summary>
        /// Gets or sets status status for filtering result sets.
        /// </summary>
        [FromQuery(Name = "source")]
        public IEnumerable<string> Sources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include test data.
        /// </summary>
        [FromQuery(Name = "includeTestData")]
        public bool IncludeTestData { get; set; }

        /// <summary>
        /// Gets or sets policyNumber filters for filtering result sets.
        /// </summary>
        [FromQuery(Name = "policyNumber")]
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets policyId filters for filtering result sets.
        /// </summary>
        [FromQuery(Name = "policyId")]
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets Product Id or Alias filters for filtering result sets.
        /// </summary>
        [FromQuery(Name = "product")]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the page for pagination.
        /// </summary>
        [FromQuery(Name = "page")]
        public int? Page { get; set; }

        /// <summary>
        /// Gets or sets the pageSize for pagination.
        /// </summary>
        [FromQuery(Name = "pageSize")]
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets tenant Id or Alias filter for filtering result sets.
        /// </summary>
        [FromQuery(Name = "tenant")]
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets organisation Id or Alias filter for filtering result sets.
        /// </summary>
        [FromQuery(Name = "organisation")]
        public string Organisation { get; set; }

        /// <summary>
        /// Gets or sets tenantId filter for filtering result sets.
        /// </summary>
        [FromQuery(Name = "environment")]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the user id of the owner for filtering result sets.
        /// </summary>
        [FromQuery(Name = "ownerUserId")]
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the customer when filtering.
        /// </summary>
        [FromQuery(Name = "customerId")]
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether product feature setting will be included.
        /// </summary>
        [FromQuery(Name = "IncludeProductFeatureSetting")]
        public bool IncludeProductFeatureSetting { get; set; }

        /// <summary>
        /// Gets or sets the portal alias for filtering the result sets.
        /// </summary>
        [FromQuery(Name = "portalAlias")]
        public string PortalAlias { get; set; }

        /// <summary>
        /// Gets or sets the field in which the entity will be sorted by.
        /// </summary>
        [FromQuery(Name = "entityType")]
        public EntityType? EntityType { get; set; }

        /// <summary>
        /// Gets or sets the field in which the entity will be sorted by.
        /// </summary>
        [FromQuery(Name = "entityId")]
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the field in which the entity will be sorted by.
        /// </summary>
        [FromQuery(Name = "sortBy")]
        public string SortBy { get; set; }

        /// <summary>
        /// Gets or sets the order in which the selected entity field will be sorted.
        /// </summary>
        [FromQuery(Name = "sortOrder")]
        public SortDirection? SortOrder { get; set; }

        /// <summary>
        /// Gets product Id from the input 'Product'.
        /// </summary>
        [JsonIgnore]
        [Core.JsonIgnore]
        public Guid? ProductId { get; private set; }

        /// <summary>
        /// Gets tenant Id from the input 'Tenant'.
        /// </summary>
        [JsonIgnore]
        [Core.JsonIgnore]
        public Guid? TenantId { get; private set; }

        /// <summary>
        /// Gets organisation Id from the input 'Organisation'.
        /// </summary>
        [JsonIgnore]
        [Core.JsonIgnore]
        public Guid? OrganisationId { get; private set; }

        /// <summary>
        /// Converts the input from Product/Tenant/Organisation to its Guid Ids.
        /// </summary>
        /// <param name="contextTenantId">The context tenant Id.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        /// <returns>Return a task.</returns>
        public async Task ConvertContextToGuid(Guid contextTenantId, ICachingResolver cachingResolver)
        {
            if (!this.Tenant.IsNullOrEmpty())
            {
                var tenant = await cachingResolver.GetTenantOrNull(new GuidOrAlias(this.Tenant));
                this.TenantId = tenant?.Id;
            }

            var tenantId = this.TenantId ?? contextTenantId;
            this.ProductId = new GuidOrAlias(this.Product).Guid;
            if (this.ProductId == null)
            {
                var product = await cachingResolver.GetProductOrNull(tenantId, new GuidOrAlias(this.Product));
                this.ProductId = product?.Id;
            }

            this.OrganisationId = new GuidOrAlias(this.Organisation).Guid;
            if (this.OrganisationId == null)
            {
                var organisation = await cachingResolver.GetOrganisationOrNull(tenantId, new GuidOrAlias(this.Organisation));
                this.OrganisationId = organisation?.Id;
            }
        }
    }
}
