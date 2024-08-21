// <copyright file="QuoteCreateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Quote
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Validation;

    public class QuoteCreateModel
    {
        /// <summary>
        /// Gets or sets the tenant ID. If not passed, it uses the tenant of the logged in user.
        /// Note: this is for backward compatibility with YIHQ.
        /// </summary>
        [JsonProperty]
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID or tenant alias. If not passed, it uses the tenant of the logged in user.
        /// </summary>
        [JsonProperty]
        public string? Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organisation ID.
        /// </summary>
        [JsonProperty]
        public string OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the organisation ID or organisation alias.
        /// Note: this is used for backward compatibility with YIHQ.
        /// </summary>
        [JsonProperty]
        public string Organisation { get; set; }

        /// <summary>
        /// Gets or sets the portal Id.
        /// Note: this is used for backward compatibility with YIHQ.
        /// </summary>
        [JsonProperty]
        public string PortalId { get; set; }

        /// <summary>
        /// Gets or sets the portal Id or Alias.
        /// </summary>
        [JsonProperty]
        public string Portal { get; set; }

        /// <summary>
        /// Gets or sets the product ID.
        /// Note: this is used for backward compatibility with YIHQ.
        /// </summary>
        [JsonProperty]
        public string ProductId { get; set; }

        /// <summary>
        /// Gets or sets the product ID or product alias.
        /// </summary>
        /// Note: requireIf is disabled because you can pick any of the two ( productId or product param), also
        /// error handling is already in the code to handle this.
        // [RequiredIf("QuoteType", UBind.Domain.QuoteType.NewBusiness)]
        [JsonProperty]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the Deployment Environment.
        /// </summary>
        [RequiredIf("QuoteType", QuoteType.NewBusiness)]
        [JsonProperty]
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the customer ID.
        /// </summary>
        [JsonProperty]
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this quote is to be considered a test quote.
        /// </summary>
        [JsonProperty]
        public bool IsTestData { get; set; } = false;

        /// <summary>
        /// Gets or sets the data to seed the quote form with.
        /// </summary>
        [JsonProperty]
        public JObject FormData { get; set; }

        /// <summary>
        /// Gets or sets the Quote Type to create.
        /// Possible values are:
        /// 0 = New Business
        /// 1 = Renewal
        /// 2 = Adjustment
        /// 3 = Cancellation
        /// Defaults to NewBusiness (0).
        /// </summary>
        [JsonProperty]
        public QuoteType QuoteType { get; set; } = QuoteType.NewBusiness;

        /// <summary>
        /// Gets or sets the Policy ID, which is required and applicable only if the QuoteType is anything other than NewBusiness (0).
        /// </summary>
        [RequiredIf("QuoteType", QuoteType.NewBusiness, ComparisonOperator.NotEqual)]
        [JsonProperty]
        public Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to discard the quote if a non-new business quote already exists for
        /// the given Policy. Only applicable when the Policy ID is provided.
        /// If an existing non-new business quote exists, and this property is set to false, then an error will be
        /// raised instead.
        /// </summary>
        [JsonProperty]
        public bool DiscardExistingQuote { get; set; }

        /// <summary>
        /// Gets or sets a value for the release to use when creating a quote.
        /// The format can either be a release number or release ID.
        /// If not set, the system will automatically select the relevant release based upon configuration options.
        /// </summary>
        [JsonProperty]
        public string? ProductRelease { get; set; }
    }
}
