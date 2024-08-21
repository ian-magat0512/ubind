// <copyright file="IAdditionalPropertyTransformHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyValue
{
    using System.Collections.ObjectModel;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Enums;
    using UBind.Domain.ReadModel;

    // <summary>
    // Interface for a helper class that facilitates the transformation of read-only dictionaries
    // to additional property value upsert models and retrieval of additional property definitions
    // for specific entity types and contexts.
    // </summary>
    public interface IAdditionalPropertyTransformHelper
    {
        /// <summary>
        /// Transforms a read-only dictionary into a collection of AdditionalPropertyValueUpsertModel objects.
        /// </summary>
        /// <returns>A list of AdditionalPropertyValueUpsertModel objects.</returns>
        Task<List<AdditionalPropertyValueUpsertModel>> TransformObjectDictionaryToValueUpsertModels(
            Guid tenantId,
            Guid? organisationId,
            AdditionalPropertyEntityType entityType,
            ReadOnlyDictionary<string, object> additionalPropertyDictionary);

        /// <summary>
        /// Retrieves additional property definitions for a specific entity type and context.
        /// </summary>
        /// <returns>A list of AdditionalPropertyDefinitionReadModel objects.</returns>
        Task<List<AdditionalPropertyDefinitionReadModel>> GetAdditionalPropertyDefinitions(
            Guid tenantId, Guid? organisationId, AdditionalPropertyEntityType entityType);
    }
}
