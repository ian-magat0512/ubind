// <copyright file="TimeToTextProvider.cs" company="uBind">
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
    /// Parses an time value defined by a time to text provider.
    /// </summary>
    /// <remarks>Schema key: timeToText.</remarks>
    public class TimeToTextProvider : IProvider<Data<string>>
    {
        private IProvider<Data<LocalTime>> timeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeToTextProvider"/> class.
        /// </summary>
        /// <param name="timeProvider">The time to be parsed.</param>
        public TimeToTextProvider(IProvider<Data<LocalTime>> timeProvider)
        {
            this.timeProvider = timeProvider;
        }

        public string SchemaReferenceKey => "timeToText";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            LocalTime time = (await this.timeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            var cultureInfo = CultureInfo.GetCultureInfo(Locales.en_AU);
            return ProviderResult<Data<string>>.Success(new Data<string>(time.To12HrFormat(cultureInfo)));
        }
    }
}
