export interface ScrollToElementInfo {
    elementPositionPixels: number;
    elementHeightPixels: number;
    visibleContentStartPixels: number;
    behaviour: ScrollBehavior;
    scrollMarginPixels: number;
    startTimeMillis: number;
    notifyWhenScrollingFinished: boolean;
}
