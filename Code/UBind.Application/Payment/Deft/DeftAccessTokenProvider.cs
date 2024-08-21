// <copyright file="DeftAccessTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Deft
{
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// For obtaining access tokens for the DEFT API.
    /// </summary>
    public class DeftAccessTokenProvider
    {
        private const int MinimumUsableTokenLifespanInSeconds = 10;
        private readonly IDeftConfiguration configuration;
        private readonly IClock clock;
        private DeftAccessTokenResponse currentToken;
        private Instant currentTokenUsefulnessExpiry;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeftAccessTokenProvider"/> class.
        /// </summary>
        /// <param name="configuration">DEFT Configuration.</param>
        /// <param name="clock">A clock.</param>
        public DeftAccessTokenProvider(IDeftConfiguration configuration, IClock clock)
        {
            this.configuration = configuration;
            this.clock = clock;
        }

        /// <summary>
        /// Get a usable access token for the DEFT gateway.
        /// </summary>
        /// <returns>A usable access token for the DEFT gateway.</returns>
        public async Task<string> GetAccessToken()
        {
            if (!this.CurrentTokenIsUsable())
            {
                await this.ObtainNewToken();
            }

            return this.currentToken.AccessToken;
        }

        private async Task ObtainNewToken()
        {
            DeftAccessTokenResponse response;
            try
            {
                response = await this.configuration.AuthorizationUrl
                    .SetQueryParam("grant_type", "client_credentials")
                    .SetQueryParam("scope", "deft_biller_write")
                    .WithBasicAuth(this.configuration.ClientId, this.configuration.ClientSecret)
                    .PostAsync(null)
                    .ReceiveJson<DeftAccessTokenResponse>();
            }
            catch (FlurlHttpException ex)
            {
                string rawErrorResponse = await ex.GetResponseStringAsync();
                string usefulMessage = null;
                try
                {
                    var jObject = JObject.Parse(rawErrorResponse);
                    if (jObject.ContainsKey("Error"))
                    {
                        usefulMessage = jObject["Error"].ToString();
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
