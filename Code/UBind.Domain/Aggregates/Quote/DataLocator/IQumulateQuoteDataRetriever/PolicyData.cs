// <copyright file="PolicyData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using Newtonsoft.Json;

    /// <summary>
    /// Policies data for IQumulate requests.
    /// </summary>
    public class PolicyData
    {
        /// <summary>
        /// Gets or sets the policy number
        /// string (maxlen=50)
        /// (required) Policy number.
        /// </summary>
        [JsonProperty]
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the invoice number.
        /// invoiceNumber
        /// string (maxlen=20)
        /// (optional) Policy invoice number.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets the insurance policy class code.
        /// string (maxlen=10)
        /// E.g.:
        /// AV = Aviation
        /// BF = Broker Fees
        /// CI = Business Interruption
        /// IS = Industrial Special Risks
        /// MV = Motor Vehicle (cancellable)
        /// MVN = Motor Vehicle (non-cancellable)
        /// etc
        /// (required) Insurance policy class.
        /// Refer to appendix in the iQumulate documentation for a full list of codes.
        /// </summary>
        [JsonProperty]
        public string PolicyClassCode { get; set; }

        /// <summary>
        /// Gets or sets the policy underwriter code.
        /// string (maxlen=30)
        /// (required) Underwriter code for the policy’s underwriter.
        /// Note: Must match a code in IQPF’s system. This needs to be provided to IQPF before testing.
        /// </summary>
        [JsonProperty]
        public string PolicyUnderwriterCode { get; set; }

        /// <summary>
        /// Gets or sets the policy inception date.
        /// date (format yyyy-mm-dd)
        /// (optional) Policy inception date.
        /// • If not supplied, current date will be assumed.
        /// • Cannot be more than 30 days before the first Instalment date.
        /// • Cannot be after the policy expiry date.
        /// </summary>
        [JsonProperty]
        public string PolicyInceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the policy expiry date.
        /// date (format yyyy-mm-dd)
        /// (required) Expiry date of the policy.
        /// • The expiry date of the policy cannot be before the policy inception date.
        /// • Cannot be before the last instalment date.
        /// </summary>
        [JsonProperty]
        public string PolicyExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the amount of the policy.
        /// xsd:decimal
        /// (required) Amount of the policy.
        /// </summary>
        [JsonProperty]
        public string PolicyAmount { get; set; }

        /// <summary>
        /// Gets or sets the DEFT reference number.
        /// xsd:string (maxlen=50)
        /// (optional) The valid DEFT Reference Number of a policy.
        /// Refer to the appendix for more details on DEFT Reference Number.
        /// </summary>
        [JsonProperty("DEFTReferenceNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string DeftReferenceNumber { get; set; }
    }
}
