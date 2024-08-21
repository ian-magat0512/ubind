// <copyright file="IframeField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents the configuration for a field which will render an Iframe.
    /// </summary>
    [WorkbookFieldType("Iframe")]
    [JsonFieldType("iframe")]
    public class IframeField : VisibleField
    {
        /// <summary>
        /// Gets or sets an expression which evaluates to the URL the iframe should load.
        /// </summary>
        [Required]
        [WorkbookTableColumnName("Placeholder")]
        public string UrlExpression { get; set; }
    }
}
