import { Injectable, Output, EventEmitter, Directive } from '@angular/core';
import { Subject } from 'rxjs';
import { ConfigService } from './config.service';
import { AttachmentService } from './attachment.service';
import { QuestionsWidget } from '../components/widgets/questions/questions.widget';
import { DomSanitizer } from '@angular/platform-browser';
import * as _ from 'lodash-es';
import { filter } from 'rxjs/operators';
import { UnifiedFormModelService } from './unified-form-model.service';
import { AttachmentFileProperties } from '@app/models/attachment-file-properties';
import { ApplicationService } from './application.service';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { EncryptionService } from './encryption.service';
import { PaymentService } from './payment.service';
import { UuidHelper } from '@app/helpers/uuid.helper';
import { SectionDisplayMode } from '@app/models/section-display-mode.enum';
import { EventService } from './event.service';
/**
 * An object which holds each question widget, as object properties
 */
export interface QuestionSetsObject {
    [key: string]: QuestionsWidget;
}

/**
 * Export form service class.
 * TODO: Write a better class header: form functions.
 */
@Directive()
@Injectable()
export class FormService {

    @Output() public valueChanges: EventEmitter<any> = new EventEmitter<any>();
    @Output() public keyDown: EventEmitter<any> = new EventEmitter<any>();
    @Output() public formFocus: EventEmitter<any> = new EventEmitter<any>();
    @Output() public fieldBlur: EventEmitter<any> = new EventEmitter<any>();
    @Output() public statusChanges: EventEmitter<any> = new EventEmitter<any>();

    protected cachedValue: any;
    protected cacheStatus: string = 'none';
    public registeredQuestionSets: QuestionSetsObject = {};
    public resetActionsSubject: Subject<void> = new Subject<void>();
    public visibleQuestionSetPaths: Set<string> = new Set<string>();
    public visibleQuestionSetPathsChangedSubject: Subject<void> = new Subject<void>();
    public questionSetValidityChangeSubject: Subject<void> = new Subject<void>();

    private fieldPathsForFieldsWithUnstableValidities: Set<string> = new Set<string>();
    private fieldPathsForFieldsWithUnstableValues: Set<string> = new Set<string>();

    /**
     * A registry of auto trigger action buttons by name. We need to have this
     * registry so that we don't auto trigger the button twice if it's
     * in more than one location (e.g. footer and sidebar)
     */
    private autoTriggerActionButtonNamesByWidgetId: Map<string, Set<string>> = new Map<string, Set<string>>();

    /**
     * A centralised location for a "field data".
     * Field data is additional data that can be set on a field,
     * in a javascript structure. They are not stored in the form model,
     * only in memory.
     */
    private fieldData: Map<string, any> = new Map<string, any>();
    public fieldDataUpdatedSubject: Subject<void> = new Subject<void>();

    /**
     * contains the current validity state of question sets by questionSetPath
     */
    private questionSetValidityMap: Map<string, boolean> = new Map<string, boolean>();

    private renderedQuestionsSetsByStepMap: Map<string, Map<string, QuestionsWidget>>
        = new Map<string, Map<string, QuestionsWidget>>();

    private renderedQuestionsSetsByArticleMap: Map<string, Array<Map<string, QuestionsWidget>>>
        = new Map<string, Array<Map<string, QuestionsWidget>>>();

    protected ignoredWorkflowRoles: Array<string> = [
        'savePassword',
        'loadPassword',
        'creditCardNumber',
        'creditCardName',
        'creditCardExpiry',
        'creditCardCCV',
        'bankAccountNumber',
        'bankAccountName',
        'bankAccountBSB',
    ];

    protected sensitiveWorfklowRoles: Array<WorkflowRole> = [
        WorkflowRole.CreditCardNumber,
        WorkflowRole.CreditCardCcv,
    ];

    protected fieldsIgnoredBecauseOfWorkflowRole: Array<any> = [];

    public constructor(
        protected configService: ConfigService,
        protected attachmentService: AttachmentService,
        protected sanitizer: DomSanitizer,
        private unifiedFormModelService: UnifiedFormModelService,
        private applicationService: ApplicationService,
        private encryptionService: EncryptionService,
        private paymentService: PaymentService,
        private eventService: EventService,
    ) {
        this.onConfigurationReady();
    }

