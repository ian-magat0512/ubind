import { Injectable } from '@angular/core';
import { ConfigService } from './config.service';
import { FormService } from './form.service';
import { CalculationOperation, CalculationOperationArguments } from '../operations/calculation.operation';
import { ApplicationService } from './application.service';
import { BehaviorSubject, Observable, Subject, SubscriptionLike } from 'rxjs';
import { buffer, debounceTime, take } from 'rxjs/operators';
import { Errors } from '@app/models/errors';
import { QuoteState } from '@app/models/quote-state.enum';
import { FormType } from '@app/models/form-type.enum';
import { ClaimState } from '@app/models/claim-state.enum';
import { CalculationState } from '@app/models/calculation-result-state';
import { DeploymentEnvironment } from '@app/models/deployment-environment';
import { StringHelper } from '@app/helpers/string.helper';
import {
    CalculationAffectingElement, CalculationAffectingField, CalculationAffectingQuestionSet,
} from '@app/models/calculation-affecting-element';
import { ExpressionInputSubjectService } from '@app/expressions/expression-input-subject.service';
import { OperationConfiguration } from '@app/models/configuration/operation-configuration';
import { OperationInstruction } from '@app/models/operation-instruction';
import { EventService } from './event.service';
import { OperationInstructionService } from './operation-instruction.service';
import { CalculationResponseCache } from '@app/operations/calculation-response-cache';
import { OperationArguments } from '@app/operations/operation';

/**
 * Stores information about a form element and watches for it's validity changes
 */
interface WatchedFormElement extends CalculationAffectingElement {
    validSubscription: SubscriptionLike;
}

export interface WatchedField extends WatchedFormElement {
    fieldPath: string;
}

interface WatchedQuestionSet extends WatchedFormElement {
    questionSetPath: string;
}


/**
 * Executes calculation requests when appropriate.
 */
@Injectable()
export class CalculationService {

    private static useCache: boolean = true;
    private static longDebounceMillis: number = 1500;
    private static shortDebounceMillis: number = 20;

    public premiumCalculationInProgressSubject: Subject<boolean> = new Subject<boolean>();
    public triggerCalculationInProgressSubject: Subject<boolean> = new Subject<boolean>();

    protected valueChangeQuoteRequestDelayMs: number = 0;
    protected quoteRequestTimeoutId: any;
    protected quoteGetCalculationTimeoutIds: Array<any> = [];
    protected calculationCount: number = 0;
    private _backgroundCalculationCount: number = 0;
    protected calculationForPremiumCount: number = 0;
    protected calculationForTriggerCount: number = 0;
    protected _latestCalculationResultId: string;

    private fieldsRequiredForCalculation: Array<WatchedField>
        = new Array<WatchedField>();
    private questionSetsRequiredForCalculation: Array<WatchedQuestionSet>
        = new Array<WatchedQuestionSet>();
    private fieldsWithInitialValueAndAffectsPremiumOrAffectsTriggers: Array<WatchedField>
        = new Array<WatchedField>();

    private premiumCalculationInProgress: boolean = false;
    private triggerCalculationInProgress: boolean = false;
    private _calculationPending: boolean = false;
    private _calculationPendingForPremium: boolean = false;
    private _calculationPendingForTrigger: boolean = false;
    private calculationTimestamp: number;
    private previousCalculationTimestamp: number;

    private lastNotificationTimestamp: number = performance.now();

    /**
     * If a calculation is in progress, we want to use a long debounce (e.g. 2 seconds)
     * so that we don't spam the server with requests if someone is clicking lots of fields
     * at once. Otherwise we use a super short debounce (20ms) to ensusre the first request
     * is quick to fire.
     */
    private debounceDurationMillis$: BehaviorSubject<number>
        = new BehaviorSubject(CalculationService.shortDebounceMillis);

    /**
     * use a Subject for handling debouncing of multiple calculation requests.
     * The boolean value passed represents whether the calculation request should be silent or not.
     * Silent requests do not hold up the UI, because we already have a cached response so we just use that.
     * The request still happens in the background to make sure the server is updated.
     */
    private calculationRequestSubject: Subject<[
        boolean /* forPremium */,
        boolean /* forTrigger */,
        boolean /* immediate */,
    ]> = new Subject<[boolean, boolean, boolean]>();

