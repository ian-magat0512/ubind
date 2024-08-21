import { Injectable } from "@angular/core";
import { OperationInstruction } from "@app/models/operation-instruction";
import { WorkflowOperation } from "@app/models/workflow-operation.constant";

/**
 * Stores the status of operations so that expressions can access the status
 * without creating circular dependencies.
 */
@Injectable({
    providedIn: 'root',
})
export class OperationStatusService {

    /**
     * The currently executing instruction
     */
    public executingInstruction: OperationInstruction;

    /**
     * Indicates whether an queued operation is in progress
     */
    public operationInProgress: boolean;

    /**
     * The list of priority operations in progress, that were not queued.
     */
    public priorityOperationsInProgress: Array<OperationInstruction> = new Array<OperationInstruction>();

    public isOperationInProgress(name: string): boolean {
        if (name) {
            if (name == WorkflowOperation.Calculation) {
                if (this.priorityOperationsInProgress.length > 0) {
                    return true;
                }
            }
            return this.executingInstruction?.name == name;
        }

        return this.priorityOperationsInProgress.length > 0
            || this.executingInstruction != null;
    }
}
