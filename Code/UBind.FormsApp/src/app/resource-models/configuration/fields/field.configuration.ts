import { FieldDataType } from "@app/models/field-data-type.enum";
import { FieldType } from "@app/models/field-type.enum";
import { KeyboardInputMode } from "@app/models/keyboard-input-mode.enum";
import { WorkflowRole } from "@app/models/workflow-role.enum";

/**
 * Represents the configuration options common to all fields.
 */
export interface FieldConfiguration {
    $type: FieldType;
    name: string;
    key: string;
    dataType: FieldDataType;
    hideConditionExpression: string;
    workflowRole: WorkflowRole;
    tags: Array<string>;
    questionSetKey: string;
    currencyCode: string;
    keyboardInputMode: KeyboardInputMode;
}