    /**
     * A Subject for when a calculation is initially triggered. This is used so that we can
     * debounce triggering calculations with a very small debounce. The reason this is needed is the case
     * where the user enters a value for a field which triggers a calculation, but immediately reveals another
     * field which gets a default value and also triggers a calcuation. This ensures that only one request
     * is triggered.
     */
    private triggerCalculationSubject: Subject<[
        boolean /* forPremium */,
        boolean /* forTrigger */,
        boolean /* immediate */,
    ]> = new Subject<[boolean, boolean, boolean]>();

    public constructor(
        protected formService: FormService,
        protected calculationOperation: CalculationOperation,
        protected applicationService: ApplicationService,
        protected config: ConfigService,
        private expressionInputSubjectService: ExpressionInputSubjectService,
        private eventService: EventService,
        private operationInstructionService: OperationInstructionService,
        private calculationResponseCache: CalculationResponseCache,
    ) {
        this.formService.resetActionsSubject.subscribe(() => this.resetCalculationCount());
        this.listenForCalculationResponses();
        this.listenForLongDebounceCalculationRequests();
        this.listenForTriggeredCalculations();
        this.listenForRetriggerCalculation();
    }

    /*
    * Background calculation count is the counter for which calculations that came from cache are counted.
    * This is because the regular calculationCount does not count them so we created a separate counter.
    * It also trigger the frontend/expressions to let them know that background calculation happened.
    * This is helpful for the backgroundCalculationInProgress() expression used by the workflow to dictate
    * that the UI needs act accordingly ( like disable a button IF a background calculation is in progress ).
    */
    protected set backgroundCalculationCount(newValue: number) {
        const backgrounCalculationInProgress: boolean = newValue > this._backgroundCalculationCount;
        this._backgroundCalculationCount = newValue;
        this.applicationService.backgroundCalculationInProgress = this.backgroundCalculationCount > 0;
        this.applicationService.backgroundCalculationInProgressSubject.next(backgrounCalculationInProgress);
    }

    protected get backgroundCalculationCount() {
        return this._backgroundCalculationCount;
    }

    public get calculationInProgress(): boolean {
        return this.calculationCount > 0;
    }

    private resetCalculationCount(): void {
        this.calculationCount = 0;
        this.backgroundCalculationCount = 0;
        this.calculationForPremiumCount = 0;
        this.calculationForTriggerCount = 0;
        this.notifyIfCalculationInProgressChanged();
    }

    private incrementCalculationCount(forPremium: boolean, forTrigger: boolean): void {
        this.debounceDurationMillis$.next(CalculationService.shortDebounceMillis);
        this.calculationCount++;
        this.calculationForPremiumCount += forPremium ? 1 : 0;
        this.calculationForTriggerCount += forTrigger ? 1 : 0;
        this.notifyIfCalculationInProgressChanged();
    }

    private decrementCalculationCount(): void {
        this.calculationCount--;
        this.debounceDurationMillis$.next(this.calculationCount <= 0
            ? CalculationService.shortDebounceMillis
            : CalculationService.longDebounceMillis);
        this.notifyIfCalculationInProgressChanged();
    }

    private incrementBackgroundCalculationCount(): void {
        this.backgroundCalculationCount++;
    }

    private decrementBackgroundCalculationCount(): void {
        this.backgroundCalculationCount =
            this.backgroundCalculationCount > 0
                ? this.backgroundCalculationCount - 1
                : this.backgroundCalculationCount;
    }

    private setCalculationPending(forPremium: boolean, forTrigger: boolean) {
        this._calculationPending = forPremium || forTrigger;
        this._calculationPendingForPremium = forPremium;
        this._calculationPendingForTrigger = forTrigger;
        this.notifyIfCalculationInProgressChanged();
    }

