// <copyright file="ApplyNewIdEvent.cs" company="uBind">
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
    /// Aggregate for person.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// Event raised when specifically to apply New Ids on a person aggregate.
        /// </summary>
        public class ApplyNewIdEvent : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ApplyNewIdEvent"/> class.
            /// </summary>
            /// <param name="tenantNewId">The guid tenant new id.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="performingUserId">The performing user id.</param>
            /// <param name="createdTimestamp">The created time.</param>
            public ApplyNewIdEvent(
                Guid tenantNewId,
                Guid aggregateId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantNewId, aggregateId, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private ApplyNewIdEvent()
            {
            }
        }
    }
}
