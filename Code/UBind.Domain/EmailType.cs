// <copyright file="EmailType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Email types. DO NOT USE.
    /// </summary>
    /// [Obsolete("We instead use relationships on an email to know it's recipient or purpose.")]
    public enum EmailType
    {
        /// <summary>
        /// Email for admins.
        /// </summary>
        Admin = 0,

        /// <summary>
        /// Email for customers.
        /// </summary>
        Customer = 1,

        /// <summary>
        /// User Email Type for user.
        /// </summary>
        User = 2,
    }
}
