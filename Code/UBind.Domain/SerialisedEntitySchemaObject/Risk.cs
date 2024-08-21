// <copyright file="Risk.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class is needed because we need to generate json representation of risk object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class Risk : ISchemaObject
    {
        /// <summary>
        /// This is the map where the key is the old property name, and the value is the new property name.
        /// </summary>
        Dictionary<string, string> paymentOldToNewPropertyMap = new Dictionary<string, string>()
        {
            { "premium", "basePremium"},
            { "ESL", "esl" },
            { "GST", "premiumGst" },
            { "SDACT", "stampDutyAct" },
            { "SDNSW", "stampDutyNsw" },
            { "SDNT", "stampDutyNt" },
            { "SDQLD", "stampDutyQld" },
            { "SDSA", "stampDutySa" },
            { "SDTAS", "stampDutyTas" },
            { "SDVIC", "stampDutyVic" },
            { "SDWA", "stampDutyWa" },
            { "total", "totalPremium" },
        };

        /// <summary>
        /// These are the property names used for generating the Risk class object,
        /// this will be useful for generating 'other' where we would exclude these property names,
        /// and add properties with names other than on the list.
        /// </summary>
        List<string> usedPropertyNames = new List<string>()
        {
            "settings",
            "ratingFactors",
            "checks",
            "premium",
            "other",
            "statutoryRates",
            "payment",
        };

        [JsonConstructor]
        public Risk()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Risk"/> class.
        /// </summary>
        /// <param name="risk">The risk object.</param>
        public Risk(JObject risk)
        {
            this.Name = risk.SelectToken("name")?.ToString() ?? risk.SelectToken("settings.riskName")?.ToString() ?? string.Empty;
            this.RatingFactors = this.GetChildObject(risk, "ratingFactors");
            this.Checks = this.GetChildObject(risk, "checks");
            this.Premium = this.GetChildObject(risk, "premium");
            this.Other = this.GetChildObject(risk, "other");
            this.StatutoryRates = new StatutoryRates(this.GetChildObject(risk, "statutoryRates"));
            this.Payment = this.GetChildObject(risk, "payment").RenamePropertyNamesUsingMap(this.paymentOldToNewPropertyMap);
            this.PopulateUnusedPropertyIntoOtherSection(risk);
        }

        /// <summary>
        /// Gets or sets the risk name.
        /// </summary>
        [JsonProperty(PropertyName = "name", Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the rating factors.
        /// </summary>
        [JsonProperty(PropertyName = "ratingFactors", Order = 2)]
        public JObject RatingFactors { get; set; }

        /// <summary>
        /// Gets or sets the checks.
        /// </summary>
        [JsonProperty(PropertyName = "checks", Order = 3)]
        public JObject Checks { get; set; }

        /// <summary>
        /// Gets or sets the premium.
        /// </summary>
        [JsonProperty(PropertyName = "premium", Order = 4)]
        public JObject Premium { get; set; }

        /// <summary>
        /// Gets or sets the statutory rates.
        /// </summary>
        [JsonProperty(PropertyName = "statutoryRates", Order = 5)]
        public StatutoryRates StatutoryRates { get; set; }

        /// <summary>
        /// Gets or sets the payment.
        /// </summary>
        [JsonProperty(PropertyName = "payment", Order = 6)]
        public JObject Payment { get; set; }

        /// <summary>
        /// Gets or sets the other.
        /// </summary>
        [JsonProperty(PropertyName = "other", Order = 7)]
        public JObject Other { get; set; }

        /// <summary>
        /// Fill up others section with properties that are not used here yet.
        /// </summary>
        private void PopulateUnusedPropertyIntoOtherSection(JObject risk)
        {
            // Iterate through all properties of the JObject
            if (risk == null)
            {
                return;
            }

            this.Other = this.Other ?? new JObject();
            var propertyNames = risk.Properties()
                .Where(x => !this.usedPropertyNames.Contains(x.Name))
                .Select(x => x.Name).ToList();
            foreach (var propertyName in propertyNames)
            {
                this.Other[propertyName] = risk[propertyName];
            }
        }

        private JObject GetChildObject(JObject parent, string childName)
        {
            return parent.SelectToken(childName) as JObject;
        }
    }
}
