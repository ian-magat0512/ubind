// <copyright file="IAccessTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.PremiumFunding
{
    using System.Threading.Tasks;

    /// <summary>
    /// Service for obtaining access tokens for premium funding.
    /// </summary>
    public interface IAccessTokenProvider
    {
        /// <summary>
        /// Get an access token for the Premium Funding API.
        /// </summary>
        /// <param name="username">The username to authenticate with.</param>
        /// <param name="password">The password to authenticate with.</param>
        /// <param name="apiVersion">The API version access is required for.</param>
        /// <returns>A usable access token for the Premium Funding Company's API gateway.</returns>
        Task<string> GetAccessToken(string username, string password, string apiVersion);
    }
}
