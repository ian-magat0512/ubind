// <copyright file="IProductWithRelatedEntities.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.WithRelatedEntities;

    /// <summary>
    /// Interface for product and its related entities.
    /// </summary>
    public interface IProductWithRelatedEntities : IEntityReadModelWithRelatedEntities, IEntitySupportingAdditionalProperties
    {
        /// <summary>
        /// Gets or sets the product.
        /// </summary>
        UBind.Domain.Product.Product Product { get; set; }

        /// <summary>
        /// Gets or sets the product details.
        /// </summary>
        ProductDetails Details { get; set; }

        /// <summary>
        /// Gets or sets the tenant.
        /// </summary>
        Tenant Tenant { get; set; }

        /// <summary>
        /// Gets or sets the tenant details.
        /// </summary>
        TenantDetails TenantDetails { get; set; }
    }
}
