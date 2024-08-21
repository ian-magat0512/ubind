// <copyright file="IAuthConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Authentication
{
    /// <summary>
    /// Represents the Auth0 configuration.
    /// </summary>
    public interface IAuthConfiguration
    {
        /// <summary>
        /// Gets permitted origins for CORS.
        /// </summary>
        string[] PermittedCorsOrigins { get; }

        /// <summary>
        /// Gets the issuer to use in access tokens.
        /// </summary>
        string TokenIssuer { get; }

        /// <summary>
        /// Gets the audience to use in access tokens.
        /// </summary>
        string TokenAudience { get; }
    }
}
