// <copyright file="BackgroundJobFilter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Filters
{
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Represents filters to apply when querying Background Job.
    /// </summary>
    public class BackgroundJobFilter : EntityListFilters
    {
        /// <summary>
        /// Gets or sets a value indicating whether Jobs will be filter by Environment.
        /// </summary>
        public bool FilterEnvironment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Jobs will be filter by Tenant.
        /// </summary>
        public bool FilterTenant { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether jobs will be filter by product.
        /// </summary>
        public bool FilterProduct { get; set; }
    }
}
