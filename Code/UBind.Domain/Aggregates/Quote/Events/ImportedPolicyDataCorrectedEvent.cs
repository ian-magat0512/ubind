// <copyright file="ImportedPolicyDataCorrectedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when policy data (form or calculation) has been patched.
        /// </summary>
        public class PolicyDataPatchedEvent
            : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyDataPatchedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the quote aggregate which will be converted into policy id.</param>
            /// <param name="correction">The policy data correction entity.</param>
            /// <param name="performingUserId">The userId who patched the policy data.</param>
            /// <param name="timestamp">The time when the event was created.</param>
            public PolicyDataPatchedEvent(Guid tenantId, Guid aggregateId, PolicyDataPatch correction, Guid? performingUserId, Instant timestamp)
                : base(tenantId, aggregateId, performingUserId, timestamp)
            {
                this.PolicyDatPatch = correction;
            }

            [JsonConstructor]
            private PolicyDataPatchedEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the policy whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid PolicyId => this.AggregateId;

            /// <summary>
            /// Gets the policy data correction model. The model contains type, path and value.
            /// </summary>
            [JsonProperty]
            public PolicyDataPatch PolicyDatPatch { get; private set; }
        }
    }
}
