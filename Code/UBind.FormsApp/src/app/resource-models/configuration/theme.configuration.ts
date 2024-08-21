import { GoogleFont } from "@app/models/configuration/settings";
import { StyleConfiguration } from "./style.configuration";
import { ProgressWidgetSettings } from "./progress-widget-settings";
import { SliderStylingSettings } from "./slider-styling-settings";
import { TransitionAnimationOrientation } from "@app/models/transition-animation-orientation.enum";

/**
 * Represents the configuration of a theme.
 */
export interface ThemeConfiguration {
    fontAwesomeKitId: string;
    tooltipIcon: string;
    externalStyleSheetUrlExpressions: Array<string>;
    includeQuoteReferenceInSidebar: boolean;
    scrollDurationMultiplier: number;
    sidebarWidthPixels: number;
    mobileBreakpointPixels: number; // max width where it's still considered mobile, e.g. 767

    /**
     * Defaults to true.
     */
    showPaymentOptionsInSidebar?: boolean;
    offsetTopExtraSmall: number;
    offsetTopSmall: number;
    offsetTopMedium: number;
    offsetTopLarge: number;
    offsetTopExtraLarge: number;
    styles: Array<StyleConfiguration>;
    googleFonts: Array<GoogleFont>;

    /**
     * Defaults to true
     */
    showTopHeading?: boolean;
    progressWidgetSettings: ProgressWidgetSettings;

    /**
     * @deprecated this has been moved to progressWidgetSettings
     */
    showProgressWidget: boolean;

    /**
     * @deprecated this has been moved to progressWidgetSettings
     */
    minimumProgressStepWidthPixels: number;

    sliderStylingSettings: SliderStylingSettings;

    transitionAnimationOrientation: TransitionAnimationOrientation;
}
