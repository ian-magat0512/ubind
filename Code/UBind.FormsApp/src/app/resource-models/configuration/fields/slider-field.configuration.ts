import { SliderStylingSettings } from "../slider-styling-settings";
import { OptionsFieldConfiguration } from "./options-field.configuration";

export interface SliderFieldConfiguration extends OptionsFieldConfiguration {

    /**
     * Gets or sets an expression which evaluates to the start value of the axis.
     * If not set, defaults to using 0 for the axis start value.
     */
    axisStartValueExpression: string;

    /**
     * Gets or sets an expression which evaluates to the end value of the axis.
     * If not set, defaults to using 100 for the axis end value.
     */
    axisEndValueExpression: string;

    /**
     * Gets or sets the lowest value on the access which the user can select.
     * Defaults to the axis start value if not set.
     */
    minSelectableValueExpression: string;

    /**
     * Gets or sets the highest value on the access which the user can select.
     * Defaults to the axis end value if not set.
     */
    maxSelectableValueExpression: string;

    /**
     * Gets or sets an expression which rseolves to the defined interval at which steps should be selectable
     * along the axis of the slider.
     */
    stepIntervalExpression: string;

    /**
     * Gets or sets a value indicating whether tick marks should be rendered along the axis of the slider.
     * Defaults to false.
     */
    showTickMarks?: boolean;

    /**
     * Gets or sets an expression which resolves to the defined interval at which tick marks should be
     * rendered along the axis of the slider.
     */
    tickMarkIntervalExpression: string;

    /**
     * Gets or sets an expression which resolves to an array of step indexes at which tick marks should be rendered
     * along the axis of the slider.
     */
    tickMarkStepIndexArrayExpression: string;

    /**
     * Gets or sets a value indicating whether a value label should be shown with each tick mark on the slider axis.
     * Defaults to false.
     */
    showTickMarkValueLabels?: boolean;

    /**
     * Gets or sets a value indicating whether a value label should be shown at the start of the slider axis.
     * The default value is true when there is no legend.
     */
    showAxisStartValueLabel?: boolean;

    /**
     * Gets or sets a value indicating whether a value label should be shown at the end of the slider axis.
     * The default value is true when there is no legend.
     */
    showAxisEndValueLabel?: boolean;

    /**
     * Gets or sets a value indicating whether a value label should be shown at the thumb position of the slider axis.
     * The default value is true.
     */
    showThumbValueLabel?: boolean;

    /**
     * Gets or sets a value indicating whether a set of value labels should be displayed in the legend position,
     * below the slider axis.
     */
    showLegend?: boolean;

    /**
     * Gets or sets a value indicating whether the slider bar should be coloured in from the start of the axis to
     * the thumb position. Defaults to true.
     */
    showSelectionBarFromAxisStart?: boolean;

    /**
     * Gets or sets a value indicating whether the slider bar should be coloured in from the thumb position to
     * the end of the axis. Defaults to false.
     */
    showSelectionBarFromAxisEnd?: boolean;

    /**
     * Gets or sets an expression which evaluates the value starting at which the bar should be coloured,
     * through to the thumb position.
     */
    showSelectionBarFromValueExpression: string;

    /**
     * Gets or sets an expression to format the value when being rendered as a label above the slider axis and
     * thumbs.
     */
    formatValueLabelExpression: string;

    /**
     * Gets or sets an expression to format the value that is rendered as a label above the thumb.
     * thumbs.
     */
    formatThumbValueLabelExpression: string;

    /**
     * Gets or sets an expression to format the value that is rendered as part of the legend below the axis.
     */
    formatLegendItemExpression: string;

    /**
     * Gets or sets a value indicating whether the axis of the slider should be reversed.
     * Defaults to false.
     */
    invertAxis?: boolean;

    /**
     * Gets or sets a value indicating whether the slider will be drawn vertically instead of horizontally.
     */
    vertical?: boolean;

    /**
     * Instead of snapping to each step, all the thumb to always move. It will detect the nearest step.
     */
    continuous?: boolean;

    /**
     * Gets or sets a value indicating whether the thumb should snap to the nearest step when the control is
     * released by the user. This is relevant for continuous sliders. Defaults to true.
     */
    snapToStep?: boolean;

    /** 
     * Gets or sets a value indicating whether labels can be rotated diagonally or vertically to fit more.
     */
    rotateLabelsToFit?: boolean;

    /**
     * Settings to style this instance of the slider.
     * Any settings added here will override the slider styling settings defined in the Theme.
     */
    styling: SliderStylingSettings;
}
