// <copyright file="AdditionalPropertyValue.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System;
    using System.Text.Json.Serialization;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// A class representing the additional property definition id and the value for the given entity.
    /// </summary>
    public class AdditionalPropertyValue : IAdditionalPropertyValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyValue"/> class.
        /// </summary>
        /// <param name="tenantId">Tenants Id.</param>
        /// <param name="entityId">ID of entity associated to.</param>
        /// <param name="additionalPropertyDefinitionId">ID of additional property definition.</param>
        /// <param name="value">Value to be persisted.</param>
        [JsonConstructor]
        public AdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefinitionId,
            string value)
        {
            this.AdditionalPropertyDefinitionId = additionalPropertyDefinitionId;
            this.EntityId = entityId;
            this.Value = value;
            this.TenantId = tenantId;
        }

        /// <inheritdoc/>
        public Guid AdditionalPropertyDefinitionId { get; private set; }

        /// <inheritdoc/>
        public Guid EntityId { get; private set; }

        /// <inheritdoc/>
        public string Value { get; set; }

        /// <inheritdoc/>
        public Guid TenantId { get; private set; }
    }
}
