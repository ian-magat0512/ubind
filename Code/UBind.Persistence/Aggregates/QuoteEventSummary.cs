// <copyright file="QuoteEventSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Aggregates
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Events;
    using UBind.Domain.Repositories;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    /// <inheritdoc/>
    public class QuoteEventSummary : IQuoteEventSummary
    {
        /// <inheritdoc/>
        public int SequenceNumber { get; set; }

        /// <inheritdoc/>
        public Instant Timestamp { get; set; }

        /// <inheritdoc/>
        public string EventType { get; set; }

        /// <inheritdoc/>
        public string WorkflowStepAssigned { get; set; }

        public static List<QuoteEventSummary> CreateFromEvents(EventRecordWithGuidId eventRecord)
        {
            var @event = eventRecord.GetEvent<IEvent<QuoteAggregate, Guid>, QuoteAggregate>();
            var eventTypes = QuoteEventTypeMap.Map(@event);
            var workflowEvent = @event as WorkflowStepAssignedEvent;
            string workflowStepAssigned = null;
            if (workflowEvent != null)
            {
                workflowStepAssigned = workflowEvent.WorkflowStep;
            }

            List<QuoteEventSummary> summaries = new List<QuoteEventSummary>();
            foreach (var eventType in eventTypes)
            {
                summaries.Add(
                    new QuoteEventSummary
                    {
                        SequenceNumber = eventRecord.Sequence,
                        Timestamp = eventRecord.Timestamp,
                        WorkflowStepAssigned = workflowStepAssigned,
                        EventType = eventType.ToString(),
                    });
            }

            return summaries;
        }
    }
}
