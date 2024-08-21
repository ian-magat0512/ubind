// <copyright file="DateToTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using System.Globalization;
    using MorseCode.ITask;
    using NodaTime;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Parses a text value from a date value defined by a date to text provider.
    /// </summary>
    /// <remarks>Schema key: dateToText.</remarks>
    public class DateToTextProvider : IProvider<Data<string>>
    {
        private IProvider<Data<LocalDate>> dateProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateToTextProvider"/> class.
        /// </summary>
        /// <param name="dateProvider">The date to be parsed.</param>
        public DateToTextProvider(IProvider<Data<LocalDate>> dateProvider)
        {
            this.dateProvider = dateProvider;
        }

        public string SchemaReferenceKey => "dateToText";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var date = (await this.dateProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();

            if (date?.DataValue != null)
            {
                var cultureInfo = CultureInfo.GetCultureInfo(Locales.en_AU);
                return ProviderResult<Data<string>>.Success(new Data<string>(date.DataValue.ToDMMMYYYY(cultureInfo)));
            }

            return ProviderResult<Data<string>>.Success(null);
        }
    }
}
