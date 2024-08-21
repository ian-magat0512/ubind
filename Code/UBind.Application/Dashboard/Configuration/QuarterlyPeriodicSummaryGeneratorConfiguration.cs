// <copyright file="QuarterlyPeriodicSummaryGeneratorConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard.Configuration
{
    using NodaTime;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class represents the configuration for the <see cref="QuarterlyPeriodicSummaryGenerator{TRecord, TSummary}"/>.
    /// From this configuration, the <see cref="QuarterlyPeriodicSummaryGenerator{TRecord, TSummary}"/> is expected to
    /// return quarterly summaries from the collection of records. The expected number of summaries
    /// is the number of quarters between the provided startDateTime and endDateTime. The summary should have a label
    /// of the date in the format "QQ yyyy".
    /// </summary>
    public class QuarterlyPeriodicSummaryGeneratorConfiguration : IPeriodicSummaryGeneratorConfiguration
    {
        private ZonedDateTime startDateTime;
        private ZonedDateTime endDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuarterlyPeriodicSummaryGeneratorConfiguration"/> class.
        /// </summary>
        /// <param name="startDateTime">Start date of the summary.</param>
        /// <param name="endDateTime">End date of the summary.</param>
        public QuarterlyPeriodicSummaryGeneratorConfiguration(ZonedDateTime startDateTime, ZonedDateTime endDateTime, DateTimeZone timeZone)
        {
            this.startDateTime = startDateTime;
            this.endDateTime = endDateTime;
            this.TimeZone = timeZone;
        }

        /// <inheritdoc/>
        public SamplePeriodLength PeriodType => SamplePeriodLength.Quarter;

        /// <inheritdoc/>
        public ZonedDateTime StartDateTime => this.startDateTime.ToStartOfQuarter(this.TimeZone);

        /// <inheritdoc/>
        public ZonedDateTime EndDateTime => this.endDateTime
            .ToStartOfQuarter(this.TimeZone, Period.FromMonths(3))
            .AtEndOfDayInZone(this.TimeZone);

        /// <inheritdoc/>
        public Period PeriodInterval => Period.FromMonths(3);

        /// <inheritdoc/>
        public Period PeriodLength => this.PeriodInterval - Period.FromTicks(1);

        /// <inheritdoc/>
        public long NumberOfExpectedPeriods => (int)Math.Ceiling(
                Period.Between(this.StartDateTime.LocalDateTime, this.EndDateTime.LocalDateTime, PeriodUnits.Months).Months / 3.0);

        /// <inheritdoc/>
        public Func<ZonedDateTime, string> LabelFormatter => (date) => date.ToQQYYYY();

        /// <inheritdoc/>
        public DateTimeZone TimeZone { get; }
    }
}