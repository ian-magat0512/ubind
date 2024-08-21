// <copyright file="TextToIntegerProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Integer
{
    using Humanizer;
    using MorseCode.ITask;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Parses an integer value from a text value defined by a integer provider.
    /// </summary>
    /// <remarks>Schema key: textToInteger.</remarks>
    public class TextToIntegerProvider : IProvider<Data<long>>
    {
        private readonly IProvider<Data<string>> textProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextToIntegerProvider"/> class.
        /// </summary>
        /// <param name="textProvider">The text to be parsed.</param>
        public TextToIntegerProvider(IProvider<Data<string>> textProvider)
        {
            this.textProvider = textProvider;
        }

        public string SchemaReferenceKey => "textToInteger";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<long>>> Resolve(IProviderContext providerContext)
        {
            string stringToParse = (await this.textProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var success = int.TryParse(stringToParse, out int result);
            if (success)
            {
                return ProviderResult<Data<long>>.Success(new Data<long>(result));
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, stringToParse.Truncate(80, "..."));
            throw new ErrorException(Errors.Automation.ValueResolutionError(this.SchemaReferenceKey, errorData));
        }
    }
}
