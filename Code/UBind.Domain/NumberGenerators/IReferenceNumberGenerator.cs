// <copyright file="IReferenceNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using System;

    /// <summary>
    /// For generating reference numbers from a seed.
    /// </summary>
    public interface IReferenceNumberGenerator
    {
        /// <summary>
        /// Generate a unique reference number.
        /// </summary>
        /// <returns>A new reference number.</returns>
        string Generate();

        /// <summary>
        /// Sets the internal class properties related to the quote generation.
        /// </summary>
        /// <param name="tenantId">The tenant Id relevant to the quote generating the reference number.</param>
        /// <param name="productId">The products id of which the reference number is being generated for.</param>
        /// <param name="env">The product version environment of which the reference number is being generated for.</param>
        void SetProperties(Guid tenantId, Guid productId, DeploymentEnvironment env);
    }
}
