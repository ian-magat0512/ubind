// <copyright file="SummaryGeneratorFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Dashboard;

using System.Net;
using UBind.Application.Dashboard.Configuration;
using UBind.Application.Dashboard.Model;
using UBind.Application.Dashboard.Summary;
using UBind.Domain;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.ReadModel;

/// <summary>
/// The abstract factory class for generating periodic summaries.
/// </summary>
/// <typeparam name="TRecord">The records to summarise.</typeparam>
/// <typeparam name="TSummary">The summary of a collection of records.</typeparam>
public abstract class SummaryGeneratorFactory<TRecord, TSummary> : ISummaryGeneratorFactory<TRecord, TSummary>
    where TSummary : IPeriodicSummaryModel, new()
    where TRecord : IDashboardSummaryModel, new()
{

    protected IEnumerable<string> IncludeProperties { get; private set; }

    /// <inheritdoc/>
    public ISummaryGeneratorFactory<TRecord, TSummary> WithIncludeProperties(IEnumerable<string> includeProperties)
    {
        this.IncludeProperties = includeProperties;
        return this;
    }

    /// <inheritdoc/>
    public IPeriodicSummaryGenerator<TRecord, TSummary> ForPeriodAndDates(SamplePeriodLength periodType, string fromDateTime, string toDateTime, string timeZoneId, int? periodLengthInMinutes = null)
    {
        var timeZone = Timezones.GetTimeZoneByIdOrNull(timeZoneId);
        var fromZonedDateTime = fromDateTime.ToZonedDateTimeFromExtendedISO8601(timeZone);
        var toZonedDateTime = toDateTime.ToZonedDateTimeFromExtendedISO8601(timeZone);
        IPeriodicSummaryGenerator<TRecord, TSummary> periodicSummaryGenerator = null;
        switch (periodType)
        {
            case SamplePeriodLength.All:
                periodicSummaryGenerator = new AllPeriodicSummaryGenerator<TRecord, TSummary>(
                    fromZonedDateTime,
                    toZonedDateTime,
                    timeZone,
                    this.IncludePropertiesSetter,
                    this.EmptySummaryIncludePropertiesSetter);
                break;
            case SamplePeriodLength.Day:
                periodicSummaryGenerator = new DailyPeriodicSummaryGenerator<TRecord, TSummary>(
                    fromZonedDateTime,
                    toZonedDateTime,
                    timeZone,
                    this.IncludePropertiesSetter,
                    this.EmptySummaryIncludePropertiesSetter);
                break;
            case SamplePeriodLength.Month:
                periodicSummaryGenerator = new MonthlyPeriodicSummaryGenerator<TRecord, TSummary>(
                    fromZonedDateTime,
                    toZonedDateTime,
                    timeZone,
                    this.IncludePropertiesSetter,
                    this.EmptySummaryIncludePropertiesSetter);
                break;
            case SamplePeriodLength.Quarter:
                periodicSummaryGenerator = new QuarterlyPeriodicSummaryGenerator<TRecord, TSummary>(
                    fromZonedDateTime,
                    toZonedDateTime,
                    timeZone,
                    this.IncludePropertiesSetter,
                    this.EmptySummaryIncludePropertiesSetter);
                break;
            case SamplePeriodLength.Year:
                periodicSummaryGenerator = new YearlyPeriodicSummaryGenerator<TRecord, TSummary>(
                    fromZonedDateTime,
                    toZonedDateTime,
                    timeZone,
                    this.IncludePropertiesSetter,
                    this.EmptySummaryIncludePropertiesSetter);
                break;
            case SamplePeriodLength.Custom:
                periodicSummaryGenerator = new CustomPeriodicSummaryGenerator<TRecord, TSummary>(
                    fromZonedDateTime,
                    toZonedDateTime,
                    timeZone,
                    periodLengthInMinutes.Value,
                    this.IncludePropertiesSetter,
                    this.EmptySummaryIncludePropertiesSetter);
                break;
            default:
                break;
        }

        return periodicSummaryGenerator;
    }

    /// <inheritdoc/>
    public long GetNumberOfExpectedPeriods(SamplePeriodLength periodType, string fromDateTime, string toDateTime, string timeZoneId, int? customPeriodInMinutes = null)
    {
        var timeZone = Timezones.GetTimeZoneByIdOrNull(timeZoneId);
        var fromZonedDateTime = fromDateTime.ToZonedDateTimeFromExtendedISO8601(timeZone);
        var toZonedDateTime = toDateTime.ToZonedDateTimeFromExtendedISO8601(timeZone);
        IPeriodicSummaryGeneratorConfiguration summaryConfig;
        switch (periodType)
        {
            case SamplePeriodLength.All:
                summaryConfig = new AllPeriodicSummaryGeneratorConfiguration(fromZonedDateTime, toZonedDateTime, timeZone);
                break;
            case SamplePeriodLength.Day:
                summaryConfig = new DailyPeriodicSummaryGeneratorConfiguration(fromZonedDateTime, toZonedDateTime, timeZone);
                break;
            case SamplePeriodLength.Month:
                summaryConfig = new MonthlyPeriodicSummaryGeneratorConfiguration(fromZonedDateTime, toZonedDateTime, timeZone);
                break;
            case SamplePeriodLength.Quarter:
                summaryConfig = new QuarterlyPeriodicSummaryGeneratorConfiguration(fromZonedDateTime, toZonedDateTime, timeZone);
                break;
            case SamplePeriodLength.Year:
                summaryConfig = new YearlyPeriodicSummaryGeneratorConfiguration(fromZonedDateTime, toZonedDateTime, timeZone);
                break;
            case SamplePeriodLength.Custom:
                summaryConfig = new CustomPeriodicSummaryGeneratorConfiguration(fromZonedDateTime, toZonedDateTime, timeZone, customPeriodInMinutes.Value);
                break;
            default:
                summaryConfig = null;
                break;
        }

        if (summaryConfig == null)
        {
            throw new ErrorException(
                this.GetErrorParameterInvalid(
                    "samplePeriodLength",
                    periodType.ToString(),
                    $"The samplePeriodLength parameter value must be one of" +
                    $" \"day\", \"month\", \"quarter\", \"year\", \"all\" and \"custom\""));
        }

        return summaryConfig.NumberOfExpectedPeriods;
    }

    /// <summary>
    /// Sets the properties to be included in the summary.
    /// </summary>
    protected abstract TSummary IncludePropertiesSetter(IEnumerable<TRecord> collection, TSummary summaryModel);

    protected abstract TSummary EmptySummaryIncludePropertiesSetter(TSummary summaryModel);

    private Error GetErrorParameterInvalid(string parameterName, string value, string reason)
    {
        string details = "When trying to process your request, the attempt failed because the parameter" +
            $" \"{parameterName}\" had invalid value(s) (\"{value}\"). {reason} To resolve the issue," +
            " please ensure that all request parameters have valid values." +
            " If you require further assistance please contact technical support.";
        return new Error(
            $"request.parameter.invalid",
            $"A request parameter had an invalid value",
            details,
            HttpStatusCode.BadRequest);
    }
}