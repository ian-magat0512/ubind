// <copyright file="PremiumBreakdown.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is needed because we need to generate json representation of price breakdown object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class PremiumBreakdown : ISchemaObject
    {
        private static CultureInfo culture = CultureInfo.CurrentUICulture;
        private static NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="PremiumBreakdown"/> class.
        /// </summary>
        /// <param name="priceObject">The price object.</param>
        public PremiumBreakdown(JObject priceObject)
        {
            if (priceObject != null && priceObject.Type != JTokenType.Null)
            {
                var properties = new Dictionary<string, string>(priceObject.ToObject<IDictionary<string, string>>(), StringComparer.CurrentCultureIgnoreCase);
                this.BasePremium = this.GetPropertyValue(properties, "basePremium") ?? 0.00;
                this.TerrorismPremium = this.GetPropertyValue(properties, "terrorismPremium") ?? 0.00;
                this.EslNsw = this.GetPropertyValue(properties, "emergencyServicesLevyNsw") ?? 0.00;
                this.EslTas = this.GetPropertyValue(properties, "emergencyServicesLevyTas") ?? 0.00;
                this.Esl = this.GetPropertyValue(properties, "esl") ?? 0.00;
                this.PremiumGst = this.GetPropertyValue(properties, "premiumGst") ?? 0.00;
                this.StampDutyAct = this.GetPropertyValue(properties, "stampDutyAct") ?? 0.00;
                this.StampDutyNsw = this.GetPropertyValue(properties, "stampDutyNsw") ?? 0.00;
                this.StampDutyNt = this.GetPropertyValue(properties, "stampDutyNt") ?? 0.00;
                this.StampDutyQld = this.GetPropertyValue(properties, "stampDutyQld") ?? 0.00;
                this.StampDutySa = this.GetPropertyValue(properties, "stampDutySa") ?? 0.00;
                this.StampDutyTas = this.GetPropertyValue(properties, "stampDutyTas") ?? 0.00;
                this.StampDutyVic = this.GetPropertyValue(properties, "stampDutyVic") ?? 0.00;
                this.StampDutyWa = this.GetPropertyValue(properties, "stampDutyWa") ?? 0.00;
                this.StampDutyTotal = this.GetPropertyValue(properties, "stampDutyTotal") ?? 0.00;
                this.TotalPremium = this.GetPropertyValue(properties, "totalPremium") ?? 0.00;
            }
        }

        /// <summary>
        /// Gets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "basePremium", Order = 1)]
        [Required]
        public double BasePremium { get; protected set; }

        /// <summary>
        /// Gets the terrorism premium
        /// </summary>
        [JsonProperty("terrorismPremium", Order = 2)]
        public double TerrorismPremium { get; protected set; }

        /// <summary>
        /// Gets the esl for NSW, if any.
        /// </summary>
        [JsonProperty("emergencyServicesLevyNsw", Order = 3)]
        public double EslNsw { get; protected set; }

        /// <summary>
        /// Gets the esl for TAS, if any.
        /// </summary>
        [JsonProperty("emergencyServicesLevyTas", Order = 4)]
        public double EslTas { get; protected set; }

        /// <summary>
        /// Gets the esl.
        /// </summary>
        [JsonProperty(PropertyName = "emergencyServicesLevyTotal", Order = 5)]
        public double Esl { get; protected set; }

        /// <summary>
        /// Gets the premium Gst.
        /// </summary>
        [JsonProperty(PropertyName = "premiumGst", Order = 6)]
        [Required]
        public double PremiumGst { get; protected set; }

        /// <summary>
        /// Gets the stamp duty ACT.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyAct", Order = 7)]
        public double StampDutyAct { get; protected set; }

        /// <summary>
        /// Gets the stamp duty NSW.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyNsw", Order = 8)]
        public double StampDutyNsw { get; protected set; }

        /// <summary>
        /// Gets the stamp duty NT.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyNt", Order = 9)]
        public double StampDutyNt { get; protected set; }

        /// <summary>
        /// Gets the stamp duty QLD.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyQld", Order = 10)]
        public double StampDutyQld { get; protected set; }

        /// <summary>
        /// Gets the stamp duty SA.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutySa", Order = 11)]
        public double StampDutySa { get; protected set; }

        /// <summary>
        /// Gets the stamp duty TAS.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyTas", Order = 12)]
        public double StampDutyTas { get; protected set; }

        /// <summary>
        /// Gets the stamp duty VIC.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyVic", Order = 13)]
        public double StampDutyVic { get; protected set; }

        /// <summary>
        /// Gets the stamp duty WA.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyWa", Order = 14)]
        public double StampDutyWa { get; protected set; }

        /// <summary>
        /// Gets the stamp duty total.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyTotal", Order = 15)]
        public double StampDutyTotal { get; protected set; }

        /// <summary>
        /// Gets the total premium.
        /// </summary>
        [JsonProperty(PropertyName = "totalPremium", Order = 16)]
        [Required]
        public double TotalPremium { get; protected set; }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="priceObject">The price object.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The property value.</returns>
        protected double? GetPropertyValue(IDictionary<string, string> priceObject, string propertyName)
        {
            if (!priceObject.ContainsKey(propertyName) && !priceObject.ContainsKey(propertyName.ToUpper()))
            {
                return null;
            }

            if (double.TryParse(priceObject[propertyName], style, culture, out double value))
            {
                return value;
            }

            return null;
        }
    }
}
