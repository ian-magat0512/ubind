// <copyright file="PasswordReuseValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Helpers;

    /// <inheritdoc/>
    public class PasswordReuseValidator : IPasswordReuseValidator
    {
        private readonly IEnumerable<PasswordReuseRule> reuseRules;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordReuseValidator"/> class.
        /// </summary>
        /// <param name="reuseRule">Rule specifying password re-use limitations.</param>
        public PasswordReuseValidator(params PasswordReuseRule[] reuseRule)
        {
            this.reuseRules = reuseRule;
        }

        /// <summary>
        /// Gets a default validator to use when specific validation rules are not specified for a tenant.
        /// </summary>
        public static IPasswordReuseValidator Default
        {
            get
            {
                return new PasswordReuseValidator(StandardPasswordRules.CannotReuseEver);
            }
        }

        /// <inheritdoc/>
        public Result<Void, IEnumerable<string>> Validate(
            string saltedHashedPassword,
            IEnumerable<PasswordLifespan> history,
            Instant time,
            IPasswordHashingService passwordHashingService)
        {
            var results = this.reuseRules
                .Select(rule => rule.Invoke(saltedHashedPassword, history, time, passwordHashingService));
            return results.Any(result => !result.Succeeded)
                ? Result.Failure<Void, IEnumerable<string>>(results.Select(result => result.Error))
                : Result.Success<Void, IEnumerable<string>>(default);
        }
    }
}
