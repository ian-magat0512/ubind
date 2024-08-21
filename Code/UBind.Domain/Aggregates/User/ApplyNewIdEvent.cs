// <copyright file="ApplyNewIdEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        /// <summary>
        /// Event raised when specifically to apply New Ids on a report aggregate.
        /// </summary>
        public class ApplyNewIdEvent : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ApplyNewIdEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="personId">The person id.</param>
            /// <param name="performingUserId">The performing user id.</param>
            /// <param name="createdTimestamp">The created time.</param>
            public ApplyNewIdEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid personId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.PersonId = personId;
            }

            [JsonConstructor]
            private ApplyNewIdEvent()
            {
            }

            /// <summary>
            /// Gets the person id.
            /// </summary>
            [JsonProperty]
            public Guid PersonId { get; private set; }
        }
    }
}
