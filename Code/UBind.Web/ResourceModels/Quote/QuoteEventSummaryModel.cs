// <copyright file="QuoteEventSummaryModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Resource model for serving quote aggregate events.
    /// </summary>
    public class QuoteEventSummaryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEventSummaryModel"/> class.
        /// </summary>
        /// <param name="event">The event summary.</param>
        public QuoteEventSummaryModel(IQuoteEventSummary @event)
        {
            this.SequenceNumber = @event.SequenceNumber;
            this.EventType = @event.EventType;
            this.CreatedDateTime = @event.Timestamp.ToExtendedIso8601String();
            this.WorkflowStep = @event.WorkflowStepAssigned;
        }

        /// <summary>
        /// Gets the sequence number of the event.
        /// </summary>
        public int SequenceNumber { get; private set; }

        /// <summary>
        /// Gets the date and time the event was created.
        /// </summary>
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the type of event.
        /// </summary>
        public string EventType { get; private set; }

        /// <summary>
        /// Gets the workflow step, if applicable, otherwise null.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string WorkflowStep { get; private set; }
    }
}
