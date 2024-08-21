// <copyright file="ClientData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Client data for IQumulate requests.
    /// </summary>
    public class ClientData
    {
        /// <summary>
        /// Gets or sets the legal name of the client.
        /// string (maxlen=50)
        /// (required) Legal name of the client.
        /// </summary>
        [JsonProperty]
        public string LegalName { get; set; }

        /// <summary>
        /// Gets or sets the trading name of the client's business.
        /// string (maxlen=50)
        /// Trading name of the client.
        /// (optional) if the loan is predominantly for commercial policies.
        /// (not required) if the loan is for domestic policies.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TradingName { get; set; }

        /// <summary>
        /// Gets or sets the entity type.
        /// string (maxlen=3)
        /// (optional) ABR Entity Type of the client.
        /// e.g.:
        /// CUT = Corporate Unit Trust
        /// FPT = Family Partnership
        /// FXT = Fixed Trust
        /// FUT = Fixed Unit Trust
        /// IND = Individual/Sole Trader
        /// LPT = Limited Partnership
        /// OIE = Other Incorporated Entity
        /// PRV = Australian Private Company
        /// PUB = Australian Public Company
        /// Refer to appendix in the iQumulate documentation for a full list.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string EntityType { get; set; }

        /// <summary>
        /// Gets or sets the Australian Business Number.
        /// string (maxlen=11)
        /// ABN of the client.
        /// (optional) if the loan is predominantly for commercial policies.
        /// (not required) if the loan is for domestic policies.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Abn { get; set; }

        /// <summary>
        /// Gets or sets the industry code.
        /// string (maxlen=4)
        /// (optional) ABS Industry Code of the client’s industry.
        /// e.g. 0152 Cotton Growing.
        /// Refer to appendix in the iQumulate documentation for a full list.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Industry { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Address StreetAddress { get; set; }

        /// <summary>
        /// Gets or sets the street address.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Address PostalAddress { get; set; }

        /// <summary>
        /// Gets or sets the mobile number.
        /// (optional) Mobile number of the client or, if the client is a company,
        /// the mobile phone number of the primary contact at the company.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string MobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the telphone number of the client.
        /// string (maxlen=20)
        /// (optional) Phone number of the client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string TelephoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the fax number of the client.
        /// string (maxlen=20)
        /// (optional) Fax number of the client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FaxNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address of the client.
        /// string (maxlen=255)
        /// (required) Email address of the client or, if the client is a company,
        /// the email address of the primary contact at the company.
        /// </summary>
        [JsonProperty]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the client's title, e.g "Mr", "Dr", "Miss", "Mrs" etc
        /// string (maxlen=5)
        /// (optional) Preferred title of the primary contact for the client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the preferred first name of the primary contact for the client.
        /// string (maxlen=50)
        /// (required) Preferred first name of the primary contact for the client.
        /// </summary>
        [JsonProperty]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the preferred surname of the primary contact for the client.
        /// string (maxlen=50)
        /// (optional) Preferred surname of the primary contact for the client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the introducer client reference.
        /// string (maxlen=255)
        /// (optional) Introducer’s client reference – free form field that allows the referring broker firm to enter
        /// their tracking ID of the client/loan.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string IntroducerClientReference { get; set; }

        /// <summary>
        /// Gets or sets the borrowers.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<Borrower> Borrowers { get; set; }
    }
}
