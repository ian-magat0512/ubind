import { filter, takeUntil } from 'rxjs/operators';
import {
    Component, OnInit, ElementRef, AfterViewInit, HostBinding, HostListener } from '@angular/core';
import { trigger, state, style, animate, transition } from '@angular/animations';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { DynamicWidget } from '../dynamic.widget';
import { FormService } from '@app/services/form.service';
import { ConfigService } from '@app/services/config.service';
import { WorkflowService } from '@app/services/workflow.service';
import { CalculationService } from '@app/services/calculation.service';
import { ApplicationService } from '@app/services/application.service';
import { SourceRatingSummaryItem } from '@app/models/source-rating-summary-item';
import { ApplicationStatus } from '@app/models/application-status.enum';
import { CalculationState } from '@app/models/calculation-result-state';
import { QuoteType } from '@app/models/quote-type.enum';
import { CurrencyHelper } from '@app/helpers/currency.helper';
import { TextWithExpressions } from '@app/expressions/text-with-expressions';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { Expression } from '@app/expressions/expression';
import { FormType } from '@app/models/form-type.enum';
import { StringHelper } from '@app/helpers/string.helper';
import { SidebarTextElementsService } from '@app/services/sidebar-text-elements.service';
import { ClaimState } from '@app/models/claim-state.enum';
import { QuoteState } from '@app/models/quote-state.enum';
import { QuoteResult } from '@app/models/quote-result';
import { ClaimResult } from '@app/models/claim-result';
import { CalculationResult } from '@app/models/calculation-result';
import { ClaimResultProcessor } from '@app/services/claim-result-processor';
import { QuoteResultProcessor } from '@app/services/quote-result-processor';
import { ApplicationMode } from '@app/models/application-mode.enum';
import { WorkflowDestinationService } from '@app/services/workflow-destination.service';
import { EventService } from '@app/services/event.service';
import { BrowserDetectionService } from '@app/services/browser-detection.service';

/**
 * A summary property which is shown in the calculation widget in the summary table
 */
export interface SummaryProperty {
    key: string;
    value: string;
    sortValue: number;
}

/**
 * Export calculation widget component class.
 * This class manage calculation widgets functions.
 */
@Component({
    selector: 'calculation-widget',
    templateUrl: './calculation.widget.html',
    animations: [
        trigger('sidebarAnimation', [
            state('in', style({ transform: 'translateX(0)' })),
            transition('void => *', [
                style({
                    opacity: 0,
                    transform: 'translateX(-10px)',
                }),
                animate('400ms 400ms ease-out'),
            ]),
            transition('* => void', [
                animate('400ms ease-in', style({
                    opacity: 0,
                    transform: 'translateX(-10px)',
                })),
            ]),
        ]),
        trigger('loadingAnimation', [
            state('in', style({ transform: 'translateX(0)' })),
            transition('void => *', [
                style({
                    opacity: 0,
                    transform: 'translateX(50px)',
                }),
                animate('100ms ease-out'),
            ]),
            transition('* => void', [
                animate('100ms ease-in', style({
                    opacity: 0,
                    transform: 'translateX(50px)',
                })),
            ]),
        ]),
    ],
    styleUrls: [
        './calculation.widget.scss',
    ],
})

export class CalculationWidget extends DynamicWidget implements OnInit, AfterViewInit {

    /**
     * Instead of using "@media (max-width: 767px) {" style queries, we instead
     * work off the following css classes to know if we're in mobile size.
     * The reasons for this are:
     * 1. The mobile breakpoint is set by the form configuration.
     * 2. When loading the webform in the portal, the viewport width returned by the media query
     * doesn't include the scroll bar, which it normally would. Using our css classes "mobile-size"
     * and "wider-than-mobile" ensures the scrollbar width is taken into account.
     */
    @HostBinding('class.mobile-width')
    public isMobileWidth: boolean = true;

    @HostBinding('class.wider-than-mobile')
    public isWiderThanMobile: boolean = false;

    public paymentOptionsDefinition: any = { 'affectsPremium': true };
    public oldStateCssClass: string = '';
    public calculationStateCssClass: string = '';
    public triggerTypeCssClass: string = '';
    public applicationStateCssClass: string = '';

