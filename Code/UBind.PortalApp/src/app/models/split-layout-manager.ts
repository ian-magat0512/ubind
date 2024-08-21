/**
 * Represents a component that manages whether and when to allow a split/pane or
 * master/detail view layout
 */
export interface SplitLayoutManager {
    shouldShowSplit(): boolean;
}
