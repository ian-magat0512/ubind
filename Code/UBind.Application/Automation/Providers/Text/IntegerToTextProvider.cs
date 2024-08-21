// <copyright file="IntegerToTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using MorseCode.ITask;
    using UBind.Application.Automation.Extensions;

    /// <summary>
    /// Parses an text value from a integer value defined by a integer to text provider.
    /// </summary>
    /// <remarks>Schema key: integerToText.</remarks>
    public class IntegerToTextProvider : IProvider<Data<string>>
    {
        private IProvider<Data<long>> integerProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerToTextProvider"/> class.
        /// </summary>
        /// <param name="integerProvider">The integer to be parsed.</param>
        public IntegerToTextProvider(IProvider<Data<long>> integerProvider)
        {
            this.integerProvider = integerProvider;
        }

        public string SchemaReferenceKey => "integerToText";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var resolveInteger = (await this.integerProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var integer = resolveInteger.DataValue;

            if (integer != null)
            {
                return ProviderResult<Data<string>>.Success(new Data<string>(integer.ToString()));
            }

            return ProviderResult<Data<string>>.Success(null);
        }
    }
}