    private onConfigurationReady(): void {
        this.configService.configurationReadySubject.pipe(filter((ready: boolean) => ready))
            .subscribe((ready: boolean) => {
                this.determineFieldsToIgnoreBecauseOfWorkflowRole();
            });
    }

    public setQuestionSetValidity(questionSetPath: string, valid: boolean): void {
        this.questionSetValidityMap.set(questionSetPath, valid);
        this.questionSetValidityChangeSubject.next();
    }

    public removeQuestionSetValidity(questionSetPath: string): void {
        this.questionSetValidityMap.delete(questionSetPath);
        this.questionSetValidityChangeSubject.next();
    }

    public setQuestionSetVisible(questionSetPath: string, visible: boolean): void {
        if (visible) {
            this.visibleQuestionSetPaths.add(questionSetPath);
        } else {
            this.visibleQuestionSetPaths.delete(questionSetPath);
        }
        this.visibleQuestionSetPathsChangedSubject.next();
    }

    public registerQuestionSet(questionSet: QuestionsWidget): void {
        if (questionSet.sectionDisplayMode == SectionDisplayMode.Page) {
            this.addRenderedQuestionSetByStep(questionSet);
        } else if (questionSet.sectionDisplayMode == SectionDisplayMode.Article) {
            this.addRenderedQuestionSetByArticle(questionSet);
        }
        this.registeredQuestionSets[questionSet.questionSetPath] = questionSet;
        this.clearValueCache();
    }

    public deregisterQuestionSet(questionSet: QuestionsWidget): void {
        if (questionSet.sectionDisplayMode == SectionDisplayMode.Page) {
            this.removeRenderedQuestionSetByStep(questionSet);
        } else if (questionSet.sectionDisplayMode == SectionDisplayMode.Article) {
            this.removeRenderedQuestionSetByArticle(questionSet);
        }
        if (this.registeredQuestionSets[questionSet.questionSetPath]) {
            delete this.registeredQuestionSets[questionSet.questionSetPath];
        }
        this.clearValueCache();
    }

    public clearValueCache(): void {
        this.cacheStatus = 'invalid';
    }

    public onFormFocus(data: any): void {
        this.formFocus.emit(data);
    }

    public onFormBlur(data: any): void {
        this.clearValueCache();
        this.fieldBlur.emit(data);
    }

    public getValues(
        includeLoadedModel: boolean = true,
        includeEmptyValues: boolean = true,
        includeIgnoredWorkflowRoles: boolean = false,
        includePrivateFields: boolean = true,
        includeFileData: boolean = false,
        encryptSensitiveData: boolean = true,
    ): any {
        if (includeLoadedModel && includeEmptyValues &&
            !includeIgnoredWorkflowRoles && includePrivateFields &&
            !includeFileData) {
            if (this.cachedValue == null || this.cacheStatus == 'invalid') {
                // first time generating, or cache has been invalidated
                this.cacheStatus = 'regenerating';
                this.cachedValue = this.generateValues();
                this.cacheStatus = 'valid';
            }
            return this.cachedValue || {};
        } else {
            return this.generateValues(
                includeLoadedModel,
                includeEmptyValues,
                includeIgnoredWorkflowRoles,
                includePrivateFields,
                includeFileData,
                encryptSensitiveData);
        }
    }

    private generateValues(
        includeLoadedModel: boolean = true,
        includeEmptyValues: boolean = true,
        includeIgnoredWorkflowRoles: boolean = true,
        includePrivateFields: boolean = true,
        includeFileData: boolean = false,
        encryptSensitiveData: boolean = true,
    ): any {
        // patch in all the formModel's from each question set into the unifiedFormModel
        // eslint-disable-next-line no-unused-vars
        for (const [questionSetPath, questionSet] of Object.entries(this.registeredQuestionSets)) {
            questionSet.patchQuestionSetModelIntoUnifiedFormModels();
        }

        // clone the unified form model, since we will be making changes to it
        let values: object = _.cloneDeep(this.unifiedFormModelService.strictFormModel.model);

        // generate the credit card bin before we encrypt the card number
        let paymentCardNumber: string = this.getValueForWorkflowRole(WorkflowRole.CreditCardNumber, values);
        this.paymentService.generatePaymentCardBin(paymentCardNumber);
        this.paymentService.determinePaymentCardNumberLength(paymentCardNumber);

        // make the necessary changes in accordance with the passed parameters
        this.processFieldValues(
            values,
            includeLoadedModel,
            includeEmptyValues,
            includeIgnoredWorkflowRoles,
            includePrivateFields,
            includeFileData,
            encryptSensitiveData);
        return values;
    }

