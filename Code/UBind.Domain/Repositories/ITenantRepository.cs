// <copyright file="ITenantRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Repository for tenants.
    /// </summary>
    public interface ITenantRepository : IRepository
    {
        /// <summary>
        /// Get all active/inactive the tenants in the repository.
        /// Includes deleted tenants.
        /// </summary>
        /// <returns>All the tenants in the repository.</returns>
        List<Tenant> GetTenants(EntityListFilters filters = null, bool includeMasterTenant = true);

        /// <summary>
        /// Retrieves a tenant record in the system with the given portal domain.
        /// </summary>
        /// <param name="customDomain">The custom domain.</param>
        /// <returns>A tenant record.</returns>
        Tenant GetTenantByCustomDomain(string customDomain);

        /// <summary>
        /// Get all tenants in the repository that have not been deleted.
        /// </summary>
        /// <param name="filters">Filters to apply to the query.</param>
        /// <returns>All the tenants in the repository.</returns>
        List<Tenant> GetActiveTenants(EntityListFilters filters = null, bool includeMasterTenant = true);

        /// <summary>
        /// Find the tenant with a given Guid.
        /// </summary>
        /// <param name="tenantId">The GUID of the tenant.</param>
        /// <returns>The tenant with given Guid, or null if not found.</returns>
        Tenant? GetTenantById(Guid tenantId);

        /// <summary>
        /// Find the tenant alias with a given Guid.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns>The tenant alias, or null if not found.</returns>
        Task<string?> GetTenantAliasById(Guid tenantId);

        /// <summary>
        /// Retrieve latest detail.
        /// </summary>
        /// <param name="tenantId">The tenant GUID.</param>
        /// <returns>Tenant Detail.</returns>
        TenantDetails GetLatestDetail(Guid tenantId);

        /// <summary>
        /// Gets the tenant with related entities by id.
        /// </summary>
        /// <param name="tenantId">The tenant GUID.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The product read model with related entities.</returns>
        ITenantWithRelatedEntities GetTenantWithRelatedEntitiesById(Guid tenantId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets the tenant with related entities by alias.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="tenantAlias">The tenant Alias.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The product read model with related entities.</returns>
        ITenantWithRelatedEntities GetTenantWithRelatedEntitiesByAlias(
            Guid tenantId, string tenantAlias, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Find the tenant using the tenant name alias.
        /// </summary>
        /// <param name="alias">The name alias of the tenant.</param>
        /// <param name="includeDeleted">Search includes deleted tenants.</param>
        /// <returns>The tenant with the alias, or null if not found.</returns>
        Tenant GetTenantByAlias(string alias, bool includeDeleted = false);

        /// <summary>
        /// Retrieves tenant using the old string id ( which was removed ).
        /// Note: Please dont use this outside events, for backward compatibility use only.
        /// </summary>
        /// <param name="tenantStringId">The old tenant identifier.</param>
        /// <returns>The tenant.</returns>
        Tenant GetTenantByStringId(string tenantStringId);

        /// <summary>
        /// Checks if tenant id was deleted.
        /// </summary>
        /// <param name="tenantId">The GUID of the tenant.</param>
        /// <returns><c>true</c> if the tenant id was deleted, otherwise <c>false</c>.</returns>
        bool TenantIdWasDeleted(Guid tenantId);

        /// <summary>
        /// Store a tenant in the repository.
        /// </summary>
        /// <param name="tenant">The tenant to store.</param>
        void Insert(Tenant tenant);

        /// <summary>
        /// Checks to see if a given name is being used as a tenant name.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns><c>false</c> if the name not being used, otherwise <c>false</c>.</returns>
        bool IsNameInUse(string name);

        /// <summary>
        /// Checks to see if a given alias being used as a tenant alias.
        /// </summary>
        /// <param name="alias">The alias to check.</param>
        bool IsAliasInUse(string alias);

        /// <summary>
        /// Checks to see if a given custom portal domain is available.
        /// </summary>
        /// <param name="customDomainName">The alias to check.</param>
        bool IsCustomDomainInUse(string customDomainName);

        /// <summary>
        /// Method for creating IQueryable method that retrieve tenants and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for tenants.</returns>
        IQueryable<TenantWithRelatedEntities> CreateQueryForTenantWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Save changes.
        /// </summary>
        void SaveChanges();
    }
}
