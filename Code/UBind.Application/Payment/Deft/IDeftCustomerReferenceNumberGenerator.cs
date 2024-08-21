// <copyright file="IDeftCustomerReferenceNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Service for generating customer reference numbers for use in DEFT transactions.
    /// </summary>
    public interface IDeftCustomerReferenceNumberGenerator
    {
        /// <summary>
        /// Generate a new unique customer reference number for use in a DEFT transaction.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the transaction is for.</param>
        /// <param name="productId">The ID of the product the transaction is for.</param>
        /// <param name="environment">The environment the transaction is for.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <returns>A CRN that is unique within the context specified.</returns>
        string GenerateDeftCrnNumber(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            CrnGenerationConfiguration configuration);
    }
}
