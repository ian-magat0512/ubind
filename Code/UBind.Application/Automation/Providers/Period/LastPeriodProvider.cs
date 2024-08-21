// <copyright file="LastPeriodProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Period
{
    using MorseCode.ITask;
    using NodaTime;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Provides a period ended by the current time and starting a given duration earlier.
    /// </summary>
    public class LastPeriodProvider : IProvider<Data<Interval>>
    {
        private readonly IProvider<Data<Period>> durationProvider;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="LastPeriodProvider"/> class.
        /// </summary>
        /// <param name="durationProvider">The provider for the from date/time.</param>
        /// <param name="clock">A clock for obtaining the current time (for ending the period).</param>
        public LastPeriodProvider(
            IProvider<Data<Period>> durationProvider,
            IClock clock)
        {
            this.durationProvider = durationProvider;
            this.clock = clock;
        }

        public string SchemaReferenceKey => "lastPeriod";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<Interval>>> Resolve(IProviderContext providerContext)
        {
            Period period = (await this.durationProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var end = this.clock.Now();
            var start = this.CanPerformInstantArithmeticWithPeriod(period)
                ? end.Minus(period.ToDuration())
                : end.InZone(Timezones.AET).LocalDateTime.Minus(period).InZoneStrictly(Timezones.AET).ToInstant();

            if (end < start)
            {
                var error = Errors.Automation.ParameterValueTypeInvalid(
                    this.SchemaReferenceKey,
                    "duration",
                    start.ToString(),
                    reasonWhyValueIsInvalidIfAvailable: $"The {this.SchemaReferenceKey} requires that the value for \"duration\" to be positive. ");
                throw new ErrorException(error);
            }

            return ProviderResult<Data<Interval>>.Success(new Interval(start, end));
        }

        // If the period is expressed in years or months then we cannot use it to perform
        // instant-based arithmetic, and need to convert to a local date time and back instead.
        private bool CanPerformInstantArithmeticWithPeriod(Period period)
            => period.Years == 0
            && period.Months == 0;
    }
}
