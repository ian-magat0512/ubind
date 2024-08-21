// <copyright file="IAuthenticationTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System.Threading.Tasks;

    /// <summary>
    /// Authenticator for obtaining OAuth bearer token for Microsoft Graph.
    /// </summary>
    public interface IAuthenticationTokenProvider
    {
        /// <summary>
        /// Return an OAuth bearer token for Microsoft Graph asynchronously either from the cache or
        /// directly from MS Graph if one is not already cached.
        /// </summary>
        /// <returns>The OOauth bearer token.</returns>
        Task<AuthenticationToken> GetAuthenticationTokenAsync();
    }
}
