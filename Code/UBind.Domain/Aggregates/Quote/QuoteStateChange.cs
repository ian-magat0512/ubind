// <copyright file="QuoteStateChange.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using System.Text.Json.Serialization;
    using NodaTime;

    /// <summary>
    /// Stores relevant information for a quote's transition.
    /// </summary>
    public class QuoteStateChange
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteStateChange"/> class.
        /// </summary>
        /// <param name="operationName">The name of the operation performed that triggered the state change.</param>
        /// <param name="userId">The ID of the user that performed the action that triggered the state change.</param>
        /// <param name="originalState">The original state of the quote prior to state change.</param>
        /// <param name="resultingState">The resulting state of the quote after the state change.</param>
        /// <param name="createdTimestamp">A timestamp.</param>
        [JsonConstructor]
        public QuoteStateChange(string operationName, Guid? userId, string originalState, string resultingState, Instant createdTimestamp)
        {
            this.OperationName = operationName;
            this.UserId = userId;
            this.OriginalState = originalState;
            this.ResultingState = resultingState;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Gets the name of the operation that triggered the state change.
        /// </summary>
        public string OperationName { get; private set; }

        /// <summary>
        /// Gets the ID of the user that triggered the state change.
        /// </summary>
        public Guid? UserId { get; private set; }

        /// <summary>
        /// Gets the value of the original state prior to state change.
        /// </summary>
        public string OriginalState { get; private set; }

        /// <summary>
        /// Gets the value of the resulting state after completion of state change.
        /// </summary>
        public string ResultingState { get; private set; }

        /// <summary>
        /// Gets the time the status change was completed.
        /// </summary>
        public Instant CreatedTimestamp { get; private set; }
    }
}
