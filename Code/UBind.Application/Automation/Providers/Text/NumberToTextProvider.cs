// <copyright file="NumberToTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    /// <summary>
    /// Parses an text value from a number value defined by a number to text provider.
    /// </summary>
    /// <remarks>Schema key: numberToText.</remarks>
    public class NumberToTextProvider : IProvider<Data<string>>
    {
        private IProvider<Data<decimal>> numberProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberToTextProvider"/> class.
        /// </summary>
        /// <param name="numberProvider">The number to be parsed.</param>
        public NumberToTextProvider(IProvider<Data<decimal>> numberProvider)
        {
            this.numberProvider = numberProvider;
        }

        public string SchemaReferenceKey => "numberToText";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var resolveNumber = (await this.numberProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var number = resolveNumber.DataValue;
            if (number != null)
            {
                return ProviderResult<Data<string>>.Success(new Data<string>(number.ToString()));
            }

            return ProviderResult<Data<string>>.Success(null);
        }
    }
}
