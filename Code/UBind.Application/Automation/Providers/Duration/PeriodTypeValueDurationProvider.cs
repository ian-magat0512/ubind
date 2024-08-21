// <copyright file="PeriodTypeValueDurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Duration
{
    using MorseCode.ITask;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;

    /// <summary>
    /// Represents an instance of period type value duration.
    /// </summary>
    public class PeriodTypeValueDurationProvider : IProvider<Data<Period>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodTypeValueDurationProvider"/> class.
        /// </summary>
        /// <param name="value">The period value.</param>
        /// <param name="periodType">The period type.</param>
        public PeriodTypeValueDurationProvider(
            IProvider<Data<long>> value,
            PeriodType periodType)
        {
            this.Value = value;
            this.PeriodType = periodType;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public IProvider<Data<long>> Value { get; }

        /// <summary>
        /// Gets the period type.
        /// </summary>
        public PeriodType PeriodType { get; }

        public string SchemaReferenceKey => "periodTypeValueDuration";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<Period>>> Resolve(IProviderContext providerContext)
        {
            int value = (int)(await this.Value.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            switch (this.PeriodType)
            {
                case PeriodType.Year:
                    return ProviderResult<Data<Period>>.Success(Period.FromYears(value));
                case PeriodType.Quarter:
                    return ProviderResult<Data<Period>>.Success(Period.FromMonths(value * 3));
                case PeriodType.Month:
                    return ProviderResult<Data<Period>>.Success(Period.FromMonths(value));
                case PeriodType.Week:
                    return ProviderResult<Data<Period>>.Success(Period.FromWeeks(value));
                case PeriodType.Day:
                    return ProviderResult<Data<Period>>.Success(Period.FromDays(value));
                case PeriodType.Hour:
                    return ProviderResult<Data<Period>>.Success(Period.FromHours(value));
                case PeriodType.Minute:
                    return ProviderResult<Data<Period>>.Success(Period.FromMinutes(value));
                case PeriodType.Second:
                    return ProviderResult<Data<Period>>.Success(Period.FromSeconds(value));
                case PeriodType.Millisecond:
                    return ProviderResult<Data<Period>>.Success(Period.FromMilliseconds(value));
                default:
                    return ProviderResult<Data<Period>>.Success(Period.Zero);
            }
        }
    }
}
