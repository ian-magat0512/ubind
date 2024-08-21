// <copyright file="IPasswordComplexityValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using System.Collections.Generic;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// For validating password complexity requirements.
    /// </summary>
    public interface IPasswordComplexityValidator
    {
        /// <summary>
        /// Check that a given password meets at least one of the validator's sets of complexity rules.
        /// </summary>
        /// <param name="cleartextPassword">The cleartext password.</param>
        /// <returns>Success if the new password meets at least one of the validator's sets of complexity rules,
        /// otherwise failure and details of the reasons.</returns>
        Result<Void, IEnumerable<string>> Validate(string cleartextPassword);
    }
}
