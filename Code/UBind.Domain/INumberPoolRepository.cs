// <copyright file="INumberPoolRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Product;

    /// <summary>
    /// interface for managing reference numbers.
    /// </summary>
    public interface INumberPoolRepository
    {
        /// <summary>
        /// Gets the Prefix for seeding purposes.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Seed default numbers for a product.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the product belongs to.</param>
        /// <param name="productId">The ID of the product.</param>
        /// <param name="environment">The deployemnt environment to seed number in.</param>
        void Seed(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets a unique number for a given product, if available.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the  is for.</param>
        /// <param name="productId">The ID of the product the  is for.</param>
        /// <param name="environment">The deployment environment the number is for.</param>
        /// <returns>A reference number.</returns>
        string? ConsumeForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets a unique number for a given product context, if available, and save immediately to reserve the number.
        /// </summary>
        /// <param name="productContext">The productContext.</param>
        /// <returns>A reference number.</returns>
        string ConsumeAndSave(IProductContext productContext);

        /// <summary>
        /// unconsumes a number taken from the pool, does not save to database.
        /// </summary>
        /// <param name="productContext">The productContext.</param>
        /// <param name="number">The reference number.</param>
        void Unconsume(IProductContext productContext, string number);

        /// <summary>
        /// unconsumes a number taken from the pool, and save immediately to release the number for future use.
        /// </summary>
        /// <param name="productContext">The productContext.</param>
        /// <param name="number">The reference number.</param>
        void UnconsumeAndSave(IProductContext productContext, string number);

        /// <summary>
        /// Loads the number service with a new batch of numbers for a given product and
        /// deployment environment.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the numbers are for.</param>
        /// <param name="productId">The ID of the product the numbers are for.</param>
        /// <param name="environment">The deployment environment the numbers are for.</param>
        /// <param name="numbers">The number/s.</param>
        /// <returns>Resource NumberLoad Result.</returns>
        NumberPoolAddResult LoadForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment, IEnumerable<string> numbers);

        /// <summary>
        /// Delete unassigned numbers for a given product and deployment environment.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the numbers are for.</param>
        /// <param name="productId">The ID of the product the numbers are for.</param>
        /// <param name="environment">The deployment environment the numbers are for.</param>
        /// <param name="numbers">The numbers to delete.</param>
        /// <returns>List of Deleted Invoice Numbers.</returns>
        IReadOnlyList<string> DeleteForProduct(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            IEnumerable<string> numbers);

        /// <summary>
        /// Purge assigned and unassigned numbers for a given product and deployment environment.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the numbers are to be purged from.</param>
        /// <param name="productId">The ID of the product the numbers are to be purged from.</param>
        /// <param name="environment">The deployment environment the numbers are to be purged from.</param>
        void PurgeForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets all available numbers for a given product and environment.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the number/s are for.</param>
        /// <param name="productId">The ID of the product the number/s are for.</param>
        /// <param name="environment">The deployment environment the number/s are for.</param>
        /// <returns>A collection of available numbers for asisgnment.</returns>
        IReadOnlyList<string> GetAvailableForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets the count of all available reference numbers.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the number/s are for.</param>
        /// <param name="productId">The ID of the product the number/s are for.</param>
        /// <param name="environment">The deployment environment the number/s are for.</param>
        /// <returns>The count of available reference numbers.</returns>
        int GetAvailableReferenceNumbersCount(Guid tenantId, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Gets all numbers for a given product and environment.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the number/s are for.</param>
        /// <param name="productId">The ID of the product the number/s are for.</param>
        /// <param name="environment">The deployment environment the number/s are for.</param>
        /// <returns>A collection of available numbers for asisgnment.</returns>
        IReadOnlyList<string> GetAllForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment);
    }
}
