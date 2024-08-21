// <copyright file="IAdditionalPropertyValueProcessor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyValue
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Dto;
    using UBind.Domain.Queries.AdditionalPropertyValue;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Contract of property type evaluator.
    /// This evaluate the additional property read.
    /// </summary>
    public interface IAdditionalPropertyValueProcessor
    {
        /// <summary>
        /// Creates a new additional property value for an entity.
        /// The two situations this is called is:
        /// a) when you create a new property definition
        /// b) when a new entity instance is created.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="entityId">ID of an entity.</param>
        /// <param name="additionalPropertyDefinitionId">Additional property definition ID.</param>
        /// <param name="value">Value set either automatically or by an edit form.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task SetNewValueOnEntityForAdditionalPropertyDefinitionForNonAggregateEntity(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefinitionId,
            string value);

        /// <summary>
        /// Checks whether the given entity has a value for the given additional property definition.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="additionalPropertyDefinitionId">Additional property definition id.</param>
        /// <param name="entityId">The primary id of the entity record.</param>
        /// <returns>True if exists otherwise false.</returns>
        Task<bool> EntityHasAdditionalPropertyValue(
            Guid tenantId,
            Guid additionalPropertyDefinitionId,
            Guid entityId);

        /// <summary>
        /// Creates a new additional property value to the eav model such as
        /// <see cref="TextAdditionalPropertyValueReadModel"/>.
        /// This is normally being called as part of the upsert operation in conjuction with the aggregate.
        /// </summary>
        /// <param name="id">Primary ID.</param>
        /// <param name="additionalPropertyValue">An instance that implements
        /// <see cref="IAdditionalPropertyValue"/>.</param>
        void CreateNewAdditionalPropertyValueForAggregateEntity(
            Guid id,
            IAdditionalPropertyValue additionalPropertyValue);

        void DeleteAdditionalPropertyValueForAggregateEntity(Guid tenantId, Guid id);

        /// <summary>
        /// Gets the additional property value from an eav table (<see cref="TextAdditionalPropertyValueReadModel"/>).
        /// It should only return single additional property value.
        /// </summary>
        /// <param name="tenantId">Tenant's new ID in GUID.</param>
        /// <param name="additionalPropertyDefinitionId">ID which the eav is mapped to.</param>
        /// <param name="entityId">ID of the entity.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<AdditionalPropertyValueDto> GetAdditionalPropertyValue(
            Guid tenantId, Guid additionalPropertyDefinitionId, Guid entityId);

        /// <summary>
        /// Gets the additional property values from eav table (<see cref="TextAdditionalPropertyValueReadModel"/>).
        /// This is normally being invoked in the get method of the additional property values controller.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="queryModel">Instance of a model that implements <see cref="IAdditionalPropertyValueListFilter"/>.
        /// </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<List<AdditionalPropertyValueDto>> GetAdditionalPropertyValues(
            Guid tenantId,
            IAdditionalPropertyValueListFilter queryModel);

        /// <summary>
        /// Checks for the existence of duplicate additional property values in the EAV table (<see cref="TextAdditionalPropertyValueReadModel"/>).
        /// This method is designed to verify whether duplicate additional properties exist based on the provided query parameters.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// The task result is a boolean indicating whether a duplicate additional property value exists or not.</returns>
        Task<bool> IsAdditionalPropertyValueUnique(
            Guid tenantId,
            Guid additionalPropertyDefinitionId,
            string? value,
            Guid? entityId);

        /// <summary>
        /// Update the value of an eav table (<see cref="TextAdditionalPropertyValueReadModel"/>).
        /// This is normally being called by non aggregate entities such as tenant, product and portal.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="id">ID of the record where the value is supposed to be updated.</param>
        /// <param name="value">Update value.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpdateValueForNonAggregateEntity(Guid tenantId, Guid id, string value);

        /// <summary>
        /// Updates an existing additional property value in eav table
        /// (<see cref="TextAdditionalPropertyValueReadModel"/>).
        /// This is normally being called as part of the upsert operation in conjunction with the aggregate.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="additionalPropertyValueId">ID of the additional property value.</param>
        /// <param name="value">Set value in the edit form.</param>
        void UpdateAdditionalPropertyValueForAggregateEntity(Guid tenantId, Guid additionalPropertyValueId, string value);
    }
}
