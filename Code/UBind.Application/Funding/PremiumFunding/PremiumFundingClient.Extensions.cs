// <copyright file="PremiumFundingClient.Extensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.PremiumFunding
{
    using System.Net.Http;
    using System.Text;

    /// <summary>
    /// Extensions to support using access tokens.
    /// </summary>
    public partial class PremiumFundingClient
    {
        private readonly string accessToken;

        private PremiumFundingClient(string accessToken)
            : this()
        {
            this.accessToken = accessToken;
        }

        /// <summary>
        /// Create a new instance of the <see cref="PremiumFundingClient"/> class that will include an authentication header on all requests.
        /// </summary>
        /// <param name="accessToken">The bearer token to use in the authentication header.</param>
        /// <returns>A new instance of the <see cref="PremiumFundingClient"/> class.</returns>
        public static PremiumFundingClient CreateAuthenticatingClient(string accessToken)
        {
            return new PremiumFundingClient(accessToken);
        }

        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
        {
            if (this.accessToken != null)
            {
                urlBuilder.Append($"?AccessToken={this.accessToken}");
            }
        }

        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
        {
            ////if (this.accessToken != null)
            ////{
            ////    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.accessToken);
            ////}
        }
    }
}
