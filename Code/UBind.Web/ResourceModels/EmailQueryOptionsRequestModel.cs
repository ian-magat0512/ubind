// <copyright file="EmailQueryOptionsRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel.Email;

    /// <summary>
    /// Model representing the filtering options when querying for emails.
    /// </summary>
    public class EmailQueryOptionsRequestModel : QueryOptionsModel
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
                this.SortBy = $"{nameof(Email)}.{this.SortBy}";
            }

            return await base.ToFilters(contextTenantId, cachingResolver, defaultSortBy);
        }
    }
}
