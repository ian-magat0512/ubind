// <copyright file="AllPeriodicSummaryGenerator.cs" company="uBind">
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
/// Generates a single summary from a collection of records.
/// </summary>
public class AllPeriodicSummaryGenerator<TRecord, TSummary> : PeriodicSummaryGenerator<TRecord, TSummary>
    where TSummary : IPeriodicSummaryModel, new()
    where TRecord : IDashboardSummaryModel, new()
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AllPeriodicSummaryGenerator"/> class.
    /// </summary>
    /// <param name="includePropertiesSetter">Sets the properties to be included for each summary with records.</param>
    /// <param name="emptySummaryIncludePropertiesSetter">Sets the properties to be included for each empty summary.</param>
    public AllPeriodicSummaryGenerator(
        ZonedDateTime fromDate,
        ZonedDateTime toDate,
        DateTimeZone timeZone,
        Func<IGrouping<dynamic, TRecord>, TSummary, TSummary> includePropertiesSetter,
        Func<TSummary, TSummary> emptySummaryIncludePropertiesSetter)
    {
        this.InitialisePropertySetters(includePropertiesSetter, emptySummaryIncludePropertiesSetter);
        this.SummaryConfiguration = new AllPeriodicSummaryGeneratorConfiguration(fromDate, toDate, timeZone);
    }

    /// <inheritdoc/>
    public override List<TSummary> GenerateSummary(IEnumerable<TRecord> records)
    {
        using (MiniProfiler.Current.Step(nameof(AllPeriodicSummaryGenerator<TRecord, TSummary>) + "." + nameof(this.GenerateSummary)))
        {
            if (records.Any())
            {
                var result = records
                .GroupBy(x => new { })
                .Select(this.ApplySelector((q) => this.SummaryConfiguration.StartDateTime))
                .ToList();
                return result;
            }

            var emptySet = new TSummary()
            {
                FromDateTime = this.SummaryConfiguration.StartDateTime.ToIso8601WithUTCOffset(),
                ToDateTime = this.SummaryConfiguration.EndDateTime.ToIso8601WithUTCOffset(),
            };
            emptySet = this.EmptySummaryIncludePropertiesSetter(emptySet);
            return new List<TSummary>() { emptySet };
        }
    }
}