    public headerActionsEmpty: boolean = true;
    public bodyActionsEmpty: boolean = true;

    protected displayablePriceAmount: number;
    public initialInstalmentAmount: string = null;
    public numberOfInstalments: string = null;

    public headerText: string;
    private currentHeaderTextExpressionSource: string;
    private headerTextExpression: TextWithExpressions;

    public priceLabelText: string;
    private currentPriceLabelTextExpressionSource: string;
    private priceLabelTextExpression: TextWithExpressions;

    public messageText: string;
    private currentMessageTextExpressionSource: string;
    private messageTextExpression: TextWithExpressions;

    private summaryHeadingTextExpression: TextWithExpressions;
    private quoteReferenceLabelExpression: TextWithExpressions;

    public loading: boolean = false;
    protected referralMessage: string = '';
    public showPaymentOptions: boolean;
    public displayPrice: boolean = false;
    public sourceRatingSummaryItems: Array<SourceRatingSummaryItem>;
    protected quoteReference: string = '';
    public canShowQuoteReference: boolean = false;
    public quoteReferenceLabelText: string = "Quote ref: ";
    public summaryProperties: Array<SummaryProperty>;
    public visibleSummaryPropertyCount: number;
    public summaryHeadingText: string;
    public stringHelper: typeof StringHelper = StringHelper;

    private lastCalculationResult: CalculationResult;

    /**
     * The message which is shown to someone when they are in test mode.
     * Typically this message will say that payment transaction amounts will be the minimimum 
     * allowed by the payment gateway.
     */
    public testMessageText: string;

    private hasShownSummaryLabelExpressionDeprecatedWarning: boolean = false;

    public constructor(
        protected currencyPipe: CurrencyPipe,
        protected formService: FormService,
        protected configService: ConfigService,
        protected workflowService: WorkflowService,
        public calculationService: CalculationService,
        public applicationService: ApplicationService,
        protected elementRef: ElementRef,
        protected expressionDependencies: ExpressionDependencies,
        private sidebarTextElementsService: SidebarTextElementsService,
        protected workflowDestinationService: WorkflowDestinationService,
        private eventService: EventService,
        private browserDetectionService: BrowserDetectionService,
    ) {
        super(workflowService, configService, workflowDestinationService);
    }

    public ngOnInit(): void {
        super.ngOnInit();
        this.updatePaymentOptionsVisibility();
        this.generateTestMessageText();
        this.applicationService.calculationInProgressSubject.pipe(
            takeUntil(this.destroyed))
            .subscribe((inProgress: boolean) => {
                this.onCalculation(inProgress);
            });

        this.eventService.quoteResultSubject
            .pipe(
                filter((qs: QuoteResult) => qs != null),
                takeUntil(this.destroyed),
            )
            .subscribe((quoteResult: QuoteResult) => {
                this.onCalculationResult(quoteResult);
            });
        this.eventService.claimResultSubject
            .pipe(
                filter((qs: ClaimResult) => qs != null),
                takeUntil(this.destroyed),
            )
            .subscribe((claimResult: ClaimResult) => {
                this.onCalculationResult(claimResult);
            });

        this.setupSummaryHeadingTextExpression();
        this.setupQuoteReferenceLabelTextExpression();

        let initialCalculationResult: CalculationResult;
        if (this.applicationService.formType == FormType.Claim) {
            initialCalculationResult = this.applicationService.latestClaimResult
                ? this.applicationService.latestClaimResult
                : ClaimResultProcessor.createEmpty();
        } else if (this.applicationService.formType == FormType.Quote) {
            initialCalculationResult = this.applicationService.latestQuoteResult
                ? this.applicationService.latestQuoteResult
                : QuoteResultProcessor.createEmpty();
        } else {
            throw new Error("Unexpected: could not determine the form type - whether it's for a quote or a claim.");
        }
        this.onCalculationResult(initialCalculationResult);
        this.onConfigurationUpdated();
        this.onWindowResize(null);
    }

