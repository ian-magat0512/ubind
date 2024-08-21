// <copyright file="FundingProposalCreationFailedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using NodaTime;

    /// <summary>
    /// Aggregate for quotes.
    /// </summary>
    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when payment attempt has failed.
        /// </summary>
        public class FundingProposalCreationFailedEvent : QuoteSnapshotFailureEvent<FundingProposalCreationFailedEvent>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FundingProposalCreationFailedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quote">The quote.</param>
            /// <param name="errors">Any errors.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public FundingProposalCreationFailedEvent(
                Guid tenantId, Guid aggregateId, Quote quote, IEnumerable<string> errors, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, quote, errors, performingUserId, createdTimestamp)
            {
            }

            [JsonConstructor]
            private FundingProposalCreationFailedEvent()
            {
            }
        }
    }
}
