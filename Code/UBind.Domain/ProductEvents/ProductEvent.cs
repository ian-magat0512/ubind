// <copyright file="ProductEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ProductEvents
{
    /// <summary>
    /// Base class for product events.
    /// </summary>
    public abstract class ProductEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductEvent"/> class.
        /// </summary>
        /// <param name="product">The product the event relates to.</param>
        public ProductEvent(Product.Product product)
        {
            this.Product = product;
        }

        /// <summary>
        /// Gets the product the event relates to.
        /// </summary>
        public Product.Product Product { get; }
    }
}
