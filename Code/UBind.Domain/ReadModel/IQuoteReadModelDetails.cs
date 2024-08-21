// <copyright file="IQuoteReadModelDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Collections.Generic;
    using NodaTime;

    /// <summary>
    /// Data transfer object for quotereadmodel
    /// for a specific product.
    /// </summary>
    public interface IQuoteReadModelDetails : IQuoteReadModelSummary
    {
        /// <summary>
        /// Gets the full name of the person who owns this quote.
        /// </summary>
        string OwnerFullName { get; }

        /// <summary>
        /// Gets the name of the organisation this quote was created under.
        /// </summary>
        string OrganisationName { get; }

        /// <summary>
        /// Gets the policy owner user id.
        /// </summary>
        Guid? PolicyOwnerUserId { get; }

        /// <summary>
        /// Gets the customer owner user id.
        /// </summary>
        Guid? CustomerOwnerUserId { get; }

        /// <summary>
        /// Gets or sets the documents associated with the quote.
        /// </summary>
        IEnumerable<QuoteDocumentReadModel> Documents { get; set; }

        /// <summary>
        /// Gets the ID of the latest calculation result.
        /// </summary>
        Guid LatestCalculationResultId { get; }

        /// <summary>
        /// Gets the ID of the form data used for the latest calculation result.
        /// </summary>
        Guid LatestCalculationResultFormDataId { get; }

        /// <summary>
        /// Gets or sets the last modified time by user.
        /// </summary>
        Instant? LastModifiedByUserTimestamp { get; }
    }
}
