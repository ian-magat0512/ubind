// <copyright file="Policy.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate.Response
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Iqumulate Premium Funding response - Policy into.
    /// </summary>
    public class Policy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Policy"/> class.
        /// </summary>
        [JsonConstructor]
        public Policy()
        {
        }

        /// <summary>
        /// Gets the DEFTReferenceNumber.
        /// </summary>
        [JsonProperty]
        public string DEFTReferenceNumber { get; private set; }

        /// <summary>
        /// Gets InvoiceNumber.
        /// </summary>
        [JsonProperty]
        public string InvoiceNumber { get; private set; }

        /// <summary>
        /// Gets the PolicyNumber.
        /// </summary>
        [JsonProperty]
        public string PolicyNumber { get; private set; }

        /// <summary>
        /// Gets thePolicyAmount.
        /// </summary>
        [JsonProperty]
        public decimal PolicyAmount { get; private set; }

        /// <summary>
        /// Gets the PolicyClassCode.
        /// </summary>
        [JsonProperty]
        public string PolicyClassCode { get; private set; }

        /// <summary>
        /// Gets the PolicyUnderwriterCode.
        /// </summary>
        [JsonProperty]
        public string PolicyUnderwriterCode { get; private set; }

        /// <summary>
        /// Gets the PolicyExpiryDate.
        /// </summary>
        [JsonProperty]
        public DateTime PolicyExpiryDate { get; private set; }

        /// <summary>
        /// Gets the PolicyInceptionDate.
        /// </summary>
        public DateTime PolicyInceptionDate { get; private set; }
    }
}
