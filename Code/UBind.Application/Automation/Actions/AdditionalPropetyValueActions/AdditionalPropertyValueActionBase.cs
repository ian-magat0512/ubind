// <copyright file="AdditionalPropertyValueActionBase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions.AdditionalPropetyValueActions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Queries.AdditionalPropertyDefinition;
    using UBind.Domain.Queries.AdditionalPropertyValue;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services.AdditionalPropertyValue;

    /// <summary>
    /// Base class for additional property actions.
    /// </summary>
    public abstract class AdditionalPropertyValueActionBase : Actions.Action
    {
        private readonly IAdditionalPropertyValueService addPropertyService;
        private readonly PropertyTypeEvaluatorService addpropertyEvaluatorService;
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;
        private readonly ICqrsMediator mediator;

        protected AdditionalPropertyValueActionBase(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunConditions,
            IEnumerable<ErrorCondition> afterRunConditions,
            IEnumerable<IRunnableAction> errorActions,
            IAdditionalPropertyValueService addPropertyService,
            PropertyTypeEvaluatorService addpropertyEvaluatorService,
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository,
            ICqrsMediator mediator)
            : base(name, alias, description, asynchronous, runCondition, beforeRunConditions, afterRunConditions, errorActions)
        {
            this.addPropertyService = addPropertyService;
            this.addpropertyEvaluatorService = addpropertyEvaluatorService;
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets or sets the additional details.
        /// </summary>
        protected List<string> AdditionalDetails { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the error data.
        /// </summary>
        protected JObject ErrorData { get; set; }

        /// <summary>
        /// Sets the value of an additional property.
        /// </summary>
        /// <param name="providerContext">The provider context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>The additional property value value.</returns>
        protected async Task SetAdditionalPropertyValue(
            IProviderContext providerContext,
            Domain.SerialisedEntitySchemaObject.IEntity entity,
            Guid entityId,
            string entityType,
            string propertyAlias,
            string newValue)
        {
            this.AdditionalDetails.Add($"Additional Property Alias: {propertyAlias}");
            if (!string.IsNullOrWhiteSpace(newValue))
            {
                this.AdditionalDetails.Add($"Value: {newValue}");
            }

            var productContext = providerContext.AutomationData.GetProductContextFromContext();
            var isAdditionalPropertyEntityType = Enum.TryParse(entityType, out AdditionalPropertyEntityType additionalPropertyEntityType);
            if (!isAdditionalPropertyEntityType)
            {
                throw new ErrorException(
                    Errors.Automation.AdditionalPropertiesNotSupportedOnEntityType(
                        this.ErrorData,
                        entity.DomainEntityType.Name,
                        this.AdditionalDetails));
            }

            var definition = await this.mediator.Send(new GetAdditionalPropertyDefinitionByEntityTypeAndAliasQuery(
                productContext.TenantId, additionalPropertyEntityType, propertyAlias));
            if (definition == null)
            {
                throw new ErrorException(Errors.AdditionalProperties.DefinitionNotFoundOnEntity(
                    propertyAlias,
                    additionalPropertyEntityType,
                    $"set an additional property value using the automation action \"{this.Name}\"",
                    this.ErrorData,
                    this.AdditionalDetails));
            }

            this.AdditionalDetails.Add($"Additional Property Name: {definition.Name}");
            if (definition.IsRequired && string.IsNullOrWhiteSpace(newValue))
            {
                throw new ErrorException(Errors.Automation.AdditionalPropertyValueRequired(
                                propertyAlias, this.AdditionalDetails, this.ErrorData, entityType));
            }

            if (definition.IsUnique)
            {
                var otherEntityWithSameValue = await this.GetOtherEntityWithSameValue(
                    productContext.TenantId, entityId, definition, newValue);
                if (otherEntityWithSameValue != null)
                {
                    this.AdditionalDetails.Add($"Other Entity ID: {otherEntityWithSameValue.EntityId}");
                    var entityReference = this.GetEntityReference(
                        entityType, otherEntityWithSameValue.EntityId, providerContext);
                    this.AdditionalDetails.Add($"Other Entity Reference: {entityReference}");
                    throw new ErrorException(Errors.Automation.AdditionalPropertyValueMustBeUnique(
                        propertyAlias, this.AdditionalDetails, this.ErrorData, entityType));
                }
            }

            await this.addPropertyService.UpdateAdditionalPropertyValueForEntity(
                productContext.TenantId,
                entityId,
                productContext.Environment,
                definition,
                newValue);
        }

        /// <summary>
        /// Gets the additional property value.
        /// </summary>
        /// <param name="providerContext">The provider context.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>The additional property value.</returns>
        protected async Task<AdditionalPropertyValueDto> GetAdditionalPropertyValue(
            IProviderContext providerContext, Guid entityId, string entityType, string propertyAlias)
        {
            var entityProperties = await this.addPropertyService.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                            providerContext.AutomationData.ContextManager.Tenant.Id,
                            (AdditionalPropertyEntityType)Enum.Parse(typeof(AdditionalPropertyEntityType), entityType),
                            entityId);
            var property = entityProperties.Any()
                ? entityProperties.FirstOrDefault(c => c.AdditionalPropertyDefinition.Alias.EqualsIgnoreCase(propertyAlias))
                : null;
            if (property == null || property.AdditionalPropertyDefinition == null)
            {
                throw new ErrorException(
                    Errors.Automation.AdditionalPropertyDefinitionDoesNotExist(
                        propertyAlias, this.AdditionalDetails, this.ErrorData, entityType));
            }

            return property;
        }

        /// <summary>
        /// Retrieves the combined error data from the provider context and the entity used, if available.
        /// </summary>
        /// <param name="providerContext">The provider context.</param>
        /// <param name="entity">The entity being used.</param>
        protected async Task RetrieveErrorDataFromContextAndEntity(IProviderContext providerContext, Domain.SerialisedEntitySchemaObject.IEntity entity = null)
        {
            var errorData = await providerContext.GetDebugContext();
            if (entity != null)
            {
                var entityErrorDetails = GenericErrorDataHelper.RetrieveErrorData(entity);
                foreach (var property in entityErrorDetails)
                {
                    errorData.Add(property.Key, property.Value);
                }
            }

            this.ErrorData = errorData;
        }

        private async Task<AdditionalPropertyValueDto> GetOtherEntityWithSameValue(
            Guid tenantId,
            Guid entityId,
            AdditionalPropertyDefinitionReadModel definition,
            string newValue)
        {
            var evaluator = this.addpropertyEvaluatorService.GeneratePropertyTypeValueProcessorBasedOnPropertyType(
                        definition.PropertyType);
            var request = new GetAdditionalPropertyValuesQuery(
                Guid.Empty,
                default(AdditionalPropertyEntityType),
                Guid.Empty,
                AdditionalPropertyDefinitionType.Text,
                definition.Id,
                newValue);
            var values = await evaluator.GetAdditionalPropertyValues(tenantId, request);
            return values.FirstOrDefault(c => c.EntityId != entityId && c.Value == newValue);
        }

        private async Task<string> GetEntityReference(string entityType, Guid entityId, IProviderContext providerContext)
        {
            var entityIdBuilder = new StaticBuilder<Data<string>>() { Value = entityId.ToString() };
            var entityTypeBuilder = new StaticBuilder<Data<string>>() { Value = entityType };
            var dynamicEntityProviderConfig = new DynamicEntityProviderConfigModel()
            {
                EntityId = entityIdBuilder,
                EntityType = entityTypeBuilder,
            };

            var dynamicEntityProvider = dynamicEntityProviderConfig.Build(providerContext.AutomationData.ServiceProvider);
            var resolveEntity = await dynamicEntityProvider.Resolve(providerContext);
            var entity = resolveEntity.GetValueOrThrowIfFailed();
            return entity.DataValue.EntityReference;
        }
    }
}
