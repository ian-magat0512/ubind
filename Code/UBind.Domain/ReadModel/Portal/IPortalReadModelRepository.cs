// <copyright file="IPortalReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Filters;

    public interface IPortalReadModelRepository : IRepository
    {
        /// <summary>
        /// Get all the portals in the repository that satisfy given parameters.
        /// </summary>
        /// <returns>All the portals in the repository that satisfy given parameters.</returns>
        /// <param name="filters">Filters to apply.</param>
        IEnumerable<PortalReadModelSummary> GetPortals(Guid tenantId, PortalListFilters filters);

        /// <summary>
        /// Find the portal with a given ID.
        /// </summary>
        PortalReadModel GetPortalById(Guid tenantId, Guid id);

        /// <summary>
        /// Retrieves a portal record in the system that satisfy the given parameters.
        /// </summary>
        PortalReadModel GetPortalByAlias(Guid tenantId, string portalAlias);

        /// <summary>
        /// Gets the portal with related entities by id.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The portal read model with related entities.</returns>
        IPortalWithRelatedEntities GetPortalWithRelatedEntities(
            Guid tenantId, Guid portalId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Gets the portal with related entities by alias.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>The portal read model with related entities.</returns>
        IPortalWithRelatedEntities GetPortalWithRelatedEntitiesByAlias(
            Guid tenantId, string portalAlias, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Method for creating IQueryable method that retrieve portals and its related entities.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="relatedEntities">The related entities to retrieve.</param>
        /// <returns>IQueryable for portals.</returns>
        IQueryable<IPortalWithRelatedEntities> CreateQueryForPortalWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities);

        /// <summary>
        /// Get all the portals by tenant in the repository that is ever created.
        /// Note: this should only be used by a migration to set the tenant guid id, nothing more.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>All the portals in the repository filtered by tenant.</returns>
        IEnumerable<PortalReadModelSummary> GetAllPortalsByTenant(Guid tenantId);

        /// <summary>
        /// Get all the portals by tenant in the repository.
        /// </summary>
        /// <param name="tenantId">The tenant Guid ID.</param>
        /// <returns>All the portals in the repository filtered by tenant.</returns>
        IEnumerable<PortalReadModelSummary> GetPortalsByTenant(Guid tenantId);

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
        /// Gets the list of portal ids.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="skip">The current page index.</param>
        /// <param name="pageSize">The max count per page.</param>
        /// <returns>List of ids.</returns>
        List<Guid> GetPortalIdsBy(Guid tenantId, int skip, int pageSize);

        /// <summary>
        /// Gets all portals within the tenant, matching a given filter.
        /// </summary>
        /// <param name="filters">Additional filters to apply.</param>
        /// <param name="relatedEntities">The list of related entities to include.</param>
        /// <returns>All the portals within the tenant matching a given filter.</returns>
        IEnumerable<IPortalWithRelatedEntities> GetPortalsWithRelatedEntities(
            Guid tenantId, EntityListFilters filters, IEnumerable<string> relatedEntities);

        Guid? GetDefaultPortalId(Guid tenantId, Guid organisationId, PortalUserType userType);

        PortalReadModel GetDefaultPortal(Guid tenantId, Guid organisationId, PortalUserType userType);

        Guid? GetFirstPortalId(Guid tenantId, Guid organisationId, PortalUserType userType);

        /// <summary>
        /// Gets the first portal for the given tenant, organisation and user type.
        /// </summary>
        PortalReadModel GetFirstPortal(Guid tenantId, Guid organisationId, PortalUserType userType);
    }
}
