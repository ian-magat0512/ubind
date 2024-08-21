// <copyright file="QuoteVersionListModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Resource model responsible for serving quote versions available for a quote.
    /// </summary>
    public class QuoteVersionListModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionListModel"/> class.
        /// </summary>
        /// <param name="quoteVersion">The quote version read model.</param>
        public QuoteVersionListModel(IQuoteVersionReadModelSummary quoteVersion)
        {
            if (quoteVersion != null)
            {
                this.Id = quoteVersion.QuoteVersionId;
                this.QuoteId = quoteVersion.QuoteId;
                this.QuoteNumber = quoteVersion.QuoteNumber;
                this.QuoteVersionNumber = quoteVersion.QuoteVersionNumber;
                this.CreatedDateTime = quoteVersion.CreatedTimestamp.ToString();
                this.LastModifiedDateTime = quoteVersion.LastModifiedTimestamp.ToString();
            }
        }

        /// <summary>
        /// Gets the quote version id.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the quote id for this quote version.
        /// </summary>
        [JsonProperty]
        public Guid QuoteId { get; private set; }

        /// <summary>
        /// Gets the quote number of this quote version.
        /// </summary>
        [JsonProperty]
        public string QuoteNumber { get; private set; }

        /// <summary>
        /// Gets the version number for this quote.
        /// </summary>
        [JsonProperty]
        public int QuoteVersionNumber { get; private set; }

        /// <summary>
        /// Gets the created date and time in ISO8601 format.
        /// </summary>
        [JsonProperty]
        public string CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the last modified date and time in ISO8601 format.
        /// </summary>
        [JsonProperty]
        public string LastModifiedDateTime { get; private set; }
    }
}
