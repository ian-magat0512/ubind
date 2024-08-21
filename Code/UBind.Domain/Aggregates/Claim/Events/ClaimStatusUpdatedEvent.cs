// <copyright file="ClaimStatusUpdatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for people.
    /// </summary>
    public partial class ClaimAggregate
    {
        /// <summary>
        /// Event for when a claim's status has changed.
        /// </summary>
        /// <remarks>
        /// This event has been superceded by <see cref="ClaimAggregate.ClaimStateChangedEvent"/>.
        /// It is retained for the handling of existing events, but new events of this type should not be created.
        /// </remarks>
        public class ClaimStatusUpdatedEvent : Event<ClaimAggregate, Guid>
        {
            [JsonConstructor]
            private ClaimStatusUpdatedEvent()
            {
            }

            /// <summary>
            /// Gets the new status of the claim.
            /// </summary>
            [JsonProperty]
            public LegacyClaimStatus ClaimStatus { get; private set; }
        }
    }
}
