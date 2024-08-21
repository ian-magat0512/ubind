// <copyright file="UserStatus.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;

    /// <summary>
    /// User statuses.
    /// </summary>
    [Flags]
    public enum UserStatus
    {
        /// <summary>
        /// Users who have not ben invited yet.
        /// </summary>
        New = 0,

        /// <summary>
        /// Active users.
        /// </summary>
        Active = 1,

        /// <summary>
        /// Deactivated users.
        /// </summary>
        Deactivated = 1 << 1,

        /// <summary>
        /// For backwards compatibility
        /// </summary>
        Disabled = Deactivated,

        /// <summary>
        /// Invited users.
        /// </summary>
        Invited = 1 << 2,

        /// <summary>
        /// All statuses.
        /// </summary>
        All = Active | Deactivated | Invited,
    }
}
