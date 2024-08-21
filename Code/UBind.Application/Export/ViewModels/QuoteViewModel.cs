// <copyright file="QuoteViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using System;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;

    /// <summary>
    /// Quote view model for Razor Templates to use.
    /// </summary>
    public class QuoteViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteViewModel"/> class.
        /// </summary>
        /// <param name="quoteAggregate">The quote aggregate.</param>
        /// <param name="product">The product.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        public QuoteViewModel(Domain.Aggregates.Quote.QuoteAggregate quoteAggregate, Product product, Guid quoteId)
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);

            if (product == null)
            {
                throw new ArgumentNullException("Product is null");
            }

            if (quote.VersionNumber != default)
            {
                this.VersionNumber = quote.VersionNumber.ToString();
            }

            var expiryTime = (quote?.ExpiryTimestamp).GetValueOrDefault();

            if (expiryTime != default && product.Details.QuoteExpirySetting.Enabled)
            {
                this.ExpiryDate = expiryTime.ToRfc5322DateStringInAet();
                this.ExpiryTimeOfDay = expiryTime.To12HourClockTimeInAet();
            }

            this.Id = quote.Id.ToString();
            this.TenantId = quoteAggregate.TenantId.ToString();
            this.QuoteNumber = quote.QuoteNumber;
            var createdTimestamp = quote?.CreatedTimestamp;
            this.CreationDate = createdTimestamp?.ToRfc5322DateStringInAet();
            this.CreationTime = createdTimestamp?.To12HourClockTimeInAet();
        }

        /// <summary>
        /// Gets the quote string tenant Id.
        /// </summary>
        public string TenantId { get; private set; }

        /// <summary>
        /// Gets the quote Id.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the quote number.
        /// </summary>
        public string QuoteNumber { get; private set; }

        /// <summary>
        /// Gets the version number of the latest quote version.
        /// </summary>
        public string VersionNumber { get; private set; }

        /// <summary>
        /// Gets the quote created date.
        /// </summary>
        public string CreationDate { get; private set; }

        /// <summary>
        /// Gets the quote expiry date.
        /// </summary>
        public string ExpiryDate { get; private set; }

        /// <summary>
        /// Gets the quote expiry time. example format: h:mm tt.
        /// </summary>
        public string ExpiryTimeOfDay { get; private set; }

        /// <summary>
        /// Gets the policy created time.
        /// </summary>
        public string CreationTime { get; private set; }
    }
}
