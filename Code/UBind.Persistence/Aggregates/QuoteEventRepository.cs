// <copyright file="QuoteEventRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Aggregates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class QuoteEventRepository : IQuoteEventRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEventRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public QuoteEventRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IEnumerable<IQuoteEventSummary> GetEventSummaries(Guid aggregateId)
        {
            var events = this.dbContext.EventRecordsWithGuidIds
                .Where(record => record.AggregateId.Equals(aggregateId))
                .OrderBy(record => record.Sequence)
                .ToList()
                .SelectMany(rec => QuoteEventSummary.CreateFromEvents(rec));
            if (events.Any())
            {
                return events;
            }

            return Enumerable.Empty<IQuoteEventSummary>();
        }
    }
}
