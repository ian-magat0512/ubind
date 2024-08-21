// <copyright file="ClaimCreateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Claim
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;

    public class ClaimCreateModel
    {
        /// <summary>
        /// Gets or sets the tenant ID or tenant alias.
        /// </summary>
        [JsonProperty]
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the organisation ID or organisation alias.
        /// </summary>
        [JsonProperty]
        public string Organisation { get; set; }

        /// <summary>
        /// Gets or sets the product ID or product alias.
        /// </summary>
        [JsonProperty]
        public string Product { get; set; }

        /// <summary>
        /// Gets or sets the Deployment Environment.
        /// </summary>
        [JsonProperty]
        public DeploymentEnvironment? Environment { get; set; }

        /// <summary>
        /// Gets or sets the policy ID to associate the claim with.
        /// </summary>
        [JsonProperty]
        public Guid? PolicyId { get; set; }

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
        /// Gets or sets the data to seed the claim form with.
        /// </summary>
        [JsonProperty]
        public JObject FormData { get; set; }
    }
}
