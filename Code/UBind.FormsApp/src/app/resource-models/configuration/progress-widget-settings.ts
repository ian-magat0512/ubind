import { ProgressStepIcons } from "./progress-step-icons";

/**
 * Settings which control the presentation and layout of the progress widget
 */
export interface ProgressWidgetSettings {

    /**
     * a value indicating whether the progress widget should be rendered.
     */
    showProgressWidget: boolean;

    /**
     * a value that determines the minimum width of a step when rendered in the progress widget.
     */
    minimumStepWidthPixels: number;

    /**
     * a value indicating whether the progress widget's visuals should be rendered.
     * If set to true, instead of just a set a headings, the progress widget will render html for
     * lines, circles, icons and numbers.
     */
    showVisuals: boolean;

    /**
     * a value indicating whether the progress widget steps should be have equal widths.
     * If set to false, a progress step with a longer label will be allowed to take up more width than the others.
     */
    useEqualWidthSteps: boolean;

    /**
     * a value indicating whether numbers should be rendered inside the container for each progress step.
     */
    showNumbers: boolean;

    /**
     * a value indicating icons should be rendered inside the container for each progress step.
     */
    showIcons: boolean;

    /**
     * a value indicating whether the progress widget should render an additional line
     * or bar representing the percentage of fields which are completed for ech step.
     */
    showCompletion: boolean;

    /**
     * the progress step icons to use when rendering the symbol of a progress step.
     */
    icons: ProgressStepIcons;

    /**
     * a value for the active color of a progress step.
     * CSS colors accepted, e.g. "#cacaca", "red", "rgb(19, 146, 255)".
     */
    activeColor: string;

    /**
     * a value for the colour when an active bar fades out.
     * This would typically be the same as the ActiveColor, with an alpha component of 0, e.g
     * rgba(19, 146, 255, 0)".
     */
    activeFadeOutColor: string;

    /**
     * a value for the color of a progress step before the active one.
     * CSS colors accepted, e.g. "#cacaca", "red", "rgb(69, 199, 133);".
     */
    pastColor: string;

    /**
     * a value for the colour when an past bar fades out.
     * This would typically be the same as the ActiveColor, with an alpha component of 0, e.g
     * rgba(69, 199, 133, 0);".
     */
    pastFadeOutColor: string;

    /**
     * a value for the color of a progress step after the active one.
     * CSS colors accepted, e.g. "#cacaca", "red", "rgb(197, 197, 197)".
     */
    futureColor: string;

    /**
     * a value for the colour when a future bar fades out.
     * This would typically be the same as the ActiveColor, with an alpha component of 0, e.g
     * rgba(197, 197, 197, 0)".
     */
    futureFadeOutColor: string;

    /**
     * the CSS size of the symbol container when numbers and/or icons are turned on.
     */
    symbolContainerSize: string;

    /**
     * the CSS size of the symbol container when numbers and icons are turned off.
     */
    emptySymbolContainerSize: string;

    /**
     * the thickness of the line as a CSS value.
     */
    lineThickness: string;

    /**
     * the color of icons rendered.
     */
    iconColor: string;

    /**
     * the color of numbers rendered.
     */
    numberColor: string;

    /**
     * the size of the icons as a CSS font-size value.
     */
    iconFontSize: string;

    /**
     * the size of the numbers as a CSS font-size setting.
     */
    numberFontSize: string;

    /**
     * the weight of the numbers as a CSS font-weight setting.
     */
    numberFontWeight: string;

    /**
     * the font faily to use for the numbers, as a CSS font-family setting.
     */
    numberFontFamily: string;

    /**
     * the margin around the symbols, so we can have a gap between the symbol container and lines.
     */
    symbolContainerMargin: string;

    /**
     * the CSS border-radius of the symbol container. Set this to 0 for a square container.
     */
    symbolContainerBorderRadius: string;

    /**
     * the breakpoint in pixels below which the progress widget will display in simple mode.
     * When in simple mode, the step headings are not rendered, and bars between steps are hidden
     * except for the current step.
     */
    simpleModeBreakpointPixels?: number;

    /**
     * Gets or sets a value indicating whether to show a line between collapsed steps when in simple mode.
     * This can be useful if there is no symbolContainerMargin, so that in simple mode the steps don't jut up
     * against each other.
     * Defaults to true when the symbolContainerMargin is "0", "0px" or "-1px", otherwise defaults to false.
     */
    showLineForCollapsedSteps?: boolean;

    /**
     * Gets or sets the total width of the line between collapsed steps (in simple mode).
     * Only applicable when showLineForCollapsedStepsInSimpleMode is true.
     * Defaults to 4px.
     */
    collapsedStepLineWidth: string;

    /**
     * Gets or sets a value indicating whether to render a line for the last step when in simple mode.
     * This can be useful to fill the screen so there is no blank space.
     */
    showLineForLastStepInSimpleMode?: boolean;
}
