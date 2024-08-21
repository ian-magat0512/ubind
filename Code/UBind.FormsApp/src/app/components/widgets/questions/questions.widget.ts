import {
    Component, OnInit, OnDestroy, Input, Output, EventEmitter,
    ElementRef, AfterViewInit, HostBinding, OnChanges, SimpleChanges,
} from '@angular/core';
import { FormGroup } from '@angular/forms';
import { FormlyFieldConfig } from '@ngx-formly/core';
import { Form } from '../../fields/form';
import { Widget } from '../widget';
import { ConfigService } from '@app/services/config.service';
import { CalculationService, WatchedField } from '@app/services/calculation.service';
import { WorkflowService } from '@app/services/workflow.service';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { FormService } from '@app/services/form.service';
import { Subject } from 'rxjs';
import { ExpressionInputSubjectService } from '@app/expressions/expression-input-subject.service';
import { Field } from '@app/components/fields/field';
import { CalculationAffectingQuestionSet } from '@app/models/calculation-affecting-element';
import { ChildrenValidityTracker } from '../children-validity-tracker';
import { distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { RepeatingField } from '@app/components/fields/repeating/repeating.field';
import * as _ from 'lodash-es';
import { UnifiedFormModelService } from '@app/services/unified-form-model.service';
import { Expression } from '@app/expressions/expression';
import { ApplicationService } from '@app/services/application.service';
import { StringHelper } from '@app/helpers/string.helper';
import { Hideable } from '@app/models/hideable';
import { FieldType } from '@app/models/field-type.enum';
import { FieldSelector } from '@app/models/field-selectors.enum';
import { ArticleElement } from '@app/models/configuration/workflow-step';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { VisibleFieldConfiguration } from '@app/resource-models/configuration/fields/visible-field.configuration';
import { RevealGroup as RevealGroup } from './reveal-group';
import { TypeHelper } from '@app/helpers/type.helper';
import { EventService } from '@app/services/event.service';
import { QuestionSetConfigSyncHelper, QuestionSetSyncResults } from '@app/helpers/question-set-config-sync.helper';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { FieldDataType } from '@app/models/field-data-type.enum';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { SectionDisplayMode } from '@app/models/section-display-mode.enum';

/**
 * Export question widget component class.
 * This class manag the questions sets of the producs.
 */
@Component({
    selector: 'questions-widget',
    templateUrl: './questions.widget.html',
})
export class QuestionsWidget extends Widget
    implements OnInit, OnChanges, OnDestroy, CalculationAffectingQuestionSet, Hideable, AfterViewInit {

    @HostBinding('class.no-layout') public hasNoLayout: boolean = false;

    @Input('definition')
    protected definition: ArticleElement;

    @Input('name')
    public name: string;

    /**
     * If the question set has the affectsPremium flag (as set in the workflow.json)
     * then this will be set to true.
     * A question set which has affectsPremium set will cause a new calculation request
     * each time question in it changes, only when all fields/questionSets marked as requiredForCalculation
     * are valid.
     */
    @Input('affectsPremium')
    public affectsPremium: boolean = false;

    /**
     * If the question set has the affectsPremium flag (as set in the workflow.json)
     * then this will be set to true.
     */
    @Input('affectsTriggers')
    public affectsTriggers: boolean = false;

    /**
     * If the question set has the requiredForCalculation flag (as set in the workflow.json)
     * then this will be set to true.
     * If true then all fields in this question set must be valid for a calculation to happen.
     */
    @Input('requiredForCalculation')
    public requiredForCalculation: boolean = false;

    /**
     * The name of the step/page this question set belongs to
     */
    @Input('stepName')
    public stepName: string;

    /**
     * The index of the article within the step
     */
    @Input('articleIndex')
    public articleIndex: string;

    @Input('articleElementIndex')
    public articleElementIndex: string;

    @Input('sectionDisplayMode')
    public sectionDisplayMode: SectionDisplayMode;

    @Input('visible')
    public visible: boolean;

    /** Only used if this is within a repeating field */
    @Input('isRepeating')
    public isRepeating: boolean = false;

    /** Only used if this is within a repeating field */
    @Input('repeatingFieldIsComplete')
    public repeatingFieldIsComplete: boolean = true;

    /** Only used if this is within a repeating field */
    @Input('parentKey')
    protected parentKey: string;

    /** Only used if this is within a repeating field */
    @Input('parentFieldPath')
    public parentFieldPath: string;

    @Input('parentFieldChildrenValidityTracker')
    protected parentFieldChildrenValidityTracker: ChildrenValidityTracker;

    /**
     * If this is a repeating question set and we've just deleted it, we mark this flag so that
     * we don't copy it's values into the unified form model
     */
    public deleted: boolean = false;

    /**
     * Only used if this is within a repeating field
     */
    @Input('repeatingIndex')
    protected repeatingIndex: number;

    @Input('parentQuestionsWidget')
    protected parentQuestionsWidget: QuestionsWidget;

    @Input('parentHideable')
    protected parentHideable: Hideable;

    /**
     * We need to subscribe to this to know when this questions widget becomes hidden,
     * so that we can clear the values from this question set and the form model.
     */
    @Input('hiddenExpression')
    protected hiddenExpression: Expression;

    @Output() public formValueChanges: EventEmitter<any> = new EventEmitter<any>();
    @Output() public formKeyDown: EventEmitter<any> = new EventEmitter<any>();
    @Output() public formFocus: EventEmitter<any> = new EventEmitter<any>();
    @Output() public formBlur: EventEmitter<any> = new EventEmitter<any>();
    @Output() public formStatusChanges: EventEmitter<any> = new EventEmitter<any>();
    @Output() public fieldValidityChange: EventEmitter<void> = new EventEmitter<void>();

    public form: Form = new Form({});
    public model: object;
    public options: any = {};
    public fieldsByRow: Array<FormlyFieldConfig>;
    protected deregistered: boolean = false;
    protected previousStatus: string;
    protected validSubject: Subject<boolean>;
    public debug: boolean = false;
    public childrenValidityTracker: ChildrenValidityTracker = new ChildrenValidityTracker();
    protected hidden: boolean = false;
    private revealGroups: Array<RevealGroup> = new Array<RevealGroup>();
    private fullyRevealed: boolean = true;

    public constructor(
        public element: ElementRef,
        protected windowScroll: WindowScrollService,
        protected configService: ConfigService,
        protected formService: FormService,
        protected expressionInputSubjectService: ExpressionInputSubjectService,
        protected workflowService: WorkflowService,
        private calculationService: CalculationService,
        private unifiedFormModelService: UnifiedFormModelService,
        public applicationService: ApplicationService,
        private workflowStatusService: WorkflowStatusService,
        private eventService: EventService,
        private revealGroupTrackingService: RevealGroupTrackingService,
    ) {
        super();
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.prepareFieldDefinitions();
        this.loadFormModel();
        this.form.registerQuestionSet(this);
        this.formService.registerQuestionSet(this);
        this.updateRequiredForCalculation();
        if (this.requiredForCalculation) {
            this.calculationService.registerQuestionSetRequiredForCalculation(this);
        }
        this.validSubject = this.expressionInputSubjectService.getQuestionSetValidSubject(this.questionSetPath);
        this.handleBeingHidden();
    }

    public ngAfterViewInit(): void {
        this.createRevealGroups();
        this.formService.setQuestionSetValidity(this.questionSetPath, this.valid);

        this.validSubject.next(this.valid);

        this.handleValidityChanges();
        if (!this.valid) {
            this.notifyParentOfValidityChange(this.valid);
        }
        this.formService.setQuestionSetVisible(this.questionSetPath, !this.isHidden());
        this.onFieldValidityChange();
        this.removeLayoutIfNoFieldsHaveLayout();
        this.onConfigurationUpdated();
        if (this.calculationService.shouldPerformInitialCalculation()) {
            let fieldsWithInitialValueAndAffectsPremiumOrTriggers: Array<WatchedField> =
                this.calculationService.getFieldsWithInitialValueAndAffectsPremiumOrTriggers();

            this.generateQuoteRequest(
                fieldsWithInitialValueAndAffectsPremiumOrTriggers[0].affectsPremium,
                fieldsWithInitialValueAndAffectsPremiumOrTriggers[0].affectsTriggers,
                200);
        }
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.prepareFieldDefinitions();
            });
    }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes.requiredForCalculation?.currentValue != changes.requiredForCalculation?.previousValue) {
            this.updateRequiredForCalculation();
        }
    }

    private updateRequiredForCalculation(): void {
        if (this.name == 'ratingPrimary') {
            this.requiredForCalculation = true;
        }
        if (this.requiredForCalculation) {
            this.calculationService.registerQuestionSetRequiredForCalculation(this);
        } else {
            this.calculationService.deregisterQuestionSetRequiredForCalculation(this);
        }
    }

    public ngOnDestroy(): void {
        if (!this.deleted) {
            this.patchQuestionSetModelIntoUnifiedFormModels();
        } else {
            this.deregisterChildrenRequiredForCalculation();
            this.calculationService.deregisterFieldRequiredForCalculation(this);
        }

        // Retain the paymentOption question set in all the step
        if (this.questionSetPath !== 'paymentOptions') {
            this.formService.removeQuestionSetValidity(this.questionSetPath);
            this.formService.deregisterQuestionSet(this);
        }

        this.formService.setQuestionSetVisible(this.questionSetPath, false);
        this.deregistered = true;
        super.ngOnDestroy();
        if (this.parentFieldChildrenValidityTracker) {
            if (!this.valid) {
                this.parentFieldChildrenValidityTracker
                    .onChildValidityChange(`QuestionsWidget ${this.questionSetPath}`, true);
            }
        }

        if (this.form) {
            this.form.deregisterQuestionSet();
        }
    }

    /**
     * We need to create reveal groups to know which groups of fields should be revealed together, so that
     * field groups can be revealed one by one as each prior one becomes valid.
     */
    private createRevealGroups(): void {
        const stepUsesProgressiveReveal: boolean
            = this.revealGroupTrackingService.doesStepUseProgressiveReveal(this.stepName);
        let revealGroups: Array<RevealGroup> = new Array<RevealGroup>();
        let revealGroup: RevealGroup = null;
        let firstField: boolean = true;
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            let fieldConfiguration: VisibleFieldConfiguration = field.field.templateOptions.fieldConfiguration;
            if ((fieldConfiguration && fieldConfiguration.startsNewRevealGroup)
                || (firstField && stepUsesProgressiveReveal)
            ) {
                if (revealGroup) {
                    revealGroups.push(revealGroup);
                }
                revealGroup = new RevealGroup();
            }
            if (revealGroup) {
                revealGroup.fields.push(field);
                if (TypeHelper.isRepeatingField(field)) {
                    field.isWithinRevealGroup = true;
                    field.updateRevealedInstances();
                }
            }
            firstField = false;
        }
        if (revealGroup) {
            revealGroups.push(revealGroup);
        }
        this.revealGroups = revealGroups;
        if (this.revealGroups.length) {
            this.revealGroupTrackingService.registerStepUsingProgressiveReveal(this.stepName);
        }
    }

    private handleBeingHidden(): void {
        if (this.hiddenExpression) {
            this.hiddenExpression.nextResultObservable.pipe(
                takeUntil(this.destroyed),
                // Once we start navigating out, we don't want to respond to hiding situations
                // because the process of navigating out would hide question sets and then
                // cause their data to be wiped unintentionally in the unified form model.
                takeUntil(this.workflowStatusService.navigatingOutStarted),
            ).subscribe((hidden: boolean) => this.setHidden(hidden));
        }
    }

    /**
     * Loads the form model for this question set. This is a subset of the data from the working form model.
     * This comes either from a json file stored along with the product, 
     * or from existing form data from a previous form fill.
     */
    private loadFormModel(): void {
        this.model = this.unifiedFormModelService.workingFormModel.getOrCreateFormModelForQuestionSet(this.fieldPath);
    }

    /**
     * Gets an array of all of the field keys for the fields in this question set
     */
    private getFieldKeys(): Array<string> {
        let keys: Array<string> = new Array<string>();
        if (this.fieldsByRow) {
            for (let fieldGroupContainer of this.fieldsByRow) {
                for (let fieldItem of fieldGroupContainer.fieldGroup) {
                    keys.push(<string>fieldItem.key);
                }
            }
        }
        return keys;
    }

    private getRepeatingFieldKeys(): Array<string> {
        let keys: Array<string> = new Array<string>();
        if (this.fieldsByRow) {
            for (let fieldGroupContainer of this.fieldsByRow) {
                for (let fieldItem of fieldGroupContainer.fieldGroup) {
                    if (fieldItem.type == FieldSelector.Repeating) {
                        keys.push(<string>fieldItem.key);
                    }
                }
            }
        }
        return keys;
    }

    private getWebhookFieldKeys(): Array<string> {
        let keys: Array<string> = new Array<string>();
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field?.fieldType == FieldType.Webhook) {
                keys.push(controlName);
            }
        }

        return keys;
    }

    public getFieldKeysThatAreContentTypeOrWithNoDataType(): Array<string> {
        return Object.values(this.controls)
            .filter((formControl: any) => {
                let field: Field = formControl.field;
                return field?.interactiveFieldConfiguration.dataType == FieldDataType.None ||
                field?.interactiveFieldConfiguration.$type == FieldType.Content;
            })
            .map((controls: any) => controls.field.fieldKey);
    }

    public removeRepeatingAndForeignValuesFromModel(model: object): object {
        let repeatingFieldKeys: Array<string> = this.getRepeatingFieldKeys();
        let fieldKeys: Array<string> = this.getFieldKeys();
        let webhookFieldKeys: Array<string> = this.getWebhookFieldKeys();
        let fieldKeysThatAreContentTypeOrWithNoDataType: Array<string> =
            this.getFieldKeysThatAreContentTypeOrWithNoDataType();

        Object.getOwnPropertyNames(model)
            .filter((propertyName: string) => !fieldKeys.includes(propertyName)
                || repeatingFieldKeys.includes(propertyName)
                || webhookFieldKeys.includes(propertyName)
                || fieldKeysThatAreContentTypeOrWithNoDataType.includes(propertyName))
            .forEach((propertyName: string) => delete model[propertyName]);
        return model;
    }

    /**
     * iterates through the passed model, removing values which are for fields
     * that have become hidden by expressions (or if their parent is hidden).
     * @param model 
     */
    private blankValuesForHiddenFieldsInModel(model: object): object {
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field?.isHidden()) {
                if (model[field.fieldKey]) {
                    model[field.fieldKey] = '';
                }
            }
        }
        return model;
    }

    public get valid(): boolean {
        return this.childrenValidityTracker.valid;
    }

    private notifyParentOfValidityChange(valid: boolean): void {
        if (this.parentFieldChildrenValidityTracker) {
            this.parentFieldChildrenValidityTracker
                .onChildValidityChange(`QuestionsWidget ${this.questionSetPath}`, valid);
        }
    }

    /** 
     * Saves our form model into the unified form model so the field values for this question set
     * can be used in payloads after the questions widget has been destroyed.
     */
    public patchQuestionSetModelIntoUnifiedFormModels(): void {
        let model: object = this.removeRepeatingAndForeignValuesFromModel(this.model);
        this.unifiedFormModelService.workingFormModel.patchQuestionSetModelIntoUnifiedFormModel(
            this.fieldPath,
            model);

        // we need to clone the model here so that we don't blank out fields that are hidden in the working model.
        model = _.clone(model);
        this.blankValuesForHiddenFieldsInModel(model);
        this.unifiedFormModelService.strictFormModel.patchQuestionSetModelIntoUnifiedFormModel(
            this.fieldPath,
            model);
    }

    public onFieldFocus(data: any): void {
        data.questionSetName = this.name;
        this.formService.onFormFocus(data);
    }

    public onFieldBlur(data: any): void {
        data.questionSetName = this.name;
        this.formService.onFormBlur(data);
    }

    public handleValidityChanges(): void {
        this.childrenValidityTracker.validObservable
            .pipe(
                takeUntil(this.destroyed),
                distinctUntilChanged(),
            )
            .subscribe((valid: boolean) => {
                this.formService.setQuestionSetValidity(this.questionSetPath, valid);
                if (valid && this.affectsPremium && this.affectsTriggers) {
                    this.generateQuoteRequest(this.affectsPremium, this.affectsTriggers);
                }
                this.notifyParentOfValidityChange(valid);
                this.validSubject.next(valid);
            });
    }

    /**
     * Called by the field when it's valid value changes to another valid value.
     * This allows us to generate a new quote request for a premium and triggers if the
     * question set has affectsPremium or affectsTriggers set on it.
     */
    public onFieldValidValueChange(): void {
        // we are using setTimeout here to introduce a slight delay to allow expressions to update the other fields
        // which might result in the question set becoming invalid. We want the time to pass that the
        // question set's validity has a chance to change before triggering the quote request.
        if (this.valid && (this.affectsPremium || this.affectsTriggers)) {
            this.generateQuoteRequest(this.affectsPremium, this.affectsTriggers);
        }
    }

    public onFieldValidityChange(): void {
        if (this.revealGroups.length && !this.deleted) {
            this.updateRevealedFields();
        }
    }

    public updateRevealedFields(): void {
        let previousQuestionSet: QuestionsWidget = null;
        if (this.sectionDisplayMode == SectionDisplayMode.Page) {
            previousQuestionSet = this.formService.getPreviousQuestionSetByStep(this);
        } else if (this.sectionDisplayMode == SectionDisplayMode.Article) {
            previousQuestionSet = this.formService.getPreviousQuestionSetByArticle(this);
        }
        let stillValid: boolean = !previousQuestionSet || previousQuestionSet.isFullyRevealed();
        let numValidGroups: number = 0;
        for (let revealGroup of this.revealGroups) {
            revealGroup.reveal = stillValid;
            if (stillValid && !revealGroup.valid) {
                stillValid = false;
            }

            if (stillValid) {
                numValidGroups++;
            }
        }
        this.revealGroupTrackingService.updateCompletion(
            this.stepName,
            this.questionSetPath,
            this.revealGroups.length,
            numValidGroups);

        if (stillValid != this.fullyRevealed) {
            this.fullyRevealed = stillValid;
            let nextQuestionSet: QuestionsWidget = null;
            if (this.sectionDisplayMode == SectionDisplayMode.Page) {
                nextQuestionSet = this.formService.getNextQuestionSetByStep(this);
            } else if (this.sectionDisplayMode == SectionDisplayMode.Article) {
                nextQuestionSet = this.formService.getNextQuestionSetByArticle(this);
            }
            if (nextQuestionSet) {
                nextQuestionSet.updateRevealedFields();
            }
        }
    }

    public isFullyRevealed(): boolean {
        return this.fullyRevealed;
    }

    /**
     * Gets the field definitions for the current question set and stores them for formly to access.
     */
    protected prepareFieldDefinitions(): void {
        let fieldsByRow: Array<FormlyFieldConfig>;
        if (this.repeatingIndex == null) {
            fieldsByRow = this.configService.questionSets[this.name];
        } else {
            fieldsByRow = _.cloneDeep(this.configService.repeatingQuestionSets[this.parentKey]);
        }
        if (!this.fieldsByRow) {
            this.fieldsByRow = fieldsByRow;
        } else {
            // add and remove fields and rows to match
            let results: QuestionSetSyncResults = QuestionSetConfigSyncHelper.synchronise(
                fieldsByRow,
                this.fieldsByRow);

            // update field properties that have changed for certain fields
            for (let i: number = 0; i < fieldsByRow.length; i++) {
                for (let j: number = 0; j < fieldsByRow[i].fieldGroup.length; j++) {
                    let fieldConfigChanged: boolean
                        = !_.isEqual(
                            this.fieldsByRow[i].fieldGroup[j].templateOptions.fieldConfiguration,
                            fieldsByRow[i].fieldGroup[j].templateOptions.fieldConfiguration);
                    if (fieldConfigChanged) {
                        let oldConfig: FieldConfiguration
                            = _.clone(this.fieldsByRow[i].fieldGroup[j].templateOptions.fieldConfiguration);
                        let newConfig: FieldConfiguration
                            = fieldsByRow[i].fieldGroup[j].templateOptions.fieldConfiguration;

                        // clear the validators so we always take the new one
                        delete this.fieldsByRow[i].fieldGroup[j].validators;

                        // merge the new field data into the old
                        _.merge(this.fieldsByRow[i].fieldGroup[j], fieldsByRow[i].fieldGroup[j]);

                        // replace the FieldConfiguration
                        this.fieldsByRow[i].fieldGroup[j].templateOptions.fieldConfiguration = newConfig;

                        // publish that this field changed, with the new fieldConfiguration
                        let key: string = <string>fieldsByRow[i].fieldGroup[j].key;
                        this.eventService.getFieldConfigUpdatedSubject(key)
                            .next({ old: oldConfig, new: newConfig });
                    }
                }
            }

            // formly requires us to rebuild the question set if fields or rows are added, it's pretty crap.
            if (results.fieldsAdded || results.rowsAdded) {
                this.options._buildForm();
            }

            // notify if any fields were deleted
            for (let key of results.removedFieldKeys) {
                let removedFieldPath: string = this.fieldPath.length > 0
                    ? this.fieldPath + '.' + key
                    : key;
                this.eventService.fieldPathRemovedSubject.next(removedFieldPath);
            }

            // re-calculate reveal groups
            if (results.fieldsAdded || results.fieldsRemoved) {
                // wait until the fields are created/removed, then re-create the reveal groups
                setTimeout(() => this.createRevealGroups(), 0);
            }
        }
    }

    public getName(): string {
        return this.name;
    }

    public getKey(): string {
        return this.parentKey;
    }

    public getFieldName(): string {
        return this.parentKey;
    }

    public getForm(): FormGroup {
        return this.form;
    }

    public getModel(): any {
        return this.model;
    }

    public getRepeatingQuestionSetIndex(): number {
        return this.repeatingIndex;
    }

    public getValues(includeEmptyValues: boolean = true): object {
        let values: object = _.cloneDeep(this.model);
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field != null && field.fieldType == 'repeating') {
                values[controlName] = (<RepeatingField>field).getValues(includeEmptyValues);
            }
        }
        return values;
    }

    public reset(): void {
        this.form.reset();
        let index: number = 0;
        let fieldsList: Array<any> = [];
        for (let row in this.fieldsByRow) {
            for (let field in this.fieldsByRow[row].fieldGroup) {
                fieldsList.push(this.fieldsByRow[row].fieldGroup[field]);
            }
        }
        for (let control in this.controls) {
            if (fieldsList[index] && !StringHelper.isNullOrEmpty(fieldsList[index].defaultValue)) {
                this.controls[control].setValue(fieldsList[index].defaultValue);
            }
            index++;
        }
    }

    public replicate(questionSet: any): void {
        this.form.reset();
        let destinationControls: any = this.controls;
        let sourceControls: any = questionSet.getForm()
            .controls[this.parentKey]['controls'][questionSet.getRepeatingQuestionSetIndex()].controls;
        for (let controlName in sourceControls) {
            let sourceControl: any = sourceControls[controlName];
            let destinationControl: any = destinationControls[controlName];
            if (!StringHelper.isNullOrEmpty(sourceControl.value)) {
                destinationControl.setValue(sourceControl.value);
            }
            if (sourceControl.touched) {
                destinationControl.markAsTouched();
            } else {
                destinationControl.markAsUntouched();
            }
            sourceControl.markAsUntouched();
        }
    }

    protected get controls(): any {
        return this.form.controls;
    }

    public markAllInvalidFieldsTouched(): void {
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (!field.isHidden() && !field.valid) {
                let fieldIsRepeating: boolean = field.fieldType == 'repeating';
                if (fieldIsRepeating) {
                    (<RepeatingField>field).markAllInvalidFieldsTouched();
                } else {
                    field.formControl.markAsTouched();
                }
            }
        }
    }

    /**
     * @returns true if it found a visible field and scrolled to it.
     */
    public scrollToFirstInvalidField(): boolean {
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (!field.isHidden() && !field.valid) {
                let fieldIsRepeating: boolean = field.fieldType == FieldType.Repeating;
                if (fieldIsRepeating) {
                    let scrolled: boolean = (<RepeatingField>field).scrollToFirstInvalidField();
                    if (scrolled) {
                        return true;
                    }
                }
                let el: any = this.windowScroll.getInvalidNativeElementByFieldPath(field.fieldPath);
                if (el != null) {
                    while (el.tagName != 'LABEL-WRAPPER' && el.tagName != 'QUESTION-WRAPPER'
                        && el.tagName != 'FORMLY-FIELD') {
                        el = el.parentElement;
                    }
                    this.windowScroll.scrollElementIntoView(el, { shake: true });
                    return true;
                }
            }
        }
        return false;
    }

    /**
     * Attempts to set the value of a field within this QuestionsWidget
     * @param key 
     * @param value 
     */
    public setFieldValue(key: string, value: any): boolean {
        if (this.controls[key]) {
            let field: Field = this.controls[key].field;
            field.setValue(value);
            field.onChange();
            return true;
        }
        return false;
    }

    /**
     * Field path has the following structure:
     * 
     * If it's in a root question set, then it's just the field key, e.g. "contactName".
     * If it's in a repeating question set, then it's {repeatingQuestionSetFieldKey}[{index}].{fieldKey}
     * e.g. "drivers[0].name".
     * 
     * If it's in a repating question set inside a repeating question set it would be:
     * e.g. "drivers[0].claims[0].amount".
     * 
     * Since this is a questions widget and not a field, it doesn't have a field path, so this will get the 
     * parent field path, if there is one. If this questions widget is one of many in a repeating 
     * question set, then it will add the index of to the field path in array notation.
     */
    public get fieldPath(): string {
        if (this.isRepeating) {
            return this.parentFieldPath + '[' + this.repeatingIndex + ']';
        } else {
            return this.parentFieldPath || '';
        }
    }

    /**
     * Question set path has the following structure:
     * 
     * If it's a root question set, then it's just the question set name, e.g. "ratingPrimary".
     * If it's in a repeating question set, then it's {questionSetName}.{questionSetName}[{index}]
     * 
     * Examples:
     * ratingPrimary
     * ratingPrimary.theVehicles[0]
     * ratingPrimary.theDrivers[1].theClaims[0]
     */
    public get questionSetPath(): string {
        let parentSegmentOrEmpty: string = this.parentQuestionsWidget
            ? this.parentQuestionsWidget.questionSetPath + '.'
            : '';
        if (this.isRepeating) {
            return parentSegmentOrEmpty + this.name + '[' + this.repeatingIndex + ']';
        } else {
            return parentSegmentOrEmpty + this.name;
        }
    }

    public setHidden(hidden: boolean): void {
        if (this.hidden != hidden) {
            this.hidden = hidden;
            this.notifyChildrenOfHiddenChange(hidden);
            this.formService.setQuestionSetVisible(this.questionSetPath, !this.isHidden());
            this.eventService.formElementHiddenChangeSubject.next();
        }
    }

    public onParentHiddenChange(hidden: boolean): void {
        this.notifyChildrenOfHiddenChange(hidden);
        this.formService.setQuestionSetVisible(this.questionSetPath, !this.isHidden());
    }

    private generateQuoteRequest(affectsPremium: boolean, affectsTriggers: boolean, delayMillis: number = 50): void {
        // delay the calculation request to give calculated fields a chance to calculate
        setTimeout(() => {
            this.calculationService.generateQuoteRequest(affectsPremium, affectsTriggers);
        }, delayMillis);
    }

    private notifyChildrenOfHiddenChange(hidden: boolean): void {
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field != null) {
                field.onParentHiddenChange(hidden);
            }
        }
    }

    private deregisterChildrenRequiredForCalculation(): void {
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field != null && field.requiredForCalculation) {
                this.calculationService.deregisterFieldRequiredForCalculation(field);
            }
        }
    }

    /**
     * @returns true if this questions widget is hidden OR it's parent Article/QuestionsWidget is hidden
     */
    public isHidden(): boolean {
        return this.hidden || (this.parentHideable && this.parentHideable.isHidden());
    }

    public clearValues(): void {
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field != null) {
                field.clearValue();
            }
        }
    }

    /**
     * Republish the field values for this question set in ExpressionInputSubjectService.
     * This would normally only be called if the question set was part of a repeating set and one was removed.
     */
    private republishFieldValues(): void {
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field != null) {
                this.eventService.fieldPathAddedSubject.next(field.fieldPath);
                field.publishChange();
            }
        }
    }

    /**
     * Recreate the expressions associated with fields in this question set.
     * This would normally only be called if the question set was part of a repeating set and one was removed.
     */
    private recreateExpressions(): void {
        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field != null) {
                field.recreateExpressions();
            }
        }
    }

    /**
     * Recreate the expressions for fields in this fieldset and republish field values for this question set.
     * This would normally only be called if the question set was part of a repeating set and one was removed.
     */
    public onRepeatingInstanceReorder(): void {
        this.formService.setQuestionSetVisible(this.questionSetPath, !this.isHidden());
        this.recreateExpressions();
        this.republishFieldValues();

        // re-register this question set since it's question set path might have changed.
        this.formService.registerQuestionSet(this);

        // re-set it's validity
        this.formService.setQuestionSetValidity(this.questionSetPath, this.valid);
    }

    private removeLayoutIfNoFieldsHaveLayout(): void {
        if (this.questionSetPath == 'paymentOptions') {
            // we always show the payment options in the calcualtion widget
            return;
        }

        for (let controlName in this.controls) {
            let field: Field = this.controls[controlName].field;
            if (field != null) {
                if (field.hasLayout) {
                    this.hasNoLayout = false;
                    return;
                }
            }
        }

        // if we got this far, then none of the fields have layout
        // we need to do it in a separate thread to stop ExpressionChangedAfterItHasBeenCheckedError
        setTimeout(() => this.hasNoLayout = true, 0);
    }
}
