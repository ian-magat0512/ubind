// <copyright file="CalculationDataTriggerModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel.CalculationTrigger
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// View model for calculation trigger.
    /// </summary>
    public class CalculationDataTriggerModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationDataTriggerModel"/> class.
        /// </summary>
        [JsonConstructor]
        public CalculationDataTriggerModel()
        {
            // Nothing to do
        }

        /// <summary>
        /// Gets or sets the base premium in the calculation result.
        /// </summary>
        [JsonProperty]
        public Dictionary<string, Dictionary<string, bool>> Triggers { get; set; }
    }
}
