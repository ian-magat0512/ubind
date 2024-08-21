// <copyright file="CalculationResultCreatedEvent.cs" company="uBind">
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
        /// Event raised when a quote has been created.
        /// </summary>
        public class CalculationResultCreatedEvent : Event<QuoteAggregate, Guid>
        {
            private Guid quoteId;

            /// <summary>
            /// Initializes a new instance of the <see cref="CalculationResultCreatedEvent"/> class.
            /// </summary>
            /// <param name="tenantId">The ID of the tenant.</param>
            /// <param name="aggregateId">The ID of the quote aggregate.</param>
            /// <param name="quoteId">The ID of the quote.</param>
            /// <param name="calculationResult">The calculation result.</param>
            /// <param name="performingUserId">The userId who triggered calculation.</param>
            /// <param name="createdTimestamp">A created timestamp.</param>
            public CalculationResultCreatedEvent(Guid tenantId, Guid aggregateId, Guid quoteId, ReadWriteModel.CalculationResult calculationResult, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, aggregateId, performingUserId, createdTimestamp)
            {
                if (calculationResult == null)
                {
                    throw new ArgumentNullException(nameof(calculationResult), "Calculation result should have value.");
                }

                this.QuoteId = quoteId;
                this.CalculationResultId = Guid.NewGuid();
                this.CalculationResult = calculationResult;
            }

            [JsonConstructor]
            private CalculationResultCreatedEvent()
            {
            }

            /// <summary>
            /// Gets an ID uniquely identifying the quote whose form data was updated.
            /// </summary>
            [JsonProperty]
            public Guid QuoteId
            {
                get
                {
                    // Quote ID will not exist for quotes created before introduction of renewals and updates, in which case the ID
                    // of the aggregate will be used.
                    return this.quoteId == default
                        ? this.AggregateId
                        : this.quoteId;
                }

                private set
                {
                    this.quoteId = value;
                }
            }

            /// <summary>
            /// Gets an ID uniquely identifying the calculation result.
            /// </summary>
            [JsonProperty]
            public Guid CalculationResultId { get; private set; }

            /// <summary>
            /// Gets an Id uniquely identifying the form update used to create the calculation result.
            /// </summary>
            [JsonProperty]
            public ReadWriteModel.CalculationResult CalculationResult { get; private set; }
        }
    }
}
