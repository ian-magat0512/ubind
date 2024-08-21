// <copyright file="Client.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Funding.Iqumulate.Response
{
    using Newtonsoft.Json;
    using UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever;

    /// <summary>
    /// Iqumulate Premium Funding response - client into.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Client"/> class.
        /// </summary>
        [JsonConstructor]
        public Client()
        {
        }

        /// <summary>
        /// Gets the payment details.
        /// </summary>
        [JsonProperty]
        public PaymentDetails PaymentDetails { get; private set; }

        /// <summary>
        /// Gets the ABN.
        /// </summary>
        [JsonProperty]
        public string ABN { get; private set; }

        /// <summary>
        /// Gets the email.
        /// </summary>
        [JsonProperty]
        public string Email { get; private set; }

        /// <summary>
        /// Gets the EntityType(ABR).
        /// </summary>
        [JsonProperty]
        public string EntityType { get; private set; }

        /// <summary>
        /// Gets the FaxNumber.
        /// </summary>
        [JsonProperty]
        public string FaxNumber { get; private set; }

        /// <summary>
        /// Gets the Firstname.
        /// </summary>
        [JsonProperty]
        public string Firstname { get; private set; }

        /// <summary>
        /// Gets the Firstname.
        /// </summary>
        [JsonProperty]
        public string Lastname { get; private set; }

        /// <summary>
        /// Gets the legal name.
        /// </summary>
        [JsonProperty]
        public string LegalName { get; private set; }

        /// <summary>
        /// Gets the TelephoneNumber.
        /// </summary>
        [JsonProperty]
        public string TelephoneNumber { get; private set; }

        /// <summary>
        /// Gets the MobileNumber.
        /// </summary>
        [JsonProperty]
        public string MobileNumber { get; private set; }

        /// <summary>
        /// Gets the industry.
        /// </summary>
        [JsonProperty]
        public string Industry { get; private set; }

        /// <summary>
        /// Gets the postal address.
        /// </summary>
        [JsonProperty]
        public Address PostalAddress { get; private set; }

        /// <summary>
        /// Gets the main address.
        /// </summary>
        [JsonProperty]
        public Address StreetAddress { get; private set; }

        /// <summary>
        /// Gets the preferred contact method.
        /// </summary>
        [JsonProperty]
        public Address PreferredCommMethod { get; private set; }
    }
}
