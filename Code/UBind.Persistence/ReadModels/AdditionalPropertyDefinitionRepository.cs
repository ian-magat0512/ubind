// <copyright file="AdditionalPropertyDefinitionRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using LinqKit;
    using UBind.Domain.Attributes;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Read repository for additional property definition.
    /// </summary>
    public class AdditionalPropertyDefinitionRepository : IAdditionalPropertyDefinitionRepository
    {
        private readonly IUBindDbContext uBindDbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionRepository"/> class.
        /// </summary>
        /// <param name="uBindDbContext">db context for ubind database.</param>
        public AdditionalPropertyDefinitionRepository(IUBindDbContext uBindDbContext)
        {
            this.uBindDbContext = uBindDbContext;
        }

        /// <inheritdoc/>
        public async Task<AdditionalPropertyDefinitionReadModel> GetById(Guid tenantId, Guid id)
        {
            var result = await Task.FromResult(this.uBindDbContext.AdditionalPropertyDefinitions.
                FirstOrDefault(apd => apd.TenantId == tenantId && apd.Id == id));
            return result;
        }

        /// <inheritdoc/>
        public IQueryable<AdditionalPropertyDefinitionReadModel> GetByContextAndEntityTypesAndContextIdsAsQueryable(
            Guid tenantId,
            AdditionalPropertyDefinitionContextType contextType,
            AdditionalPropertyEntityType entityType,
            Guid contextId,
            Guid? parentContextId = null)
        {
            var query = this.GetQueryableForActiveRecord(
                tenantId,
                null,
                null,
                contextId,
                entityType,
                contextType,
                parentContextId);

            return query;
        }

        /// <inheritdoc/>
        public Task<List<AdditionalPropertyDefinitionReadModel>> GetByModelFilter(
            Guid tenantId,
            AdditionalPropertyDefinitionReadModelFilters readModelFilters)
        {
            var queryable = this.GetQueryableForActiveRecord(
                tenantId,
                readModelFilters.Name,
                readModelFilters.Aliases,
                readModelFilters.ContextId,
                readModelFilters.Entity,
                readModelFilters.ContextType,
                readModelFilters.ParentContextId);
            return Task.FromResult(queryable.ToList());
        }

        /// <inheritdoc/>
        public IQueryable<AdditionalPropertyDefinitionReadModel>
            GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                Guid tenantId,
                Guid mainContextId,
                AdditionalPropertyEntityType entityType)
        {
            var query = this.GetByEntityTypeQuery(tenantId, entityType)
                .Where(apdf => apdf.ContextId == mainContextId || apdf.ParentContextId == mainContextId);

            return query;
        }

        public IQueryable<AdditionalPropertyDefinitionReadModel> GetByEntityTypeAndPropertyTypeQuery(
                Guid tenantId,
                AdditionalPropertyEntityType entityType,
                AdditionalPropertyDefinitionType propertyType)
        {
            var query = this.GetByEntityTypeQuery(tenantId, entityType)
                .Where(apdf => apdf.PropertyType == propertyType);

            return query;
        }

        public IQueryable<AdditionalPropertyDefinitionReadModel>
            GetByEntityTypeQuery(
                Guid tenantId,
                AdditionalPropertyEntityType entityType)
        {
            var query = this.uBindDbContext.AdditionalPropertyDefinitions
                .Where(apdf => apdf.TenantId == tenantId && !apdf.IsDeleted);

            if (entityType != AdditionalPropertyEntityType.None)
            {
                if (this.IsQuoteType(entityType))
                {
                    query = this.FilterQuoteType(query, entityType);
                }
                else if (this.IsPolicyTransactionType(entityType))
                {
                    query = this.FilterPolicyTransactionType(query, entityType);
                }
                else
                {
                    query = query.Where(apdf => apdf.EntityType == entityType);
                }
            }

            return query;
        }

        /// <inheritdoc/>
        public Task<List<AdditionalPropertyDefinitionReadModel>> GetAdditionalPropertiesByExpression(
            Guid tenantId,
            Expression<Func<AdditionalPropertyDefinitionReadModel, bool>> customExpression)
        {
            var query = this.uBindDbContext.AdditionalPropertyDefinitions.Where(customExpression);
            return Task.FromResult(query.ToList());
        }

        /// <inheritdoc/>
        public bool DoesEntityAdditionalPropertyDefinitionsContainPropertyAlias(
            Guid tenantId,
            string propertyAlias,
            AdditionalPropertyEntityType additionalPropertyEntityType)
        {
            return this.uBindDbContext.AdditionalPropertyDefinitions
                .Where(apd => apd.EntityType == additionalPropertyEntityType)
                .Where(apd => apd.TenantId == tenantId)
                .Where(apd => apd.Alias == propertyAlias)
                .Where(apd => !apd.IsDeleted)
                .Any();
        }

        /// <inheritdoc/>
        public AdditionalPropertyDefinitionReadModel? GetAdditionalPropertyDefinitionByEntityTypeAndPropertyAlias(
            Guid tenantId,
            string propertyAlias,
            AdditionalPropertyEntityType additionalPropertyEntityType)
        {
            return this.uBindDbContext.AdditionalPropertyDefinitions
                .Where(apd => apd.EntityType == additionalPropertyEntityType)
                .Where(apd => apd.TenantId == tenantId)
                .Where(apd => apd.Alias == propertyAlias)
                .Where(apd => !apd.IsDeleted)
                .FirstOrDefault();
        }

        private IQueryable<AdditionalPropertyDefinitionReadModel> GetQueryableForActiveRecord(
            Guid tenantId,
            string name,
            IEnumerable<string> aliases,
            Guid? contextId,
            AdditionalPropertyEntityType? entityType,
            AdditionalPropertyDefinitionContextType? contextType,
            Guid? parentContextId)
        {
            var queryable = this.uBindDbContext.AdditionalPropertyDefinitions
                .Where(apd => apd.TenantId == tenantId && !apd.IsDeleted);

            if (!string.IsNullOrEmpty(name))
            {
                queryable = queryable.Where(apd => apd.Name == name);
            }

            if (aliases != null && aliases.Any(x => !string.IsNullOrEmpty(x)))
            {
                var predicate = PredicateBuilder.New<AdditionalPropertyDefinitionReadModel>(false);
                foreach (var alias in aliases)
                {
                    if (!string.IsNullOrEmpty(alias))
                    {
                        predicate = predicate.Or(x => x.Alias == alias);
                    }
                }

                queryable = queryable.Where(predicate);
            }

            if (contextId.HasValue)
            {
                queryable = queryable.Where(apd => apd.ContextId == contextId);
            }

            if (entityType.HasValue)
            {
                queryable = queryable.Where(apd => apd.EntityType == entityType.Value);
            }

            if (contextType.HasValue)
            {
                queryable = queryable.Where(apd => apd.ContextType == contextType.Value);
            }

            if (parentContextId.HasValue)
            {
                queryable = queryable.Where(apd => apd.ParentContextId == parentContextId);
            }

            return queryable.OrderBy(apd => apd.Name);
        }

        private bool IsQuoteType(AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            return category?.Category == AdditionalPropertyEntityTypeCategory.Quote;
        }

        private bool IsPolicyTransactionType(AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            return category?.Category == AdditionalPropertyEntityTypeCategory.PolicyTransaction;
        }

        private IQueryable<AdditionalPropertyDefinitionReadModel> FilterQuoteType(IQueryable<AdditionalPropertyDefinitionReadModel> query, AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            if (category?.Category == AdditionalPropertyEntityTypeCategory.Quote && entityType == AdditionalPropertyEntityType.Quote)
            {
                query = query.Where(apdf => apdf.EntityType == AdditionalPropertyEntityType.Quote || apdf.EntityType == AdditionalPropertyEntityType.NewBusinessQuote
                || apdf.EntityType == AdditionalPropertyEntityType.AdjustmentQuote || apdf.EntityType == AdditionalPropertyEntityType.RenewalQuote
                || apdf.EntityType == AdditionalPropertyEntityType.CancellationQuote);
            }

            if (category?.Category == AdditionalPropertyEntityTypeCategory.Quote && entityType != AdditionalPropertyEntityType.Quote)
            {
                query = query.Where(apdf => apdf.EntityType == entityType || apdf.EntityType == AdditionalPropertyEntityType.Quote);
            }

            return query;
        }

        private IQueryable<AdditionalPropertyDefinitionReadModel> FilterPolicyTransactionType(IQueryable<AdditionalPropertyDefinitionReadModel> query, AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            if (category?.Category == AdditionalPropertyEntityTypeCategory.PolicyTransaction && entityType == AdditionalPropertyEntityType.PolicyTransaction)
            {
                query = query.Where(apdf => apdf.EntityType == AdditionalPropertyEntityType.PolicyTransaction
                || apdf.EntityType == AdditionalPropertyEntityType.NewBusinessPolicyTransaction || apdf.EntityType == AdditionalPropertyEntityType.AdjustmentPolicyTransaction
                || apdf.EntityType == AdditionalPropertyEntityType.RenewalPolicyTransaction || apdf.EntityType == AdditionalPropertyEntityType.CancellationPolicyTransaction);
            }

            if (category?.Category == AdditionalPropertyEntityTypeCategory.PolicyTransaction && entityType != AdditionalPropertyEntityType.PolicyTransaction)
            {
                query = query.Where(apdf => apdf.EntityType == entityType || apdf.EntityType == AdditionalPropertyEntityType.PolicyTransaction);
            }

            return query;
        }
    }
}
