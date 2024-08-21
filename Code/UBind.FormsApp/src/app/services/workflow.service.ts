import { Injectable, Output, EventEmitter, Directive } from '@angular/core';
import { ConfigService } from './config.service';
import { CalculationService } from './calculation.service';
import { FormService } from './form.service';
import { OperationFactory } from '../operations/operation.factory';
import { AppEventService } from './app-event.service';
import { MessageService } from './message.service';
import { WorkflowStepOperation } from '../operations/workflow-step.operation';
import { ApplicationService } from './application.service';
import { BroadcastService } from './broadcast.service';
import { ActionType } from './action-type.enum';
import { ResumeApplicationService } from './resume-application.service';
import { BehaviorSubject, Subject } from 'rxjs';
import { filter, take, takeUntil } from 'rxjs/operators';
import { Expression, FixedArguments, ObservableArguments } from '@app/expressions/expression';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { WorkflowStatusService } from './workflow-status.service';
import { Disposable } from '@app/models/disposable';
import { Errors } from '@app/models/errors';
import { WorkflowAction } from '@app/models/configuration/workflow-action';
import { ApplicationLoadService } from './application-load-service';
import { FormType } from '@app/models/form-type.enum';
import { StringHelper } from '@app/helpers/string.helper';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { WorkflowDestinationService } from './workflow-destination.service';
import { WorkflowStep } from '@app/models/configuration/workflow-step';
import { SectionDisplayMode } from '@app/models/section-display-mode.enum';
import { RepeatingQuestionSetTrackingService } from './repeating-question-set-tracking.service';
import * as _ from 'lodash-es';
import { TriggerService } from './trigger.service';
import { EventService } from './event.service';
import { MapHelper } from '@app/helpers/map.helper';
import { ApplicationMode } from '@app/models/application-mode.enum';
import { UnifiedFormModelService } from './unified-form-model.service';
import { WorkflowHelper } from '@app/helpers/workflow.helper';
import { OperationInstruction } from '@app/models/operation-instruction';
import { OperationConfiguration } from '@app/models/configuration/operation-configuration';
import { WorkflowNavigation } from '@app/models/workflow-navigation';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { Operation } from '@app/operations/operation';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { CallbackDataResult } from '@app/models/callback-data-result.model';
import { FormEventCallbackService } from './form-event-callback.service';

export interface WorkflowActionCommand {
    actionName: string;
    widgetPosition: string;
    expressionFixedArguments: FixedArguments;
    expressionObservableArguments: ObservableArguments;
}

/**
 * Export workflow service class.
 * This handles all logic related to workflow steps,
 * navigation, transitions, and performing actions.  
 */
@Directive()
@Injectable({
    providedIn: 'root',
})
export class WorkflowService implements Disposable {

    public navigateToSubject: Subject<WorkflowDestination> = new Subject<WorkflowDestination>();
    @Output() public actionAborted: EventEmitter<any> = new EventEmitter<any>();
    @Output() public actionCompleted: EventEmitter<any> = new EventEmitter<any>();
    public initialisedSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

    private rollbackPointCurrentDestination: WorkflowDestination;
    private rollbackPointPreviousDestination: WorkflowDestination;
    private _previousDestination: WorkflowDestination;
    public quoteType: string;
    public isEditMode: boolean;
    public isCreateMode: boolean;
    public quoteState: string;
    public claimState: string;
    private _currentAction: WorkflowAction;
    private _currentActionName: string;
    private currentActionCommand: WorkflowActionCommand;
    public currentNavigation: WorkflowNavigation;
    private destroyed: Subject<void> = new Subject<void>();

    private startScreenExpressions: Map<string, Expression> = new Map<string, Expression>();
    private tabIndexExpressions: Map<string, Expression> = new Map<string, Expression>();

    // This is set to true during unit test runs to skip the animations
    public skipWorkflowAnimations: boolean = false;

    private actionQueue: Array<WorkflowActionCommand> = new Array<WorkflowActionCommand>();

    private skipWorkflowStepCalculationWhenOneIsInProgress: boolean = true;
    public allowActionsWhilstCalculationInProgress: boolean = false;

