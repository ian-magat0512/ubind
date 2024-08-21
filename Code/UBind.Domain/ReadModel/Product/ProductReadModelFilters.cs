// <copyright file="ProductReadModelFilters.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Product
{
    using System.Collections.Generic;
    using UBind.Domain.Enums;
    using UBind.Domain.Product;

    public class ProductReadModelFilters : EntityListFilters
    {
        public ProductReadModelFilters()
        {
            this.SortBy = nameof(Product.CreatedTicksSinceEpoch);
            this.SortOrder = SortDirection.Descending;
        }

        /// <summary>
        /// Gets or sets a list of feature settings that would need to be enabled for the product to be included in
        /// the set of returned products.
        /// </summary>
        public IEnumerable<ProductFeatureSettingItem> HasFeatureSettingsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a list of product component types that would need to be available in the given environment
        /// for the product to be included in the set of returned products.
        /// </summary>
        public IEnumerable<WebFormAppType> HasComponentTypes { get; set; }
    }
}
