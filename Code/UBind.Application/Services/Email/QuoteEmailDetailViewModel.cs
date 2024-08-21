// <copyright file="QuoteEmailDetailViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Email
{
    using System;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel.Email;

    /// <summary>
    /// View model for quote email detail.
    /// </summary>
    public class QuoteEmailDetailViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteEmailDetailViewModel"/> class.
        /// </summary>
        /// <param name="quoteEmailReadModelId">The quote email read model id.</param>
        /// <param name="emailModel">The email model.</param>
        /// <param name="quote">The quote record of the email.</param>
        public QuoteEmailDetailViewModel(Guid quoteEmailReadModelId, QuoteEmailReadModel emailModel, IQuoteReadModelSummary quote)
        {
            this.QuoteEmailReadModelId = quoteEmailReadModelId;
            this.QuoteEmailReadModel = emailModel;
            this.QuoteNumber = quote.QuoteNumber;
            this.PolicyNumber = quote.PolicyNumber;
            this.CustomerId = quote.CustomerId;
            this.CustomerName = quote.CustomerFullName;
        }

        /// <summary>
        /// Gets or sets the ID of the quote.
        /// </summary>
        public Guid QuoteEmailReadModelId { get; set; }

        /// <summary>
        /// Gets or sets quote number.
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets the policy number.
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the customer id.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the customers name.
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the quote email model.
        /// </summary>
        public Email Email { get; set; }

        /// <summary>
        /// Gets or sets the ID of the quote email model.
        /// </summary>
        public QuoteEmailReadModel QuoteEmailReadModel { get; set; }
    }
}
