// <copyright file="PeriodicSummaryGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Dashboard;

using NodaTime;
using StackExchange.Profiling;
using UBind.Application.Dashboard.Configuration;
using UBind.Application.Dashboard.Model;
using UBind.Application.Dashboard.Summary;
using UBind.Domain.Enums;
using UBind.Domain.ReadModel;
using UBind.Domain.Extensions;

/// <inheritdoc/>
public abstract class PeriodicSummaryGenerator<TRecord, TSummary> : IPeriodicSummaryGenerator<TRecord, TSummary>
    where TSummary : IPeriodicSummaryModel, new()
    where TRecord : IDashboardSummaryModel, new()
{
    protected Func<IGrouping<dynamic, TRecord>, TSummary, TSummary> IncludePropertiesSetter { get; private set; }

    protected Func<TSummary, TSummary> EmptySummaryIncludePropertiesSetter { get; private set; }

    protected IPeriodicSummaryGeneratorConfiguration SummaryConfiguration { get; set; }

    /// <inheritdoc/>
    public abstract List<TSummary> GenerateSummary(IEnumerable<TRecord> records);

    protected void InitialisePropertySetters(
        Func<IGrouping<dynamic, TRecord>, TSummary, TSummary> includePropertiesSetter,
        Func<TSummary, TSummary> emptySummaryIncludePropertiesSetter)
    {
        this.IncludePropertiesSetter = includePropertiesSetter;
        this.EmptySummaryIncludePropertiesSetter = emptySummaryIncludePropertiesSetter;
    }

    protected Func<IGrouping<dynamic, TRecord>, TSummary> ApplySelector(Func<dynamic, ZonedDateTime> fromDate)
    {
        return (q) =>
        {
            var summarySet = new TSummary();
            var fromZonedDateTime = (ZonedDateTime)fromDate.Invoke(q.Key);
            summarySet.FromDateTime = fromZonedDateTime.ToIso8601WithUTCOffset();
            summarySet.ToDateTime = fromZonedDateTime
                .AddPeriod(this.SummaryConfiguration.TimeZone, this.SummaryConfiguration.PeriodLength)
                .ToIso8601WithUTCOffset();
            if (this.SummaryConfiguration.PeriodType != SamplePeriodLength.All)
            {
                summarySet.Label = this.SummaryConfiguration.LabelFormatter.Invoke(fromZonedDateTime);
            }

            summarySet = this.IncludePropertiesSetter.Invoke(q, summarySet);
            return summarySet;
        };
    }

    protected void CompleteListWithEmptySampleSets(List<TSummary> records)
    {
        using (MiniProfiler.Current.Step(nameof(PeriodicSummaryGenerator<TRecord, TSummary>) + "." + nameof(this.CompleteListWithEmptySampleSets)))
        {
            var periodStartDate = this.SummaryConfiguration.StartDateTime;
            var periodEndDate = this.SummaryConfiguration.StartDateTime.AddPeriod(this.SummaryConfiguration.TimeZone, this.SummaryConfiguration.PeriodLength);
            for (int i = 0; i < this.SummaryConfiguration.NumberOfExpectedPeriods; i++)
            {
                if (periodStartDate.LocalDateTime < this.SummaryConfiguration.StartDateTime.LocalDateTime ||
                    periodEndDate.LocalDateTime > this.SummaryConfiguration.EndDateTime.LocalDateTime)
                {
                    continue;
                }

                var fromDateOfPeriod = periodStartDate.ToIso8601WithUTCOffset();
                var toDateOfPeriod = periodEndDate.ToIso8601WithUTCOffset();
                var hasSummaryForThePeriod = records.Any(p => p.ToDateTime.Equals(toDateOfPeriod));
                if (!hasSummaryForThePeriod)
                {
                    var summarySet = new TSummary();
                    summarySet.Label = this.SummaryConfiguration.LabelFormatter.Invoke(periodStartDate);
                    summarySet.FromDateTime = fromDateOfPeriod;
                    summarySet.ToDateTime = toDateOfPeriod;
                    summarySet = this.EmptySummaryIncludePropertiesSetter.Invoke(summarySet);
                    records.Add(summarySet);
                }

                periodStartDate = periodStartDate.AddPeriod(this.SummaryConfiguration.TimeZone, this.SummaryConfiguration.PeriodInterval);
                periodEndDate = periodStartDate.AddPeriod(this.SummaryConfiguration.TimeZone, this.SummaryConfiguration.PeriodLength);
            }
        }
    }
}