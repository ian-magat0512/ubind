// <copyright file="ApplicationEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// Domain event relating to an application.
    /// </summary>
    public class ApplicationEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEvent"/> class.
        /// </summary>
        /// <param name="integrationEventId">The integration event ID.</param>
        /// <param name="eventType">The event type.</param>
        /// <param name="quoteAggregate">The quote aggregate.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="eventSequenceNumber">The sequence number of the event within its aggregate.</param>
        /// <param name="jobId">The ID of the Hangfire job executing the integration (used to allow storing of data in the job).</param>
        /// <param name="isRetriggering">A value indicating whether the event was re-triggered by the replay feature. Default value is false.</param>
        public ApplicationEvent(
            Guid integrationEventId,
            ApplicationEventType eventType,
            QuoteAggregate quoteAggregate,
            Guid quoteId,
            int eventSequenceNumber,
            string jobId,
            Guid productReleaseId,
            bool isRetriggering = false)
        {
            this.JobId = jobId;
            this.IntegrationEventId = integrationEventId;
            this.EventType = eventType;
            this.Aggregate = quoteAggregate;
            this.EventSequenceNumber = eventSequenceNumber;
            this.IsRetriggering = isRetriggering;
            this.QuoteId = quoteId;
            this.ProductReleaseId = productReleaseId;
        }

        /// <summary>
        /// Gets The job id.
        /// </summary>
        public string JobId { get; }

        /// <summary>
        /// Gets the integration event Id.
        /// </summary>
        public Guid IntegrationEventId { get; }

        /// <summary>
        /// Gets the event type.
        /// </summary>
        public ApplicationEventType EventType { get; }

        /// <summary>
        /// Gets the application the event refers to.
        /// </summary>
        public QuoteAggregate Aggregate { get; }

        /// <summary>
        /// Gets the sequence number of the event within its aggregate.
        /// </summary>
        public int EventSequenceNumber { get; }

        /// <summary>
        /// Gets the quote id of the event within its aggregate.
        /// </summary>
        public Guid QuoteId { get; }

        /// <summary>
        /// Gets a value indicating whether it is retriggering or not.
        /// </summary>
        public bool IsRetriggering { get; }

        public Guid ProductReleaseId { get; }
    }
}
