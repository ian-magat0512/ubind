// <copyright file="PolicyStateChangedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Humanizer;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a policies state has been changed.
        /// </summary>
        [Obsolete("Policy state is calculated dynamically by aggregates, and should not be set.")]
        public class PolicyStateChangedEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyStateChangedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="policyId">The ID of the aggregate.</param>
            /// <param name="policyState">The original state of the policy prior to state change.</param>
            /// <param name="timestamp">A created timestamp.</param>
            public PolicyStateChangedEvent(Guid tenantId, Guid policyId, string policyState, Instant timestamp)
                : base(tenantId, policyId, null, timestamp)
            {
                this.PolicyState = policyState?.Pascalize();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PolicyStateChangedEvent"/> class.
            /// </summary>
            [JsonConstructor]
            private PolicyStateChangedEvent()
            {
            }

            /// <summary>
            /// Gets the state of the policy the event is for.
            /// </summary>
            [JsonProperty]
            public string PolicyState { get; private set; }
        }
    }
}
