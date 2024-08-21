import {
    Component, OnInit, ViewChildren, QueryList, ElementRef, ChangeDetectorRef, AfterViewInit, OnDestroy,
} from '@angular/core';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { Field } from '../field';
import { ConfigService } from '@app/services/config.service';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { QuestionsWidget } from '../../widgets/questions/questions.widget';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { takeUntil } from 'rxjs/operators';
import { Expression, FixedArguments } from '@app/expressions/expression';
import { BehaviorSubject } from 'rxjs';
import { CalculationService } from '@app/services/calculation.service';
import { ChildrenValidityTracker } from '@app/components/widgets/children-validity-tracker';
import { AbstractControl } from '@angular/forms';
import { UnifiedFormModelService } from '@app/services/unified-form-model.service';
import { ApplicationService } from '@app/services/application.service';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { RepeatingFieldDisplayMode } from './repeating-field-display-mode.enum';
import { RepeatingQuestionSetTrackingService } from '@app/services/repeating-question-set-tracking.service';
import { Errors } from '@app/models/errors';
import { MatchingFieldsSubjectService } from '@app/expressions/matching-fields-subject.service';
import { WorkflowDestination } from '@app/models/workflow-destination';
import * as _ from 'lodash-es';
import { NavigationDirection } from '@app/models/navigation-direction.enum';
import { RepeatingFieldConfiguration } from '@app/resource-models/configuration/fields/repeating-field.configuration';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { EventService } from '@app/services/event.service';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';
import { FieldConfigurationHelper } from '@app/helpers/field-configuration.helper';

/**
 * Holds the stateful properties of a repeating question set.
 */
export interface RepeatingInstance {
    index: number;
    animationState: string;
    deleted?: boolean;
    heading: string;
    removeButtonLabel: string;
    headingExpression: TextWithExpressions;
    reveal: boolean;
    validExpression: Expression;
}

/**
 * Export repeating field component class.
 * TODO: Write a better class header: repeating field component functions.
 */
@Component({
    selector: '' + FieldSelector.Repeating,
    templateUrl: './repeating.field.html',
    animations: [
        trigger('itemSlide', [
            state('in', style({ height: '*' })),
            state('animate-in', style({ height: '*' })),
            state('out', style({ height: 0 })),
            state('animate-out', style({ height: 0 })),
            transition('* => animate-in', [
                animate('200ms ease-in-out'),
            ]),
            transition('* => animate-out', [
                animate('300ms 200ms ease-in-out'),
            ]),
        ]),
        trigger('itemFade', [
            state('in', style({ opacity: 100 })),
            state('animate-in', style({ opacity: 100 })),
            state('out', style({ opacity: 0 })),
            state('animate-out', style({ opacity: 0 })),
            transition('* => animate-in', [
                animate('100ms 200ms ease-in-out'),
            ]),
            transition('* => animate-out', [
                animate('100ms ease-in-out'),
            ]),
        ]),
        trigger('buttonSlide', [
            state('in', style({ height: '*' })),
            state('out', style({ height: 0 })),
            transition('out => in', [
                animate('300ms ease-in-out'),
            ]),
            transition('in => out', [
                animate('300ms 300ms ease-in-out'),
            ]),
        ]),
        trigger('buttonFade', [
            state('in', style({ opacity: 100 })),
            state('out', style({ opacity: 100 })),
            transition('out => in', [
                animate('5300ms 300ms ease-in-out'),
            ]),
            transition('in => out', [
                animate('5300ms ease-in-out'),
            ]),
        ]),
    ],
})

export class RepeatingField extends Field implements OnInit, OnDestroy, AfterViewInit {

    protected readonly DefaultMinQuantity: number = 0;

    @ViewChildren(QuestionsWidget)
    protected questionsWidgets: QueryList<QuestionsWidget>;
    private removalIndex: number;
    private needsReorderAfterRemoval: boolean;

    public repeatingInstances: Array<RepeatingInstance> = new Array<RepeatingInstance>();
    public isComplete: boolean = true;

    private _minQuantity: number = this.DefaultMinQuantity;
    private _maxQuantity: number;

    public showRemoveItemButton: boolean = false;
    public showAddItemButton: boolean = false;
    public addButtonLabel: string;
    public addButtonLabelExpression: TextWithExpressions;
    public addButtonIcon: string;
    public removeButtonIcon: string;

    public childrenValidityTracker: ChildrenValidityTracker = new ChildrenValidityTracker();

    private repeatingCountSubject: BehaviorSubject<number>;

    public displayMode: RepeatingFieldDisplayMode;
    // eslint-disable-next-line @typescript-eslint/naming-convention
    public RepeatingFieldDisplayMode: any = RepeatingFieldDisplayMode;
    public currentRepeatingInstanceIndex: number = 0;

    public isWithinRevealGroup: boolean = false;
    public fullyRevealed: boolean = true;
    public repeatingFieldConfiguration: RepeatingFieldConfiguration;

