// <copyright file="IInputFieldMask.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents the configuration of a field which can have an input mask
    /// e.g. SingleLineText, Currency, and SearchSelect.
    /// </summary>
    public interface IInputFieldMask
    {
        /// <summary>
        /// Gets or sets the input mask of the field.
        /// </summary>
        InputFieldMaskSetting InputMask { get; set; }

        /// <summary>
        /// Gets or sets the input mask list of the field.
        /// </summary>
        List<InputFieldMaskList> InputMaskList { get; set; }
    }
}
