// <copyright file="ClaimImportData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Imports
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a claim import object.
    /// </summary>
    public class ClaimImportData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimImportData"/> class.
        /// </summary>
        /// <param name="jObject">Represents a JSON object.</param>
        /// <param name="mapping">Represents the claim mapping definition.</param>
        public ClaimImportData(JObject jObject, ClaimMapping mapping)
        {
            if (jObject == null || mapping == null)
            {
                return;
            }

            this.PolicyNumber = jObject.Value<string>(mapping.PolicyNumber);
            this.ClaimNumber = jObject.Value<string>(mapping.ClaimNumber);
            this.Amount = jObject.Value<decimal>(mapping.Amount);
            this.Description = jObject.Value<string>(mapping.Description);
            this.IncidentDate = jObject.Value<string>(mapping.IncidentDate);
            this.NotifiedDate = jObject.Value<string>(mapping.NotifiedDate);
            if (mapping.Status != null)
            {
                this.Status = jObject.Value<string>(mapping.Status);
            }

            if (mapping.ReferenceNumber != null)
            {
                this.ReferenceNumber = jObject.Value<string>(mapping.ReferenceNumber);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimImportData"/> class.
        /// </summary>
        [JsonConstructor]
        public ClaimImportData()
        {
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
        /// Gets the reference number of the claim.
        /// </summary>
        /// <remarks>
        /// Only used for fixing claims imported before automatic reference number assignment was implemented.
        /// </remarks>
        [JsonProperty(Required = Required.Default)]
        public string ReferenceNumber { get; private set; }

        /// <summary>
        /// Gets the current amount of the claim.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public decimal Amount { get; private set; }

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
        /// Gets the status to set the imported quote to, if specified, otherwise null.
        /// </summary>
        [JsonProperty(Required = Required.Default)]
        public string Status { get; private set; }
    }
}
