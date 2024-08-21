// <copyright file="DocumentGeneratedEvent.cs" company="uBind">
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
        /// Base class for document generation events.
        /// </summary>
        /// <typeparam name="TEvent">The type of the derived class.</typeparam>
        public abstract class DocumentGeneratedEvent<TEvent>
            : Event<QuoteAggregate, Guid>
            where TEvent : DocumentGeneratedEvent<TEvent>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DocumentGeneratedEvent{TEvent}"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="quoteId">The ID of the quote aggregate.</param>
            /// <param name="document">The document.</param>
            /// <param name="documentTargetId">The ID of the target of the document (a quote or policy transaction).</param>
            /// <param name="performingUserId">The userId who generated document.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public DocumentGeneratedEvent(Guid tenantId, Guid quoteId, QuoteDocument document, Guid documentTargetId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, quoteId, performingUserId, createdTimestamp)
            {
                this.Document = document;
                this.DocumentTargetId = documentTargetId;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DocumentGeneratedEvent{TEvent}"/> class.
            /// </summary>
            [JsonConstructor]
            protected DocumentGeneratedEvent()
            {
            }

            /// <summary>
            /// Gets the content of the document.
            /// </summary>
            [JsonProperty]
            public QuoteDocument Document { get; private set; }

            /// <summary>
            /// Gets ID of the target the document is for (a quote or a policy transaction).
            /// </summary>
            [JsonProperty]
            protected Guid DocumentTargetId { get; private set; }
        }
    }
}
