// <copyright file="IProductCompatibilityService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using UBind.Domain.Upgrade;

    /// <summary>
    /// Service for handling backward compatibility issues with pre-release 2.1 production products.
    /// </summary>
    public interface IProductCompatibilityService
    {
        /// <summary>
        /// Checks the tenant and product IDs for the pre-release 2.1 product ID passed.
        /// </summary>
        /// <param name="productId">The previous ID for the product.</param>
        /// <returns>The updated product.</returns>
        ProductMigrationMapping GetCompatibleProductMapping(string productId);
    }
}
