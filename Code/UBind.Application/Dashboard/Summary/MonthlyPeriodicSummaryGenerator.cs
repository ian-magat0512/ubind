// <copyright file="MonthlyPeriodicSummaryGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard;

using NodaTime;
using StackExchange.Profiling;
using UBind.Application.Dashboard.Configuration;
using UBind.Application.Dashboard.Model;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;

/// <summary>
/// Generates monthly summaries from a collection of records.
/// </summary>
public class MonthlyPeriodicSummaryGenerator<TRecord, TSummary> : PeriodicSummaryGenerator<TRecord, TSummary>
    where TSummary : IPeriodicSummaryModel, new()
    where TRecord : IDashboardSummaryModel, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MonthlyPeriodicSummaryGenerator"/> class.
    /// </summary>
    /// <param name="includePropertiesSetter">Sets the properties to be included for each summary with records.</param>
    /// <param name="emptySummaryIncludePropertiesSetter">Sets the properties to be included for each empty summary.</param>
    public MonthlyPeriodicSummaryGenerator(
            ZonedDateTime fromDate,
            ZonedDateTime toDate,
            DateTimeZone timeZone,
            Func<IGrouping<dynamic, TRecord>, TSummary, TSummary> includePropertiesSetter,
            Func<TSummary, TSummary> emptySummaryIncludePropertiesSetter)
    {
        this.InitialisePropertySetters(includePropertiesSetter, emptySummaryIncludePropertiesSetter);
        this.SummaryConfiguration = new MonthlyPeriodicSummaryGeneratorConfiguration(fromDate, toDate, timeZone);
    }

    /// <inheritdoc/>
    public override List<TSummary> GenerateSummary(IEnumerable<TRecord> records)
    {
        using (MiniProfiler.Current.Step(nameof(MonthlyPeriodicSummaryGenerator<TRecord, TSummary>) + "." + nameof(this.GenerateSummary)))
        {
            var recordsByMonth = records
                .GroupBy(x => new
                {
                    Date = x.Timestamp.ToStartOfMonthInZone(this.SummaryConfiguration.TimeZone),
                })
                .Select(this.ApplySelector((q) => q.Date))
                .ToList();
            this.CompleteListWithEmptySampleSets(recordsByMonth);
            return recordsByMonth.OrderBy(c => c.FromDateTime).ToList();
        }
    }
}
