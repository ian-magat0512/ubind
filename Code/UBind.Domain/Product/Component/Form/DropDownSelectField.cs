// <copyright file="DropDownSelectField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the configuration for a field which when clicked, drops down or reveals a fixed set of options.
    /// </summary>
    [WorkbookFieldType("Select")]
    [JsonFieldType("drop-down-select")]
    public class DropDownSelectField : OptionsField, ILineInputField
    {
        /// <summary>
        /// Gets or sets the icon to display on the left side of the field.
        /// This is a css class or a number of css classes, e.g. "fa fa-phone".
        /// </summary>
        [JsonProperty]
        public string IconLeft { get; set; }

        /// <summary>
        /// Gets or sets the icon to display on the right side of the field.
        /// This is a css class or a number of css classes, e.g. "fa fa-phone".
        /// </summary>
        [JsonProperty]
        public string IconRight { get; set; }

        /// <summary>
        /// Gets or sets the text to display on the left side of the field.
        /// </summary>
        [JsonProperty]
        public string TextLeft { get; set; }

        /// <summary>
        /// Gets or sets the text to display on the right side of the field.
        /// </summary>
        [JsonProperty]
        public string TextRight { get; set; }

        /// <summary>
        /// Gets or sets the keyboard input mode of the field.
        /// </summary>
        public string KeyboardInputMode { get; set; }
    }
}
