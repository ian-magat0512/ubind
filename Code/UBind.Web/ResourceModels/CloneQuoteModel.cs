// <copyright file="CloneQuoteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Resource model for serving a new quote instance for the quote.
    /// </summary>
    public class CloneQuoteModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloneQuoteModel"/> class.
        /// </summary>
        /// <param name="quote">The new quote to be served.</param>
        public CloneQuoteModel(NewQuoteReadModel quote)
        {
            this.PolicyId = quote.PolicyId;
            this.QuoteId = quote.Id;
        }

        /// <summary>
        /// Gets or sets the Id of the policy.
        /// </summary>
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the quote.
        /// </summary>
        public Guid QuoteId { get; set; }
    }
}
