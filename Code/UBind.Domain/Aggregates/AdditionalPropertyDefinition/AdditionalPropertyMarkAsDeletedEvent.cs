// <copyright file="AdditionalPropertyMarkAsDeletedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.AdditionalPropertyDefinition
{
    using System;

    /// <summary>
    /// Aggregate of additional property definition.
    /// </summary>
    public partial class AdditionalPropertyDefinition
    {
        /// <summary>
        /// Event raised when status is updated.
        /// </summary>
        public class AdditionalPropertyMarkAsDeletedEvent
            : PropertyUpdateEvent<bool, AdditionalPropertyDefinition, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AdditionalPropertyMarkAsDeletedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="id">Primary key value of additional property definition.</param>
            /// <param name="performingUserId">Performing user id.</param>
            /// <param name="instant">Timestamp.</param>
            public AdditionalPropertyMarkAsDeletedEvent(
                Guid tenantId, Guid id, Guid? performingUserId, NodaTime.Instant instant)
                : base(tenantId, id, true, performingUserId, instant)
            {
            }
        }
    }
}
