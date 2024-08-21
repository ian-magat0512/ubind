// <copyright file="IAdditionalPropertyValueEntityAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System;
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// Contracts for adding an aggregate event for additional property value associated with an entity.
    /// This contains methods in creating or updating additional properties' value via edit form.
    /// </summary>
    public interface IAdditionalPropertyValueEntityAggregate
    {
        /// <summary>
        /// Adds an initialized event in the aggregate for additional property value.
        /// </summary>
        /// <param name="tenantId">Tenant's ID.</param>
        /// <param name="entityId">Entity ID.</param>
        /// <param name="additionalPropertyDefinitionId">ID of additional property definition.</param>
        /// <param name="value">Default value of additional property definition.</param>
        /// <param name="propertyType"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="performingUserId">ID of the performing user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        void AddAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefinitionId,
            string value,
            AdditionalPropertyDefinitionType propertyType,
            Guid? performingUserId,
            Instant createdTimestamp);

        /// <summary>
        /// Adds the update initialized event to an aggregate.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="entityId">ID of an associated entity.</param>
        /// <param name="type"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="additionalPropertyDefinitionId">ID of an additional property definition.</param>
        /// <param name="additionalPropertyValueId">The ID of the additional property value.</param>
        /// <param name="value">Set value in the edit form.</param>
        /// <param name="performingUserId">ID of the performing user id.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        void UpdateAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            AdditionalPropertyDefinitionType type,
            Guid additionalPropertyDefinitionId,
            Guid additionalPropertyValueId,
            string value,
            Guid? performingUserId,
            Instant createdTimestamp);
    }
}
