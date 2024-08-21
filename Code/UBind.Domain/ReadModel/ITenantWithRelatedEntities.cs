// <copyright file="ITenantWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.ReadModel.WithRelatedEntities;

    /// <summary>
    /// Interface for tenant and its related entities.
    /// </summary>
    public interface ITenantWithRelatedEntities : IEntityReadModelWithRelatedEntities, IEntitySupportingAdditionalProperties
    {
        /// <summary>
        /// Gets or sets list of products of the tenant.
        /// </summary>
        List<Domain.Product.Product> Products { get; set; }

        /// <summary>
        /// Gets or sets the tenant.
        /// </summary>
        Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the tenant details.
        /// </summary>
        TenantDetails Details { get; set; }

        /// <summary>
        /// Gets or sets the organisation.
        /// </summary>
        OrganisationReadModel DefaultOrganisation { get; set; }
    }
}
