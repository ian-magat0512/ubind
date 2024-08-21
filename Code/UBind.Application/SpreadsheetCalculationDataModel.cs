// <copyright file="SpreadsheetCalculationDataModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Form data and calculation spreadsheet input for an application POSTed from client.
    /// </summary>
    public class SpreadsheetCalculationDataModel : CalculationDataModel
    {
        /// <summary>
        /// Gets or sets question answers for spreadsheet.
        /// </summary>
        [JsonProperty(PropertyName = "questionAnswers")]
        public JArray QuestionAnswers { get; set; }

        /// <summary>
        /// Gets or sets repeating question answers for spreadsheet.
        /// </summary>
        [JsonProperty(PropertyName = "repeatingQuestionAnswers")]
        public JArray RepeatingQuestionAnswers { get; set; }
    }
}
