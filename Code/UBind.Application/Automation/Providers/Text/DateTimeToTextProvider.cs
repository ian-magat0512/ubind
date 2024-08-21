// <copyright file="DateTimeToTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Text
{
    using MorseCode.ITask;
    using NodaTime;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Parses an text value from a datetime value defined by a datetime to text provider.
    /// </summary>
    /// <remarks>Schema key: dateTimeToText.</remarks>
    public class DateTimeToTextProvider : IProvider<Data<string>>
    {
        private IProvider<Data<Instant>> dateTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeToTextProvider"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">The date time to be parsed.</param>
        public DateTimeToTextProvider(IProvider<Data<Instant>> dateTimeProvider)
        {
            this.dateTimeProvider = dateTimeProvider;
        }

        public string SchemaReferenceKey => "dateTimeToText";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<string>>> Resolve(IProviderContext providerContext)
        {
            var dateTime = (await this.dateTimeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            if (dateTime?.DataValue != null)
            {
                return ProviderResult<Data<string>>.Success(new Data<string>(dateTime.DataValue.ToExtendedIso8601String()));
            }

            return ProviderResult<Data<string>>.Success(null);
        }
    }
}
