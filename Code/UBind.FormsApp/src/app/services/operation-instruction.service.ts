import { Injectable } from "@angular/core";
import { OperationInstruction } from "@app/models/operation-instruction";
import { HttpErrorResponse, HttpResponse } from "@angular/common/http";
import { Operation } from "@app/operations/operation";
import { AlertService } from "@app/services/alert.service";
import { ApplicationService } from "@app/services/application.service";
import { BroadcastService } from "@app/services/broadcast.service";
import { ConfigService } from "@app/services/config.service";
import { ContextEntityService } from "@app/services/context-entity.service";
import { take, takeUntil } from 'rxjs/operators';
import { MessageService } from "@app/services/message.service";
import { OperationFactory } from "@app/operations/operation.factory";
import { OperationInstructionStatus } from "@app/models/operation-instruction-status.enum";
import { WorkflowOperation } from "@app/models/workflow-operation.constant";
import { ProblemDetails } from "@app/models/problem-details";
import { Alert } from "@app/models/alert";
import { Errors } from "@app/models/errors";
import { ContextEntitiesConfigResourceModel } from "@app/models/context-entities-config.model";
import { FormType } from "@app/models/form-type.enum";
import { OperationStatusService } from "./operation-status.service";
import { OperationError } from "@app/operations/operation-error";
import { LoggerService } from "@app/services/logger.service";
import { EventService } from "./event.service";

/**
 * Represents an operation which ran async.
 * We need to store these if they are Calculation or FormUpdate operations because
 * critical operations need to wait for these to complete before proceeding.
 */
interface PromisedOperationInstruction {
    promise: Promise<void>;
    operation: OperationInstruction;
}

/**
 * Most operations have to end up creating events on aggregates.
 * If they are executed at the same time, they tend to cause concurrency
 * exceptions, which slow them down and make the whole thing inefficient
 * as they have to be retried many times until they succeed.
 * To avoid this, we'll add them to a background queue and only
 * trigger them to execute one after the other.
 */
@Injectable()
export class OperationInstructionService {

    /**
     * The priority queue is for calculation requests specified with priority execution
     */
    private priorityQueue: Array<OperationInstruction> = new Array<OperationInstruction>();

    /**
     * The background queue is for operations which can execute in the background asynchronously
     */
    private backgroundQueue: Array<OperationInstruction> = new Array<OperationInstruction>();

    /**
     * The foreground queue is for top level operations which need to execute sychronously, and the
     * current workflow action is waiting for them to complete.
     */
    private foregroundQueue: Array<OperationInstruction> = new Array<OperationInstruction>();

    private workflowCompletionOperations: Array<WorkflowOperation> = [
        WorkflowOperation.Policy,
        WorkflowOperation.Submission,
        WorkflowOperation.Invoice,
        WorkflowOperation.Bind,
        WorkflowOperation.Settle,
    ];

    public constructor(
        private operationFactory: OperationFactory,
        private configService: ConfigService,
        private applicationService: ApplicationService,
        private contextEntityService: ContextEntityService,
        private alertService: AlertService,
        private broadcastService: BroadcastService,
        private messageService: MessageService,
        private operationStatusService: OperationStatusService,
        private logger: LoggerService,
        private eventService: EventService,
    ) {
    }

    /**
     * Operations that are run async and either calculation or formUpdate operations
     * are stored here so we can await them for critical operations.
     */
    private blockingInstructions: Array<PromisedOperationInstruction> = new Array<PromisedOperationInstruction>();

    /**
     * Executes a set of operations. 
     * Because we taking a queued approach, having a background operation before a foreground operation will
     * cause the background operation to execute and then the foreground one, which will make it seem like
     * both were foreground operations.
     * Since foreground operations are prioritised, if you have 2 background operations and 1 foreground, they
     * will execute in the following order:
     * 1. operation 1
     * 2. operation 3
     * 3. operation 2.
     */
    public async executeAll(instructions: Array<OperationInstruction>): Promise<void> {
        for (let instruction of instructions) {
            let noFailedOperation: boolean = instructions.findIndex((i: OperationInstruction) =>
                i.status === OperationInstructionStatus.Failed) == -1;
            if (noFailedOperation) {
                await this.execute(instruction);
            }
        }
    }

