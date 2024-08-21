import {
    Component, ElementRef, HostBinding, Input, OnChanges, OnDestroy, OnInit, SimpleChanges,
} from "@angular/core";
import { Expression, FixedArguments, ObservableArguments } from "@app/expressions/expression";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { StringHelper } from "@app/helpers/string.helper";
import { ApplicationMode } from "@app/models/application-mode.enum";
import { WorkflowAction } from "@app/models/configuration/workflow-action";
import { ApplicationService } from "@app/services/application.service";
import { ConfigService } from "@app/services/config.service";
import { EventService } from "@app/services/event.service";
import { FormService } from "@app/services/form.service";
import { WindowScrollService } from "@app/services/window-scroll.service";
import { WorkflowDestinationService } from "@app/services/workflow-destination.service";
import { WorkflowStatusService } from "@app/services/workflow-status.service";
import { WorkflowService } from "@app/services/workflow.service";
import _ from "lodash";
import { takeUntil } from "rxjs/operators";
import { Widget } from "../widget";
import { BehaviorSubject, Subject } from "rxjs";

/**
 * A button as defined in the workflow for the current step.
 */
@Component({
    selector: 'ubind-action-widget-ng',
    templateUrl: './action.widget.html',
})
export class ActionWidget extends Widget implements OnInit, OnDestroy, OnChanges {

    @HostBinding('class')
    public classes: string;

    @Input('action-name')
    public name: string;

    @Input('location')
    public location: string;

    @Input('label')
    public passedLabel: string;

    @Input('icon')
    public passedIcon: string;

    public cssPrefix: string;
    public disabled: boolean;
    public busy: boolean;
    public active: boolean;
    public primary: boolean;
    public usedIcon: string;
    public usedLabel: string;
    public autoTriggered: boolean;
    public autoTriggerExpressionSource: string;
    public triggerExpressionSource: string;
    public triggerConditionExpressionSource: string;
    public canTrigger: boolean;
    public render: boolean;
    public hiddenExpressionSource: string;
    public hiddenExpression?: Expression;
    public hidden: boolean;
    public changeApplicationModeTo: ApplicationMode;
    public definition: WorkflowAction;
    public autoTriggerExpression?: Expression;
    public triggerExpression?: Expression;
    public triggerConditionExpression?: Expression;
    public cssClass: string;
    public disabledExpressionSource: string;
    public disabledExpression?: Expression;
    public busyExpressionSource: string;
    public busyExpression?: Expression;

    /**
     * These are the arguments that will be passed in to expressions, that could change over time.
     * If there are any data attributes on the element, their values will be included here.
     */
    private expressionObservableArguments: ObservableArguments = {};
    private expressionFixedArguments: FixedArguments = {};

    /**
     * @deprecated please use location instead.
     */
    public widgetPosition: string;

    public constructor(
        protected workflowService: WorkflowService,
        protected configService: ConfigService,
        protected windowScrollService: WindowScrollService,
        protected expressionDependencies: ExpressionDependencies,
        public applicationService: ApplicationService,
        private workflowStatusService: WorkflowStatusService,
        private eventService: EventService,
        private workflowDestinationService: WorkflowDestinationService,
        private formService: FormService,
        private elementRef: ElementRef,
    ) {
        super();
    }

