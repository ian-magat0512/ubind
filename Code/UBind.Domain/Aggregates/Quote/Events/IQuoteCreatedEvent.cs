// <copyright file="IQuoteCreatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Events which implement this empty interface are denominated as events which create a new quote.
    /// This is used in the QuoteReadModelWriter, during replaying of events, we'll skip the creation
    /// of a new quote read model for these events.
    /// </summary>
    public interface IQuoteCreatedEvent
    {
        /// <summary>
        /// Gets the ID of the quote.
        /// </summary>
        [JsonProperty]
        Guid QuoteId { get; }
    }
}
