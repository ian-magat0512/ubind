// <copyright file="InputFieldMaskSetting.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    /// <summary>
    /// Represents an input mask setting.
    /// </summary>
    public class InputFieldMaskSetting
    {
        /// <summary>
        /// Gets or sets the Input mask type.
        /// </summary>
        /// <remarks>default value is pattern.</remarks>
        public string Type { get; set; } = "pattern";

        /// <summary>
        /// Gets or sets the Input mask pattern.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets the Input mask decimal precision.
        /// </summary>
        public int? DecimalPrecision { get; set; }

        /// <summary>
        /// Gets or sets the Input mask pre decimal digit count limit.
        /// </summary>
        public int? PreDecimalDigitCountLimit { get; set; }

        /// <summary>
        /// Gets or sets the Input mask thousand separator.
        /// </summary>
        /// <remarks>default value is ",".</remarks>
        public string ThousandSeparator { get; set; } = ",";

        /// <summary>
        /// Gets or sets a value indicating whether negative number value is allowed.
        /// </summary>
        /// <remarks>default value is false.</remarks>
        public bool AllowNegativeNumberValue { get; set; }

        /// <summary>
        /// Gets or sets the Input mask prefix.
        /// </summary>
        public string Prefix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Input mask suffix.
        /// </summary>
        public string Suffix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to hide the previx when input value is empty.
        /// </summary>
        /// <remarks>default value is false.</remarks>
        public bool HidePrefixWhenInputValueIsEmpty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove non input character from value.
        /// </summary>
        /// <remarks>default value is false.</remarks>
        public bool RemoveNonInputCharactersFromValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to hide the suffix when input value is empty.
        /// </summary>
        /// <remarks>default value is false.</remarks>
        public bool HideSuffixWhenInputValueIsEmpty { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include prefix in value.
        /// </summary>
        public bool IncludePrefixInValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include suffix in value.
        /// </summary>
        public bool IncludeSuffixInValue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to remove only specific non input character from value.
        /// </summary>
        public string RemoveOnlySpecificNonInputCharactersFromValue { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether zero will be padded for date and time field.
        /// </summary>
        public bool PadDateAndTimeValuesWithLeadingZeros { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the pattern as place holder.
        /// </summary>
        public bool DisplayPatternAsPlaceholder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether type over place holder pattern is used.
        /// </summary>
        public bool TypeOverPlaceholderPattern { get; set; }

        /// <summary>
        /// Gets or sets the Input mask place holder pattern input character.
        /// </summary>
        public string PlaceholderPatternInputCharacter { get; set; } = "_";
    }
}
