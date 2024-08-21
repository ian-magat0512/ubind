// <copyright file="IPeriodicSummaryGeneratorConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard.Configuration
{
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// This interface represents the configuration for the <see cref="IPeriodicSummaryGenerator{TRecord, TSummary}"/>.
    /// From a given start and end date, this creates a configuration that determines the number of summaries expected,
    /// the period length and interval, the format that is expected for the dates and label properties of each summary.
    /// These properties are necessary for the periodic summary generator to produce a complete collection of
    /// summaries all of which are formatted consistently.
    /// </summary>
    public interface IPeriodicSummaryGeneratorConfiguration
    {
        /// <summary>
        /// Gets the summary period type.
        /// </summary>
        SamplePeriodLength PeriodType { get; }

        /// <summary>
        /// Gets the start date of first summary.
        /// </summary>
        ZonedDateTime StartDateTime { get; }

        /// <summary>
        /// Gets the end date of the last summary
        /// and is used to get expected number of summaries.
        /// </summary>
        ZonedDateTime EndDateTime { get; }

        /// <summary>
        /// Gets the period interval between two summary.
        /// </summary>
        Period PeriodInterval { get; }

        /// <summary>
        /// Gets the period between the start and end date of a summary.
        /// </summary>
        Period PeriodLength { get; }

        /// <summary>
        /// Gets the number of expected summaries/periods.
        /// </summary>
        long NumberOfExpectedPeriods { get; }

        /// <summary>
        /// Gets the label formatter used for every period.
        /// </summary>
        Func<ZonedDateTime, string> LabelFormatter { get; }

        /// <summary>
        /// Gets the timezone used for every period.
        /// </summary>
        DateTimeZone TimeZone { get; }
    }
}