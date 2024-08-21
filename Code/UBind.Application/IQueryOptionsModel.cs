// <copyright file="IQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Resource model for binding query filters to a QueryOptions instance.
    /// </summary>
    public interface IQueryOptionsModel
    {
        /// <summary>
        /// Gets or sets search keywords for filtering result sets.
        /// </summary>
        IEnumerable<string> SearchTerms { get; set; }

        /// <summary>
        /// Gets or sets the name of the date field which DateIsAfterTicks and DateIsBeforeTicks relates to.
        /// </summary>
        string DateFilteringPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the after datetime value to be used in filtering.
        /// </summary>
        string AfterDateTime { get; set; }

        /// <summary>
        /// Gets or sets the before datetime value to be used in filtering.
        /// </summary>
        string BeforeDateTime { get; set; }

        /// <summary>
        /// Gets or sets status filters for filtering result sets.
        /// </summary>
        IEnumerable<string> Statuses { get; set; }

        /// <summary>
        /// Gets or sets statuses that will be excluded in filtering result sets.
        /// </summary>
        public IEnumerable<string> ExcludedStatuses { get; set; }

        /// <summary>
        /// Gets or sets status status for filtering result sets.
        /// </summary>
        IEnumerable<string> Sources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include test data.
        /// </summary>
        bool IncludeTestData { get; set; }

        /// <summary>
        /// Gets or sets policyNumber filters for filtering result sets.
        /// </summary>
        string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets policyId filters for filtering result sets.
        /// </summary>
        Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets product Id or Alias filters for filtering result sets.
        /// </summary>
        string Product { get; set; }

        /// <summary>
        /// Gets or sets the page for pagination.
        /// </summary>
        int? Page { get; set; }

        /// <summary>
        /// Gets or sets the pageSize for pagination.
        /// </summary>
        int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets tenant Id or Alias filter for filtering result sets.
        /// </summary>
        string Tenant { get; set; }

        /// <summary>
        /// Gets or sets organisation Id or Alias filter for filtering result sets.
        /// </summary>
        string? Organisation { get; set; }

        /// <summary>
        /// Gets or sets tenantId filter for filtering result sets.
        /// </summary>
        string Environment { get; set; }

        /// <summary>
        /// Gets or sets the user id of the owner for filtering result sets.
        /// </summary>
        Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the customer when filtering.
        /// </summary>
        Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the field in which the entity will be sorted by.
        /// </summary>
        Domain.EntityType? EntityType { get; set; }

        /// <summary>
        /// Gets or sets the field in which the entity will be sorted by.
        /// </summary>
        Guid? EntityId { get; set; }
    }
}