    public async execute(instruction: OperationInstruction, priority: boolean = false): Promise<void> {
        if (instruction.backgroundExecution) {
            let blockingInstruction: PromisedOperationInstruction = {
                promise: null,
                operation: instruction,
            };
            if (priority && (this.operationStatusService.executingInstruction == null
                || this.operationStatusService.executingInstruction.name == WorkflowOperation.Calculation)
            ) {
                blockingInstruction.promise = this.executePriorityOperation(instruction);
            } else if (priority) {
                blockingInstruction.promise = this.queueInstruction(instruction, this.priorityQueue);
            } else {
                blockingInstruction.promise = this.queueInstruction(instruction, this.backgroundQueue);
            }
            if (instruction.isBlockingOperation()) {
                this.blockingInstructions.push(blockingInstruction);
            }
        } else {
            if (priority) {
                await this.executePriorityOperation(instruction);
            } else {
                await this.queueInstruction(instruction, this.foregroundQueue);
            }
        }
    }

    private async executePriorityOperation(instruction: OperationInstruction): Promise<void> {
        const priorityOpsInProgress: Array<OperationInstruction>
            = this.operationStatusService.priorityOperationsInProgress;
        priorityOpsInProgress.push(instruction);
        this.publishOperationInProgress();
        this.abortPreviousInstructions(instruction);
        instruction.completedSubject.pipe(take(1)).subscribe((reason: string) => {
            const index: number = priorityOpsInProgress.indexOf(instruction);
            priorityOpsInProgress.splice(index, 1);
            this.publishOperationInProgress();
            this.checkAndProcessQueue();
        });
        await this.doExecute(instruction);
    }

    private async queueInstruction(
        instruction: OperationInstruction,
        queue: Array<OperationInstruction>,
    ): Promise<void> {
        await this.waitForBlockingOperationsToComplete(instruction);
        const completionPromise: Promise<void> = this.getCompletionPromise(instruction);
        queue.push(instruction);
        this.checkAndProcessQueue();
        return completionPromise;
    }

    private getCompletionPromise(instruction: OperationInstruction): Promise<void> {
        return new Promise((resolve: (value: void) => void, reject: (reason: any) => void) => {
            instruction.completedSubject.pipe(take(1)).subscribe(() => {
                resolve();
            });
        });
    }

    private checkAndProcessQueue(): void {
        if (this.operationStatusService.operationInProgress
            || this.operationStatusService.priorityOperationsInProgress.length > 0
        ) {
            // We only allow one operation to execute at a time.
            return;
        }
        let instructionToExecute: OperationInstruction;
        if (this.priorityQueue.length) {
            instructionToExecute = this.priorityQueue.shift();
        } else if (this.foregroundQueue.length) {
            instructionToExecute = this.foregroundQueue.shift();
        } else if (this.backgroundQueue.length) {
            instructionToExecute = this.backgroundQueue.shift();
        }
        if (instructionToExecute) {
            this.operationStatusService.operationInProgress = true;
            this.abortPreviousInstructions(instructionToExecute);
            this.operationStatusService.executingInstruction = instructionToExecute;
            this.publishOperationInProgress();
            if (this.operationStatusService.executingInstruction.shouldAbortExistingOperations()) {
                this.removeQueuedDuplicates(this.operationStatusService.executingInstruction.name);
            }
            this.operationStatusService.executingInstruction.completedSubject.pipe(take(1)).subscribe(() => {
                this.operationStatusService.operationInProgress = false;
                this.operationStatusService.executingInstruction = null;
                this.publishOperationInProgress();
                this.checkAndProcessQueue();
            });
            this.doExecute(this.operationStatusService.executingInstruction);
        }
    }