    private processFieldValues(
        values: object,
        includeLoadedModel: boolean = true,
        includeEmptyValues: boolean = true,
        includeIgnoredWorkflowRoles: boolean = true,
        includePrivateFields: boolean = true,
        includeFileData: boolean = false,
        encryptSensitiveData: boolean = true,
    ) {
        let sensitiveFieldKeys: Array<string> = this.getSensitiveFieldKeys();
        for (let key in values) {
            let value: any = values[key];

            if (value == null) {
                delete values[key];
            } else if (!includeEmptyValues && includeLoadedModel && value == '') {
                delete values[key];
            } else if (!includeIgnoredWorkflowRoles && this.ignoredWorkflowRoles.includes(key)) {
                delete values[key];
            } else if (!includePrivateFields && this.configService.privateFieldKeys.includes(key)) {
                delete values[key];
            } else if (encryptSensitiveData && value != '' && sensitiveFieldKeys.includes(key)) {
                values[key] = this.encryptionService.encryptData(value);
            } else if (includeFileData && this.attachmentService.isAttachmentSignature(value)) {
                const parts: Array<string> = value.split(':');
                if (parts.length >= 3) {
                    const attachmentId: string = parts[2];
                    if (UuidHelper.isUuid(attachmentId)) {
                        const fileProperties: AttachmentFileProperties =
                            this.attachmentService.getAttachment(attachmentId);
                        if (fileProperties) {
                            // Replace the attachment Id with the full file data
                            values[key] = value.replace(attachmentId, fileProperties['fileData']);
                        }
                    }
                }
            } else if (_.isArray(values[key])) {
                for (let obj of values[key]) {
                    this.processFieldValues(
                        obj,
                        includeLoadedModel,
                        includeEmptyValues,
                        includeIgnoredWorkflowRoles,
                        includePrivateFields,
                        includeFileData,
                        encryptSensitiveData);
                }
            } else if (typeof (values[key]) == 'object') {
                // we wouldn't normally have this structure
                this.processFieldValues(
                    values[key],
                    includeLoadedModel,
                    includeEmptyValues,
                    includeIgnoredWorkflowRoles,
                    includePrivateFields,
                    includeFileData,
                    encryptSensitiveData);
            }
        }
    }

    public getValueForWorkflowRole(workflowRole: WorkflowRole, formValues: object): any {
        let formValue: any = this.getFormValueViaFormDataLocator(workflowRole, formValues);

        if (!formValue && this.configService.workflowRoles) {
            let fieldName: any = this.configService.workflowRoles[workflowRole];
            formValue = fieldName ? formValues[fieldName] : formValues[workflowRole];
        }

        return formValue;
    }

    private getSensitiveFieldKeys(): Array<string> {
        let sensitiveFieldKeys: Array<string> = new Array<string>();
        for (let sensitiveWorkflowRole of this.sensitiveWorfklowRoles) {
            let fieldKey: string = this.getFieldKeyForWorkflowRole(sensitiveWorkflowRole);
            if (fieldKey) {
                sensitiveFieldKeys.push(fieldKey);
            }
        }
        return sensitiveFieldKeys;
    }

    public getFieldKeyForWorkflowRole(workflowRole: WorkflowRole): any {
        let fieldKey: string = this.getFieldKeyViaFormDataLocator(workflowRole);

        if (!fieldKey) {
            fieldKey = this.configService.workflowRoles?.[workflowRole];
        }

        return fieldKey;
    }

    private getFormValueViaFormDataLocator(workflowRole: string, formValues: object): any {
        let formValue: any = null;
        if (this.configService.dataLocators) {
            let locators: Array<any> = this.configService.dataLocators[workflowRole];
            if (locators) {
                for (let locator of locators) {
                    if (locator.source === "formData") {
                        formValue = _.get(formValues, locator.path);
                        if (formValue) {
                            break;
                        }
                    }
                }
            }
        }

        return formValue;
    }

