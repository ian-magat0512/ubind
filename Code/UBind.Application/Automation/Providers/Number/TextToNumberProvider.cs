// <copyright file="TextToNumberProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Number
{
    using Humanizer;
    using MorseCode.ITask;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Parses a number value from a text value defined by a number provider.
    /// </summary>
    /// <remarks>Schema key: textToNumber.</remarks>
    public class TextToNumberProvider : IProvider<Data<decimal>>
    {
        private readonly IProvider<Data<string>> textProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextToNumberProvider"/> class.
        /// </summary>
        /// <param name="textProvider">The text to be parsed.</param>
        public TextToNumberProvider(IProvider<Data<string>> textProvider) =>
            this.textProvider = textProvider;

        public string SchemaReferenceKey => "textToNumber";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<decimal>>> Resolve(IProviderContext providerContext)
        {
            string stringToParse = (await this.textProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var success = decimal.TryParse(stringToParse, out decimal value);
            if (success)
            {
                return ProviderResult<Data<decimal>>.Success(new Data<decimal>(value));
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, stringToParse.Truncate(80, "..."));
            var error = Errors.Automation.ValueResolutionError(this.SchemaReferenceKey, errorData);
            throw new ErrorException(error);
        }
    }
}
