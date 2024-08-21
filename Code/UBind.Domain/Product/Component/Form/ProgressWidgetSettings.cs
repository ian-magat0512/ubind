// <copyright file="ProgressWidgetSettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    public class ProgressWidgetSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the progress widget should be rendered.
        /// </summary>
        public bool ShowProgressWidget { get; set; } = true;

        /// <summary>
        /// Gets or sets a value that determines the minimum width of a step when rendered in the progress widget.
        /// </summary>
        [PopulateWhenEmpty(false)]
        public int MinimumStepWidthPixels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the progress widgets visuals should be rendered.
        /// If set to true, instead of just a set of headings, the progress widget will render html for
        /// lines, circles, icons and numbers.
        /// </summary>
        public bool ShowVisuals { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the steps in the progress widget should be have equal widths.
        /// If set to false, a progress step with a longer label will be allowed to take up more width than the others.
        /// </summary>
        public bool UseEqualWidthSteps { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether numbers should be rendered inside the container for each progress step.
        /// </summary>
        public bool ShowNumbers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether icons should be rendered inside the container for each progress step.
        /// </summary>
        public bool ShowIcons { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the progress widget should render an additional line
        /// or bar representing the percentage of fields which are completed for each step.
        /// </summary>
        public bool ShowCompletion { get; set; } = true;

        /// <summary>
        /// Gets or sets the progress step icons to use when rendering the symbol of a progress step.
        /// </summary>
        public ProgressStepIcons Icons { get; set; }

        /// <summary>
        /// Gets or sets a value for the active color of a progress step.
        /// CSS colors accepted, e.g. "#cacaca", "red", "rgb(19, 146, 255)".
        /// </summary>
        public string ActiveColor { get; set; }

        /// <summary>
        /// Gets or sets a value for the colour when an active bar fades out.
        /// This would typically be the same as the ActiveColor, with an alpha component of 0, e.g
        /// rgba(19, 146, 255, 0)".
        /// </summary>
        public string ActiveFadeOutColor { get; set; }

        /// <summary>
        /// Gets or sets a value for the color of a progress step before the active one.
        /// CSS colors accepted, e.g. "#cacaca", "red", "rgb(69, 199, 133)".
        /// </summary>
        public string PastColor { get; set; }

        /// <summary>
        /// Gets or sets a value for the colour when a past bar fades out.
        /// This would typically be the same as the PastColor, with an alpha component of 0, e.g
        /// rgba(69, 199, 133, 0);".
        /// </summary>
        public string PastFadeOutColor { get; set; }

        /// <summary>
        /// Gets or sets a value for the color of a progress step after the active one.
        /// CSS colors accepted, e.g. "#cacaca", "red", "rgb(197, 197, 197)".
        /// </summary>
        public string FutureColor { get; set; }

        /// <summary>
        /// Gets or sets a value for the colour when a future bar fades out.
        /// This would typically be the same as the FutureColor, with an alpha component of 0, e.g
        /// rgba(197, 197, 197, 0)".
        /// </summary>
        public string FutureFadeOutColor { get; set; }

        /// <summary>
        /// Gets or sets the CSS size of the visual container when numbers and/or icons are turned on.
        /// </summary>
        public string SymbolContainerSize { get; set; }

        /// <summary>
        /// Gets or sets the CSS size of the symbold container when numbers and icons are turned off.
        /// </summary>
        public string EmptySymbolContainerSize { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the line as a CSS value.
        /// </summary>
        public string LineThickness { get; set; }

        /// <summary>
        /// Gets or sets the color of icons rendered.
        /// </summary>
        public string IconColor { get; set; }

        /// <summary>
        /// Gets or sets the size of the icons as a CSS font-size value.
        /// </summary>
        public string IconFontSize { get; set; }

        /// <summary>
        /// Gets or sets the color of numbers rendered.
        /// </summary>
        public string NumberColor { get; set; }

        /// <summary>
        /// Gets or sets the size of the numbers as a CSS font-size setting.
        /// </summary>
        public string NumberFontSize { get; set; }

        /// <summary>
        /// Gets or sets the font weight of the numbers as a CSS font-weight setting.
        /// </summary>
        public string NumberFontWeight { get; set; }

        /// <summary>
        /// Gets or sets the font family to use for the numbers, as a CSS font-family setting.
        /// </summary>
        public string NumberFontFamily { get; set; }

        /// <summary>
        /// Gets or sets the margin around the symbols, so we can have a gap between the symbol container and lines.
        /// </summary>
        public string SymbolContainerMargin { get; set; }

        /// <summary>
        /// Gets or sets the CSS border-radius of the symbol container. Set this to 0 for a square container.
        /// </summary>
        public string SymbolContainerBorderRadius { get; set; }

        /// <summary>
        /// Gets or sets the breakpoint in pixels below which the progress widget will display in simple mode.
        /// When in simple mode, the step headings are not rendered, and bars between steps are hidden
        /// except for the current step.
        /// Defaults to 600.
        /// </summary>
        public int? SimpleModeBreakpointPixels { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show a line between collapsed steps when in simple mode.
        /// This can be useful if there is no symbolContainerMargin, so that in simple mode the steps don't jut up
        /// against each other.
        /// Defaults to true when the symbolContainerMargin is "0", "0px" or "-1px", otherwise defaults to false.
        /// </summary>
        public bool? ShowLineForCollapsedSteps { get; set; }

        /// <summary>
        /// Gets or sets the total width of the line between collapsed steps (in simple mode).
        /// Only applicable when showLineForCollapsedStepsInSimpleMode is true.
        /// Defaults to 4px.
        /// </summary>
        public string CollapsedStepLineWidth { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render a line for the last step when in simple mode.
        /// This can be useful to fill the screen so there is no blank space.
        /// </summary>
        public bool? ShowLineForLastStepInSimpleMode { get; set; }
    }
}
