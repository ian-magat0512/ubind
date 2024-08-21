// <copyright file="PortalUserType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.ComponentModel;

    public enum PortalUserType
    {
        /// <summary>
        /// For brokers, underwriters and agents to log into.
        /// </summary>
        [Description("Agent")]
        Agent = 0,

        /// <summary>
        /// For customers to to log into.
        /// </summary>
        [Description("Customer")]
        Customer = 1,
    }
}
