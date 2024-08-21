// <copyright file="QuoteEventPayload.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.SystemEvents.Payload
{
    using System;

    /// <summary>
    /// The system event payload specific for quote events.
    /// </summary>
    public class QuoteEventPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEventPayload"/> class.
        /// </summary>
        /// <param name="policyId">The policy id.</param>
        /// <param name="quoteId">The quote id.</param>
        public QuoteEventPayload(
            Guid? policyId,
            Guid quoteId)
        {
            this.PolicyId = policyId;
            this.QuoteId = quoteId;
        }

        /// <summary>
        /// Gets the Event Identification.
        /// </summary>
        public Guid QuoteId { get; }

        /// <summary>
        /// Gets the serialized payload.
        /// </summary>
        public Guid? PolicyId { get; }
    }
}
