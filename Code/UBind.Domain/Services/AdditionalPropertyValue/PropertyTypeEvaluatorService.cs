// <copyright file="PropertyTypeEvaluatorService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyValue
{
    using System;
    using System.Collections.Generic;
    using Humanizer;
    using UBind.Domain.Aggregates.AdditionalPropertyDefinition;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Service class in creating the specific instance of property type evaluator.
    /// </summary>
    public class PropertyTypeEvaluatorService
    {
        private readonly IReadOnlyDictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor> propertyTypeEvaluatorByKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTypeEvaluatorService"/> class.
        /// </summary>
        /// <param name="propertyTypeEvaluatorByKey">Readonly dictionary that contains mapping between the concrete
        /// classes of <see cref="AdditionalPropertyDefinitionType"/> and <see cref="IAdditionalPropertyValueProcessor"/>.</param>
        public PropertyTypeEvaluatorService(
            IReadOnlyDictionary<AdditionalPropertyDefinitionType, IAdditionalPropertyValueProcessor> propertyTypeEvaluatorByKey)
        {
            this.propertyTypeEvaluatorByKey = propertyTypeEvaluatorByKey;
        }

        /// <summary>
        /// Generates the concrete property type evaluator.
        /// </summary>
        /// <param name="propertyType">The property type.</param>
        /// <returns>Concrete class that implements / inherits <see cref="IAdditionalPropertyValueProcessor"/>.</returns>
        public IAdditionalPropertyValueProcessor GeneratePropertyTypeValueProcessorBasedOnPropertyType(
            AdditionalPropertyDefinitionType propertyType)
        {
            if (!this.propertyTypeEvaluatorByKey.TryGetValue(propertyType, out IAdditionalPropertyValueProcessor evaluator))
            {
                var propertyTypeDescription = propertyType.Humanize();
                throw new ErrorException(Errors.AdditionalProperties.PropertyTypeNotFound(propertyTypeDescription));
            }

            return evaluator;
        }

        /// <summary>
        /// Wrapper method that adds new additional property value in memory before persisting to the database.
        /// Instead of calling the GeneratePropertyTypeValueProcessorBasedOnPropertyType to execute
        /// CreateNewAdditionalPropertyValue. This method will do that instead. This increases usabality and avoid
        /// code redundancy.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="entityId">The ID of the entity where the values belong to.</param>
        /// <param name="propertyDefinitionType"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="additionalPropertyDefinitionId">The primary ID of the property definition.</param>
        /// <param name="value">The value either comes from the default value of the
        /// <see cref="AdditionalPropertyDefinitionReadModel"/> or edit form (UI).</param>
        public void CreateNewAdditionalPropertyValueByPropertyType(
            TGuid<Tenant> tenantId,
            Guid entityId,
            AdditionalPropertyDefinitionType propertyDefinitionType,
            TGuid<AdditionalPropertyDefinition> additionalPropertyDefinitionId,
            TGuid<AdditionalPropertyValue> additionalPropertyValueId,
            string value)
        {
            // We need to generate a new ID if the additional property value ID is default to handle rollback.
            // This is because old events don't have the primary ID of the additional property value.
            additionalPropertyValueId = additionalPropertyValueId.IsDefault
                ? TGuid<AdditionalPropertyValue>.NewGuid()
                : additionalPropertyValueId;
            var processor = this.GeneratePropertyTypeValueProcessorBasedOnPropertyType(propertyDefinitionType);
            processor.CreateNewAdditionalPropertyValueForAggregateEntity(
                additionalPropertyValueId,
                new AdditionalPropertyValue(
                    tenantId,
                    entityId,
                    additionalPropertyDefinitionId,
                    value));
        }

        /// <summary>
        /// Wrapper method that updates existing additional property value in memory before persisting to the database.
        /// Instead of calling the GeneratePropertyTypeValueProcessorBasedOnPropertyType to execute UpdateAdditionalPropertyValue.
        /// This method will do that instead. This increases usability and avoid code redundancy.
        /// This also handles rollback because old events don't have the primary ID of the additional property value.
        /// Instead, we use the entity Id and the additional property definition Id to identify the primary ID.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="entityId">The additional property value's entity Id.</param>
        /// <param name="additionalPropertyDefinitionType"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="additionalPropertyDefinitionId">The additional property definition Id.</param>
        /// <param name="additionalPropertyValueId">Primary ID of the additional property value. It depends on the
        /// <see cref="AdditionalPropertyDefinitionType"/> where it will be persisted.</param>
        /// <param name="value">The value which normally comes from the edit form (UI).</param>
        public void UpdateAdditionalPropertyValue(
            TGuid<Tenant> tenantId,
            Guid entityId,
            AdditionalPropertyDefinitionType additionalPropertyDefinitionType,
            TGuid<AdditionalPropertyDefinition> additionalPropertyDefinitionId,
            TGuid<AdditionalPropertyValue> additionalPropertyValueId,
            string value)
        {
            var processor = this.GeneratePropertyTypeValueProcessorBasedOnPropertyType(additionalPropertyDefinitionType);
            if (additionalPropertyValueId.IsDefault)
            {
                additionalPropertyValueId =
                    (TGuid<AdditionalPropertyValue>)processor.GetAdditionalPropertyValue(tenantId, additionalPropertyDefinitionId, entityId).Result.Id.Value;
            }

            processor.UpdateAdditionalPropertyValueForAggregateEntity(tenantId, additionalPropertyValueId, value);
        }

        public void DeleteAdditionalPropertyValue(
            TGuid<Tenant> tenantId,
            AdditionalPropertyDefinitionType additionalPropertyDefinitionType,
            TGuid<AdditionalPropertyValue> additionalPropertyValueId)
        {
            var processor = this.GeneratePropertyTypeValueProcessorBasedOnPropertyType(additionalPropertyDefinitionType);
            processor.DeleteAdditionalPropertyValueForAggregateEntity(tenantId.Value, additionalPropertyValueId.Value);
        }
    }
}
