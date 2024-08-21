// <copyright file="ClaimCalculation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is needed because we need to generate json representation of claim calculation object that conforms with serialized-entity-schema.json file.
    /// </summary>
    public class ClaimCalculation : FormsAppCalculation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimCalculation"/> class.
        /// </summary>
        /// <param name="calculation">The calculation object.</param>
        public ClaimCalculation(JObject calculation)
        {
            if (calculation.Type != JTokenType.Null)
            {
                this.State = calculation.SelectToken("state")?.ToString();
                this.Triggers = JObject.Parse(!string.IsNullOrWhiteSpace(calculation.SelectToken("triggers")?.ToString()) ? calculation.SelectToken("triggers")?.ToString() : "{}");
            }
        }
    }
}
