// <copyright file="PasswordSettingResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the result of a password setting attempt.
    /// </summary>
    public class PasswordSettingResult
    {
        private PasswordSettingResult()
        {
            this.ErrorType = PasswordSetErrorType.None;
        }

        private PasswordSettingResult(IEnumerable<string> validationErrors)
        {
            this.ErrorType = PasswordSetErrorType.PasswordValidationError;
            this.Errors = validationErrors;
        }

        /// <summary>
        /// Gets a value indicating whether the password setting succeeded.
        /// </summary>
        public bool Succeeded => this.ErrorType == PasswordSetErrorType.None;

        /// <summary>
        /// Gets the type of error that was encountered if the setting failed, otherwise <see cref="PasswordSetErrorType.None"/>.
        /// </summary>
        public PasswordSetErrorType ErrorType { get; }

        /// <summary>
        /// Gets descriptions of the errors that occurred.
        /// </summary>
        public IEnumerable<string> Errors { get; }

        /// <summary>
        /// Creates a new instance of <see cref="PasswordSettingResult"/> representing a successful result.
        /// </summary>
        /// <returns>A new instance of <see cref="PasswordSettingResult"/> representing a successful result.</returns>
        public static PasswordSettingResult Success()
        {
            return new PasswordSettingResult();
        }

        /// <summary>
        /// Creates a new instance of <see cref="PasswordSettingResult"/> representing a error caused by a password
        /// validation error.
        /// </summary>
        /// <param name="validationErrors">The validation error.</param>
        /// <returns>
        /// A new instance of <see cref="PasswordSettingResult"/> representing an error caused by a password
        /// validation error.
        /// .</returns>
        public static PasswordSettingResult FromValidationError(IEnumerable<string> validationErrors)
        {
            return new PasswordSettingResult(validationErrors);
        }
    }
}
