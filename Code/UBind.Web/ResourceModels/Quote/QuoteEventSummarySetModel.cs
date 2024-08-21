// <copyright file="QuoteEventSummarySetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Resource model for returning set of events for an aggregate.
    /// </summary>
    public class QuoteEventSummarySetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEventSummarySetModel"/> class.
        /// </summary>
        /// <param name="policyId">The ID of the policy.</param>
        /// <param name="events">The event collection.</param>
        public QuoteEventSummarySetModel(Guid policyId, IEnumerable<IQuoteEventSummary> events)
        {
            this.AggregateId = policyId;
            this.Events = events.Select(eventSummary => new QuoteEventSummaryModel(eventSummary));
        }

        /// <summary>
        /// Gets the ID of the aggregate the events are for.
        /// </summary>
        public Guid AggregateId { get; private set; }

        /// <summary>
        /// Gets a collection of events for the aggregate.
        /// </summary>
        public IEnumerable<QuoteEventSummaryModel> Events { get; private set; }
    }
}
