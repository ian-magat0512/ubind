// <copyright file="QuoteCalculationRequest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ValueTypes
{
    using System;
    using System.Threading;
    using NodaTime;

    /// <summary>
    /// This class is needed to track all the calculation requests sent to application.
    /// </summary>
    public class QuoteCalculationRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCalculationRequest"/> class.
        /// </summary>
        /// <param name="quoteId">The quote Id.</param>
        /// <param name="tokenSource">The cancellation token source.</param>
        public QuoteCalculationRequest(Guid quoteId, CancellationTokenSource tokenSource, Instant createdTime)
        {
            this.QuoteId = quoteId;
            this.TokenSource = tokenSource;
            this.CreatedTimeInTicksSinceEpoch = createdTime.ToUnixTimeTicks();
        }

        /// <summary>
        /// Gets the quote Id.
        /// </summary>
        public Guid QuoteId { get; private set; }

        /// <summary>
        /// Gets the cancellation token source.
        /// </summary>
        public CancellationTokenSource TokenSource { get; private set; }

        /// <summary>
        /// Gets the created time in ticks since epoch.
        /// </summary>
        public long CreatedTimeInTicksSinceEpoch { get; private set; }
    }
}
