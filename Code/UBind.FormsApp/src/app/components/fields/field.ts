
import { debounceTime, takeUntil, filter, take } from 'rxjs/operators';
import { OnInit, OnDestroy, Output, EventEmitter, Directive } from '@angular/core';
import { FieldType as FormlyFieldType, FormlyFieldConfig } from '@ngx-formly/core';
import { FormService } from '@app/services/form.service';
import { WorkflowService } from '@app/services/workflow.service';
import { BehaviorSubject, Subject } from 'rxjs';
import { Expression, FixedArguments, ObservableArguments } from '@app/expressions/expression';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { CalculationService } from '@app/services/calculation.service';
import { CalculationAffectingField } from '@app/models/calculation-affecting-element';
import { FieldDataType } from '@app/models/field-data-type.enum';
import { QuestionsWidget } from '../widgets/questions/questions.widget';
import { ApplicationService } from '@app/services/application.service';
import { StringHelper } from '@app/helpers/string.helper';
import { BooleanHelper } from '@app/helpers/boolean.helper';
import { sentenceCase } from 'sentence-case';
import { Hideable } from '@app/models/hideable';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { FieldMetadataService } from '@app/services/field-metadata.service';
import { FieldType } from '@app/models/field-type.enum';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import {
    DataStoringFieldConfiguration,
} from '@app/resource-models/configuration/fields/data-storing-field.configuration';
import {
    InteractiveFieldConfiguration,
} from '@app/resource-models/configuration/fields/interactive-field.configuration';
import { EventService } from '@app/services/event.service';
import * as _ from 'lodash-es';
import { AnyHelper } from '@app/helpers/any.helper';
import { FieldEventLogRegistry } from '@app/services/debug/field-event-log-registry';
import { FieldHelper } from '@app/helpers/field.helper';
/**
 * Export field abstract class.
 * TODO: Write a better class header: field with setting up the expressions,
 * values and changes functions.
 */
