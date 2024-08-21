// <copyright file="QuoteSetModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;

    /// <summary>
    /// Resource model for serving the list of quotes currently available for a product.
    /// </summary>
    public class QuoteSetModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteSetModel"/> class.
        /// </summary>
        /// <param name="quote">The quote to be passed back.</param>
        public QuoteSetModel(IQuoteReadModelSummary quote)
        {
            this.Id = quote.QuoteId;
            this.QuoteTitle = quote.QuoteTitle;
            this.OrganisationId = quote.OrganisationId;
            this.OrganisationAlias = quote.OrganisationAlias;
            this.PolicyId = quote.PolicyId;

            this.ProductAlias = quote.ProductAlias;
            this.QuoteNumber = quote.QuoteNumber;
            this.ProductId = quote.ProductId;
            this.ProductName = quote.ProductName;

            if (quote.CustomerId.HasValue)
            {
                this.CustomerDetails = new CustomerSimpleModel(quote.CustomerId.Value, quote.CustomerFullName);
            }

            this.OwnerUserId = quote.OwnerUserId;
            this.LastModifiedDateTime = quote.LastModifiedTimestamp.ToExtendedIso8601String();
            this.CreatedDateTime = quote.CreatedTimestamp.ToExtendedIso8601String();
            this.ExpiryDateTime = quote.ExpiryTimestamp?.ToExtendedIso8601String();
            this.Status = quote.QuoteState;
            this.QuoteType = (int)quote.QuoteType;
            this.TotalAmount = quote.LatestCalculationResult.PayablePrice.TotalPayable.ToString();
            this.IsTestData = quote.IsTestData;
            this.IsDiscarded = quote.IsDiscarded;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteSetModel"/> class.
        /// </summary>
        /// <param name="quote">The quote to be passed back.</param>
        /// <param name="timestamp">The current timestamp.</param>
        /// <param name="productAlias">The product alias.</param>
        public QuoteSetModel(
            Domain.Aggregates.Quote.Quote quote,
            Instant timestamp,
            string productAlias)
        {
            this.Id = quote.Id;
            this.QuoteTitle = quote.QuoteTitle;
            this.PolicyId = quote.PolicyId;

            this.ProductAlias = productAlias;
            this.QuoteNumber = quote.QuoteNumber;
            this.ProductId = quote.Aggregate.ProductId;
            if (quote.Aggregate.CustomerId.HasValue)
            {
                this.CustomerDetails = new CustomerSimpleModel(quote.Aggregate.CustomerId.Value, null);
            }

            this.CreatedDateTime = quote.CreatedTimestamp.ToExtendedIso8601String();
            this.Status = quote.QuoteStatus;
            this.LastModifiedDateTime = timestamp.ToExtendedIso8601String();
            this.QuoteType = (int)quote.Type;
            this.IsTestData = quote.IsTestData;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteSetModel"/> class.
        /// </summary>
        /// <param name="quoteFromSearchIndex">The quote data from search index.</param>
        /// <param name="timestamp">The current time.</param>
        public QuoteSetModel(IQuoteSearchResultItemReadModel quoteFromSearchIndex, Instant timestamp)
        {
            this.Id = quoteFromSearchIndex.Id;
            this.QuoteTitle = quoteFromSearchIndex.QuoteTitle;
            this.QuoteNumber = quoteFromSearchIndex.QuoteNumber;
            this.ProductId = quoteFromSearchIndex.ProductId;
            this.ProductName = quoteFromSearchIndex.ProductName;
            if (quoteFromSearchIndex.CustomerId != default)
            {
                this.CustomerDetails = new CustomerSimpleModel(
                    quoteFromSearchIndex.CustomerId,
                    PersonPropertyHelper.GetDisplayName(quoteFromSearchIndex.CustomerFullName));
            }

            this.LastModifiedDateTime = quoteFromSearchIndex.LastModifiedTimestamp.ToExtendedIso8601String();
            this.CreatedDateTime = quoteFromSearchIndex.CreatedTimestamp.ToExtendedIso8601String();
            this.ExpiryDateTime = quoteFromSearchIndex.ExpiryTimestamp.HasValue ?
                quoteFromSearchIndex.ExpiryTimestamp.Value.ToExtendedIso8601String() : string.Empty;

            this.Status = quoteFromSearchIndex.GetQuoteState(timestamp);
            this.QuoteType = (int)quoteFromSearchIndex.QuoteType;
            this.IsDiscarded = quoteFromSearchIndex.IsDiscarded;
            this.IsTestData = quoteFromSearchIndex.IsTestData;
        }

        // Parameterless constructor for deserialization from json.
        [JsonConstructor]
        private QuoteSetModel()
        {
        }

        /// <summary>
        /// Gets the ID of the quote.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the Policy ID, if any, of the quote.
        /// </summary>
        [JsonProperty]
        public Guid? PolicyId { get; private set; }

        /// <summary>
        /// Gets the quote title.
        /// </summary>
        [JsonProperty]
        public string QuoteTitle { get; private set; }

        /// <summary>
        /// Gets the quote number.
        /// </summary>
        [JsonProperty]
        public string QuoteNumber { get; private set; }

        /// <summary>
        /// Gets the ID of the product the quote is for.
        /// </summary>
        /// <remarks>Public setter for deserializer.</remarks>
        [JsonProperty]
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the alias of the product the quote is for.
        /// </summary>
        /// <remarks>Public setter for deserializer.</remarks>
        [JsonProperty]
        public string ProductAlias { get; private set; }

        /// <summary>
        /// Gets the name of the product the quote is for.
        /// </summary>
        [JsonProperty]
        public string ProductName { get; private set; }

        /// <summary>
        /// Gets the customer details.
        /// </summary>
        [JsonProperty]
        public CustomerSimpleModel CustomerDetails { get; private set; }

        /// <summary>
        /// Gets the total amount in the latest calculation result.
        /// </summary>
        [JsonProperty]
        public string TotalAmount { get; private set; }

        /// <summary>
        /// Gets the owner user id.
        /// </summary>
        public Guid? OwnerUserId { get; private set; }

        /// <summary>
        /// Gets the date the quote was last modified.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }

        /// <summary>
        /// Gets the current value of the Status item in the quote's form data.
        /// </summary>
        [JsonProperty]
        public string Status { get; private set; }

        /// <summary>
        /// Gets the date the quote was created.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the expiry date the quote was created.
        /// </summary>
        [JsonProperty]
        public string ExpiryDateTime { get; private set; }

        /// <summary>
        /// Gets the Type of the quote.
        /// </summary>
        [JsonProperty]
        public int QuoteType { get; private set; }

        /// <summary>
        /// Gets a value indicating whether data is for test.
        /// </summary>
        public bool IsTestData { get; private set; }

        /// <summary>
        /// Gets a value indicating whether quote is discarded.
        /// </summary>
        public bool IsDiscarded { get; private set; }

        /// <summary>
        /// Gets the ID of the organisation this quote was created under.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        public string OrganisationAlias { get; private set; }
    }
}
