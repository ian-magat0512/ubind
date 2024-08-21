// <copyright file="QuoteSearchResultItemReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using System;
    using Lucene.Net.Documents;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Search;
    using UBind.Domain.ValueTypes;

    /// <inheritdoc/>
    public class QuoteSearchResultItemReadModel : EntitySearchResultItemReadModel, IQuoteSearchResultItemReadModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteSearchResultItemReadModel"/> class.
        /// </summary>
        public QuoteSearchResultItemReadModel(Document document, Guid id, long createdTimestamp, long lastUpdatedTimestamp)
            : base(id, createdTimestamp, lastUpdatedTimestamp)
        {
            this.QuoteState = document.Get(QuoteLuceneFieldsNames.FieldQuoteState);
            this.QuoteTitle = document.Get(QuoteLuceneFieldsNames.FieldQuoteTitle);
            this.ProductId = Guid.Parse(document.Get(QuoteLuceneFieldsNames.FieldProductId));
            this.QuoteType = (QuoteType)int.Parse(document.Get(QuoteLuceneFieldsNames.FieldQuoteType));
            this.ProductName = document.Get(QuoteLuceneFieldsNames.FieldProductName);
            this.QuoteNumber = document.Get(QuoteLuceneFieldsNames.FieldQuoteNumber);
            this.IsDiscarded = bool.Parse(document.Get(QuoteLuceneFieldsNames.IsDiscarded));
            this.IsTestData = bool.Parse(document.Get(QuoteLuceneFieldsNames.IsTestData));

            if (!string.IsNullOrEmpty(document.Get(QuoteLuceneFieldsNames.FieldCustomerId)))
            {
                this.CustomerId = Guid.Parse(document.Get(QuoteLuceneFieldsNames.FieldCustomerId));
            }

            if (!string.IsNullOrEmpty(document.Get(QuoteLuceneFieldsNames.FieldOrganisationId)))
            {
                this.OrganisationId = Guid.Parse(document.Get(QuoteLuceneFieldsNames.FieldOrganisationId));
            }

            this.CustomerFullName = document.Get(QuoteLuceneFieldsNames.FieldCustomerFullName);

            var fieldExpiryTimestamp = document.Get(QuoteLuceneFieldsNames.FieldExpiryTimestamp);
            this.ExpiryTicksSinceEpoch = fieldExpiryTimestamp != null ? long.Parse(fieldExpiryTimestamp) : (long?)null;
        }

        /// <inheritdoc/>
        public Guid ProductId { get; private set; }

        /// <inheritdoc/>
        public string ProductName { get; private set; }

        /// <inheritdoc/>
        public string QuoteTitle { get; private set; }

        /// <inheritdoc/>
        public string QuoteNumber { get; private set; }

        /// <inheritdoc/>
        public Guid CustomerId { get; private set; }

        /// <inheritdoc/>
        public Guid OrganisationId { get; private set; }

        /// <inheritdoc/>
        public Guid OwnerId { get; private set; }

        /// <inheritdoc/>
        public string CustomerFullName { get; private set; }

        /// <inheritdoc/>
        public string CustomerPreferredName { get; private set; }

        /// <inheritdoc/>
        public string QuoteState { get; private set; }

        /// <inheritdoc/>
        public QuoteType QuoteType { get; private set; }

        /// <inheritdoc/>
        public bool IsDiscarded { get; private set; }

        /// <inheritdoc/>
        public bool IsTestData { get; private set; }

        /// <inheritdoc/>
        public long? ExpiryTicksSinceEpoch { get; private set; }

        /// <inheritdoc/>
        public Instant? ExpiryTimestamp
        {
            get => this.ExpiryTicksSinceEpoch.HasValue
              ? Instant.FromUnixTimeTicks(this.ExpiryTicksSinceEpoch.Value)
              : (Instant?)null;

            set => this.ExpiryTicksSinceEpoch = value.HasValue
                ? value.Value.ToUnixTimeTicks()
                : (long?)null;
        }

        /// <summary>
        /// Retrieve quote state taking into account whether it might have expired given the curent time.
        /// </summary>
        /// <param name="timestamp">The current time.</param>
        /// <returns>the quote state.</returns>
        public string GetQuoteState(Instant timestamp)
        {
            return this.ExpiryTimestamp != null
                && this.ExpiryTimestamp <= timestamp
                && !this.QuoteState.EqualsIgnoreCase(StandardQuoteStates.Declined)
                && !this.QuoteState.EqualsIgnoreCase(StandardQuoteStates.Complete)
                    ? StandardQuoteStates.Expired
                    : this.QuoteState;
        }
    }
}
