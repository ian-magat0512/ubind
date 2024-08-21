import { Component, DebugElement, EventEmitter, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfigurationOperation } from '@app/operations/configuration.operation';
import { ConfigProcessorService } from '@app/services/config-processor.service';
import { EvaluateService } from '@app/services/evaluate.service';
import { EventService } from '@app/services/event.service';
import { MessageService } from '@app/services/message.service';
import { sharedConfig } from '@app/app.module.shared';
import { CalculationService } from '@app/services/calculation.service';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { FormService } from '@app/services/form.service';
import { AttachmentService } from '@app/services/attachment.service';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { ApplicationService } from '@app/services/application.service';
import { AlertService } from '@app/services/alert.service';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { BroadcastService } from '@app/services/broadcast.service';
import { CssProcessorService } from '@app/services/css-processor.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { BehaviorSubject, Subject } from 'rxjs';
import { ValidationService } from '@app/services/validation.service';
import { ExpressionMethodService } from '@app/expressions/expression-method.service';
import { OperationFactory } from '@app/operations/operation.factory';
import { WebhookService } from '@app/services/webhook.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { PolicyOperation } from '@app/operations/policy.operation';
import { UserService } from '@app/services/user.service';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { TimePipe } from '@app/pipes/time.pipe';
import { AbnPipe } from '@app/pipes/abn.pipe';
import { BsbPipe } from '@app/pipes/bsb.pipe';
import { CreditCardNumberPipe } from '@app/pipes/credit-card-number.pipe';
import { PhoneNumberPipe } from '@app/pipes/phone-number.pipe';
import { NumberPlatePipe } from '@app/pipes/number-plate.pipe';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { NgSelectModule } from '@ng-select/ng-select';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { By } from '@angular/platform-browser';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { OptionSetChangePublisher } from '@app/services/option-set-change-publisher';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { PriceWidget, ShowSign } from './price.widget';
import { CalculationState, TriggerState } from '@app/models/calculation-result-state';
import { QuoteResult } from '@app/models/quote-result';
import { QuoteState } from '@app/models/quote-state.enum';
import { SourceRatingSummaryItem } from '@app/models/source-rating-summary-item';
import { FormType } from '@app/models/form-type.enum';
import { ApplicationMode } from '@app/models/application-mode.enum';
import { QuoteType } from '@app/models/quote-type.enum';
import { DeploymentEnvironment } from '@app/models/deployment-environment';
import { ToolTipService } from '@app/services/tooltip.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationStatusService } from '@app/services/operation-status.service';
import { LocaleService } from '@app/services/locale.service';
import { MaskPipe } from 'ngx-mask';

/* global spyOn */

// eslint-disable-next-line prefer-arrow/prefer-arrow-functions
function createQuoteResult(
    amount: string = "$123.45",
    calculationState: CalculationState = CalculationState.PremiumEstimate,
    triggerState: TriggerState = null,
): QuoteResult {
    let quoteResult: QuoteResult = <any>{};
    quoteResult.oldStateDeprecated = calculationState;
    quoteResult.calculationState = calculationState;
    quoteResult.quoteState = QuoteState.Incomplete;
    quoteResult.triggerState = triggerState;
    quoteResult.amountPayable = amount;
    quoteResult.payment = {
        instalments: {
            instalmentsPerYear: 1,
            instalmentAmount: amount,
        },
    };
    quoteResult.ratingSummaryItems = [
        <SourceRatingSummaryItem>{
            summaryLabel: 'Summary Label 1',
            value: "Summary Value 1",
        },
    ];
    return quoteResult;
}

