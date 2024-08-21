// <copyright file="RoleHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using System;
    using NodaTime;
    using UBind.Domain.Entities;
    using UBind.Domain.Permissions;

    /// <summary>
    /// Class for creating dummy roles for test projects.
    /// </summary>
    public class RoleHelper
    {
        /// <summary>
        /// Creates the UBind admin role.
        /// </summary>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateMasterAdminRole(Instant createdTimestamp)
        {
            return new Role(Tenant.MasterTenantId, Guid.NewGuid(), DefaultRole.MasterAdmin, createdTimestamp);
        }

        /// <summary>
        /// Creates the UBind product developer role.
        /// </summary>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateUBindProductDeveloperRole(Instant createdTimestamp)
        {
            return new Role(Tenant.MasterTenantId, Guid.NewGuid(), DefaultRole.MasterProductDeveloper, createdTimestamp);
        }

        /// <summary>
        /// Creates a Tenant Admin Role. ( formerly called Client Admin ).
        /// </summary>
        /// <param name="tenantId">the parent tenant Id.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateTenantAdminRole(Guid tenantId, Guid organisationId, Instant createdTimestamp)
        {
            return new Role(tenantId, organisationId, DefaultRole.TenantAdmin, createdTimestamp);
        }

        /// <summary>
        /// Creates a Product Developer Role.
        /// </summary>
        /// <param name="tenantId">the parent tenant Id.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateClientProductDeveloperRole(Guid tenantId, Guid organisationId, Instant createdTimestamp)
        {
            return new Role(tenantId, organisationId, DefaultRole.ClientProductDeveloper, createdTimestamp);
        }

        /// <summary>
        /// Creates a Broker Role.
        /// </summary>
        /// <param name="tenantId">the parent tenant Id.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateClientBrokerRole(Guid tenantId, Guid organisationId, Instant createdTimestamp)
        {
            return new Role(tenantId, organisationId, DefaultRole.Broker, createdTimestamp);
        }

        /// <summary>
        /// Creates a Underwriter Role.
        /// </summary>
        /// <param name="tenantId">the parent tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateClientUnderwriterRole(Guid tenantId, Guid organisationId, Instant createdTimestamp)
        {
            return new Role(tenantId, organisationId, DefaultRole.UnderWriter, createdTimestamp);
        }

        /// <summary>
        /// Creates a Claims Agent Role.
        /// </summary>
        /// <param name="tenantId">the parent tenant Id.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateClientClaimsAgentRole(Guid tenantId, Guid organisationId, Instant createdTimestamp)
        {
            return new Role(tenantId, organisationId, DefaultRole.ClaimsAgent, createdTimestamp);
        }

        /// <summary>
        /// Creates a default Role for customer type.
        /// </summary>
        /// <param name="tenantId">the parent tenant Id.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateCustomerRole(Guid tenantId, Guid organisationId, Instant createdTimestamp)
        {
            var customerRole = new Role(tenantId, organisationId, DefaultRole.Customer, createdTimestamp);
            return customerRole;
        }

        /// <summary>
        /// Creates a Role for any type.
        /// </summary>
        /// <param name="tenantId">the parent tenant Id.</param>
        /// <param name="organisationId">The organisation id.</param>
        /// <param name="name">A descriptive name for the role.</param>
        /// <param name="description"> A descrtion of the role.</param>
        /// <param name="createdTimestamp">The created time (for auditing purposes).</param>
        /// <returns>A new instance of Role.</returns>
        public static Role CreateRole(
            Guid tenantId, Guid organisationId, string name, string description, Instant createdTimestamp)
        {
            var roleType = tenantId == Tenant.MasterTenantId ? RoleType.Master : RoleType.Client;
            return new Role(tenantId, organisationId, roleType, name, description, createdTimestamp, false);
        }
    }
}
