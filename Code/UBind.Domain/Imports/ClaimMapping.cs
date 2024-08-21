// <copyright file="ClaimMapping.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Imports
{
    using Newtonsoft.Json;

    /// <summary>
    /// Container that represents the mapped claim properties.
    /// </summary>
    public class ClaimMapping
    {
        /// <summary>
        /// Gets the default mapping to be used.
        /// </summary>
        public static ClaimMapping Default
        {
            get
            {
                var defaultMapping = new ClaimMapping
                {
                    PolicyNumber = "PolicyNumber",
                    ClaimNumber = "ClaimNumber",
                    ReferenceNumber = "ReferenceNumber",
                    Amount = "Amount",
                    Description = "Description",
                    IncidentDate = "IncidentDate",
                    NotifiedDate = "NotifiedDate",
                    Status = "Status",
                };

                return defaultMapping;
            }
        }

        /// <summary>
        /// Gets the policy number of the claim.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string PolicyNumber { get; private set; }

        /// <summary>
        /// Gets the claim number of the claim.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string ClaimNumber { get; private set; }

        /// <summary>
        /// Gets the reference number for the claim.
        /// </summary>
        /// <remarks>
        /// This should only be used for fixing claims that were imported before automatic reference assignment was implemented.
        /// </remarks>
        [JsonProperty(Required = Required.Default)]
        public string ReferenceNumber { get; private set; }

        /// <summary>
        /// Gets the current amount of the claim.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Amount { get; private set; }

        /// <summary>
        /// Gets the current description of the claim.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the current incident date of the claim.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string IncidentDate { get; private set; }

        /// <summary>
        /// Gets the notified date of the claim.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string NotifiedDate { get; private set; }

        /// <summary>
        /// Gets the namem of the property specifying the status for the imported claim, if specified, otherwise null.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string Status { get; private set; }
    }
}
