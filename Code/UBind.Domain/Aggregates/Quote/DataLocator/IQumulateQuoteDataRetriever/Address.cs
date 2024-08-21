// <copyright file="Address.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using Newtonsoft.Json;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// For representing addresses.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Gets or sets the first line of the address, i.e. street and number etc.
        /// </summary>
        [JsonProperty]
        public string StreetLine1 { get; set; }

        /// <summary>
        /// Gets or sets the first line of the address, i.e. street and number etc.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StreetLine2 { get; set; }

        /// <summary>
        /// Gets or sets the suburb or city.
        /// </summary>
        [JsonProperty]
        public string Suburb { get; set; }

        /// <summary>
        /// Gets or sets the postcode.
        /// </summary>
        [JsonProperty]
        public string Postcode { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        [JsonProperty]
        public State State { get; set; }
    }
}
