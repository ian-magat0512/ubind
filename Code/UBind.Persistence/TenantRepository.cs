// <copyright file="TenantRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Temporary in-memory repository for use during UI development.
    /// </summary>
    public class TenantRepository : ITenantRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public TenantRepository(
            IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public List<Tenant> GetTenants(EntityListFilters filters = null, bool includeMasterTenant = true)
        {
            var query = this.dbContext.Tenants
                .IncludeAllProperties();

            if (!includeMasterTenant)
            {
                query = query.Where(t => t.Id != Tenant.MasterTenantId);
            }

            if (filters != null)
            {
                query = this.FilterTenants(query, filters);
            }

            return query.ToList();
        }

        /// <inheritdoc/>
        public List<Tenant> GetActiveTenants(EntityListFilters filters = null, bool includeMasterTenant = true)
        {
            var query = this.GetTenantsQuery(false);

            if (!includeMasterTenant)
            {
                query = query.Where(t => t.Id != Tenant.MasterTenantId);
            }

            if (filters != null)
            {
                query = this.FilterTenants(query, filters);
            }

            return query.Paginate(filters).ToList();
        }

        /// <inheritdoc/>
        public ITenantWithRelatedEntities GetTenantWithRelatedEntitiesById(
            Guid tenantId, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForTenantWithRelatedEntities(tenantId, relatedEntities);
            return query.FirstOrDefault(c => c.Tenant.Id == tenantId);
        }

        /// <inheritdoc/>
        public ITenantWithRelatedEntities GetTenantWithRelatedEntitiesByAlias(
            Guid tenantId, string alias, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForTenantWithRelatedEntities(tenantId, relatedEntities);
            return query.FirstOrDefault(t => string.Equals(t.Tenant.Details.Alias, alias, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public Tenant? GetTenantById(Guid tenantId)
        {
            return this.dbContext.Tenants.IncludeAllProperties().SingleOrDefault(t => t.Id == tenantId);
        }

        /// <inheritdoc/>
        public async Task<string?> GetTenantAliasById(Guid tenantId)
        {
            return await this.dbContext.Tenants
                .IncludeAllProperties()
                .Where(t => t.Id == tenantId)
                .Select(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Alias).FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public Tenant GetTenantByAlias(string alias, bool includeDeleted = false)
        {
            return this.GetTenantsQuery(includeDeleted)
                .Where(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Alias == alias).FirstOrDefault();
        }

        /// <inheritdoc/>
        public Tenant GetTenantByStringId(string tenantStringId)
        {
            var tenants = this.GetTenantsQuery(true).OrderBy(x => x.CreatedTicksSinceEpoch).ToList();
            return tenants.FirstOrDefault(t => string.Equals(t.GetStringId(), tenantStringId, StringComparison.OrdinalIgnoreCase));
        }

        /// <inheritdoc/>
        public Tenant GetTenantByCustomDomain(string customDomain)
        {
            if (customDomain.IsNullOrEmpty())
            {
                return null;
            }

            return this.GetTenantsQuery(false)
                .Where(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().CustomDomain == customDomain).FirstOrDefault();
        }

        /// <inheritdoc/>
        public bool TenantIdWasDeleted(Guid tenantId)
        {
            var tenant = this.dbContext.Tenants
                .Where(t => t.Id == tenantId)
                .Take(1)
                .Include(t => t.DetailsCollection)
                .FirstOrDefault();

            return tenant != null && tenant.Details.Deleted;
        }

        /// <inheritdoc/>
        public void Insert(Tenant tenant)
        {
            var existingTenant = this.GetTenantByAlias(tenant.Details.Alias);
            if (existingTenant != null)
            {
                throw new ErrorException(Errors.Tenant.AliasInUse(tenant.Details.Alias, existingTenant.Details.Name));
            }

            this.dbContext.Tenants.Add(tenant);
        }

        /// <inheritdoc/>
        public TenantDetails GetLatestDetail(Guid tenantId)
        {
            var tenant = this.dbContext.Tenants
                .IncludeAllProperties()
                .FirstOrDefault(t => t.Id == tenantId);

            if (tenant != null)
            {
                return tenant.Details;
            }

            return null;
        }

        /// <inheritdoc/>
        public bool IsNameInUse(string name)
        {
            return this.GetTenantsQuery(false)
                .Any(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <inheritdoc/>
        public bool IsAliasInUse(string alias)
        {
            return this.GetTenantsQuery(false)
                    .Any(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                        .FirstOrDefault().Alias.Equals(alias, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <inheritdoc/>
        public bool IsCustomDomainInUse(string customDomainName)
        {
            return this.GetTenantsQuery(false)
                .Any(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().CustomDomain.Equals(customDomainName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public IQueryable<TenantWithRelatedEntities> CreateQueryForTenantWithRelatedEntities(
            Guid tenantId, IEnumerable<string> includedProperties)
        {
            var query = from tenant in this.dbContext.Tenants.IncludeAllProperties()
                        where tenant.Id == tenantId
                        select new TenantWithRelatedEntities()
                        {
                            Tenant = tenant,
                            Details = tenant.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                                .FirstOrDefault(),
                            DefaultOrganisation = default,
                            TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                            StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                        };

            if (includedProperties?.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Tenant.DefaultOrganisation))) == true)
            {
                query = query.Join(
                    this.dbContext.OrganisationReadModel,
                    c => c.Details.DefaultOrganisationId,
                    t => t.Id,
                    (c, organisation) => new TenantWithRelatedEntities
                    {
                        Tenant = c.Tenant,
                        Details = c.Details,
                        DefaultOrganisation = organisation,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                    });
            }

            if (includedProperties?.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Tenant.AdditionalProperties))) == true)
            {
                query = query.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    t => t.Tenant.Id,
                    apv => apv.EntityId,
                    (t, apv) => new TenantWithRelatedEntities
                    {
                        Tenant = t.Tenant,
                        Details = t.Details,
                        DefaultOrganisation = t.DefaultOrganisation,
                        TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        StructuredDataAdditionalPropertyValues = t.StructuredDataAdditionalPropertyValues,
                    })
                    .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    t => t.Tenant.Id,
                    apv => apv.EntityId,
                    (t, apv) => new TenantWithRelatedEntities
                    {
                        Tenant = t.Tenant,
                        Details = t.Details,
                        DefaultOrganisation = t.DefaultOrganisation,
                        TextAdditionalPropertiesValues = t.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    });
            }

            return query;
        }

        private IQueryable<Tenant> GetTenantsQuery(bool includeDeleted = false)
        {
            var tenants = this.dbContext.Tenants
                .IncludeAllProperties();
            if (includeDeleted)
            {
                return tenants;
            }
            else
            {
                return tenants.Where(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                        .FirstOrDefault().Deleted == false);
            }
        }

        private Expression<Func<Tenant, bool>> GetStatusExpression(string status)
        {
            if (status.ToLowerInvariant() == "active")
            {
                return tenant => !tenant.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Disabled;
            }

            if (status.ToLowerInvariant() == "disabled")
            {
                return tenant => tenant.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Disabled;
            }

            throw new InvalidOperationException("Unknown tenant status " + status.ToString());
        }

        private IQueryable<Tenant> FilterTenants(IQueryable<Tenant> query, EntityListFilters filters)
        {
            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<Tenant>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Name.IndexOf(searchTerm) >= 0 ||
                        t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Alias.IndexOf(searchTerm) >= 0);
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(Tenant.CreatedTicksSinceEpoch))
                {
                    query = query.Where(t => t.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
                else
                {
                    query = query.Where(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                        .FirstOrDefault().CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(Tenant.CreatedTicksSinceEpoch))
                {
                    query = query.Where(t => t.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
                else
                {
                    query = query.Where(t => t.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                        .FirstOrDefault().CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<Tenant>(false);
                foreach (var status in filters.Statuses)
                {
                    statusPredicate = statusPredicate.Or(this.GetStatusExpression(status));
                }

                query = query.Where(statusPredicate);
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                // We can't use this since if we pass Details.Name as the sort by param, Details can't be resolved
                // because it's a dynamic property.
                // This will no longer be an issue if we switch to using a TenantReadModel, which we plan to do.
                // https://jira.aptiture.com/browse/UB-8687
                /*
                query = query.Order(filters.SortBy, filters.SortOrder);
                */

                query = query.OrderBy(p => p.DetailsCollection.OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Name);
            }

            return query;
        }
    }
}
