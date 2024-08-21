// <copyright file="IGenericNumberGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using System;

    /// <summary>
    /// Service for generating unique numbers with or without obfuscation for various purposes.
    /// </summary>
    public interface IGenericNumberGenerator
    {
        /// <summary>
        /// Generates a number according to the given scheme, ensuring uniqueness for that use case.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="environment">The environment.</param>
        /// <returns>The debit note reference number.</returns>
        string Generate(
            Guid tenantId,
            DeploymentEnvironment environment,
            UniqueNumberUseCase useCase,
            NumberObfuscationMethod method);
    }
}
