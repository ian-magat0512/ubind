// <copyright file="ICalculationJsonSanitizer.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Service for sanitizing json from calculation results.
    /// </summary>
    public interface ICalculationJsonSanitizer
    {
        /// <summary>
        /// Sanitize calculation result pseudo-json.
        /// </summary>
        /// <param name="pseudoJson">The pseudo-json.</param>
        /// <returns>Valid json.</returns>
        /// <exception cref="JsonSanitizationException">Thrown when the pseudo-json cannot be sanitized.</exception>
        string Sanitize(string pseudoJson);
    }
}
