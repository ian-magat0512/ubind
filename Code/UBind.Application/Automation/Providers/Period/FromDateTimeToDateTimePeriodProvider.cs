// <copyright file="FromDateTimeToDateTimePeriodProvider.cs" company="uBind">
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

    /// <summary>
    /// Provides a period defined by start and end date/times.
    /// </summary>
    public class FromDateTimeToDateTimePeriodProvider : IProvider<Data<Interval>>
    {
        private readonly IProvider<Data<Instant>> fromDateTimeProvider;
        private readonly IProvider<Data<Instant>> toDateTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="FromDateTimeToDateTimePeriodProvider"/> class.
        /// </summary>
        /// <param name="fromDateTimeProvider">The provider for the from date/time.</param>
        /// <param name="toDateTimeProvider">The provider for the to date/time.</param>
        public FromDateTimeToDateTimePeriodProvider(
            IProvider<Data<Instant>> fromDateTimeProvider,
            IProvider<Data<Instant>> toDateTimeProvider)
        {
            this.fromDateTimeProvider = fromDateTimeProvider;
            this.toDateTimeProvider = toDateTimeProvider;
        }

        public string SchemaReferenceKey => "fromDateTimeToDateTimePeriod";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<Interval>>> Resolve(IProviderContext providerContext)
        {
            Instant from = (await this.fromDateTimeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var localFromDateTime = from.InUtc().LocalDateTime;
            from = localFromDateTime.InZoneLeniently(providerContext.AutomationData.System.TimeZone).ToInstant();
            Instant to = (await this.toDateTimeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var localToDateTime = to.InUtc().LocalDateTime;
            to = localToDateTime.InZoneLeniently(providerContext.AutomationData.System.TimeZone).ToInstant();

            if (to < from)
            {
                var reasonMessage = $"The {this.SchemaReferenceKey} provider requires that the value resolved for the \"toDateTime\" " +
                    $"parameter is greater than the value resolved for the \"fromDateTime\" parameter (\"{from}\"). ";
                var error = Errors.Automation.ParameterValueTypeInvalid(
                    this.SchemaReferenceKey,
                    "toDateTime",
                    to.ToString(),
                    reasonWhyValueIsInvalidIfAvailable: reasonMessage);
                throw new ErrorException(error);
            }

            return ProviderResult<Data<Interval>>.Success(new Interval(from, to));
        }
    }
}
