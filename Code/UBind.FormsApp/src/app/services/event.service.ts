import { Injectable } from '@angular/core';
import { Expression } from '@app/expressions/expression';
import { ClaimResult } from '@app/models/claim-result';
import { WorkingConfiguration } from '@app/models/configuration/working-configuration';
import { QuoteResult } from '@app/models/quote-result';
import { FieldConfiguration } from '@app/resource-models/configuration/fields/field.configuration';
import { OptionConfiguration } from '@app/resource-models/configuration/option.configuration';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

/**
 * Export event service class.
 * TODO: Write a better class header: event functions.
 */
@Injectable({
    providedIn: 'root',
})
export class EventService {
    public appLoadedSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public webFormLoadedSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public scrollingFinishedSubject: Subject<boolean> = new Subject<boolean>();
    public scrollingFinished$: Observable<boolean> = this.scrollingFinishedSubject.asObservable();

    /**
     * A subject which publishes just before rendering the next workflow step.
     * This triggers after animation out has completed and before animation in is
     * about to start.
     */
    public readyForNextStepSubject: Subject<void> = new Subject<void>();
    public readyForNextStep$: Observable<void> = this.readyForNextStepSubject.asObservable();

    /**
     * Every time the sidebar changes height it's published. This is used by the sidebar offset service.
     */
    public sidebarHeightSubject: Subject<number> = new Subject<number>();
    public sidebarBottomSubject: Subject<number> = new Subject<number>();
    public windowResizeSubject: Subject<void> = new Subject<void>();
    public formElementHiddenChangeSubject: Subject<void> = new Subject<void>();

    /**
     * So that alerts will not be cut off, we'll publish the bottom in pixels
     * so that we can resize the content pane to be large enough to show it all.
     */
    public alertBottomSubject: Subject<number> = new Subject<number>();

    /**
     * When a new field path is known to the system (either because a field was rendered,
     * or a quote is loaded/edited and a value exists in the form model) this subject 
     * publishes that fieldpath.
     * 
     * This is used by the MatchingFieldsSubject servce to match fields by json path with a wildcard.
     */
    public fieldPathAddedSubject: Subject<string> = new Subject<string>();
    public fieldPathRemovedSubject: Subject<string> = new Subject<string>();

    /**
     * This publishes the top level page transition state. This is used for animations that
     * want to be in sync with the transition between pages, articles, article elements, and 
     * repeating question instances.
     */
    public pageAnimationTransitionStateSubject: Subject<string> = new Subject<string>();

    /**
     * This publishes when the top level page transition completes. This is used for animations that
     * want to be in sync with the transition between pages, articles, article elements, and 
     * repeating question instances.
     */
    public pageAnimationTransitionCompletedSubject: Subject<string> = new Subject<string>();

    /**
     * When a section widget is initialising, it can be CPU intensive, so we don't want to run certain
     * animations during this time. This subject helps us decide when to run those animations.
     */
    public sectionWidgetCompletedViewInitSubject: Subject<void> = new Subject<void>();

    /**
     * Publishes the percentage completion of the current step.
     */
    public stepCompletionSubject: Subject<number> = new Subject<number>();

    /**
     * This publishes whenever a question set becomes visible or hidden.
     * This is needed so that expressions which use article element index dependent methods
     * can cause the expression to re-evaluate, since a visibility of a question set changing
     * would cause changes to the next/previous question set when using displayMode articleElement.
     */
    public questionSetVisibleChangeSubject: Subject<void> = new Subject<void>();
    public articleVisibleChangeSubject: Subject<void> = new Subject<void>();

    public loadedConfiguration: Subject<WorkingConfiguration> = new Subject<WorkingConfiguration>();
    public updatedConfiguration: Subject<WorkingConfiguration> = new Subject<WorkingConfiguration>();

    public appMinimumHeightSubject: Subject<string> = new Subject<string>();

