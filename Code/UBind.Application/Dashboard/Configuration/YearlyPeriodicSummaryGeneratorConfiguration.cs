// <copyright file="YearlyPeriodicSummaryGeneratorConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard.Configuration
{
    using NodaTime;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class represents the configuration for the <see cref="YearlyPeriodicSummaryGenerator{TRecord, TSummary}"/>.
    /// From this configuration, the <see cref="YearlyPeriodicSummaryGenerator{TRecord, TSummary}"/> is expected to
    /// return yearly summaries from the collection of records. The expected number of summaries
    /// is the number of quarters between the provided startDateTime and endDateTime. The summary should have a label
    /// of the date in the format "yyyy".
    /// </summary>
    public class YearlyPeriodicSummaryGeneratorConfiguration : IPeriodicSummaryGeneratorConfiguration
    {
        private ZonedDateTime startDateTime;
        private ZonedDateTime endDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="YearlyPeriodicSummaryGeneratorConfiguration"/> class.
        /// </summary>
        /// <param name="startDateTime">Start date of the summary.</param>
        /// <param name="endDateTime">End date of the summary.</param>
        public YearlyPeriodicSummaryGeneratorConfiguration(ZonedDateTime startDateTime, ZonedDateTime endDateTime, DateTimeZone timeZone)
        {
            this.startDateTime = startDateTime;
            this.endDateTime = endDateTime;
            this.TimeZone = timeZone;
        }

        /// <inheritdoc/>
        public SamplePeriodLength PeriodType => SamplePeriodLength.Year;

        /// <inheritdoc/>
        public ZonedDateTime StartDateTime => this.startDateTime.ToStartOfYear(this.TimeZone);

        /// <inheritdoc/>
        public ZonedDateTime EndDateTime => this.endDateTime
            .ToStartOfYear(this.TimeZone, Period.FromYears(1))
            .AtEndOfDayInZone(this.TimeZone);

        /// <inheritdoc/>
        public Period PeriodInterval => Period.FromYears(1);

        /// <inheritdoc/>
        public Period PeriodLength => this.PeriodInterval - Period.FromTicks(1);

        /// <inheritdoc/>
        public long NumberOfExpectedPeriods => Period.Between(this.StartDateTime.LocalDateTime, this.EndDateTime.LocalDateTime, PeriodUnits.Years).Years;

        /// <inheritdoc/>
        public Func<ZonedDateTime, string> LabelFormatter => (date) => date.Year.ToString();

        /// <inheritdoc/>
        public DateTimeZone TimeZone { get; }
    }
}