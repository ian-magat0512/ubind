// <copyright file="ClaimSummaryGeneratorFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard;

using UBind.Application.Dashboard.Model;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel.Claim;

public class ClaimSummaryGeneratorFactory : SummaryGeneratorFactory<ClaimDashboardSummaryModel, ClaimPeriodicSummaryModel>
{
    protected override ClaimPeriodicSummaryModel IncludePropertiesSetter(IEnumerable<ClaimDashboardSummaryModel> collection, ClaimPeriodicSummaryModel summaryModel)
    {
        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.ProcessedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.ProcessedCount = collection.Count(j => j.IsSettled || j.IsDeclined);
        }

        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.SettledCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.SettledCount = collection.Count(b => b.IsSettled);
        }

        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.DeclinedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.DeclinedCount = collection.Count(j => j.IsDeclined);
        }

        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.AverageProcessingTime), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.AverageProcessingTime = Convert.ToSingle(Math.Round(collection.Average(x => x.ProcessingTime), 1));
        }

        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.AverageSettlementAmount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.AverageSettlementAmount = !collection.Any(p => p.IsSettled)
                ? 0
                : Convert.ToSingle(Math.Round(collection.Where(p => p.IsSettled).Average(x => x.SettledAmount), 2));
        }

        return summaryModel;
    }

    protected override ClaimPeriodicSummaryModel EmptySummaryIncludePropertiesSetter(ClaimPeriodicSummaryModel summaryModel)
    {
        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.ProcessedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.ProcessedCount = 0;
        }

        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.SettledCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.SettledCount = 0;
        }

        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.DeclinedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.DeclinedCount = 0;
        }

        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.AverageProcessingTime), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.AverageProcessingTime = 0;
        }

        if (this.IncludeProperties.Contains(nameof(ClaimPeriodicSummaryModel.AverageSettlementAmount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.AverageSettlementAmount = 0;
        }

        return summaryModel;
    }
}