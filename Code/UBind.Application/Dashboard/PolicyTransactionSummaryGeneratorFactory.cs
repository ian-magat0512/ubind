// <copyright file="PolicyTransactionSummaryGeneratorFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard;

using UBind.Application.Dashboard.Model;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;

/// <summary>
/// The summary generator factory for policy transaction.
/// This class defines the specific properties to be included for policy transaction summaries.
/// </summary>
public class PolicyTransactionSummaryGeneratorFactory : SummaryGeneratorFactory<PolicyTransactionDashboardSummaryModel, PolicyTransactionPeriodicSummaryModel>
{
    protected override PolicyTransactionPeriodicSummaryModel IncludePropertiesSetter(IEnumerable<PolicyTransactionDashboardSummaryModel> collection, PolicyTransactionPeriodicSummaryModel summaryModel)
    {
        if (this.IncludeProperties.Contains(nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.CreatedCount = collection.Count();
        }

        if (this.IncludeProperties.Contains(nameof(PolicyTransactionPeriodicSummaryModel.CreatedTotalPremium), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.CreatedTotalPremium = Convert.ToSingle(Math.Round(collection.Sum(b => b.Amount), 2));
        }

        return summaryModel;
    }

    protected override PolicyTransactionPeriodicSummaryModel EmptySummaryIncludePropertiesSetter(PolicyTransactionPeriodicSummaryModel summaryModel)
    {
        if (this.IncludeProperties.Contains(nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.CreatedCount = 0;
        }

        if (this.IncludeProperties.Contains(nameof(PolicyTransactionPeriodicSummaryModel.CreatedTotalPremium), StringComparer.InvariantCultureIgnoreCase))
        {
            summaryModel.CreatedTotalPremium = 0;
        }

        return summaryModel;
    }
}