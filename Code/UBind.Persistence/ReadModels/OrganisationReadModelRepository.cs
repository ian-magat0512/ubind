// <copyright file="OrganisationReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using Dapper;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    /// <summary>
    /// The repository for organisation read models.
    /// </summary>
    public class OrganisationReadModelRepository : IOrganisationReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganisationReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public OrganisationReadModelRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets an expression for instantiating summaries from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<OrganisationReadModel, IOrganisationReadModelSummary>> SummarySelector =>
            org => new OrganisationReadModelSummary
            {
                TenantId = org.TenantId,
                Id = org.Id,
                Alias = org.Alias,
                Name = org.Name,
                IsActive = org.IsActive,
                IsDeleted = org.IsDeleted,
                IsDefault = org.IsDefault,
                CreatedTicksSinceEpoch = org.CreatedTicksSinceEpoch,
                LastModifiedTicksSinceEpoch = org.LastModifiedTicksSinceEpoch,
            };

        /// <inheritdoc/>
        public IEnumerable<OrganisationReadModel> Get(Guid tenantId, OrganisationReadModelFilters filters)
        {
            return this.CreateQueryForFilters(tenantId, filters).AsEnumerable();
        }

        public IReadOnlyList<IOrganisationReadModelSummary> GetSummaries(Guid tenantId, OrganisationReadModelFilters filters)
        {
            return this.CreateQueryForFilters(tenantId, filters)
                .Select(this.SummarySelector)
                .ToList().AsReadOnly();
        }

        public List<Guid> GetIds(Guid tenantId, OrganisationReadModelFilters filter)
        {
            return this.CreateQueryForFilters(tenantId, filter)
                .Select(org => org.Id)
                .ToList();
        }

        /// <inheritdoc/>
        public OrganisationReadModel? Get(Guid tenantId, Guid organisationId)
        {
            return this.dbContext.OrganisationReadModel
                .Where(o => o.TenantId == tenantId && o.Id == organisationId && !o.IsDeleted)
                .Include(o => o.LinkedIdentities)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public OrganisationReadModel? GetByAlias(Guid tenantId, string alias)
        {
            return this.dbContext.OrganisationReadModel
                .Where(o => o.Alias == alias && o.TenantId == tenantId && !o.IsDeleted)
                .Include(o => o.LinkedIdentities)
                .FirstOrDefault();
        }

        public Guid GetIdForAlias(Guid tenantId, string alias)
        {
            return this.dbContext.OrganisationReadModel
                .Where(o => o.Alias == alias && o.TenantId == tenantId && !o.IsDeleted)
                .Select(org => org.Id).FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<string?> GetOrganisationAliasById(Guid tenantId, Guid organisationId)
        {
            return await this.dbContext.OrganisationReadModel
                .Where(o => o.TenantId == tenantId && o.Id == organisationId && !o.IsDeleted)
                .OrderByDescending(o => o.CreatedTicksSinceEpoch)
                .Select(o => o.Alias)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public IEnumerable<OrganisationReadModel> GetOrganisations(Guid tenantId)
        {
            return this.dbContext.OrganisationReadModel.Where(o => o.TenantId == tenantId && !o.IsDeleted);
        }

        /// <inheritdoc/>
        public IOrganisationReadModelWithRelatedEntities? GetOrganisationWithRelatedEntities(
            Guid tenantId, Guid organisationId, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForOrganisationDetailsWithRelatedEntities(tenantId, relatedEntities);
            return query.FirstOrDefault(c => c.Organisation.Id == organisationId);
        }

        /// <inheritdoc/>
        public IOrganisationReadModelWithRelatedEntities? GetOrganisationWithRelatedEntities(
            Guid tenantId, string organisationAlias, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForOrganisationDetailsWithRelatedEntities(tenantId, relatedEntities);
            return query.FirstOrDefault(o => o.Organisation.Alias == organisationAlias);
        }

        /// <inheritdoc/>
        public List<Guid> GetOrganisationIdsByTenant(Guid tenantId, int skip, int pageSize)
        {
            return this.dbContext.OrganisationReadModel.Where(orm => orm.TenantId == tenantId && !orm.IsDeleted)
                .OrderByDescending(orm => orm.CreatedTicksSinceEpoch)
                .Skip(skip)
                .Take(pageSize)
                .Select(orm => orm.Id).ToList();
        }

        /// <inheritdoc/>
        public bool IsAliasInUse(Guid tenantId, string alias, Guid? organisationId = null)
        {
            var query = this.dbContext.OrganisationReadModel
                .Where(o => o.TenantId == tenantId);
            if (organisationId.HasValue)
            {
                query = query.Where(o => o.Id != organisationId);
            }

            bool inUse = query.Where(o => !o.IsDeleted)
                .Any(o => o.Alias.Equals(alias, StringComparison.CurrentCultureIgnoreCase));
            return inUse;
        }

        public async Task<IEnumerable<Guid>> GetIdsOfDescendantOrganisationsOfOrganisation(Guid tenantId, Guid organisationId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@OrganisationId", organisationId);
            parameters.Add("@TenantId", tenantId);

            // It would be very expensive to do this in EF (LINQ) since it will take multiple database transactions (depending on the
            // depth) to do recursive queries. As per GPT, this would be the ideal way to do this in one single database transaction.
            var sql = @"
                WITH RecursiveCTE AS (
                    SELECT Id, ManagingOrganisationId
                    FROM OrganisationReadModels
                    WHERE Id = @OrganisationId AND TenantId = @TenantId
                    UNION ALL
                    SELECT i.Id, i.ManagingOrganisationId
                    FROM OrganisationReadModels AS i
                    INNER JOIN RecursiveCTE AS cte ON i.ManagingOrganisationId = cte.Id
                )
                SELECT Id
                FROM RecursiveCTE WHERE Id != @OrganisationId";

            var descendants = (await this.dbContext.Database.Connection.QueryAsync<Guid>(
                    sql, parameters, commandTimeout: 180, commandType: System.Data.CommandType.Text))?.ToList();
            return descendants ?? new List<Guid>();
        }

        /// <inheritdoc/>
        public bool IsNameInUse(Guid tenantId, string name, Guid? organisationId)
        {
            var query = this.dbContext.OrganisationReadModel
                .Where(o => o.TenantId == tenantId);
            if (organisationId.HasValue)
            {
                query = query.Where(o => o.Id != organisationId);
            }

            bool inUse = query.Where(o => !o.IsDeleted)
                .Any(o => o.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            return inUse;
        }

        /// <inheritdoc/>
        public IQueryable<OrganisationReadModelWithRelatedEntities> CreateQueryForOrganisationDetailsWithRelatedEntities(
            Guid tenantId, IEnumerable<string> relatedEntities)
        {
            var orgDatasource = this.dbContext.OrganisationReadModel.Where(x => x.TenantId == tenantId && !x.IsDeleted);
            return this.CreateQueryForOrganisationDetailsWithRelatedEntities(orgDatasource, relatedEntities);
        }

        public IEnumerable<OrganisationReadModelWithRelatedEntities> GetOrganisationsWithRelatedEntities(
            Guid tenantId, OrganisationReadModelFilters filters, IEnumerable<string> relatedEntities)
        {
            var organisations = this.Get(tenantId, filters).AsQueryable();
            return this.CreateQueryForOrganisationDetailsWithRelatedEntities(organisations, relatedEntities);
        }

        /// <summary>
        /// Retrieves the organisation linked based on the given tenant, authentication method and external id.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="authenticationMethodId"></param>
        /// <param name="organisationExternalId"></param>
        /// <returns>Throws an error when there are more than one linked organisation</returns>
        public OrganisationReadModel? GetLinkedOrganisation(Guid tenantId, Guid authenticationMethodId, string organisationExternalId, Guid? excludeOrganisationId = null)
        {
            var query = this.dbContext.OrganisationReadModel
                .Join(
                    this.dbContext.OrganisationLinkedIdentities,
                    o => o.Id,
                    oli => oli.OrganisationId,
                    (o, oli) => new { Organisation = o, OrganisationLinkedIdentity = oli })
                .Where(t => t.OrganisationLinkedIdentity.TenantId == tenantId
                    && t.Organisation.TenantId == tenantId
                    && t.OrganisationLinkedIdentity.AuthenticationMethodId == authenticationMethodId
                    && t.OrganisationLinkedIdentity.UniqueId == organisationExternalId
                    && !t.Organisation.IsDeleted);
            if (excludeOrganisationId.HasValue)
            {
                query = query.Where(t => t.Organisation.Id != excludeOrganisationId.Value);
            }
            return query.Select(t => t.Organisation).FirstOrDefault();
        }

        private IQueryable<OrganisationReadModelWithRelatedEntities> CreateQueryForOrganisationDetailsWithRelatedEntities(
            IQueryable<OrganisationReadModel> dataSource, IEnumerable<string> relatedEntities)
        {
            var query = dataSource.Select(organisation => new OrganisationReadModelWithRelatedEntities()
            {
                Tenant = null,
                TenantDetails = new TenantDetails[] { },
                Organisation = organisation,
                TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
            });

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Organisation.Tenant))))
            {
                query = query.Join(this.dbContext.Tenants.IncludeAllProperties(), e => e.Organisation.TenantId, t => t.Id, (e, tenant) => new OrganisationReadModelWithRelatedEntities
                {
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Organisation = e.Organisation,
                    TextAdditionalPropertiesValues = e.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = e.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Organisation.AdditionalProperties))))
            {
                query = query.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    o => o.Organisation.Id,
                    apv => apv.EntityId,
                    (o, apv) => new OrganisationReadModelWithRelatedEntities
                    {
                        Tenant = o.Tenant,
                        TenantDetails = o.TenantDetails,
                        Organisation = o.Organisation,
                        TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        StructuredDataAdditionalPropertyValues = o.StructuredDataAdditionalPropertyValues,
                    })
                    .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    o => o.Organisation.Id,
                    apv => apv.EntityId,
                    (o, apv) => new OrganisationReadModelWithRelatedEntities
                    {
                        Tenant = o.Tenant,
                        TenantDetails = o.TenantDetails,
                        Organisation = o.Organisation,
                        TextAdditionalPropertiesValues = o.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    });
            }

            return query;
        }

        private Expression<Func<OrganisationReadModel, bool>> GetStatusExpression(string status)
        {
            if (status.ToLower() == "active")
            {
                return organisation => organisation.IsActive;
            }

            if (status.ToLower() == "disabled")
            {
                return organisation => !organisation.IsActive;
            }

            throw new InvalidOperationException("Unknown organisation status " + status.ToString());
        }

        private IQueryable<OrganisationReadModel> CreateQueryForFilters(Guid tenantId, OrganisationReadModelFilters filters)
        {
            var query = this.dbContext.OrganisationReadModel.Where(o => !o.IsDeleted);
            if (filters == null)
            {
                return query;
            }

            foreach (var name in filters.Names)
            {
                query = query.Where(a => a.Name.ToLower() == name.ToLower());
            }

            foreach (var alias in filters.Aliases)
            {
                query = query.Where(a => a.Alias.ToLower() == alias.ToLower());
            }

            if (filters.OrganisationIds != null && filters.OrganisationIds.Any())
            {
                query = query.Where(a => filters.OrganisationIds.Contains(a.Id));
            }

            if (filters.ManagingOrganisationId != null)
            {
                query = query.Where(a => a.ManagingOrganisationId == filters.ManagingOrganisationId);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<OrganisationReadModel>(false);
                foreach (var search in filters.SearchTerms)
                {
                    searchExpression.Or(p => p.Name.Contains(search) || p.Alias.Contains(search));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<OrganisationReadModel>(
                    filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<OrganisationReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<OrganisationReadModel>(false);
                foreach (var status in filters.Statuses)
                {
                    statusPredicate = statusPredicate.Or(this.GetStatusExpression(status));
                }

                query = query.Where(statusPredicate);
            }

            query = query.Where(organisation => organisation.TenantId == tenantId);

            if (filters.SortBy != null)
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            if (filters.Page.HasValue || filters.PageSize.HasValue)
            {
                query = query.Paginate(filters);
            }
            return query;
        }
    }
}
