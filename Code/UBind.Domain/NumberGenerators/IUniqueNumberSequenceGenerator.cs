// <copyright file="IUniqueNumberSequenceGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using System;

    /// <summary>
    /// Service for generating a unique sequence of contiguous integers for a given tenant, product, environment, use case and method.
    /// </summary>
    public interface IUniqueNumberSequenceGenerator
    {
        /// <summary>
        /// Generate the next integer in a unique sequence of contiguous integers for a given tenant, product, environment, use case and method.
        /// </summary>
        /// <param name="tenantId">The tenant id the sequence is for.</param>
        /// <param name="productId">The product id the sequence is for.</param>
        /// <param name="environment">The environment the sequence is for.</param>
        /// <param name="useCase">The use case the sequence is for.</param>
        /// <returns>The next sequential integer in the sequence generated for that product and environment.</returns>
        int Next(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            UniqueNumberUseCase useCase);

        /// <summary>
        /// Generate the next integer in a unique sequence of contiguous integers for a given tenant, environment, use case and method.\
        /// Uses default product.
        /// </summary>
        /// <param name="tenantId">The tenant the sequence is for.</param>
        /// <param name="environment">The environment the sequence is for.</param>
        /// <param name="useCase">The use case the sequence is for.</param>
        /// <returns>The next sequential integer in the sequence generated for that product and environment.</returns>
        int Next(
            Guid tenantId,
            DeploymentEnvironment environment,
            UniqueNumberUseCase useCase);
    }
}
