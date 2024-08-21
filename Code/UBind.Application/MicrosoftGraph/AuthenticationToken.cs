// <copyright file="AuthenticationToken.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using NodaTime;

    /// <summary>
    /// An OAuth bearer token with an expiry time.
    /// </summary>
    public class AuthenticationToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationToken"/> class.
        /// </summary>
        public AuthenticationToken()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationToken"/> class.
        /// </summary>
        /// <param name="bearerToken">The bearer token.</param>
        /// <param name="expiryTime">The expiry time.</param>
        public AuthenticationToken(string bearerToken, Instant expiryTime)
        {
            this.BearerToken = bearerToken;
            this.ExpiryTime = expiryTime;
        }

        /// <summary>
        /// Gets the OAuth bearer token.
        /// </summary>
        public string BearerToken { get; }

        /// <summary>
        /// Gets the expiry time.
        /// </summary>
        public Instant ExpiryTime { get; }
    }
}
