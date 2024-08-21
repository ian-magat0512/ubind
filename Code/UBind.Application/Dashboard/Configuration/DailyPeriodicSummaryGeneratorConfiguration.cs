// <copyright file="DailyPeriodicSummaryGeneratorConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard.Configuration
{
    using NodaTime;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class represents the configuration for the <see cref="DailyPeriodicSummaryGenerator{TRecord, TSummary}"/>.
    /// From this configuration, the <see cref="DailyPeriodicSummaryGenerator{TRecord, TSummary}"/> is expected to
    /// return daily summaries from the collection of records. The expected number of summaries
    /// is the number of days between the provided startDateTime and endDateTime. The summary should have a label
    /// of the date in the format "dd MMM yyyy".
    /// </summary>
    public class DailyPeriodicSummaryGeneratorConfiguration : IPeriodicSummaryGeneratorConfiguration
    {
        private ZonedDateTime startDateTime;
        private ZonedDateTime endDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="DailyPeriodicSummaryGeneratorConfiguration"/> class.
        /// </summary>
        /// <param name="startDateTime">Start date of the summary.</param>
        /// <param name="endDateTime">End date of the summary.</param>
        public DailyPeriodicSummaryGeneratorConfiguration(ZonedDateTime startDateTime, ZonedDateTime endDateTime, DateTimeZone timeZone)
        {
            this.startDateTime = startDateTime;
            this.endDateTime = endDateTime;
            this.TimeZone = timeZone;
        }

        /// <inheritdoc/>
        public SamplePeriodLength PeriodType => SamplePeriodLength.Day;

        /// <inheritdoc/>
        public ZonedDateTime StartDateTime => this.startDateTime.AtStartOfDayInZone(this.TimeZone);

        /// <inheritdoc/>
        public ZonedDateTime EndDateTime => this.endDateTime.AtEndOfDayInZone(this.TimeZone);

        /// <inheritdoc/>
        public Period PeriodInterval => Period.FromDays(1);

        /// <inheritdoc/>
        public Period PeriodLength => this.PeriodInterval - Period.FromTicks(1);

        /// <inheritdoc/>
        public long NumberOfExpectedPeriods => Period.Between(this.StartDateTime.LocalDateTime, this.EndDateTime.LocalDateTime, PeriodUnits.Days).Days + 1;

        /// <inheritdoc/>
        public Func<ZonedDateTime, string> LabelFormatter => (date) => date.ToDMMMYYYY();

        /// <inheritdoc/>
        public DateTimeZone TimeZone { get; }
    }
}