    private notifyIfCalculationInProgressChanged(): void {
        const wasInProgress: boolean = this.applicationService.calculationInProgress;
        const isInProgress: boolean = this.calculationCount > 0 || this._calculationPending;
        if (isInProgress != wasInProgress) {
            if (this.applicationService.debug && this.applicationService.debugLevel > 2) {
                const now: number = performance.now();
                console.log(`CHANGE: calculationInProggress: ${isInProgress}, `
                    + `calculationCount: ${this.calculationCount}, `
                    + `offsetMillis: ${now - this.lastNotificationTimestamp}`);
            }
            this.applicationService.calculationInProgress = isInProgress;
            this.applicationService.calculationInProgressSubject.next(isInProgress);
        }

        const wasInProgressForPremium: boolean = this.premiumCalculationInProgress;
        const isInProgressForPremium: boolean
            = this.calculationForPremiumCount > 0 || this._calculationPendingForPremium;
        if (wasInProgressForPremium != isInProgressForPremium) {
            this.premiumCalculationInProgress = isInProgressForPremium;
            this.premiumCalculationInProgressSubject.next(isInProgressForPremium);
        }

        const wasInProgressForTrigger: boolean = this.triggerCalculationInProgress;
        const isInProgressForTrigger: boolean
            = this.calculationForTriggerCount > 0 || this._calculationPendingForTrigger;
        if (wasInProgressForTrigger != isInProgressForTrigger) {
            this.triggerCalculationInProgress = isInProgressForTrigger;
            this.triggerCalculationInProgressSubject.next(isInProgressForTrigger);
        }
    }

    public get latestCalculationResultId(): string {
        return this._latestCalculationResultId;
    }

    public registerFieldRequiredForCalculation(field: CalculationAffectingField): void {
        let index: number = this.getRequiredFieldIndex(field.fieldPath);
        if (index == -1) {
            this.fieldsRequiredForCalculation.push(this.createWatchedField(field));
        } else {
            // it already exists so let's just update the key properties
            this.fieldsRequiredForCalculation[index].affectsPremium = field.affectsPremium;
            this.fieldsRequiredForCalculation[index].affectsTriggers = field.affectsTriggers;
        }
    }

    public registerQuestionSetRequiredForCalculation(questionSet: CalculationAffectingQuestionSet): void {
        let index: number = this.getRequiredQuestionSetIndex(questionSet.questionSetPath);
        if (index == -1) {
            this.questionSetsRequiredForCalculation.push(this.createWatchedQuestionSet(questionSet));
        } else {
            // it already exists so let's just update the key properties
            this.questionSetsRequiredForCalculation[index].affectsPremium = questionSet.affectsPremium;
            this.questionSetsRequiredForCalculation[index].affectsTriggers = questionSet.affectsTriggers;
        }
    }

    public registerFieldWithInitialValueAndAffectsPremiumOrAffectsTrigger(
        field: CalculationAffectingField,
    ): void {
        let index: number = this.getInitialFieldIndex(field.fieldPath);
        if (index == -1) {
            this.fieldsWithInitialValueAndAffectsPremiumOrAffectsTriggers.push(this.createWatchedField(field));
        } else {
            // it already exists so let's just update the key properties
            this.fieldsWithInitialValueAndAffectsPremiumOrAffectsTriggers[index].affectsPremium
                = field.affectsPremium;
            this.fieldsWithInitialValueAndAffectsPremiumOrAffectsTriggers[index].affectsTriggers
                = field.affectsTriggers;
        }
    }

    private createWatchedField(field: CalculationAffectingField): WatchedField {
        // make a copy so we don't hold a reference to the original object, so that memory is
        // disposed of when the element is no longer rendered.
        let watchedField: WatchedField = {
            valid: field.valid,
            fieldPath: field.fieldPath,
            affectsPremium: field.affectsPremium,
            affectsTriggers: field.affectsTriggers,
            validSubscription: null,
        };
        watchedField.validSubscription
            = this.expressionInputSubjectService.getFieldValidObservable(field.fieldPath)
                .subscribe((valid: boolean) => watchedField.valid = valid);
        return watchedField;
    }

    private createWatchedQuestionSet(questionSet: CalculationAffectingQuestionSet): WatchedQuestionSet {
        // make a copy so we don't hold a reference to the original object, so that memory is
        // disposed of when the field is no longer rendered.
        let watchedQuestionSet: WatchedQuestionSet = {
            valid: questionSet.valid,
            questionSetPath: questionSet.questionSetPath,
            affectsPremium: questionSet.affectsPremium,
            affectsTriggers: questionSet.affectsTriggers,
            validSubscription: null,
        };
        watchedQuestionSet.validSubscription
            = this.expressionInputSubjectService.getQuestionSetValidSubject(questionSet.questionSetPath)
                .subscribe((valid: boolean) => watchedQuestionSet.valid = valid);
        return watchedQuestionSet;
    }

