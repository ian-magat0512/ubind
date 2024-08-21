// <copyright file="IAdditionalPropertyValueRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Queries.AdditionalPropertyValue;

    /// <summary>
    /// Generic contract for all additional property eav tables.
    /// </summary>
    public interface IAdditionalPropertyValueRepository
    {
        /// <summary>
        /// Get the list of additional property values by parameter.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="entityId">Entity id.</param>
        /// <returns>List of <see cref="AdditionalPropertyValueDto"/>.</returns>
        Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValuesByEntityId(Guid tenantId, Guid entityId);

        /// <summary>
        /// Gets the additional property value by parameter.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="entityId">Entity id.</param>
        /// <param name="additionalPropertyDefintionId">Additional property definition id.</param>
        /// <returns>Instance of <see cref="AdditionalPropertyValueDto"/>.</returns>
        Task<AdditionalPropertyValueDto> GetAdditionalPropertyValueByAdditionalPropertyDefinitionIdAndEntity(
            Guid tenantId, Guid entityId, Guid additionalPropertyDefintionId);

        /// <summary>
        /// Get the list of additional property values by parameters.
        /// </summary>
        /// <param name="tenantId">Tenant ID.</param>
        /// <param name="queryModel">Query model instance that implements <see cref="IAdditionalPropertyValueListFilter"/>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValuesBy(
            Guid tenantId,
            IAdditionalPropertyValueListFilter queryModel);

        /// <summary>
        /// Checks for the existence of a duplicate additional property value based on specified parameters.
        /// </summary>
        /// <param name="tenantId">The unique identifier of the tenant.</param>
        /// <param name="additionalPropertyDefinitionId">The unique identifier of the additional property definition.</param>
        /// <param name="value">The value of the additional property.</param>
        /// <param name="entityId">
        /// The unique identifier of the entity (optional).
        /// Set to null when creating a new entity and planning to set an additional property value upon creation.
        /// Provide the entity identifier when updating an existing entity to exclude its current value from the uniqueness check.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// The task result is a boolean indicating whether the additional property value is unique (true) or not (false).
        /// </returns>
        Task<bool> IsAdditionalPropertyValueUnique(
            Guid tenantId,
            Guid additionalPropertyDefinitionId,
            string? value,
            Guid? entityId);

        /// <summary>
        /// Get the additional property value by Entity Id and Property Alias.
        /// </summary>
        /// <param name="tenantId">The id of the tenant the value is for.</param>
        /// <param name="entityId">The id the entity.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <returns>The addition property value.</returns>
        AdditionalPropertyValueDto GetAdditionalPropertyValueByEntityIdAndPropertyAlias(Guid tenantId, Guid entityId, string propertyAlias);

        /// <summary>
        /// Check whether the additional property value exists for given entity id and property alias.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="entityType">The enetity id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <returns>The value wether the property value exists.</returns>
        bool DoesAdditionalPropertyValueExistsForEntityIdAndPropertyAlias(
            Guid tenantId, AdditionalPropertyEntityType entityType, string propertyAlias, string propertyValue);

        /// <summary>
        /// Remove entries with empty values.
        /// </summary>
        void RemoveEntryWithEmptyValueMigration();

        /// <summary>
        /// Incerement additional property entity type column value after adding None type.
        /// </summary>
        void IncrementAdditionalPropertyEntityTypeColumnValueMigration();

        /// <summary>
        /// Decrement additional property entity type column value when reverting migration for adding None type.
        /// </summary>
        void DecrementAdditionalPropertyEntityTypeColumnValueMigration();
    }
}
