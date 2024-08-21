// <copyright file="TextToDateTimeProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

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
    /// Provider for obtaining a date time (represented as an Instant) from a text representation.
    /// </summary>
    /// <remarks>Schema key: textToDateTime.</remarks>
    public class TextToDateTimeProvider : IProvider<Data<Instant>>
    {
        private readonly IProvider<Data<string>> textProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextToDateTimeProvider"/> class.
        /// </summary>
        /// <param name="textProvider">A provider for the text representation of the date time.</param>
        public TextToDateTimeProvider(IProvider<Data<string>> textProvider) =>
            this.textProvider = textProvider;

        public string SchemaReferenceKey => "textToDateTime";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<Instant>>> Resolve(IProviderContext providerContext)
        {
            string stringToParse = (await this.textProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var parseResult = InstantPattern.ExtendedIso.Parse(stringToParse);
            if (parseResult.Success)
            {
                return ProviderResult<Data<Instant>>.Success(new Data<Instant>(parseResult.Value));
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, stringToParse.Truncate(80, "..."));
            errorData.Add(ErrorDataKey.ErrorMessage, parseResult.Exception.Message);

            var error = Errors.Automation.ValueParseFailure(stringToParse, "dateTime", this.SchemaReferenceKey, errorData);
            throw new ErrorException(error);
        }
    }
}
