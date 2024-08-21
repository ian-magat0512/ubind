// <copyright file="ITenantService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;

    /// <summary>
    /// Application service for handling tenant-related functionality.
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// Retrieves a tenant record in the system with the given ID.
        /// </summary>
        /// <param name="tenantId">The guid ID of the tenant to be retrieved.</param>
        /// <returns>A tenant record.</returns>
        Domain.Tenant GetTenant(Guid tenantId);

        void ThrowIfTenantAliasIsNull(string alias);

        void ThrowIfTenantAliasInUse(string alias);

        void ThrowIfTenantNameInUse(string name);

        void ThrowIfCustomDomainInUse(string customDomain);
    }
}
