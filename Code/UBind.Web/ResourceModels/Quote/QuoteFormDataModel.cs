// <copyright file="QuoteFormDataModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Resource model for wrapping results for requests of latest form data from quote.
    /// </summary>
    public class QuoteFormDataModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteFormDataModel"/> class.
        /// </summary>
        /// <param name="quote">The quote.</param>
        public QuoteFormDataModel(IQuoteReadModelDetails quote)
        {
            this.Id = quote.QuoteId;
            this.OrganisationId = quote.OrganisationId;
            this.ProductId = quote.ProductId;
            this.Environment = quote.Environment.ToString();
            this.LatestFormData = new FormData(quote.LatestFormData);
            this.PolicyId = quote.PolicyId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteFormDataModel"/> class.
        /// </summary>
        [JsonConstructor]
        private QuoteFormDataModel()
        {
        }

        /// <summary>
        /// Gets or sets the ID of the quote.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets the ID of the organisation this quote was created under.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets or sets the latest form data.
        /// </summary>
        [JsonProperty(PropertyName = "policyId")]
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the product of the quote.
        /// </summary>
        [JsonProperty(PropertyName = "productId")]
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the environment the quote belongs to.
        /// </summary>
        [JsonProperty(PropertyName = "environment")]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets the latest form data.
        /// </summary>
        [JsonProperty(PropertyName = "formModel")]
        public FormData LatestFormData { get; set; }
    }
}
