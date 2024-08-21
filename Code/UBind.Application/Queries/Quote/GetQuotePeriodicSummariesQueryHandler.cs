// <copyright file="GetQuotePeriodicSummariesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Quote
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Dashboard;
    using UBind.Application.Dashboard.Model;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Command handler for obtaining a list of quote summaries with the given filters.
    /// </summary>
    public class GetQuotePeriodicSummariesQueryHandler :
        IQueryHandler<GetQuotePeriodicSummariesQuery, List<QuotePeriodicSummaryModel>>
    {
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly ISummaryGeneratorFactory<QuoteDashboardSummaryModel, QuotePeriodicSummaryModel> periodicSummaryGeneratorFactory;

        public GetQuotePeriodicSummariesQueryHandler(
            IQuoteReadModelRepository quoteReadModelRepository,
            ISummaryGeneratorFactory<QuoteDashboardSummaryModel, QuotePeriodicSummaryModel> periodicSummaryGeneratorFactory)
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.periodicSummaryGeneratorFactory = periodicSummaryGeneratorFactory;
        }

        public async Task<List<QuotePeriodicSummaryModel>> Handle(GetQuotePeriodicSummariesQuery request, CancellationToken cancellationToken)
        {
            var quotes = await this.quoteReadModelRepository.ListQuotesForPeriodicSummary(request.TenantId, request.Filters, cancellationToken);
            var includeProperties = request.Options.IncludeProperties ?? Enumerable.Empty<string>();
            var periodType = request.Options.SamplePeriodLength;
            var startDateTime = request.Options.FromDateTime;
            if (startDateTime == null && quotes.Any())
            {
                request.Options.SetFromDateTime(quotes.FirstOrDefault()?.CreatedTimestamp);
                startDateTime = request.Options.FromDateTime;
            }

            var endDateTime = request.Options.ToDateTime;
            var result = new List<QuotePeriodicSummaryModel>();
            if (periodType == null || startDateTime == null || endDateTime == null)
            {
                return result;
            }

            var periodicSummaryGenerator = this.periodicSummaryGeneratorFactory
                .WithIncludeProperties(includeProperties)
                .ForPeriodAndDates(
                    periodType.Value,
                    startDateTime,
                    endDateTime,
                    request.Options.TimeZoneId,
                    request.Options.CustomSamplePeriodMinutes);
            result = periodicSummaryGenerator.GenerateSummary(quotes);
            return result;
        }
    }
}