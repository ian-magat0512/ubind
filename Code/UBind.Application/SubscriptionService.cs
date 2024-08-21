// <copyright file="SubscriptionService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Threading.Tasks;
    using Flurl;
    using Flurl.Http;
    using UBind.Application.MicrosoftGraph;

    /// <inheritdoc/>
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ICachingAuthenticationTokenProvider authenticator;
        private readonly IGraphUrlProvider urlProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionService"/> class.
        /// </summary>
        /// <param name="authenticator">Authenticator for providing authentication token.</param>
        /// <param name="urlProvider">For providing URLs for accessing Graph functionality.</param>
        public SubscriptionService(ICachingAuthenticationTokenProvider authenticator, IGraphUrlProvider urlProvider)
        {
            this.authenticator = authenticator;
            this.urlProvider = urlProvider;
        }

        /// <inheritdoc/>
        public async Task SubscribeToNotifications(Url notificationUrl)
        {
            var tokenWithExpiry = await this.authenticator.GetAuthenticationTokenAsync();
            var token = tokenWithExpiry.BearerToken;
            var expiry = DateTime.UtcNow.AddYears(1);
            var payload = new
            {
                notificationUrl = notificationUrl.ToString(),
                expirationDateTime = "2016-01-01T11:23:00.000Z",
                resource = "/me/drive/root",
                changeType = "updated",
                clientState = "client-specific string",
            };

            await Url.Combine("https://graph.microsoft.com/v1.0/subscriptions")
                .WithOAuthBearerToken(token)
                .PostJsonAsync(payload);
        }
    }
}
