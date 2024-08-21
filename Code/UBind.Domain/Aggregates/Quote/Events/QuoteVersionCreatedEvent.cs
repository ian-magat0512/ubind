// <copyright file="QuoteVersionCreatedEvent.cs" company="uBind">
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
        /// Event raised when a quote version has been created.
        /// </summary>
        public class QuoteVersionCreatedEvent : QuoteSnapshotEvent<QuoteVersionCreatedEvent>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteVersionCreatedEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="quote">The quote.</param>
            /// <param name="performingUserId">The userId who created quote version.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public QuoteVersionCreatedEvent(
                Guid tenantId, Guid aggregateId, Guid versionId, Quote quote, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, quote, performingUserId, createdTimestamp)
            {
                this.VersionId = versionId;
                this.VersionNumber = quote.VersionNumber + 1;
            }

            [JsonConstructor]
            private QuoteVersionCreatedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the quote version.
            /// </summary>
            [JsonProperty]
            public Guid VersionId { get; private set; }

            /// <summary>
            /// Gets the version number.
            /// </summary>
            [JsonProperty]
            public int VersionNumber { get; private set; }
        }
    }
}
