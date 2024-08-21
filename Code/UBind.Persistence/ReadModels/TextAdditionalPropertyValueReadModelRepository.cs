// <copyright file="TextAdditionalPropertyValueReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using MoreLinq;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Queries.AdditionalPropertyValue;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.AdditionalPropertyDefinition;

    /// <summary>
    /// Repository for managing text additional property values in the read model.
    /// This repository provides methods for retrieving, updating, and checking the uniqueness of additional property values.
    /// </summary>
    public class TextAdditionalPropertyValueReadModelRepository : ITextAdditionalPropertyValueReadModelRepository
    {
        private readonly IUBindDbContext context;
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;
        private readonly IAdditionalPropertyDefinitionFilterResolver additionalPropertyDefinitionFilterResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextAdditionalPropertyValueReadModelRepository"/> class.
        /// </summary>
        /// <param name="context">UBind db context.</param>
        public TextAdditionalPropertyValueReadModelRepository(
            IUBindDbContext context,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository,
            IAdditionalPropertyDefinitionFilterResolver additionalPropertyDefinitionFilterResolver)
        {
            this.context = context;
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
            this.additionalPropertyDefinitionFilterResolver = additionalPropertyDefinitionFilterResolver;
        }

        private Expression<Func<TextAdditionalPropertyValueReadModel, AdditionalPropertyValueDto>> Selector =>
          (tapv) => new AdditionalPropertyValueDto
          {
              Value = tapv.Value,
              Id = tapv.Id,
              EntityId = tapv.EntityId,
              AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
              {
                  Alias = tapv.AdditionalPropertyDefinition.Alias,
                  ContextId = tapv.AdditionalPropertyDefinition.ContextId,
                  ContextType = tapv.AdditionalPropertyDefinition.ContextType,
                  EntityType = tapv.AdditionalPropertyDefinition.EntityType,
                  Id = tapv.AdditionalPropertyDefinition.Id,
                  IsRequired = tapv.AdditionalPropertyDefinition.IsRequired,
                  IsUnique = tapv.AdditionalPropertyDefinition.IsUnique,
                  Name = tapv.AdditionalPropertyDefinition.Name,
                  ParentContextId = tapv.AdditionalPropertyDefinition.ParentContextId,
                  PropertyType = tapv.AdditionalPropertyDefinition.PropertyType,
              },
          };

        /// <inheritdoc/>
        public Task<AdditionalPropertyValueDto> GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefintionId)
        {
            var dto = this.Select(tenantId, AdditionalPropertyEntityType.None, entityId, additionalPropertyDefintionId, string.Empty).FirstOrDefault();
            return Task.FromResult(dto);
        }

        /// <inheritdoc/>
        public Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValuesByEntityId(
            Guid tenantId,
            Guid entityId)
        {
            var readModels = this.Select(tenantId, AdditionalPropertyEntityType.None, entityId, null, string.Empty);
            return Task.FromResult(readModels.ToList());
        }

        /// <inheritdoc/>
        public Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValuesBy(
            Guid tenantId,
            IAdditionalPropertyValueListFilter queryModel)
        {
            var result = this.Select(
                tenantId,
                queryModel.EntityType,
                queryModel.EntityId,
                queryModel.AdditionalPropertyDefinitionId,
                queryModel.Value);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public async Task<bool> IsAdditionalPropertyValueUnique(
            Guid tenantId,
            Guid additionalPropertyDefinitionId,
            string? value,
            Guid? entityId)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            var isUnique = !await this.context.TextAdditionalPropertValues
                .Where(v =>
                    v.TenantId == tenantId &&
                    v.AdditionalPropertyDefinitionId == additionalPropertyDefinitionId &&
                    v.Value == value &&
                    v.EntityId != entityId)
                .AnyAsync();

            return isUnique;
        }

        /// <inheritdoc/>
        public AdditionalPropertyValueDto GetAdditionalPropertyValueByEntityIdAndPropertyAlias(
           Guid tenantId, Guid entityId, string propertyAlias)
        {
            return this.context.TextAdditionalPropertValues
                .Include(tpv => tpv.AdditionalPropertyDefinition)
                .Where(tpv => tpv.AdditionalPropertyDefinition.Alias == propertyAlias)
                .Where(tpv => tpv.AdditionalPropertyDefinition.IsDeleted == false)
                .Where(tpv => tpv.EntityId == entityId)
                .Where(tpv => tpv.TenantId == tenantId)
                .Select(this.Selector).FirstOrDefault();
        }

        /// <inheritdoc/>
        public bool DoesAdditionalPropertyValueExistsForEntityIdAndPropertyAlias(
             Guid tenantId, AdditionalPropertyEntityType entityType, string propertyAlias, string propertyValue)
        {
            return this.context.TextAdditionalPropertValues
               .Include(tpv => tpv.AdditionalPropertyDefinition)
               .Where(tpv => tpv.AdditionalPropertyDefinition.Alias == propertyAlias)
               .Where(tpv => tpv.AdditionalPropertyDefinition.IsDeleted == false)
               .Where(tpv => tpv.AdditionalPropertyDefinition.EntityType == entityType)
               .Where(tpv => tpv.TenantId == tenantId)
               .Where(tpv => tpv.Value == propertyValue)
               .Any();
        }

        /// <inheritdoc/>
        public void RemoveEntryWithEmptyValueMigration()
        {
            var emptyValues = this.context.TextAdditionalPropertValues.Where(v => v.Value == string.Empty);
            this.context.TextAdditionalPropertValues.RemoveRange(emptyValues);
            this.context.SaveChanges();
        }

        /// <inheritdoc/>
        public void IncrementAdditionalPropertyEntityTypeColumnValueMigration()
        {
            this.context.AdditionalPropertyDefinitions
                .ForEach(apd =>
                {
                    apd.EntityType = apd.EntityType + 1;
                });
            this.context.SaveChanges();
        }

        /// <inheritdoc/>
        public void DecrementAdditionalPropertyEntityTypeColumnValueMigration()
        {
            this.context.AdditionalPropertyDefinitions
                .ForEach(apd =>
                {
                    apd.EntityType = apd.EntityType - 1;
                });
            this.context.SaveChanges();
        }

        private List<AdditionalPropertyValueDto> Select(
            Guid tenantId, AdditionalPropertyEntityType entityType, Guid entityId, Guid? additionalPropertyDefinitionId, string value)
        {
            var additionalPropertyDefinitionsQuery =
                this.additionalPropertyDefinitionRepository.GetByEntityTypeAndPropertyTypeQuery(tenantId, entityType, AdditionalPropertyDefinitionType.Text);

            if (additionalPropertyDefinitionId.HasValue)
            {
                additionalPropertyDefinitionsQuery = additionalPropertyDefinitionsQuery.Where(
                    apd => apd.Id == additionalPropertyDefinitionId);
            }

            additionalPropertyDefinitionsQuery = this.additionalPropertyDefinitionFilterResolver.FilterAdditionalPropertyByEntityType(additionalPropertyDefinitionsQuery, tenantId, entityId, entityType);

            var additionalPropertyDefinitionsQueryResult = additionalPropertyDefinitionsQuery.ToList();

            var additionalPropertyDefinitionsDtos = additionalPropertyDefinitionsQuery
                .Select(apd => new AdditionalPropertyDefinitionDto
                {
                    Alias = apd.Alias,
                    ContextId = apd.ContextId,
                    ContextType = apd.ContextType,
                    EntityType = apd.EntityType,
                    Id = apd.Id,
                    IsRequired = apd.IsRequired,
                    IsUnique = apd.IsUnique,
                    Name = apd.Name,
                    ParentContextId = apd.ParentContextId,
                    PropertyType = apd.PropertyType,
                    DefaultValue = apd.DefaultValue,
                    IsDeleted = apd.IsDeleted,
                });

            List<AdditionalPropertyValueDto> output = new List<AdditionalPropertyValueDto>();
            foreach (var additionalPropertyDefinition in additionalPropertyDefinitionsDtos)
            {
                var apvQuery = this.context.TextAdditionalPropertValues
                    .Where(apv => apv.AdditionalPropertyDefinitionId == additionalPropertyDefinition.Id
                        && apv.TenantId == tenantId);

                if (entityId != Guid.Empty)
                {
                    apvQuery = apvQuery.Where(apv => apv.EntityId == entityId);
                }

                if (!string.IsNullOrEmpty(value))
                {
                    apvQuery = apvQuery.Where(apv => apv.Value == value);
                }

                var apvQueryResult = apvQuery.Select(this.Selector).FirstOrDefault();

                if (apvQueryResult == null && string.IsNullOrEmpty(value))
                {
                    apvQueryResult = new AdditionalPropertyValueDto
                    {
                        EntityId = entityId,
                        Id = null,
                        Value = additionalPropertyDefinition.DefaultValue ?? string.Empty,
                        AdditionalPropertyDefinition = additionalPropertyDefinition,
                    };
                }

                if (apvQueryResult != null)
                {
                    output.Add(apvQueryResult);
                }
            }

            return output;
        }
    }
}
