// <copyright file="CardAccount.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Zai.ZaiEntities
{
    using Newtonsoft.Json;

    public class CardAccount
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("active")]
        public bool IsActive { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("cvv_verified")]
        public bool IsCvvVerified { get; set; }

        [JsonProperty("verification_status")]
        public string VerificationStatus { get; set; }

        [JsonProperty("card")]
        public Card Card { get; set; }
    }
}
