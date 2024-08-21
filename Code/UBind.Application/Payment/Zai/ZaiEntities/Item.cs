// <copyright file="Item.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Zai.ZaiEntities
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the properties of a Zai <see cref="Item"/>.
    /// </summary>
    public class Item
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("state")]
        [JsonConverter(typeof(Domain.JsonConverters.StringEnumHumanizerJsonConverter))]
        public ZaiItemState State { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("total_amount")]
        public decimal TotalAmount { get; set; }

        [JsonProperty("payment_type_id")]
        public int PaymentTypeId { get; set; }

        [JsonProperty("dynamic_descriptor")]
        public string ReferenceId { get; set; }
    }
}
