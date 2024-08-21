// <copyright file="IProduct.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    ///  Interfrace for Product Entity and Product Summary.
    /// </summary>
    public interface IProduct
    {
        /// <summary>
        /// Gets the products Tenant Id.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets or sets product events.
        /// </summary>
        Collection<ProductEvent> Events { get; set; }
    }

    /// <summary>
    /// Extension methods for IProduct.
    /// </summary>
    public static class IProductExtensions
    {
        /// <summary>
        /// Extension method for retrieving product status.
        /// </summary>
        /// <param name="product">The product object.</param>
        /// <returns>The status of the product object.</returns>
        public static string GetStatus(this IProduct product)
        {
            return product.Events.Any(ev => ev.Type == ProductEventType.OneDriveInitialized)
                        ? "Initialised"
                        : product.Events.Any(ev => ev.Type == ProductEventType.OneDriveInitializedFailed)
                            ? "Initialisation Failed"
                            : "Initialising";
        }
    }
}