    public constructor(
        private formService: FormService,
        private configService: ConfigService,
        private calculationService: CalculationService,
        private operationFactory: OperationFactory,
        private appEventService: AppEventService,
        private messageService: MessageService,
        private workflowStepOperation: WorkflowStepOperation,
        private applicationService: ApplicationService,
        private broadcastService: BroadcastService,
        private resumeApplicationService: ResumeApplicationService,
        private expressionDependencies: ExpressionDependencies,
        private workflowStatusService: WorkflowStatusService,
        private applicationLoadService: ApplicationLoadService,
        private workflowDestinationService: WorkflowDestinationService,
        private repeatingQuestionSetTrackingService: RepeatingQuestionSetTrackingService,
        private triggerService: TriggerService,
        private eventService: EventService,
        private unifiedFormModelService: UnifiedFormModelService,
        private workflowHelper: WorkflowHelper,
        private operationInstructionService: OperationInstructionService,
        private formEventCallbackService: FormEventCallbackService,
    ) {
        this.formService.resetActionsSubject.subscribe(() => this.abortAction());
        this.broadcastService.on('TransactionDisabledError').pipe(takeUntil(this.destroyed)).subscribe(() => {
            this.rollbackToPreviousStep();
        });
    }

    private rollbackToPreviousStep(): void {
        // When execute workflow step is triggered it sets the current step before executing workflow step.
        // But when execute method throw an error, it needs to rollback to the previous step.
        this._previousDestination = this.rollbackPointPreviousDestination;
        this.setCurrentDestination(this.rollbackPointCurrentDestination);
    }

    private setCurrentNavigation(navigation: WorkflowNavigation) {
        this.currentNavigation = navigation;
        this.workflowStatusService.targetDestination = navigation.to;
    }

    public initialise(): void {
        this.setupExpressions();
        const previousStepName: string = this.applicationService.currentWorkflowDestination?.stepName;
        const startingDestination: WorkflowDestination = this.workflowHelper.getStartingDestination();
        this.setCurrentNavigation(new WorkflowNavigation(null, startingDestination));
        if (startingDestination.stepName != previousStepName) {
            const currentStepConf: WorkflowStep = this.configService.workflowSteps[startingDestination.stepName];
            if (currentStepConf.updateBackendWithCurrentWorkflowStepWhenItDiffers !== false) {
                // let the backend know what workflow step we are now on.
                this.triggerWorkflowStepOperation();
            }
        }
        this.updateSettingsForCurrentWorkflowStep();
        this.setCurrentDestination(this.currentNavigation.to);
        this.initialisedSubject.next(true);
        this.onConfigurationUpdated();
        this.listenForActionNoLongerInProgressAndProcessQueue();
        this.listenForCalculationNoLongerInProgressAndProcessQueue();
    }

    private listenForActionNoLongerInProgressAndProcessQueue(): void {
        this.workflowStatusService.actionInProgressSubject
            .pipe(filter((actionInProgress: boolean) => !actionInProgress))
            .subscribe(async (actionInProgress: boolean) => await this.checkAndProcessActionQueue());
    }

    private listenForCalculationNoLongerInProgressAndProcessQueue(): void {
        this.applicationService.calculationInProgressSubject
            .pipe(filter((calculationInProgress: boolean) => !calculationInProgress))
            .subscribe(async (calculationInProgress: boolean) => await this.checkAndProcessActionQueue());
    }

