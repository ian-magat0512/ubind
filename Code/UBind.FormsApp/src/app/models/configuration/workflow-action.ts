import { ActionType } from "@app/services/action-type.enum";
import { ApplicationMode } from "../application-mode.enum";
import { OperationConfiguration } from "./operation-configuration";

/**
 * A button belonging to a step in the form that can be triggered and then does things.
 */
export interface WorkflowAction {
    /**
     * a list of operations that will be executed in order when this action is triggered
     */
    operations: Array<string | OperationConfiguration>;

    /**
     * The list of question sets that are required to be valid before this action can be performed
     */
    requiresValidQuestionSets: Array<string>;

    /**
     * The list of question sets that are required to be valid or hidden before this action can be performed
     */
    requiresValidOrHiddenQuestionSets: Array<string>;

    /**
     * the name of the destination step
     */
    destinationStep: string;

    /**
     * An expression which resolves to the name of the destination step
     */
    destinationStepExpression: string;

    destinationArticleIndex: number;
    destinationArticleIndexExpression: string;
    destinationArticleElementIndex: number;
    destinationArticleElementIndexExpression: string;
    destinationRepeatingInstanceIndex: number;
    destinationRepeatingInstanceIndexExpression: string;

    /**
     * When an action is primary it is presented so that it stands out, usually with a solid background
     */
    primaryExpression: string;

    /**
     * When an action is disabled, the button gets the class "disabled" and is not clicable.
     */
    disabledExpression: string;

    /**
     * When an action is busy, it gets a spinning animated icon.
     */
    busyExpression: string;

    /**
     * @deprecated use primaryExpression instead.
     */
    primary: string;

    /**
     * An expression which evaluations to true or false 
     * which when true, the action button will be not be shown
     */
    hiddenExpression?: string;

    /**
     * @deprecated please use hiddenExpression instead.
     */
    hidden?: string;

    cssClass?: string;

    /**
     * Automatically triggers the action when the expression's result evaluates to true.
     */
    autoTriggerExpression?: string;

    /**
     * Automatically triggers the action when the expression's result changes
     */
    triggerExpression?: string;

    /**
     * An expression which must evaluate to true for the auto trigger to happen.
     */
    triggerConditionExpression?: string;

    /**
     * The action to take when clicked. Defaults to "transition".
     */
    type?: ActionType;

    /**
     * If set, after the workflow action is executed, it will change the applicatiom mode to the one specified.
     * This is useful for when going from review directly to the endorse step, 
     * you can change the application mode to "endorse"
     * so that the correct messaging will be shown in the calculation widget.
     */
    changeApplicationModeTo?: ApplicationMode;

    /**
     * A list of locations that this button should be rendered.
     * 
     * If no locations are listed, the button will be rendered at the following locations by default: formFooter.
     */
    locations?: Array<string>;

    /**
     * A list of locations that this button should be rendered, if and when it's a primary action, and any required
     * question sets are valid (as specified in the requiresValidQuestionSets, equiresValidOrHiddenQuestionSets and
     * requiresValidQuestionSetsExpression parameters).
     * 
     * If no locations are listed, the button will be rendered at the following locations by default when
     * primary: calculationWidgetFooter.
     */
    locationsWhenPrimaryAndQuestionSetsValid?: Array<string>;

    /**
     * The label to put on the button.
     * If you leave this blank, the textElement with the same name as this action for the current workflow step will be
     * used.
     */
    label: string;

    /**
     * The icon to render on the button.
     * If you leave this blank, the icon associated with the textElement with the same name as this action for the
     * current workflow step will be used.
     */
    icon: string;
}
