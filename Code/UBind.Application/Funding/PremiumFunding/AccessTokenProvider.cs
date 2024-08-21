// <copyright file="AccessTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using System;
    using System.Threading.Tasks;
    using Flurl.Http;
    using Microsoft.CSharp.RuntimeBinder;
    using StackExchange.Profiling;

    /// <summary>
    /// For obtaining access tokens for the DEFT API.
    /// </summary>
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private const string Url = "https://api.premiumfunding.net.au/token";

        /// <inheritdoc/>
        public async Task<string> GetAccessToken(string username, string password, string apiVersion)
        {
            using (MiniProfiler.Current.Step(nameof(AccessTokenProvider) + "." + nameof(this.GetAccessToken)))
            {
                // Payload properties are case-sensitive.
                var payload = new
                {
                    Username = username,
                    Password = password,
                    ApiVersion = apiVersion,
                };
                var response = await Url
                .PostJsonAsync(payload)
                .ReceiveJson();

                try
                {
                    return response?.data?.attributes?.AccessToken?.ToString();
                }
                catch (RuntimeBinderException ex)
                {
                    throw new InvalidOperationException("JSON not in expected format", ex);
                }
            }
        }
    }
}
