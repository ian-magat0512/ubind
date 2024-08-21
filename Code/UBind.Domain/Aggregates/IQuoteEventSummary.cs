// <copyright file="IQuoteEventSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates
{
    using NodaTime;

    /// <summary>
    /// A model referencing 0event summary details.
    /// </summary>
    public interface IQuoteEventSummary
    {
        /// <summary>
        /// Gets the sequence number.
        /// </summary>
        int SequenceNumber { get; }

        /// <summary>
        /// Gets the created timestamp.
        /// </summary>
        Instant Timestamp { get; }

        /// <summary>
        /// Gets the type of event.
        /// </summary>
        string EventType { get; }

        /// <summary>
        /// Gets the workflow step assigned by the event, if any, otherwise null.
        /// </summary>
        string WorkflowStepAssigned { get; }
    }
}
