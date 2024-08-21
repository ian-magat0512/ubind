// <copyright file="StructuredDataAdditionalPropertyUpdateValueEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyValue
{
    using System;

    /// <summary>
    /// Aggregate for structured data additional property definition.
    /// </summary>
    public partial class StructuredDataAdditionalPropertyValue
    {
        /// <summary>
        /// Event in updating value for structured data additional property.
        /// </summary>
        public class StructuredDataAdditionalPropertyUpdateValueEvent
            : PropertyUpdateEvent<string, StructuredDataAdditionalPropertyValue, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StructuredDataAdditionalPropertyUpdateValueEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="id">ID of structured data additional property definition.</param>
            /// <param name="value">value to be updated.</param>
            /// <param name="timestamp">Time the update occurs.</param>
            public StructuredDataAdditionalPropertyUpdateValueEvent(
                Guid tenantId,
                Guid id,
                string value,
                NodaTime.Instant timestamp)
                : base(tenantId, id, value, default(Guid), timestamp)
            {
            }
        }
    }
}