    public ngOnInit(): void {
        if (this.name) {
            this.updateWidgetPositionFromLocation();
            this.generate();
            this.cssPrefix = this.generatePrefix();
        }
        this.listenForActionInProgress();
        this.onConfigurationUpdated();
        this.readCustomDataAttributesAndCreateSubjects();
        this.observeChangesToCustomDataAttributes();
    }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes.name || changes.location || changes.label || changes.icon) {
            this.updateWidgetPositionFromLocation();
            this.generate();
            this.cssPrefix = this.generatePrefix();
        }
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
        this.disposeOfActionButtonExpressions();
    }

    private readCustomDataAttributesAndCreateSubjects(): void {
        // Get the host DOM element
        const element: HTMLElement = this.elementRef.nativeElement;

        // Iterate through the attributes
        Array.from(element.attributes).forEach((attr: Attr) => {
            // Check if the attribute name starts with 'data-'
            if (attr.name.startsWith('data-')) {
                // convert the kebab-case attribute name to camelCase, stripping out the 'data-' prefix
                const camelCaseName: string = StringHelper.toCamelCase(attr.name.substring(5));
                this.expressionObservableArguments[camelCaseName] = new BehaviorSubject<string>(attr.value);
            }
        });
    }

    private observeChangesToCustomDataAttributes() {
        const element: HTMLElement = this.elementRef.nativeElement;

        // Create a MutationObserver to watch for attribute changes
        const observer: MutationObserver = new MutationObserver((mutations: Array<MutationRecord>) => {
            mutations.forEach((mutation: MutationRecord) => {
                if (mutation.type === 'attributes') {
                    const attributeName: string = mutation.attributeName;
                    if (attributeName && attributeName.startsWith('data-')) {
                        const attributeValue: string = element.getAttribute(attributeName);

                        // convert the kebab-case attribute name to camelCase, stripping out the 'data-' prefix
                        const camelCaseName: string = StringHelper.toCamelCase(attributeName.substring(5));
                        const subject: Subject<string>
                            = <Subject<string>>this.expressionObservableArguments[camelCaseName];
                        subject.next(attributeValue);
                    }
                }
            });
        });

        // Set up the observer to watch for attribute changes
        observer.observe(element, { attributes: true });
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                if (this.name) {
                    const actionButtonKey: string = `${this.name}Button`;
                    const sourceActions: Array<WorkflowAction> =
                        this.configService.workflowSteps[this.workflowService.currentDestination.stepName].actions;
                    const sourceActionDefinition: WorkflowAction = sourceActions[this.name];
                    const textElementsForStep: any
                        = this.configService.textElements?.workflow?.[this.workflowService.currentDestination.stepName];
                    let icon: string = this.passedIcon
                        ?? sourceActionDefinition.icon
                        ?? (textElementsForStep && textElementsForStep[actionButtonKey]
                            ? textElementsForStep[actionButtonKey].icon
                            : false);
                    let text: string = this.passedLabel
                        ?? sourceActionDefinition.label
                        ?? (textElementsForStep && textElementsForStep[actionButtonKey]
                            ? textElementsForStep[actionButtonKey].text
                            : StringHelper.toTitleCase(this.name));
                    if (this.usedIcon != icon || this.usedLabel != text) {
                        this.formService.deregisterAutoTriggerActionButtons(
                            this.workflowService.currentDestination.stepName);
                        this.generate();
                    }
                }
            });
    }

    private generate(): void {
        const textElementsForStep: any
            = this.configService.textElements?.workflow?.[this.workflowService.currentDestination.stepName];
        const sourceActions: Array<WorkflowAction> =
            this.configService.workflowSteps[this.workflowService.currentDestination.stepName].actions;
        if (!sourceActions) {
            // the workflow step didn't define any actions, so this worfklow action button can't be used.
            return;
        }
        let sourceActionDefinition: WorkflowAction = sourceActions[this.name];
        if (!sourceActionDefinition) {
            // sometimes when we change workflow step the action widget will get the new step before the actions widget
            // does and in that case the definition will not be found. So we'll just hide the button until the parent
            // widget removes it from the DOM.
            this.render = false;
            return;
        }
        this.workflowService.applyDefaultLocations(sourceActionDefinition);
        const actionButtonKey: string = `${this.name}Button`;
        let icon: string = this.passedIcon
            ?? sourceActionDefinition.icon
            ?? (textElementsForStep && textElementsForStep[actionButtonKey]
                ? textElementsForStep[actionButtonKey].icon
                : null);
        let text: string = this.passedLabel
            ?? sourceActionDefinition.label
            ?? (textElementsForStep && textElementsForStep[actionButtonKey]
                ? textElementsForStep[actionButtonKey].text
                : StringHelper.toTitleCase(this.name));
        let cssClass: string = sourceActionDefinition.cssClass;
        this.disabled = false;
        this.active = this.workflowService.currentActionName == this.name;
        this.primary = false;
        this.usedIcon = icon;
        this.usedLabel = text;
        this.autoTriggered = false;
        this.canTrigger = true;
        this.render = false;
        this.hidden = true;
        this.cssClass = cssClass ? `tabbable ${cssClass}` : 'tabbable';
        this.hiddenExpressionSource = sourceActionDefinition.hiddenExpression ||
        sourceActionDefinition.hidden;
        this.changeApplicationModeTo = sourceActionDefinition.changeApplicationModeTo;
        this.definition = sourceActionDefinition;
        this.autoTriggerExpressionSource = sourceActionDefinition.autoTriggerExpression;
        this.triggerExpressionSource = sourceActionDefinition.triggerExpression;
        this.triggerConditionExpressionSource = sourceActionDefinition.triggerConditionExpression;

        let hiddenExpressionSource: string = this.hiddenExpressionSource;
        let requiresValidQuestionSets: Array<string> =
            this.workflowDestinationService.getApplicableProperty(sourceActionDefinition,
                'requiresValidQuestionSets', this.location);
        let additionalHiddenExpression: string;
        if (sourceActionDefinition.locationsWhenPrimaryAndQuestionSetsValid.includes(this.location)) {
            if (requiresValidQuestionSets && requiresValidQuestionSets.length > 0) {
                additionalHiddenExpression =
                    "!questionSetsAreValid(['" +
                    requiresValidQuestionSets.join("', '") + "'])";
            }
            let requiresValidOrHiddenQuestionSets: Array<string> =
                this.workflowDestinationService.getApplicableProperty(sourceActionDefinition,
                    'requiresValidOrHiddenQuestionSets', this.location);
            if (requiresValidOrHiddenQuestionSets && requiresValidOrHiddenQuestionSets.length > 0) {
                additionalHiddenExpression =
                    "!questionSetsAreValidOrHidden(['" +
                    requiresValidOrHiddenQuestionSets.join("', '") + "'])";
            }
            let requiresValidQuestionSetsExpression: string =
                this.workflowDestinationService.getApplicableProperty(sourceActionDefinition,
                    'requiresValidQuestionSetsExpression', this.location);
            if (requiresValidQuestionSetsExpression) {
                additionalHiddenExpression =
                    `!questionSetsAreValid(${requiresValidQuestionSetsExpression})`;
            }
        }

        if (!hiddenExpressionSource && additionalHiddenExpression) {
            hiddenExpressionSource = additionalHiddenExpression;
        } else if (hiddenExpressionSource && additionalHiddenExpression) {
            hiddenExpressionSource = `(${additionalHiddenExpression}) || ${hiddenExpressionSource}`;
        }

        this.expressionFixedArguments = {
            widgetPosition: this.widgetPosition,
            location: this.location,
            isPrimary: this.primary,
            actionName: this.name,
        };

        if (!hiddenExpressionSource) {
            this.hidden = false;
            this.evaluateWhetherToRender();
        } else {
            this.hiddenExpressionSource = hiddenExpressionSource;
            this.hiddenExpression = this.setupExpression(
                hiddenExpressionSource,
                `${this.name} action button hidden`,
                (hidden: boolean) => {
                    this.hidden = hidden;
                    this.evaluateWhetherToRender();
                });
        }

        const primaryExpression: string =
            sourceActionDefinition.primaryExpression || sourceActionDefinition.primary;
        this.setupExpression(primaryExpression,
            `${this.name} action button primary`, (primary: boolean) => {
                this.primary = primary;
                this.evaluateWhetherToRender();
            });

        this.disabledExpressionSource = sourceActionDefinition.disabledExpression === undefined
            ? "actionInProgress() || navigationInProgress()"
            : sourceActionDefinition.disabledExpression;
        if (!this.workflowService.allowActionsWhilstCalculationInProgress) {
            this.disabledExpressionSource += ' || calculationInProgress()';
        }
        this.disabledExpression = this.setupExpression(this.disabledExpressionSource,
            `${this.name} action button disabled`, (disabled: boolean) => {
                this.disabled = disabled;
            });

        this.busyExpressionSource = sourceActionDefinition.busyExpression == undefined
            ? "actionInProgress(actionName) || navigationInProgress(actionName)"
            : sourceActionDefinition.busyExpression;
        if (!this.workflowService.allowActionsWhilstCalculationInProgress) {
            this.busyExpressionSource += ' || (isPrimary && calculationInProgress())';
        }
        this.busyExpression = this.setupExpression(this.busyExpressionSource,
            `${this.name} action button busy`, (busy: boolean) => {
                this.busy = busy;
            });

        if (this.triggerExpressionSource || this.autoTriggerExpressionSource) {
            this.setupAutoTriggering();
        }
    }

    private setupExpression(
        expressionSource: string,
        debugIdentifier: string,
        callback: (value: any) => void,
        trigger: boolean = true,
    ): Expression {
        if (expressionSource) {
            let expression: Expression = new Expression(
                expressionSource,
                this.expressionDependencies,
                debugIdentifier,
                this.expressionFixedArguments,
                this.expressionObservableArguments);
            expression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: any) => callback(value));
            if (trigger) {
                expression.triggerEvaluation();
            }
            return expression;
        }

        return null;
    }

    private setupAutoTriggering(): void {
        // if an auto trigger has already been registered for a button with this name (e.g in another widget location)
        // then we'll disable the auto trigger for this instance, since we don't want to trigger twice
        if (this.formService.isAutoTriggerActionButtonRegistered(this.name)) {
            this.canTrigger = false;
            return;
        }
        this.formService.registerAutoTriggerActionButton(this.name, this.workflowService.currentDestination.stepName);
        if (StringHelper.isNullOrEmpty(this.triggerConditionExpressionSource)) {
            this.canTrigger = true;
        } else {
            this.canTrigger = false;
            this.triggerConditionExpression = this.setupExpression(
                this.triggerConditionExpressionSource,
                `${this.name} action button auto trigger condition`,
                (conditionResult: boolean) => {
                    this.canTrigger = conditionResult;
                });
        }

        if (this.triggerExpressionSource) {
            this.autoTriggerExpression = this.setupExpression(
                this.triggerExpressionSource,
                `${this.name} action button trigger`,
                (result: any) => {
                    // This setTimeout is required so that all expressions
                    // can complete their reevaulation before triggering
                    // The click, otherwise the question set will be considered invalid and give a popup error.
                    setTimeout(() => {
                        this.onAutoTrigger();
                    }, 0);
                },
                false);
        }

        if (this.autoTriggerExpressionSource) {
            this.autoTriggerExpression = this.setupExpression(
                this.autoTriggerExpressionSource,
                `${this.name} action button auto trigger`,
                (autoTrigger: boolean) => {
                    if (autoTrigger) {
                        // This setTimeout is required so that all expressions
                        // can complete their reevaulation before triggering
                        // The click, otherwise the question set will be considered invalid and give a popup error.
                        setTimeout(() => {
                            this.onAutoTrigger();
                        }, 0);
                    }
                },
                true);
        }
    }

    protected onAutoTrigger(): void {
        if (this.canTrigger) {
            this.autoTriggered = true;
            this.onActionClick();
        }
    }

    public onActionClick(): void {
        this.workflowService.queueAction(
            this.name,
            this.location,
            this.expressionFixedArguments,
            this.expressionObservableArguments);
    }

    private disposeOfActionButtonExpressions(): void {
        if (this.hiddenExpression) {
            this.hiddenExpression.dispose();
        }
        if (this.autoTriggerExpression) {
            this.autoTriggerExpression.dispose();
        }
        if (this.triggerExpression) {
            this.triggerExpression.dispose();
        }
        if (this.triggerConditionExpression) {
            this.triggerConditionExpression.dispose();
        }
        if (this.disabledExpression) {
            this.disabledExpression.dispose();
        }
        if (this.busyExpression) {
            this.busyExpression.dispose();
        }
    }

    private listenForActionInProgress(): void {
        this.workflowStatusService.actionInProgressSubject.pipe(takeUntil(this.destroyed))
            .subscribe((actionInProgress: boolean) => {
                this.active = actionInProgress &&
                    this.workflowService.currentActionName == this.name;
            });
    }

    private generatePrefix(): string {
        return this.location
            ? this.location + '-'
            : _.uniqueId + '-';
    }

    private updateWidgetPositionFromLocation(): void {
        switch (this.location) {
            case 'calculationWidgetFooter':
                this.widgetPosition = 'sidebar';
                break;
            case 'formFooter':
                this.widgetPosition = 'footer';
                break;
        }
    }

    private evaluateWhetherToRender(): void {
        if (this.definition.locationsWhenPrimaryAndQuestionSetsValid.includes(this.location)) {
            this.render = !this.hidden && this.primary;
        } else {
            this.render = !this.hidden;
        }
        this.classes = this.render ? '' : 'no-display';
    }
}
