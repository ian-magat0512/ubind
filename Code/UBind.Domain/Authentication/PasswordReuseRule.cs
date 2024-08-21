// <copyright file="PasswordReuseRule.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.Aggregates.User;

    /// <summary>
    /// Delegate for defining a rule determining whether a new password can be used for a user based on password history.
    /// </summary>
    /// <param name="cleartextPassword">The cleartext password to check.</param>
    /// <param name="historicPasswords">The user's historic salted, hashed passwords.</param>
    /// <param name="time">The time the reuse rule is being applied.</param>
    /// <param name="passwordHashingService">A service for hashing and verifying passwords.</param>
    /// <returns>A result indicating success if the password can be used, or specifying an error if the rule denies use of the password.</returns>
    public delegate ResultOld<string> PasswordReuseRule(
        string cleartextPassword, IEnumerable<PasswordLifespan> historicPasswords, Instant time, IPasswordHashingService passwordHashingService);
}
