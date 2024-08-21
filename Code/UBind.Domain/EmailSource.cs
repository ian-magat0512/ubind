// <copyright file="EmailSource.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Email source.
    /// </summary>
    public enum EmailSource
    {
        /// <summary>
        /// Email generated from System email.
        /// </summary>
        SystemEmail = 0,

        /// <summary>
        /// Email generated from automation.
        /// </summary>
        Automation = 1,

        /// <summary>
        /// Email generated for sending test email.
        /// </summary>
        TestEmail = 2,
    }
}
