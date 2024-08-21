// <copyright file="FormsAppCalculation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is needed to define and instantiate the base properties of a calculations object that conforms with serialized-entity.schema.json file.
    /// </summary>
    public class FormsAppCalculation : ISchemaObject
    {
        /// <summary>
        /// Gets or sets the calculation state.
        /// </summary>
        [JsonProperty(PropertyName = "state", Order = 1)]
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the calculation triggers.
        /// </summary>
        [JsonProperty(PropertyName = "triggers", Order = 3)]
        public JObject Triggers { get; set; }
    }
}
