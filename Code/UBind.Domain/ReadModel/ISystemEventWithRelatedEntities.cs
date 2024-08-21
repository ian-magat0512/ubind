// <copyright file="ISystemEventWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain.Events;
    using UBind.Domain.Product;

    /// <summary>
    /// Interface for system event and its related entities.
    /// </summary>
    public interface ISystemEventWithRelatedEntities : IEntityWithRelatedEntities
    {
        /// <summary>
        /// Gets or sets the tenant.
        /// </summary>
        Tenant? Tenant { get; set; }

        /// <summary>
        /// Gets or sets the list of tenant details.
        /// </summary>
        IEnumerable<TenantDetails>? TenantDetails { get; set; }

        /// <summary>
        /// Gets or sets the organisation.
        /// </summary>
        OrganisationReadModel? Organisation { get; set; }

        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        public UBind.Domain.Product.Product? Product { get; set; }

        /// <summary>
        /// Gets or sets the product details.
        /// </summary>
        public ProductDetails? ProductDetails { get; set; }

        /// <summary>
        /// Gets or sets the event.
        /// </summary>
        SystemEvent? SystemEvent { get; set; }

        /// <summary>
        /// Gets or sets the source relationships associated with the event.
        /// </summary>
        IEnumerable<ReadWriteModel.Relationship>? FromRelationships { get; set; }

        /// <summary>
        /// Gets or sets the target relationships associated with the event.
        /// </summary>
        IEnumerable<ReadWriteModel.Relationship>? ToRelationships { get; set; }
    }
}
