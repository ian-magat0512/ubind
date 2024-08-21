// <copyright file="QuoteRollbackEvent.cs" company="uBind">
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
        /// Event raised when a quote is to be rolled back to an earlier state (or sequence number).
        /// </summary>
        public class QuoteRollbackEvent : Event<QuoteAggregate, Guid>, IAggregateRollbackEvent<QuoteAggregate, Guid>
        {
            private Guid quoteId;
            private Func<IEnumerable<IEvent<QuoteAggregate, Guid>>> replayEventsCallback;
            private Func<IEnumerable<IEvent<QuoteAggregate, Guid>>> strippedEventsCallback;

            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteRollbackEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The tenant Id.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quoteId">The ID of the aggregate.</param>
            /// <param name="rollbackToSequenceNumber">The sequence number to roll back to.</param>
            /// <param name="performingUserId">The userId who rolls back the quote.</param>
            /// <param name="timestamp">A timestamp.</param>
            /// <param name="replayEventsCallback">A function that returns events left after the rollback.</param>
            /// <param name="strippedEventsCallback">A function that returns events being ignored due to rollbacks.</param>
            public QuoteRollbackEvent(
                Guid tenantId,
                Guid aggregateId,
                Guid quoteId,
                int rollbackToSequenceNumber,
                Guid? performingUserId,
                Instant timestamp,
                Func<IEnumerable<IEvent<QuoteAggregate, Guid>>> replayEventsCallback,
                Func<IEnumerable<IEvent<QuoteAggregate, Guid>>> strippedEventsCallback)
                : base(tenantId, aggregateId, performingUserId, timestamp)
            {
                this.quoteId = quoteId;
                this.RollbackToSequenceNumber = rollbackToSequenceNumber;
                this.replayEventsCallback = replayEventsCallback;
                this.strippedEventsCallback = strippedEventsCallback;
            }

            [JsonConstructor]
            private QuoteRollbackEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote aggregate which will be rolled back.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId
            {
                get
                {
                    // Quote ID will not exist for quotes created before introduction of renewals and updates, in which case the ID
                    // of the aggregate will be used.
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
            /// Gets the sequence number to roll back to.
            /// </summary>
            [JsonProperty]
            public int RollbackToSequenceNumber { get; private set; }

            /// <summary>
            /// Gets the events after the rolled back events have been stripped out.
            /// </summary>
            [JsonIgnore]
            public IEnumerable<IEvent<QuoteAggregate, Guid>> ReplayEvents
            {
                get
                {
                    return this.replayEventsCallback();
                }
            }

            [JsonIgnore]
            public IEnumerable<IEvent<QuoteAggregate, Guid>> StrippedEvents
            {
                get
                {
                    return this.strippedEventsCallback();
                }
            }
        }
    }
}
