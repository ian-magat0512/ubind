// <copyright file="SearchSelectField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the configuration for a field which allows you to search through
    /// the options by typing, and drops down matches for selection.
    /// </summary>
    [WorkbookFieldType("Search Select")]
    [JsonFieldType("search-select")]
    public class SearchSelectField : DropDownSelectField, IInputFieldMask
    {
        /// <summary>
        /// Gets or sets the input mask of the field.
        /// </summary>
        public InputFieldMaskSetting InputMask { get; set; }

        /// <summary>
        /// Gets or sets the input mask list of the field.
        /// </summary>
        public List<InputFieldMaskList> InputMaskList { get; set; }
    }
}
