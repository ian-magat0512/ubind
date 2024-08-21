// <copyright file="StaticDurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Duration
{
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Parses the hard-coded duration value in ISO 8601 format, e.g. 'P3Y6M4DT12H30M5S', 'P1D', 'PT1H30M' etc.
    /// </summary>
    public class StaticDurationProvider : IProvider<Data<Period>>
    {
        private IProvider<Data<string>> textValueProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticDurationProvider"/> class.
        /// </summary>
        /// <param name="textValueProvider">The text provider.</param>
        public StaticDurationProvider(IProvider<Data<string>> textValueProvider)
        {
            this.textValueProvider = textValueProvider;
        }

        public string SchemaReferenceKey => "#duration";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<Period>>> Resolve(IProviderContext providerContext)
        {
            var isoDurationString = (await this.textValueProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var period = await this.ConvertToPeriod(isoDurationString, providerContext);
            return ProviderResult<Data<Period>>.Success(period);
        }

        private async Task<Period> ConvertToPeriod(string isoDurationString, IProviderContext providerContext)
        {
            var periodParseResult = PeriodPattern.NormalizingIso.Parse(isoDurationString);

            if (periodParseResult.Success)
            {
                return periodParseResult.Value;
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ErrorMessage, periodParseResult.Exception.Message);
            throw new ErrorException(
                Errors.Automation.InvalidValueTypeObtained(
                    "ISO Duration Format", this.SchemaReferenceKey, errorData));
        }
    }
}
