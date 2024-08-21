import { Injectable } from '@angular/core';
import { ExpressionInputSubjectService } from './expression-input-subject.service';
import { interval, Observable, Subject, SubscriptionLike } from 'rxjs';
import { distinctUntilChanged, filter, shareReplay } from 'rxjs/operators';
import { ApplicationService } from '@app/services/application.service';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { WebhookService } from '@app/services/webhook.service';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { FormService } from '@app/services/form.service';
import { RepeatingQuestionSetTrackingService } from '@app/services/repeating-question-set-tracking.service';
import { EventService } from '@app/services/event.service';

/**
 * Export expression method dependency service class.
 * TODO: Write a better class header: dependencies of the expression method function.
 */
@Injectable({
    providedIn: 'root',
})
export class ExpressionMethodDependencyService {

    private quoteHasCustomerSubject: Subject<void>;

    /**
     * The number of milliseconds after which expressions which depend on the current time are re-evaluated
     */
    private timeUpdateIntervalMillis: number = 1000;
    private timeUpdateSubscription: SubscriptionLike;

    public constructor(
        private expressionInputSubjectService: ExpressionInputSubjectService,
        private applicationService: ApplicationService,
        private workflowStatusService: WorkflowStatusService,
        private resumeApplicationService: ResumeApplicationService,
        private attachmentOperation: AttachmentOperation,
        protected webhookService: WebhookService,
        private formService: FormService,
        private repeatingQuestionSetTrackingService: RepeatingQuestionSetTrackingService,
        private eventService: EventService,
    ) {
        this.createExpressionMethodSubjects();
    }

    public deallocate(): void {
        this.timeUpdateSubscription?.unsubscribe();
    }

    /**
     * Creates rxjs subjects for each of the expression methods which have dependent and possibly
     * changing inputs (other than their passed parameters)
     */
    private createExpressionMethodSubjects(): void {
        this.createTimeDependantExpressionMethodSubjects();
        this.createOperationResultDependentExpressionMethodSubjects();
        this.createCalculationResultDependentExpressionMethodSubjects();
        this.createWorkflowStepDependentExpressionMethodSubjects();
        this.createArticleIndexDependentExpressionMethodSubjects();
        this.createArticleElementIndexDependentExpressionMethodSubjects();
        this.createRepeatingFieldDependentExpressionMethodSubjects();
        this.createCanNavigateToStepDependentSubjects();
        this.createCanNavigateToArticleDependentSubjects();
        this.createCanNavigateToArticleElementDependentSubjects();
        this.createCanNavigateToRepeatingInstanceDependentSubjects();
        this.createVisibleQuestionSetsExpressionMethodSubjects();
        this.createSectionWidgetExpressionMethodSubjects();
        this.createPortalIdSubject();
        this.createPortalAliasSubject();
        this.createQuoteStateSubject();
        this.createClaimStateSubject();
        this.createApplicationStatusSubject();
        this.createApplicationModeSubject();
        this.createApplicationValuesSubject();
        this.createFieldDataSubjects();
        this.createQuoteTypeSubject();
        this.createPolicyIdSubject();
        this.createQuoteIdSubject();
        this.createClaimIdSubject();
        this.createQuoteHasCustomerSubject();
        this.createUserAccountSubject();
        this.createCustomerHasAccountSubject();
        this.createQuoteReferenceSubject();
        this.createPreviousQuoteIdSubject();
        this.createPreviousClaimIdSubject();
        this.createCurrentUserSubject();
        this.createCalculationDependentExpressionMethodSubjects();
        this.createBackgroundCalculationDependentExpressionMethodSubjects();
        this.createAttachmentDependentExpressionMethodSubjects();
        this.createWebhookRequestDependentExpressionMethodSubjects();
        this.createActionDependentExpressionMethodSubjects();
        this.createOperationDependentExpressionMethodSubjects();
        this.createNavigationDependentExpressionMethodSubjects();
        this.createDebugSettingDependentExpressionMethodSubjects();
        this.createDebugLevelDependentExpressionMethodSubjects();
        this.createCurrencyCodeDependentExpressionMethodSubjects();
        this.createQuestionSetValidityDependentExpressionMethodSubjects();
    }

