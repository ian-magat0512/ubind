// <copyright file="Style.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the configuration of a style, which is used to apply css to form elements.
    /// </summary>
    public class Style
    {
        /// <summary>
        /// Gets or sets a category for which this style belongs. Used for organisational purposes only.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets a name for this style. Used for organisational purposes only.
        /// </summary>
        [WorkbookTableColumnName("Element")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a wrapper for the selector.
        /// This can be used to apply a media query to the style.
        /// </summary>
        [WorkbookTableColumnName("Wrapper")]
        [PopulateWhenEmpty(false)]
        public string Wrapper { get; set; }

        /// <summary>
        /// Gets or sets the css selector.
        /// </summary>
        [Required]
        [WorkbookTableColumnName("Selector")]
        public string Selector { get; set; }

#pragma warning disable SA1600 // Elements should be documented
        [WorkbookTableColumnName("Font Family Used")]
        [PopulateWhenEmpty(false)]
        public string FontFamily { get; set; }

        [WorkbookTableColumnName("Font Size Used")]
        [PopulateWhenEmpty(false)]
        public string FontSize { get; set; }

        [WorkbookTableColumnName("Font Weight Used")]
        [PopulateWhenEmpty(false)]
        public string FontWeight { get; set; }

        [WorkbookTableColumnName("Colour Used")]
        [PopulateWhenEmpty(false)]
        public string Colour { get; set; }

        [WorkbookTableColumnName("Background Used")]
        [PopulateWhenEmpty(false)]
        public string Background { get; set; }

        [WorkbookTableColumnName("Border Used")]
        [PopulateWhenEmpty(false)]
        public string Border { get; set; }

        [WorkbookTableColumnName("Border Radius Used")]
        [PopulateWhenEmpty(false)]
        public string BorderRadius { get; set; }

        [WorkbookTableColumnName("MT Used")]
        [PopulateWhenEmpty(false)]
        public string MarginTop { get; set; }

        [WorkbookTableColumnName("MR Used")]
        [PopulateWhenEmpty(false)]
        public string MarginRight { get; set; }

        [WorkbookTableColumnName("MB Used")]
        [PopulateWhenEmpty(false)]
        public string MarginBottom { get; set; }

        [WorkbookTableColumnName("ML Used")]
        [PopulateWhenEmpty(false)]
        public string MarginLeft { get; set; }

        [WorkbookTableColumnName("PT Used")]
        [PopulateWhenEmpty(false)]
        public string PaddingTop { get; set; }

        [WorkbookTableColumnName("PR Used")]
        [PopulateWhenEmpty(false)]
        public string PaddingRight { get; set; }

        [WorkbookTableColumnName("PB Used")]
        [PopulateWhenEmpty(false)]
        public string PaddingBottom { get; set; }

        [WorkbookTableColumnName("PL Used")]
        [PopulateWhenEmpty(false)]
        public string PaddingLeft { get; set; }

        [WorkbookTableColumnName("BT Used")]
        [PopulateWhenEmpty(false)]
        public string BorderTop { get; set; }

        [WorkbookTableColumnName("BR Used")]
        [PopulateWhenEmpty(false)]
        public string BorderRight { get; set; }

        [WorkbookTableColumnName("BB Used")]
        [PopulateWhenEmpty(false)]
        public string BorderBottom { get; set; }

        [PopulateWhenEmpty(false)]
        [WorkbookTableColumnName("BL Used")]
        public string BorderLeft { get; set; }

        [WorkbookTableColumnName("Width Used")]
        [PopulateWhenEmpty(false)]
        public string Width { get; set; }

        [WorkbookTableColumnName("Height Used")]
        [PopulateWhenEmpty(false)]
        public string Height { get; set; }

        [WorkbookTableColumnName("Top Used")]
        [PopulateWhenEmpty(false)]
        public string Top { get; set; }

        [WorkbookTableColumnName("Right Used")]
        [PopulateWhenEmpty(false)]
        public string Right { get; set; }

        [WorkbookTableColumnName("Bottom Used")]
        [PopulateWhenEmpty(false)]
        public string Bottom { get; set; }

        [WorkbookTableColumnName("Left Used")]
        [PopulateWhenEmpty(false)]
        public string Left { get; set; }

        [WorkbookTableColumnName("Custom")]
        [PopulateWhenEmpty(false)]
        public string CustomCss { get; set; }

#pragma warning restore SA1600 // Elements should be documented
    }
}
