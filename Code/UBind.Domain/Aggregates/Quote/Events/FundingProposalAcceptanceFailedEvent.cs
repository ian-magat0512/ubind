// <copyright file="FundingProposalAcceptanceFailedEvent.cs" company="uBind">
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
        /// Event raised when funding proposal acceptance has failed.
        /// </summary>
        public class FundingProposalAcceptanceFailedEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FundingProposalAcceptanceFailedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="quoteId">The ID of the quote aggregate.</param>
            /// <param name="fundingProposal">The proposal that was to be accepted.</param>
            /// <param name="performingUserId">The userId who created function proposal.</param>
            /// <param name="errors">The errors.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public FundingProposalAcceptanceFailedEvent(
                Guid tenantId, Guid quoteId, FundingProposal fundingProposal, IEnumerable<string> errors, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, quoteId, performingUserId, createdTimestamp)
            {
                this.FundingProposal = fundingProposal;
                this.Errors = errors;
            }

            [JsonConstructor]
            private FundingProposalAcceptanceFailedEvent()
            {
            }

            /// <summary>
            /// Gets the proposal that has been accepted.
            /// </summary>
            [JsonProperty]
            public FundingProposal FundingProposal { get; private set; }

            /// <summary>
            /// Gets the errors.
            /// </summary>
            [JsonProperty]
            public IEnumerable<string> Errors { get; private set; }
        }
    }
}
