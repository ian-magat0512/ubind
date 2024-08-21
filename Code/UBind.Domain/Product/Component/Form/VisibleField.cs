// <copyright file="VisibleField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents configuration associated with a field which renders a visible output.
    /// </summary>
    public abstract class VisibleField : Field
    {
        /// <summary>
        /// Gets or sets validation rules for the field.
        /// </summary>
        [WorkbookTableColumnName("Validation Rules")]
        public string ValidationRules { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field should start a new row when being rendered.
        /// </summary>
        [WorkbookTableColumnName("Row")]
        public bool? StartsNewRow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field should start a new reveal group.
        /// If a question set contains reveal groups it will incrementally reveal them as each reveal
        /// group becomes valid.
        /// </summary>
        [WorkbookTableColumnName("Reveal")]
        public bool? StartsNewRevealGroup { get; set; }

        /// <summary>
        /// Gets or sets the number of Bootstrap columns the field should take up when the screen width is
        /// Extra Small.
        /// </summary>
        [WorkbookTableColumnName("X")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? BootstrapColumnsExtraSmall { get; set; }

        /// <summary>
        /// Gets or sets the number of Bootstrap columns the field should take up when the screen width is
        /// Small.
        /// </summary>
        [WorkbookTableColumnName("S")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? BootstrapColumnsSmall { get; set; }

        /// <summary>
        /// Gets or sets the number of Bootstrap columns the field should take up when the screen width is
        /// Medium.
        /// </summary>
        [WorkbookTableColumnName("M")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? BootstrapColumnsMedium { get; set; }

        /// <summary>
        /// Gets or sets the number of Bootstrap columns the field should take up when the screen width is
        /// Large.
        /// </summary>
        [WorkbookTableColumnName("L")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? BootstrapColumnsLarge { get; set; }

        /// <summary>
        /// Gets or sets a value for the width to be applied to the widget via css, e.g "250px" or "100%".
        /// </summary>
        [WorkbookTableColumnName("Width")]
        public string WidgetCssWidth { get; set; }

        /// <summary>
        /// Gets or sets the label to be rendered above the field.
        /// </summary>
        [WorkbookTableColumnName("Label")]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the question to be rendered before the field.
        /// </summary>
        [WorkbookTableColumnName("Question")]
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the value for a level 2 heading to be rendered above the field.
        /// </summary>
        [WorkbookTableColumnName("Heading 2")]
        public string Heading2 { get; set; }

        /// <summary>
        /// Gets or sets the value for a level 3 heading to be rendered above the field.
        /// </summary>
        [WorkbookTableColumnName("Heading 3")]
        public string Heading3 { get; set; }

        /// <summary>
        /// Gets or sets the value for a level 4 heading to be rendered above the field.
        /// </summary>
        [WorkbookTableColumnName("Heading 4")]
        public string Heading4 { get; set; }

        /// <summary>
        /// Gets or sets the value for a paragraph of text to be rendered above the field.
        /// </summary>
        [WorkbookTableColumnName("Paragraph")]
        public string Paragraph { get; set; }

        /// <summary>
        /// Gets or sets the value for html to be rendered above the field.
        /// </summary>
        [WorkbookTableColumnName("HTML")]
        public string Html { get; set; }

        /// <summary>
        /// Gets or sets the value for html to be rendered above the field for the purpose of rendering
        /// terms &amp; conditions.
        /// </summary>
        [WorkbookTableColumnName("Terms")]
        public string HtmlTermsAndConditions { get; set; }

        /// <summary>
        /// Gets or sets text or HTML to be displayed in a popup box, which is revealed when someone clicks the help icon next
        /// to the field.
        /// </summary>
        [WorkbookTableColumnName("Tooltip")]
        public string Tooltip { get; set; }

        /// <summary>
        /// Gets or sets text which would be displayed under the field to provide guidance to how it should be filled.
        /// </summary>
        [WorkbookTableColumnName("Help Message")]
        public string HelpMessage { get; set; }

        /// <summary>
        /// Gets or sets a CSS class name to be added to the container of a field, to allow the field to be styled with
        /// CSS.
        /// </summary>
        [WorkbookTableColumnName("Container Class")]
        public string ContainerClass { get; set; }

        /// <summary>
        /// Gets or sets some CSS styles to be set directly on the container of a field, to adjust the style or layout
        /// of the field.
        /// </summary>
        [WorkbookTableColumnName("Container Style")]
        public string ContainerCss { get; set; }

        /// <summary>
        /// Gets or sets the label for this field's value when shown in the summary table of the calculation widget.
        /// </summary>
        [WorkbookTableColumnName("Summary Label")]
        public string SummaryLabel { get; set; }

        /// <summary>
        /// Gets or sets a UBind expression which evaluates to a numeric value which determines this fields
        /// order within the summary table of the calculation widget.
        /// </summary>
        [WorkbookTableColumnName("Summary Position Expression")]
        public string SummaryPositionExpression { get; set; }
    }
}