    private removeQueuedDuplicates(name: string): void {
        this.removeQueuedDuplicatesFromQueue(name, this.priorityQueue, 'priority');
        this.removeQueuedDuplicatesFromQueue(name, this.foregroundQueue, 'foreground');
        this.removeQueuedDuplicatesFromQueue(name, this.backgroundQueue, 'background');
    }

    private removeQueuedDuplicatesFromQueue(
        name: string,
        queue: Array<OperationInstruction>,
        queueName: string,
    ): void {
        const index: number
            = queue.findIndex((instruction: OperationInstruction) => instruction.name == name);
        if (index != -1) {
            const instruction: OperationInstruction = queue[index];
            this.logger.debug(`Removing duplicate ${name} operation with ID ${instruction.id} `
                + `from ${queueName} queue`);
            instruction.completedSubject.next('7 - The operation was removed from the queue as a duplicate.');
            this.removeFromBlockingInstructions(instruction);
            this.priorityQueue.splice(index, 1);
        }
    }

    private removeFromBlockingInstructions(instruction: OperationInstruction): void {
        const index: number = this.blockingInstructions
            .findIndex((promisedInstruction: PromisedOperationInstruction) => {
                return promisedInstruction.operation.id == instruction.id;
            });
        if (index != -1) {
            this.blockingInstructions.splice(index, 1);
        }
    }

    private async doExecute(
        instruction: OperationInstruction,
    ): Promise<void> {
        const time: number = Date.now();
        this.logger.debug(`Executing operation ${instruction.name} with id ${instruction.id} ${time}`);
        instruction.status = OperationInstructionStatus.Started;
        instruction.startTime = Date.now();
        return new Promise((resolve: (value: void) => void, reject: (reason: any) => void) => {
            let operation: Operation = this.operationFactory.create(instruction.name);
            instruction.abortSubject
                .pipe(
                    takeUntil(instruction.completedSubject))
                .subscribe(() => {
                    this.logger.debug(`Operation ${instruction.name} ${instruction.id} cancelled.`);
                    instruction.status = OperationInstructionStatus.Cancelled;
                    instruction.endTime = Date.now();
                    instruction.completedSubject.next("1 - operation cancelled/aborted " + instruction.getDebugInfo());
                    instruction.completedSubject.complete();
                    resolve();
                });
            operation.execute(instruction.params, instruction.args, instruction.id, instruction.abortSubject)
                .pipe(
                    takeUntil(instruction.completedSubject))
                .subscribe(
                    (data: Response) => {
                        if (data instanceof HttpResponse) {
                            let httpResponseData: HttpResponse<any> = data;
                            if (httpResponseData && '' + httpResponseData['status'] == 'success') {
                                // the operation succeeded if we reached this point
                                resolve();
                            } else {
                                this.logger.debug(`${instruction.name} ${instruction.id} got a failure response`);
                                instruction.status = OperationInstructionStatus.Failed;
                                instruction.endTime = Date.now();
                                instruction.completedSubject.next("2 - failure - received a non success status");
                                instruction.completedSubject.complete();
                                reject(httpResponseData['status']);
                            }
                        } else if (data instanceof HttpErrorResponse) {
                            let httpErrorResponseData: HttpErrorResponse = data;
                            if (ProblemDetails.isProblemDetailsResponse(httpErrorResponseData)) {
                                this.alertService.alert(
                                    Alert.fromError(ProblemDetails.fromJson(httpErrorResponseData.error)));
                            } else if (httpErrorResponseData.status >= 500 && httpErrorResponseData.status <= 599) {
                                this.alertService.alert(new Alert(
                                    'Something went wrong',
                                    `The server reported an error during the handling of your request. ` +
                                    `The issue has been logged and a notification ` +
                                    `has been sent to our support team. In the mean time, ` +
                                    `please try again, and if you're still having issues, ` +
                                    `please don't hesitate to contact us for assistance.`,
                                ));
                            }

                            setTimeout(() => {
                                this.broadcastService.broadcast('ErrorPromptEvent', {});
                            }, 500);

                            instruction.status = OperationInstructionStatus.Failed;
                            this.logger.debug(`${instruction.name} ${instruction.id} failed.`);
                            instruction.endTime = Date.now();
                            instruction.completedSubject.next("3 - failed - received an HttpErrorResponse");
                            instruction.completedSubject.complete();
                            reject(httpErrorResponseData.error);
                        } else if (data == null) {
                            // we never proceeded with the request since it was redundant
                            instruction.status = OperationInstructionStatus.Cancelled;
                            resolve();
                        } else {
                            throw Errors.General.Unexpected("When processing a workflow action operation, the "
                                + "response received was not an instance of HttpResponse or HttpErrorResponse");
                        }
                    },
                    (err: any) => {
                        this.operationStatusService.operationInProgress = false;
                        if (this.applicationService.debug) {
                            console.log(`Operation ${instruction.name} ${instruction.id} got an error.`);
                        }
                        if (err?.error?.error?.doc_url?.includes('stripe.com')) {
                            // Specific handler for stripe payment server side error
                            throw Errors.Payment.ProviderError('Stripe', err.error.error['message']);
                        } else if (err?.error?.code === 'operation.bind.not.permitted') {
                            this.alertService.alert(
                                Alert.fromError(ProblemDetails.fromJson(err.error)));
                            // We need to retrigger the calculation to get the latest data
                            // since the current calculation result was not the latest during the bind operation.
                            // so the user/client need to retry the bind operation after the calculation is done.
                            this.eventService.retriggerCalculationSubject.next();
                        } else if (err.name === OperationError.name) {
                            let opError: OperationError = err as OperationError;
                            this.alertService.alert(new Alert(
                                opError.title,
                                opError.message,
                                opError.additionalDetails,
                                null,
                                opError.additionalDetailsTitle,
                            ));
                        }

                        instruction.status = OperationInstructionStatus.Failed;
                        instruction.endTime = Date.now();
                        instruction.completedSubject.next("4 - failed - the code produced an error");
                        instruction.completedSubject.complete();
                        reject(err.error);
                    });
        }).then(async () => {
            if (instruction.status == OperationInstructionStatus.Cancelled) {
                instruction.endTime = Date.now();
                instruction.completedSubject.next("6 - Cancelled - the operation was cancelled XXXXXXXXXXXXXXXXXXXXXX. "
                    + instruction.getDebugInfo());
                instruction.completedSubject.complete();
            } else {
                const time: number = Date.now();
                this.logger.debug(`${instruction.name} ${instruction.id} completed ${time}`);
                this.notifyIfWorkflowCompleted(instruction);
                await this.reloadContextEntitiesIfOperationIsConfiguredToDoSo(instruction.name);
                instruction.status = OperationInstructionStatus.Completed;
                instruction.endTime = Date.now();
                instruction.completedSubject.next("5 - success - we completed this operaiton and associated tasks. "
                    + instruction.getDebugInfo());
                instruction.completedSubject.complete();
            }
        });
    }