    public deregisterFieldRequiredForCalculation(field: CalculationAffectingField): void {
        let index: number = this.getRequiredFieldIndex(field.fieldPath);
        if (index !== -1) {
            let watchedField: WatchedField
                = this.fieldsRequiredForCalculation.splice(index, 1)[0];
            watchedField.validSubscription.unsubscribe();
            watchedField.validSubscription = null;
        }
    }

    public deregisterQuestionSetRequiredForCalculation(questionSet: CalculationAffectingQuestionSet): void {
        let index: number = this.getRequiredQuestionSetIndex(questionSet.questionSetPath);
        if (index !== -1) {
            let watchedQuestionSet: WatchedQuestionSet
                = this.questionSetsRequiredForCalculation.splice(index, 1)[0];
            watchedQuestionSet.validSubscription.unsubscribe();
            watchedQuestionSet.validSubscription = null;
        }
    }

    public deregisterFieldWithInitialValueAndAffectsPremiumOrAffectsTrigger(field: CalculationAffectingField): void {
        let index: number = this.getInitialFieldIndex(field.fieldPath);
        if (index !== -1) {
            this.fieldsWithInitialValueAndAffectsPremiumOrAffectsTriggers.splice(index, 1);
        }
    }

    public generateQuoteRequest(forPremium: boolean, forTrigger: boolean): void {
        if (!this.canProceedWithCalculation()) {
            if (this.applicationService.debug) {
                console.log(this.getReasonCalculationDidNotProceed());
            }
            return;
        }
        if (this.applicationService.formType == FormType.Quote
            && this.applicationService.quoteState == QuoteState.Complete
        ) {
            throw Errors.Product.Configuration(
                'A calculation request was triggered, however the quote has already been completed and it cannot be '
                + 'updated. This request will be ignored.');
        }
        if (this.applicationService.formType == FormType.Claim
            && this.applicationService.claimState == ClaimState.Complete
        ) {
            throw Errors.Product.Configuration(
                'A calculation request was triggered, however the claim has already been completed and it cannot be '
                + ' updated. This request will be ignored.');
        }

        this.triggerCalculation(forPremium, forTrigger);
    }

    private canProceedWithCalculation(): boolean {
        let numberOfInvalidFields: number = this.fieldsRequiredForCalculation.filter(
            (field: WatchedField) => !field.valid).length;
        if (numberOfInvalidFields > 0) {
            return false;
        }
        let numberOfInvalidQuestionSets: number = this.questionSetsRequiredForCalculation.filter(
            (questionSet: WatchedQuestionSet) => !questionSet.valid).length;
        if (numberOfInvalidQuestionSets > 0) {
            return false;
        }

        return true;
    }

    private getReasonCalculationDidNotProceed(): string {
        const invalidFields: Array<WatchedField> = this.fieldsRequiredForCalculation.filter(
            (field: WatchedField) => !field.valid);
        const invalidQuestionSets: Array<WatchedQuestionSet> = this.questionSetsRequiredForCalculation.filter(
            (questionSet: WatchedQuestionSet) => !questionSet.valid);
        let reason: string = 'Not triggering a calculation because ';
        if (invalidFields.length > 0) {
            reason += 'the following fields are required to be valid: ';
            const invalidFieldPaths: Array<string> = invalidFields.map((field: WatchedField) => field.fieldPath);
            reason += invalidFieldPaths.join(', ');
        }
        if (invalidQuestionSets.length > 0) {
            if (invalidFields.length > 0) {
                reason += 'AND ';
            }
            reason += 'the following question sets are required to be valid: ';
            const invalidQuestionSetPaths: Array<string>
                = invalidQuestionSets.map((questionSet: WatchedQuestionSet) => questionSet.questionSetPath);
            reason += invalidQuestionSetPaths.join(', ');
        }
        reason += '.';
        return reason;
    }

