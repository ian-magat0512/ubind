// <copyright file="DataTableDefinitionRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using LinqKit;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class DataTableDefinitionRepository : IDataTableDefinitionRepository
    {
        private readonly IUBindDbContext context;

        public DataTableDefinitionRepository(IUBindDbContext context)
        {
            this.context = context;
        }

        /// <inheritdoc/>
        public void CreateDataTableDefinition(DataTableDefinition dataTableDefinition)
        {
            this.context.DataTableDefinitions.Add(dataTableDefinition);
            this.context.SaveChanges();
        }

        /// <inheritdoc/>
        public async Task UpdateDatabaseTableName(DataTableDefinition dataTableDefinition, string newDatabaseTableName)
        {
            dataTableDefinition.UpdateDatabaseTableName(newDatabaseTableName);
            await this.context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public IList<DataTableDefinition> GetDataTableDefinitionsByDatabaseTableName(Guid tenantId, string databaseTableName)
        {
            var truncatedDatabaseTableName =
                databaseTableName.Length > 124 ? databaseTableName.Substring(0, 124) : databaseTableName;
            return this.context.DataTableDefinitions
                .Where(dt => dt.TenantId == tenantId && dt.IsDeleted == false
                    && dt.DatabaseTableName.Contains(truncatedDatabaseTableName))
                .OrderByDescending(dt => dt.CreatedTicksSinceEpoch)
                .ToList();
        }

        /// <inheritdoc/>
        public DataTableDefinition GetDataTableDefinitionsByEntityAndAlias(Guid tenantId, Guid entityId, EntityType entityType, string alias)
        {
            return this.context.DataTableDefinitions
                .Where(dt => dt.TenantId == tenantId && dt.IsDeleted == false
                    && dt.EntityId == entityId && dt.EntityType == entityType && dt.Alias == alias)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public bool IsNameIsInUse(Guid tenantId, Guid entityId, string name)
        {
            return this.context.DataTableDefinitions
                .Where(dt => dt.TenantId == tenantId && dt.IsDeleted == false
                    && dt.EntityId == entityId && dt.Name == name)
                .Any();
        }

        /// <inheritdoc/>
        public bool IsAliasIsInUse(Guid tenantId, Guid entityId, string alias)
        {
            return this.context.DataTableDefinitions
                .Where(dt => dt.TenantId == tenantId && dt.IsDeleted == false
                    && dt.EntityId == entityId && dt.Alias == alias)
                .Any();
        }

        /// <inheritdoc/>
        public IQueryable<DataTableDefinition> GetDataTableDefinitionsQuery(EntityListFilters filters)
        {
            var query = this.context.DataTableDefinitions
               .Where(dt => dt.TenantId == filters.TenantId && dt.IsDeleted == false
                   && dt.EntityType == filters.EntityType && dt.EntityId == filters.EntityId);

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<DataTableDefinition>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchExpression.Or(dtf =>
                        dtf.Name.Contains(searchTerm)
                        || dtf.Alias.Contains(searchTerm));
                }

                query = query.Where(searchExpression);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(Product.CreatedTicksSinceEpoch))
                {
                    query = query.Where(dtf => dtf.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
                }
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                if (filters.DateFilteringPropertyName == nameof(Product.CreatedTicksSinceEpoch))
                {
                    query = query.Where(dtf => dtf.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
                }
            }

            return query.Order(filters.SortBy, filters.SortOrder);
        }

        /// <inheritdoc/>
        public DataTableDefinition GetDataTableDefinitionById(Guid tenantId, Guid dataTableDefinitionId)
        {
            return this.context.DataTableDefinitions
                .Where(dt => dt.TenantId == tenantId && dt.IsDeleted == false
                    && dt.Id == dataTableDefinitionId)
                .FirstOrDefault();
        }

        /// <inheritdoc/>
        public List<DataTableDefinition> GetDataTableDefinitionsByTenantId(Guid tenantId)
        {
            return this.context.DataTableDefinitions
                .Where(dt => dt.TenantId == tenantId && dt.IsDeleted == false)
                .ToList();
        }

        /// <inheritdoc/>
        public List<DataTableDefinition> GetDataTableDefinitionsByOrganisationId(Guid tenantId, Guid organisationId)
        {
            return this.context.DataTableDefinitions
                .Where(dt => dt.TenantId == tenantId && dt.IsDeleted == false
                    && dt.EntityType == EntityType.Organisation && dt.EntityId == organisationId)
                .ToList();
        }

        /// <inheritdoc/>
        public List<DataTableDefinition> GetDataTableDefinitionsByProductId(Guid tenantId, Guid productId)
        {
            return this.context.DataTableDefinitions
                .Where(dt => dt.TenantId == tenantId && dt.IsDeleted == false
                    && dt.EntityType == EntityType.Product && dt.EntityId == productId)
                .ToList();
        }

        public async Task RemoveDataTableDefinition(DataTableDefinition dataTableDefinition)
        {
            dataTableDefinition.Delete();
            await this.context.SaveChangesAsync();
        }
    }
}
