// <copyright file="DateAndTimeDateTimeProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.DateTime
{
    using Humanizer;
    using MorseCode.ITask;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Provider for obtaining a date and time (represented as an Instant) then as datetime provider.
    /// </summary>
    /// <remarks>Schema key: dateAndTimeDateTime.</remarks>
    public class DateAndTimeDateTimeProvider : IProvider<Data<Instant>>
    {
        private readonly IProvider<Data<LocalDate>> dateProvider;
        private readonly IProvider<Data<LocalTime>> timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateAndTimeDateTimeProvider"/> class.
        /// </summary>
        /// <param name="dateProvider">A provider for the date representation of the date.</param>
        /// <param name="timeProvider">A provider for the time representation of the time.</param>
        public DateAndTimeDateTimeProvider(IProvider<Data<LocalDate>> dateProvider, IProvider<Data<LocalTime>> timeProvider)
        {
            this.dateProvider = dateProvider;
            this.timeProvider = timeProvider;
        }

        public string SchemaReferenceKey => "dateAndTimeDateTime";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<Instant>>> Resolve(IProviderContext providerContext)
        {
            var dateParse = (await this.dateProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var timeParse = (await this.timeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var dateTime = $"{LocalDatePattern.Iso.Format(dateParse)}T{LocalTimePattern.ExtendedIso.Format(timeParse)}Z";
            var parseResult = InstantPattern.ExtendedIso.Parse(dateTime);
            if (parseResult.Success)
            {
                return ProviderResult<Data<Instant>>.Success(new Data<Instant>(parseResult.Value));
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, dateTime.Truncate(80, "..."));
            errorData.Add(ErrorDataKey.ErrorMessage, parseResult.Exception.Message);
            var error = Errors.Automation.ValueResolutionError(nameof(DateAndTimeDateTimeProvider), errorData);
            throw new ErrorException(error);
        }
    }
}
