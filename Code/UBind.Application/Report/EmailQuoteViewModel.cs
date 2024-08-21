// <copyright file="EmailQuoteViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using UBind.Domain.ReadModel.Email;

    /// <summary>
    /// Email's quote view model for liquid template.
    /// </summary>
    public class EmailQuoteViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailQuoteViewModel"/> class.
        /// </summary>
        /// <param name="quote">The quote data.</param>
        public EmailQuoteViewModel(QuoteData quote)
        {
            this.Type = quote.Type.ToString();
            this.QuoteReference = quote.QuoteNumber;
        }

        /// <summary>
        /// Gets the type of the quote.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the quote reference number.
        /// </summary>
        public string QuoteReference { get; }
    }
}