    private getFieldKeyViaFormDataLocator(workflowRole: string): string {
        if (this.configService.dataLocators) {
            let locators: Array<any> = this.configService.dataLocators[workflowRole];
            if (locators) {
                for (let locator of locators) {
                    if (locator.source === "formData") {
                        return locator.path;
                    }
                }
            }
        }

        return null;
    }

    private determineFieldsToIgnoreBecauseOfWorkflowRole(): void {
        for (let key in this.configService.workflowRoles ? this.configService.workflowRoles : []) {
            if (this.ignoredWorkflowRoles.indexOf(key) != -1) {
                this.fieldsIgnoredBecauseOfWorkflowRole.push(this.configService.workflowRoles[key]);
            }
        }
    }

    /**
     * returns an object where properties are the "questionSetPath" 
     * of the question set and values are true or false, e.g.
     * validQuestionSets['ratingPrimary'] = true
     * validQuestionSets['drivers[0]'] = false
     * validQuestionSets['drivers[0].claims[1]'] = true
     */
    public getValidityOfQuestionSetsAsObject(): object {
        let validQuestionSets: any = {};
        for (let [questionSetPath, valid] of this.questionSetValidityMap) {
            validQuestionSets[questionSetPath] = valid;
        }
        return validQuestionSets;
    }

    /**
     * @returns true if the question set is valid or hidden
     */
    public questionSetIsValidOrHidden(questionSetPath: string): boolean {
        let questionsWidget: QuestionsWidget = this.registeredQuestionSets[questionSetPath];
        if (!questionsWidget || !questionsWidget.visible) {
            return true;
        }
        return this.questionSetValidityMap.get(questionSetPath);
    }

    /**
     * @returns true if the question set is invalid or hidden
     */
    public questionSetIsInvalidOrHidden(questionSetPath: string): boolean {
        let questionsWidget: QuestionsWidget = this.registeredQuestionSets[questionSetPath];
        if (!questionsWidget || !questionsWidget.visible) {
            return true;
        }
        return !this.questionSetValidityMap.get(questionSetPath);
    }

    /**
     * Checks whether the passed questions set paths are all valid or hidden, and if so returns true.
     * If no questionSetPaths are passed, then it checks all question sets
     * @param questionSetPaths 
     */
    public questionSetsAreValidOrHidden(questionSetPaths?: Array<string>): boolean {
        if (questionSetPaths == null) {
            questionSetPaths = Object.getOwnPropertyNames(this.registeredQuestionSets);
        }
        for (let questionSetName of questionSetPaths) {
            if (!this.questionSetIsValidOrHidden(questionSetName)) {
                return false;
            }
        }
        return true;
    }

    /**
     * Checks whether the passed questions set paths are all invalid or hidden, and if so returns true.
     * If no questionSetPaths are passed, then it checks all question sets
     * @param questionSetPaths 
     */
    public questionSetsAreInvalidOrHidden(questionSetPaths?: Array<string>): boolean {
        if (questionSetPaths == null) {
            questionSetPaths = Object.getOwnPropertyNames(this.registeredQuestionSets);
        }
        for (let questionSetName of questionSetPaths) {
            if (!this.questionSetIsInvalidOrHidden(questionSetName)) {
                return false;
            }
        }
        return true;
    }

    /**
     * @returns true if the question set is registered and known to be valid.
     */
    public questionSetIsValid(questionSetPath: string): boolean {
        let valid: boolean = this.questionSetValidityMap.get(questionSetPath);
        return valid == null ? false : valid;
    }

    /**
     * Checks whether the passed questions set paths are all valid, and if so returns true.
     * If no questionSetPaths are passed, then it checks all registered question sets
     * @param questionSetPaths 
     */
    public questionSetsAreValid(questionSetPaths?: Array<string>): boolean {
        if (questionSetPaths == null) {
            questionSetPaths = Object.getOwnPropertyNames(this.registeredQuestionSets);
        }
        for (let questionSetPath of questionSetPaths) {
            if (!this.questionSetIsValid(questionSetPath)) {
                return false;
            }
        }
        return true;
    }

