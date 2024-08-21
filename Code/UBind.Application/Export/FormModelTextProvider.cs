// <copyright file="FormModelTextProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// For providing the latest form data json from an application.
    /// </summary>
    public class FormModelTextProvider : ITextProvider
    {
        private const string EmptyObject = "{}";

        /// <inheritdoc/>
        public Task<string> Invoke(ApplicationEvent applicationEvent)
        {
            var quote = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            return Task.FromResult(
                quote.LatestFormData?.Data.FormModel.ToString() ?? EmptyObject);
        }
    }
}
