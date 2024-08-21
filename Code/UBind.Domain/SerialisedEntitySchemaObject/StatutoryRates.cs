// <copyright file="StatutoryRates.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class is needed because we need to generate json representation of statutory rates object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class StatutoryRates : ISchemaObject
    {
        private static CultureInfo culture = CultureInfo.CurrentUICulture;
        private static NumberStyles style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint;

        [JsonConstructor]
        public StatutoryRates()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StatutoryRates"/> class.
        /// </summary>
        /// <param name="statutoryRate">The statutory rate object.</param>
        public StatutoryRates(JObject statutoryRate)
        {
            this.RiskType = statutoryRate.SelectToken("riskType")?.ToString().RemoveWhitespace().ToCamelCase();
            this.IsEslApplicable = statutoryRate.SelectToken("ESLApplicability")?.ToString().ToLowerInvariant() == "not applicable" ? false : true;
            this.EslRate = this.GetPropertyValue(statutoryRate, "ESLRate");
            this.GstRate = this.GetPropertyValue(statutoryRate, "GSTRate");
            this.StampDutyRateAct = this.GetPropertyValue(statutoryRate, "SDRateACT");
            this.StampDutyRateNsw = this.GetPropertyValue(statutoryRate, "SDRateNSW");
            this.StampDutyRateNt = this.GetPropertyValue(statutoryRate, "SDRateNT");
            this.StampDutyRateQld = this.GetPropertyValue(statutoryRate, "SDRateQLD");
            this.StampDutyRateSa = this.GetPropertyValue(statutoryRate, "SDRateSA");
            this.StampDutyRateTas = this.GetPropertyValue(statutoryRate, "SDRateTAS");
            this.StampDutyRateVic = this.GetPropertyValue(statutoryRate, "SDRateVIC");
            this.StampDutyRateWa = this.GetPropertyValue(statutoryRate, "SDRateWA");
        }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "riskType", Order = 1)]
        public string RiskType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "eslApplicable", Order = 2)]
        public bool IsEslApplicable { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "eslRate", Order = 3)]
        public double EslRate { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "gstRate", Order = 4)]
        public double GstRate { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyRateAct", Order = 5)]
        public double StampDutyRateAct { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyRateNsw", Order = 6)]
        public double StampDutyRateNsw { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyRateNt", Order = 7)]
        public double StampDutyRateNt { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyRateQld", Order = 8)]
        public double StampDutyRateQld { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyRateSa", Order = 9)]
        public double StampDutyRateSa { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyRateTas", Order = 10)]
        public double StampDutyRateTas { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyRateVic", Order = 11)]
        public double StampDutyRateVic { get; set; }

        /// <summary>
        /// Gets or sets the base premium.
        /// </summary>
        [JsonProperty(PropertyName = "stampDutyRateWa", Order = 12)]
        public double StampDutyRateWa { get; set; }

        private double GetPropertyValue(JObject priceObject, string propertyName1)
        {
            var propertyValue = priceObject.SelectToken(propertyName1)?.ToString();
            if (!string.IsNullOrWhiteSpace(propertyValue))
            {
                if (double.TryParse(propertyValue, style, culture, out double value))
                {
                    return value;
                }
            }

            return 0.00;
        }
    }
}