// disabled due to intermittent failures. Will be fixed in UB-10955
xdescribe('PriceWidget', () => {
    let fixture: ComponentFixture<PriceWidget>;
    let sut: PriceWidget;
    let eventService: EventService;
    let applicationService: ApplicationService;

    let workflowServiceStub: any = {
        navigate: new EventEmitter<any>(),
        currentDestination: { stepName: "step1" },
        initialised: new BehaviorSubject<boolean>(true),
        actionAborted: new EventEmitter<any>(),
        actionCompleted: new EventEmitter<any>(),
        completedNavigationIn: (): void => { },
        quoteLoadedSubject: new Subject<boolean>(),
        loadedCustomerHasUserSubject: new Subject<boolean>(),
        navigateToSubject: new Subject<boolean>(),
    };

    let alertServiceStub: any = {
        updateSubject: new Subject<Alert>(),
        visibleSubject: new Subject<boolean>(),
        alert: (alert: Alert): void => {},
    };

    let broadcastServiceStub: any = {
        on: (key: any): Subject<any> => new Subject<any>(),
    };

    let operationFactoryStub: any = {
        getStatus: (operation: any): string => 'success',
    };

    let webhookServiceStub: any = {
        getActiveWebhookCount: (): number => 0,
        inProgressSubject: new Subject<boolean>(),
        webhookFieldInProgressSubject: new Subject<boolean>(),
    };

    let attachmentOperationStub: any = {
        operationInProgress: false,
        inProgressSubject: new Subject<boolean>(),
    };

    let policyOperationStub: any = {
        getQuoteType: (): string => 'renewal',
    };

    let encryptionServiceStub: any = {
        loadPublicKey: (): void => {},
    };

    let localeService: LocaleService = new LocaleService();

    /**
     * Test host component class
     */
    @Component({
        selector: `host-component`,
        template: `<web-form *ngIf="ready"></web-form>`,
        // We need to use OnPush change detection here so that we don't get
        // ExpressionChangedAfterItHasBeenCheckedError during unit tests.
        /* changeDetection: ChangeDetectionStrategy.OnPush*/
    })
    class TestHostComponent {
        public ready: boolean = false;
    }

    beforeEach(async () => {
        return TestBed.configureTestingModule({
            declarations: [
                TestHostComponent,
                ...sharedConfig.declarations,
            ],
            providers: [
                { provide: EncryptionService, useValue: encryptionServiceStub },
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                { provide: EvaluateService, useClass: EvaluateService },
                { provide: EventService, useClass: EventService },
                { provide: CalculationService, useClass: CalculationService },
                { provide: WorkflowService, useValue: workflowServiceStub },
                ConfigService,
                { provide: FormService, useClass: FormService },
                { provide: AttachmentService, useClass: AttachmentService },
                { provide: CalculationOperation, useValue: {} },
                { provide: ApplicationService, useClass: ApplicationService },
                { provide: AlertService, useValue: alertServiceStub },
                { provide: WindowScrollService, useValue: {} },
                { provide: BroadcastService, useValue: broadcastServiceStub },
                { provide: CssProcessorService, useValue: {} },
                { provide: ValidationService, useClass: ValidationService },
                ExpressionMethodService,
                { provide: OperationFactory, useValue: operationFactoryStub },
                { provide: WebhookService, useValue: webhookServiceStub },
                { provide: AttachmentOperation, useValue: attachmentOperationStub },
                { provide: PolicyOperation, useValue: policyOperationStub },
                { provide: UserService, useClass: UserService },
                { provide: ResumeApplicationService, useClass: ResumeApplicationService },
                { provide: OperationFactory, useClass: FakeOperationFactory },
                { provide: LocaleService, useValue: localeService },
                { provide: MaskPipe, useClass: MaskPipe },
                ToolTipService,
                AbnPipe,
                BsbPipe,
                CreditCardNumberPipe,
                CurrencyPipe,
                CssIdentifierPipe,
                TimePipe,
                PhoneNumberPipe,
                NumberPlatePipe,
                ExpressionDependencies,
                WorkflowStatusService,
                NotificationService,
                ConfigurationV2Processor,
                OptionSetChangePublisher,
                BrowserDetectionService,
                RevealGroupTrackingService,
                OperationStatusService,
                OperationInstructionService,
            ],
            imports: [
                NoopAnimationsModule,
                NgSelectModule,
                ...sharedConfig.imports,
            ],
            schemas: [
                CUSTOM_ELEMENTS_SCHEMA,
            ],
        }).compileComponents().then(() => {
            let messageService: MessageService = TestBed.inject<MessageService>(MessageService);
            spyOn(messageService, 'sendMessage'); // make it do nothing.

            fixture = TestBed.createComponent(PriceWidget);
            applicationService = TestBed.inject<ApplicationService>(ApplicationService);
            eventService = TestBed.inject<EventService>(EventService);
            applicationService.setApplicationConfiguration(
                null,
                'test-tenant-id',
                'test-tenant-alias',
                null,
                null,
                true,
                'test-product-id',
                'test-product-alias',
                DeploymentEnvironment.Staging,
                FormType.Quote,
                null,
                null,
                null,
                null,
                null,
                ApplicationMode.Create,
                QuoteType.NewBusiness,
                null,
                false,
                false,
                0,
                false,
                null,
                null);
            let expressionMethodService: ExpressionMethodService = TestBed.inject(ExpressionMethodService);
            spyOn(expressionMethodService, 'getCurrencyCode').and.returnValue('AUD');
            fixture.detectChanges();
        });
    });

    afterEach(() => {
        fixture.destroy();
    });

    // Intermittently fails. To be fixed in UB-10955
    xit('when the show-sign attribute is not set, the sign is shown when the price is negative', async () => {
        // Arrange
        sut = fixture.componentInstance;
        let quoteResult: QuoteResult = createQuoteResult("-$123.34");

        // Act
        await sut.onCalculationResult(quoteResult).then(() => {
            fixture.detectChanges();

            // Assert
            const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
            expect(debugElement != null).toBeTruthy("The sign span should be rendered");
            const signValue: string = debugElement.nativeElement.innerHTML;
            expect(signValue).toBe('-');
        });
    });

    it('when the show-sign attribute is not set, the sign is not shown when the price is positive', () => {
        // Arrange
        sut = fixture.componentInstance;
        let quoteResult: QuoteResult = createQuoteResult("$123.34");

        // Act
        eventService.quoteResultSubject.next(quoteResult);
        fixture.detectChanges();

        // Assert
        const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
        expect(debugElement == null).toBeTruthy("The sign span should not be rendered");
    });

    it('when the show-sign attribute is set to "never", the sign is not shown when the price is negative', () => {
        // Arrange
        sut = fixture.componentInstance;
        sut.showSign = ShowSign.Never;
        let quoteResult: QuoteResult = createQuoteResult("-$123.34");

        // Act
        eventService.quoteResultSubject.next(quoteResult);
        fixture.detectChanges();

        // Assert
        const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
        expect(debugElement == null).toBeTruthy("The sign span should not be rendered");
    });

    // Intermittently fails. To be fixed in UB-10955
    xit('when the show-sign attribute is set to "always", the sign is shown when the price is positive', async () => {
        // Arrange
        sut = fixture.componentInstance;
        sut.showSign = ShowSign.Always;
        let quoteResult: QuoteResult = createQuoteResult("$123.34");

        // Act
        await sut.onCalculationResult(quoteResult).then(() => {
            fixture.detectChanges();

            // Assert
            const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
            expect(debugElement != null).toBeTruthy("The sign span should be rendered");
            const signValue: string = debugElement.nativeElement.innerHTML;
            expect(signValue).toBe('+');
        });
    });

    it('when the show-sign attribute is set to "always", the sign is shown when the price is negative', async () => {
        // Arrange
        sut = fixture.componentInstance;
        sut.showSign = ShowSign.Always;
        let quoteResult: QuoteResult = createQuoteResult("-$123.34");

        // Act
        await sut.onCalculationResult(quoteResult).then(() => {
            fixture.detectChanges();

            // Assert
            const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
            expect(debugElement != null).toBeTruthy("The sign span should be rendered");
            const signValue: string = debugElement.nativeElement.innerHTML;
            expect(signValue).toBe('-');
        });
    });

    // Intermittently fails. To be fixed in UB-10955
    xit('when the show-sign attribute is set to "negative", the sign is shown when the price is negative', async () => {
        // Arrange
        sut = fixture.componentInstance;
        sut.showSign = ShowSign.Negative;
        let quoteResult: QuoteResult = createQuoteResult("-$123.34");

        // Act
        await sut.onCalculationResult(quoteResult).then(() => {
            fixture.detectChanges();

            // Assert
            const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
            expect(debugElement != null).toBeTruthy("The sign span should be rendered");
            const signValue: string = debugElement.nativeElement.innerHTML;
            expect(signValue).toBe('-');
        });
    });

    it('when the show-sign attribute is set to "negative", '
        + 'the sign is not shown when the price is positive', () => {
        // Arrange
        sut = fixture.componentInstance;
        sut.showSign = ShowSign.Negative;
        let quoteResult: QuoteResult = createQuoteResult("$123.34");

        // Act
        eventService.quoteResultSubject.next(quoteResult);
        fixture.detectChanges();

        // Assert
        const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
        expect(debugElement == null).toBeTruthy("The sign span should not be rendered");
    });

    it('when the show-sign attribute is set to "positive", '
        + 'the sign is not shown when the price is negative', () => {
        // Arrange
        sut = fixture.componentInstance;
        sut.showSign = ShowSign.Positive;
        let quoteResult: QuoteResult = createQuoteResult("-$123.34");

        // Act
        eventService.quoteResultSubject.next(quoteResult);
        fixture.detectChanges();

        // Assert
        const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
        expect(debugElement == null).toBeTruthy("The sign span should not be rendered");
    });

    // To be fixed on UB-10955
    xit('when the show-sign attribute is set to "positive", '
        + 'the sign is shown when the price is positive', async () => {
        // Arrange
        sut = fixture.componentInstance;
        sut.showSign = ShowSign.Positive;
        let quoteResult: QuoteResult = createQuoteResult("$123.34");

        // Act
        await sut.onCalculationResult(quoteResult).then(() => {
            fixture.detectChanges();

            // Assert
            const debugElement: DebugElement = fixture.debugElement.query(By.css('.currency-sign'));
            expect(debugElement != null).toBeTruthy("The sign span should be rendered");
            const signValue: string = debugElement.nativeElement.innerHTML;
            expect(signValue).toBe('+');
        });
    });

});
