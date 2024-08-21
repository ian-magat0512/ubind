// <copyright file="EncryptionItemValues.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Eway
{
    /// <summary>
    /// Model for instantiating item values for encryption.
    /// </summary>
    public class EncryptionItemValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionItemValues"/> class.
        /// </summary>
        /// <param name="name">The item name.</param>
        /// <param name="value">The item value.</param>
        public EncryptionItemValues(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the item name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the item value.
        /// </summary>
        public string Value { get; set; }
    }
}