    private onConfigurationUpdated(): void {
        this.eventService.updatedConfiguration.pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                if (this.lastCalculationResult) {
                    this.setupSummaryHeadingTextExpression();
                    this.setupQuoteReferenceLabelTextExpression();
                    this.updateQuoteReferenceVisibility();
                    this.updatePaymentOptionsVisibility();
                }
            });
    }

    public ngAfterViewInit(): void {
        super.ngAfterViewInit();
    }

    protected destroyExpressions(): void {
        this.headerTextExpression?.dispose();
        this.headerTextExpression = null;
        this.priceLabelTextExpression?.dispose();
        this.priceLabelTextExpression = null;
        this.messageTextExpression?.dispose();
        this.messageTextExpression = null;
        this.summaryHeadingTextExpression?.dispose();
        this.summaryHeadingTextExpression = null;
        this.quoteReferenceLabelExpression?.dispose();
        this.quoteReferenceLabelExpression = null;
        super.destroyExpressions();
    }

    private generateTestMessageText(): void {
        if (this.applicationService.isTestData) {
            this.testMessageText = `<strong>This is a test quote:</strong> All associated data including any `
                + `policies generated will be tagged as test objects.`;
            let usesStripe: boolean = this.configService.settings?.paymentForm?.['type'] == 'stripe';
            let minimumAmount: number = usesStripe ? 0.5 : 0.01;
            let minimumAmountFormatted: string =
                this.expressionDependencies.expressionMethodService.currencyAsString(
                    minimumAmount, null, null);
            this.testMessageText += ` All associated credit card transactions `
                + `will use an amount of <strong>${minimumAmountFormatted}</strong>.`;
        }
    }

    protected onCalculation(inProgress: boolean): void {
        this.loading = inProgress;
    }

    private updateQuoteReferenceVisibility(): void {
        if (this.configService.theme?.includeQuoteReferenceInSidebar
            && this.applicationService?.applicationState != ApplicationStatus.Nascent
        ) {
            this.canShowQuoteReference = true;
            this.quoteReference = this.applicationService.quoteReference;
        } else {
            this.canShowQuoteReference = false;
        }
    }

    private updatePaymentOptionsVisibility(): void {
        this.showPaymentOptions = this.configService.theme?.showPaymentOptionsInSidebar ?? true;
    }

    private setupSummaryHeadingTextExpression(): void {
        if (this.summaryHeadingTextExpression) {
            this.summaryHeadingTextExpression.dispose();
            this.summaryHeadingTextExpression = null;
        }
        if (this.configService.textElements?.sidebar?.summaryLabel?.text) {
            let textExpressionSource: any = this.configService.textElements.sidebar.summaryLabel.text;
            this.summaryHeadingTextExpression = new TextWithExpressions(
                textExpressionSource,
                this.expressionDependencies,
                'calculation widget summary label text');
            this.summaryHeadingTextExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: string) => this.summaryHeadingText = text);
            this.summaryHeadingTextExpression.triggerEvaluationWhenFormLoaded();
        } else {
            this.summaryHeadingText = 'Quote Summary';
        }
    }

    private setupQuoteReferenceLabelTextExpression(): void {
        if (this.quoteReferenceLabelExpression) {
            this.quoteReferenceLabelExpression.dispose();
            this.quoteReferenceLabelExpression = null;
        }
        if (this.configService.textElements?.sidebar?.quoteReferenceLabel?.text) {
            let textExpressionSource: any = this.configService.textElements.sidebar.quoteReferenceLabel.text;
            this.quoteReferenceLabelExpression = new TextWithExpressions(
                textExpressionSource,
                this.expressionDependencies,
                'calculation widget quote reference label text');
            this.quoteReferenceLabelExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((text: string) => this.quoteReferenceLabelText = text);
            this.quoteReferenceLabelExpression.triggerEvaluationWhenFormLoaded();
        } else {
            this.quoteReferenceLabelText = 'Quote ref: ';
        }
    }

    public onCalculationResult(calculationResult: CalculationResult): void {
        this.lastCalculationResult = calculationResult;
        this.onCalculationResultUpdatePrice(calculationResult);
        this.onCalculationResultUpdatePriceLabelText(calculationResult);
        this.onCalculationResultUpdateHeaderText(calculationResult);
        this.onCalculationResultUpdateMessage(calculationResult);
        this.onCalculationResultUpdateSummaryProperties(calculationResult);
        this.onCalculationResultUpdateCssClasses(calculationResult);
        this.updateQuoteReferenceVisibility();

        this.loading = false;
    }

    private onCalculationResultUpdateCssClasses(calculationResult: CalculationResult): void {
        const applicationState: string = this.applicationService.applicationState || ApplicationStatus.Nascent;
        if (applicationState != ApplicationStatus.Approved && applicationState
            != ApplicationStatus.Endorsement && applicationState != ApplicationStatus.Review) {
            this.oldStateCssClass = calculationResult.oldStateDeprecated
                ? 'state-' + calculationResult.oldStateDeprecated
                : '';
        } else {
            this.oldStateCssClass = '';
        }

        this.applicationStateCssClass = applicationState
            ? 'application-state-' + applicationState
            : '';
        this.calculationStateCssClass = calculationResult.calculationState
            ? 'calculation-state-' + calculationResult.calculationState
            : '';
        this.triggerTypeCssClass = calculationResult.trigger && calculationResult.trigger.type
            ? this.triggerTypeCssClass = 'trigger-state-' + calculationResult.trigger.type
            : '';
    }

    /**
     * Set the message with the following order of precedence:
     * 1. If we're doing a review/endorse, set the review or endorse message
     * 1. If it's set in the trigger, and the quote/claim is nascent or incomplete, use that
     * 2. If payable is $0 or negative, set it according to the refundMessage or noPaymentMessage
     * 3. If it's not nascent or incomplete, set it according 
     * to application status by matching it against the associated text element
     * 4. Set it according to the calculation state by matching it against the associated text element
     * @param calculationResult 
     */
    private onCalculationResultUpdateMessage(calculationResult: CalculationResult): void {
        const quoteType: QuoteType = this.applicationService.quoteType || QuoteType.NewBusiness;
        const isReviewOrEndorseMode: boolean = this.applicationService.mode == ApplicationMode.Review ||
            this.applicationService.mode == ApplicationMode.Endorse;
        if (isReviewOrEndorseMode) {
            let propertyName: string = this.applicationService.mode == ApplicationMode.Review
                ? 'reviewMessage' : 'endorsementMessage';
            let messageTextSource: any =
                this.sidebarTextElementsService.getSidebarTextElementForQuoteType(quoteType, propertyName);
            if (messageTextSource) {
                this.updateMessageTextSubscription(messageTextSource);
                return;
            }
        }

        if (this.isNascentOrIncomplete(calculationResult)) {
            if (calculationResult.trigger && calculationResult.trigger.message) {
                // it doesn't need to be parsed for expressions 
                // because that's already been done to quoteResult.trigger.message
                if (this.messageTextExpression) {
                    this.messageTextExpression.dispose();
                    delete this.messageTextExpression;
                }
                this.messageText = calculationResult.trigger.message;

                // store this so we know if it changes later
                this.currentMessageTextExpressionSource = calculationResult.trigger.message;
                return;
            }
        }

        // try to set the refund or no payment message if appropriate
        const hasPayableAmount: boolean = calculationResult.amountPayable != null;
        const payableAmount: number = hasPayableAmount ?
            CurrencyHelper.commaSeparatedToNumber(calculationResult.amountPayable) : null;
        if (this.displayPrice && hasPayableAmount && payableAmount <= 0) {
            if (payableAmount < 0) {
                let refundMessageSource: any = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                    quoteType,
                    'refundMessage');
                if (refundMessageSource) {
                    this.updateMessageTextSubscription(refundMessageSource);
                    return;
                }
            } else { // payableAmount == 0
                let noPaymentMessageSource: any =
                    this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                        quoteType,
                        'noPaymentMessage');
                if (noPaymentMessageSource) {
                    this.updateMessageTextSubscription(noPaymentMessageSource);
                    return;
                }
            }
        }

        // try to set it according to application status
        let messageTextSource: string;
        if (!this.isNascentOrIncomplete(calculationResult)) {
            if (this.isQuoteResult(calculationResult)) {
                let quoteState: string = calculationResult.quoteState == QuoteState.Approved &&
                    calculationResult.trigger ? 'approved' : 'autoApproved';
                messageTextSource = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                    quoteType, quoteState + 'Message');
            } else if (this.isClaimResult(calculationResult)) {
                messageTextSource = this.sidebarTextElementsService.getSidebarTextElementForClaim(
                    calculationResult.claimState + 'Message');
            }
            if (messageTextSource) {
                this.updateMessageTextSubscription(messageTextSource);
                return;
            }
        }

        // try to set it according to calculation state
        messageTextSource = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
            quoteType, calculationResult.calculationState + 'Message');
        if (messageTextSource) {
            this.updateMessageTextSubscription(messageTextSource);
            return;
        }

        // If nothing has set it to anything, then it needs to be blanked out
        this.updateMessageTextSubscription('');
    }

    /**
     * Set the header text with the following order of precedence:
     * 1. If we're doing a review/endorse, set the review or endorse header
     * 2. If it's set in the trigger, and the quote/claim is nascent or incomplete, use that
     * 3. If payable is $0 or negative, set it according to the refundHeader or noPaymentHeader
     * 4. If it's not nascent or incmplete, set it according to 
     * application status by matching it against the associated text element
     * 5. Set it according to the calculation state by matching it against the associated text element
     * @param calculationResult 
     */
    private onCalculationResultUpdateHeaderText(calculationResult: CalculationResult): void {
        const quoteType: QuoteType = this.applicationService.quoteType || QuoteType.NewBusiness;
        const isReviewOrEndorseMode: boolean = this.applicationService.mode == ApplicationMode.Review ||
            this.applicationService.mode == ApplicationMode.Endorse;
        if (isReviewOrEndorseMode) {
            let propertyName: string = this.applicationService.mode == ApplicationMode.Review
                ? 'reviewHeader' : 'endorsementHeader';
            let headerTextSource: any = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(quoteType,
                propertyName);
            if (headerTextSource) {
                this.updateHeaderTextSubscription(headerTextSource);
                return;
            }
        }

        if (this.isNascentOrIncomplete(calculationResult)) {
            if (calculationResult.trigger && !StringHelper.isNullOrEmpty(calculationResult.trigger.header)) {
                // it doesn't need to be parsed for expressions 
                // because that's already been done to quoteResult.trigger.header
                if (this.headerTextExpression) {
                    this.headerTextExpression.dispose();
                    delete this.headerTextExpression;
                }
                this.headerText = calculationResult.trigger.header;
                this.currentHeaderTextExpressionSource = null;
                return;
            }
        }

        // try to set the refund or no payment header if appropriate
        const hasPayableAmount: boolean = calculationResult.amountPayable != null;
        const payableAmount: number = hasPayableAmount ?
            CurrencyHelper.commaSeparatedToNumber(calculationResult.amountPayable) : null;
        if (this.displayPrice && hasPayableAmount && payableAmount <= 0) {
            if (payableAmount < 0) {
                let refundHeaderSource: any = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                    quoteType,
                    'refundHeader');
                if (refundHeaderSource) {
                    this.updateHeaderTextSubscription(refundHeaderSource);
                    return;
                }
            } else { // payableAmount == 0
                let noPaymentHeaderSource: any = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                    quoteType,
                    'noPaymentHeader');
                if (noPaymentHeaderSource) {
                    this.updateHeaderTextSubscription(noPaymentHeaderSource);
                    return;
                }
            }
        }

        // try to set it according to application status
        let headerTextSource: string;
        if (!this.isNascentOrIncomplete(calculationResult)) {
            if (this.isQuoteResult(calculationResult)) {
                let quoteState: string = calculationResult.quoteState == QuoteState.Approved &&
                    calculationResult.trigger ? 'approved' : 'autoApproved';
                headerTextSource = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                    quoteType, quoteState + 'Header');
            } else if (this.isClaimResult(calculationResult)) {
                headerTextSource = this.sidebarTextElementsService.getSidebarTextElementForClaim(
                    calculationResult.claimState + 'Header');
            }
            if (headerTextSource) {
                this.updateHeaderTextSubscription(headerTextSource);
                return;
            }
        }

        // try to set it according to calculation state
        headerTextSource = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(quoteType,
            calculationResult.calculationState + 'Header');
        if (headerTextSource) {
            this.updateHeaderTextSubscription(headerTextSource);
            return;
        }
    }

    /**
     * Set the price label text with the following order of precedence:
     * 1. If payable is $0 or negative, set it according to the refundHeader or noPaymentHeader
     * 2. If it's set in the trigger, and the quote/claim state is nascent or incomplete, use that     
     * 3. If it's not nascent or incmplete, set it according 
     * to application status by matching it against the associated text element
     * 4. Set it according to the calculation state by matching it against the associated text element
     * @param calculationResult 
     */
    private onCalculationResultUpdatePriceLabelText(calculationResult: CalculationResult): void {
        // try to set the refund or no payment label if appropriate
        const quoteType: QuoteType = this.applicationService.quoteType || QuoteType.NewBusiness;
        if (this.displayablePriceAmount != null && this.displayablePriceAmount <= 0) {
            if (this.displayablePriceAmount < 0) {
                let refundLabelSource: any = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                    quoteType,
                    'refundLabel');
                if (refundLabelSource) {
                    this.updatePriceLabelTextSubscription(refundLabelSource);
                    return;
                }
            } else { // this.displayablePriceAmount == 0
                let noPaymentLabelSource: any = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                    quoteType,
                    'noPaymentLabel');
                if (noPaymentLabelSource) {
                    this.updatePriceLabelTextSubscription(noPaymentLabelSource);
                    return;
                }
            }
        }

        if (this.isNascentOrIncomplete(calculationResult)) {
            if (calculationResult.trigger && !StringHelper.isNullOrEmpty(calculationResult.trigger.title)) {
                // it doesn't need to be parsed for expressions 
                // because that's already been done to quoteResult.trigger.title
                if (this.priceLabelTextExpression) {
                    this.priceLabelTextExpression.dispose();
                    delete this.priceLabelTextExpression;
                }
                this.updatePriceLabelTextSubscription(calculationResult.trigger.title);
                return;
            }
        }

        // try to set it according to application status
        let priceLabelTextSource: string;
        const isReviewOrEndorseMode: boolean = this.applicationService.mode == ApplicationMode.Review ||
            this.applicationService.mode == ApplicationMode.Endorse;
        if (!this.isNascentOrIncomplete(calculationResult) && !isReviewOrEndorseMode) {
            if (this.isQuoteResult(calculationResult)) {
                let quoteState: string = calculationResult.quoteState == QuoteState.Approved &&
                    calculationResult.trigger ? 'approved' : 'autoApproved';
                priceLabelTextSource = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(
                    quoteType, quoteState + 'Label');
            } else if (this.isClaimResult(calculationResult)) {
                priceLabelTextSource = this.sidebarTextElementsService.getSidebarTextElementForClaim(
                    calculationResult.claimState + 'Label');
            }
            if (priceLabelTextSource) {
                this.updatePriceLabelTextSubscription(priceLabelTextSource);
                return;
            }
        }

        // try to set it according to calculation state
        priceLabelTextSource = this.sidebarTextElementsService.getSidebarTextElementForQuoteType(quoteType,
            calculationResult.calculationState + 'Label');
        if (priceLabelTextSource) {
            this.updatePriceLabelTextSubscription(priceLabelTextSource);
            return;
        }
    }

    private updateMessageTextSubscription(messageTextExpressionSource: string): void {
        if (messageTextExpressionSource != this.currentMessageTextExpressionSource) {
            this.currentMessageTextExpressionSource = messageTextExpressionSource;
            if (this.messageTextExpression) {
                this.messageTextExpression.dispose();
            }
            this.messageTextExpression = new TextWithExpressions(
                messageTextExpressionSource,
                this.expressionDependencies,
                'calculation widget message text');
            this.messageTextExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.messageText = value);
            this.messageTextExpression.triggerEvaluationWhenFormLoaded();
        }
    }

    private updateHeaderTextSubscription(headerTextExpressionSource: string): void {
        if (headerTextExpressionSource != this.currentHeaderTextExpressionSource) {
            this.currentHeaderTextExpressionSource = headerTextExpressionSource;
            if (this.headerTextExpression) {
                this.headerTextExpression.dispose();
            }
            this.headerTextExpression = new TextWithExpressions(
                headerTextExpressionSource,
                this.expressionDependencies,
                'calculation widget header text');
            this.headerTextExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.headerText = value);
            this.headerTextExpression.triggerEvaluationWhenFormLoaded();
        }
    }

    private updatePriceLabelTextSubscription(priceLabelTextExpressionSource: string): void {
        if (priceLabelTextExpressionSource != this.currentPriceLabelTextExpressionSource) {
            this.currentPriceLabelTextExpressionSource = priceLabelTextExpressionSource;
            if (this.priceLabelTextExpression) {
                this.priceLabelTextExpression.dispose();
            }
            this.priceLabelTextExpression = new TextWithExpressions(
                priceLabelTextExpressionSource,
                this.expressionDependencies,
                'calculation widget price label text');
            this.priceLabelTextExpression.nextResultObservable.pipe(takeUntil(this.destroyed))
                .subscribe((value: string) => this.priceLabelText = value);
            this.priceLabelTextExpression.triggerEvaluationWhenFormLoaded();
        }
    }

    private onCalculationResultUpdateSummaryProperties(calculationResult: CalculationResult): void {
        const sourceItems: Array<SourceRatingSummaryItem> = calculationResult.ratingSummaryItems;
        let summaryProperties: Array<SummaryProperty> = new Array<SummaryProperty>();
        if (sourceItems) {
            sourceItems.forEach((item: SourceRatingSummaryItem) => {
                let summaryProperty: SummaryProperty = <SummaryProperty>{
                    key: '',
                    value: item.value,
                    sortValue: item.defaultPosition,
                };
                if (item.summaryLabel) {
                    let summaryLabelExpression: TextWithExpressions = new TextWithExpressions(
                        item.summaryLabel,
                        this.expressionDependencies,
                        'calculation widget summary label');
                    summaryLabelExpression.nextResultObservable
                        .pipe(takeUntil(this.destroyed))
                        .subscribe((result: string) => {
                            summaryProperty.key = result;
                            this.updateVisibleSummaryPropertyCount();
                        });
                    summaryLabelExpression.triggerEvaluation();
                }
                if (item.summaryPositionExpression) {
                    let summaryPositionExpression: Expression = new Expression(item.summaryPositionExpression,
                        this.expressionDependencies, 'calculation widget summary position');
                    summaryPositionExpression.nextResultObservable
                        .pipe(takeUntil(this.destroyed))
                        .subscribe((result: number) => {
                            summaryProperty.sortValue = result;
                            if (this.summaryProperties) {
                                this.summaryProperties = this.sortSummaryProperties(this.summaryProperties);
                            }
                        });
                    summaryPositionExpression.triggerEvaluation();
                }
                summaryProperties.push(summaryProperty);
            });
            summaryProperties = this.sortSummaryProperties(summaryProperties);
        }
        this.summaryProperties = summaryProperties;
        this.updateVisibleSummaryPropertyCount();
    }

    private updateVisibleSummaryPropertyCount(): void {
        if (this.summaryProperties) {
            this.visibleSummaryPropertyCount
                = this.summaryProperties.filter((sp: SummaryProperty) => !StringHelper.isNullOrEmpty(sp.key)).length;
        }
    }

    private sortSummaryProperties(summaryProperties: Array<SummaryProperty>): Array<SummaryProperty> {
        return summaryProperties.sort((a: SummaryProperty, b: SummaryProperty) => {
            return (a.sortValue < b.sortValue)
                ? -1
                : (a.sortValue == b.sortValue) ? 0 : 1;
        });
    }

    private onCalculationResultUpdatePrice(calculationResult: CalculationResult): void {
        this.displayPrice = false;
        if (calculationResult.calculationState == CalculationState.Incomplete) {
            // do not display the price since the calculation state is incomplete
            return;
        }
        let triggerSaysNotToDisplayPrice: boolean = calculationResult.trigger != null &&
            calculationResult.trigger.displayPrice == false;
        let thereIsNoPaymentInformation: boolean = calculationResult.payment == null;
        let isReviewOrEndorseMode: boolean = this.applicationService.mode == ApplicationMode.Review ||
            this.applicationService.mode == ApplicationMode.Endorse;
        let isQuoteApproved: boolean =
            this.isQuoteResult(calculationResult) && calculationResult.quoteState == QuoteState.Approved;
        if ((!isReviewOrEndorseMode && !isQuoteApproved && triggerSaysNotToDisplayPrice)
            || thereIsNoPaymentInformation
            // TODO: work out if a price can be displayed for a claim. 
            // For now claims does not display a monetary amount.
            || this.applicationService.formType == FormType.Claim
        ) {
            return;
        }

        this.initialInstalmentAmount = null;
        let payment: any = calculationResult.payment;
        let funding: any = calculationResult.funding;
        let regularInstalmentAmountString: string;
        let initialInstalmentAmountString: string;
        // we don't currently support instalments for adjustments or cancellations. 
        // The calculation result shouldn't actually be sending it 
        // through, but if it is we are going to ignore it anyway.
        let isAdjustmentOrCancellation: boolean =
            this.applicationService.quoteType == QuoteType.Adjustment ||
            this.applicationService.quoteType == QuoteType.Cancellation;

        this.displayablePriceAmount = null;
        if (!isAdjustmentOrCancellation && payment && payment.instalments &&
            payment.instalments.instalmentsPerYear > 1 && calculationResult.funding != null) {
            regularInstalmentAmountString = '' + funding.regularInstalmentAmount || funding.regularInstalmentAmount;
            initialInstalmentAmountString = '' + funding.initialInstalmentAmount || funding.intialInstalmentAmount;
            this.numberOfInstalments = funding.numberOfInstalments;
            this.displayablePriceAmount =
                this.expressionDependencies.expressionMethodService.currencyAsNumber(regularInstalmentAmountString);
            if (regularInstalmentAmountString != initialInstalmentAmountString) {
                let firstAmount: number =
                    this.expressionDependencies.expressionMethodService.currencyAsNumber(
                        initialInstalmentAmountString);
                this.initialInstalmentAmount =
                    this.expressionDependencies.expressionMethodService.currencyAsString(firstAmount,
                        true, null);
            }
        } else if (!isAdjustmentOrCancellation && payment.instalments &&
            payment.instalments.instalmentAmount) {
            this.displayablePriceAmount =
                this.expressionDependencies.expressionMethodService.currencyAsNumber(
                    payment.instalments.instalmentAmount);
            this.numberOfInstalments = null;
        } else {
            this.displayablePriceAmount =
                this.expressionDependencies.expressionMethodService.currencyAsNumber(
                    calculationResult.amountPayable);
            this.numberOfInstalments = null;
        }

        if (this.displayablePriceAmount != null) {
            this.displayPrice = true;
        }
    }

    public onHeaderActionsEmpty(empty: boolean): void {
        this.headerActionsEmpty = empty;
    }

    public onBodyActionsEmpty(empty: boolean): void {
        this.bodyActionsEmpty = empty;
    }

    private isNascentOrIncomplete(calculationResult: CalculationResult): boolean {
        if (this.isClaimResult(calculationResult)) {
            return calculationResult.claimState == ClaimState.Nascent ||
                calculationResult.claimState == ClaimState.Incomplete;
        } else if (this.isQuoteResult(calculationResult)) {
            return calculationResult.quoteState == QuoteState.Nascent ||
                calculationResult.quoteState == QuoteState.Incomplete;
        } else {
            throw new Error("Fatal Error: Could not determing whether the form is for a quote or a claim.");
        }
    }

    private isQuoteResult(calculationResult: any): calculationResult is QuoteResult {
        return calculationResult.quoteState != undefined;
    }

    private isClaimResult(calculationResult: any): calculationResult is ClaimResult {
        return calculationResult.claimState != undefined;
    }

    @HostListener("window:resize", ['$event'])
    protected onWindowResize(event: any): void {
        this.isMobileWidth = this.browserDetectionService.isMobileWidth();
        this.isWiderThanMobile = !this.isMobileWidth;
    }
}
