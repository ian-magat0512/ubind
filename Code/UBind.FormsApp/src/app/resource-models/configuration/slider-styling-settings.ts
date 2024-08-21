export interface SliderStylingSettings {
    /**
     * Gets or sets a value for the height of the slider when its orientation is vertical.
     * Defaults to 250px.
     */
    verticalHeight: string;

    /**
     * Gets or sets the settings for the unselected part of the slider bar.
     * This is normally thinner and de-emphasised a little compared to the selection bar.
     */
    bar: BarSettings;

    /**
     * Gets or sets the settings for the part of the bar the represents the selection.
     * This is normally thicker and brighter than the unselected part of the bar.
     */
    selectionBar: BarSettings;

    /**
     * Gets or sets the the control that you move up and down the bar to make your selection.
     */
    thumb: ThumbSettings;

    /**
     * Gets or sets settings for ticks.
     * A tick is a small mark on the bar to indicate a position or value.
     * Typically there will be ticks along the bar at regular intervals.
     */
    tick: TickSettings;

    /**
     * Gets or sets settings for disabled sliders.
     * When the slider is disabled or read only, it's typically greyed out.
     */
    disabled: DisabledSettings;

    /**
     * Gets or sets settings for the bubble.
     * The bubble is a speech like bubble which appears when you select or move the thumb along the axis of the bar.
     * The value that you select appears inside the bubble.
     */
    bubble: BubbleSettings;

    /**
     * Gets or sets legend settings.
     * The legend are a set of labels which appear under the axis of the slider, or in vertical mode to the left
     * of the axis.
     */
    legend: LegendSettings;
}

export interface BarSettings
{
    /**
     * Gets or sets the CSS colour to give the unselected part of the bar.
     */
    color: string;

    /**
     * Gets or sets the thickness of the bar.
     */
    thickness: string;

    /**
     * Gets or sets the border radius to apply so that the end of the bar is rounded.
     */
    borderRadius: string;
}

export interface ThumbSettings
{
    /**
     * Gets or sets the CSS colour to give the thumb. Defaults to 30px.
     */
    color: string;

    /**
     * Gets or sets the width of the thumb. Defaults to 30px.
     */
    width: string;

    /**
     * Gets or sets the height of the thumb. Defaults to the same as the width of the thumb.
     */
    height: string;

    /**
     * Gets or sets the border radius of the thumb, so it can appear round or rounded.
     * Defaults to the same as the height of the thumb.
     */
    borderRadius: string;

    /**
     * Gets or sets the ratio to the size of the thumb which a highlight area will be drawn around the thumb.
     * This is used to show the following states:
     * 1. Hover(e.g.whilst the mouse is positioned over the thumb)
     * 2. Focus(e.g. if you have tabbed onto the slider using they keyboard, or you have clicked it)
     * 3. Active(e.g.whilst the mouse is pressed down)
     * Defaults to 2.5.
     */
    highlightRatio: string;
}

export interface TickSettings
{
    /**
     * Gets or sets the colour of the tick when it's rendered on the unselected part of the bar.
     */
    color: string;

    /**
     * Gets or sets the colour of the tick when it's rendered on the selected part of the bar.
     */
    selectedColor: string;

    /**
     * Gets or sets the width of the tick. Defaults to 60% of the thickness of the unselected bar.
     */
    width: string;

    /**
     * Gets or sets the height of the tick. Defaults to the same as the width of the tick.
     */
    height: string;

    /**
     * Gets or sets the border radius of the tick, so that it can appear round or rounded.
     * Defaults to the same as the width of the tick.
     */
    borderRadius: string;
}

export interface DisabledSettings
{
    /**
     * Gets or sets the color of the unselected part bar when the slider is disabled or read only.
     */
    barColor: string;

    /**
     * Gets or sets the color of the selected part of the bar when the slider is disabled or read only.
     */
    selectionbarColor: string;

    /**
     * Gets or sets the color of the thumb when the slider is disabled or read only.
     */
    thumbColor: string;

    /**
     * Gets or sets the color of the ticks when the slider is disabled or read only.
     */
    tickColor: string;
}

export interface BubbleSettings
{
    /**
     * Gets or sets the color of the bubble.
     */
    color: string;

    /**
     * Gets or sets the border radius of the bubble. Defaults to 5px.
     */
    borderRadius: string;

    /**
     * Gets or sets the color of the text inside the bubble.
     */
    textColor: string;

    /**
     * Gets or sets the font size of the text in the bubble. Defaults to 13px.
     */
    fontSize: string;
}

export interface LegendSettings
{
    /**
     * Gets or sets the width of the legend item.
     * Too small a width will cause wrapping for the legend items, but too big a width will cause text to
     * overlap and look ugly.
     * When using a vertical orientation, this width will cause to increase the overall width of the slider.
     * Defaults to 70px.
     */
    itemWidth: string;

    /**
     * Gets or sets the font size of legend items. Defaults to 13px.
     */
    fontSize: string;
}
