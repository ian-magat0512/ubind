// <copyright file="UserReadModelFilters.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Represents filters to apply when querying read models.
    /// </summary>
    public class UserReadModelFilters : EntityListFilters
    {
        public UserReadModelFilters()
        {
            this.SortBy = nameof(UserReadModel.CreatedTicksSinceEpoch);
            this.SortOrder = SortDirection.Descending;
        }

        /// <summary>
        /// Gets or sets the user type names which should be matched when filtering.
        /// </summary>
        public string[] UserTypes { get; set; }

        /// <summary>
        /// Gets or sets the role names which the user should have at least one of, to be considered a match.
        /// </summary>
        public string[] RoleNames { get; set; }
    }
}
