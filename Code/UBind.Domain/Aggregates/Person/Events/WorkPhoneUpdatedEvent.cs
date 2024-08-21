// <copyright file="WorkPhoneUpdatedEvent.cs" company="uBind">
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
        /// The person's work phone has been updated.
        /// </summary>
        public class WorkPhoneUpdatedEvent : PropertyUpdateEvent<string, PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkPhoneUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="personId">The ID of the person.</param>
            /// <param name="workPhone">The new work phone number.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public WorkPhoneUpdatedEvent(Guid tenantId, Guid personId, string workPhone, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, personId, workPhone, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private WorkPhoneUpdatedEvent()
            {
            }
        }
    }
}
