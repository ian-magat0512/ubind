// <copyright file="StandardPasswordRules.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using NodaTime;

    /// <summary>
    /// Class specifying standard password validation rules.
    /// </summary>
    public static class StandardPasswordRules
    {
        private const string ContainsAtLeastOneLetterPattern = @"[a-zA-Z]";
        private const string ContainsAtLeastOneDigitPattern = @"[0-9]";
        private const string ContainsAtLeastOneNonAlphaNumericCharacterPattern = @"[^a-zA-Z0-9]";

        /// <summary>
        /// Gets a rule specifying that the password must contain at least one letter.
        /// </summary>
        public static PasswordComplexityRule MustContainLetter => CreateRegExRule(
            ContainsAtLeastOneLetterPattern,
            "Must contain at least one letter.");

        /// <summary>
        /// Gets a rule specifying that the password must contain at least one digit.
        /// </summary>
        public static PasswordComplexityRule MustContainDigit => CreateRegExRule(
            ContainsAtLeastOneDigitPattern,
            "Must contain at least one digit.");

        /// <summary>
        /// Gets a rule specifying that the password must contain at least one non-alphanumeric character.
        /// </summary>
        public static PasswordComplexityRule MustContainNonAlphaNumeric => CreateRegExRule(
            ContainsAtLeastOneNonAlphaNumericCharacterPattern,
            "Must contain at least one non-alphanumeric character.");

        /// <summary>
        /// Gets a rule specifying that a password can never be re-used.
        /// </summary>
        public static PasswordReuseRule CannotReuseEver => (saltedHashedPassword, history, time, hashingService) =>
            history.Any(password => hashingService.Verify(saltedHashedPassword, password.SaltedHashedPassword))
                ? ResultOld<string>.Failure($"Cannot reuse passwords.")
                : ResultOld<string>.Success();

        /// <summary>
        /// Gets a rule specifying that a password cannot match any of last five passwords.
        /// </summary>
        public static PasswordReuseRule MustNotBeOneOfLastFivePasswords => CreateSequenceBasedRecencyRule(5);

        /// <summary>
        /// Gets a rule specifying that a password cannot be reused within one year.
        /// </summary>
        public static PasswordReuseRule MustNotBeReusedWithinOneYear => CreateTimeBasedRecencyRuleInDays(365);

        /// <summary>
        /// Gets a rule specifying the minimum length of the password.
        /// </summary>
        /// <param name="minimumLength">The minimum number of characters in the password,.</param>
        /// <returns>A rule specifying the minimum length of the password.</returns>
        public static PasswordComplexityRule MinimumLength(int minimumLength) =>
            (cleartext) => cleartext.Length >= minimumLength
                ? ResultOld<string>.Success()
                : ResultOld<string>.Failure($"Must be at least {minimumLength} characters long.");

        private static PasswordReuseRule CreateSequenceBasedRecencyRule(int numberOfPasswordsToCheck) =>
            (cleartextPassword, history, time, hashingService) => history
                .OrderByDescending(password => password.ValidFrom)
                .Take(numberOfPasswordsToCheck)
                .Any(password => hashingService.Verify(cleartextPassword, password.SaltedHashedPassword))
                    ? ResultOld<string>.Failure($"Cannot reuse any of last {numberOfPasswordsToCheck} passwords.")
                    : ResultOld<string>.Success();

        private static PasswordReuseRule CreateTimeBasedRecencyRuleInDays(int numberOfDays) =>
            (cleartextPassword, history, time, hashingService) => history
                .OrderByDescending(password => password.ValidFrom)
                .Where(password => password.ValidUntil.Plus(Duration.FromDays(numberOfDays)) >= time)
                .Any(password => hashingService.Verify(cleartextPassword, password.SaltedHashedPassword))
                    ? ResultOld<string>.Failure($"Cannot reuse a password that has been used in the last {numberOfDays} days.")
                    : ResultOld<string>.Success();

        private static PasswordComplexityRule CreateRegExRule(string pattern, string errorMessage) =>
            (cleartext) => Regex.IsMatch(cleartext, pattern)
                ? ResultOld<string>.Success()
                : ResultOld<string>.Failure(errorMessage);
    }
}
