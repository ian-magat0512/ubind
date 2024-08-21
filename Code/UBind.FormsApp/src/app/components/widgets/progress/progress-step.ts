import { Expression } from "@app/expressions/expression";
import { TextWithExpressions } from "@app/expressions/text-with-expressions";

/**
 * A single step within the set ot steps to be displayed in the progress widget.
 */
export interface ProgressStep {

    /**
     * The unique name of this workflow step
     */
    name: string;

    /**
     * The actual text displayed for the step
     */
    title: string;

    /**
     * An icon to display next to the title
     */
    titleIcon: string;

    /**
     * This is the first step
     */
    first: boolean;

    /**
     * This is the last step
     */
    last: boolean;

    /**
     * This step has another step after it
     */
    hasNext: boolean;

    /**
     * This step has a step before it
     */
    hasPrevious: boolean;

    /**
     * If it's active then this is the step for the page that's currently being shown
     */
    active: boolean;

    /**
     * The next step is truncated so not visible
     */
    nextTruncated: boolean;

    /**
     * The previous step is truncated so not visible
     */
    previousTruncated: boolean;

    /**
     * The next step is active
     */
    nextActive: boolean;

    /**
     * The previous step is active
     */
    previousActive: boolean;

    /**
     * This step is after the active step
     */
    future: boolean;

    /**
     * This step is before the active step
     */
    past: boolean;

    /**
     * if true the step is to be rendered, if false it's not.
     * Note it still may not be rendered if it's truncated.
     */
    render: boolean;

    /**
     * if true the step is not shown due to screen size restrictions
     */
    truncated: boolean;

    /**
     * A field which is used to sort the steps into a particular order
     */
    tabIndex: number;

    /**
     * The index of the step in the full list, after sorting
     */
    index: number;

    /**
     * The index of the progress step for rendered steps
     * If the step is not rendered, this value will be -1
     */
    renderIndex: number;

    /**
     * The icon to render, or null if no icon should be rendered.
     */
    icon: string;

    /* --- for internal use --- */
    titleExpression?: TextWithExpressions;
    tabIndexExpression?: Expression;

    canClickToNavigate: boolean;
}
