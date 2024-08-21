// <copyright file="IPasswordReuseValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System.Collections.Generic;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Helpers;

    /// <summary>
    /// For validating password reuse requirements.
    /// </summary>
    public interface IPasswordReuseValidator
    {
        /// <summary>
        /// Check that a given password meets a set of reuse rules.
        /// </summary>
        /// <param name="cleartextPassword">The cleartext password.</param>
        /// <param name="history">The history of passwords of the user.</param>
        /// <param name="time">The time the validation is being performed (for time-based validation).</param>
        /// <param name="passwordHashingService">A service for hashing and verifying passwords.</param>
        /// <returns>Success if the new password meets the reuse rules, otherwise failure and details of the reasons.</returns>
        Result<Void, IEnumerable<string>> Validate(
            string cleartextPassword,
            IEnumerable<PasswordLifespan> history,
            Instant time,
            IPasswordHashingService passwordHashingService);
    }
}