    /**
     * Checks whether the passed questions set paths are all invalid, and if so returns true.
     * If no questionSetPaths are passed, then it checks all question sets
     * @param questionSetPaths 
     */
    public questionSetsAreInvalid(questionSetPaths?: Array<string>): boolean {
        if (questionSetPaths == null) {
            questionSetPaths = Object.getOwnPropertyNames(this.registeredQuestionSets);
        }
        for (let questionSetPath of questionSetPaths) {
            if (!this.questionSetIsInvalid(questionSetPath)) {
                return false;
            }
        }
        return true;
    }

    /**
     * @returns true if the quesiton set is registered and known to be invalid.
     */
    public questionSetIsInvalid(questionSetPath: string): boolean {
        let valid: boolean = this.questionSetValidityMap.get(questionSetPath);
        return valid == null ? false : !valid;
    }

    public questionSetIsHidden(questionSetPath: string): boolean {
        let questionsWidget: QuestionsWidget = this.registeredQuestionSets[questionSetPath];
        if (questionsWidget) {
            return !questionsWidget.visible;
        }
        return true;
    }

    /**
     * @returns true if we found a visible invalid field to scroll to and it actually scrolled.
     */
    public scrollToFirstInvalidField(questionSetPaths: Array<string>): boolean {
        for (let questionSetPath of questionSetPaths) {
            let questionSet: QuestionsWidget = this.registeredQuestionSets[questionSetPath];
            if (questionSet && !questionSet.valid) {
                let scrolled: boolean = questionSet.scrollToFirstInvalidField();
                if (scrolled) {
                    return true;
                }
            }
        }
        return false;
    }

    public markAllInvalidFieldsTouched(questionSetNames: Array<string>): void {
        for (let questionSetName of questionSetNames) {
            let questionSet: QuestionsWidget = this.registeredQuestionSets[questionSetName];
            if (questionSet) {
                questionSet.markAllInvalidFieldsTouched();
            }
        }
    }

    public resetActions(): void {
        this.resetActionsSubject.next();
    }

    public get debug(): boolean {
        return this.configService.debug;
    }

    public setFieldData(fieldPath: string, data: any): void {
        this.fieldData.set(fieldPath, data);
        this.fieldDataUpdatedSubject.next();
    }

    public getFieldData(fieldPath: string): any {
        return this.fieldData.get(fieldPath);
    }

    public deleteFieldData(fieldPath: string): void {
        this.fieldData.delete(fieldPath);
    }

    private addRenderedQuestionSetByStep(questionSet: QuestionsWidget): void {
        let questionSetsForStep: Map<string, QuestionsWidget>
            = this.renderedQuestionsSetsByStepMap.get(questionSet.stepName);
        if (!questionSetsForStep) {
            questionSetsForStep = new Map<string, QuestionsWidget>();
            this.renderedQuestionsSetsByStepMap.set(questionSet.stepName, questionSetsForStep);
        }
        questionSetsForStep.set(questionSet.questionSetPath, questionSet);
    }

    private addRenderedQuestionSetByArticle(questionSet: QuestionsWidget): void {
        let questionSetsByArticles: Array<Map<string, QuestionsWidget>>
            = this.renderedQuestionsSetsByArticleMap.get(questionSet.stepName);
        if (!questionSetsByArticles) {
            questionSetsByArticles = new Array<Map<string, QuestionsWidget>>();
            this.renderedQuestionsSetsByArticleMap.set(questionSet.stepName, questionSetsByArticles);
        }
        let questionSetsForArticle: Map<string, QuestionsWidget>
            = questionSetsByArticles[questionSet.articleIndex];
        if (!questionSetsForArticle) {
            questionSetsForArticle = new Map<string, QuestionsWidget>();
            questionSetsByArticles[questionSet.articleIndex] = questionSetsForArticle;
        }
        questionSetsForArticle.set(questionSet.questionSetPath, questionSet);
    }

    private removeRenderedQuestionSetByStep(questionSet: QuestionsWidget): void {
        let questionSetsForStep: Map<string, QuestionsWidget>
            = this.renderedQuestionsSetsByStepMap.get(questionSet.stepName);
        if (!questionSetsForStep) {
            console.error('Trying to remove a rendered question set from the step map but couldn\'t find it.');
            return;
        }
        questionSetsForStep.delete(questionSet.questionSetPath);
    }

