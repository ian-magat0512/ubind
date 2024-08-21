// <copyright file="MonthlyPeriodicSummaryGeneratorConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard.Configuration
{
    using NodaTime;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class represents the configuration for the <see cref="MonthlyPeriodicSummaryGenerator{TRecord, TSummary}"/>.
    /// From this configuration, the <see cref="MonthlyPeriodicSummaryGenerator{TRecord, TSummary}"/> is expected to
    /// return monthly summaries from the collection of records. The expected number of summaries
    /// is the number of months between the provided startDateTime and endDateTime. The summary should have a label
    /// of the date in the format "MMM yyyy".
    /// </summary>
    public class MonthlyPeriodicSummaryGeneratorConfiguration : IPeriodicSummaryGeneratorConfiguration
    {
        private ZonedDateTime startDateTime;
        private ZonedDateTime endDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonthlyPeriodicSummaryGeneratorConfiguration"/> class.
        /// </summary>
        /// <param name="startDateTime">Start date of the summary.</param>
        /// <param name="endDateTime">End date of the summary.</param>
        public MonthlyPeriodicSummaryGeneratorConfiguration(ZonedDateTime startDateTime, ZonedDateTime endDateTime, DateTimeZone timeZone)
        {
            this.startDateTime = startDateTime;
            this.endDateTime = endDateTime;
            this.TimeZone = timeZone;
        }

        /// <inheritdoc/>
        public SamplePeriodLength PeriodType => SamplePeriodLength.Month;

        /// <inheritdoc/>
        public ZonedDateTime StartDateTime => this.startDateTime.ToStartOfMonth(this.TimeZone);

        /// <inheritdoc/>
        public ZonedDateTime EndDateTime => this.endDateTime
            .ToStartOfMonth(this.TimeZone, Period.FromMonths(1))
            .AtEndOfDayInZone(this.TimeZone);

        /// <inheritdoc/>
        public Period PeriodInterval => Period.FromMonths(1);

        /// <inheritdoc/>
        public Period PeriodLength => this.PeriodInterval - Period.FromTicks(1);

        /// <inheritdoc/>
        public long NumberOfExpectedPeriods => Period.Between(this.StartDateTime.LocalDateTime, this.EndDateTime.LocalDateTime, PeriodUnits.Months).Months;

        /// <inheritdoc/>
        public Func<ZonedDateTime, string> LabelFormatter => (date) => date.ToMMMMYYYY();

        /// <inheritdoc/>
        public DateTimeZone TimeZone { get; }
    }
}