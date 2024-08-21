// <copyright file="IqumulateRequestData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate
{
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;

    /// <summary>
    /// For data requird fot an IQumulate funding request.
    /// </summary>
    public class IqumulateRequestData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IqumulateRequestData"/> class.
        /// </summary>
        /// <param name="quoteData">The quote data.</param>
        /// <param name="configuration">The IQumulate configuration.</param>
        /// <param name="quote">The quote.</param>
        /// <param name="personalDetails">The personal details of the customer.</param>
        public IqumulateRequestData(
            IQumulateQuoteData quoteData,
            IIqumulateConfiguration configuration,
            Quote quote,
            IPersonalDetails personalDetails,
            bool isMutual)
        {
            this.QuoteData = quoteData;
            this.Configuration = configuration;
            this.QuoteNumber = quote.QuoteNumber;
            this.CustomerEmail = personalDetails.Email;
            this.MobileNumber = personalDetails.MobilePhone;
            this.TelephoneNumber = personalDetails.HomePhone;
            this.IsMutual = isMutual;
        }

        /// <summary>
        /// Gets a value indicating whether a mutual tenant.
        /// </summary>
        public bool IsMutual { get; }

        /// <summary>
        /// Gets the quote data.
        /// </summary>
        public IQumulateQuoteData QuoteData { get; }

        /// <summary>
        /// Gets the IQumulate configuration.
        /// </summary>
        public IIqumulateConfiguration Configuration { get; }

        /// <summary>
        /// Gets the quote number of the quote the funding is for.
        /// </summary>
        public string QuoteNumber { get; }

        /// <summary>
        /// Gets the email of the customer.
        /// </summary>
        public string CustomerEmail { get; }

        /// <summary>
        /// Gets the mobile phone number of the customer.
        /// </summary>
        public string MobileNumber { get; }

        /// <summary>
        /// Gets the home phone number of the customer.
        /// </summary>
        public string TelephoneNumber { get; }

        /// <summary>
        /// Gets the company's trading name if available, otherwise null.
        /// </summary>
        public string TradingName { get; }

        /// <summary>
        /// Gets the company's ABN if available, otherwise null.
        /// </summary>
        public string Abn { get; }
    }
}
