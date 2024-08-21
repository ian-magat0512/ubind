// <copyright file="IUniqueIdentifierService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for pre-loading and consuming unique identifiers.
    /// </summary>
    public interface IUniqueIdentifierService
    {
        /// <summary>
        /// Consume a unique identifier of a given type for a given product under a given tenant in a given environment, if available.
        /// </summary>
        /// <param name="type">The type of identifier.</param>
        /// <param name="tenant">The tenant the quote numbers are for.</param>
        /// <param name="productId">The ID of the product the quote is for.</param>
        /// <param name="environment">The deployment environment the quote number is for.</param>
        /// <returns>The unique identifer that has been consumed.</returns>
        Task<string> ConsumeUniqueIdentifier(IdentifierType type, Tenant tenant, Guid productId, DeploymentEnvironment environment);

        /// <summary>
        /// Loads the a new batch of unique identifiers of a given type for a given product under a given tenant in a given environment.
        /// </summary>
        /// <param name="type">The type of identifier.</param>
        /// <param name="tenant">The tenant the quote numbers are for.</param>
        /// <param name="product">The product the quote numbers are for.</param>
        /// <param name="environment">The deployment environment the quote numbers are for.</param>
        /// <param name="quoteNumbers">The quote number/s.</param>
        void LoadUniqueIdentifiers(
            IdentifierType type,
            Tenant tenant,
            UBind.Domain.Product.Product product,
            DeploymentEnvironment environment,
            IEnumerable<string> quoteNumbers);

        /// <summary>
        /// Gets all available identifiers of a given type for a given product under a given tenant and environment.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the quote number/s are for.</param>
        /// <param name="productId">The ID of the product the quote number/s are for.</param>
        /// <param name="environment">The deployment environment the quote number/s are for.</param>
        /// <param name="type">The type of identifier.</param>
        /// <returns>A collection of available quote numbers for asisgnment.</returns>
        IEnumerable<string> GetAvailableUniqueIdentifiers(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, IdentifierType type);
    }
}
