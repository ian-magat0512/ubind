// <copyright file="ZaiAccessTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Zai
{
    using System.Threading.Tasks;
    using Flurl.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class ZaiAccessTokenProvider
    {
        private const int MinimumUsableTokenLifespanInSeconds = 10;
        private readonly IZaiConfiguration configuration;
        private readonly IClock clock;

        private Instant currentTokenUsefulnessExpiry;
        private ZaiAccessTokenResponse currentToken;

        public ZaiAccessTokenProvider(IZaiConfiguration configuration, IClock clock)
        {
            this.configuration = configuration;
            this.clock = clock;
        }

        /// <summary>
        /// Retrieves a usable access token for the Zai payment gateway.
        /// </summary>
        /// <returns>A usable access token for the payment gateway.</returns>
        public async Task<string> GetAccessTokenAsync()
        {
            if (!this.CurrentTokenIsUsable())
            {
                await this.ObtainNewToken();
            }

            return this.currentToken.AccessToken;
        }

        private async Task ObtainNewToken()
        {
            ZaiAccessTokenResponse response;
            var requestPayload = new
            {
                grant_type = "client_credentials",
                scope = this.configuration.Scope,
                client_id = this.configuration.ClientId,
                client_secret = this.configuration.ClientSecret,
            };
            try
            {
                response = await this.configuration.AuthorizationUrl
                    .PostJsonAsync(requestPayload)
                    .ReceiveJson<ZaiAccessTokenResponse>();
            }
            catch (FlurlHttpException ex)
            {
                string rawErrorResponse = await ex.GetResponseStringAsync();
                string usefulMessage = null;
                try
                {
                    var jObject = JObject.Parse(rawErrorResponse);
                    if (jObject.ContainsKey("error"))
                    {
                        usefulMessage = jObject["error"].ToString();
                    }
                    else
                    {
                        usefulMessage = rawErrorResponse;
                    }
                }
                catch (JsonReaderException)
                {
                    usefulMessage = rawErrorResponse;
                }

                throw new ErrorException(Errors.Payment.CouldNotObtainAccessToken(usefulMessage));
            }

            this.currentToken = response;
            this.currentTokenUsefulnessExpiry =
                this.clock.GetCurrentInstant()
                .Plus(Duration.FromSeconds(this.currentToken.ExpiresIn))
                .Minus(Duration.FromSeconds(MinimumUsableTokenLifespanInSeconds));
        }

        private bool CurrentTokenIsUsable()
        {
            return this.currentToken != null
                && this.clock.GetCurrentInstant() < this.currentTokenUsefulnessExpiry;
        }
    }
}
