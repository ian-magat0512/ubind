// <copyright file="IQuoteSearchResultItemReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;
    using NodaTime;
    using UBind.Domain;

    /// <summary>
    /// Data transfer object for quote search results document from search indexes.
    /// for a specific product.
    /// </summary>
    public interface IQuoteSearchResultItemReadModel : IEntitySearchResultItemReadModel
    {
        /// <summary>
        /// Gets customer Id.
        /// </summary>
        Guid CustomerId { get; }

        /// <summary>
        /// Gets organisation Id.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets owner Id.
        /// </summary>
        Guid OwnerId { get; }

        /// <summary>
        /// Gets string product Id.
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Gets product Name.
        /// </summary>
        string ProductName { get; }

        /// <summary>
        /// Gets quote title.
        /// </summary>
        string QuoteTitle { get; }

        /// <summary>
        /// Gets quote number.
        /// </summary>
        string QuoteNumber { get; }

        /// <summary>
        /// Gets customer Fullname..
        /// </summary>
        string CustomerFullName { get; }

        /// <summary>
        /// Gets customerPreferredName.
        /// </summary>
        string CustomerPreferredName { get; }

        /// <summary>
        /// Gets quote QuoteState.
        /// </summary>
        string QuoteState { get; }

        /// <summary>
        /// Gets quoteType.
        /// </summary>
        QuoteType QuoteType { get; }

        /// <summary>
        /// Gets the expiry date(in unix ticks).
        /// </summary>
        long? ExpiryTicksSinceEpoch { get; }

        /// <summary>
        /// Gets the expiry date.
        /// </summary>
        Instant? ExpiryTimestamp { get; }

        /// <summary>
        /// Gets a value indicating whether quote is discarded or not.
        /// </summary>
        bool IsDiscarded { get; }

        /// <summary>
        /// Gets a value indicating whether quote is test data or not.
        /// </summary>
        bool IsTestData { get; }

        string GetQuoteState(Instant timestamp);
    }
}
