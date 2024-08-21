import { WorkflowAction } from "@app/models/configuration/workflow-action";

/**
 * An action button which can be clicked or activated. Rendered as part of the ActionsWidget.
 */
export interface ActionButton {
    name: string;
    definition: WorkflowAction;
}
