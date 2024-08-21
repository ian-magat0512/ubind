// <copyright file="OptionsField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the configuration for fields which allows a user to select from options.
    /// </summary>
    public abstract class OptionsField : InteractiveField
    {
        /// <summary>
        /// Gets or sets the option set.
        /// </summary>
        [JsonIgnore]
        public OptionSet OptionSet { get; set; }

        /// <summary>
        /// Gets the name of the option set, if a fixed option set is to be used.
        /// </summary>
        public string OptionSetName => this.OptionSet?.Name;

        /// <summary>
        /// Gets or sets the key of the option set.
        /// </summary>
        public string OptionSetKey { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the request which is to load the options from the API.
        /// </summary>
        public OptionsRequest OptionsRequest { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the request which will load a single option from the API.
        /// </summary>
        public SelectedOptionRequest SelectedOptionRequest { get; set; }

        /// <summary>
        /// Gets or sets a uBind expression which evaluates to a value which is used to filter the list to
        /// only options that have matching keywords.
        /// You could use either the same field or another field's value as the input filter to the options list.
        /// </summary>
        [Obsolete("Use SearchTextExpression instead")]
        public string FilterOptionsByExpression { get; set; }

        /// <summary>
        /// Gets or sets a uBind expression which evaluates to a value which is used to filter the list to
        /// only options that have matching keywords.
        /// You could use either the same field or another field's value as the input filter to the options list.
        /// </summary>
        public string SearchTextExpression { get; set; }

        /// <summary>
        /// Gets or sets a uBind expression which if it evaluates to true, options will not be shown.
        /// If not set, options will always be shown.
        /// This can be useful for an options field with a lot of options, e.g. more than 50. That could be too many to
        /// show at once, so we only want to show them once someone starts typing/filtering.
        /// </summary>
        [Obsolete("Use HideAllOptionsConditionExpression instead")]
        public string HideOptionsConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a uBind expression which if it evaluates to true, options will not be shown.
        /// If not set, options will always be shown.
        /// This can be useful for an options field with a lot of options, e.g. more than 50. That could be too many to
        /// show at once, so we only want to show them once someone starts typing/filtering.
        /// </summary>
        public string HideAllOptionsConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets a uBind expression which is evaluated for each option, and if it returns true, that option
        /// is hidden.
        /// </summary>
        public string OptionHideConditionExpression { get; set; }

        /// <summary>
        /// Gets or sets the text to show when no options are found.
        /// The message can contain embedded expressions using the {% %} syntax.
        /// </summary>
        public string NoOptionsFoundText { get; set; }
    }
}
