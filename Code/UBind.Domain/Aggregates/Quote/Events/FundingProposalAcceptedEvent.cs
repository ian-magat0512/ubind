// <copyright file="FundingProposalAcceptedEvent.cs" company="uBind">
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
        /// Event raised when funding proposal acceptance has failed.
        /// </summary>
        public class FundingProposalAcceptedEvent : Event<QuoteAggregate, Guid>
        {
            private Guid quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="FundingProposalAcceptedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="quoteId">The ID of the quote aggregate.</param>
            /// <param name="fundingProposal">The proposal that was accepted.</param>
            /// <param name="performingUserId">The userId who accept the funding proposal.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public FundingProposalAcceptedEvent(
                Guid tenantId, Guid aggregateId, Guid quoteId, FundingProposal fundingProposal, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.quoteId = quoteId;
                this.FundingProposal = fundingProposal;
            }

            [JsonConstructor]
            private FundingProposalAcceptedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the quote.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId
            {
                get
                {
                    return this.quoteId == default
                        ? this.AggregateId
                        : this.quoteId;
                }

                private set
                {
                    this.quoteId = value;
                }
            }

            /// <summary>
            /// Gets the proposal that has been accepted.
            /// </summary>
            [JsonProperty]
            public FundingProposal FundingProposal { get; private set; }
        }
    }
}