    private fieldConfigUpdatedSubjectMap: Map<string, Subject<{ old: FieldConfiguration; new: FieldConfiguration}>>
        = new Map<string, Subject<{ old: FieldConfiguration; new: FieldConfiguration}>>();

    private optionSetUpdatedSubjectMap: Map<string, Subject<Array<OptionConfiguration>>>
        = new Map<string, Subject<Array<OptionConfiguration>>>();

    // Whether to render the footer or not
    public renderFooterSubject: Subject<boolean> = new Subject<boolean>();
    public renderFooter$: Observable<boolean> = this.renderFooterSubject.asObservable();

    // Whether to render the header or not
    public renderHeaderSubject: Subject<boolean> = new Subject<boolean>();
    public renderHeader$: Observable<boolean> = this.renderHeaderSubject.asObservable();

    // Where certain fields use debouncing to delay their values from being published, we
    // need to know when they have finished publishing those values, because we can't trigger
    // workflow step button validation until they have actually published, otherwise we might
    // start navigating to the next step when a field ends up being invalid, which would create
    // a form in an invalid state.
    public fieldValuesAndValiditiesAreStableSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);

    // The following allow the ExpressionDebugRegistry to track which expresses currently exist:
    public expressionCreatedSubject: Subject<Expression> = new Subject<Expression>();
    public expressionDisposedSubject: Subject<Expression> = new Subject<Expression>();

    // For publishing app events from operations, since we can't inject AppEventService into
    // operations as that would cause a circular dependency.
    public appEventSubject: Subject<string> = new Subject<string>();

    // each time a calculation response is received, it's published so it can be processed
    // by the calculation service.
    public calculationResponseSubject: Subject<any> = new Subject<any>();
    public calculationResponse$: Observable<any> = this.calculationResponseSubject.asObservable();

    public retriggerCalculationSubject: Subject<void> = new Subject<void>();
    public retriggerCalculation$: Observable<void> = this.retriggerCalculationSubject.asObservable();

    // to avoid circular dependencies we'll publish the quote/claim results so they can be processed
    // separately by the processors.
    public quoteResponseSubject: Subject<any> = new Subject<any>();
    public claimResponseSubject: Subject<any> = new Subject<any>();

    public quoteResultSubject: BehaviorSubject<QuoteResult> = new BehaviorSubject<QuoteResult>(null);
    public claimResultSubject: BehaviorSubject<ClaimResult> = new BehaviorSubject<ClaimResult>(null);

    public getFieldConfigUpdatedSubject(
        fieldKey: string,
    ): Subject<{ old: FieldConfiguration; new: FieldConfiguration}> {
        let subject: Subject<{ old: FieldConfiguration; new: FieldConfiguration}>
            = this.fieldConfigUpdatedSubjectMap.get(fieldKey);
        if (!subject) {
            subject = new Subject<{ old: FieldConfiguration; new: FieldConfiguration}>();
            this.fieldConfigUpdatedSubjectMap.set(fieldKey, subject);
        }
        return subject;
    }

    public getFieldConfigUpdatedObservable(
        fieldKey: string,
    ): Observable<{ old: FieldConfiguration; new: FieldConfiguration}> {
        return this.getFieldConfigUpdatedSubject(fieldKey).asObservable();
    }

    public getOptionSetUpdatedSubject(optionSetKey: string): Subject<Array<OptionConfiguration>> {
        let subject: Subject<Array<OptionConfiguration>> = this.optionSetUpdatedSubjectMap.get(optionSetKey);
        if (!subject) {
            subject = new Subject<Array<OptionConfiguration>>();
            this.optionSetUpdatedSubjectMap.set(optionSetKey, subject);
        }
        return subject;
    }

    public getOptionSetUpdatedObservable(optionSetKey: string): Observable<Array<OptionConfiguration>> {
        return this.getOptionSetUpdatedSubject(optionSetKey).asObservable();
    }
}
