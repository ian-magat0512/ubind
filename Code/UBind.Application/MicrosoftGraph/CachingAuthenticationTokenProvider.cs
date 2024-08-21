// <copyright file="CachingAuthenticationTokenProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using StackExchange.Profiling;
    using UBind.Domain.Helpers;

    /// <inheritdoc/>
    public class CachingAuthenticationTokenProvider : ICachingAuthenticationTokenProvider
    {
        private readonly IAuthenticationTokenProvider tokenProvider;
        private AuthenticationToken cachedToken;
        private string cacheKey = "authenticationToken";

        /// <summary>
        /// Initializes a new instance of the <see cref="CachingAuthenticationTokenProvider"/> class.
        /// </summary>
        /// <param name="tokenProvider">Provider for obtaining new tokens.</param>
        public CachingAuthenticationTokenProvider(IAuthenticationTokenProvider tokenProvider)
        {
            this.tokenProvider = tokenProvider;
        }

        /// <inheritdoc/>
        public async Task<AuthenticationToken> GetAuthenticationTokenAsync()
        {
            using (MiniProfiler.Current.Step(nameof(CachingAuthenticationTokenProvider) + "." + nameof(this.GetAuthenticationTokenAsync)))
            {
                return await MemoryCachingHelper.AddOrGetAsync(
                    this.cacheKey,
                    () =>
                        {
                            return this.tokenProvider.GetAuthenticationTokenAsync();
                        },
                    DateTimeOffset.Now.AddMinutes(20));
            }
        }

        /// <inheritdoc/>
        [DisplayName("Cache Microsoft Graph Authentication token.")]
        public async Task CacheAuthenticationTokenAsync()
        {
            this.cachedToken = await this.tokenProvider.GetAuthenticationTokenAsync();
        }

        /// <inheritdoc/>
        public void CacheReset()
        {
            MemoryCachingHelper.Remove(this.cacheKey);
        }
    }
}
