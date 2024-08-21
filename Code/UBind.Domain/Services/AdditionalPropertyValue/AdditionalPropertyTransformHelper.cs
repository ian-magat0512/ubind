// <copyright file="AdditionalPropertyTransformHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyValue
{
    using System.Collections.ObjectModel;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Queries.AdditionalPropertyValue;
    using UBind.Domain.ReadModel;

    public class AdditionalPropertyTransformHelper : IAdditionalPropertyTransformHelper
    {
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;
        private readonly ICqrsMediator mediator;

        public AdditionalPropertyTransformHelper(
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository,
            ICqrsMediator mediator)
        {
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
            this.mediator = mediator;
        }

        public async Task<List<AdditionalPropertyValueUpsertModel>> TransformObjectDictionaryToValueUpsertModels(
            Guid tenantId,
            Guid? organisationId,
            AdditionalPropertyEntityType entityType,
            ReadOnlyDictionary<string, object> additionalPropertyDictionary)
        {
            if (additionalPropertyDictionary == null)
            {
                return new List<AdditionalPropertyValueUpsertModel>();
            }

            var definitions = await this.GetAdditionalPropertyDefinitions(tenantId, organisationId, entityType);

            // Check if there is a type that is not string.
            foreach (var additionalPropertyItem in additionalPropertyDictionary)
            {
                var type = additionalPropertyItem.Value?.GetType();
                if (type != typeof(string) && type != null)
                {
                    var error = Errors.AdditionalProperties.AdditionalPropertyHasInvalidType(
                        additionalPropertyItem.Key,
                        type);
                    throw new ErrorException(error);
                }

                var definition = definitions.FirstOrDefault(def => def.Alias == additionalPropertyItem.Key);

                // It was not found.
                if (definition == null)
                {
                    var error = Errors.AdditionalProperties.AdditionalPropertyNotFound(
                        entityType,
                        additionalPropertyItem.Key);
                    throw new ErrorException(error);
                }
            }

            var additionalPropertyValues = new List<AdditionalPropertyValueUpsertModel>();
            foreach (var definition in definitions)
            {
                KeyValuePair<string, object> propertyItem = additionalPropertyDictionary.FirstOrDefault(x => x.Key == definition.Alias);
                string? value = propertyItem.Value as string;
                bool hasValue = !string.IsNullOrWhiteSpace(value);

                // The definition is required but it doesn't have value.
                if (definition.IsRequired)
                {
                    if (!additionalPropertyDictionary.Any(x => x.Key == definition.Alias))
                    {
                        var error = Errors.AdditionalProperties.AdditionalPropertyIsRequired(
                            definition.Alias);
                        throw new ErrorException(error);
                    }

                    if (!hasValue)
                    {
                        var error = Errors.AdditionalProperties.RequiredAdditionalPropertyIsEmpty(
                            definition.Alias);
                        throw new ErrorException(error);
                    }
                }

                if (definition.IsUnique && hasValue)
                {
                    var query = new GetAdditionalPropertyValuesQuery(
                        tenantId,
                        entityType,
                        Guid.Empty,
                        AdditionalPropertyDefinitionType.Text,
                        definition.Id,
                        value);
                    var result = await this.mediator.Send(query);

                    if (result != null && result.Any())
                    {
                        // Throw error that it's not unique anymore.
                        var error = Errors.AdditionalProperties.UniqueAdditionalPropertyValueAlreadyUsed(
                            entityType,
                            definition.Alias,
                            value);
                        throw new ErrorException(error);
                    }
                }

                if (propertyItem.Key != null)
                {
                    additionalPropertyValues.Add(new AdditionalPropertyValueUpsertModel()
                    {
                        DefinitionId = definition.Id,
                        Type = AdditionalPropertyDefinitionType.Text,
                        Value = value,
                    });
                }
            }

            return additionalPropertyValues;
        }

        public async Task<List<AdditionalPropertyDefinitionReadModel>> GetAdditionalPropertyDefinitions(
            Guid tenantId, Guid? organisationId, AdditionalPropertyEntityType entityType)
        {
            var definitions = new List<AdditionalPropertyDefinitionReadModel>();
            if (organisationId.HasValue)
            {
                definitions = await this.additionalPropertyDefinitionRepository.GetByModelFilter(
                    tenantId,
                    new AdditionalPropertyDefinitionReadModelFilters
                    {
                        ContextType = AdditionalPropertyDefinitionContextType.Organisation,
                        ContextId = organisationId.Value,
                        Entity = entityType,
                        Aliases = null,
                    });
            }

            definitions.AddRange(await this.additionalPropertyDefinitionRepository.GetByModelFilter(
                    tenantId,
                    new AdditionalPropertyDefinitionReadModelFilters
                    {
                        ContextType = AdditionalPropertyDefinitionContextType.Tenant,
                        ContextId = tenantId,
                        Entity = entityType,
                        Aliases = null,
                    }));
            return definitions;
        }
    }
}
