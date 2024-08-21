// <copyright file="TextElement.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents some text with an identifier which is referenced and rendered by the form.
    /// </summary>
    public class TextElement
    {
        /// <summary>
        /// Gets or sets the category which the text element sits in.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the category which the text element sits in.
        /// </summary>
        public string Subcategory { get; set; }

        /// <summary>
        /// Gets or sets the name of the text element.
        /// </summary>
        [Required]
        [WorkbookTableColumnName("Property")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the text itself.
        /// </summary>
        [WorkbookTableColumnName("Text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets an icon identifier for an icon to be displayed alongside the text.
        /// This used for text elements that appear on buttons.
        /// </summary>
        [WorkbookTableColumnName("Icon")]
        public string Icon { get; set; }
    }
}
