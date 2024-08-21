// <copyright file="AccessToken.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Authentication
{
    using NodaTime;

    /// <summary>
    /// Represents the access token.
    /// </summary>
    public class AccessToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessToken"/> class.
        /// </summary>
        /// <param name="issueTime">Unix timestamp issue time.</param>
        /// <param name="token">Token.</param>
        /// <param name="timeToLiveInSeconds">Time to live in seconds.</param>
        public AccessToken(Instant issueTime, string token, int timeToLiveInSeconds)
        {
            this.Token = token;
            this.ExpiryTimestamp = issueTime.Plus(Duration.FromSeconds(timeToLiveInSeconds));
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        public string Token { get; private set; }

        /// <summary>
        /// Gets the expiration time.
        /// </summary>
        public Instant ExpiryTimestamp { get; private set; }
    }
}