    private updateSettingsForCurrentWorkflowStep(): void {
        this.applicationService.workflowStepSubject.subscribe((stepName: string) => {
            if (stepName) {
                const workflowStep: WorkflowStep = this.configService.workflowSteps[stepName];

                // these must be explicitly set to true, because they default to false.
                this.allowActionsWhilstCalculationInProgress
                    = workflowStep.allowActionsWhilstCalculationInProgress === true;
                this.skipWorkflowStepCalculationWhenOneIsInProgress
                    = workflowStep.skipWorkflowStepCalculationWhenOneIsInProgress === true;
            }
        });
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.setupExpressions();
                if (this.configService.workflowSteps[this.currentDestination.stepName] === undefined) {
                    // this workflow step has been deleted
                    this.setCurrentNavigation(new WorkflowNavigation(
                        null, this.workflowHelper.getStartingDestination()));
                    this.navigateTo(this.currentNavigation.to);
                }
            });
    }

    public dispose(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public setCurrentDestination(workflowDestination: WorkflowDestination): void {
        const stepChanged: boolean = this.currentDestination.stepName != workflowDestination.stepName;
        this.applicationService.currentWorkflowDestination = workflowDestination;
        if (stepChanged) {
            this.workflowStatusService.workflowStepHistory.push(this.currentDestination.stepName);
        }
    }

    public get currentDestination(): WorkflowDestination {
        return this.applicationService.currentWorkflowDestination;
    }

    public get previousDestination(): WorkflowDestination {
        return this._previousDestination;
    }

    /**
     * Sets up the expressions associated with workflow steps, including:
     *  - start screen expression
     *  - tab index expressions
     */
    private setupExpressions(): void {
        let workflowSteps: any = this.configService.workflowSteps;
        for (let [key, value] of Object.entries(workflowSteps)) {
            let workflowStep: any = value;
            let startScreenExpressionSource: any = workflowStep.startScreenExpression ?
                workflowStep.startScreenExpression : workflowStep.startScreen ?
                    workflowStep.startScreen : null;
            if (startScreenExpressionSource) {
                let expression: Expression = this.startScreenExpressions.get(key);
                if (expression) {
                    expression.dispose();
                }
                expression = new Expression(
                    startScreenExpressionSource,
                    this.expressionDependencies,
                    `workflow step "${key}" start screen`);
                expression.nextResultObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((result: boolean) => workflowStep.startScreen = result);
                expression.triggerEvaluation();
                this.startScreenExpressions.set(key, expression);
            }

            let tabIndexExpressionSource: any = workflowStep.tabIndexExpression ? workflowStep.tabIndexExpression
                : workflowStep.tabIndex ? workflowStep.tabIndex : null;
            if (tabIndexExpressionSource) {
                let expression: Expression = this.tabIndexExpressions.get(key);
                if (expression) {
                    expression.dispose();
                }
                expression = new Expression(
                    tabIndexExpressionSource,
                    this.expressionDependencies,
                    `workflow step "${key}" tab index`);
                expression.nextResultObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((result: any) => workflowStep.tabIndex = result);
                expression.triggerEvaluation();
                this.tabIndexExpressions.set(key, expression);
            }
        }

        // if we have updated configuration and removed a workflow step, we need to dispose of its expressions.
        MapHelper.remove(
            this.startScreenExpressions,
            (key: string, value: Expression): boolean => !Object.keys(workflowSteps).includes(key),
            (key: string, value: Expression): void => value.dispose());
        MapHelper.remove(
            this.tabIndexExpressions,
            (key: string, value: Expression): boolean => !Object.keys(workflowSteps).includes(key),
            (key: string, value: Expression): void => value.dispose());
    }

    public get currentAction(): WorkflowAction {
        return this._currentAction;
    }

    public get currentActionName(): string {
        return this._currentActionName;
    }

    public requiredQuestionSetsAreValid(actionCommand: WorkflowActionCommand): boolean {
        const questionSetNamesRequiredToBeValid: Array<string> =
            this.getQuestionSetPathsRequiredToBeValid(actionCommand);
        return questionSetNamesRequiredToBeValid.length == 0
            || this.formService.questionSetsAreValid(questionSetNamesRequiredToBeValid);
    }

    private getQuestionSetPathsRequiredToBeValid(actionCommand: WorkflowActionCommand): Array<string> {
        let questionSetPathsRequiredToBeValid: Array<string> = new Array<string>();
        let requiresValidQuestionSetsExpression: string
            = this.workflowDestinationService.getApplicableProperty(
                this._currentAction,
                'requiresValidQuestionSetsExpression',
                actionCommand.widgetPosition);
        if (requiresValidQuestionSetsExpression) {
            questionSetPathsRequiredToBeValid = new Expression(
                requiresValidQuestionSetsExpression,
                this.expressionDependencies,
                `requiresValidQuestionSetsExpression on the ${this._currentActionName} action`,
                actionCommand.expressionFixedArguments,
                actionCommand.expressionObservableArguments,
            ).evaluateAndDispose();
        } else {
            questionSetPathsRequiredToBeValid = this.workflowDestinationService.getApplicableProperty(
                this._currentAction,
                'requiresValidQuestionSets',
                actionCommand.widgetPosition) || new Array<string>();
            let questionSetPathsRequiredToBeValidOrHidden: Array<string>
                = this.workflowDestinationService.getApplicableProperty(
                    this._currentAction,
                    'requiresValidOrHiddenQuestionSets',
                    actionCommand.widgetPosition);
            if (questionSetPathsRequiredToBeValidOrHidden) {
                questionSetPathsRequiredToBeValidOrHidden.forEach((path: string) => {
                    if (!this.formService.questionSetIsHidden(path)) {
                        if (questionSetPathsRequiredToBeValid.indexOf(path) == -1) {
                            questionSetPathsRequiredToBeValid.push(path);
                        }
                    }
                });
            }
        }

        return questionSetPathsRequiredToBeValid;
    }

    public async closeApp(): Promise<void> {
        await this.executeOperations(this._currentAction.operations);
        const actionType: string = this.getActionType(this._currentAction);
        const callbackData: CallbackDataResult = this.formEventCallbackService.createDataObject();

        if (Object.prototype.hasOwnProperty.call(this.configService.textElements?.product, 'closedAppMessage')) {
            const message: any = this.configService.textElements.product.closedAppMessage;
            this.messageService.sendMessage(
                'closeApp',
                {
                    actionType: actionType,
                    message: message.text,
                    severity: 4,
                    state: this.applicationService.applicationState,
                    data: callbackData,
                });
        } else {
            this.messageService.sendMessage(
                'closeApp',
                {
                    actionType: actionType,
                    message: '',
                    severity: 4,
                    state: this.applicationService.applicationState,
                    data: callbackData,
                });
        }
    }

    /**
     * Evaluates the next destination based on current action destination configuration
     * @returns the next workflow destination
     * REMARKS:
     * Destination can either be for the next workflow step, article, element or instance
     * depending on the current display mode configured.
     */
    public evaluateDestination(): WorkflowDestination {
        let destinationStepName: string = this.getCurrentActionDestinationStep();
        if (this.configService.workflowSteps[destinationStepName] === undefined) {
            throw Errors.Workflow.StepNotFound(destinationStepName);
        }
        let targetDestination: WorkflowDestination = {
            stepName: destinationStepName,
            articleIndex: this.getCurrentActionDestinationArticleIndex(),
            articleElementIndex: this.getCurrentActionDestinationArticleElementIndex(),
            repeatingInstanceIndex: this.getCurrentActionDestinationRepeatingInstanceIndex(),
        };

        if (targetDestination.articleElementIndex != undefined && targetDestination.articleIndex == undefined) {
            targetDestination.articleIndex = this.workflowStatusService.currentSectionWidgetStatus
                .getArticleIndexForArticleElementIndex(targetDestination.articleElementIndex);
        }

        if (targetDestination.repeatingInstanceIndex != undefined) {
            if (this.currentDestination.articleIndex != undefined) {
                targetDestination.articleIndex = this.currentDestination.articleIndex;
            }
            if (this.currentDestination.articleElementIndex != undefined) {
                targetDestination.articleElementIndex = this.currentDestination.articleElementIndex;
            }
        }

        return targetDestination;
    }

    private getCurrentActionDestinationStep(): string {
        let destinationStep: any = this._currentAction.destinationStep;
        if (_.isObject(destinationStep)) {
            // This is to support really old workflow.json configurations where destinationStep can be an object
            // where each property is a trigger type.
            // Are there any triggers?
            let matched: boolean = false;
            if (this.triggerService.activeTriggerDisplayConfigs) {
                for (let trigger of this.triggerService.activeTriggerDisplayConfigs) {
                    let type: string = trigger.type;
                    if (destinationStep[type]) {
                        destinationStep = destinationStep[type];
                        matched = true;
                        break;
                    }
                }
            }
            if (!matched) {
                destinationStep = destinationStep['default'] || null;
            }
        } else if (this._currentAction.destinationStepExpression) {
            destinationStep = new Expression(
                this._currentAction.destinationStepExpression,
                this.expressionDependencies,
                `destination step for action`,
                this.currentActionCommand?.expressionFixedArguments,
                this.currentActionCommand?.expressionObservableArguments).evaluateAndDispose();
        }
        return destinationStep || this.currentDestination.stepName;
    }

    private getCurrentActionDestinationArticleIndex(): number {
        let destinationArticleIndex: number = this._currentAction.destinationArticleIndex;
        if (this._currentAction.destinationArticleIndexExpression) {
            destinationArticleIndex = new Expression(
                this._currentAction.destinationArticleIndexExpression,
                this.expressionDependencies,
                `destination article index for action`,
                this.currentActionCommand?.expressionFixedArguments,
                this.currentActionCommand?.expressionObservableArguments).evaluateAndDispose();
        }
        return destinationArticleIndex;
    }

    private getCurrentActionDestinationRepeatingInstanceIndex(): number {
        let destinationRepeatingInstanceIndex: number = this._currentAction.destinationRepeatingInstanceIndex;
        if (this._currentAction.destinationRepeatingInstanceIndexExpression) {
            destinationRepeatingInstanceIndex = new Expression(
                this._currentAction.destinationRepeatingInstanceIndexExpression,
                this.expressionDependencies,
                `destination repeating instance index for action`,
                this.currentActionCommand?.expressionFixedArguments,
                this.currentActionCommand?.expressionObservableArguments).evaluateAndDispose();
        }
        return destinationRepeatingInstanceIndex;
    }

    private getCurrentActionDestinationArticleElementIndex(): number {
        let destinationArticleElementIndex: number = this._currentAction.destinationArticleElementIndex;
        if (this._currentAction.destinationArticleElementIndexExpression) {
            destinationArticleElementIndex = new Expression(
                this._currentAction.destinationArticleElementIndexExpression,
                this.expressionDependencies,
                `destination article element index for action`,
                this.currentActionCommand?.expressionFixedArguments,
                this.currentActionCommand?.expressionObservableArguments).evaluateAndDispose();
        }
        return destinationArticleElementIndex;
    }

    public async resumePreviousQuote(): Promise<void> {
        let quoteId: string = this.resumeApplicationService.loadExistingQuoteId();
        this.unifiedFormModelService.clear();
        await this.applicationLoadService.loadQuote(quoteId);
        if (this.applicationService.mode == ApplicationMode.Create) {
            this.applicationService.mode = ApplicationMode.Edit;
        }
        this.performNavigation(this.workflowHelper.getStartingDestination());
    }

    public copyExpiredQuote(): void {
        const params: any = {
            'quoteId': this.applicationService.quoteId,
        };

        this.executeCopyExpiredQuoteOperation(params);
    }

    public copyPreviousExpiredQuote(): void {
        const existingQuoteid: string = this.resumeApplicationService.loadExistingQuoteId();
        const params: any = {
            'quoteId': existingQuoteid,
        };

        this.executeCopyExpiredQuoteOperation(params);
    }

    private executeCopyExpiredQuoteOperation(params: any): void {
        let operation: Operation = this.operationFactory.create('copyExpiredQuote');
        operation.execute(params)
            .subscribe(async (data: any) => {
                if (data && '' + data['status'] == 'success') {
                    await this.applicationLoadService.handleLoadResponse(data);
                    this.performNavigation(this.workflowHelper.getStartingDestination());
                }
            });
    }

    public async queueAction(
        actionName: string,
        widgetPosition: string,
        expressionFixedArguments: FixedArguments = null,
        expressionObservableArguments: ObservableArguments = null,
    ): Promise<void> {
        this.actionQueue.push({
            actionName: actionName,
            widgetPosition: widgetPosition,
            expressionFixedArguments: expressionFixedArguments,
            expressionObservableArguments: expressionObservableArguments,
        });
        await this.checkAndProcessActionQueue();
    }

    public async checkAndProcessActionQueue(): Promise<void> {
        if (this.actionQueue.length
            && !this.workflowStatusService.isActionInProgress()
            && (this.allowActionsWhilstCalculationInProgress || !this.calculationService.calculationInProgress)
        ) {
            const actionCommand: WorkflowActionCommand = this.actionQueue.shift();
            await this.performAction(actionCommand);
        }
    }

    /**
     * @returns success or failure.
     */
    private async performAction(actionCommand: WorkflowActionCommand): Promise<boolean> {
        if (!this.hasAction(actionCommand.actionName)) {
            // The only time this can happen (there is no action by that name) is when two actions in a workflow
            // step are triggered and the first one completes. This can really only happen due to autoTrigger firing
            // on two buttons at the same time. If this is the case, we'll just ignore the second one.
            // it does mean that the workflow is not quite right, so we'll leave a warning for the product developer:
            console.warn(`We came across an attempt to perform the action ${actionCommand.actionName} on workflow `
                + `step ${this.currentDestination}, however that action does not exist on this step. It may exist on `
                + `the previous step. This can usually only happen when someone configures an autoTriggerExpression `
                + `on more than two buttons, and somehow both of those buttons fire. The first one will cause a `
                + `navigation to the next step, and then the second one will try to do the same, not knowing that `
                + `the workflow step has already changed. This is a bug with the configuration of the product, so `
                + `the product developer should fix it. For now we will just ignore this action.`);
            return false;
        }
        if (!this.allowActionsWhilstCalculationInProgress && this.calculationService.calculationInProgress) {
            console.log(
                'Cannot peform action "' + actionCommand.actionName + '" because a calculation is in progress.');
            return false;
        }
        if (this.workflowStatusService.isActionInProgress()) {
            console.log('Cannot peform action "' + actionCommand.actionName +
                '" because the action "' + this._currentActionName + '" is in progress.');
            return false;
        }

        this.currentActionCommand = actionCommand;
        this.setCurrentAction(actionCommand.actionName, actionCommand.widgetPosition);
        this.workflowStatusService.addActionInProgress(actionCommand.actionName);
        await this.waitUntilFieldValuesAndValiditiesAreStable();
        if (!this.requiredQuestionSetsAreValid(actionCommand)) {
            let questionSetPathsRequiredToBeValid: Array<string> = this.getQuestionSetPathsRequiredToBeValid(
                actionCommand);
            setTimeout(() => {
                this.formService.markAllInvalidFieldsTouched(questionSetPathsRequiredToBeValid);
                let scrolled: boolean = this.formService.scrollToFirstInvalidField(questionSetPathsRequiredToBeValid);
                if (!scrolled) {
                    throw Errors.Product.CouldNotScrollToInvalidFieldInQuestionSet(
                        actionCommand.actionName,
                        questionSetPathsRequiredToBeValid);
                }
            }, 0);
            this.abortAction();
            return false;
        } else {
            const actionType: string = this.getActionType(this._currentAction);
            if (actionType == ActionType.CloseApp) {
                await this.closeApp();
            } else if (actionType == ActionType.Transition || actionType == ActionType.PageTransition) {
                await this.executeCurrentAction();
            } else if (actionType == ActionType.ResumePreviousQuote) {
                this.resumePreviousQuote();
            } else if (actionType == ActionType.CopyExpiredQuote) {
                this.copyExpiredQuote();
            } else if (actionType == ActionType.CopyPreviousExpiredQuote) {
                this.copyPreviousExpiredQuote();
            }
            return true;
        }
    }

    private async executeCurrentAction(): Promise<void> {
        let targetDestination: WorkflowDestination = this.evaluateDestination();
        const workflowStep: WorkflowStep = this.configService.workflowSteps[targetDestination.stepName];
        let targetStepDisplayMode: SectionDisplayMode;
        if (workflowStep) {
            if (workflowStep.displayModeExpression) {
                targetStepDisplayMode = new Expression(
                    workflowStep.displayModeExpression,
                    this.expressionDependencies,
                    `workflow step display mode pre-navigation`,
                    this.currentActionCommand?.expressionFixedArguments,
                    this.currentActionCommand?.expressionObservableArguments,
                ).evaluateAndDispose();
            } else {
                targetStepDisplayMode = workflowStep.displayMode;
            }
        }
        if (targetStepDisplayMode == SectionDisplayMode.Article
            || targetStepDisplayMode == SectionDisplayMode.ArticleElement
        ) {
            // store the navigation direction so that section widget can start at the last element or article if needed
            this.applicationService.lastNavigationDirection =
                this.workflowDestinationService.getNavigationDirection(this.currentDestination, targetDestination);
        }
        await this.executeOperations(this._currentAction.operations);
        if (!this.currentActionHasDestination()) {
            this.completeAction();
            return;
        }
        this.performNavigation(targetDestination);
    }

    /**
     * Wait for field values and validities to become stable.
     * This is needed because some fields have a debounce on their value updates.
     */
    private async waitUntilFieldValuesAndValiditiesAreStable(): Promise<void> {
        return new Promise((resolve: () => void) => {
            // Wait for field values and validities to become stable
            // this is needed because some fields have a debounce on their value updates
            this.eventService.fieldValuesAndValiditiesAreStableSubject.pipe(
                filter((stable: boolean) => stable),
                take(1),
            ).subscribe((stable: boolean) => {
                resolve();
            });
        });
    }

    private async executeOperations(operationConfigs: Array<string | OperationConfiguration>): Promise<void> {
        if (operationConfigs && operationConfigs.length) {
            let instructions: Array<OperationInstruction> = new Array<OperationInstruction>();
            for (let operationConfig of operationConfigs) {
                const operation: OperationInstruction = new OperationInstruction(
                    operationConfig,
                    this.currentDestination.stepName);
                if (this.skipWorkflowStepCalculationWhenOneIsInProgress
                    && operation.name == CalculationOperation.opName && this.calculationService.calculationInProgress) {
                    // there's already a calculation in progress, so don't bother triggering this one.
                    continue;
                }
                instructions.push(operation);
            }
            await this.operationInstructionService.executeAll(instructions);
        }
    }

    private createRollbackPoint(): void {
        this.rollbackPointCurrentDestination = this.currentDestination;
        this.rollbackPointPreviousDestination = this._previousDestination;
    }

    public async navigateTo(destination: WorkflowDestination): Promise<void> {
        this.currentActionCommand = null;
        this.performNavigation(destination);
    }

    private async performNavigation(destination: WorkflowDestination): Promise<void> {
        this.createRollbackPoint();
        this.workflowStatusService.isNavigatingOut = true;
        this.setCurrentNavigation(new WorkflowNavigation(this.currentDestination, destination));
        this._previousDestination = this.currentDestination;
        this.navigateToSubject.next(this.currentNavigation.to);
        this.workflowStatusService.startNavigation(this.currentActionName);
        this.workflowStatusService.setWorkflowStepChangeInProgress(this.currentNavigation?.isDifferentStep());
        if (this.currentNavigation?.isDifferentStep()) {
            this.notifyStepChange();
        }
    }

    /**
     * let the back end know that the workflow step has changed.
     */
    private notifyStepChange(): void {
        this.triggerWorkflowStepOperation();
        if (this.applicationService.formType == FormType.Quote) {
            // send a quote step changed message so the portal can respond to it by updating the list
            this.messageService.sendMessage('quoteStepChanged', {
                quoteId: this.applicationService.quoteId,
                previousStep: this.currentNavigation.from?.stepName,
                destinationStep: this.currentNavigation.to.stepName,
            });
        }

        this.appEventService.createEvent('navigateTo', {
            'sourceStep': this.currentDestination,
            'destinationStep': this.currentNavigation.to.stepName,
        });
    }

    private triggerWorkflowStepOperation(): void {
        // Don't let this slow down the page transition. We'll run this in the background.
        const operationConfiguration: OperationConfiguration = {
            name: WorkflowStepOperation.opName,
            backgroundExecution: true,
        };
        const operation: OperationInstruction = new OperationInstruction(operationConfiguration);
        this.operationInstructionService.execute(operation);
    }

    private abortAction(): void {
        let abortedActionName: string = this._currentActionName;
        this._currentAction = null;
        this._currentActionName = null;
        this.workflowStatusService.removeActionInProgress(abortedActionName);
        this.workflowStatusService.stopNavigation();
        this.actionAborted.emit(abortedActionName);
    }

    private completeAction(): void {
        let completedActionName: string = this._currentActionName;
        this.changeApplicationMode();
        this._currentAction = null;
        this._currentActionName = null;
        this.workflowStatusService.removeActionInProgress(completedActionName);
        this.actionCompleted.emit(completedActionName);
    }

    public completedNavigationIn(): void {
        this.workflowStatusService.stopNavigation();
        if (this._currentAction) {
            this.completeAction();
        }
    }

    public completedNavigationOut(): void {
        this.workflowStatusService.isNavigatingOut = false;
        this._previousDestination = this.currentDestination;
        this.setCurrentDestination(this.currentNavigation.to);
        this.repeatingQuestionSetTrackingService.updateRepeatingInstanceOnNavigation(this.currentNavigation);
        if (this._currentAction) {
            this.changeApplicationMode();
        }
    }

    public getActionType(action: any): string {
        const defaultActionType: ActionType = ActionType.Transition;
        const actionTypeIsNotNullOrEmpty: any = action['type'] && action.type;
        if (actionTypeIsNotNullOrEmpty) {
            return action.type;
        }
        return defaultActionType;
    }

    private changeApplicationMode(): void {
        if (this._currentAction && !StringHelper.isNullOrEmpty(this._currentAction.changeApplicationModeTo)) {
            this.applicationService.mode = this._currentAction.changeApplicationModeTo;
            // since we just changed the application mode, we need to update the calculation widget,
            // and the best way to do that is to publish the latest quote/claim result 
            if (this.applicationService.formType == FormType.Quote) {
                this.eventService.quoteResultSubject.next(this.applicationService.latestQuoteResult);
            } else if (this.applicationService.formType == FormType.Claim) {
                this.eventService.claimResultSubject.next(this.applicationService.latestClaimResult);
            }
        }
    }

    private setCurrentAction(actionName: string, widgetPosition: string): void {
        this._currentAction = this.configService.workflowSteps[this.currentDestination.stepName].actions[actionName];
        if (!this._currentAction) {
            // There is no action by that name for the current workflow step. This is therefore probably an action
            // from a previous workflow step - probably a case of two actions with an autoTrigger which both
            // triggered, and so the first one completed.
            throw new Error(`Could not find action ${actionName} for workflow step ${this.currentDestination}`);
        }
        this._currentActionName = actionName;
    }

    private hasAction(actionName: string): boolean {
        let step: any = this.configService.workflowSteps[this.currentDestination.stepName];
        return (step && step.actions && step.actions[actionName] !== undefined);
    }

    /**
     * Checks if the current action has a destination based on the following properties:
     * destinationStep
     * destinationStepExpression
     * destinationArticleIndex
     * destinationArticleIndexExpression
     * destinationArticleElementIndex
     * destinationArticleElementIndexExpression
     * destinationRepeatingInstanceIndex
     * destinationRepeatingInstanceIndexExpression
     */
    private currentActionHasDestination(): boolean {
        if (!this._currentAction) {
            return false;
        }

        return this._currentAction.destinationArticleElementIndex != undefined ||
        this._currentAction.destinationArticleElementIndexExpression != undefined ||
        this._currentAction.destinationArticleIndex != undefined ||
        this._currentAction.destinationArticleIndexExpression != undefined ||
        this._currentAction.destinationRepeatingInstanceIndex != undefined ||
        this._currentAction.destinationRepeatingInstanceIndexExpression != undefined ||
        this._currentAction.destinationStep != undefined||
        this._currentAction.destinationStepExpression != undefined;
    }

    public applyDefaultLocations(workflowAction: WorkflowAction): void {
        if (workflowAction.locations == null) {
            // use the default location
            workflowAction.locations = ['formFooter'];
        }
        if (workflowAction.locationsWhenPrimaryAndQuestionSetsValid == null) {
            // use the default location for primary and valid actions
            workflowAction.locationsWhenPrimaryAndQuestionSetsValid = ['calculationWidgetFooter'];
        }
    }
}
