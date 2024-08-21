// <copyright file="PaymentMethod.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;

    public class PaymentMethod : ISchemaObject
    {
        public PaymentMethod(Domain.Payment.PaymentMethod model)
        {
            this.Id = model.Id.ToString();
            this.Name = model.Name;
            this.Alias = model.Alias;
        }

        [JsonConstructor]
        public PaymentMethod()
        {
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; set; }
    }
}
