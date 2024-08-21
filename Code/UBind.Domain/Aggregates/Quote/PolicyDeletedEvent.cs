// <copyright file="PolicyDeletedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote;

using Newtonsoft.Json;
using NodaTime;

/// <summary>
/// Aggregate for quotes.
/// </summary>
public partial class QuoteAggregate
{
    /// <summary>
    /// Event raised when a policy has been deleted.
    /// </summary>
    public class PolicyDeletedEvent : Event<QuoteAggregate, Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyDeletedEvent"/> class.
        /// </summary>
        /// <param name="aggregateId">The ID of the aggregate.</param>
        /// <param name="performingUserId">The userId who deleted the quote.</param>
        /// <param name="timestamp">A timestamp.</param>
        public PolicyDeletedEvent(Guid tenantId, Guid aggregateId, Guid? performingUserId, Instant timestamp)
            : base(tenantId, aggregateId, performingUserId, timestamp)
        {
        }

        [JsonConstructor]
        private PolicyDeletedEvent()
        {
        }
    }
}
