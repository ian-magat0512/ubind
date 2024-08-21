// <copyright file="IOrganisationReadModelWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.ReadModel.WithRelatedEntities;

    /// <summary>
    /// Interface for organisation and its related entities.
    /// </summary>
    public interface IOrganisationReadModelWithRelatedEntities : IEntityReadModelWithRelatedEntities, IEntitySupportingAdditionalProperties
    {
        /// <summary>
        /// Gets or sets the organisation.
        /// </summary>
        OrganisationReadModel Organisation { get; set; }

        /// <summary>
        /// Gets or sets the tenant.
        /// </summary>
        Tenant? Tenant { get; set; }

        /// <summary>
        /// Gets or sets the list of tenant details.
        /// </summary>
        IEnumerable<TenantDetails> TenantDetails { get; set; }
    }
}
