// <copyright file="CachingAccessTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Funding.PremiumFunding
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Domain.Extensions;
    using UBind.Domain.Funding;

    /// <summary>
    /// For obtaining access tokens for the Premium Funding Company's API, that caches tokens for re-use.
    /// </summary>
    public class CachingAccessTokenProvider : ICachingAccessTokenProvider
    {
        private const int TokenLifespanInMinutes = 8 * 60;
        private const int SafetyMarginInMinutes = 30;
        private readonly IAccessTokenProvider accessTokenProvider;
        private readonly IClock clock;
        private readonly Dictionary<string, CachedToken> cachedTokens = new Dictionary<string, CachedToken>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingAccessTokenProvider"/> class.
        /// </summary>
        /// <param name="accessTokenProvider">Service for obtaining token that will be cached.</param>
        /// <param name="clock">A clock.</param>
        public CachingAccessTokenProvider(IAccessTokenProvider accessTokenProvider, IClock clock)
        {
            this.accessTokenProvider = accessTokenProvider;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<string> GetAccessToken(string username, string password, string apiVersion)
        {
            using (MiniProfiler.Current.Step(nameof(CachingAccessTokenProvider) + "." + nameof(this.GetAccessToken)))
            {
                var cachingKey = apiVersion + username;
                if (this.cachedTokens.TryGetValue(cachingKey, out CachedToken? cachedToken))
                {
                    var dateTimeMargin = this.clock.Now().Plus(Duration.FromMinutes(SafetyMarginInMinutes)).ToDateTimeOffset();
                    if (cachedToken?.ExpiryTime > dateTimeMargin)
                    {
                        return cachedToken.Token;
                    }
                }

                var newToken = await this.accessTokenProvider.GetAccessToken(username, password, apiVersion);
                var newExpiryTime = this.clock.Now().Plus(Duration.FromMinutes(TokenLifespanInMinutes)).ToDateTimeOffset();
                var newCachedToken = new CachedToken(newToken, newExpiryTime);
                if (this.cachedTokens.ContainsKey(cachingKey))
                {
                    this.cachedTokens.Remove(cachingKey);
                }

                if (!this.cachedTokens.ContainsKey(cachingKey))
                {
                    this.cachedTokens.Add(cachingKey, newCachedToken);
                }

                return newToken;
            }
        }

        /// <inheritdoc/>
        public void ClearCachedAccessToken(string username, string apiVersion)
        {
            var key = this.CreateCachingKey(username, apiVersion);
            this.cachedTokens.Remove(key);
        }

        private string CreateCachingKey(string username, string apiVersion)
        {
            return apiVersion + username;
        }
    }
}
