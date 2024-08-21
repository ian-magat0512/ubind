// <copyright file="QuoteCalculation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class is needed because we need to generate json representation of quote calculation object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class QuoteCalculation : FormsAppCalculation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCalculation"/> class.
        /// </summary>
        /// <param name="calculation">The calculation model.</param>
        /// <param name="isFormatted">An optional flag that indicates whether the payment-related fields should have their values formatted.</param>
        [JsonConstructor]
        public QuoteCalculation(Domain.ReadWriteModel.CalculationResult calculation, bool isFormatted = false)
        {
            if (calculation != null)
            {
                this.AssignValues(calculation, isFormatted);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteCalculation"/> class.
        /// </summary>
        /// <param name="latestCalculation">The latest calculation object.</param>
        /// <param name="isFormatted">An optional flag that indicates whether the payment-related fields should have their values formatted.</param>
        public QuoteCalculation(string latestCalculation, bool isFormatted = false)
        {
            if (latestCalculation.IsNotNullOrEmpty())
            {
                var setting = CustomSerializerSetting.JsonSerializerSettings;
                var calculation = JsonConvert.DeserializeObject<Domain.ReadWriteModel.CalculationResult>(latestCalculation, setting);
                this.AssignValues(calculation, isFormatted);
            }
        }

        /// <summary>
        /// Gets or sets the calculation risks.
        /// </summary>
        [JsonProperty(PropertyName = "risks", Order = 4)]
        public List<Risk> Risks { get; set; }

        /// <summary>
        /// Gets or sets the calculation payment.
        /// </summary>
        [JsonProperty(PropertyName = "payment", Order = 5)]
        public Payment Payment { get; set; }

        public void AssignValues(ReadWriteModel.CalculationResult calculation, bool isFormatted = false)
        {
            this.State = calculation.JObject.SelectToken("state")?.ToString();
            var triggers = calculation.JObject.SelectToken("triggers");
            this.Triggers = triggers == null ? new JObject() : (JObject)triggers;

            var risks = new List<Risk>();
            int i = 0;
            while (true)
            {
                i++;
                var token = calculation.JObject[$"risk{i}"];
                if (token == null || token.Type == JTokenType.Null)
                {
                    break;
                }

                risks.Add(new Risk(JObject.Parse(token?.ToString())));
            }

            this.Risks = risks;
            var paymentAvailable = calculation.JObject.TryGetValue("payment", out JToken paymentToken);
            if (paymentAvailable && paymentToken.Type != JTokenType.Null)
            {
                this.Payment = new Payment(JObject.FromObject(calculation), isFormatted);
            }
        }
    }
}
