// <copyright file="GetAdditionalPropertyValuesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyValue
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query model in getting additional property value.
    /// </summary>
    public class GetAdditionalPropertyValuesQuery
        : IQuery<List<AdditionalPropertyValueDto>>, IAdditionalPropertyValueListFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetAdditionalPropertyValuesQuery"/> class.
        /// </summary>
        /// <param name="tenantId">Tenant's Id.</param>
        /// <param name="entityType">Entity type.</param>
        /// <param name="entityId">The ID of an entity.</param>
        /// <param name="propertyDefinitionType"><see cref="AdditionalPropertyDefinitionType"/>.</param>
        /// <param name="additionalPropertyDefinitionId">Primary ID of
        /// <see cref="AdditionalPropertyDefinitionReadModel"/>.</param>
        /// <param name="value">Set value for additional property value.</param>
        public GetAdditionalPropertyValuesQuery(
            Guid tenantId,
            AdditionalPropertyEntityType entityType,
            Guid entityId,
            AdditionalPropertyDefinitionType propertyDefinitionType,
            Guid? additionalPropertyDefinitionId,
            string value)
        {
            this.AdditionalPropertyDefinitionId = additionalPropertyDefinitionId;
            this.EntityId = entityId;
            this.PropertyType = propertyDefinitionType;
            this.Value = value;
            this.TenantId = tenantId;
            this.EntityType = entityType;
        }

        /// <summary>
        /// Gets or sets the ID of additional property definition.
        /// </summary>
        public Guid? AdditionalPropertyDefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the value of additional property value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// Gets or sets the property type <see cref="AdditionalPropertyDefinitionType"/>.
        /// </summary>
        public AdditionalPropertyDefinitionType PropertyType { get; set; }

        /// <summary>
        /// Gets or sets the Tenant's Id.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Additional property entity type.
        /// </summary>
        public AdditionalPropertyEntityType EntityType { get; set; }
    }
}
