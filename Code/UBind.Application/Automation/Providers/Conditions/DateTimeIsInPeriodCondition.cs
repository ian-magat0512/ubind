// <copyright file="DateTimeIsInPeriodCondition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using MorseCode.ITask;
    using NodaTime;
    using UBind.Application.Automation.Extensions;

    /// <summary>
    /// Returns a boolean true if a give date/time is in a given period..
    /// </summary>
    public class DateTimeIsInPeriodCondition : IProvider<Data<bool>>
    {
        private readonly IProvider<Data<Instant>> dateTimeProvider;
        private readonly IProvider<Data<Interval>> periodProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeIsInPeriodCondition"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">A provider for the date time.</param>
        /// <param name="periodProvider">A provider for the period.</param>
        public DateTimeIsInPeriodCondition(
            IProvider<Data<Instant>> dateTimeProvider,
            IProvider<Data<Interval>> periodProvider)
        {
            this.dateTimeProvider = dateTimeProvider;
            this.periodProvider = periodProvider;
        }

        public string SchemaReferenceKey => "dateTimeIsInPeriodCondition";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<bool>>> Resolve(IProviderContext providerContext)
        {
            Instant timestamp = (await this.dateTimeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            Interval period = (await this.periodProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            return ProviderResult<Data<bool>>.Success(period.Contains(timestamp));
        }
    }
}
