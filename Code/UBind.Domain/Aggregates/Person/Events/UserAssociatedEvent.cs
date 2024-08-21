// <copyright file="UserAssociatedEvent.cs" company="uBind">
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

    public partial class PersonAggregate
    {
        public class UserAssociatedEvent : Event<PersonAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="UserAssociatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The Id of the tenant represented as <see cref="Guid"/>.</param>
            /// <param name="personId">The Id of the person represented as <see cref="Guid"/>.</param>
            /// <param name="userId">The Id of the user to be associated represented as <see cref="Guid"/>.</param>
            /// <param name="performingUserId">The Id of the performing user Id represented as nullable <see cref="Guid"/>.</param>
            /// <param name="timestamp">The time the event was created.</param>
            public UserAssociatedEvent(
                Guid tenantId, Guid personId, Guid userId, Guid? performingUserId, Instant timestamp)
                : base(tenantId, personId, performingUserId, timestamp)
            {
                this.UserId = userId;
            }

            [JsonProperty]
            public Guid UserId { get; private set; }
        }
    }
}
