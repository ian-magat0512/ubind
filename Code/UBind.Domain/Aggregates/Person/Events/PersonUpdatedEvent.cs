// <copyright file="PersonUpdatedEvent.cs" company="uBind">
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
        /// The person's properties has been updated.
        /// </summary>
        public class PersonUpdatedEvent : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonUpdatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant the person is under.</param>
            /// <param name="personId">A unique ID for the Person.</param>
            /// <param name="personData">The person data.</param>
            /// <param name="performingUserId">The user authentication data.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public PersonUpdatedEvent(Guid tenantId, Guid personId, PersonData personData, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, personId, performingUserId, createdTimestamp)
            {
                this.PersonData = personData;
            }

            [JsonConstructor]
            public PersonUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the information of the person this data represents.
            /// </summary>
            [JsonProperty]
            public PersonData PersonData { get; private set; }
        }
    }
}
