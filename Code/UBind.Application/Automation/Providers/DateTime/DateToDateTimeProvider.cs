// <copyright file="DateToDateTimeProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.DateTime
{
    using System.Globalization;
    using Humanizer;
    using MorseCode.ITask;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// A provider for that resolves to a datetime value given that input can be a date-type or datetime-type.
    /// </summary>
    public class DateToDateTimeProvider : IProvider<Data<Instant>>
    {
        private readonly IProvider<Data<string>> inputStringProvider;
        private readonly IProvider<IData> dateOrDateTimeProvider;

        public DateToDateTimeProvider(
            IProvider<Data<string>> dateTimeProvider,
            IProvider<IData> dateProvider)
        {
            this.inputStringProvider = dateTimeProvider;
            this.dateOrDateTimeProvider = dateProvider;
        }

        public string SchemaReferenceKey => "date";

        public async ITask<IProviderResult<Data<Instant>>> Resolve(IProviderContext providerContext)
        {
            if (this.inputStringProvider == null && this.dateOrDateTimeProvider == null)
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing(
                    "#dateOrDateTime",
                    this.SchemaReferenceKey));
            }

            if (this.inputStringProvider != null)
            {
                // try parse input string as date and as datetime.
                string stringValue = (await this.inputStringProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                var instantParseResult = InstantPattern.ExtendedIso.Parse(stringValue);
                if (instantParseResult.Success)
                {
                    return ProviderResult<Data<Instant>>.Success(new Data<Instant>(instantParseResult.Value));
                }

                try
                {
                    var date = stringValue.ToLocalDateFromIso8601OrddMMyyyyOrddMMyyOrdMMMyyyyWithCulture(CultureInfo.GetCultureInfo(Locales.en_AU));
                    return ProviderResult<Data<Instant>>.Success(
                        new Data<Instant>(InstantPattern.ExtendedIso.Parse($"{date.ToIso8601()}T00:00:00Z").Value));
                }
                catch (Exception ex)
                {
                    var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                    errorData.Add(ErrorDataKey.ValueToParse, stringValue.Truncate(80, "..."));
                    errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);
                    throw new ErrorException(Errors.Automation.ValueParseFailure(stringValue, "date", this.SchemaReferenceKey, errorData));
                }
            }

            var dateOrDateTimeValue = (await this.dateOrDateTimeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var valueData = dateOrDateTimeValue.GetValueFromGeneric();
            if (valueData is Instant)
            {
                return ProviderResult<Data<Instant>>.Success(new Data<Instant>(valueData));
            }

            var dateToInstant = InstantPattern.ExtendedIso.Parse($"{valueData}T00:00:00Z").Value;
            return ProviderResult<Data<Instant>>.Success(new Data<Instant>(dateToInstant));
        }
    }
}
