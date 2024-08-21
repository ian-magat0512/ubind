// <copyright file="IQuoteSearchIndexWriteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Quotes Model for writing into search indexes/documents.
    /// for a specific product.
    /// </summary>
    public interface IQuoteSearchIndexWriteModel : IEntitySearchIndexWriteModel
    {
        /// <summary>
        /// Gets customer Id.
        /// </summary>
        Guid? CustomerId { get; }

        /// <summary>
        /// Gets organisation ID.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets product Id.
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Gets quote title.
        /// </summary>
        string QuoteTitle { get; }

        /// <summary>
        /// Gets quote number.
        /// </summary>
        string QuoteNumber { get; }

        /// <summary>
        /// Gets the Form Data Json.
        /// </summary>
        string FormDataJson { get; }

        /// <summary>
        /// Gets customer fullname.
        /// </summary>
        string CustomerFullname { get; }

        /// <summary>
        /// Gets owner's user id.
        /// </summary>
        Guid? OwnerUserId { get; }

        /// <summary>
        /// Gets owner's Person id.
        /// </summary>
        Guid? OwnerPersonId { get; }

        /// <summary>
        /// Gets owner's fullname.
        /// </summary>
        string OwnerFullname { get; }

        /// <summary>
        /// Gets the customer Preferred Name.
        /// </summary>
        string CustomerPreferredName { get; }

        /// <summary>
        /// Gets the customer Email.
        /// </summary>
        string CustomerEmail { get; }

        /// <summary>
        /// Gets the customer Alternative email.
        /// </summary>
        string CustomerAlternativeEmail { get; }

        /// <summary>
        /// Gets customerPreferredName.
        /// </summary>
        string CustomerHomePhone { get; }

        /// <summary>
        /// Gets customerPreferredName.
        /// </summary>
        string CustomerWorkPhone { get; }

        /// <summary>
        /// Gets Customer mobile phone..
        /// </summary>
        string CustomerMobilePhone { get; }

        /// <summary>
        /// Gets quote QuoteState.
        /// </summary>
        string QuoteState { get; }

        /// <summary>
        /// Gets quoteType.
        /// </summary>
        QuoteType QuoteType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the quote is discarded or not.
        /// </summary>
        bool IsDiscarded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the quote is test data or not.
        /// </summary>
        bool IsTestData { get; set; }

        /// <summary>
        /// Gets the expiry date time.
        /// </summary>
        long? ExpiryTicksSinceEpoch { get; }

        string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the policy number of the quote.
        /// </summary>
        string PolicyNumber { get; set; }
    }
}
