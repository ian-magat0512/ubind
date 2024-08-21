// <copyright file="ProductMigrationMapping.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Upgrade
{
    /// <summary>
    /// Mapping specifying how existing products should be imported into new tenants.
    /// </summary>
    public class ProductMigrationMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductMigrationMapping"/> class.
        /// </summary>
        /// <param name="existingProductId">The ID of the existing product to import.</param>
        /// <param name="newTenantName">The name of the new tenant the product should be imported into.</param>
        /// <param name="newTenantAbbreviation">The abbreviation of the new tenant the product should be imported into.</param>
        /// <param name="newProductName">The new name for the product.</param>
        /// <param name="newProductAbbreviation">The new abbreviation for the product.</param>
        public ProductMigrationMapping(
            string existingProductId,
            string newTenantName,
            string newTenantAbbreviation,
            string newProductName,
            string newProductAbbreviation)
        {
            this.ExistingProductId = existingProductId;
            this.NewTenantName = newTenantName;
            this.NewTenantAbbreviation = newTenantAbbreviation;
            this.NewProductName = newProductName;
            this.NewProductAbbreviation = newProductAbbreviation;
        }

        /// <summary>
        /// Gets the ID of the product to import.
        /// </summary>
        public string ExistingProductId { get; }

        /// <summary>
        /// Gets the name of the new tenant to import the product into.
        /// </summary>
        public string NewTenantName { get; }

        /// <summary>
        /// Gets the abbreviation of the new tenant to import the product into.
        /// </summary>
        public string NewTenantAbbreviation { get; }

        /// <summary>
        /// Gets the new name for the product.
        /// </summary>
        public string NewProductName { get; }

        /// <summary>
        /// Gets the new abbreviation for the product.
        /// </summary>
        public string NewProductAbbreviation { get; }
    }
}
