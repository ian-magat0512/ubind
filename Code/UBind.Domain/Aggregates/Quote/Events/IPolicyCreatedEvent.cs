// <copyright file="IPolicyCreatedEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using NodaTime;

    /// <summary>
    /// This event allows us to know if it's an event which creates a policy.
    /// This is needed so that during a replay, so we can identify the events which require a policy to be created,
    /// because those have special handling.
    /// </summary>
    public interface IPolicyCreatedEvent
    {
        public QuoteDataSnapshot DataSnapshot { get; }

        public LocalDateTime InceptionDateTime { get; set; }

        public LocalDateTime? ExpiryDateTime { get; set; }
    }
}
