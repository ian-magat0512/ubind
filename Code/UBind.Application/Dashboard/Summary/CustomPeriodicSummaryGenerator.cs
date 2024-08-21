// <copyright file="CustomPeriodicSummaryGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard;

using NodaTime;
using ServiceStack;
using StackExchange.Profiling;
using UBind.Application.Dashboard.Configuration;
using UBind.Application.Dashboard.Model;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;

/// <summary>
/// Generates custom summaries from a collection of records.
/// </summary>
public class CustomPeriodicSummaryGenerator<TRecord, TSummary> : PeriodicSummaryGenerator<TRecord, TSummary>
    where TSummary : IPeriodicSummaryModel, new()
    where TRecord : IDashboardSummaryModel, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomPeriodicSummaryGenerator"/> class.
    /// </summary>
    /// <param name="includePropertiesSetter">Sets the properties to be included for each summary with records.</param>
    /// <param name="emptySummaryIncludePropertiesSetter">Sets the properties to be included for each empty summary.</param>
    public CustomPeriodicSummaryGenerator(
        ZonedDateTime fromDate,
        ZonedDateTime toDate,
        DateTimeZone timeZone,
        int customSamplePeriodMinutes,
        Func<IGrouping<dynamic, TRecord>, TSummary, TSummary> includePropertiesSetter,
        Func<TSummary, TSummary> emptySummaryIncludePropertiesSetter)
    {
        this.InitialisePropertySetters(includePropertiesSetter, emptySummaryIncludePropertiesSetter);
        this.SummaryConfiguration = new CustomPeriodicSummaryGeneratorConfiguration(fromDate, toDate, timeZone, customSamplePeriodMinutes);
    }

    /// <inheritdoc/>
    public override List<TSummary> GenerateSummary(IEnumerable<TRecord> records)
    {
        using (MiniProfiler.Current.Step(nameof(CustomPeriodicSummaryGenerator<TRecord, TSummary>) + "." + nameof(this.GenerateSummary)))
        {
            var customConfig = this.SummaryConfiguration as CustomPeriodicSummaryGeneratorConfiguration;
            if (customConfig == null)
            {
                return Enumerable.Empty<TSummary>().ToList();
            }

            var startTimeStamp = customConfig.StartDateTime.ToInstant();
            var recordsByCustomPeriod = records
                .Where(x => x.Timestamp >= startTimeStamp)
                .GroupBy(x => new
                {
                    StartDate = customConfig.GetPeriodStartDateOfTimestamp(x.Timestamp),
                })
                .Select(this.ApplySelector((q) => q.StartDate))
                .ToList();
            this.CompleteListWithEmptySampleSets(recordsByCustomPeriod);
            return recordsByCustomPeriod.OrderBy(c => c.FromDateTime).ToList();
        }
    }
}