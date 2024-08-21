// <copyright file="UserPassword.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System;
    using NodaTime;

    /// <summary>
    /// Credentials for a user to log in to UBind.
    /// </summary>
    public class UserPassword : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserPassword"/> class.
        /// </summary>
        /// <param name="saltedHashedPassword">The user's salted hashed password.</param>
        /// <param name="createdTimestamp">The time the credentials where created.</param>
        public UserPassword(
            string saltedHashedPassword,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.SaltedHashedPassword = saltedHashedPassword;
        }

        // Parameterless constructor for EF.
        private UserPassword()
            : base(default, default)
        {
        }

        /// <summary>
        /// Gets the user's salted hashed password.
        /// </summary>
        public string SaltedHashedPassword { get; private set; }

        /// <summary>
        /// Gets the ID of the User (Account) this password is for.
        /// Navigation property for EF.
        /// </summary>
        public Guid UserAccountId { get; private set; }
    }
}
