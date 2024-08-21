// <copyright file="IProductSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using NodaTime;
    using UBind.Domain.Product;

    /// <summary>
    /// Data transfer object for quotereadmodel
    /// for a specific product.
    /// </summary>
    public interface IProductSummary : IProduct
    {
        /// <summary>
        /// Gets or sets the entity's unique identifier.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets the instant in time the entity was created.
        /// </summary>
        Instant CreatedTimestamp
        {
            get;
        }

        /// <summary>
        /// Gets or sets the entity created time (in ticks since Epoch).
        /// </summary>
        /// <remarks> Primitive typed property for EF to store created time.</remarks>
        long CreatedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets tenant name.
        /// </summary>
        string TenantName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product's tenant is disabled.
        /// </summary>
        bool TenantDisabled { get; set; }

        /// <summary>
        /// Gets the product details.
        /// </summary>
        ProductDetails Details { get; }

        /// <summary>
        /// Gets all the details versions with most recent first.
        /// </summary>
        IEnumerable<ProductDetails> History
        {
            get;
        }

        /// <summary>
        /// Gets or sets historic product details.
        /// </summary>
        /// <remarks>
        /// Required for EF to persist all historic and current details (unordered).
        /// .</remarks>
        Collection<ProductDetails> DetailsCollection { get; set; }
    }
}