    private getExpressionMethodSubjectsFor(methods: Array<string>): Array<Subject<void>> {
        let subjects: Array<Subject<void>> = new Array<Subject<void>>();
        for (let method of methods) {
            subjects.push(
                this.expressionInputSubjectService.createExpressionMethodSubject(method));
        }
        return subjects;
    }

    private notifyExpressionMethodSubjects(source: Observable<any>, subjects: Array<Subject<void>>): void {
        source
            .pipe(shareReplay({ bufferSize: 1, refCount: true }))
            .subscribe(() => {
                for (let subject of subjects) {
                    subject.next();
                }
            });
    }

    private createQuoteStateSubject(): void {
        let quoteStateSubject: Subject<void>
            = this.expressionInputSubjectService.createExpressionMethodSubject('getQuoteState');
        this.applicationService.quoteStateSubject.subscribe((quoteState: string) => {
            quoteStateSubject.next();
        });
    }

    private createClaimStateSubject(): void {
        let claimStateSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getClaimState');
        this.applicationService.claimStateSubject.subscribe((claimState: string) => {
            claimStateSubject.next();
        });
    }

    /**
     * @deprecated
     */
    private createApplicationStatusSubject(): void {
        let applicationStatusSubject: Subject<void>
            = this.expressionInputSubjectService.createExpressionMethodSubject('getApplicationState');
        this.applicationService.claimStateSubject.subscribe((claimState: string) => {
            applicationStatusSubject.next();
        });
        this.applicationService.quoteStateSubject.subscribe((quoteState: string) => {
            applicationStatusSubject.next();
        });
    }