    private displayModeExpression: Expression;
    private minQuantityExpression: Expression;
    private maxQuantityExpression: Expression;

    private tempFieldPath: string;

    public constructor(
        protected configService: ConfigService,
        protected windowScroll: WindowScrollService,
        protected formService: FormService,
        protected workflowService: WorkflowService,
        public element: ElementRef,
        protected expressionDependencies: ExpressionDependencies,
        calculationService: CalculationService,
        private unifiedFormModelService: UnifiedFormModelService,
        applicationService: ApplicationService,
        fieldMetadataService: FieldMetadataService,
        protected repeatingQuestionSetTrackingService: RepeatingQuestionSetTrackingService,
        private matchingFieldsSubjectService: MatchingFieldsSubjectService,
        eventService: EventService,
        private changeDetectorRef: ChangeDetectorRef,
        fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        super(
            formService,
            workflowService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry);
        this.fieldType = FieldType.Repeating;
        this.listenForChildrenValidityChange();
    }

    public ngOnInit(): void {
        this.tempFieldPath = this.fieldPath;
        this.repeatingFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        super.ngOnInit();
        this.setButtonIcons();
    }

    public ngOnDestroy(): void {
        // ensure each of the question sets are marked as no longer visible.
        this.repeatingInstances.forEach((repeatingInstance: RepeatingInstance, index: number) => {
            let questionWidgets: Array<QuestionsWidget> = this.questionsWidgets.toArray();
            if (questionWidgets.length > 0 && questionWidgets[index]) {
                let questionWidget: QuestionsWidget = questionWidgets[index];
                if (!questionWidget.deleted) {
                    this.formService.setQuestionSetVisible(questionWidget.questionSetPath, false);
                }
            }
        });
        super.ngOnDestroy();
    }

    protected destroyExpressions(): void {
        this.displayModeExpression?.dispose();
        this.displayModeExpression = null;
        this.minQuantityExpression?.dispose();
        this.minQuantityExpression = null;
        this.maxQuantityExpression?.dispose();
        this.maxQuantityExpression = null;
        this.addButtonLabelExpression?.dispose();
        this.addButtonLabelExpression = null;
        super.destroyExpressions();
    }

    protected initialiseField(): void {
        this.initialisationStarted = true;
        this.formControl['field'] = this;
        this.parentQuestionsWidget = this.form.root['questionSet'];

        this.setupDisplayMode();
        this.setupMinQuantityExpression();
        this.setupMaxQuantityExpression();
        this.generateItems();
        this.deleteExtraFieldValuesFromModel();
        this.fieldValidSubject = this.expressionDependencies.expressionInputSubjectService.getFieldValidSubject(
            this.fieldPath,
            this.childrenValidityTracker.valid);

        // listen to changes to the status (validity) of this field
        this.formControl.statusChanges.subscribe((newStatus: string) => this.onStatusChange(newStatus));

        this.setupValidator();
        this.repeatingCountSubject = this.expressionDependencies.expressionInputSubjectService
            .getFieldRepeatingCountSubject(this.fieldPath, this.hidden ? 0 : this.repeatingInstances.length);
        this.repeatingQuestionSetTrackingService.setRepeatingInstanceCount(
            this.fieldPath,
            this.repeatingInstances.length);
        this.repeatingQuestionSetTrackingService.setCurrentRepeatingFieldPath(this.fieldPath);
        if (this.displayMode == RepeatingFieldDisplayMode.Instance) {
            this.setInitialRepeatingInstanceIndex();
            this.listenToRepeatingInstanceIndexChanges();
        }
        this.displayCorrectAddRemoveButtons();
        this.listenToRepeatingInstanceValueChanges();
        this.initialised = true;
    }

    public ngAfterViewInit(): void {
        this.onRenderedQuestionWidgetsChanges();
    }

