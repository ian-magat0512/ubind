// <copyright file="INumberPoolService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides operations for manipulating reference numbers.
    /// </summary>
    public interface INumberPoolService
    {
        /// <summary>
        /// Adds numbers to a pool of numbers.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="numberPoolId">The number pool ID.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="numbers">The numbers to add.</param>
        /// <returns>A result showing which numbers were added and which, if any were already there.</returns>
        NumberPoolAddResult Add(
            Guid tenantId,
            Guid productId,
            string numberPoolId,
            DeploymentEnvironment environment,
            IEnumerable<string> numbers);

        /// <summary>
        /// Removes numbers from a pool of numbers.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="numberPoolId">The number pool ID.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="numbers">The numbers to remove.</param>
        /// <returns>A list of the numbers removed.</returns>
        IReadOnlyList<string> Remove(
            Guid tenantId,
            Guid productId,
            string numberPoolId,
            DeploymentEnvironment environment,
            IEnumerable<string> numbers);

        /// <summary>
        /// Returns a list of the available numbers for a pool of numbers.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="numberPoolId">The number pool ID.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>The available numbers.</returns>
        IReadOnlyList<string> GetAvailable(
            Guid tenantId,
            Guid productId,
            string numberPoolId,
            DeploymentEnvironment environment);

        /// <summary>
        /// Returns a list of all numbers for a pool of numbers, including those which have been allocated.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="productId">The product ID.</param>
        /// <param name="numberPoolId">The number pool ID.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>The numbers.</returns>
        IReadOnlyList<string> GetAll(
            Guid tenantId,
            Guid productId,
            string numberPoolId,
            DeploymentEnvironment environment);
    }
}