    private createCurrentUserSubject(): void {
        let currentUserSubject: Subject<void>
            = this.expressionInputSubjectService.createExpressionMethodSubject('getCurrentUserPersonDetails');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name === 'currentUser'))
            .subscribe(() => {
                currentUserSubject.next();
            });
    }

    private createApplicationModeSubject(): void {
        let applicationModeSubject: Subject<void>
            = this.expressionInputSubjectService.createExpressionMethodSubject('getApplicationMode');
        this.applicationService.applicationPropertyChangedSubject.subscribe(() => {
            applicationModeSubject.next();
        });
    }

    private createApplicationValuesSubject(): void {
        // This one may be deprecated but we have it here just in case
        let applicationValuesSubject: Subject<void>
            = this.expressionInputSubjectService.createExpressionMethodSubject('getApplicationValues');
        this.applicationService.applicationPropertyChangedSubject.subscribe(() => {
            applicationValuesSubject.next();
        });
    }

    private createFieldDataSubjects(): void {
        let fieldDataDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'getFieldPropertyValue',
            'getFieldData',
        ]);
        this.notifyExpressionMethodSubjects(
            this.formService.fieldDataUpdatedSubject,
            fieldDataDependentSubjects);
    }

    private createPortalIdSubject(): void {
        let subject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getPortalId');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'portalId'))
            .subscribe(() => subject.next());
    }

    private createPortalAliasSubject(): void {
        let subject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getPortalAlias');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'portalAlias'))
            .subscribe(() => subject.next());
    }

    private createQuoteTypeSubject(): void {
        let quoteTypeSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getQuoteType');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'quoteType'))
            .subscribe((val: any) => quoteTypeSubject.next());
    }

    private createPolicyIdSubject(): void {
        let policyIdSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getPolicyId');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'policyId'))
            .subscribe(() => policyIdSubject.next());
    }

    private createQuoteIdSubject(): void {
        let quoteIdSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getQuoteId');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'quoteId'))
            .subscribe(() => quoteIdSubject.next());
    }

    private createClaimIdSubject(): void {
        let claimIdSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getClaimId');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'claimId'))
            .subscribe(() => claimIdSubject.next());
    }

    private createQuoteHasCustomerSubject(): void {
        this.quoteHasCustomerSubject =
            this.expressionInputSubjectService.createExpressionMethodSubject('quoteHasCustomer');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'customerId'))
            .subscribe(() => this.quoteHasCustomerSubject.next());
    }

    private createUserAccountSubject(): void {
        let userHasAccountSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('userHasAccount');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'userHasAccount'))
            .subscribe(() => userHasAccountSubject.next());
        this.workflowStatusService.quoteLoadedSubject
            .pipe(shareReplay({ bufferSize: 1, refCount: true }))
            .subscribe(() => userHasAccountSubject.next());
    }

    private createCustomerHasAccountSubject(): void {
        let customerHasAccountSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('customerHasAccount');
        this.quoteHasCustomerSubject.subscribe(() => customerHasAccountSubject.next());
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'userHasAccount'))
            .subscribe(() => customerHasAccountSubject.next());
        this.workflowStatusService.loadedCustomerHasUserSubject
            .pipe(shareReplay({ bufferSize: 1, refCount: true }))
            .subscribe(() => customerHasAccountSubject.next());
    }

    private createQuoteReferenceSubject(): void {
        let getQuoteReferenceSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getQuoteReference');
        this.applicationService.applicationPropertyChangedSubject
            .pipe(filter((property: any) => property.name == 'quoteReference'))
            .subscribe(() => getQuoteReferenceSubject.next());
    }

    private createPreviousQuoteIdSubject(): void {
        let getPreviousQuoteIdSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getPreviousQuoteId');
        this.resumeApplicationService.savedQuoteIdChangeSubject.subscribe(() => getPreviousQuoteIdSubject.next());
    }

    private createPreviousClaimIdSubject(): void {
        let getPreviousClaimIdSubject: Subject<void> =
            this.expressionInputSubjectService.createExpressionMethodSubject('getPreviousClaimId');
        this.resumeApplicationService.savedClaimIdChangeSubject.subscribe(() => getPreviousClaimIdSubject.next());
    }

    private createTimeDependantExpressionMethodSubjects(): void {
        let timeDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'now',
            'today',
            'thisMonth',
            'thisYear',
            'inThePast',
            'inTheFuture',
            'inTheNextYears',
            'inTheNextMonths',
            'inTheNextDays',
            'inTheLastYears',
            'inTheLastMonths',
            'inTheLastDays',
            'atLeastYearsInTheFuture',
            'atLeastMonthsInTheFuture',
            'atLeastDaysInTheFuture',
            'atMostYearsInTheFuture',
            'atMostMothsInTheFuture',
            'atMostDaysInTheFuture',
            'atLeastYearsInThePast',
            'atLeastMonthsInThePast',
            'atLeastDaysInThePast',
            'atMostYearsInThePast',
            'atMostMonthsInThePast',
            'atMostDaysInThePast',
        ]);

        // Every second we'll let expressions know that the current time has updated 
        // itself to be a second later (if they have subscribed to the relevant 
        // time dependent subject)
        if (this.timeUpdateSubscription) {
            this.timeUpdateSubscription.unsubscribe();
        }

        const timeUpdateObservable: Observable<number> = interval(this.timeUpdateIntervalMillis);
        this.timeUpdateSubscription = timeUpdateObservable
            .pipe(shareReplay({ bufferSize: 1, refCount: true }))
            .subscribe(() => {
                for (let subject of timeDependentSubjects) {
                    subject.next(null);
                }
            });
    }

    private createOperationResultDependentExpressionMethodSubjects(): void {
        let operationDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'operationStatus',
            'operationHasError',
            'operationErrorCode',
            'operationErrorMessage',
        ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.operationResultSubject,
            operationDependentSubjects);
    }

    private createCalculationResultDependentExpressionMethodSubjects(): void {
        let calculationResultDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'getQuoteResult',
            'getClaimResult',
            'getCalculationResult',
            'getCalculationQuoteState',
            'getCalculationClaimState',
            'getQuoteCalculationState',
            'getClaimCalculationState',
            'getActiveTrigger',
            'getActiveTriggerByType',
            'getCalculationResultByPath',
            'getQuoteResultByPath',
            'getClaimResultByPath',
            'calculationResult',
            'calculationHasActiveTriggerOfType',
            'generateSummaryTableOfActiveTriggersOfType',
            'getFinancialTransactionType',
        ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.calculationResultSubject,
            calculationResultDependentSubjects);
    }

    private createWorkflowStepDependentExpressionMethodSubjects(): void {
        let workflowStepDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'getCurrentWorkflowStep',
            'getPreviousWorkflowStep',
        ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.workflowStepSubject,
            workflowStepDependentSubjects);
    }

    private createArticleIndexDependentExpressionMethodSubjects(): void {
        let articleIndexDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'isFirstArticle',
            'isLastArticle',
            'getFirstArticleIndex',
            'getLastArticleIndex',
            'hasNextArticle',
            'hasPreviousArticle',
            'getCurrentArticleIndex',
            'getNextArticleIndex',
            'getPreviousArticleIndex',
        ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.articleIndexSubject,
            articleIndexDependentSubjects);

        // We also need to trigger these to re-evaluate when the section widget changes so it can get the new
        // values it calcultes
        this.notifyExpressionMethodSubjects(
            this.applicationService.sectionWidgetStatusSubject,
            articleIndexDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.eventService.articleVisibleChangeSubject,
            articleIndexDependentSubjects);
    }

    private createArticleElementIndexDependentExpressionMethodSubjects(): void {
        let articleElementIndexDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'isFirstArticleElement',
            'isLastArticleElement',
            'getFirstArticleElementIndex',
            'getLastArticleElementIndex',
            'hasNextArticleElement',
            'hasPreviousArticleElement',
            'getCurrentArticleElementIndex',
            'getNextArticleElementIndex',
            'getPreviousArticleElementIndex',
        ]);

        this.notifyExpressionMethodSubjects(
            this.applicationService.articleElementIndexSubject,
            articleElementIndexDependentSubjects);

        // We also need to trigger these to re-evaluate when the section widget changes so it can get the new
        // values it calcultes
        this.notifyExpressionMethodSubjects(
            this.applicationService.sectionWidgetStatusSubject,
            articleElementIndexDependentSubjects);

        // we also need to trigger these if a question set becomes hidden/unhidden since that would affect which
        // is the next/previous article element index:
        this.notifyExpressionMethodSubjects(
            this.eventService.questionSetVisibleChangeSubject,
            articleElementIndexDependentSubjects);
    }

    private createRepeatingFieldDependentExpressionMethodSubjects(): void {
        let repeatingFieldDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'getCurrentRepeatingFieldPath',
            'getRepeatingInstanceCount',
            'getCurrentRepeatingInstanceIndex',
            'getRepeatingFieldMaxQuantity',
            'getRepeatingFieldMinQuantity',
            'isFirstRepeatingInstance',
            'isLastRepeatingInstance',
            'hasNextRepeatingInstance',
            'hasPreviousRepeatingInstance',
            'getNextRepeatingInstanceIndex',
            'getPreviousRepeatingInstanceIndex',
            'getLastRepeatingInstanceIndex',
            'getFirstRepeatingInstanceIndex',
            'getRepeatingFieldDisplayMode',
        ]);
        this.notifyExpressionMethodSubjects(
            this.repeatingQuestionSetTrackingService.repeatingFieldChangeSubject,
            repeatingFieldDependentSubjects);
    }

    private createSectionWidgetExpressionMethodSubjects(): void {
        let sectionWidgetDependentSubjects: Array<Subject<void>> =
            this.getExpressionMethodSubjectsFor(['getCurrentWorkflowStepDisplayMode']);
        this.notifyExpressionMethodSubjects(
            this.applicationService.sectionWidgetStatusSubject,
            sectionWidgetDependentSubjects);
    }

    private createCanNavigateToStepDependentSubjects(): void {
        let canNavigateToStepDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'canNavigateToNextStep',
            'canNavigateToPreviousStep',
        ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.sectionWidgetStatusSubject,
            canNavigateToStepDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.applicationService.articleIndexSubject,
            canNavigateToStepDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.applicationService.articleElementIndexSubject,
            canNavigateToStepDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.repeatingQuestionSetTrackingService.repeatingFieldChangeSubject,
            canNavigateToStepDependentSubjects);

        // we also need to trigger these if a question set becomes hidden/unhidden since that would affect which
        // is the next/previous article element index:
        this.notifyExpressionMethodSubjects(
            this.eventService.questionSetVisibleChangeSubject,
            canNavigateToStepDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.eventService.articleVisibleChangeSubject,
            canNavigateToStepDependentSubjects);
    }

    private createCanNavigateToArticleDependentSubjects(): void {
        let canNavigateToArticleDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'canNavigateToNextArticle',
            'canNavigateToPreviousArticle',
        ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.sectionWidgetStatusSubject,
            canNavigateToArticleDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.applicationService.articleIndexSubject,
            canNavigateToArticleDependentSubjects);

        // we also need to trigger these if a question set becomes hidden/unhidden since that would affect which
        // is the next/previous article element index:
        this.notifyExpressionMethodSubjects(
            this.eventService.questionSetVisibleChangeSubject,
            canNavigateToArticleDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.eventService.articleVisibleChangeSubject,
            canNavigateToArticleDependentSubjects);
    }

    private createCanNavigateToArticleElementDependentSubjects(): void {
        let canNavigateToArticleElementDependentSubjects: Array<Subject<void>> =
            this.getExpressionMethodSubjectsFor([
                'canNavigateToNextArticleElement',
                'canNavigateToPreviousArticleElement',
            ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.sectionWidgetStatusSubject,
            canNavigateToArticleElementDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.applicationService.articleElementIndexSubject,
            canNavigateToArticleElementDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.repeatingQuestionSetTrackingService.repeatingFieldChangeSubject,
            canNavigateToArticleElementDependentSubjects);

        // we also need to trigger these if a question set becomes hidden/unhidden since that would affect which
        // is the next/previous article element index:
        this.notifyExpressionMethodSubjects(
            this.eventService.questionSetVisibleChangeSubject,
            canNavigateToArticleElementDependentSubjects);
    }

    private createCanNavigateToRepeatingInstanceDependentSubjects(): void {
        let canNavigateToRepeatingInstanceDependentSubjects: Array<Subject<void>> =
            this.getExpressionMethodSubjectsFor([
                'canNavigateToNextRepeatingInstance',
                'canNavigateToPreviousRepeatingInstance',
            ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.sectionWidgetStatusSubject,
            canNavigateToRepeatingInstanceDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.applicationService.articleElementIndexSubject,
            canNavigateToRepeatingInstanceDependentSubjects);
        this.notifyExpressionMethodSubjects(
            this.repeatingQuestionSetTrackingService.repeatingFieldChangeSubject,
            canNavigateToRepeatingInstanceDependentSubjects);
    }

    private createVisibleQuestionSetsExpressionMethodSubjects(): void {
        let visibleQuestionSetsSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'getVisibleQuestionSets',
        ]);
        this.notifyExpressionMethodSubjects(
            this.formService.visibleQuestionSetPathsChangedSubject,
            visibleQuestionSetsSubjects);
    }

    private createCalculationDependentExpressionMethodSubjects(): void {
        let calculationDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'calculationsAreActive',
            'calculationInProgress',
        ]);
        this.notifyExpressionMethodSubjects(
            this.applicationService.calculationInProgressSubject,
            calculationDependentSubjects);
    }

    private createBackgroundCalculationDependentExpressionMethodSubjects(): void {
        let calculationDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'backgroundCalculationInProgress',
        ]);

        this.notifyExpressionMethodSubjects(
            this.applicationService.backgroundCalculationInProgressSubject,
            calculationDependentSubjects);
    }

    private createAttachmentDependentExpressionMethodSubjects(): void {
        let attachmentDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'attachmentUploadsAreActive',
            'attachmentUploadsInProgress',
        ]);
        this.attachmentOperation.inProgressSubject
            .pipe(distinctUntilChanged())
            .subscribe((inProgress: boolean) => {
                for (let subject of attachmentDependentSubjects) {
                    subject.next();
                }
            });
    }

    private createWebhookRequestDependentExpressionMethodSubjects(): void {
        let webhookDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'webhookRequestsAreActive',
            'webhookRequestsInProgress',
        ]);

        let webhookFieldRequestDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'webhookRequestIsActive',
        ]);

        this.webhookService.inProgressSubject
            .pipe(distinctUntilChanged())
            .subscribe((inProgress: boolean) => {
                for (let subject of webhookDependentSubjects) {
                    subject.next();
                }
            });

        this.notifyExpressionMethodSubjects(
            this.webhookService.webhookFieldInProgressSubject,
            webhookFieldRequestDependentSubjects);
    }

    private createActionDependentExpressionMethodSubjects(): void {
        let actionDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'actionInProgress',
        ]);
        this.workflowStatusService.actionInProgressSubject
            .pipe(
                distinctUntilChanged(),
                shareReplay({ bufferSize: 1, refCount: true }),
            )
            .subscribe((inProgress: boolean) => {
                for (let subject of actionDependentSubjects) {
                    subject.next();
                }
            });
    }

    private createOperationDependentExpressionMethodSubjects(): void {
        let operationDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'operationInProgress',
        ]);
        this.applicationService.operationInProgressSubject
            .pipe(
                distinctUntilChanged(),
                shareReplay({ bufferSize: 1, refCount: true }),
            )
            .subscribe((operationName: string) => {
                for (let subject of operationDependentSubjects) {
                    subject.next();
                }
            });
    }

    private createNavigationDependentExpressionMethodSubjects(): void {
        let navigationDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'navigationInProgress',
        ]);
        this.workflowStatusService.navigationInProgressSubject
            .pipe(
                distinctUntilChanged(),
                shareReplay({ bufferSize: 1, refCount: true }),
            )
            .subscribe((inProgress: boolean) => {
                for (let subject of navigationDependentSubjects) {
                    subject.next();
                }
            });
    }

    private createDebugSettingDependentExpressionMethodSubjects(): void {
        let debugSettingDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'isDebugEnabled',
        ]);
        this.applicationService.debugSubject
            .pipe(distinctUntilChanged())
            .subscribe((debug: boolean) => {
                for (let subject of debugSettingDependentSubjects) {
                    subject.next();
                }
            });
    }

    private createDebugLevelDependentExpressionMethodSubjects(): void {
        let debugLevelDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'getDebugLevel',
        ]);
        this.applicationService.debugLevelSubject
            .pipe(distinctUntilChanged())
            .subscribe((debugLevel: number) => {
                for (let subject of debugLevelDependentSubjects) {
                    subject.next();
                }
            });
    }

    private createCurrencyCodeDependentExpressionMethodSubjects(): void {
        let currencyCodeDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'getCurrencyCode',
            'currencyAsString',
        ]);

        // TODO: make this work out the field based upon a workflow role or quoteDataLocator.
        this.expressionInputSubjectService.getFieldValueObservable('currencyCode')
            .pipe(distinctUntilChanged())
            .subscribe((value: string) => {
                for (let subject of currencyCodeDependentSubjects) {
                    subject.next();
                }
            });
    }

    private createQuestionSetValidityDependentExpressionMethodSubjects(): void {
        let questionSetValidityDependentSubjects: Array<Subject<void>> = this.getExpressionMethodSubjectsFor([
            'questionSetIsValid',
            'questionSetsAreValid',
            'questionSetIsInvalid',
            'questionSetsAreInvalid',
            'questionSetIsValidOrHidden',
            'questionSetsAreValidOrHidden',
            'questionSetIsInvalidOrHidden',
            'questionSetsAreInvalidOrHidden',
        ]);
        this.notifyExpressionMethodSubjects(
            this.formService.questionSetValidityChangeSubject,
            questionSetValidityDependentSubjects);
    }
}
