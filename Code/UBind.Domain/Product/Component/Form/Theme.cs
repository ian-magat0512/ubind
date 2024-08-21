// <copyright file="Theme.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product.Component.Form
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;
    using UBind.Domain.Validation;

    /// <summary>
    /// The theme determines how the form looks when rendered.
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Gets or sets the identifier of an FontAwesome kit to use for this theme.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Font Awesome Kit")]
        public string FontAwesomeKitId { get; set; }

        /// <summary>
        /// Gets or sets an icon to use for tooltips.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Tooltip Icon")]
        public string TooltipIcon { get; set; }

        /// <summary>
        /// Gets or sets a list of expressions which each evaluate to an external style sheet URL.
        /// </summary>
        public List<string> ExternalStyleSheetUrlExpressions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include the quote reference number in the sidebar.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Include Quote Reference In Sidebar")]
        public bool? IncludeQuoteReferenceInSidebar { get; set; }

        /// <summary>
        /// Gets or sets a multiplier for the scroll duration, which allows you to speed up or slow down the time it
        /// takes to scroll to invalid form fields.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Scroll Duration Multiplier")]
        public float ScrollDurationMultipler { get; set; }

        /// <summary>
        /// Gets or sets a value that determines the width of the sidebar when position on the side (not the top).
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Sidebar Width")]
        public int SidebarWidthPixels { get; set; }

        /// <summary>
        /// Gets or sets a value that determines what the maximum width of the viewport in pixels would still be
        /// considered mobile size. Defaults to 767.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Mobile Breakpoint Pixels")]
        public int MobileBreakpointPixels { get; set; } = 767;

        /// <summary>
        /// Gets or sets a value indicating whether the sidebar should show the payment options.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Sidebar Payment Options")]
        public bool? ShowPaymentOptionsInSidebar { get; set; }

        /// <summary>
        /// Gets or sets the offset in pixels to leave on extra small devices before rendering uBind due to a floating header on the page
        /// which uBind is injected into.
        /// Note this will be deprecated soon because it doesn't allow it to be set separately for each different website.
        /// </summary>
        [Obsolete("Offsets are now configured at the time of embedding, as per UB-5701")]
        [PopulateWhenEmpty(false)]
        [WorkbookTableSectionPropertyName("Styling", "Other", "Offset Top XS")]
        public int? OffsetTopExtraSmall { get; set; }

        /// <summary>
        /// Gets or sets the offset in pixels to leave on small devices before rendering uBind due to a floating header on the page
        /// which uBind is injected into.
        /// Note this will be deprecated soon because it doesn't allow it to be set separately for each different website.
        /// </summary>
        [Obsolete("Offsets are now configured at the time of embedding, as per UB-5701")]
        [PopulateWhenEmpty(false)]
        [WorkbookTableSectionPropertyName("Styling", "Other", "Offset Top SM")]
        public int? OffsetTopSmall { get; set; }

        /// <summary>
        /// Gets or sets the offset in pixels to leave on medium size devices before rendering uBind due to a floating header on the page
        /// which uBind is injected into.
        /// Note this will be deprecated soon because it doesn't allow it to be set separately for each different website.
        /// </summary>
        [Obsolete("Offsets are now configured at the time of embedding, as per UB-5701")]
        [PopulateWhenEmpty(false)]
        [WorkbookTableSectionPropertyName("Styling", "Other", "Offset Top MD")]
        public int? OffsetTopMedium { get; set; }

        /// <summary>
        /// Gets or sets the offset in pixels to leave on large devices before rendering uBind due to a floating header on the page
        /// which uBind is injected into.
        /// Note this will be deprecated soon because it doesn't allow it to be set separately for each different website.
        /// </summary>
        [Obsolete("Offsets are now configured at the time of embedding, as per UB-5701")]
        [PopulateWhenEmpty(false)]
        [WorkbookTableSectionPropertyName("Styling", "Other", "Offset Top LG")]
        public int? OffsetTopLarge { get; set; }

        /// <summary>
        /// Gets or sets the offset in pixels to leave on extra large devices before rendering uBind due to a floating header on the page
        /// which uBind is injected into.
        /// Note this will be deprecated soon because it doesn't allow it to be set separately for each different website.
        /// </summary>
        [Obsolete("Offsets are now configured at the time of embedding, as per UB-5701")]
        [PopulateWhenEmpty(false)]
        [WorkbookTableSectionPropertyName("Styling", "Other", "Offset Top XL")]
        public int? OffsetTopExtraLarge { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the top heading should be rendered.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Use Top Heading")]
        public bool? ShowTopHeading { get; set; }

        /// <summary>
        /// Gets or sets the styles for this theme.
        /// </summary>
        [ValidateItems]
        public List<Style> Styles { get; set; }

        /// <summary>
        /// Gets or sets the list of google fonts used in this Theme.
        /// </summary>
        [ValidateItems]
        public List<GoogleFont> GoogleFonts { get; set; }

        /// <summary>
        /// Gets or sets the article margin to use on mobile devices.
        /// </summary>
        [PopulateWhenEmpty(false)]
        [WorkbookTableSectionPropertyName("Styling", "Other", "Small Device Margin")]
        public int? SmallDeviceMarginPixels { get; set; }

        /// <summary>
        /// Gets or sets the background colour to use for the section of the calculation widget
        /// that shows that a quote has a review trigger.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Primary Colours", "Calculation Widget Header")]
        public string CalculationWidgetHeaderBackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the background colour to use for the section of the calculation widget
        /// that shows that a quote has a review trigger.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Primary Colours", "Review")]
        public string CalculationWidgetReviewBackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the background colour to use for the section of the calculation widget
        /// that shows that a quote has a soft referral.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Primary Colours", "Soft Referral")]
        public string CalculationWidgetSoftReferralBackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the background colour to use for the section of the calculation widget
        /// that shows that a quote has a endorsement trigger.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Primary Colours", "Endorsement")]
        public string CalculationWidgetEndorsementBackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the background colour to use for the section of the calculation widget
        /// that shows that a quote has a hard referral.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Primary Colours", "Hard Referral")]
        public string CalculationWidgetHardReferralBackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the background colour to use for the section of the calculation widget
        /// that shows that a quote has a decline trigger.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Primary Colours", "Decline")]
        public string CalculationWidgetDeclineBackgroundColour { get; set; }

        /// <summary>
        /// Gets or sets the settings which control the presentation and layout of the progress widget.
        /// </summary>
        public ProgressWidgetSettings ProgressWidgetSettings { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the progress widget should be rendered.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Show Progress Widget")]
        [Obsolete("Use ProgressWidgetSettings instead")]
        public bool ShowProgressWidget { get; set; }

        /// <summary>
        /// Gets or sets a value that determines the minimum width of a step when rendered in the progress widget.
        /// </summary>
        [PopulateWhenEmpty(false)]
        [WorkbookTableSectionPropertyName("Styling", "Other", "Minimum Progress Step Width Pixels")]
        [Obsolete("Use ProgressWidgetSettings instead")]
        public int MinimumProgressStepWidthPixels { get; set; }

        public SliderStylingSettings SliderStylingSettings { get; set; }

        /// <summary>
        /// Gets or sets the orientiation for transition animations, which can be horizontal or vertical.
        /// </summary>
        [WorkbookTableSectionPropertyName("Styling", "Other", "Transition Animation Orientation")]
        [JsonConverter(typeof(StringEnumHumanizerJsonConverter))]
        public TransitionAnimationOrientation TransitionAnimationOrientation { get; set; }
    }
}