    public shouldPerformInitialCalculation(): boolean {
        if (!StringHelper.isNullOrEmpty(this.applicationService.calculationResultId)) {
            return false;
        }
        if (this.fieldsRequiredForCalculation.length == 0 && this.questionSetsRequiredForCalculation.length == 0) {
            return false;
        }
        let hasInvalidFieldsRequiredForCalculation: boolean = this.fieldsRequiredForCalculation.filter(
            (field: WatchedField) => !field.valid).length > 0;
        let hasInvalidQuestionSetsRequiredForCalculation: boolean = this.questionSetsRequiredForCalculation.filter(
            (questionSet: WatchedQuestionSet) => !questionSet.valid).length > 0;
        let hasFieldsWithInitialValueAndAffectsPremiumOrTriggers: boolean =
            this.getFieldsWithInitialValueAndAffectsPremiumOrTriggers().length > 0;
        return !hasInvalidFieldsRequiredForCalculation
            && !hasInvalidQuestionSetsRequiredForCalculation
            && hasFieldsWithInitialValueAndAffectsPremiumOrTriggers;
    }

    public getFieldsWithInitialValueAndAffectsPremiumOrTriggers(): Array<WatchedField> {
        let formElementsWithAffectsPremiumAndAffectsTrigger: Array<WatchedField> =
            this.fieldsWithInitialValueAndAffectsPremiumOrAffectsTriggers.filter(
                (fe: CalculationAffectingElement) => fe.valid);
        return formElementsWithAffectsPremiumAndAffectsTrigger;
    }

    private triggerCalculation(forPremium: boolean, forTrigger: boolean): void {
        // just in case this results in a cancel and re-request, let's keep status as in progress
        // so the spinner doesn't flicker
        if (this.calculationInProgress) {
            this.setCalculationPending(forPremium, forTrigger);
        }
        const immediate: boolean = this.shouldCalculationBeImmediate();

        // since we're about to trigger a calculation, let's cancel any existing ones right away, so we
        // don't have to wait for the throttling to complete before cancelling. Might as well cancel
        // right away and then do nothing until the next calculation timeslot opens up.
        this.operationInstructionService.abortExecutingAndDeleteQueuedCalculationOperations();

        this.triggerCalculationSubject.next([forPremium, forTrigger, immediate]);
    }

    private shouldCalculationBeImmediate(): boolean {
        const now: number = performance.now();
        this.previousCalculationTimestamp = this.calculationTimestamp;
        this.calculationTimestamp = now;
        const timeSinceLastCalculationMillis: number = now - this.previousCalculationTimestamp;
        const itsBeenAWhile: boolean = timeSinceLastCalculationMillis > CalculationService.longDebounceMillis;
        return !this.calculationInProgress && itsBeenAWhile;
    }

    private listenForTriggeredCalculations(): void {
        this.triggerCalculationSubject.pipe(debounceTime(20))
            .subscribe(async ([forPremium, forTrigger, immediate]: [boolean, boolean, boolean]) => {
                this.setCalculationPending(forPremium, forTrigger);
                if (CalculationService.useCache) {
                    const requestPayload: any = this.calculationOperation.getNextPayload();
                    const cachedResponse: any
                        = await this.calculationResponseCache.tryGetCachedResponse(requestPayload);
                    if (cachedResponse) {
                        this.setCalculationPending(false, false);
                        if (this.applicationService.debug) {
                            console.log('using cached calculation result');
                        }
                        this.eventService.calculationResponseSubject.next(cachedResponse);
                    }
                    let isSilent: boolean = cachedResponse != null;
                    this.triggerTimelyCalculation(forPremium, forTrigger, immediate, isSilent);
                } else {
                    this.triggerTimelyCalculation(forPremium, forTrigger, immediate, false);
                }
            });
    }

    private triggerTimelyCalculation(
        forPremium: boolean,
        forTrigger: boolean,
        immediate: boolean,
        isSilent: boolean,
    ): void {
        if (isSilent) {
            this.incrementBackgroundCalculationCount();
        }
        if (immediate) {
            // perform the calculation immediately
            this.performCalculation(forPremium, forTrigger, isSilent);
        } else {
            // debounce so we don't overload the server
            this.calculationRequestSubject.next([forPremium, forTrigger, isSilent]);
        }
    }

    private listenForLongDebounceCalculationRequests(): void {
        this.listenToSkippedCalculationCalls();
        this.calculationRequestSubject.pipe(
            debounceTime(CalculationService.longDebounceMillis),
        ).subscribe(([forPremium, forTrigger, isSilent]: [boolean, boolean, boolean]) => {
            this.performCalculation(forPremium, forTrigger, isSilent);
        });
    }

