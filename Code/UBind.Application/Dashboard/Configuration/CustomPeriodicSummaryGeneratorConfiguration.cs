// <copyright file="CustomPeriodicSummaryGeneratorConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Dashboard.Configuration
{
    using NodaTime;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class represents the configuration for the <see cref="CustomPeriodicSummaryGenerator{TRecord, TSummary}"/>.
    /// From this configuration, the <see cref="CustomPeriodicSummaryGenerator{TRecord, TSummary}"/> is expected to
    /// return custom length of summaries from the collection of records. The expected number of summaries
    /// is the number of period (in minutes) between the provided startDateTime and endDateTime.
    /// </summary>
    public class CustomPeriodicSummaryGeneratorConfiguration : IPeriodicSummaryGeneratorConfiguration
    {
        private ZonedDateTime startDateTime;
        private ZonedDateTime endDateTime;
        private int periodIntervalInMinutes;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomPeriodicSummaryGeneratorConfiguration"/> class.
        /// </summary>
        public CustomPeriodicSummaryGeneratorConfiguration(ZonedDateTime startDateTime, ZonedDateTime endDateTime, DateTimeZone timeZone, int periodIntervalInMinutes)
        {
            this.TimeZone = timeZone;
            this.startDateTime = startDateTime;
            this.endDateTime = endDateTime;
            this.periodIntervalInMinutes = periodIntervalInMinutes;
        }

        /// <inheritdoc/>
        public SamplePeriodLength PeriodType => SamplePeriodLength.Custom;

        /// <inheritdoc/>
        public ZonedDateTime StartDateTime => this.GetStartDateOfSummary();

        /// <inheritdoc/>
        public ZonedDateTime EndDateTime => this.endDateTime;

        /// <inheritdoc/>
        public Period PeriodInterval => Period.FromMinutes(this.periodIntervalInMinutes);

        /// <inheritdoc/>
        public Period PeriodLength => this.PeriodInterval - Period.FromTicks(1);

        /// <inheritdoc/>
        public long NumberOfExpectedPeriods => this.GetNumberOfPeriods();

        /// <inheritdoc/>
        public Func<ZonedDateTime, string> LabelFormatter => (date) =>
        {
            var start = date;
            var end = (date.LocalDateTime + this.PeriodLength).InZoneLeniently(this.TimeZone);
            return $"From {start.ToDMMMYYYY()} to {end.ToDMMMYYYY()}";
        };

        /// <inheritdoc/>
        public DateTimeZone TimeZone { get; }

        private long StartDateTimeInTicks => this.StartDateTime.ToInstant().ToUnixTimeTicks();

        private long PeriodLengthInTicks => Convert.ToInt64(this.PeriodLength.ToDuration().TotalTicks);

        public ZonedDateTime GetPeriodStartDateOfTimestamp(Instant timestamp)
        {
            var dateTimeTicks = timestamp.ToUnixTimeTicks();
            var difference = dateTimeTicks - this.StartDateTimeInTicks;
            var numberOfPeriodFromStartTime = difference / this.PeriodLengthInTicks;
            var excess = numberOfPeriodFromStartTime == 0 ? 0 : difference % this.PeriodLengthInTicks;
            var date = this.StartDateTime;
            if (numberOfPeriodFromStartTime <= 1 && excess <= 0)
            {
                return date;
            }
            else
            {
                for (int i = 0; i < numberOfPeriodFromStartTime; i++)
                {
                    date = date.AddPeriod(this.TimeZone, this.PeriodInterval);
                }

                return date;
            }
        }

        private long GetNumberOfPeriods()
        {
            var totalMinutesWithPartialPeriod = this.GetTotalMinutesWithPartialPeriod();
            var totalMinutesExpected = Math.Ceiling(this.PeriodInterval.ToDuration().TotalMinutes);
            double numberOfPeriods = totalMinutesWithPartialPeriod / totalMinutesExpected;
            return Convert.ToInt64(Math.Ceiling(numberOfPeriods));
        }

        private ZonedDateTime GetStartDateOfSummary()
        {
            var totalMinutesWithPartialPeriod = this.GetTotalMinutesWithPartialPeriod();
            var totalMinutesExpected = Math.Ceiling(this.PeriodInterval.ToDuration().TotalMinutes);
            var partialPeriodInMinutes = Convert.ToInt64(Math.Ceiling(totalMinutesWithPartialPeriod % totalMinutesExpected));
            var totalMinutesToDeduct = Convert.ToInt64((totalMinutesWithPartialPeriod - partialPeriodInMinutes) * -1);
            var expectedStartDate = this.endDateTime.AddPeriod(this.TimeZone, Period.FromMinutes(totalMinutesToDeduct) + Period.FromTicks(1));
            if (this.GetDifferenceInNanoseconds(expectedStartDate, this.startDateTime) > 0)
            {
                totalMinutesToDeduct += Convert.ToInt64(totalMinutesExpected);
                expectedStartDate = this.endDateTime.AddPeriod(this.TimeZone, Period.FromMinutes(totalMinutesToDeduct) + Period.FromTicks(1));
            }

            return expectedStartDate;
        }

        private double GetTotalMinutesWithPartialPeriod()
        {
            var from = this.startDateTime.LocalDateTime.InUtc().ToInstant();
            var to = this.endDateTime.LocalDateTime.InUtc().ToInstant();
            return Math.Round((to - from).ToTimeSpan().TotalMinutes, 0);
        }

        private double GetDifferenceInNanoseconds(ZonedDateTime expected, ZonedDateTime start)
        {
            var from = expected.ToInstant();
            var to = start.ToInstant();
            return Math.Round((to - from).ToTimeSpan().TotalMilliseconds * 1000000, 0);
        }
    }
}