// <copyright file="AfterQuoteCalculationExtensionResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This is the After quote calculation extension result model for returning data.
    /// </summary>
    public class AfterQuoteCalculationExtensionResultModel
    {
        public AfterQuoteCalculationExtensionResultModel(JObject formModel, JObject calculationResult)
        {
            this.FormModel = formModel;
            this.CalculationResult = calculationResult;
        }

        public JObject FormModel { get; }

        public JObject CalculationResult { get; }
    }
}
