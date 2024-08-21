// <copyright file="Concern.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    /// <summary>
    /// Concerns are categories for permissions to be grouped by.
    /// They are presently only used for grouping permissions in the portal UI.
    /// </summary>
    public enum Concern
    {
        /// <summary>
        /// My Account.
        /// </summary>
        MyAccount,

        /// <summary>
        /// Users.
        /// </summary>
        Users,

        /// <summary>
        /// Roles.
        /// </summary>
        Roles,

        /// <summary>
        /// Quotes.
        /// </summary>
        Quotes,

        /// <summary>
        /// Quote Versions.
        /// </summary>
        QuoteVersions,

        /// <summary>
        /// Policies.
        /// </summary>
        Policies,

        /// <summary>
        /// Claims.
        /// </summary>
        Claims,

        /// <summary>
        /// Claims.
        /// </summary>
        Customers,

        /// <summary>
        /// Emails.
        /// </summary>
        Messages,

        /// <summary>
        /// Reports.
        /// </summary>
        Reports,

        /// <summary>
        /// Environments.
        /// </summary>
        Environments,

        /// <summary>
        /// Tenants.
        /// </summary>
        Tenants,

        /// <summary>
        /// Organisations.
        /// </summary>
        Organisations,

        /// <summary>
        /// Products.
        /// </summary>
        Products,

        /// <summary>
        /// Releases.
        /// </summary>
        Releases,

        /// <summary>
        /// Portals.
        /// </summary>
        Portals,

        /// <summary>
        /// Imports.
        /// </summary>
        Imports,

        /// <summary>
        /// Background Jobs.
        /// </summary>
        BackgroundJobs,

        /// <summary>
        /// Maintenance like integration events etc.
        /// </summary>
        Maintenance,

        /// <summary>
        /// Accounting
        /// </summary>
        Accounting,

        /// <summary>
        /// Additional Properties.
        /// </summary>
        AdditionalProperties,

        /// <summary>
        /// Startup Jobs.
        /// </summary>
        StartupJobs,

        /// <summary>
        /// Data table definition.
        /// </summary>
        DataTableDefinition,
    }
}
