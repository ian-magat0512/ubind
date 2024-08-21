// <copyright file="TextToTimeProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Time
{
    using System;
    using System.Globalization;
    using Humanizer;
    using MorseCode.ITask;
    using NodaTime;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Parses a time value from a text value defined by a text provider.
    /// Only values defined in time formats supported by the 'en-AU' locale can currently be parsed.
    /// </summary>
    /// <remarks>Schema key: textToTime.</remarks>
    public class TextToTimeProvider : IProvider<Data<LocalTime>>
    {
        private IProvider<Data<string>> textProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextToTimeProvider"/> class.
        /// </summary>
        /// <param name="textProvider">The text time to be parsed.</param>
        public TextToTimeProvider(IProvider<Data<string>> textProvider)
        {
            this.textProvider = textProvider;
        }

        public string SchemaReferenceKey => "textToTime";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<LocalTime>>> Resolve(IProviderContext providerContext)
        {
            string stringToParse = (await this.textProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var cultureInfo = CultureInfo.GetCultureInfo(Locales.en_AU);

            try
            {
                var parseResult = stringToParse.ToString().ToLocalTimeFromIso8601OrhmmttOrhhmmttOrhhmmssttWithCulture(cultureInfo);
                return ProviderResult<Data<LocalTime>>.Success(new Data<LocalTime>(parseResult));
            }
            catch (Exception ex)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add(ErrorDataKey.ValueToParse, stringToParse.ToString().Truncate(80, "..."));
                errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);
                throw new ErrorException(Errors.Automation.TimeParseFailure(stringToParse, errorData));
            }
        }
    }
}
