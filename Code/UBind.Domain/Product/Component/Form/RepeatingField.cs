// <copyright file="RepeatingField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using Newtonsoft.Json;
    using UBind.Domain.Extensions;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// Represents the configuration for a field which allows a defined repeating question set
    /// to render itself one or more times, so as to collect a list of data objects.
    /// </summary>
    [WorkbookFieldType("List")]
    [JsonFieldType("repeating")]
    public class RepeatingField : VisibleField
    {
        /// <summary>
        /// Gets or sets a value for an expression which evaluates to the minimum number of instances of the repeating
        /// question set that will be rendered.
        /// </summary>
        [WorkbookTableColumnName("Min Qty")]
        public string MinimumQuantityExpression { get; set; }

        /// <summary>
        /// Gets or sets a value for an expression which evaluates to the maximum number of instances of the repeating
        /// question set that will be allowed to be rendered.
        /// </summary>
        [WorkbookTableColumnName("Max Qty")]
        public string MaximumQuantityExpression { get; set; }

        /// <summary>
        /// Gets or sets the question set which is to be repeated.
        /// </summary>
        [JsonIgnore]
        public RepeatingQuestionSet QuestionSetToRepeat { get; set; }

        /// <summary>
        /// Gets the name of the question set to repeat.
        /// </summary>
        public string QuestionSetNameToRepeat => this.QuestionSetToRepeat.Name;

        /// <summary>
        /// Gets the key of the question set to repeat.
        /// </summary>
        public string QuestionSetKeyToRepeat => this.QuestionSetNameToRepeat.ToCamelCase();

        /// <summary>
        /// Gets or sets a value indicating whether to render the repeating instance heading inside each repeating
        /// instance. Defaults to true.
        /// </summary>
        public bool? DisplayRepeatingInstanceHeading { get; set; }

        /// <summary>
        /// Gets or sets the heading to be displayed inside each repeating instance.
        /// This can contain expressions, e.g. "Claim %{ getRepeatingIndex(fieldPath) + 1 }%".
        /// </summary>
        public string RepeatingInstanceHeading { get; set; }

        /// <summary>
        /// Gets or sets the name of the repeating instance which is used as the heading for each repeating instance
        /// and also appears on the button label.
        /// e.g. "Claim 1" and "Add another Claim".
        /// </summary>
        public string RepeatingInstanceName { get; set; }

        /// <summary>
        /// Gets or sets the heading level to use within each repeating instance, e.g. 4 = "h4", 3 = "h3".
        /// </summary>
        public int RepeatingInstanceHeadingLevel { get; set; }

        /// <summary>
        /// Gets or sets the label for the button to add a repeating instance.
        /// If AddFirstRepeatingInstanceButtonLabel is not defined, this label will be used for the first button,
        /// otherwise it will only be used for subsequent buttons.
        /// </summary>
        public string AddRepeatingInstanceButtonLabel { get; set; }

        /// <summary>
        /// Gets or sets the label for the button to add the first repeating instance, which is displayed when there
        /// are no repeating instances yet. If not set, it will just use the AddRepeatingInstanceButtonLabel.
        /// </summary>
        public string AddFirstRepeatingInstanceButtonLabel { get; set; }

        /// <summary>
        /// Gets or sets the label for the button to remove a repeating instance.
        /// </summary>
        public string RemoveRepeatingInstanceButtonLabel { get; set; }

        /// <summary>
        /// Gets or sets a value which specifies whether you want to display all or one instance at a time.
        /// </summary>
        [JsonConverter(typeof(StringEnumHumanizerJsonConverter))]
        public RepeatingFieldDisplayMode? DisplayMode { get; set; }

        /// <summary>
        /// Gets or sets an expression which evaluates to a RepeatingFieldDisplayMode.
        /// </summary>
        public string DisplayModeExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this field should be displayed to users in the portal.
        /// Defaults to true.
        /// </summary>
        [WorkbookTableColumnName("Displayable")]
        public bool? Displayable { get; set; }

        /// <summary>
        /// Gets or sets the location of the repeating field in the question set table.
        /// This was added for the repeating question set for the purpose of
        /// identifying the cell that will be updated into "complete" or "incomplete" text value.
        /// </summary>
        public WorkbookCellLocation? CalculationWorkbookCellLocation { get; set; }
    }
}
