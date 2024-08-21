// <copyright file="GoogleFont.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Configuration for a google font.
    /// </summary>
    public class GoogleFont
    {
        /// <summary>
        /// Gets or sets where or what the font is used for.
        /// </summary>
        public string Usage { get; set; }

        /// <summary>
        /// Gets or sets the name of the font family.
        /// </summary>
        [Required]
        public string Family { get; set; }

        /// <summary>
        /// Gets or sets the weight of the font.
        /// </summary>
        [Required]
        public string Weight { get; set; }
    }
}
