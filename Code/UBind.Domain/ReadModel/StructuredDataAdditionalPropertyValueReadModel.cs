// <copyright file="StructuredDataAdditionalPropertyValueReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;

    /// <summary>
    /// Table entity that contains all the historical values assigned to an entity with reference to its parent additional property definition.
    /// </summary>
    public class StructuredDataAdditionalPropertyValueReadModel : Entity<Guid>, IReadModel<Guid>, IAdditionalPropertyValueReadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredDataAdditionalPropertyValueReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="entityId">Entity primary ID.</param>
        /// <param name="additionalPropertyDefinitionId">Additional property definition ID.</param>
        /// <param name="value">The default value of Additional property definition.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        public StructuredDataAdditionalPropertyValueReadModel(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefinitionId,
            string value,
            Instant createdTimestamp)
            : this(tenantId, entityId, Guid.NewGuid(), additionalPropertyDefinitionId, value, createdTimestamp)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredDataAdditionalPropertyValueReadModel"/> class.
        /// </summary>
        /// <param name="tenantId">Tenant Id.</param>
        /// <param name="entityId">Entity primary ID.</param>
        /// <param name="id">Primary ID.</param>
        /// <param name="additionalPropertyDefinitionId">Additional property definition ID.</param>
        /// <param name="value">The default value of Additional property definition.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        public StructuredDataAdditionalPropertyValueReadModel(
            Guid tenantId,
            Guid entityId,
            Guid id,
            Guid additionalPropertyDefinitionId,
            string value,
            Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
            this.AdditionalPropertyDefinitionId = additionalPropertyDefinitionId;
            this.Value = value;
            this.EntityId = entityId;
            this.TenantId = tenantId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructuredDataAdditionalPropertyValueReadModel"/> class.
        /// </summary>
        public StructuredDataAdditionalPropertyValueReadModel()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the foreign key of additional property definition.
        /// </summary>
        public Guid AdditionalPropertyDefinitionId { get; private set; }

        /// <summary>
        /// Gets the additional property definition.
        /// </summary>
        public virtual AdditionalPropertyDefinitionReadModel AdditionalPropertyDefinition { get; private set; }

        /// <summary>
        /// Gets or sets the persisted value for the mapping between additional property definition and entity.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets the entity id that owns this value.
        /// </summary>
        public Guid EntityId { get; private set; }

        /// <summary>
        /// Gets or sets the new tenant ID in GUID type.
        /// </summary>
        public Guid TenantId { get; set; }
    }
}
