// <copyright file="PortalRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    [Obsolete("Will be removed in ticket UB-9510")]
    public class PortalRepository : IPortalRepository
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public PortalRepository(
            IUBindDbContext dbContext,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IEnumerable<Portal> GetPortals(Guid tenantId, EntityListFilters filters)
        {
            var query = this.GetPortals(tenantId);
            bool hasOrganisationFilters = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilters)
            {
                query = query.Where(p => filters.OrganisationIds.Contains(p.OrganisationId));
            }

            if (filters.Statuses.Any())
            {
                var statusExpression = PredicateBuilder.New<Portal>(false);
                foreach (var status in filters.Statuses)
                {
                    if (string.Equals(status, "Disabled", StringComparison.OrdinalIgnoreCase))
                    {
                        statusExpression.Or(portal => portal.Details.Disabled || portal.Tenant.Details.Disabled);
                    }
                    else if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                    {
                        statusExpression.Or(portal => !portal.Details.Disabled);
                    }
                    else
                    {
                        throw new InvalidOperationException($"{status} is not a valid portal status (must be 'Active' or 'Disabled')");
                    }
                }

                query = query.Where(statusExpression);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<Portal>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(p =>
                        p.Details.Name.ToLower().Contains(searchTerm) ||
                        p.Details.Title.ToLower().Contains(searchTerm) ||
                        p.Details.Alias.ToLower().Contains(searchTerm) ||
                        p.Id.ToString().Contains(searchTerm) ||
                        p.Tenant.Id.ToString().Contains(searchTerm));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(Portal.CreatedTicksSinceEpoch))
                {
                    query = query.Where(p => p.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
                else
                {
                    query = query.Where(p => p.Details.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(Portal.CreatedTicksSinceEpoch))
                {
                    query = query.Where(p => p.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
                else
                {
                    query = query.Where(p => p.Details.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
            }

            return query.ToList();
        }

        /// <inheritdoc/>
        public Portal GetPortalById(Guid tenantId, Guid id)
        {
            return this.GetPortals(tenantId).SingleOrDefault(t => t.Id == id);
        }

        public Portal GetPortalByIdWithoutTenantIdForMigrations(Guid id)
        {
            return this.dbContext.Portals.Where(p => p.Id == id).FirstOrDefault();
        }

        /// <inheritdoc/>
        public Portal GetPortalByAlias(Guid tenantId, string portalAlias)
        {
            return this.GetPortals(tenantId).SingleOrDefault(t => t.Details.Alias == portalAlias);
        }

        /// <inheritdoc/>
        public IEnumerable<Portal> GetAllPortalsByTenant(Guid tenantId)
        {
            return this.dbContext.Portals.IncludeAllProperties().Where(x => x.Tenant.Id == tenantId);
        }

        /// <inheritdoc/>
        public IEnumerable<Portal> GetPortalsByTenant(Guid tenantId)
        {
            return this.GetPortals(tenantId).Where(x => x.Tenant.Id == tenantId);
        }

        /// <inheritdoc/>
        public void AddPortal(Portal portal)
        {
            if (this.GetPortals(portal.Tenant.Id).Any(t => t.Id == portal.Id))
            {
                throw new InvalidOperationException("Duplicate portal ID.");
            }

            this.dbContext.Portals.Add(portal);
        }

        /// <inheritdoc/>
        public bool PortalNameExistingForTenant(Guid tenantId, string name, Guid portalId = default)
        {
            return this.GetPortals(tenantId)
                .Where(p => p.Id != portalId)
                .Where(p => p.Details.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Any();
        }

        /// <inheritdoc/>
        public bool PortalAliasExistingForTenant(Guid tenantId, string alias, Guid portalId = default)
        {
            return this.GetPortals(tenantId)
                .Where(p => p.Id != portalId)
                .Where(p => p.Details.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase))
                .Any();
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public List<Guid> GetPortalIdsBy(Guid tenantId, int skip, int pageSize)
        {
            var queryable = this.dbContext.Portals.Include("Tenant").Where(p => p.Tenant.Id == tenantId)
                .OrderByDescending(p => p.CreatedTicksSinceEpoch).Skip(skip).Take(pageSize);
            return queryable.Select(q => q.Id).ToList();
        }

        /// <summary>
        /// Sets the tenant's default organanisation id on portals.
        /// This is needed because we are addding organisation ID to portals
        /// and so we need to set an initial value. Since all portals were only
        /// against the tenant previously, it's fine to set the organisation ID
        /// to the default.
        /// </summary>
        public void SetOrganisationIdOnPortalsForTenantWhenEmpty(Guid tenantId, Guid organisationId)
        {
            var updateSql = "UPDATE dbo.Portals "
                + $"SET OrganisationId = '{organisationId}' "
                + $"WHERE Tenant_Id = '{tenantId}' "
                + $"AND OrganisationId = '{Guid.Empty}'";
            this.dbContext.Database.ExecuteSqlCommand(updateSql);
        }

        private IEnumerable<Portal> GetPortals(Guid tenantId)
        {
            var tenants = this.dbContext.Tenants.IncludeAllProperties().Where(t => t.Id == tenantId);
            var portals = this.dbContext.Portals.IncludeAllProperties().Where(p => p.Tenant.Id == tenantId);

            foreach (var portal in portals)
            {
                portal.Tenant.DetailsCollection = tenants.FirstOrDefault(x => x.Id == portal.Tenant.Id)?.DetailsCollection;
            }

            return portals
                .ToList()
                .Where(portal =>
                !portal.Details.Deleted &&
                !portal.Tenant.Details.Deleted);
        }
    }
}
