// <copyright file="IntroducerData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using Newtonsoft.Json;

    /// <summary>
    /// Introducer data for IQumulate requests.
    /// </summary>
    public class IntroducerData
    {
        /// <summary>
        /// Gets or sets the affinity scheme code.
        /// string (maxlen=50)
        /// (required) Affinity Scheme Code to be used when calculating the quote.Schemes exist at a introducer level.
        /// Note: Contact your IQPF Relationship Manager to retrieve your Affinity Scheme Code(s).
        /// </summary>
        [JsonProperty]
        public string AffinitySchemeCode { get; set; }

        /// <summary>
        /// Gets or sets the introducer's contact email address.
        /// string (maxlen=50)
        /// (optional) If supplied, this email address will be CC’ed on welcome letter email communication with the client.
        /// Note: Contact your IQPF Relationship Manager to disable these CC emails for all clients.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string IntroducerContactEmail { get; set; }
    }
}
