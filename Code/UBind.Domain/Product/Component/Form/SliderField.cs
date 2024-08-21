// <copyright file="SliderField.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    /// <summary>
    /// Represents the configuration for a field which renders options, a range of values of
    /// which one can be selected.
    /// </summary>
    [WorkbookFieldType("Slider")]
    [JsonFieldType("slider")]
    public class SliderField : OptionsField
    {
        /// <summary>
        /// Gets or sets an expression which evaluates to the start value of the axis.
        /// If not set, defaults to using 0 for the axis start value.
        /// </summary>
        public string AxisStartValueExpression { get; set; }

        /// <summary>
        /// Gets or sets an expression which evaluates to the end value of the axis.
        /// If not set, defaults to using 100 for the axis end value.
        /// </summary>
        public string AxisEndValueExpression { get; set; }

        /// <summary>
        /// Gets or sets the lowest value on the access which the user can select.
        /// Defaults to the axis start value if not set.
        /// </summary>
        public string MinSelectableValueExpression { get; set; }

        /// <summary>
        /// Gets or sets the highest value on the access which the user can select.
        /// Defaults to the axis end value if not set.
        /// </summary>
        public string MaxSelectableValueExpression { get; set; }

        /// <summary>
        /// Gets or sets an expression which resolves to the defined interval at which steps should be selectable
        /// along the axis of the slider.
        /// </summary>
        public string StepIntervalExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tick marks should be rendered along the axis of the slider.
        /// Defaults to false.
        /// </summary>
        public bool? ShowTickMarks { get; set; }

        /// <summary>
        /// Gets or sets an expression which resolves to the defined interval at which tick marks should be
        /// rendered along the axis of the slider.
        /// </summary>
        public string TickMarkIntervalExpression { get; set; }

        /// <summary>
        /// Gets or sets an expression which resolves to an array of step indexes at which tick marks should be rendered
        /// along the axis of the slider.
        /// </summary>
        public string TickMarkStepIndexArrayExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value label should be shown with each tick mark on the slider axis.
        /// Defaults to false.
        /// </summary>
        public bool? ShowTickMarkValueLabels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value label should be shown at the start of the slider axis.
        /// The default value is true when there is no legend.
        /// </summary>
        public bool? ShowAxisStartValueLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value label should be shown at the end of the slider axis.
        /// The default value is true when there is no legend.
        /// </summary>
        public bool? ShowAxisEndValueLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value label should be shown at the thumb position of the slider axis.
        /// The default value is true.
        /// </summary>
        public bool? ShowThumbValueLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a set of value labels should be displayed in the legend position,
        /// below the slider axis.
        /// </summary>
        public bool? ShowLegend { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the slider bar should be coloured in from the start of the axis to
        /// the thumb position. Defaults to true.
        /// </summary>
        public bool? ShowSelectionBarFromAxisStart { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the slider bar should be coloured in from the thumb position to
        /// the end of the axis. Defaults to false.
        /// </summary>
        public bool? ShowSelectionBarFromAxisEnd { get; set; }

        /// <summary>
        /// Gets or sets an expression which evaluates the value starting at which the bar should be coloured,
        /// through to the thumb position.
        /// </summary>
        public string ShowSelectionBarFromValueExpression { get; set; }

        /// <summary>
        /// Gets or sets an expression to format the value when being rendered as a label above the slider axis and
        /// thumbs.
        /// </summary>
        public string FormatValueLabelExpression { get; set; }

        /// <summary>
        /// Gets or sets an expression to format the value that is rendered as a label above the thumb.
        /// thumbs.
        /// </summary>
        public string FormatThumbValueLabelExpression { get; set; }

        /// <summary>
        /// Gets or sets an expression to format the value that is rendered as part of the legend below the axis.
        /// </summary>
        public string FormatLegendItemExpression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the axis of the slider should be reversed.
        /// Defaults to false.
        /// </summary>
        public bool? InvertAxis { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the slider will be drawn vertically instead of horizontally.
        /// </summary>
        public bool? Vertical { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the slider will smoothly move along the axis, or jump from
        /// one option to the next.
        /// </summary>
        public bool? Continuous { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the thumb should snap to the nearest step when the control is
        /// released by the user. This is relevant for continuous sliders. Defaults to true.
        /// </summary>
        public bool? SnapToStep { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether labels can be rotated diagonally or vertically to fit more.
        /// </summary>
        public bool? RotateLabelsToFit { get; set; }

        public SliderStylingSettings Styling { get; set; }
    }
}
