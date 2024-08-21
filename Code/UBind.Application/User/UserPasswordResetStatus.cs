// <copyright file="UserPasswordResetStatus.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.User
{
    /// <summary>
    /// Represents enum for user password reset status.
    /// </summary>
    public enum UserPasswordResetStatus
    {
        /// <summary>
        /// User not found.
        /// </summary>
        NotFound,

        /// <summary>
        /// User account was deactivated.
        /// </summary>
        Deactivated,

        /// <summary>
        /// Invitation already used.
        /// </summary>
        UsedInvitation,

        /// <summary>
        /// Invitation expired.
        /// </summary>
        ExpiredInvitation,

        /// <summary>
        /// Ready for password reset verification.
        /// </summary>
        ReadyForPasswordReset,

        /// <summary>
        /// Verified the password reset invitation and marked as used.
        /// </summary>
        PasswordReset,
    }
}
