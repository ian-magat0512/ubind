// <copyright file="ILineInputField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    /// <summary>
    /// Represents the configuration of a field which has a line of input.
    /// e.g. SingleLineText, Currency, DropDownSelect, SearchSelect.
    /// </summary>
    public interface ILineInputField
    {
        /// <summary>
        /// Gets or sets the icon to display on the left side of the field.
        /// This is a css class or a number of css classes, e.g. "fa fa-phone".
        /// </summary>
        string IconLeft { get; set; }

        /// <summary>
        /// Gets or sets the icon to display on the right side of the field.
        /// This is a css class or a number of css classes, e.g. "fa fa-phone".
        /// </summary>
        string IconRight { get; set; }

        /// <summary>
        /// Gets or sets the text to display on the left side of the field.
        /// </summary>
        string TextLeft { get; set; }

        /// <summary>
        /// Gets or sets the text to display on the right side of the field.
        /// </summary>
        string TextRight { get; set; }
    }
}