    protected onConfigurationUpdated(): void {
        super.onConfigurationUpdated();
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration}) => {
                this.repeatingFieldConfiguration = <RepeatingFieldConfiguration>configs.new;
                this.setupDisplayMode();
                this.setupMinQuantityExpression();
                this.setupMaxQuantityExpression();
                this.generateItems();
                this.repeatingQuestionSetTrackingService.setRepeatingInstanceCount(
                    this.fieldPath,
                    this.repeatingInstances.length);
                this.repeatingQuestionSetTrackingService.setCurrentRepeatingFieldPath(this.fieldPath);
                if (this.displayMode == RepeatingFieldDisplayMode.Instance) {
                    this.setInitialRepeatingInstanceIndex();
                }
                this.displayCorrectAddRemoveButtons();
                this.setButtonIcons();
            });
    }

    private listenToRepeatingInstanceIndexChanges(): void {
        this.repeatingQuestionSetTrackingService.repeatingFieldChangeSubject.pipe(
            takeUntil(this.destroyed),
        ).subscribe(() => {
            this.currentRepeatingInstanceIndex
                = this.repeatingQuestionSetTrackingService.getCurrentRepeatingInstanceIndex(this.fieldPath);
        });
    }

    private listenToRepeatingInstanceValueChanges() {
        this.expressionDependencies.expressionInputSubjectService.getFieldValueObservable(this.fieldPath)
            .pipe(takeUntil(this.destroyed))
            .subscribe(() => this.evaluateRepeatingFieldIsComplete());
    }

    private evaluateRepeatingFieldIsComplete() {
        let formModel: any = this.formService.getValues(true, true, false, false);
        let repeatingInstancesValues: any = formModel[this.fieldPath];
        if(!repeatingInstancesValues) {
            this.isComplete = true;
            return;
        }
        let repeatingQuestionSetFields: any =
            this.configService.configuration.repeatingQuestionSets[this.fieldPath][0].fieldGroup;
        for (const value of repeatingInstancesValues) {
            if (value == null || Object.keys(value).length === 0) {
                continue;
            }
            for (const repeatingQuestionField of repeatingQuestionSetFields) {
                if (FieldConfigurationHelper.isInteractiveField(
                    repeatingQuestionField.templateOptions.fieldConfiguration)) {
                    let isRequired: boolean =
                        repeatingQuestionField.templateOptions.fieldConfiguration.required;
                    if (isRequired && !value[repeatingQuestionField.key]) {
                        this.isComplete = false;
                        return;
                    }
                }
            }
        }
        this.isComplete = true;
    }

    private setButtonIcons(): void {
        this.addButtonIcon = this.configService.textElements?.formElements?.repeatingAddItemButton?.icon;
        this.removeButtonIcon = this.configService.textElements?.formElements?.repeatingRemoveItemButton?.icon;
    }

    private setInitialRepeatingInstanceIndex(): void {
        let workflowDestination: WorkflowDestination = _.clone(this.applicationService.currentWorkflowDestination);
        if (workflowDestination.repeatingInstanceIndex === undefined
            || workflowDestination.repeatingInstanceIndex < 0
        ) {
            if (!this.applicationService.lastNavigationDirection
                || this.applicationService.lastNavigationDirection == NavigationDirection.Forward
            ) {
                workflowDestination.repeatingInstanceIndex =
                    this.repeatingQuestionSetTrackingService.getFirstRepeatingInstanceIndex(this.fieldPath);
            } else if (this.applicationService.lastNavigationDirection == NavigationDirection.Backward) {
                workflowDestination.repeatingInstanceIndex =
                    this.repeatingQuestionSetTrackingService.getLastRepeatingInstanceIndex(this.fieldPath);
            }
            this.currentRepeatingInstanceIndex = workflowDestination.repeatingInstanceIndex;
            this.repeatingQuestionSetTrackingService.setCurrentRepeatingInstanceIndex(
                this.fieldPath,
                this.currentRepeatingInstanceIndex);
            this.applicationService.currentWorkflowDestination = workflowDestination;
        } else {
            this.currentRepeatingInstanceIndex =
                this.repeatingQuestionSetTrackingService.getCurrentRepeatingInstanceIndex(this.fieldPath);
        }
    }

    /**
     * sets up a validator that marks this field as valid if all of the question sets are valid
     */
    private setupValidator(): void {
        const formControl: AbstractControl = this.form.controls[this.fieldKey];
        if (formControl) {
            formControl.setValidators(formControl.validator ? [
                formControl.validator,
                this.childrenValidCountValidator.bind(this),
            ] : [
                this.childrenValidCountValidator.bind(this),
            ]);
        }
    }

    private generateItems(): void {
        let numberOfItemsToDisplay: number = this.getNumberOfItemsToDisplay();
        for (let i: number = 0; i < numberOfItemsToDisplay; i++) {
            if (this.repeatingInstances.length < i + 1) {
                let repeatingInstance: RepeatingInstance = this.generateRepeatingInstance(i);
                this.repeatingInstances.push(repeatingInstance);
            }
        }
        this.updateAddButtonLabel();
    }

    private generateRepeatingInstance(index: number): RepeatingInstance {
        let repeatingInstance: RepeatingInstance = {
            index: index,
            animationState: 'in',
            heading: `${this.repeatingFieldConfiguration.repeatingInstanceName} ${index + 1}`,
            removeButtonLabel: null,
            headingExpression: null,
            reveal: true,
            validExpression: null,
        };
        this.setupRepeatingInstanceValidExpression(repeatingInstance);
        this.setupRepeatingInstanceHeading(repeatingInstance);
        this.setupRemoveButtonLabel(repeatingInstance);
        return repeatingInstance;
    }

    public get minQuantity(): number {
        return this._minQuantity;
    }

    public set minQuantity(quantity: number) {
        this._minQuantity = quantity;
        this.repeatingQuestionSetTrackingService.setRepeatingFieldMinQuantity(this.fieldPath, quantity);
    }

    public get maxQuantity(): number {
        return this._maxQuantity;
    }

    public set maxQuantity(quantity: number) {
        this._maxQuantity = quantity;
        this.repeatingQuestionSetTrackingService.setRepeatingFieldMaxQuantity(this.fieldPath, quantity);
    }

    private getNumberOfItemsToDisplay(): number {
        let model: object =
            this.unifiedFormModelService.workingFormModel.getOrCreateObjectAtPathWithinUnifiedFormModel(
                this.parentQuestionsWidget.fieldPath);
        let modelQty: number = 0;
        if (model[this.fieldKey] && Array.isArray(model[this.fieldKey])) {
            modelQty = model[this.fieldKey].length;
        }
        let trackedQty: number = this.repeatingQuestionSetTrackingService.getRepeatingInstanceCount(this.fieldPath);
        return Math.max(this.minQuantity, Math.min(this.maxQuantity, Math.max(trackedQty, modelQty)));
    }

    private listenForChildrenValidityChange(): void {
        this.childrenValidityTracker.validObservable
            .subscribe((valid: boolean) => this.formControl.updateValueAndValidity());
    }

    /**
     * Update revealed instances when a repeating instance changes it's validity
     */
    private setupRepeatingInstanceValidExpression(repeatingInstance: RepeatingInstance): void {
        repeatingInstance.validExpression = new Expression(
            `questionSetIsValid('${this.getQuestionSetPathBase()}[${repeatingInstance.index}]')`,
            this.expressionDependencies,
            `repeating instance ${repeatingInstance.index} valid`);
        repeatingInstance.validExpression.nextResultObservable.subscribe((valid: boolean) => {
            this.updateRevealedInstances();
        });
        repeatingInstance.validExpression.triggerEvaluation();
    }

    private setupRepeatingInstanceHeading(repeatingInstance: RepeatingInstance): void {
        if (this.repeatingFieldConfiguration) {
            if (this.repeatingFieldConfiguration.displayRepeatingInstanceHeading === false) {
                repeatingInstance.heading = null;
            } else if (this.repeatingFieldConfiguration.repeatingInstanceHeading) {
                let fixedArgs: FixedArguments = this.getFixedExpressionArguments();
                fixedArgs['fieldPath'] = `${this.fieldPath}[${repeatingInstance.index}]`;
                if (repeatingInstance.headingExpression) {
                    repeatingInstance.headingExpression.dispose();
                }
                repeatingInstance.headingExpression = new TextWithExpressions(
                    this.repeatingFieldConfiguration.repeatingInstanceHeading,
                    this.expressionDependencies,
                    `${this.fieldPath} repeating instance ${repeatingInstance.index} heading`,
                    fixedArgs,
                    this.getObservableExpressionArguments(),
                    this.scope);
                repeatingInstance.headingExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((text: string) => repeatingInstance.heading = text);
                repeatingInstance.headingExpression.triggerEvaluation();
            } else {
                repeatingInstance.heading
                    = `${this.repeatingFieldConfiguration.repeatingInstanceName} ${repeatingInstance.index + 1}`;
            }
        }
    }

    private setupRemoveButtonLabel(repeatingInstance: RepeatingInstance): void {
        if (this.repeatingFieldConfiguration && this.repeatingFieldConfiguration.removeRepeatingInstanceButtonLabel) {
            let fixedArgs: FixedArguments = this.getFixedExpressionArguments();
            fixedArgs['fieldPath'] = `${this.fieldPath}[${repeatingInstance.index}]`;
            let expression: TextWithExpressions = new TextWithExpressions(
                this.repeatingFieldConfiguration.removeRepeatingInstanceButtonLabel,
                this.expressionDependencies,
                `${this.fieldPath} repeating instance ${repeatingInstance.index} remove button label`,
                fixedArgs,
                this.getObservableExpressionArguments(),
                this.scope);
            expression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: string) => repeatingInstance.removeButtonLabel = text);
            expression.triggerEvaluation();
            return;
        }

        // fallback code for when somone hasn't configured their remove button labels, they get the default.
        let removeText: string
            = this.configService.textElements?.formElements?.repeatingRemoveItemButton?.text ?? 'Remove';
        repeatingInstance.removeButtonLabel
            = `${removeText} this ${this.repeatingFieldConfiguration.repeatingInstanceName}`;
    }

    private updateAddButtonLabel(): void {
        if (this.repeatingFieldConfiguration) {
            const addButtonLabelSource: string = this.repeatingInstances.length == 0
                ? this.repeatingFieldConfiguration.addFirstRepeatingInstanceButtonLabel
                : this.repeatingFieldConfiguration.addRepeatingInstanceButtonLabel;
            if (addButtonLabelSource) {
                if (this.addButtonLabelExpression) {
                    this.addButtonLabelExpression.dispose();
                }
                this.addButtonLabelExpression = new TextWithExpressions(
                    addButtonLabelSource,
                    this.expressionDependencies,
                    `${this.fieldPath} repeating add button label`,
                    this.getFixedExpressionArguments(),
                    this.getObservableExpressionArguments(),
                    this.scope);
                this.addButtonLabelExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                    .subscribe((text: string) => this.addButtonLabel = text);
                this.addButtonLabelExpression.triggerEvaluation();
                return;
            }
        }

        // fallback code for when somone hasn't configured their add button labels, they get the default.
        let addText: string = this.configService.textElements?.formElements?.repeatingAddItemButton?.text ?? 'Add';
        if (this.repeatingInstances.length == 0) {
            this.addButtonLabel = `${addText} `;
            const nameFirstChar: string
                = this.repeatingFieldConfiguration.repeatingInstanceName.substr(0, 1).toLowerCase();
            const articleText: string = ['a', 'e', 'i', 'o', 'u'].indexOf(nameFirstChar) !== -1 ? 'an' : 'a';
            this.addButtonLabel += `${articleText} ${this.repeatingFieldConfiguration.repeatingInstanceName }`;
        } else {
            this.addButtonLabel = `${addText} another ${this.repeatingFieldConfiguration.repeatingInstanceName }`;
        }
    }

    private setupDisplayMode(): void {
        if (this.repeatingFieldConfiguration.displayModeExpression) {
            if (this.displayModeExpression) {
                this.displayModeExpression.dispose();
            }
            this.displayModeExpression = new Expression(
                this.repeatingFieldConfiguration.displayModeExpression,
                this.expressionDependencies,
                this.fieldPath + ' display mode',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.displayModeExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: string) => {
                    if (!Object.values(RepeatingFieldDisplayMode).includes(<any>result)) {
                        throw Errors.General.InvalidEnumValue(
                            'displayMode',
                            result,
                            RepeatingFieldDisplayMode,
                            `displayModeExpression for repeating field ${this.fieldPath}`);
                    }
                    this.displayMode = <any>result;
                    this.repeatingQuestionSetTrackingService
                        .setRepeatingFieldDisplayMode(this.fieldPath, this.displayMode);
                });
            this.displayModeExpression.triggerEvaluation();
        } else {
            this.displayMode = this.repeatingFieldConfiguration.displayMode || RepeatingFieldDisplayMode.List;
            this.repeatingQuestionSetTrackingService
                .setRepeatingFieldDisplayMode(this.fieldPath, this.displayMode);
        }
    }

    protected setupMinQuantityExpression(): void {
        if (this.repeatingFieldConfiguration.minimumQuantityExpression == null) {
            this.minQuantity = this.DefaultMinQuantity;
            return;
        }
        if (this.minQuantityExpression) {
            this.minQuantityExpression.dispose();
        }
        this.minQuantityExpression = new Expression(
            this.repeatingFieldConfiguration.minimumQuantityExpression,
            this.expressionDependencies,
            this.fieldKey + ' minimum quantity',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        this.minQuantityExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe((result: any) => {
                let newMinQuantity: number = parseInt(result, 10);
                if (isNaN(newMinQuantity)) {
                    newMinQuantity = 0;
                }
                // TODO: consider whether this field being hidden should affect min quantity
                if (newMinQuantity < this.DefaultMinQuantity) {
                    newMinQuantity = this.DefaultMinQuantity;
                }
                if (this.minQuantity != newMinQuantity) {
                    this.minQuantity = newMinQuantity;
                    if (this.initialised) {
                        while (this.repeatingInstances.length < this.minQuantity) {
                            this.addItem();
                        }
                    }
                }
                if (this.initialised) {
                    this.displayCorrectAddRemoveButtons();
                }
            });
        this.minQuantityExpression.triggerEvaluation();
    }

    protected setupMaxQuantityExpression(): void {
        if (!this.repeatingFieldConfiguration.maximumQuantityExpression) {
            this.maxQuantity = this.configService.configuration.repeatingInstanceMaxQuantity;
            return;
        }
        if (this.maxQuantityExpression) {
            this.maxQuantityExpression.dispose();
        }
        this.maxQuantityExpression = new Expression(
            this.repeatingFieldConfiguration.maximumQuantityExpression,
            this.expressionDependencies,
            this.fieldKey + ' maximum quantity',
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        this.maxQuantityExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe((result: any) => {
                let newMaxQuantity: number = parseInt(result, 10);
                if (!isNaN(newMaxQuantity)) {
                    if (newMaxQuantity < this.minQuantity) {
                        newMaxQuantity = this.minQuantity;
                    }
                    if (this.maxQuantity != newMaxQuantity) {
                        this.maxQuantity = newMaxQuantity;
                        if (this.initialised) {
                            while (this.repeatingInstances.length > this.maxQuantity) {
                                // this remove is not animated
                                this.removeItem(this.repeatingInstances.length - 1);
                            }
                        }
                    }
                }
                if (this.initialised) {
                    this.displayCorrectAddRemoveButtons();
                }
            });
        this.maxQuantityExpression.triggerEvaluation();
    }

    protected displayCorrectAddRemoveButtons(): void {
        this.updateAddButtonLabel();
        if (this.repeatingInstances.length > this.minQuantity) {
            this.showRemoveItemButton = true;
        } else {
            this.showRemoveItemButton = false;
        }
        if (this.repeatingInstances.length < this.maxQuantity) {
            if (this.displayMode == RepeatingFieldDisplayMode.List) {
                this.showAddItemButton = true;
            } else if (this.displayMode == RepeatingFieldDisplayMode.Instance) {
                this.showAddItemButton = this.repeatingInstances.length == 0
                    || this.currentRepeatingInstanceIndex == this.repeatingInstances.length - 1;
            }
        } else {
            this.showAddItemButton = false;
        }
    }

    /**
     * Publishes the number of repeated question sets via the repeatingCoundSubject.
     * Note that if this field is hidden then it will publish the number 0.
     */
    public publishRepeatingCount(): void {
        let count: number = this.repeatingInstances.filter((qs: RepeatingInstance) => !qs.deleted).length;
        this.repeatingQuestionSetTrackingService.setRepeatingInstanceCount(this.fieldPath, count);
        this.repeatingCountSubject.next(this.isHidden() ? 0 : this.repeatingInstances.length);
    }

    /**
     * adds an item and makes it animate in
     */
    public animateInItem(): void {
        // if the display mode is "Instance", you can't add another instance unless this one is valid:
        if (this.displayMode == RepeatingFieldDisplayMode.Instance) {
            if (!this.formService.questionSetIsValid(this.questionSetPath)) {
                setTimeout(() => {
                    this.formService.markAllInvalidFieldsTouched([this.questionSetPath]);
                    let scrolled: boolean =
                        this.formService.scrollToFirstInvalidField([this.questionSetPath]);
                    if (!scrolled) {
                        throw new Error(`Could not scroll to invalid field in invalid question set `
                            + `"${this.questionSetPath}".`);
                    }
                }, 0);
                return;
            }
        }
        let repeatingInstance: RepeatingInstance = this.generateRepeatingInstance(this.repeatingInstances.length);
        repeatingInstance.animationState = 'animate-in';
        this.repeatingInstances.push(repeatingInstance);
        this.publishFieldInstanceChange();
        if (this.displayMode == RepeatingFieldDisplayMode.Instance && this.repeatingInstances.length > 1) {
            let destination: WorkflowDestination = _.clone(this.applicationService.currentWorkflowDestination);
            destination.repeatingInstanceIndex
                = this.repeatingQuestionSetTrackingService.getNextRepeatingInstanceIndex(this.fieldPath);
            this.workflowService.navigateTo(destination);
        }

        // update revealed instances after it's had a chance to render and know if it's valid
        setTimeout(() => this.updateRevealedInstances());
    }

    /**
     * animates out an item then removes it,
     * scrolling to the the item above.
     */
    public animateOutItem(index: number): void {
        this.repeatingInstances[index].animationState = 'animate-out';
    }

    /**
     * adds an item without animating it in
     */
    protected addItem(): void {
        let repeatingInstance: RepeatingInstance = this.generateRepeatingInstance(this.repeatingInstances.length);
        repeatingInstance.animationState = 'in';
        this.repeatingInstances.push(repeatingInstance);
        this.publishFieldInstanceChange();

        // update revealed instances after it's had a chance to render and know if it's valid
        setTimeout(() => this.updateRevealedInstances());
    }

    /**
     * remove's an item without animating it out
     */
    protected removeItem(index: number): void {
        let questionsWidgets: Array<QuestionsWidget> = this.questionsWidgets.toArray();
        let questionsWidgetToDelete: QuestionsWidget = this.displayMode == RepeatingFieldDisplayMode.Instance
            ? questionsWidgets[0]
            : questionsWidgets[index];
        questionsWidgetToDelete.deleted = true;
        this.unifiedFormModelService.workingFormModel
            .deleteQuestionSetModelFromUnifiedFormModel(questionsWidgetToDelete.fieldPath);
        this.unifiedFormModelService.strictFormModel
            .deleteQuestionSetModelFromUnifiedFormModel(questionsWidgetToDelete.fieldPath);

        // deregister the last visible question set
        this.formService.deregisterQuestionSet(questionsWidgets[questionsWidgets.length - 1]);

        // remove the last visible question set from the validity map
        this.formService.removeQuestionSetValidity(questionsWidgets[questionsWidgets.length - 1].questionSetPath);

        // delete the last question set field values for expressions. We will republish other ones further below.
        let fieldPathsToDelete: Array<string> = this.matchingFieldsSubjectService
            .getFieldPathsMatchingPattern(`${this.fieldPath}[${this.repeatingInstances.length - 1}]`);
        for (let fieldPath of fieldPathsToDelete) {
            // publish an empty value
            this.expressionDependencies.expressionInputSubjectService.getFieldValueSubject(fieldPath, null);
            this.expressionDependencies.expressionInputSubjectService.getFieldValidSubject(fieldPath, null);
            this.expressionDependencies.expressionInputSubjectService.getFieldSearchTermSubject(fieldPath, null);

            // signal that it was removed
            this.eventService.fieldPathRemovedSubject.next(fieldPath);

            // delete the field subjects so they're not re-used with the wrong fieldpath
            this.expressionDependencies.expressionInputSubjectService.removeFieldValueSubject(fieldPath);
            this.expressionDependencies.expressionInputSubjectService.removeFieldValidValueSubject(fieldPath);
            this.expressionDependencies.expressionInputSubjectService.deleteFieldSearchTermSubject(fieldPath);
            this.formService.deleteFieldData(fieldPath);
        }

        // delete the visible status for the last question set, since we have one less visible question set
        let lastQuestionSetPath: string = questionsWidgets[questionsWidgets.length - 1].questionSetPath;
        this.formService.setQuestionSetVisible(lastQuestionSetPath, false);

        // delete the question set at the removal index
        let deletedLastInstance: boolean = index == this.repeatingInstances.length - 1;
        this.repeatingInstances.splice(index, 1);
        this.removalIndex = index;
        this.needsReorderAfterRemoval = this.displayMode == RepeatingFieldDisplayMode.List;
        for (let i: number = index; i < this.repeatingInstances.length; i++) {
            this.repeatingInstances[i].index = i;
            this.setupRepeatingInstanceHeading(this.repeatingInstances[i]);
        }
        this.updateRevealedInstances();

        if (this.displayMode == RepeatingFieldDisplayMode.List) {
            // have it update the list of rendered questions widget by calling change detection
            this.changeDetectorRef.markForCheck();
        } else if (this.displayMode == RepeatingFieldDisplayMode.Instance) {
            // we need to update the republish field values for expressions now, without the widgets doing it since
            // they aren't even rendered.
            this.republishFieldValuesFromFormModel(index);

            // if we deleted the last one, then navigate backwards
            if (deletedLastInstance && this.removalIndex > 0) {
                let destination: WorkflowDestination = _.clone(this.applicationService.currentWorkflowDestination);
                destination.repeatingInstanceIndex
                    = this.repeatingQuestionSetTrackingService.getPreviousRepeatingInstanceIndex(this.fieldPath);
                this.workflowService.navigateTo(destination);
            }

            // publish the repeating count and so forth
            this.publishFieldInstanceChange();
        }
    }

    private onRenderedQuestionWidgetsChanges(): void {
        this.questionsWidgets.changes.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                if (this.needsReorderAfterRemoval) {
                    // update all of the field value subjects after the removal index
                    // because their fieldPaths have now changed.
                    let questionsWidgetsArray: Array<QuestionsWidget> = this.questionsWidgets.toArray();
                    for (let i: number = this.removalIndex; i < questionsWidgetsArray.length; i++) {
                        questionsWidgetsArray[i].onRepeatingInstanceReorder();
                    }
                    this.needsReorderAfterRemoval = false;
                    this.removalIndex = -1;
                    this.publishFieldInstanceChange();
                }
                this.displayCorrectAddRemoveButtons();
            });
    }

    private republishFieldValuesFromFormModel(startingIndex: number): void {
        // Get the values from the form model
        let instanceValuesArray: Array<object> = <Array<object>>this.unifiedFormModelService.workingFormModel
            .getOrCreateObjectAtPathWithinUnifiedFormModel(this.fieldPath);

        // Get the subject from expression input subject service
        for (let i: number = startingIndex; i < instanceValuesArray.length; i++) {
            // publish the value
            let instanceValues: object = instanceValuesArray[i];
            for (let propertyName of Object.getOwnPropertyNames(instanceValues)) {
                let propertyFieldPath: string = `${this.fieldPath}[${i}].${propertyName}`;
                this.expressionDependencies.expressionInputSubjectService
                    .getFieldValueSubject(propertyFieldPath).next(instanceValues[propertyName]);
            }
        }
    }

    // this was likely to be removed once UB-7231 is merged to develop.
    private deleteExtraFieldValuesFromModel(): void {
        const model: object =
            this.unifiedFormModelService.workingFormModel.getOrCreateObjectAtPathWithinUnifiedFormModel(
                this.parentQuestionsWidget.fieldPath);
        const arrayField: object = model[this.fieldKey];
        if (arrayField && Array.isArray(arrayField)) {
            while (arrayField.length > this.maxQuantity) {
                const index: number = arrayField.length - 1;
                const path: string = `${this.fieldKey}[${index}]`;
                this.unifiedFormModelService.workingFormModel.deleteQuestionSetModelFromUnifiedFormModel(path);
                this.unifiedFormModelService.strictFormModel.deleteQuestionSetModelFromUnifiedFormModel(path);
            }
        }
    }

    protected onCompletedTransition(index: number): void {
        if (this.repeatingInstances[index]) {
            if (this.repeatingInstances[index].animationState == 'animate-out') {
                setTimeout(
                    () => {
                        this.repeatingInstances[index].animationState = 'out';
                        this.removeItem(index);
                        this.displayCorrectAddRemoveButtons();
                    }, 0);
            } else if (this.repeatingInstances[index].animationState == 'animate-in') {
                setTimeout(
                    () => {
                        this.displayCorrectAddRemoveButtons();
                        // disabling scroll due to jerkiness. We probably need a smarter solution
                        // that only scrolls when more than 50% of the item is not in view.
                        /*
                        this.scrollToItem(index);
                        */
                        this.repeatingInstances[index].animationState = 'in';
                    }, 0);
            }
        }
    }

    protected scrollToItem(index?: number, screenLocation: string = 'middle'): void {
        let element: any;
        if (index == null || index < 0) {
            element = this.element.nativeElement.firstChild;
        } else {
            if (index >= this.repeatingInstances.length) {
                index = this.repeatingInstances.length;
            }
            element = document.getElementById(this.fieldKey + index);
        }

        if (element) {
            this.windowScroll.scrollElementIntoView(element);
        }
    }

    /**
     * Get's the form field values for each of the question sets repeated.
     */
    public getValues(includeEmptyValues: boolean = true): Array<object> {
        let result: Array<object> = new Array<object>();
        this.questionsWidgets.forEach((qs: QuestionsWidget) => result.push(qs.getValues(includeEmptyValues)));
        return result;
    }

    public childrenValidCountValidator(control: AbstractControl): { [key: string]: boolean } | null {
        if (!this.childrenValidityTracker.valid) {
            return { childrenInvalid: true };
        }
        return null;
    }

    public markAllInvalidFieldsTouched(): void {
        for (let questionSet of this.questionsWidgets.toArray()) {
            questionSet.markAllInvalidFieldsTouched();
        }
    }

    /**
     * iterates through the question sets in order and attempts to scroll to the first invalid field.
     */
    public scrollToFirstInvalidField(): boolean {
        let scrolled: boolean = false;
        for (let questionSet of this.questionsWidgets.toArray()) {
            scrolled = questionSet.scrollToFirstInvalidField();
            if (scrolled) {
                return true;
            }
        }
        return false;
    }

    public setHidden(hidden: boolean): void {
        this.repeatingInstances.forEach((repeatingInstance: RepeatingInstance, index: number) => {
            let questionWidgets: Array<QuestionsWidget> = this.questionsWidgets.toArray();
            if (questionWidgets.length > 0 && questionWidgets[index]) {
                let questionWidget: QuestionsWidget = questionWidgets[index];
                if (!questionWidget.deleted) {
                    if (hidden
                        || this.displayMode != RepeatingFieldDisplayMode.Instance
                        || this.currentRepeatingInstanceIndex == index
                    ) {
                        this.formService.setQuestionSetVisible(questionWidget.questionSetPath, !hidden);
                    }
                    if (hidden) {
                        questionWidget.clearValues();
                        questionWidget.reset();
                    }
                }
            }
        });
        super.setHidden(hidden);
    }

    public onChange(event: any = null): void {
        // do nothing since the repeating field itself is not supposed to have a value
    }

    public setValue(value: any): void {
        // do nothing since the repeating field itself is not supposed to have a value
    }

    public clearValue(): void {
        this.questionsWidgets.forEach((qs: QuestionsWidget) => qs.clearValues());
        super.clearValue();
    }

    private publishFieldInstanceChange(): void {
        this.publishRepeatingCount();
        this.publishChange();
    }

    public updateRevealedInstances(): void {
        if (this.isWithinRevealGroup) {
            let stillValid: boolean = true;
            for (let repeatingInstance of this.repeatingInstances) {
                repeatingInstance.reveal = stillValid;
                if (!stillValid) {
                    continue;
                }
                const questionSetPath: string = this.getQuestionSetPathBase() + '[' + repeatingInstance.index + ']';
                if (this.formService.questionSetIsInvalid(questionSetPath)) {
                    stillValid = false;
                }
            }
            if (stillValid != this.fullyRevealed) {
                this.fullyRevealed = stillValid;
            }
        }
    }

    public getQuestionSetPathBase(): string {
        let parentSegmentOrEmpty: string = this.parentQuestionsWidget
            ? this.parentQuestionsWidget.questionSetPath + '.'
            : '';
        return parentSegmentOrEmpty + this.fieldKey;
    }
}