    private async reloadContextEntitiesIfOperationIsConfiguredToDoSo(operationName: string) {
        const config: ContextEntitiesConfigResourceModel = this.applicationService.formType == FormType.Claim
            ? this.configService.contextEntities?.claims
            : this.configService.contextEntities?.quotes;
        if (config?.reloadWithOperations?.includes(operationName.toString())) {
            await this.contextEntityService.loadContextEntities();
        }
    }

    private notifyIfWorkflowCompleted(instruction: OperationInstruction): void {
        if (WorkflowOperation[instruction.name]
            && this.workflowCompletionOperations.includes(WorkflowOperation[instruction.name])
        ) {
            this.messageService.sendMessage('saveInitiated', instruction.name);
            this.updateCloseButtonLabelInPortal(instruction);
        }
    }

    /**
     * updates the label on portal. this is important that this runs after "saveInitiated" message.
     */
    private updateCloseButtonLabelInPortal(instruction: OperationInstruction): void {
        let payloadText: string = 'cancel';
        const currentWorkFlowStep: any = this.configService.textElements.workflow
            ? this.configService.textElements.workflow[instruction.destinationStepName]
            : null;
        if (currentWorkFlowStep) {
            const closeButtonLabel: any = currentWorkFlowStep['closeButtonLabel'];
            payloadText = closeButtonLabel ? closeButtonLabel.text : 'complete';
        }
        this.messageService.sendMessage('closeButtonLabel', payloadText);
    }

