// <copyright file="BackgroundJobQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Filters;

    /// <summary>
    /// Model representing the filtering options when querying for background jobs.
    /// </summary>
    public class BackgroundJobQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether jobs will be filter by environment.
        /// </summary>
        public bool FilterEnvironment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether jobs will be filter by tenant.
        /// </summary>
        public bool FilterTenant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether jobs will be filter by product.
        /// </summary>
        public bool FilterProduct { get; set; }

        /// <summary>
        /// Gets or sets the environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether job is acknowledged or not.
        /// </summary>
        public bool? IsAcknowledged { get; set; }

        /// <summary>
        /// Gets or sets the tenant alias.
        /// </summary>
        public string TenantAlias { get; set; }

        /// <summary>
        ///  Gets or sets the product alias.
        /// </summary>
        public string ProductAlias { get; set; }

        /// <summary>
        /// Create filters from these options.
        /// </summary>
        /// <param name="tenantId">tenant id.</param>
        /// <param name="productId">product id.</param>
        /// <returns>filter.</returns>
        public BackgroundJobFilter ToFilters(Guid tenantId, Guid productId)
        {
            return new BackgroundJobFilter
            {
                FilterEnvironment = this.FilterEnvironment,
                FilterTenant = this.FilterTenant,
                FilterProduct = this.FilterProduct,
                Environment = this.Environment,
                TenantId = tenantId,
                ProductId = productId,
            };
        }
    }
}
