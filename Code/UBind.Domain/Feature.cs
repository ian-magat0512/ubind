// <copyright file="Feature.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.ComponentModel;

    /// <summary>
    /// Represents a feature available to tenancies.
    /// </summary>
    public enum Feature
    {
#pragma warning disable SA1602 // Enumeration items should be documented
        [Description("Policy Management")]
        PolicyManagement,

        [Description("Customer Management")]
        CustomerManagement,

        [Description("Quote Management")]
        QuoteManagement,

        [Description("Claims Management")]
        ClaimsManagement,

        [Description("User Management")]
        UserManagement,

        [Description("Message Management")]
        MessageManagement,

        [Description("Reporting")]
        Reporting,

        [Description("Product Management")]
        ProductManagement,

        [Description("Organisation Management")]
        OrganisationManagement,

        [Description("Portal Management")]
        PortalManagement,
#pragma warning restore SA1602 // Enumeration items should be documented
    }
}