    private async waitForBlockingOperationsToComplete(instruction: OperationInstruction): Promise<void> {
        if (instruction.isCritical()) {
            // wait for any outstanding calculation or formupdate operations (or operations with them nested inside)
            let outstandingInstructions: Array<PromisedOperationInstruction> = this.blockingInstructions
                .filter((op: PromisedOperationInstruction) =>
                    op.operation.status != OperationInstructionStatus.Completed
                    && op.operation.status != OperationInstructionStatus.Failed
                    && op.operation.status != OperationInstructionStatus.Cancelled);
            this.blockingInstructions.length = 0;
            if (outstandingInstructions && outstandingInstructions.length > 0) {
                this.logger.debug(`Before we run the ${instruction.name} operation, we're waiting for blocking `
                    + `operations to complete, of which there are ${outstandingInstructions.length}:`);
                for (let outstandingOperation of outstandingInstructions) {
                    this.logger.debug(`    - "${outstandingOperation.operation.name}" `
                        + `${outstandingOperation.operation.id}  ${outstandingOperation.operation.status}`);
                    try {
                        this.logger.debug(`Waiting for "${outstandingOperation.operation.name}" `
                            + `with id ${outstandingOperation.operation.id} to complete`);
                        await outstandingOperation.promise;
                        this.logger.debug(`--> "${outstandingOperation.operation.name}" `
                            + `${outstandingOperation.operation.id} completed`);
                    } catch (error) {
                        this.logger.debug(`--> "${outstandingOperation.operation.name}" `
                            + `${outstandingOperation.operation.id} failed. ${error.message}`);
                    }
                }
                this.logger.debug(`Finished waiting, so now we can execute the ${instruction.name} operation.`);
            }
        }
    }

    private abortPreviousInstructions(instruction: OperationInstruction): void {
        if (!instruction.shouldAbortExistingOperations()) {
            return;
        }
        if (this.operationStatusService.executingInstruction?.name == instruction.name
            && this.operationStatusService.executingInstruction.id != instruction.id) {
            this.operationStatusService.executingInstruction.abortSubject.next();
        }
        const priorityOpsInProgress: Array<OperationInstruction>
            = this.operationStatusService.priorityOperationsInProgress;
        for (let index: number = priorityOpsInProgress.length - 1; index >= 0; index--) {
            if (priorityOpsInProgress[index].name == instruction.name
                && priorityOpsInProgress[index].id != instruction.id
            ) {
                priorityOpsInProgress[index].abortSubject.next();
            }
        }
    }

    private publishOperationInProgress(): void {
        const priorityOpsInProgress: Array<OperationInstruction>
            = this.operationStatusService.priorityOperationsInProgress;
        if (priorityOpsInProgress.length) {
            this.applicationService.operationInProgressSubject.next(WorkflowOperation.Calculation);
        } else {
            if (this.operationStatusService.operationInProgress) {
                this.applicationService.operationInProgressSubject
                    .next(this.operationStatusService.executingInstruction.name);
            } else {
                this.applicationService.operationInProgressSubject.next(null);
            }
        }
    }

    public abortExecutingAndDeleteQueuedCalculationOperations(): void {
        if (this.operationStatusService.executingInstruction?.name == WorkflowOperation.Calculation) {
            this.operationStatusService.executingInstruction.abortSubject.next();
        }
        const priorityOpsInProgress: Array<OperationInstruction>
            = this.operationStatusService.priorityOperationsInProgress;
        for (let index: number = priorityOpsInProgress.length - 1; index >= 0; index--) {
            if (priorityOpsInProgress[index].name == WorkflowOperation.Calculation) {
                priorityOpsInProgress[index].abortSubject.next();
            }
        }
        this.removeQueuedDuplicates(WorkflowOperation.Calculation);
    }
}
