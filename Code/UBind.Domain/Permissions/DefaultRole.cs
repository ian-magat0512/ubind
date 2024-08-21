// <copyright file="DefaultRole.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Permissions
{
    /// <summary>
    /// Enumeration of default roles.
    /// </summary>
    public enum DefaultRole
    {
        /// <summary>
        /// uBind Role.
        /// </summary>
        [RoleInformation(
            name: "Master Admin",
            description: "Full admin rights across the master tenancy",
            roleType: RoleType.Master,
            isFixed: true)]
        MasterAdmin,

        /// <summary>
        /// Master Product Developer Role.
        /// </summary>
        [RoleInformation(name: "Product Developer", description: "Product Developer", roleType: RoleType.Master)]
        MasterProductDeveloper,

        /// <summary>
        /// Client Role.
        /// </summary>
        [RoleInformation(name: "Tenant Admin", description: "Full admin rights across the tenancy", roleType: RoleType.Client, isFixed: true)]
        TenantAdmin,

        /// <summary>
        /// Broker Role.
        /// </summary>
        [RoleInformation(name: "Broker", description: "Broker", roleType: RoleType.Client)]
        Broker,

        /// <summary>
        /// Underwriter Role.
        /// </summary>
        [RoleInformation(name: "Underwriter", description: "Underwriter", roleType: RoleType.Client)]
        UnderWriter,

        /// <summary>
        /// Claims Agent Role.
        /// </summary>
        [RoleInformation(name: "Claims Agent", description: "Claims Agent", roleType: RoleType.Client)]
        ClaimsAgent,

        /// <summary>
        /// Client Product Developer Role.
        /// </summary>
        [RoleInformation(name: "Product Developer", description: "Product Developer", roleType: RoleType.Client)]
        ClientProductDeveloper,

        /// <summary>
        /// Customer Role.
        /// </summary>
        [RoleInformation(
            name: "Customer",
            description: "Default role automatically assigned to customer users.",
            roleType: RoleType.Customer)]
        Customer,

        /// <summary>
        /// Client Role.
        /// </summary>
        [RoleInformation(
            name: "Organisation Admin",
            description: "Full admin rights across the organisation",
            roleType: RoleType.Client,
            isFixed: true)]
        OrganisationAdmin,

        /// <summary>
        /// Master Support Agent
        /// </summary>
        [RoleInformation(
            name: "Support Agent",
            description: "Rights to support and administer tenancies",
            roleType: RoleType.Master)]
        MasterSupportAgent,
    }
}
