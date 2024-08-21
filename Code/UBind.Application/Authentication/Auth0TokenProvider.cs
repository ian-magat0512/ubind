// <copyright file="Auth0TokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

////namespace UBind.Application.Authentication
////{
////    using System.Threading.Tasks;
////    using Flurl.Http;
////    using Newtonsoft.Json;
////    using NodaTime;

////    /// <inheritdoc />
////    public class Auth0TokenProvider : IAuth0TokenProvider
////    {
////        private readonly IClock clock;
////        private readonly IAuth0Configuration configuration;

////        private AccessToken accessToken;

////        /// <summary>
////        /// Initializes a new instance of the <see cref="Auth0TokenProvider"/> class.
////        /// </summary>////        /// <param name="configuration">Auth0 configuration.</param>
////        /// <param name="clock">Clock.</param>
////        public Auth0TokenProvider(IAuth0Configuration configuration, IClock clock)
////        {
////            this.configuration = configuration;
////            this.clock = clock;
////        }

////        /// <inheritdoc />
////        public async Task<AccessToken> GetAccessToken()
////        {
////            if (this.accessToken == null
////                || this.accessToken.ExpiryTime.Minus(Duration.FromSeconds(60)) < this.clock.GetCurrentInstant())
////            {
////                await this.RefreshToken();
////            }

////            return this.accessToken;
////        }

////        private async Task RefreshToken()
////        {
////            string oauthTokenUrl = this.configuration.OauthTokenUrl;
////            TokenResponse tokenResponse = await oauthTokenUrl.PostJsonAsync(
////                new
////                {
////                    grant_type = "client_credentials",
////                    client_id = this.configuration.ServerClientId,
////                    client_secret = this.configuration.ClientSecret,
////                    audience = this.configuration.ManagementApiUrl
////                })
////                .ReceiveJson<TokenResponse>();

////            this.accessToken = new AccessToken(
////                this.clock.GetCurrentInstant(), tokenResponse.AccessToken, tokenResponse.LifespanInSeconds);
////        }

////        private class TokenResponse
////        {
////            [JsonProperty(PropertyName = "access_token")]
////            public string AccessToken { get; set; }

////            [JsonProperty(PropertyName = "expires_in")]
////            public int LifespanInSeconds { get; set; }

////            [JsonProperty(PropertyName = "scope")]
////            public string Scope { get; set; }

////            [JsonProperty(PropertyName = "token_type")]
////            public string TokenType { get; set; }
////        }
////    }
////}
