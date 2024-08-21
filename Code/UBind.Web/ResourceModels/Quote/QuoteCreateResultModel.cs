// <copyright file="QuoteCreateResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// Resource model for serving a new quote instance for the quote.
    /// </summary>
    public class QuoteCreateResultModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCreateResultModel"/> class.
        /// </summary>
        /// <param name="quote">The new quote to be served.</param>
        /// <param name="currentUser">The user dto for logged in user.</param>
        public QuoteCreateResultModel(
            NewQuoteReadModel quote,
            UserResourceModel currentUser = null)
        {
            this.OrganisationId = quote.OrganisationId;
            this.IsTestData = quote.IsTestData;
            this.QuoteId = quote.Id;
            this.ProductId = quote.ProductId;
            this.Environment = quote.Environment;
            this.QuoteType = quote.Type;
            this.CurrentUser = currentUser;
            this.ProductReleaseId = quote.ProductReleaseId;
            FormData? formData = null;
            if (quote.LatestFormData != null)
            {
                formData = new FormData(quote.LatestFormData);
            }

            if (formData != null)
            {
                this.FormModel = formData.FormModel;
            }
        }

        /// <summary>
        /// Gets the Id of the quote.
        /// </summary>
        public Guid QuoteId { get; }

        /// <summary>
        /// Gets the ID of the organisation this quote was created under.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the Type of the quote.
        /// </summary>
        public QuoteType QuoteType { get; }

        /// <summary>
        /// Gets the current logged in user.
        /// </summary>
        public UserResourceModel CurrentUser { get; }

        /// <summary>
        /// Gets the Product Id for the specific quote.
        /// </summary>
        public Guid ProductId { get; }

        /// <summary>
        /// Gets the environment the quote is created for.
        /// </summary>
        public DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Gets a value indicating whether this application is considered to be test data.
        /// </summary>
        public bool IsTestData { get; }

        /// <summary>
        /// Gets or sets the form model.
        /// </summary>
        public JObject FormModel { get; set; }

        public Guid? ProductReleaseId { get; set; }
    }
}
