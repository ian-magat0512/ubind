// <copyright file="AuthenticationTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System;
    using System.Runtime.Caching;
    using System.Threading.Tasks;
    using Flurl.Http;
    using NodaTime;
    using StackExchange.Profiling;

    /// <summary>
    /// Encapsulate authentication token request call, so it can be re-used in integration tests etc.
    /// </summary>
    public class AuthenticationTokenProvider : IAuthenticationTokenProvider
    {
        private readonly IGraphUrlProvider urlProvider;
        private readonly IMicrosoftGraphConfiguration configuration;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationTokenProvider"/> class.
        /// </summary>
        /// <param name="urlProvider">Helper for generating MS Graph URLs.</param>
        /// <param name="configuration">MS Graph configuration settings.</param>
        /// <param name="clock">A clock!.</param>
        public AuthenticationTokenProvider(
            IGraphUrlProvider urlProvider,
            IMicrosoftGraphConfiguration configuration,
            IClock clock)
        {
            this.urlProvider = urlProvider;
            this.configuration = configuration;
            this.clock = clock;
        }

        /// <inheritdoc />
        public async Task<AuthenticationToken> GetAuthenticationTokenAsync()
        {
            using (MiniProfiler.Current.Step(nameof(AuthenticationTokenProvider) + "." + nameof(this.GetAuthenticationTokenAsync)))
            {
                var payload = new
                {
                    grant_type = "password",
                    scope = "Files.ReadWrite.All User.Read",
                    client_id = this.configuration.ClientId,
                    resource = this.urlProvider.GraphOrigin,
                    username = this.configuration.Username,
                    password = this.configuration.Password,
                };

                bool success = false;
                int attempt = 0;
                ObjectCache cache = MemoryCache.Default;
                int cacheDurationSeconds = 5;
                string cacheName = "authToken" + payload.username + payload.password;
                AuthenticationToken token = cache[cacheName] as AuthenticationToken;

                while (!success && token == null)
                {
                    try
                    {
                        // pause for half a second just to give the api some room to breath
                        if (attempt > 0)
                        {
                            await Task.Delay(500);
                        }

                        attempt++;

                        var result = await this.urlProvider.AuthProviderOrigin
                        .AppendPathSegments(this.configuration.ApplicationId, this.urlProvider.OAuthTokenPathSegment)
                        .PostUrlEncodedAsync(payload)
                        .ReceiveJson();

                        var expiryTime = this.clock.GetCurrentInstant()
                            .Plus(Duration.FromMinutes(this.configuration.AccessTokenLifeTimeInMinutes));
                        token = new AuthenticationToken(result.access_token, expiryTime);

                        // add it on cache for 5 seconds
                        CacheItemPolicy policy = new CacheItemPolicy();
                        policy.AbsoluteExpiration = DateTime.Now.AddSeconds(cacheDurationSeconds);
                        cache.Set(cacheName, token, policy);

                        success = true;
                    }
                    catch (FlurlHttpException) when (attempt < this.configuration.MaxRetryAttempts)
                    {
                    }
                }

                if (token == null)
                {
                    throw new ApplicationException("no authentication token retrieved on multiple tries");
                }

                return token;
            }
        }
    }
}
