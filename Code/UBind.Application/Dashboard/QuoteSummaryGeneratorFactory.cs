// <copyright file="QuoteSummaryGeneratorFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard;

using UBind.Application.Dashboard.Model;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;

/// <summary>
/// The summary generator factory for quotes.
/// This class defines the specific properties to be included for quote summaries.
/// </summary>
public class QuoteSummaryGeneratorFactory : SummaryGeneratorFactory<QuoteDashboardSummaryModel, QuotePeriodicSummaryModel>
{
    protected override QuotePeriodicSummaryModel IncludePropertiesSetter(IEnumerable<QuoteDashboardSummaryModel> collection, QuotePeriodicSummaryModel summaryModel)
    {
        if (this.IncludeProperties.Contains(nameof(QuotePeriodicSummaryModel.CreatedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.CreatedCount = collection.Count();
        }

        if (this.IncludeProperties.Contains(nameof(QuotePeriodicSummaryModel.CreatedTotalPremium), StringComparer.InvariantCultureIgnoreCase))
        {
            // This is to ensure we get at most 2 decimal places
            summaryModel.CreatedTotalPremium = Convert.ToSingle(Math.Round(collection.Sum(b => b.Amount), 2));
        }

        if (this.IncludeProperties.Contains(nameof(QuotePeriodicSummaryModel.ConvertedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.ConvertedCount = collection.Count(j => j.IsCompleted);
        }

        if (this.IncludeProperties.Contains(nameof(QuotePeriodicSummaryModel.AbandonedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.AbandonedCount = collection.Count(x => x.IsAbandoned);
        }

        return summaryModel;
    }

    protected override QuotePeriodicSummaryModel EmptySummaryIncludePropertiesSetter(QuotePeriodicSummaryModel summaryModel)
    {
        if (this.IncludeProperties.Contains(nameof(QuotePeriodicSummaryModel.CreatedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.CreatedCount = 0;
        }

        if (this.IncludeProperties.Contains(nameof(QuotePeriodicSummaryModel.CreatedTotalPremium), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.CreatedTotalPremium = 0;
        }

        if (this.IncludeProperties.Contains(nameof(QuotePeriodicSummaryModel.ConvertedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.ConvertedCount = 0;
        }

        if (this.IncludeProperties.Contains(nameof(QuotePeriodicSummaryModel.AbandonedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.AbandonedCount = 0;
        }

        return summaryModel;
    }
}