// <copyright file="StructuredDataAdditionalPropertyValueInitializedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Extended class for structured data additional property value.
    /// </summary>
    public partial class StructuredDataAdditionalPropertyValue
    {
        /// <summary>
        /// Event raised when creating a new structured data additional property value.
        /// </summary>
        public class StructuredDataAdditionalPropertyValueInitializedEvent :
            Event<StructuredDataAdditionalPropertyValue, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StructuredDataAdditionalPropertyValueInitializedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">Tenant's new ID in GUID.</param>
            /// <param name="id">Primary key.</param>
            /// <param name="additionalPropertyDefinitionId">Id of the additional property definition
            /// <see cref="AdditionalPropertyDefinitionReadModel"/> which owns the default value to be assigned to the
            /// attribute model.</param>
            /// <param name="entityId">The primary key id of the record that owns this value.</param>
            /// <param name="value">The value taken from the default value of the additional property.</param>
            /// <param name="localtime">When this value is created.</param>
            public StructuredDataAdditionalPropertyValueInitializedEvent(
                Guid tenantId,
                Guid id,
                Guid additionalPropertyDefinitionId,
                Guid entityId,
                string value,
                Instant localtime)
                : base(tenantId, id, default(Guid), localtime)
            {
                this.AdditionalPropertyDefinitionId = additionalPropertyDefinitionId;
                this.EntityId = entityId;
                this.Value = value;
            }

            [JsonConstructor]
            public StructuredDataAdditionalPropertyValueInitializedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the additional property definition
            /// <see cref="AdditionalPropertyDefinitionReadModel"/> which owns the default value to be assigned to the
            /// attribute model.
            /// </summary>
            [JsonProperty]
            public Guid AdditionalPropertyDefinitionId { get; private set; }

            /// <summary>
            /// Gets the id of the record that owns this value.
            /// </summary>
            [JsonProperty]
            public Guid EntityId { get; private set; }

            /// <summary>
            /// Gets the value taken from the default value of the additional property.
            /// </summary>
            [JsonProperty]
            public string Value { get; private set; }
        }
    }
}
