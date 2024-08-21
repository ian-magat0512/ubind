// <copyright file="GetAssignableRolesMatchingFiltersQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Role
{
    using System.Collections.Generic;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Gets roles which the performing user can assign to someone:
    /// - The customer role will not be included
    /// - If the organisation is not the default, the Tenant Admin role will not be included
    /// - If the performing user is not a Tenant Admin, the Tenant Admin role will not be included
    /// - If the performing user is not a Tenant Admin or Organisation Admin, the Organisation Admin role
    /// will not be included.
    /// </summary>
    public class GetAssignableRolesMatchingFiltersQuery : IQuery<IReadOnlyList<Role>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetAssignableRolesMatchingFiltersQuery"/> class.
        /// </summary>
        /// <param name="filters">The filtering options.</param>
        public GetAssignableRolesMatchingFiltersQuery(RoleReadModelFilters filters)
        {
            this.Filters = filters;
        }

        /// <summary>
        /// Gets the filtering options.
        /// </summary>
        public RoleReadModelFilters Filters { get; }
    }
}
