// <copyright file="SystemEventWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.ReadModel
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Product;

    /// <summary>
    /// This class is needed because we need a data transfer object for system event and its related entities.
    /// </summary>
    public class SystemEventWithRelatedEntities : ISystemEventWithRelatedEntities
    {
        /// <inheritdoc/>
        public Tenant? Tenant { get; set; }

        /// <inheritdoc/>
        public IEnumerable<TenantDetails>? TenantDetails { get; set; }

        /// <inheritdoc/>
        public OrganisationReadModel? Organisation { get; set; }

        /// <inheritdoc/>
        public UBind.Domain.Product.Product? Product { get; set; }

        /// <inheritdoc/>
        public ProductDetails? ProductDetails { get; set; }

        /// <inheritdoc/>
        public SystemEvent? SystemEvent { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Relationship>? FromRelationships { get; set; }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Relationship>? ToRelationships { get; set; }
    }
}
