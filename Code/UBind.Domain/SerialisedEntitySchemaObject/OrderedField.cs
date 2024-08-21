// <copyright file="OrderedField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.SerialisedEntitySchemaObject
{
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This class represents the base model for all ordered fields for use of automations.
    /// </summary>
    public abstract class OrderedField : ISchemaObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderedField"/> class.
        /// </summary>
        protected OrderedField()
        {
        }

        /// <summary>
        /// Gets or sets the label of the field.
        /// </summary>
        [JsonProperty("label", Order = 5)]
        public string Label { get; protected set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is the default value for the context.
        /// </summary>
        [JsonProperty("default", Order = 6, NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// Sets the value of label for output depending on value of retrieved from model.
        /// </summary>
        /// <param name="label">The label from the model.</param>
        /// <param name="customLabel">The custom label, if any.</param>
        public void SetLabel(string label, string customLabel = default)
        {
            string customLabelKey = "other";
            this.Label =
                label.Equals(customLabelKey, System.StringComparison.InvariantCultureIgnoreCase) ?
                customLabel.ToTitleCase() :
                label.ToTitleCase();
        }
    }
}
