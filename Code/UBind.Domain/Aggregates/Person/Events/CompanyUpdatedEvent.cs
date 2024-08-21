// <copyright file="CompanyUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Person
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for people.
    /// </summary>
    public partial class PersonAggregate
    {
        /// <summary>
        /// The person's company has been updated.
        /// </summary>
        public class CompanyUpdatedEvent : PropertyUpdateEvent<string, PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CompanyUpdatedEvent"/> class.
            /// </summary>
            /// <param name="personId">The ID of the person the event belongs to.</param>
            /// <param name="company">The person's company.</param>
            /// <param name="performingUserId">The userId who updates company.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public CompanyUpdatedEvent(Guid tenantId, Guid personId, string company, Guid? performingUserId, Instant createdTimestamp)
            : base(tenantId, personId, company, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private CompanyUpdatedEvent()
            {
            }
        }
    }
}
