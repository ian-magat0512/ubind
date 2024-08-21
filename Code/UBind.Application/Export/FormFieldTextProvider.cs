// <copyright file="FormFieldTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    /// <summary>
    /// Text provider that retrieves text from form field.
    /// </summary>
    public class FormFieldTextProvider : ITextProvider
    {
        private readonly ITextProvider fieldName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormFieldTextProvider"/> class.
        /// </summary>
        /// <param name="fieldName">The name of the form field to read the address from.</param>
        public FormFieldTextProvider(ITextProvider fieldName)
        {
            Contract.Assert(fieldName != null);

            this.fieldName = fieldName;
        }

        /// <inheritdoc />
        public async Task<string> Invoke(UBind.Domain.ApplicationEvent applicationEvent)
        {
            var fieldName = await this.fieldName.Invoke(applicationEvent);
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            return quote.LatestFormData?.Data?.GetValue(fieldName);
        }
    }
}
