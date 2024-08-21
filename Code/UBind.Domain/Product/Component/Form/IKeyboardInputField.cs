// <copyright file="IKeyboardInputField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    /// <summary>
    /// Represents the configuration of a field which can have keyboard input mode
    /// e.g. SingleLineText, Currency, SearchSelect, PasswordField, and TextAreaField.
    /// </summary>
    public interface IKeyboardInputField
    {
        /// <summary>
        /// Gets or sets the keyboard input mode of the field.
        /// </summary>
        string KeyboardInputMode { get; set; }
    }
}
