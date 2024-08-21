﻿// <copyright file="QuoteEventAggregator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Aggregates
{
    using System;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Persistence;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.Claim;
    using UBind.Persistence.ReadModels.Quote;

    /// <summary>
    /// Quote event aggregator that passes quote events to the quote read model writer and the quote event integration scheduler.
    /// </summary>
    public class QuoteEventAggregator : EventAggregator<QuoteAggregate, Guid>, IQuoteEventObserver
    {
        public QuoteEventAggregator(
            IQuoteVersionReadModelWriter quoteVersionReadModelWriter,
            IQuoteReadModelWriter quoteReadModelWriter,
            IPolicyReadModelWriter policyReadModelWriter,
            IQuoteDocumentReadModelWriter quoteDocumentReadModelWriter,
            IQuoteEventIntegrationScheduler eventIntegrationScheduler,
            IQuoteSystemEventEmitter quoteSystemEventEmitter,
            ITextAdditionalPropertyValueReadModelWriter textAdditionalPropertyValueReadModelWriter,
            IClaimReadModelWriter claimReadModelWriter)
            : base(
                  quoteVersionReadModelWriter,
                  quoteReadModelWriter,
                  policyReadModelWriter,
                  quoteDocumentReadModelWriter,
                  eventIntegrationScheduler,
                  quoteSystemEventEmitter,
                  textAdditionalPropertyValueReadModelWriter,
                  claimReadModelWriter)
        {
        }
    }
}
