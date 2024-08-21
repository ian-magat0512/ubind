// <copyright file="PortalReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Filters;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;

    public class PortalReadModelRepository : IPortalReadModelRepository
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IUBindDbContext dbContext;

        public PortalReadModelRepository(
            IUBindDbContext dbContext,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.dbContext = dbContext;
        }

        public Expression<Func<PortalReadModel, PortalReadModelSummary>> SummarySelector =>
            (p) => new PortalReadModelSummary
            {
                Id = p.Id,
                TenantId = p.TenantId,
                CreatedTicksSinceEpoch = p.CreatedTicksSinceEpoch,
                LastModifiedTicksSinceEpoch = p.LastModifiedTicksSinceEpoch,
                Name = p.Name,
                Alias = p.Alias,
                Title = p.Title,
                UserType = p.UserType,
                OrganisationId = p.OrganisationId,
                Disabled = p.Disabled,
                Deleted = p.Deleted,
                IsDefault = p.IsDefault,
            };

        public IQueryable<IPortalWithRelatedEntities> CreateQueryForPortalWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities)
        {
            var portalDatasource = this.dbContext.PortalReadModels.Where(x => x.TenantId == tenantId);
            return this.CreateQueryForPortalWithRelatedEntities(tenantId, portalDatasource, relatedEntities);
        }

        public IEnumerable<PortalReadModelSummary> GetAllPortalsByTenant(Guid tenantId)
        {
            return this.dbContext.PortalReadModels.Where(x => x.TenantId == tenantId).Select(this.SummarySelector);
        }

        public PortalReadModel GetPortalByAlias(Guid tenantId, string portalAlias)
        {
            return this.dbContext.PortalReadModels
                .SingleOrDefault(p => p.TenantId == tenantId && p.Alias == portalAlias);
        }

        public PortalReadModel GetPortalById(Guid tenantId, Guid id)
        {
            return this.dbContext.PortalReadModels.SingleOrDefault(p => p.TenantId == tenantId && p.Id == id);
        }

        public List<Guid> GetPortalIdsBy(Guid tenantId, int skip, int pageSize)
        {
            var queryable = this.dbContext.PortalReadModels.Where(p => p.TenantId == tenantId)
                .OrderByDescending(p => p.CreatedTicksSinceEpoch).Skip(skip).Take(pageSize);
            return queryable.Select(q => q.Id).ToList();
        }

        public IEnumerable<PortalReadModelSummary> GetPortals(Guid tenantId, PortalListFilters filters)
        {
            var query = this.dbContext.PortalReadModels.Where(p => p.TenantId == tenantId && !p.Deleted);
            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                query = query.Where(p => filters.OrganisationIds.Contains(p.OrganisationId));
            }

            if (filters.UserType.HasValue)
            {
                query = query.Where(p => p.UserType == filters.UserType.Value);
            }

            if (filters.Statuses.Any())
            {
                var statusExpression = PredicateBuilder.New<PortalReadModel>(false);
                foreach (var status in filters.Statuses)
                {
                    if (string.Equals(status, "Disabled", StringComparison.OrdinalIgnoreCase))
                    {
                        statusExpression.Or(portal => portal.Disabled);
                    }
                    else if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                    {
                        statusExpression.Or(portal => !portal.Disabled);
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
                var searchExpression = PredicateBuilder.New<PortalReadModel>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(p =>
                        p.Name.Contains(searchTerm) ||
                        p.Title.Contains(searchTerm) ||
                        p.Alias.Contains(searchTerm) ||
                        p.Id.ToString().Contains(searchTerm) ||
                        p.TenantId.ToString().Contains(searchTerm));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(PortalReadModel.CreatedTicksSinceEpoch))
                {
                    query = query.Where(p => p.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
                else if (filters.DateFilteringPropertyName == nameof(PortalReadModel.LastModifiedTicksSinceEpoch))
                {
                    query = query.Where(p => p.LastModifiedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
                else
                {
                    throw new ArgumentException($"Unable to filter date field {filters.DateFilteringPropertyName}. "
                        + "There is no support for filtering on this field.");
                }
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(PortalReadModel.CreatedTicksSinceEpoch))
                {
                    query = query.Where(p => p.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
                else if (filters.DateFilteringPropertyName == nameof(PortalReadModel.LastModifiedTicksSinceEpoch))
                {
                    query = query.Where(p => p.LastModifiedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
                else
                {
                    throw new ArgumentException($"Unable to filter date field {filters.DateFilteringPropertyName}. "
                        + "There is no support for filtering on this field.");
                }
            }

            if (filters.SortBy.IsNotNullOrEmpty())
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query.Select(this.SummarySelector).ToList();
        }

        public IEnumerable<PortalReadModelSummary> GetPortalsByTenant(Guid tenantId)
        {
            return this.dbContext.PortalReadModels.Where(p => p.TenantId == tenantId && !p.Disabled)
                .Select(this.SummarySelector).ToList();
        }

        public IEnumerable<IPortalWithRelatedEntities> GetPortalsWithRelatedEntities(Guid tenantId, EntityListFilters filters, IEnumerable<string> relatedEntities)
        {
            var query = this.dbContext.PortalReadModels.Where(p => p.TenantId == tenantId && !p.Disabled);
            return this.CreateQueryForPortalWithRelatedEntities(tenantId, query, relatedEntities).ToList();
        }

        public IPortalWithRelatedEntities GetPortalWithRelatedEntities(
            Guid tenantId, Guid portalId, IEnumerable<string> relatedEntities)
        {
            return this.CreateQueryForPortalWithRelatedEntities(tenantId, relatedEntities)
                .FirstOrDefault(p => p.Portal.Id == portalId);
        }

        public IPortalWithRelatedEntities GetPortalWithRelatedEntitiesByAlias(Guid tenantId, string portalAlias, IEnumerable<string> relatedEntities)
        {
            return this.CreateQueryForPortalWithRelatedEntities(tenantId, relatedEntities)
                .FirstOrDefault(p => p.Portal.Alias.EqualsIgnoreCase(portalAlias));
        }

        public bool PortalAliasExistingForTenant(Guid tenantId, string alias, Guid portalId = default)
        {
            return this.dbContext.PortalReadModels
                .Where(p => p.TenantId == tenantId && p.Id != portalId)
                .Where(p => p.Alias.Equals(alias, StringComparison.OrdinalIgnoreCase))
                .Any();
        }

        public bool PortalNameExistingForTenant(Guid tenantId, string name, Guid portalId = default)
        {
            return this.dbContext.PortalReadModels
                .Where(p => p.TenantId == tenantId && p.Id != portalId)
                .Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                .Any();
        }

        public Guid? GetDefaultPortalId(Guid tenantId, Guid organisationId, PortalUserType userType)
        {
            return this.dbContext.PortalReadModels
                .Where(p => p.TenantId == tenantId && p.OrganisationId == organisationId
                    && !p.Disabled && p.UserType == userType && p.IsDefault)
                .Select(p => (Guid?)p.Id)
                .DefaultIfEmpty(null)
                .FirstOrDefault();
        }

        public PortalReadModel GetDefaultPortal(Guid tenantId, Guid organisationId, PortalUserType userType)
        {
            return this.dbContext.PortalReadModels
                .Where(p => p.TenantId == tenantId && p.OrganisationId == organisationId
                    && !p.Disabled && p.UserType == userType && p.IsDefault)
                .FirstOrDefault();
        }

        public Guid? GetFirstPortalId(Guid tenantId, Guid organisationId, PortalUserType userType)
        {
            return this.dbContext.PortalReadModels
                .Where(p => p.TenantId == tenantId && p.OrganisationId == organisationId
                    && !p.Disabled && p.UserType == userType)
                .Select(p => (Guid?)p.Id)
                .DefaultIfEmpty(null)
                .FirstOrDefault();
        }

        public PortalReadModel GetFirstPortal(Guid tenantId, Guid organisationId, PortalUserType userType)
        {
            return this.dbContext.PortalReadModels
                .Where(p => p.TenantId == tenantId && p.OrganisationId == organisationId
                    && !p.Disabled && p.UserType == userType)
                .FirstOrDefault();
        }

        private IQueryable<IPortalWithRelatedEntities> CreateQueryForPortalWithRelatedEntities(
            Guid tenantId, IQueryable<PortalReadModel> dataSource, IEnumerable<string> relatedEntities)
        {
            var query = from portal in dataSource
                        where portal.TenantId == tenantId
                        select new PortalWithRelatedEntities()
                        {
                            Portal = portal,
                            Tenant = default,
                            Organisation = default,
                            TenantDetails = new TenantDetails[] { },
                            TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                            StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                        };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Portal.Tenant))))
            {
                query = query.Join(
                    this.dbContext.Tenants.IncludeAllProperties(),
                    e => e.Portal.TenantId,
                    t => t.Id,
                    (e, tenant) => new PortalWithRelatedEntities
                    {
                        Portal = e.Portal,
                        Tenant = tenant,
                        Organisation = e.Organisation,
                        TenantDetails = tenant.DetailsCollection,
                        TextAdditionalPropertiesValues = e.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = e.StructuredDataAdditionalPropertyValues,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Portal.Organisation))))
            {
                query = query.Join(
                    this.dbContext.OrganisationReadModel,
                    e => e.Portal.OrganisationId,
                    t => t.Id,
                    (e, organisation) => new PortalWithRelatedEntities
                    {
                        Portal = e.Portal,
                        Tenant = e.Tenant,
                        Organisation = organisation,
                        TenantDetails = e.TenantDetails,
                        TextAdditionalPropertiesValues = e.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = e.StructuredDataAdditionalPropertyValues,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Portal.AdditionalProperties))))
            {
                query = query.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    p => p.Portal.Id,
                    apv => apv.EntityId,
                    (p, apv) => new PortalWithRelatedEntities
                    {
                        Portal = p.Portal,
                        Tenant = p.Tenant,
                        TenantDetails = p.TenantDetails,
                        TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        StructuredDataAdditionalPropertyValues = p.StructuredDataAdditionalPropertyValues,
                    })
                    .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    p => p.Portal.Id,
                    apv => apv.EntityId,
                    (p, apv) => new PortalWithRelatedEntities
                    {
                        Portal = p.Portal,
                        Tenant = p.Tenant,
                        TenantDetails = p.TenantDetails,
                        TextAdditionalPropertiesValues = p.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    });
            }

            return query;
        }
    }
}
