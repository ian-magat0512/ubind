// <copyright file="ISignupService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Authentication
{
    using CSharpFunctionalExtensions;
    using UBind.Domain.Aggregates.User;

    /// <summary>
    /// Service for authenticating users.
    /// </summary>
    public interface ISignupService
    {
        /// <summary>
        /// Create a new user account, including credentials and user.
        /// </summary>
        /// <param name="request">The signpu request details.</param>
        /// <returns>A result containing the authenticated user, or an error message.</returns>
        Result<UserAggregate, string> SignUpUser(SignupRequest request);
    }
}
