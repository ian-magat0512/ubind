// <copyright file="QuoteTransferredToAnotherOrganisationEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;

    public partial class QuoteAggregate
    {
        /// <summary>
        /// Event raised when a quote is about to be transferred to another organisation.
        /// </summary>
        public class QuoteTransferredToAnotherOrganisationEvent
            : Event<QuoteAggregate, Guid>
        {
            public QuoteTransferredToAnotherOrganisationEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid organisationId,
                Guid? quoteId,
                Guid? performingUserId,
                Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
                this.OrganisationId = organisationId;
            }

            [JsonConstructor]
            private QuoteTransferredToAnotherOrganisationEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            [JsonProperty]
            public Guid? QuoteId { get; private set; }

            [JsonProperty]
            public Guid OrganisationId { get; private set; }
        }
    }
}
