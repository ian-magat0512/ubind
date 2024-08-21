// <copyright file="OrganisationReadModelFilters.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using UBind.Domain.Enums;

    /// <summary>
    /// Represents filters to apply when querying read models for Organisation.
    /// </summary>
    public class OrganisationReadModelFilters : EntityListFilters
    {
        public OrganisationReadModelFilters()
        {
            this.SortBy = nameof(OrganisationReadModel.Name);
            this.SortOrder = SortDirection.Descending;
        }

        /// <summary>
        /// Gets or sets the organisation names which should be matched when filtering.
        /// </summary>
        public string[] Names { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the organisation aliases which should be matched when filtering.
        /// </summary>
        public string[] Aliases { get; set; } = Array.Empty<string>();

        public Guid? ManagingOrganisationId { get; set; }
    }
}
