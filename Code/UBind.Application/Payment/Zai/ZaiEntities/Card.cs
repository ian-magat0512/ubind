// <copyright file="Card.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Zai.ZaiEntities
{
    using Newtonsoft.Json;

    public class Card
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("full_name")]
        public string CardholderName { get; set; }

        /// <summary>
        /// Gets or sets the card number with masked digits (BIN and last four digits).
        /// </summary>
        [JsonProperty("number")]
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the expiry month in MM-format.
        /// </summary>
        [JsonProperty("expiry_month")]
        public string ExpiryMonth { get; set; }

        /// <summary>
        /// Gets or sets the expiry year in YYYY-format.
        /// </summary>
        [JsonProperty("expiry_year")]
        public string ExpiryYear { get; set; }
    }
}
