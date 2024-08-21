// <copyright file="IAdditionalPropertyDefinitionRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UBind.Domain.Enums;

    /// <summary>
    /// Contracts for additional property repository.
    /// </summary>
    public interface IAdditionalPropertyDefinitionRepository
    {
        /// <summary>
        /// Gets additional property by ID.
        /// </summary>
        /// <returns>Instance of additional property definition <see cref="AdditionalPropertyDefinitionReadModel"/>.
        /// </returns>
        /// <param name="id">Primary id for contexts such as tenant, products and organisation.</param>
        Task<AdditionalPropertyDefinitionReadModel> GetById(Guid tenantId, Guid id);

        /// <summary>
        /// Get an queryable additional properties by defined parameters.
        /// </summary>
        /// <param name="contextType"><see cref="AdditionalPropertyDefinitionContextType"/>.</param>
        /// <param name="entityType"><see cref="AdditionalPropertyEntityType"/>.</param>
        /// <param name="contextId">The primary ID of the context.</param>
        /// <returns>Queryable <see cref="AdditionalPropertyDefinitionReadModel"/>.</returns>
        /// <param name="parentContextId">ID of the context which the immediate context falls under.</param>
        IQueryable<AdditionalPropertyDefinitionReadModel> GetByContextAndEntityTypesAndContextIdsAsQueryable(
            Guid tenantId,
            AdditionalPropertyDefinitionContextType contextType,
            AdditionalPropertyEntityType entityType,
            Guid contextId,
            Guid? parentContextId = null);

        /// <summary>
        /// Gets a list of <see cref="AdditionalPropertyDefinitionReadModel"/> using a model filter.
        /// </summary>
        /// <returns>List of <see cref="AdditionalPropertyDefinitionReadModel"/>.</returns>
        /// <param name="readModelFilters">Model filter <see cref="AdditionalPropertyDefinitionReadModelFilters"/>.</param>
        Task<List<AdditionalPropertyDefinitionReadModel>> GetByModelFilter(
            Guid tenantId,
            AdditionalPropertyDefinitionReadModelFilters readModelFilters);

        /// <summary>
        /// Gets a list of <see cref="AdditionalPropertyDefinitionReadModel"/> by entity type and context id.
        /// The context id will be query both from context id and parent context id.
        /// </summary>
        /// <param name="mainContextId">Tenant ID (since that it is the top context among other contexts).</param>
        /// <returns>Generic <see cref="IQueryable{T}"/>.</returns>
        /// <param name="entityType"><see cref="AdditionalPropertyEntityType"/>.</param>
        IQueryable<AdditionalPropertyDefinitionReadModel> GetByEntityTypeAndTopContextFromContextIdAndParentContextId(
                Guid tenantId,
                Guid mainContextId,
                AdditionalPropertyEntityType entityType);

        IQueryable<AdditionalPropertyDefinitionReadModel> GetByEntityTypeAndPropertyTypeQuery(
                Guid tenantId,
                AdditionalPropertyEntityType entityType,
                AdditionalPropertyDefinitionType propertyType);

        IQueryable<AdditionalPropertyDefinitionReadModel> GetByEntityTypeQuery(
                Guid tenantId,
                AdditionalPropertyEntityType entityType);

        /// <summary>
        /// Gets a list of additional property using expression.
        /// </summary>
        /// <returns>List of additional property read models.</returns>
        /// <param name="customExpression">Expression.</param>
        Task<List<AdditionalPropertyDefinitionReadModel>> GetAdditionalPropertiesByExpression(
            Guid tenantId,
            Expression<Func<AdditionalPropertyDefinitionReadModel, bool>> customExpression);

        /// <summary>
        /// Check wether the property alias exists in entity additional property definition.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="propertyAlias">Tee property alias.</param>
        /// <param name="additionalPropertyEntityType">The additional property entity type.</param>
        /// <returns>The value wether the property alias exists.</returns>
        bool DoesEntityAdditionalPropertyDefinitionsContainPropertyAlias(
            Guid tenantId,
            string propertyAlias,
            AdditionalPropertyEntityType additionalPropertyEntityType);

        /// <summary>
        /// Get the additional property definition given the entity type and the property alias.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="propertyAlias">The property alias.</param>
        /// <param name="additionalPropertyEntityType">The additional property entity type.</param>
        /// <returns>The additional property definition.</returns>
        AdditionalPropertyDefinitionReadModel GetAdditionalPropertyDefinitionByEntityTypeAndPropertyAlias(
            Guid tenantId,
            string propertyAlias,
            AdditionalPropertyEntityType additionalPropertyEntityType);
    }
}