    private removeRenderedQuestionSetByArticle(questionSet: QuestionsWidget): void {
        let questionSetsByArticles: Array<Map<string, QuestionsWidget>>
            = this.renderedQuestionsSetsByArticleMap.get(questionSet.stepName);
        if (!questionSetsByArticles) {
            console.error('Trying to remove a rendered question set from the question sets by articles step map but '
                + `couldn't find the step "${questionSet.stepName}" in the map.`);
            return;
        }
        let questionSetsForArticle: Map<string, QuestionsWidget>
            = questionSetsByArticles[questionSet.articleIndex];
        if (!questionSetsForArticle) {
            console.error('Trying to remove a rendered question set from the question sets by articles step map but '
                + `couldn't find article with index "${questionSet.articleIndex}" for step "${questionSet.stepName}"`
                + ' in the map.');
            return;
        }
        questionSetsForArticle.delete(questionSet.questionSetPath);
    }

    public getRenderedQuestionSetsForStep(stepName: string): Map<string, QuestionsWidget> {
        return this.renderedQuestionsSetsByStepMap.get(stepName);
    }

    /**
     * @returns the previous questions widget within the same step, or null if it's the first.
     * @param includeRepeating if set to false, skips repeating question sets
     */
    public getPreviousQuestionSetByStep(
        contextQuestionSet: QuestionsWidget,
        includeRepeating: boolean = false,
    ): QuestionsWidget {
        let previousQuestionSet: QuestionsWidget = null;
        const questionSetsForStep: Map<string, QuestionsWidget>
            = this.renderedQuestionsSetsByStepMap.get(contextQuestionSet.stepName);
        if (!questionSetsForStep) {
            return null;
        }
        const sortedQuestionsSets: Array<QuestionsWidget> = Array.from(questionSetsForStep.values())
            .sort((a: QuestionsWidget, b: QuestionsWidget) => {
                const diffArticleIndex: number = Number(a.articleIndex) - Number(b.articleIndex);
                if (diffArticleIndex != 0) {
                    return diffArticleIndex;
                }
                return Number(a.articleElementIndex) - Number(b.articleElementIndex);
            });
        for (const questionSet of sortedQuestionsSets) {
            if (questionSet.name == contextQuestionSet.questionSetPath) {
                break;
            }
            if (includeRepeating || !questionSet.isRepeating) {
                previousQuestionSet = questionSet;
            }
        }
        return previousQuestionSet;
    }

    /**
     * @returns the previous questions widget within the same step and article, or null if it's the first.
     * @param includeRepeating if set to false, skips repeating question sets
     */
    public getPreviousQuestionSetByArticle(
        contextQuestionSet: QuestionsWidget,
        includeRepeating: boolean = false,
    ): QuestionsWidget {
        let previousQuestionSet: QuestionsWidget = null;
        let questionSetsByArticles: Array<Map<string, QuestionsWidget>>
            = this.renderedQuestionsSetsByArticleMap.get(contextQuestionSet.stepName);
        if (questionSetsByArticles) {
            let questionSetsForArticle: Map<string, QuestionsWidget>
                = questionSetsByArticles[contextQuestionSet.articleIndex];
            if (questionSetsForArticle) {
                for (let [questionSetPath, questionSet] of questionSetsForArticle) {
                    if (questionSetPath == contextQuestionSet.questionSetPath) {
                        break;
                    }
                    if (includeRepeating || !questionSet.isRepeating) {
                        previousQuestionSet = questionSet;
                    }
                }
            }
        }
        return previousQuestionSet;
    }

