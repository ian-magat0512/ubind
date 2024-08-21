// <copyright file="AllPeriodicSummaryGeneratorConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Dashboard.Configuration
{
    using NodaTime;
    using UBind.Domain.Enums;

    /// <summary>
    /// This class represents the configuration for the <see cref="AllPeriodicSummaryGenerator{TRecord, TSummary}"/>.
    /// From this configuration, the <see cref="AllPeriodicSummaryGenerator{TRecord, TSummary}"/> is expected to
    /// return a single summary from the collection of records. The FromDate and ToDate of the summary should
    /// be in ISO 8601 format and the same as the provided startDateTime and endDateTime.
    /// And the summary should not have a label.
    /// </summary>
    public class AllPeriodicSummaryGeneratorConfiguration : IPeriodicSummaryGeneratorConfiguration
    {
        private ZonedDateTime startDateTime;
        private ZonedDateTime endDateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllPeriodicSummaryGeneratorConfiguration"/> class.
        /// </summary>
        /// <param name="startDateTime">Start date of the summary.</param>
        /// <param name="endDateTime">End date of the summary.</param>
        public AllPeriodicSummaryGeneratorConfiguration(ZonedDateTime startDateTime, ZonedDateTime endDateTime, DateTimeZone timeZone)
        {
            this.startDateTime = startDateTime;
            this.endDateTime = endDateTime;
            this.TimeZone = timeZone;
        }

        /// <inheritdoc/>
        public SamplePeriodLength PeriodType => SamplePeriodLength.All;

        /// <inheritdoc/>
        public ZonedDateTime StartDateTime => this.startDateTime;

        /// <inheritdoc/>
        public ZonedDateTime EndDateTime => this.endDateTime;

        /// <inheritdoc/>
        public Period PeriodInterval => Period.Between(this.StartDateTime.LocalDateTime, this.EndDateTime.LocalDateTime, PeriodUnits.Ticks) + Period.FromTicks(1);

        /// <inheritdoc/>
        public Period PeriodLength => this.PeriodInterval - Period.FromTicks(1);

        /// <inheritdoc/>
        public long NumberOfExpectedPeriods => 1;

        /// <inheritdoc/>
        public Func<ZonedDateTime, string> LabelFormatter => (date) => null;

        /// <inheritdoc/>
        public DateTimeZone TimeZone { get; }
    }
}