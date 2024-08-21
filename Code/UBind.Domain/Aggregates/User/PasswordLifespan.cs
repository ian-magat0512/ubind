// <copyright file="PasswordLifespan.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using NodaTime;

    /// <summary>
    /// For tracking the lifespan of a user password.
    /// </summary>
    public class PasswordLifespan
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordLifespan"/> class.
        /// </summary>
        /// <param name="saltedHashedPassword">The salted, hashed password.</param>
        /// <param name="validFrom">The time the password is valid from.</param>
        public PasswordLifespan(string saltedHashedPassword, Instant validFrom)
        {
            this.SaltedHashedPassword = saltedHashedPassword;
            this.ValidFrom = validFrom;
            this.ValidUntil = Instant.MaxValue;
        }

        /// <summary>
        /// Gets the salted, hashed password.
        /// </summary>
        public string SaltedHashedPassword { get; }

        /// <summary>
        /// Gets the time the password is valid from.
        /// </summary>
        public Instant ValidFrom { get; }

        /// <summary>
        /// Gets the time the password is valid until.
        /// </summary>
        public Instant ValidUntil { get; private set; }

        /// <summary>
        /// Set the password's expiry time.
        /// </summary>
        /// <param name="expiryTime">The time the password is valid until.</param>
        public void Expire(Instant expiryTime)
        {
            this.ValidUntil = expiryTime;
        }
    }
}
