// <copyright file="IEntityListFilters.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents filters to apply when querying read models.
    /// </summary>
    public interface IEntityListFilters
    {
        /// <summary>
        /// Gets or sets search keywords for filtering result sets.
        /// </summary>
        IEnumerable<string> SearchTerms { get; set; }

        /// <summary>
        /// Gets or sets the Tenant Id to filter for.
        /// </summary>
        Guid? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Environment to filter for.
        /// </summary>
        DeploymentEnvironment? Environment { get; set; }

        /// <summary>
        /// Gets or sets the page for pagination.
        /// </summary>
        int? Page { get; set; }

        /// <summary>
        /// Gets or sets the page for pagination.
        /// </summary>
        int? PageSize { get; set; }
    }
}
