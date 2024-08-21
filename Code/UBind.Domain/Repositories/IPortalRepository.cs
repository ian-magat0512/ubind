// <copyright file="IPortalRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Repository for portal.
    /// </summary>
    public interface IPortalRepository
    {
        /// <summary>
        /// Get all the portals in the repository that satisfy given parameters.
        /// </summary>
        /// <returns>All the portals in the repository that satisfy given parameters.</returns>
        /// <param name="filters">Filters to apply.</param>
        IEnumerable<Portal> GetPortals(Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Find the portal with a given ID.
        /// </summary>
        Portal GetPortalById(Guid tenantId, Guid id);

        /// <summary>
        /// This function is getting the portals without tenantId it used for migrations purposed only.
        /// </summary>
        /// <param name="id">The Id of the tenant.</param>
        /// <returns>Return the portal that without tenantId.</returns>
        Portal GetPortalByIdWithoutTenantIdForMigrations(Guid id);

        /// <summary>
        /// Retrieves a portal record in the system that satisfy the given parameters.
        /// </summary>
        Portal GetPortalByAlias(Guid tenantId, string portalAlias);

        /// <summary>
        /// Get all the portals by tenant in the repository that is ever created.
        /// Note: this should only be used by a migration to set the tenant guid id, nothing more.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>All the portals in the repository filtered by tenant.</returns>
        IEnumerable<Portal> GetAllPortalsByTenant(Guid tenantId);

        /// <summary>
        /// Get all the portals by tenant in the repository.
        /// </summary>
        /// <param name="tenantId">The tenant Guid ID.</param>
        /// <returns>All the portals in the repository filtered by tenant.</returns>
        IEnumerable<Portal> GetPortalsByTenant(Guid tenantId);

        /// <summary>
        /// Store a portal in the repository.
        /// </summary>
        /// <param name="portal">The tenant to store.</param>
        void AddPortal(Portal portal);

        /// <summary>
        /// Checks to see if a given name is available for use by a portal in a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to check in.</param>
        /// <param name="name">The name to check.</param>
        /// <param name="portalId">The ID of the portal the name would be for, or default if not specified.</param>
        /// <returns><c>true</c> if the name is already existing for tenant, othewise <c>false</c>.</returns>
        bool PortalNameExistingForTenant(Guid tenantId, string name, Guid portalId = default);

        /// <summary>
        /// Checks to see if a given alias is available for use by a portal in a given tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant to check in.</param>
        /// <param name="alias">The alias to check.</param>
        /// <param name="portalId">The ID of the portal the name would be for, or default if not specified.</param>
        /// <returns><c>true</c> if the alias is already existing for the tenant, othewise <c>false</c>.</returns>
        bool PortalAliasExistingForTenant(Guid tenantId, string alias, Guid portalId = default);

        /// <summary>
        /// Save changes.
        /// </summary>
        void SaveChanges();

        /// <summary>
        /// Gets the list of portal ids.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="skip">The current page index.</param>
        /// <param name="pageSize">The max count per page.</param>
        /// <returns>List of ids.</returns>
        List<Guid> GetPortalIdsBy(Guid tenantId, int skip, int pageSize);

        /// <summary>
        /// Sets the tenant's default organanisation id on portals.
        /// This is needed because we are addding organisation ID to portals
        /// and so we need to set an initial value. Since all portals were only
        /// against the tenant previously, it's fine to set the organisation ID
        /// to the default.
        /// </summary>
        void SetOrganisationIdOnPortalsForTenantWhenEmpty(Guid tenantId, Guid organisationId);
    }
}
