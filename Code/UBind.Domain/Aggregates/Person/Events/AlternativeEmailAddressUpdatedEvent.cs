// <copyright file="AlternativeEmailAddressUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for people.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// The person's alternate email address has been updated.
        /// </summary>
        public class AlternativeEmailAddressUpdatedEvent : PropertyUpdateEvent<string, PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AlternativeEmailAddressUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="personId">The ID of the person the event belongs to.</param>
            /// <param name="alternativeEmailAddress">The new email address.</param>
            /// <param name="performingUserId">The userId who updated alternative email.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public AlternativeEmailAddressUpdatedEvent(Guid tenantId, Guid personId, string alternativeEmailAddress, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, personId, alternativeEmailAddress, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private AlternativeEmailAddressUpdatedEvent()
            {
            }
        }
    }
}
