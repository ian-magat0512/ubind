// <copyright file="ClaimSnapshotEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Claim.Entities;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Base class for events that include a snapshot of quote data.
        /// </summary>
        /// <typeparam name="TEvent">The type of the derived event.</typeparam>
        public abstract class ClaimSnapshotEvent<TEvent> : Event<ClaimAggregate, Guid>
            where TEvent : ClaimSnapshotEvent<TEvent>
        {
            private Guid claimId;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimSnapshotEvent{TEvent}"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="claim">The quote.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public ClaimSnapshotEvent(Guid tenantId, Guid aggregateId, Claim claim, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.claimId = claim.ClaimId;
                this.DataSnapshotIds = claim.GetLatestDataSnapshotIds();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ClaimSnapshotEvent{TEvent}"/> class.
            /// </summary>
            /// <remarks>
            /// /// Parameterless constructor for JSON deserialization.
            /// </remarks>
            [JsonConstructor]
            protected ClaimSnapshotEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid ClaimId
            {
                get
                {
                    // Quote ID will not exist for quotes created before introduction of renewals and updates, in which case the ID
                    // of the aggregate will be used.
                    return this.claimId == default
                        ? this.AggregateId
                        : this.claimId;
                }

                private set
                {
                    this.claimId = value;
                }
            }

            /// <summary>
            /// Gets the ID of the the form data update submitted.
            /// </summary>
            [JsonProperty]
            public ClaimDataSnapshotIds DataSnapshotIds { get; private set; }
        }
    }
}
