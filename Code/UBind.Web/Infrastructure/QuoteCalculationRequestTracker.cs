// <copyright file="QuoteCalculationRequestTracker.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// This class is needed because we need to track all the calculation requests sent to application.
    /// If the client send quote calculation while the previous calculation is not yet finish,
    /// we will cancel the previous calculation to avoid memory and concurrency issues.
    /// This class is registered in startup as singleton.
    /// </summary>
    public class QuoteCalculationRequestTracker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCalculationRequestTracker"/> class.
        /// </summary>
        public QuoteCalculationRequestTracker()
        {
            this.Requests = new ConcurrentDictionary<Guid, QuoteCalculationRequest>();
        }

        /// <summary>
        /// Gets the quote calculation tokens.
        /// </summary>
        public ConcurrentDictionary<Guid, QuoteCalculationRequest> Requests { get; private set; }
    }
}
