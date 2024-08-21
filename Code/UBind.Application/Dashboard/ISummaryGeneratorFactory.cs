// <copyright file="ISummaryGeneratorFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard;

using UBind.Application.Dashboard.Model;
using UBind.Application.Dashboard.Summary;
using UBind.Domain.Enums;
using UBind.Domain.ReadModel;

/// <summary>
/// Interface for creating periodic summaries for policy transactions, quotes and claims.
/// To be used in the dashboard.
/// </summary>
/// <typeparam name="TRecord">The records to summarise.</typeparam>
/// <typeparam name="TSummary">The summary of a collection of records.</typeparam>
public interface ISummaryGeneratorFactory<TRecord, TSummary>
    where TSummary : IPeriodicSummaryModel, new()
    where TRecord : IDashboardSummaryModel, new()
{
    /// <summary>
    /// Sets the include properties.
    /// </summary>
    /// <param name="includeProperties">Properties to be included in the summary.</param>
    ISummaryGeneratorFactory<TRecord, TSummary> WithIncludeProperties(IEnumerable<string> includeProperties);

    /// <summary>
    /// Initialises and returns a periodic summary generator from the period type, dates and custom period length provided.
    /// </summary>
    /// <paramref name="periodLengthInMinutes">The length of period in Minutes if the periodType is custom.</paramref>
    IPeriodicSummaryGenerator<TRecord, TSummary> ForPeriodAndDates(SamplePeriodLength periodType, string fromDate, string toDate, string timezone, int? periodLengthInMinutes = null);

    /// <summary>
    /// Gets the number of expected periods from the period type and dates and an optional period length in minutes provided.
    /// </summary>
    /// <param name="periodType">Identifies a predetermined length of period (day, month, quarter, year),
    /// a single period (if it is "all") or a custom period length.</param>
    /// <param name="fromDateTime">The start date of the summary.</param>
    /// <param name="toDateTime">The end date of the summary.</param>
    /// <param name="timeZoneId">The timezone.</param>
    /// <param name="customPeriodInMinutes">The period length in minutes if the periodType is custom.</param>
    /// <returns></returns>
    long GetNumberOfExpectedPeriods(SamplePeriodLength periodType, string fromDateTime, string toDateTime, string timeZoneId, int? customPeriodInMinutes = null);
}