    /**
     * @returns the next questions widget within the same step, or null if it's the first.
     * @param includeRepeating if set to false, skips repeating question sets
     */
    public getNextQuestionSetByStep(
        contextQuestionSet: QuestionsWidget,
        includeRepeating: boolean = false,
    ): QuestionsWidget {
        let matched: boolean = false;
        let nextQuestionSet: QuestionsWidget = null;
        let questionSetsForStep: Map<string, QuestionsWidget>
            = this.renderedQuestionsSetsByStepMap.get(contextQuestionSet.stepName);
        if (!questionSetsForStep) {
            return null;
        }
        const sortedQuestionsSets: Array<QuestionsWidget> = Array.from(questionSetsForStep.values())
            .sort((a: QuestionsWidget, b: QuestionsWidget) => {
                const diffArticleIndex: number = Number(a.articleIndex) - Number(b.articleIndex);
                if (diffArticleIndex != 0) {
                    return diffArticleIndex;
                }
                return Number(a.articleElementIndex) - Number(b.articleElementIndex);
            });
        for (const questionSet of sortedQuestionsSets) {
            if (matched) {
                if (includeRepeating || !questionSet.isRepeating) {
                    nextQuestionSet = questionSet;
                    break;
                }
            }
            if (questionSet.name == contextQuestionSet.questionSetPath) {
                matched = true;
            }
        }
        return nextQuestionSet;
    }

    /**
     * @returns the next questions widget within the same step, or null if it's the first.
     * @param includeRepeating if set to false, skips repeating question sets
     */
    public getNextQuestionSetByArticle(
        contextQuestionSet: QuestionsWidget,
        includeRepeating: boolean = false,
    ): QuestionsWidget {
        let matched: boolean = false;
        let nextQuestionSet: QuestionsWidget = null;
        let questionSetsByArticles: Array<Map<string, QuestionsWidget>>
            = this.renderedQuestionsSetsByArticleMap.get(contextQuestionSet.stepName);
        if (questionSetsByArticles) {
            let questionSetsForArticle: Map<string, QuestionsWidget>
                = questionSetsByArticles[contextQuestionSet.articleIndex];
            if (questionSetsForArticle) {
                for (let [questionSetPath, questionSet] of questionSetsForArticle) {
                    if (matched) {
                        if (includeRepeating || !questionSet.isRepeating) {
                            nextQuestionSet = questionSet;
                            break;
                        }
                    }
                    if (questionSetPath == contextQuestionSet.questionSetPath) {
                        matched = true;
                    }
                }
            }
        }
        return nextQuestionSet;
    }

    /**
     * @param name the action name
     * @param workflowStep the workflow step.
     */
    public registerAutoTriggerActionButton(name: string, workflowStep: string): void {
        let buttonNames: Set<string> = this.autoTriggerActionButtonNamesByWidgetId.get(workflowStep);
        if (!buttonNames) {
            buttonNames = new Set<string>();
            this.autoTriggerActionButtonNamesByWidgetId.set(workflowStep, buttonNames);
        }
        buttonNames.add(name);
    }

    public isAutoTriggerActionButtonRegistered(name: string): boolean {
        let found: boolean = false;
        this.autoTriggerActionButtonNamesByWidgetId.forEach((buttonNames: Set<string>) => {
            if (buttonNames.has(name)) {
                found = true;
                return;
            }
        });
        return found;
    }

    public deregisterAutoTriggerActionButtons(workflowStep: string): void {
        this.autoTriggerActionButtonNamesByWidgetId.delete(workflowStep);
    }

    public setFieldValidityUnstable(fieldPath: string, unstable: boolean = true): void {
        if (unstable) {
            this.fieldPathsForFieldsWithUnstableValidities.add(fieldPath);
            this.eventService.fieldValuesAndValiditiesAreStableSubject.next(false);
        } else {
            this.fieldPathsForFieldsWithUnstableValidities.delete(fieldPath);
            this.onFieldStabilityImproved();
        }
    }

    public setFieldValueUnstable(fieldPath: string, unstable: boolean = true): void {
        if (unstable) {
            this.fieldPathsForFieldsWithUnstableValues.add(fieldPath);
            this.eventService.fieldValuesAndValiditiesAreStableSubject.next(false);
        } else {
            this.fieldPathsForFieldsWithUnstableValues.delete(fieldPath);
            this.onFieldStabilityImproved();
        }
    }

    private onFieldStabilityImproved(): void {
        if (this.fieldPathsForFieldsWithUnstableValidities.size == 0
            && this.fieldPathsForFieldsWithUnstableValues.size == 0
        ) {
            this.eventService.fieldValuesAndValiditiesAreStableSubject.next(true);
        }
    }
}
