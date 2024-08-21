// <copyright file="CalculationResultDetailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Product.Component;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Resource model for serving calculation details for an application.
    /// </summary>
    public class CalculationResultDetailModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationResultDetailModel"/> class.
        /// </summary>
        /// <param name="calculationResultId">The ID of the calculation result.</param>
        /// <param name="formDataId">The ID of the form data used in the calculation.</param>
        /// <param name="calculationResultJson">The calculation result json.</param>
        public CalculationResultDetailModel(
            Guid calculationResultId,
            Guid formDataId,
            string calculationResultJson,
            CalculationResult calculationResult = null,
            List<Trigger> triggerConfig = null)
        {
            this.CalculationResultId = calculationResultId;
            this.FormDataId = formDataId;

            // Deserialize and re-serialize the json to fix badness (e.g. trailing commas).
            this.CalculationResultJson = calculationResultJson != null ? JsonConvert.SerializeObject(JObject.Parse(calculationResultJson)) : string.Empty;
            this.DetermineTriggerTypeWhichRequiresPriceToBeHidden(calculationResult, triggerConfig);
        }

        /// <summary>
        /// Gets or sets the ID of the Calculation Result obj.
        /// </summary>
        public Guid CalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the Form Data.
        /// </summary>
        public Guid FormDataId { get; set; }

        /// <summary>
        /// Gets or sets the JSON object for calculation result.
        /// </summary>
        public string CalculationResultJson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type of trigger which requires the price to be hidden.
        /// </summary>
        public string TriggerTypeWhichRequiresPriceToBeHidden { get; protected set; }

        private void DetermineTriggerTypeWhichRequiresPriceToBeHidden(CalculationResult calculationResult, List<Trigger> triggerConfig)
        {
            string activeTriggerType = this.GetActiveCalculationTriggerType(calculationResult);
            if (string.IsNullOrEmpty(activeTriggerType) || triggerConfig == null)
            {
                return;
            }

            var triggers = calculationResult?.JObject?.SelectTokens($"$.triggers.{activeTriggerType}").First();
            if (triggers == null)
            {
                return;
            }

            foreach (JProperty trigger in triggers)
            {
                if ((bool)trigger.Value &&
                    triggerConfig.Any(config => config.Type.ToLower() == activeTriggerType &&
                                                config.Key == trigger.Name &&
                                                config.DisplayPrice == false))
                {
                    this.TriggerTypeWhichRequiresPriceToBeHidden = activeTriggerType;
                    return;
                }
            }
        }

        private string GetActiveCalculationTriggerType(CalculationResult? calculationResult)
        {
            if (calculationResult == null)
            {
                return string.Empty;
            }
            else if (calculationResult.HasDeclinedReferralTriggers)
            {
                return "decline";
            }
            else if (calculationResult.HasReviewCalculationTriggers)
            {
                return "review";
            }
            else if (calculationResult.HasEndorsementTriggers)
            {
                return "endorsement";
            }
            return string.Empty;
        }
    }
}
