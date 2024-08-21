// <copyright file="RiskPayment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    public class RiskPayment : PremiumBreakdown
    {
        public RiskPayment(JObject priceObject)
            : base(null)
        {
            if (priceObject != null)
            {
                var properties = new Dictionary<string, string>(
                    priceObject.ToObject<IDictionary<string, string>>(), StringComparer.CurrentCultureIgnoreCase);
                this.BasePremium = this.GetPropertyValue(properties, "premium") ?? 0.00;
                this.Esl = this.GetPropertyValue(properties, "ESL") ?? 0.00;
                this.PremiumGst = this.GetPropertyValue(properties, "GST") ?? 0.00;
                this.StampDutyAct = this.GetPropertyValue(properties, "SDACT") ?? 0.00;
                this.StampDutyNsw = this.GetPropertyValue(properties, "SDNSW") ?? 0.00;
                this.StampDutyNt = this.GetPropertyValue(properties, "SDNT") ?? 0.00;
                this.StampDutyQld = this.GetPropertyValue(properties, "SDQLD") ?? 0.00;
                this.StampDutySa = this.GetPropertyValue(properties, "SDSA") ?? 0.00;
                this.StampDutyTas = this.GetPropertyValue(properties, "SDTAS") ?? 0.00;
                this.StampDutyVic = this.GetPropertyValue(properties, "SDVIC") ?? 0.00;
                this.StampDutyWa = this.GetPropertyValue(properties, "SDWA") ?? 0.00;
                this.TotalPremium = this.GetPropertyValue(properties, "total") ?? 0.00;
            }
        }
    }
}
