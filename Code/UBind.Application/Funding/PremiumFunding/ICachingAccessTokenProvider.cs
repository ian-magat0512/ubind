// <copyright file="ICachingAccessTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    /// <summary>
    /// Service for obtaining access tokens for premium funding.
    /// </summary>
    public interface ICachingAccessTokenProvider : IAccessTokenProvider
    {
        /// <summary>
        /// Clear any cached access token for the given username and API version.
        /// </summary>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="apiVersion">The API version access is required for.</param>
        void ClearCachedAccessToken(string username, string apiVersion);
    }
}
