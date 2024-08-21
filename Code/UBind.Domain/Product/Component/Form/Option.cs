// <copyright file="Option.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents an option in a set of options, so is part of an OptionSet.
    /// </summary>
    public class Option
    {
        /// <summary>
        /// Gets or sets the label for the option, which is what is displayed to the user.
        /// </summary>
        [WorkbookTableColumnName("Display")]
        [WorkbookTableColumnName("Label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the value for the option.
        /// </summary>
        [Required]
        [WorkbookTableColumnName("Value")]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets an icon identifier for an icon to be displayed alongside or above the option.
        /// </summary>
        [WorkbookTableColumnName("Icon")]
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets a css class for the option.
        /// </summary>
        [WorkbookTableColumnName("Css Class")]
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets an expression which if evaluates to true, will cause the option to not be rendered or offered.
        /// </summary>
        [WorkbookTableColumnName("Hide Condition")]
        [WorkbookTableColumnName("Hide Condition Expression")]
        public string HideConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a UBind Expression which if evaluates to true, the field is disabled.
        /// </summary>
        [WorkbookTableColumnName("Disabled Condition")]
        [WorkbookTableColumnName("Disabled Condition Expression")]
        public string DisabledConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets text which is searchable so can be used when trying to match an option within a search select
        /// or other type of field which allows the user to search for a matching or similar option.
        /// </summary>
        [WorkbookTableColumnName("Searchable Text")]
        public string SearchableText { get; set; }

        /// <summary>
        /// Gets or sets custom properties assosciated with the option.
        /// </summary>
        [WorkbookTableColumnName("Properties JSON")]
        public JObject Properties { get; set; }
    }
}