    private listenToSkippedCalculationCalls(): void {
        const calls: Observable<[boolean, boolean, boolean]> =
            this.calculationRequestSubject.asObservable();
        // Apply debounceTime to the calls
        const debouncedCalls: Observable<[boolean, boolean, boolean]> =
            calls.pipe(debounceTime(CalculationService.longDebounceMillis));
        // Use buffer to collect skipped calls
        const skippedCalls: Observable<Array<[boolean, boolean, boolean]>> =
            calls.pipe(buffer(debouncedCalls));
        // Subscribe to the skipped calls and process them
        skippedCalls.subscribe((skipped: Array<[boolean, boolean, boolean]>) => {
            for (let skip of skipped) {
                let isSilent: boolean = skip[2]; // the third parameter is isSilent.
                if (isSilent) {
                    this.decrementBackgroundCalculationCount();
                }
            }
        });
    }

    private performCalculation(forPremium: boolean, forTrigger: boolean, isSilent: boolean): void {
        // double check nothing has changed since the request was originally requested
        // this could be up to 2 seconds later due to throttling
        if (!this.canProceedWithCalculation()) {
            if (this.applicationService.debug) {
                console.log(this.getReasonCalculationDidNotProceed());
            }
            this.setCalculationPending(false, false);
            return;
        }
        if (!isSilent) {
            this.incrementCalculationCount(forPremium, forTrigger);
        }
        this.setCalculationPending(false, false);
        let operationConfig: OperationConfiguration = {
            name: 'calculation',
            backgroundExecution: true,
        };
        const operation: OperationInstruction = new OperationInstruction(
            operationConfig);
        const args: CalculationOperationArguments = { silent: isSilent };
        operation.args = args as OperationArguments;
        operation.completedSubject.pipe(take(1)).subscribe((reason: string) => {
            if (this.applicationService.debug && this.applicationService.debugLevel > 2) {
                console.log('Completed calculation: ' + reason);
            }
            if (!isSilent) {
                this.decrementCalculationCount();
            } else {
                this.decrementBackgroundCalculationCount();
            }
        });
        this.operationInstructionService.execute(operation, true);
    }

    private listenForCalculationResponses(): void {
        this.eventService.calculationResponse$.subscribe((response: any) => this.processCalculationResponse(response));
    }

    private listenForRetriggerCalculation(): void {
        this.eventService.retriggerCalculationSubject.subscribe(() => {
            this.generateQuoteRequest(false, false);
        });
    }

    public processCalculationResponse(response: any): void {
        // In development environment, warn about incomplete calculation results
        if (this.applicationService.formType == FormType.Quote
            && this.premiumCalculationInProgress
            && this.applicationService.latestQuoteResult.calculationState
            == CalculationState.Incomplete
            && this.applicationService.environment == DeploymentEnvironment.Development
        ) {
            console.warn("We received a premium calculation back which was incomplete. "
                + 'Please check that fields which are required for calculation are marked as required, '
                + 'and question sets which are required for calculation are designated as such in the '
                + 'workflow configuration.');
        }

        if (this.applicationService.formType == FormType.Quote) {
            this.eventService.quoteResponseSubject.next(response);
        } else if (this.applicationService.formType == FormType.Claim) {
            this.eventService.claimResponseSubject.next(response);
        } else {
            throw new Error("When trying to process a calculation response, " +
                "we couldn't work out what form type this is for.");
        }
    }

    private getRequiredFieldIndex(fieldPath: string): number {
        return this.fieldsRequiredForCalculation.findIndex((data: WatchedField) => {
            return fieldPath && data.fieldPath && data.fieldPath == fieldPath;
        });
    }

    private getRequiredQuestionSetIndex(questionSetPath: string): number {
        return this.questionSetsRequiredForCalculation.findIndex((data: WatchedQuestionSet) => {
            return questionSetPath && data.questionSetPath && data.questionSetPath == questionSetPath;
        });
    }

    private getInitialFieldIndex(fieldPath: string): number {
        return this.fieldsWithInitialValueAndAffectsPremiumOrAffectsTriggers
            .findIndex((data: WatchedField) => {
                return fieldPath && data.fieldPath && data.fieldPath == fieldPath;
            });
    }
}
