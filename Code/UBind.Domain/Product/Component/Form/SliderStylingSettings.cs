// <copyright file="SliderStylingSettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    public class SliderStylingSettings
    {
        /// <summary>
        /// Gets or sets a value for the height of the slider when its orientation is vertical.
        /// Defaults to 250px.
        /// </summary>
        public string VerticalHeight { get; set; }

        /// <summary>
        /// Gets or sets the settings for the unselected part of the slider bar.
        /// This is normally thinner and de-emphasised a little compared to the selection bar.
        /// </summary>
        public BarSettings Bar { get; set; }

        /// <summary>
        /// Gets or sets the settings for the part of the bar the represents the selection.
        /// This is normally thicker and brighter than the unselected part of the bar.
        /// </summary>
        public BarSettings SelectionBar { get; set; }

        /// <summary>
        /// Gets or sets the the control that you move up and down the bar to make your selection.
        /// </summary>
        public ThumbSettings Thumb { get; set; }

        /// <summary>
        /// Gets or sets settings for ticks.
        /// A tick is a small mark on the bar to indicate a position or value.
        /// Typically there will be ticks along the bar at regular intervals.
        /// </summary>
        public TickSettings Tick { get; set; }

        /// <summary>
        /// Gets or sets settings for disabled sliders.
        /// When the slider is disabled or read only, it's typically greyed out.
        /// </summary>
        public DisabledSettings Disabled { get; set; }

        /// <summary>
        /// Gets or sets settings for the bubble.
        /// The bubble is a speech like bubble which appears when you select or move the thumb along the axis of the bar.
        /// The value that you select appears inside the bubble.
        /// </summary>
        public BubbleSettings Bubble { get; set; }

        /// <summary>
        /// Gets or sets legend settings.
        /// The legend are a set of labels which appear under the axis of the slider, or in vertical mode to the left
        /// of the axis.
        /// </summary>
        public LegendSettings Legend { get; set; }

        public class BarSettings
        {
            /// <summary>
            /// Gets or sets the CSS colour to give the unselected part of the bar.
            /// </summary>
            public string Color { get; set; }

            /// <summary>
            /// Gets or sets the thickness of the bar.
            /// </summary>
            public string Thickness { get; set; }

            /// <summary>
            /// Gets or sets the border radius to apply so that the end of the bar is rounded.
            /// </summary>
            public string BorderRadius { get; set; }
        }

        public class ThumbSettings
        {
            /// <summary>
            /// Gets or sets the CSS colour to give the thumb. Defaults to 30px.
            /// </summary>
            public string Color { get; set; }

            /// <summary>
            /// Gets or sets the width of the thumb. Defaults to 30px.
            /// </summary>
            public string Width { get; set; }

            /// <summary>
            /// Gets or sets the height of the thumb. Defaults to the same as the width of the thumb.
            /// </summary>
            public string Height { get; set; }

            /// <summary>
            /// Gets or sets the border radius of the thumb, so it can appear round or rounded.
            /// Defaults to the same as the height of the thumb.
            /// </summary>
            public string BorderRadius { get; set; }

            /// <summary>
            /// Gets or sets the ratio to the size of the thumb which a highlight area will be drawn around the thumb. This is used to show the following states:
            /// 1. Hover(e.g.whilst the mouse is positioned over the thumb)
            /// 2. Focus(e.g. if you have tabbed onto the slider using they keyboard, or you have clicked it)
            /// 3. Active(e.g.whilst the mouse is pressed down)
            /// Defaults to 2.5.
            /// </summary>
            public string HighlightRatio { get; set; }
        }

        public class TickSettings
        {
            /// <summary>
            /// Gets or sets the colour of the tick when it's rendered on the unselected part of the bar.
            /// </summary>
            public string Color { get; set; }

            /// <summary>
            /// Gets or sets the colour of the tick when it's rendered on the selected part of the bar.
            /// </summary>
            public string SelectedColor { get; set; }

            /// <summary>
            /// Gets or sets the width of the tick. Defaults to 60% of the thickness of the unselected bar.
            /// </summary>
            public string Width { get; set; }

            /// <summary>
            /// Gets or sets the height of the tick. Defaults to the same as the width of the tick.
            /// </summary>
            public string Height { get; set; }

            /// <summary>
            /// Gets or sets the border radius of the tick, so that it can appear round or rounded.
            /// Defaults to the same as the width of the tick.
            /// </summary>
            public string BorderRadius { get; set; }
        }

        public class DisabledSettings
        {
            /// <summary>
            /// Gets or sets the color of the unselected part bar when the slider is disabled or read only.
            /// </summary>
            public string BarColor { get; set; }

            /// <summary>
            /// Gets or sets the color of the selected part of the bar when the slider is disabled or read only.
            /// </summary>
            public string SelectionbarColor { get; set; }

            /// <summary>
            /// Gets or sets the color of the thumb when the slider is disabled or read only.
            /// </summary>
            public string ThumbColor { get; set; }

            /// <summary>
            /// Gets or sets the color of the ticks when the slider is disabled or read only.
            /// </summary>
            public string TickColor { get; set; }
        }

        public class BubbleSettings
        {
            /// <summary>
            /// Gets or sets the color of the bubble.
            /// </summary>
            public string Color { get; set; }

            /// <summary>
            /// Gets or sets the border radius of the bubble. Defaults to 5px.
            /// </summary>
            public string BorderRadius { get; set; }

            /// <summary>
            /// Gets or sets the color of the text inside the bubble.
            /// </summary>
            public string TextColor { get; set; }

            /// <summary>
            /// Gets or sets the font size of the text in the bubble. Defaults to 13px.
            /// </summary>
            public string FontSize { get; set; }
        }

        public class LegendSettings
        {
            /// <summary>
            /// Gets or sets the width of the legend item.
            /// Too small a width will cause wrapping for the legend items, but too big a width will cause text to
            /// overlap and look ugly.
            /// When using a vertical orientation, this width will cause to increase the overall width of the slider.
            /// Defaults to 70px.
            /// </summary>
            public string ItemWidth { get; set; }

            /// <summary>
            /// Gets or sets the font size of legend items. Defaults to 13px.
            /// </summary>
            public string FontSize { get; set; }
        }
    }
}