@Directive()
export abstract class Field extends FormlyFieldType
    implements OnInit, OnDestroy, Hideable, CalculationAffectingField {

    protected _fieldId: string;
    public _initialValue: string = null;
    protected _previousValueChange: string;
    protected _previousValueKeyDown: string;
    protected _keyUpChangeEventTimeoutDelayMs: number = 1000;
    protected _keyUpChangeEventTimeoutId: any;
    protected _previousCalculatedTriggerValue: any;
    protected _valueChangedTimestamp: number;
    protected _valueChangeTimeoutDelay: number = 100;
    public fieldType: FieldType;
    protected calculatedTriggerExpression: Expression;
    protected calculatedConditionExpression: Expression;
    protected calculatedValueExpression: Expression;
    protected placeholderExpression: TextWithExpressions;
    protected _valueSubject: BehaviorSubject<any>;
    protected fieldPathForValueSubject: string;
    protected keyPressSubject: Subject<any> = new Subject<any>();
    public onTouchedSubject: Subject<any> = new Subject<any>();
    protected destroyed: Subject<void> = new Subject<void>();
    public valid: boolean = true;
    protected fieldValidSubject: BehaviorSubject<boolean>;
    protected useCalculatedValue: boolean = false;
    protected latestCalculatedValue: string = '';
    public hidden: boolean = false;
    private _reveal: boolean = true;
    public initialised: boolean = false;
    protected initialisationStarted: boolean = false;
    private ngOnInitCompleted: boolean = false;
    public defaultValue: string;
    public modelValue: string;
    public latestValue: any;
    public affectsPremium: boolean = false;
    public affectsTriggers: boolean = false;
    public requiredForCalculation: boolean = false;
    public parentQuestionsWidget: QuestionsWidget;
    private valueSubjectForExpressions: BehaviorSubject<any> = new BehaviorSubject<any>('');
    public expressionsFieldValue: any;
    public placeholderText: string;
    public recreateExpressionsSubject: Subject<void> = new Subject<void>();
    public revealSubject: Subject<boolean> = new Subject<boolean>();
    public fieldKey: string;
    protected disabledExpression: Expression;
    public disabled: boolean = false;
    protected readOnlyExpression: Expression;
    public readOnly: boolean = false;
    protected calculatedValueSubject: Subject<any>;
    private publishFieldValueUpdatesSubject: Subject<void>;
    private publishFieldValiditySubject: Subject<void>;

    /**
     * A field doesn't have layout if no bootstrap column width is set. HideWrapper will set this to false
     * if there is no bootstrap column set.
     */
    public hasLayout: boolean = true;

    /**
     * A map of validation "customExpression" where by the key is the expression source.
     * This is used since there could be multiple custom validation expressions per validation setting
     * 
     * ValidationService will create these on first use and then re-use them.
     */
    protected validationCustomExpressionMap: Map<string, Expression> = new Map<string, Expression>();

    /**
     * A map of validation "requiredExpression" where by the key is the expression source.
     * This is used since there could be multiple required validation expressions per validation setting
     * 
     * ValidationService will create these on first use and then re-use them.
     */
    protected validationRequiredExpressionMap: Map<string, Expression> = new Map<string, Expression>();

    /** When this fields validation state changes, it will trigger this
     * event to be emitted to the parent component, so it may know about it
     */
    @Output() public validationStateChanged: EventEmitter<boolean> = new EventEmitter<boolean>();

    private _field: FormlyFieldConfig;

    public dataStoringFieldConfiguration: DataStoringFieldConfiguration;
    public interactiveFieldConfiguration: InteractiveFieldConfiguration;

    public constructor(
        protected formService: FormService,
        protected workflowService: WorkflowService,
        protected expressionDependencies: ExpressionDependencies,
        protected calculationService: CalculationService,
        public applicationService: ApplicationService,
        protected fieldMetadataService: FieldMetadataService,
        protected eventService: EventService,
        protected fieldEventLogRegistry: FieldEventLogRegistry,
    ) {
        super();
        this.redefineFieldPropertyAsAccessors();
        this.valueSubjectForExpressions
            .subscribe((val: any) => this.expressionsFieldValue = val);
    }

    public ngOnInit(): void {
        this.fieldKey = <string>this.key;
        this.dataStoringFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.interactiveFieldConfiguration = this.field.templateOptions.fieldConfiguration;
        this.formControl['field'] = this;
        this.parentQuestionsWidget = this.form.root['questionSet'];
        if (!this.hidden) {
            this.initialiseField();
            this.onConfigurationUpdated();
        }
        this.ngOnInitCompleted = true;
    }

    public ngOnDestroy(): void {
        if (this.requiredForCalculation) {
            this.calculationService.deregisterFieldRequiredForCalculation(this);
        }
        if (!StringHelper.isNullOrEmpty(this.initialValue) && (this.affectsPremium || this.affectsTriggers)) {
            this.calculationService.deregisterFieldWithInitialValueAndAffectsPremiumOrAffectsTrigger(this);
        }
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
        if (this.field && this.field.templateOptions.componentRef) {
            delete this.field.templateOptions.componentRef;
        }
        this.destroyExpressions();
    }

    /**
     * Because formly is pretty inflexible, we need to be able to store a copy of the component reference
     * in templateOptions so it's available to wrappers. In Typescript 4, you can't overwrite a property
     * with an accessor, so we will have to do this in pure javascript.
     */
    private redefineFieldPropertyAsAccessors(): void {
        let descriptor: object = {
            get: this.getField,
            set: this.setField,
        };

        Object.defineProperty(this, 'field', descriptor);
    }

    /**
     * Overwrite the field property to allow us to set the component so that the 
     * HideWrapper can tell us when we are hidden.
     */
    protected setField(field: FormlyFieldConfig): void {
        this._field = field;
        this._field.templateOptions.componentRef = this;
        this.parentQuestionsWidget = this.form.root['questionSet'];
    }

    protected getField(): FormlyFieldConfig {
        return this._field;
    }

    public get valueSubject(): BehaviorSubject<any> {
        if (!this._valueSubject || this.fieldPath != this.fieldPathForValueSubject) {
            // store the field path for the field value subject, so that if the field path changes, 
            // we can know that we need to get a new fieldValueSubject
            this.fieldPathForValueSubject = this.fieldPath;
            const firstTime: boolean = !this._valueSubject;
            this._valueSubject = this.expressionDependencies.expressionInputSubjectService.getFieldValueSubject(
                this.fieldPath,
                this.latestValue ?? this.initialValue);
            this.valueSubjectForExpressions.subscribe(this._valueSubject);
            if (firstTime) {
                this.eventService.fieldPathAddedSubject.next(this.fieldPath);
            }
        }
        return this._valueSubject;
    }

    public publishValueForExpressions(value: any): void {
        if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
            console.log(`${this.fieldPath}: publishing value "${value}" for expressions.`);
        }
        this.latestValue = value;
        this.valueSubjectForExpressions.next(value);
    }

    protected get initialValue(): string {
        if (!this.initialisationStarted) {
            return null;
        }
        if (this._initialValue == null) {
            this._initialValue = '';
            this.modelValue = this.getModelValue();
            if (AnyHelper.hasValue(this.modelValue)) {
                this._initialValue = this.modelValue;
            } else {
                this.defaultValue = this.getDefaultValue();
                if (AnyHelper.hasValue(this.defaultValue)) {
                    this._initialValue = this.defaultValue;
                }
            }
            // ensure valueSubjectForExpressions gets the value, so that the fieldValue argment is available
            // right away to expressions.
            if (this._initialValue != null) {
                this.valueSubjectForExpressions.next(this._initialValue);
            }
        }
        return this._initialValue;
    }

    protected set initialValue(value: string) {
        this._initialValue = value;
    }

    /**
     * When the field is first rendered we need to do a number of things including:
     * - setup its expressions
     * - Set its default value
     * - Set its validity trackers
     * - Set up its subjects for expressions
     * - listen for keypresses
     * - listen to changes to its validity
     */
    protected initialiseField(): void {
        this.initialisationStarted = true;
        this.affectsPremium = this.dataStoringFieldConfiguration.affectsPremium;
        this.affectsTriggers = this.dataStoringFieldConfiguration.affectsTriggers;
        this.requiredForCalculation = this.dataStoringFieldConfiguration.requiredForCalculations;
        if (this.requiredForCalculation) {
            this.calculationService.registerFieldRequiredForCalculation(this);
        }
        this.setupExpressions();
        this.setFormControlValue(this.initialValue);
        this.onChange();
        this.valid = this.isConsideredValid();
        this.fieldValidSubject = this.expressionDependencies.expressionInputSubjectService.getFieldValidSubject(
            this.fieldPath,
            this.valid);
        if (!this.valid) {
            this.parentQuestionsWidget.childrenValidityTracker.onChildValidityChange(
                `${this.fieldType} ${this.fieldPath}`,
                this.valid);
        }
        this.listenForKeypressesAndTriggerOnChange();

        // listen to changes to the status (validity) of this field
        this.formControl.statusChanges
            .subscribe((newStatus: string) => this.onStatusChange(newStatus));
        this.initialised = true;
        if (!StringHelper.isNullOrEmpty(this.initialValue) && (this.affectsPremium || this.affectsTriggers)) {
            this.calculationService.registerFieldWithInitialValueAndAffectsPremiumOrAffectsTrigger(this);
        }
    }

    protected onConfigurationUpdated(): void {
        this.eventService.getFieldConfigUpdatedObservable(this.fieldKey).pipe(takeUntil(this.destroyed))
            .subscribe((configs: { old: FieldConfiguration; new: FieldConfiguration }) => {
                let oldDataStoringFieldConfiguration: DataStoringFieldConfiguration
                    = <DataStoringFieldConfiguration>configs.old;
                this.dataStoringFieldConfiguration = <DataStoringFieldConfiguration>configs.new;
                this.interactiveFieldConfiguration = <InteractiveFieldConfiguration>configs.new;
                this.affectsPremium = this.dataStoringFieldConfiguration.affectsPremium;
                this.affectsTriggers = this.dataStoringFieldConfiguration.affectsTriggers;
                this.requiredForCalculation = this.dataStoringFieldConfiguration.requiredForCalculations;
                if (this.requiredForCalculation && !oldDataStoringFieldConfiguration.requiredForCalculations) {
                    this.calculationService.registerFieldRequiredForCalculation(this);
                } else if (!this.requiredForCalculation
                    && oldDataStoringFieldConfiguration.requiredForCalculations
                ) {
                    this.calculationService.deregisterFieldRequiredForCalculation(this);
                }

                if (!StringHelper.isNullOrEmpty(this.initialValue) && (this.affectsPremium || this.affectsTriggers)) {
                    this.calculationService.
                        registerFieldWithInitialValueAndAffectsPremiumOrAffectsTrigger(this);
                } else {
                    this.calculationService
                        .deregisterFieldWithInitialValueAndAffectsPremiumOrAffectsTrigger(this);
                }

                // Update the validators in case they have changed
                if (!this.field.validators) {
                    this.formControl.clearValidators();
                } else {
                    this.formControl.setValidators(this.field.validators.validation);
                }
                this.setupExpressions();
                this.formControl.updateValueAndValidity({ onlySelf: true, emitEvent: true });

                // if affectsPremium or affectsTriggers has been set, then trigger a calculation
                if (this.valid
                    && ((this.affectsPremium && !oldDataStoringFieldConfiguration.affectsPremium)
                        || (this.affectsTriggers && !oldDataStoringFieldConfiguration.affectsTriggers))
                ) {
                    this.calculationService.generateQuoteRequest(this.affectsPremium, this.affectsTriggers);
                }

                // notify when tags change
                let tagsAdded: Array<string> = _.difference(configs.new.tags, configs.old.tags);
                if (tagsAdded.length) {
                    this.fieldMetadataService.onTagsAddedToField(this.fieldPath, tagsAdded);
                    this.expressionDependencies.taggedFieldsSubjectService
                        .onTagsAddedToField(this.fieldPath, tagsAdded);
                }
                let tagsRemoved: Array<string> = _.difference(configs.old.tags, configs.new.tags);
                if (tagsRemoved.length) {
                    this.fieldMetadataService.onTagsRemovedFromField(this.fieldPath, tagsRemoved);
                    this.expressionDependencies.taggedFieldsSubjectService
                        .onTagsRemovedFromField(this.fieldPath, tagsRemoved);
                }
            });
    }

    protected setFormControlValue(value: any): void {
        this.formControl.setValue(value);
    }

    private listenForKeypressesAndTriggerOnChange(): void {
        this.keyPressSubject.pipe(debounceTime(100), takeUntil(this.destroyed))
            .subscribe((event: any) => {
                this.onChange(event);
            });
    }

    public setHidden(hidden: boolean): void {
        const wasHidden: boolean = this.hidden;
        this.hidden = hidden;
        if (this.ngOnInitCompleted && !hidden && !this.initialisationStarted) {
            this.initialiseField();
            this.onConfigurationUpdated();
        } else if (this.initialised) {
            this.onChange();
        }
        if (wasHidden != hidden) {
            this.parentQuestionsWidget.onFieldValidityChange();
        }
    }

    /**
     * @returns true if this field is hidden OR it's parent question set is hidden
     */
    public isHidden(): boolean {
        return this.hidden || (this.parentQuestionsWidget && this.parentQuestionsWidget.isHidden());
    }

    public onParentHiddenChange(hidden: boolean): void {
        if (this.initialised) {
            this.onChange();
        }
    }

    protected setupExpressions(force: boolean = false): void {
        this.debounceFieldValueAndValidityUpdates();
        this.setupCalculatedConditionExpression(force);
        this.setupCalculatedValueExpression(force);
        this.setupCalculatedTriggerExpression(force);
        this.setupDisabledExpression(force);
        this.setupReadOnlyExpression(force);
        this.setupPlaceholderExpression(force);
        this.setupAutoTabExpression();
    }

    protected destroyExpressions(): void {
        this.calculatedConditionExpression?.dispose();
        this.calculatedConditionExpression = null;
        this.calculatedValueExpression?.dispose();
        this.calculatedValueExpression = null;
        this.calculatedTriggerExpression?.dispose();
        this.calculatedTriggerExpression = null;
        this.disabledExpression?.dispose();
        this.disabledExpression = null;
        this.readOnlyExpression?.dispose();
        this.readOnlyExpression = null;
        this.placeholderExpression?.dispose();
        this.placeholderExpression = null;
    }

    public setValue(value: any): void {
        if (this.dataStoringFieldConfiguration) {
            const dataType: any = this.dataStoringFieldConfiguration.dataType;
            if (dataType == FieldDataType.Boolean) {
                let newValue: boolean = BooleanHelper.fromAny(value);
                if (this.applicationService.debug && newValue != value) {
                    console.log(
                        `Field ${this.fieldPath} just received the value [${value}], `
                        + `and we converted it to [${newValue}], `
                        + `because this field has a Boolean data type.`);
                }
                value = newValue;
            } else if (dataType == FieldDataType.Attachment){
                if (value != null || value != '') {
                    console.error(
                        `Field ${this.fieldPath} just received the value [${value}], `
                        + `however attachment fields cannot accept values other than setting it to null,`
                        + `because this field has a Attachment data type.`);
                } else {
                    value = null;
                }
            }
        }

        if (!this.initialised) {
            this.initialValue = value;
        }
        this.setFormControlValue(value);
        this.onChange();
    }

    public clearValue(): void {
        if (this.getValueForExpressions() != '') {
            this.setValue('');
            this.onChange();
        }
    }

    public toggleValue(): void {
        this.formControl.setValue(!this.formControl.value);
    }

    protected setupCalculatedTriggerExpression(force: boolean = false): void {
        if (this.calculatedTriggerExpression) {
            if (!force && this.calculatedTriggerExpression.source
                == this.dataStoringFieldConfiguration.calculatedValueTriggerExpression) {
                // no need to destroy and recreate it if it hasn't changed.
                return;
            }
            this.calculatedTriggerExpression.dispose();
            this.calculatedTriggerExpression = null;
        }

        if (this.dataStoringFieldConfiguration.calculatedValueTriggerExpression) {
            this.calculatedTriggerExpression = new Expression(
                this.dataStoringFieldConfiguration.calculatedValueTriggerExpression,
                this.expressionDependencies,
                this.fieldPath + ' calculated trigger',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.calculatedTriggerExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: any) => {
                    if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                        console.log(`${this.fieldPath}: calculated trigger expression has triggered`);
                    }
                    if (this.useCalculatedValue && this.calculatedValueExpression) {
                        if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                            console.log(`${this.fieldPath}: `
                                + 'calculated trigger now causing calculated value to publish');
                        }
                        this.calculatedValueSubject.next(this.latestCalculatedValue);
                    } else {
                        if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                            console.log(`${this.fieldPath}: `
                                + 'calculated trigger not publishing new calculated value because the calculated '
                                + 'value condition is false');
                        }
                    }
                });

            // We will auto trigger the calculation upon first load by default if there is no setting.
            let autoTriggerCalculatedValue: boolean
                = this.dataStoringFieldConfiguration.autoTriggerCalculatedValue !== undefined
                    ? this.dataStoringFieldConfiguration.autoTriggerCalculatedValue
                    : true;

            // If the form control already has a value, then it means this isn't the first time it
            // was rendered, so we shouldn't auto trigger it's calculated value, unless it's been configured to allow
            // it to retrigger.
            const fieldHasPreviousValue: boolean = AnyHelper.hasValue(this.formControl.value);
            let allowCalculatedValueToAutoRetriggerWhenFieldHadPreviousValue: boolean
                = this.dataStoringFieldConfiguration
                    .allowCalculatedValueToAutoTriggerWhenFieldHadPreviousValue !== undefined
                    ? this.dataStoringFieldConfiguration.allowCalculatedValueToAutoTriggerWhenFieldHadPreviousValue
                    : false;

            if (autoTriggerCalculatedValue &&
                (!fieldHasPreviousValue || allowCalculatedValueToAutoRetriggerWhenFieldHadPreviousValue)
            ) {
                this.calculatedTriggerExpression.triggerEvaluation();
            }
        }
    }

    protected setupCalculatedConditionExpression(force: boolean = false): void {
        if (this.calculatedConditionExpression) {
            if (!force && this.calculatedConditionExpression.source
                == this.dataStoringFieldConfiguration.calculatedValueConditionExpression) {
                // no need to destroy and recreate it if it hasn't changed.
                return;
            }
            this.calculatedConditionExpression.dispose();
            this.calculatedConditionExpression = null;
        }

        if (this.dataStoringFieldConfiguration.calculatedValueConditionExpression) {
            this.calculatedConditionExpression = new Expression(
                this.dataStoringFieldConfiguration.calculatedValueConditionExpression,
                this.expressionDependencies,
                this.fieldPath + ' calculated condition',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.calculatedConditionExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((result: boolean) => {
                    if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                        console.log(`${this.fieldPath}: `
                            + `calculated condition expression resolved a new value of "${result}"`);
                    }
                    this.useCalculatedValue = result;
                });
            this.calculatedConditionExpression.triggerEvaluation();
        } else {
            this.useCalculatedValue = true;
        }
    }

    protected setupCalculatedValueExpression(force: boolean = false): void {
        if (this.calculatedValueExpression) {
            if (!force && this.calculatedValueExpression.source
                == this.dataStoringFieldConfiguration.calculatedValueExpression) {
                // no need to destroy and recreate it if it hasn't changed.
                return;
            }
            this.calculatedValueExpression.dispose();
            this.calculatedValueExpression = null;
        }

        if (this.dataStoringFieldConfiguration.calculatedValueExpression) {
            if (this.calculatedValueSubject == null) {
                this.calculatedValueSubject = new Subject<any>();
                this.handleCalculatedValues();
            }
            this.calculatedValueExpression = new Expression(
                this.dataStoringFieldConfiguration.calculatedValueExpression,
                this.expressionDependencies,
                this.fieldPath + ' calculated value',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.calculatedValueExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: any) => {
                    if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                        console.log(`${this.fieldPath}: `
                            + `calculated value expression resolved a new value of "${value}"`);
                    }
                    this.latestCalculatedValue = value;
                    if (this.useCalculatedValue
                        && !this.dataStoringFieldConfiguration.calculatedValueTriggerExpression
                    ) {
                        if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                            console.log(`${this.fieldPath}: `
                                + `publishing value "${value}"`);
                        }
                        this.calculatedValueSubject.next(value);
                    } else {
                        if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                            if (this.dataStoringFieldConfiguration.calculatedValueTriggerExpression) {
                                console.log(`${this.fieldPath}: `
                                    + `not publishing value "${value}" because there's a trigger expression`);
                            } else {
                                console.log(`${this.fieldPath}: `
                                    + `not publishing value "${value}" because there calculated condition is false`);
                            }
                        }
                    }
                });
            this.calculatedValueExpression.triggerEvaluation();
        }
    }

    protected setupDisabledExpression(force: boolean = false): void {
        if (this.disabledExpression) {
            if (!force
                && this.disabledExpression.source == this.interactiveFieldConfiguration.disabledConditionExpression) {
                // no need to destroy and recreate it if it hasn't changed.
                return;
            }
            this.disabledExpression.dispose();
            this.disabledExpression = null;
        }
        if (this.interactiveFieldConfiguration.disabledConditionExpression) {
            this.disabledExpression = new Expression(
                this.interactiveFieldConfiguration.disabledConditionExpression,
                this.expressionDependencies,
                this.fieldPath + ' disabled',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.disabledExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: any) => {
                    if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                        console.log(`${this.fieldPath}: `
                            + `disabled expression resolved a new value of "${value}"`);
                    }
                    if (value == true) {
                        this.disabled = true;
                        this.field.formControl.disable();
                    } else if (value == false) {
                        this.disabled = false;
                        this.field.formControl.enable();
                    } else {
                        throw new Error(`Received a result from the disabledExpression `
                            + `that was not "true" or "false". It was "${value}".`);
                    }
                });
            this.disabledExpression.triggerEvaluationWhenFormLoaded();
        } else if (this.field.formControl.disabled) {
            this.field.formControl.enable();
        }
    }

    protected setupReadOnlyExpression(force: boolean = false): void {
        if (this.readOnlyExpression) {
            if (!force
                && this.readOnlyExpression.source == this.interactiveFieldConfiguration.readOnlyConditionExpression) {
                // no need to destroy and recreate it if it hasn't changed.
                return;
            }
            this.readOnlyExpression.dispose();
            this.readOnlyExpression = null;
        }
        if (this.interactiveFieldConfiguration.readOnlyConditionExpression) {
            this.readOnlyExpression = new Expression(
                this.interactiveFieldConfiguration.readOnlyConditionExpression,
                this.expressionDependencies,
                this.fieldPath + ' read only',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.readOnlyExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: any) => {
                    if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                        console.log(`${this.fieldPath}: `
                            + `readonly expression resolved a new value of "${value}"`);
                    }
                    if (value == true) {
                        this.readOnly = true;
                    } else if (value == false) {
                        this.readOnly = false;
                    } else {
                        throw new Error(`Received a result from the readOnlyExpression `
                            + `that was not "true" or "false". It was "${value}".`);
                    }
                });
            this.readOnlyExpression.triggerEvaluationWhenFormLoaded();
        } else {
            this.readOnly = false;
        }
    }

    protected setupPlaceholderExpression(force: boolean = false): void {
        if (this.placeholderExpression) {
            if (!force && this.placeholderExpression.source == this.interactiveFieldConfiguration.placeholder) {
                // no need to destroy and recreate it if it hasn't changed.
                return;
            }

            this.placeholderExpression.dispose();
            this.placeholderExpression = null;
        }

        if (this.interactiveFieldConfiguration.placeholder) {
            this.placeholderExpression = new TextWithExpressions(
                this.interactiveFieldConfiguration.placeholder,
                this.expressionDependencies,
                this.fieldPath + ' placeholder',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope);
            this.placeholderExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: any) => {
                    if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
                        console.log(`${this.fieldPath}: `
                            + `placeholder expression resolved a new value of "${text}"`);
                    }
                    this.placeholderText = text;
                });
            this.placeholderExpression.triggerEvaluation();
        } else {
            this.applyDefaultPlaceholderText();
        }
    }

    private setupAutoTabExpression(): void {
        if (!this.interactiveFieldConfiguration.autoTabExpression){
            return;
        }
        const autoTabExpression: string = this.interactiveFieldConfiguration.autoTabExpression;
        const publishEvenIfNotChanged: boolean = this.dataStoringFieldConfiguration.dataType != FieldDataType.Boolean;
        let expression: Expression = new Expression(
            autoTabExpression,
            this.expressionDependencies,
            `${this.fieldKey} auto-tab-expression`,
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope,
            publishEvenIfNotChanged);
        expression.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe((shouldAutoTab: boolean) => {
                if(shouldAutoTab){
                    this.findNextInputElementAndFocus();
                }
            });
        expression.triggerEvaluation();
    }

    private findNextInputElementAndFocus(): void {
        this.eventService.fieldValuesAndValiditiesAreStableSubject
            .pipe(
                debounceTime(10),
                filter((stable: boolean) => stable),
                take(1))
            .subscribe(async (stable: boolean) => {
                if (stable && this.isConsideredValid()) {
                    const targetFocusElement: HTMLElement =
                        FieldHelper.getNextElementToFocus(this.fieldPath, this.fieldType);
                    if (targetFocusElement){
                        if (FieldHelper.isElementVisible(targetFocusElement)) {
                            targetFocusElement.focus();
                        } else {
                            await FieldHelper.delayFocusUntilVisible(targetFocusElement);
                        }
                    }
                }
            });
    }

    protected applyDefaultPlaceholderText(): void {
        this.placeholderText = null;
    }

    protected getDefaultValue(): string {
        if (!StringHelper.isNullOrEmpty(this.dataStoringFieldConfiguration.defaultValueExpression)) {
            return new Expression(
                this.dataStoringFieldConfiguration.defaultValueExpression,
                this.expressionDependencies,
                this.fieldPath + ' default value',
                this.getFixedExpressionArguments(),
                this.getObservableExpressionArguments(),
                this.scope,
            ).evaluateAndDispose();
        }
        return null;
    }

    public onKeyDown(event: any): void {
        this.keyPressSubject.next(event);
    }

    public onKeyUp(event: any): void {
        // ignoring this since the value change actually happens on key down not up
    }

    public onChange(event: any = null): void {
        if (this.getValueForExpressions() !== this.valueSubject.value) {
            if (!this.dataStoringFieldConfiguration.debounceFieldValueUpdates) {
                this.publishChange();
            } else {
                this.formService.setFieldValueUnstable(this.fieldPath);
                this.publishFieldValueUpdatesSubject.next();
            }
        }
    }

    public debounceFieldValueAndValidityUpdates(): void {
        if (this.dataStoringFieldConfiguration.debounceFieldValueUpdates
            && !this.publishFieldValueUpdatesSubject
        ) {
            this.publishFieldValueUpdatesSubject = new Subject<void>();
            this.publishFieldValiditySubject = new Subject<void>();
            this.publishFieldValueUpdatesSubject.pipe(
                takeUntil(this.destroyed),
                debounceTime(this.dataStoringFieldConfiguration.fieldValueUpdateDebounceMilliseconds ?? 500),
            ).subscribe(() => {
                this.publishChange();
                this.formService.setFieldValueUnstable(this.fieldPath, false);
            });

            this.publishFieldValiditySubject.pipe(
                takeUntil(this.destroyed),
                debounceTime(this.dataStoringFieldConfiguration.fieldValueUpdateDebounceMilliseconds ?? 500),
            ).subscribe(() => {
                this.publishValidity();
                this.formService.setFieldValidityUnstable(this.fieldPath, false);
            });
        }
    }

    public publishChange(): void {
        this.formService.clearValueCache();
        const value: any = this.getValueForExpressions();
        this.publishValueForExpressions(value);
        if (this.parentQuestionsWidget && this.parentQuestionsWidget.isRepeating) {
            // trigger that the value of the repeating field has changed so that expressions know to re-evaluate
            this.expressionDependencies.expressionInputSubjectService.getFieldValueSubject(
                this.parentQuestionsWidget.parentFieldPath, '');
        }
        if (this.initialised && this.valid) {
            if ((this.affectsPremium || this.affectsTriggers)
                && (!this.parentQuestionsWidget.isRepeating || this.parentQuestionsWidget.repeatingFieldIsComplete)) {
                this.calculationService.generateQuoteRequest(this.affectsPremium, this.affectsTriggers);
            } else {
                this.parentQuestionsWidget.onFieldValidValueChange();
            }
        }
    }

    protected getValueForExpressions(): any {
        if (this.isHidden()) {
            return '';
        }
        if (this.formControl.value === null || this.formControl.value === undefined) {
            return '';
        }
        return this.formControl.value;
    }

    /**
     * This function is called when the validity of the field changes
     * @param status 
     */
    protected onStatusChange(status: string = null): void {
        if (this.initialised && this.isConsideredValid() != this.valid) {
            this.valid = this.isConsideredValid();
            if (!this.dataStoringFieldConfiguration.debounceFieldValueUpdates) {
                this.publishValidity();
            } else {
                this.formService.setFieldValidityUnstable(this.fieldPath);
                this.publishFieldValiditySubject.next();
            }
            this.parentQuestionsWidget.childrenValidityTracker.onChildValidityChange(this.fieldPath, this.valid);
            this.parentQuestionsWidget.onFieldValidityChange();
        }
    }

    private publishValidity(): void {
        if (this.fieldEventLogRegistry.fieldPaths.includes(this.fieldPath)) {
            console.log(`${this.fieldPath}: `
                + `publishing field validity "${this.valid}" for expressions`);
        }
        this.fieldValidSubject.next(this.valid);
    }

    /**
     * returns true if the form element is considerd invalid as per the rules of uBind.
     * A field which is not considered invalid is considered valid.
     * Note that a field which is disabled according to reactive forms would not be considered valid, 
     * nor would it be considered invalid.
     */
    protected isConsideredValid(): boolean {
        return !this.formControl.invalid || this.isHidden();
    }

    public onFocus(event: any): void {
        this.formService.formFocus.emit(this.fieldType);
    }

    public onBlur(event: any): void {
        this.formService.fieldBlur.emit(this.fieldType);

        // required field validation error does not trigger statusChanged event
        // so we will subscribe to onblur event and check the status of the formControl instead.
        this.formControl.markAsTouched();
        this.onTouchedSubject.next();
    }

    protected valueHasChangedSinceLastKeyDownEvent(): boolean {
        if (this.formControl.value != this._previousValueKeyDown) {
            if ((this.formControl.value == null || this.formControl.value == '') &&
                (this._previousValueKeyDown == null || this._previousValueKeyDown == '')) {
                return false;
            }
            this._previousValueKeyDown = this.formControl.value;
            return true;
        } else {
            return false;
        }
    }

    protected getModelValue(): string {
        return this.model ? this.model[<string>this.key] : null;
    }

    protected getFormControl(): any {
        if (this.fieldKey.indexOf('[') != -1) {
            let repeatingFieldName: string = this.fieldKey.substring(0, this.fieldKey.indexOf('['));
            let repeatingSetIndex: string
                = this.fieldKey.substring(this.fieldKey.indexOf('[') + 1, this.fieldKey.indexOf(']'));
            let fieldName: string = this.fieldKey.substring(this.fieldKey.indexOf('.') + 1, this.fieldKey.length);
            if (this.form.controls[repeatingFieldName] &&
                this.form.controls[repeatingFieldName]['controls'][repeatingSetIndex] &&
                this.form.controls[repeatingFieldName]['controls'][repeatingSetIndex].controls[fieldName]) {
                return this.form.controls[repeatingFieldName]['controls'][repeatingSetIndex].controls[fieldName];
            } else {
                return null;
            }
        } else {
            return this.form.controls[this.fieldKey];
        }
    }

    /**
     * Gets or creates a custom validation expression. 
     * If the expression is created it is also triggered to be evaluated.
     * Subsequent updates to the expression will call updateValuAndValidty(), however
     * the first one won't, since this is called from within a validation execution 
     * the first time, so if the first evaluation also triggered a validation then we'd have
     * two calls to validation where only one is required.
     * @param expressionSource 
     * @param errorMessage 
     */
    public getOrCreateCustomValidationExpression(expressionSource: string, errorMessage: string): Expression {
        let expression: Expression = this.validationCustomExpressionMap.get(expressionSource);
        if (!expression) {
            let debugIdentifier: string = this.fieldPath + ` custom validation with error message "${errorMessage}"`;
            expression = this.createValidationExpression(expressionSource, debugIdentifier);
            this.validationCustomExpressionMap.set(expressionSource, expression);
            expression.triggerEvaluation();
        }
        return expression;
    }

    public getOrCreateRequiredValidationExpression(expressionSource: string): Expression {
        let expression: Expression = this.validationRequiredExpressionMap.get(expressionSource);
        if (!expression) {
            let debugIdentifier: string = this.fieldPath + ` required validation`;
            expression = this.createValidationExpression(expressionSource, debugIdentifier);
            this.validationRequiredExpressionMap.set(expressionSource, expression);
            expression.triggerEvaluation();
        }
        return expression;
    }

    private createValidationExpression(expressionSource: string, debugIdentifier: string): Expression {
        let expression: Expression = new Expression(
            expressionSource,
            this.expressionDependencies,
            debugIdentifier,
            this.getFixedExpressionArguments(),
            this.getObservableExpressionArguments(),
            this.scope);
        let firstEvaluation: boolean = true;
        expression.nextResultObservable.pipe(takeUntil(this.destroyed))
            .subscribe((valid: boolean) => {
                if (firstEvaluation) {
                    firstEvaluation = false;
                } else {
                    if ((this.useCalculatedValue && this.calculatedValueExpression)
                        && !this.field.formControl.touched
                    ) {
                        this.field.formControl.markAsTouched();
                    }

                    this.formControl.updateValueAndValidity({ onlySelf: true, emitEvent: true });
                }
            });

        return expression;
    }

    /**
     * @returns the arguments to be passed to an expression which are obserables 
     * (because the value of the argument can change over time).
     * 
     * Can be overriden by child classes to provide additional observable arguments.
     * For example the search select may want to also provide the current search term 
     * so it can be used in expressions.
     */
    public getObservableExpressionArguments(): ObservableArguments {
        return {
            fieldValue: this.valueSubjectForExpressions.asObservable(),
        };
    }

    public getFixedExpressionArguments(): FixedArguments {
        return { fieldKey: this.fieldKey, fieldPath: this.fieldPath };
    }

    public get data(): any {
        return this.formService.getFieldData(this.fieldPath);
    }

    public set data(data: any) {
        this.formService.setFieldData(this.fieldPath, data);
    }

    /**
     * Field path has the following structure:
     * 
     * If it's in a root question set, then it's just the field key, e.g. "contactName"
     * If it's in a repeating question set, then it's {repeatingQuestionSetFieldKey}[{index}].{fieldKey}
     * e.g. "drivers[0].name"
     * 
     * If it's in a repating question set inside a repeating question set it would be:
     * e.g. "drivers[0].claims[0].amount"
     * 
     */
    public get fieldPath(): string {
        return this.scope != ''
            ? this.scope + "." + <string>this.key
            : <string>this.key;
    }

    public get scope(): string {
        return this.parentQuestionsWidget && this.parentQuestionsWidget.fieldPath != ''
            ? this.parentQuestionsWidget.fieldPath
            : '';
    }

    public get questionSetPath(): string {
        return this.parentQuestionsWidget.questionSetPath;
    }

    public get ariaLabelByKey(): string {
        return sentenceCase(this.fieldKey);
    }

    /**
     * Recreate the expressions associated with fields in this question set.
     * This would normally only be called if the question set was part of a repeating set and one was removed.
     */
    public recreateExpressions(): void {
        this.setupExpressions(true);
        this.validationCustomExpressionMap.clear();
        this.validationRequiredExpressionMap.clear();
        this.fieldValidSubject = this.expressionDependencies.expressionInputSubjectService.getFieldValidSubject(
            this.fieldPath,
            this.valid);
        this.recreateExpressionsSubject.next();
    }

    public set reveal(reveal: boolean) {
        this._reveal = reveal;
        this.revealSubject.next(reveal);
    }

    public get reveal(): boolean {
        return this._reveal;
    }

    private handleCalculatedValues(): void {
        this.calculatedValueSubject.pipe(takeUntil(this.destroyed)).subscribe((value: any) => {
            if (this.valueSubject.value !== value || this.valueSubjectForExpressions.value !== value
                || this.formControl.value !== value
            ) {
                this.setValue(value);
            }
        });
    }
}
