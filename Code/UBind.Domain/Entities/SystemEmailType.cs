// <copyright file="SystemEmailType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Enumeration of system email types.
    /// </summary>
    public enum SystemEmailType
    {
        /// <summary>
        /// Email sent when user account has been created, with link to activate account by setting a password.
        /// </summary>
        [Order(1)]
        AccountActivationInvitation = 0,

        /// <summary>
        /// Email sent when user requests password reset, with a link to reset the account password.
        /// </summary>
        [Order(2)]
        PasswordResetInvitation = 1,

        /// <summary>
        /// Email sent when sending renewal invitation, with link to renew the policy.
        /// </summary>
        [Order(4)]
        RenewalInvitation = 2,

        /// <summary>
        /// Email sent when sending quote association invitation, with link to quote association.
        /// </summary>
        [Order(5)]
        QuoteAssociationInvitation = 3,

        /// <summary>
        /// Email sent when user password expired, with a link to reset the account password
        /// </summary>
        [Order(3)]
        PasswordExpiredResetInvitation = 4,

        /// <summary>
        /// Email sent when there is an account creation request for a user that was already activated.
        /// </summary>
        [Order(6)]
        AccountAlreadyActivated = 5,
    }
}
