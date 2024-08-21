// <copyright file="QuoteStateChangedEvent.cs" company="uBind">
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
        private void Apply(QuoteStateChangedEvent @event, int sequenceNumber)
        {
            this.GetQuoteOrThrow(@event.QuoteId).Apply(@event, sequenceNumber);
        }

        /// <summary>
        /// Event raised when a quote's state has been changed.
        /// </summary>
        public class QuoteStateChangedEvent : Event<QuoteAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteStateChangedEvent"/> class.
            /// </summary>
            /// <param name="aggregateId">The ID of the aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="operationName">The name of the operation performed that triggered the state change.</param>
            /// <param name="performingUserId">The ID of the user that performed the action that triggered the state change.</param>
            /// <param name="originalState">The original state of the quote prior to state change.</param>
            /// <param name="resultingState">The resulting state of the quote after the state change.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public QuoteStateChangedEvent(Guid tenantId, Guid aggregateId, Guid quoteId, QuoteAction operationName, Guid? performingUserId, string originalState, string resultingState, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                this.QuoteId = quoteId;
                this.OperationName = operationName.Humanize();
                this.UserId = performingUserId;
                this.OriginalState = originalState?.Pascalize();
                this.ResultingState = resultingState?.Pascalize();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="QuoteStateChangedEvent"/> class.
            /// </summary>
            /// <remarks>Constructor added as backward compatibility when <see cref="QuoteAction"/> Quote operation
            /// was renamed to <see cref="QuoteAction.Actualise"/>Renamed from "quote" to "actualise".</remarks>
            [JsonConstructor]
            private QuoteStateChangedEvent()
            {
            }

            /// <summary>
            /// Gets the ID of the quote the event is for.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId { get; private set; }

            /// <summary>
            /// Gets the name of the operation that triggered the state change.
            /// </summary>
            [JsonProperty]
            public string OperationName { get; private set; }

            /// <summary>
            /// Gets the ID of the user that triggered the state change.
            /// </summary>
            [JsonProperty]
            public Guid? UserId { get; private set; }

            /// <summary>
            /// Gets the value of the original state prior to state change.
            /// </summary>
            [JsonProperty]
            public string OriginalState { get; private set; }

            /// <summary>
            /// Gets the value of the resulting state after completion of state change.
            /// </summary>
            [JsonProperty]
            public string ResultingState { get; private set; }
        }
    }
}
