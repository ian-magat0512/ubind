// <copyright file="IOrganisationReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;

    /// <summary>
    /// Data transfer object for organisation read model summary.
    /// </summary>
    public interface IOrganisationReadModelSummary
    {
        /// <summary>
        /// Gets or sets the organisation's Id.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the tenant id of the organisation.
        /// </summary>
        Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the alias of the organisation.
        /// </summary>
        string Alias { get; set; }

        /// <summary>
        /// Gets or sets the name of the organisation.
        /// </summary>
        string Name { get; set; }

        Guid? ManagingOrganisationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the organisation is active or disabled.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the organisation is marked as deleted.
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the organisation is the default or not.
        /// </summary>
        bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the organisation's created time.
        /// </summary>
        Instant CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the organisation's last modified time.
        /// </summary>
        Instant LastModifiedTimestamp { get; set; }
    }
}
