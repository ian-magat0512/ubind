/* eslint-disable no-unused-vars */
/* eslint-disable max-classes-per-file */
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
import { WorkflowNavigation } from '@app/models/workflow-navigation';
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
import { filter } from 'rxjs/operators';
import { By } from '@angular/platform-browser';
import * as _ from 'lodash-es';
import formConfig from './calculation-widget.test-form-config.json';
import defaultTextElements from '../../../services/form-configuration/default-text-elements.json';
import { ApplicationStatus } from '@app/models/application-status.enum';
import { CalculationState, TriggerState } from '@app/models/calculation-result-state';
import { SourceRatingSummaryItem } from '@app/models/source-rating-summary-item';
import { TriggerDisplayConfig } from '@app/models/trigger-display-config';
import { QuoteState } from '@app/models/quote-state.enum';
import { FormType } from '@app/models/form-type.enum';
import { QuoteType } from '@app/models/quote-type.enum';
import { QuoteResult } from '@app/models/quote-result';
import { ApplicationMode } from '@app/models/application-mode.enum';
import { ToolTipService } from '@app/services/tooltip.service';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from '@app/services/operation-status.service';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { PriceWidget } from '../price/price.widget';
import { MaskPipe } from 'ngx-mask';
import { LoggerService } from '@app/services/logger.service';
import { ApiService } from '@app/services/api.service';

/* global spyOn */

/**
 * Fake workflow service class
 */
class FakeWorkflowService {
    public navigate: EventEmitter<any> = new EventEmitter<any>();
    public currentDestination: WorkflowDestination = { stepName: "step1" };
    public currentNavigation: WorkflowNavigation = new WorkflowNavigation(null, { stepName: "step1" });
    public initialised: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
    public actionAborted: EventEmitter<any> = new EventEmitter<any>();
    public actionCompleted: EventEmitter<any> = new EventEmitter<any>();
    public quoteLoadedSubject: Subject<boolean> = new Subject<boolean>();
    public loadedCustomerHasUserSubject: Subject<boolean> = new Subject<boolean>();
    public navigateToSubject: Subject<string> = new Subject<string>();
    public completedNavigationIn(): void { }
}

// eslint-disable-next-line prefer-arrow/prefer-arrow-functions
function createQuoteResult(
    calculationState: CalculationState,
    triggerState: TriggerState,
): QuoteResult {
    let quoteResult: QuoteResult = <any>{};
    quoteResult.oldStateDeprecated = calculationState;
    quoteResult.calculationState = calculationState;
    quoteResult.quoteState = QuoteState.Incomplete;
    quoteResult.triggerState = triggerState;
    quoteResult.amountPayable = "$123.00";
    quoteResult.payment = {
        instalments: {
            instalmentsPerYear: 1,
            instalmentAmount: '$20,000',
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

describe('CalculationWidget', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;
    let applicationService: ApplicationService;
    let calculationService: CalculationService;

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
        loadPublicKey: (): void => { },
    };

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
                { provide: WorkflowService, useClass: FakeWorkflowService },
                { provide: ConfigService, useClass: ConfigService },
                { provide: FormService, useClass: FormService },
                { provide: AttachmentService, useClass: AttachmentService },
                { provide: CalculationOperation, useValue: {} },
                { provide: ApplicationService, useClass: ApplicationService },
                { provide: AlertService, useValue: alertServiceStub },
                { provide: WindowScrollService, useValue: {} },
                { provide: BroadcastService, useValue: broadcastServiceStub },
                { provide: CssProcessorService, useValue: {} },
                { provide: ValidationService, useClass: ValidationService },
                { provide: ExpressionMethodService, useClass: ExpressionMethodService },
                { provide: OperationFactory, useValue: operationFactoryStub },
                { provide: WebhookService, useValue: webhookServiceStub },
                { provide: AttachmentOperation, useValue: attachmentOperationStub },
                { provide: PolicyOperation, useValue: policyOperationStub },
                { provide: UserService, useClass: UserService },
                { provide: ResumeApplicationService, useClass: ResumeApplicationService },
                { provide: OperationFactory, useClass: FakeOperationFactory },
                { provide: ToolTipService, useClass: FakeToolTipService },
                { provide: MaskPipe, useClass: MaskPipe },
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
                BrowserDetectionService,
                RevealGroupTrackingService,
                OperationStatusService,
                ApiService,
                LoggerService,
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
            applicationService = TestBed.inject<ApplicationService>(ApplicationService);
            applicationService.setApplicationConfiguration(
                'https://localhost:44366',
                'test-tenant',
                'test-tenant',
                'test-organisation-alias',
                'test-organisation-alias',
                false,
                'test-productId',
                'test-product',
                'production',
                FormType.Quote,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);
        });
    });

    afterEach(() => {
        fixture.destroy();
    });

    it('should appear on the page when it first loads, if configured to do so', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    // Assert
                    let calculationWidgetDebugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget'));
                    expect(calculationWidgetDebugElement).not
                        .toBeNull("Could not find the calculation widget element.");
                    let asideDebugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget > #calculation'));
                    expect(asideDebugElement).not.toBeNull("Could not find the aside calculation element");
                    resolve();
                });
        });
    });

    it('should not appear on the page if it has not been configured to', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        delete response.form.workflowConfiguration.step1.sidebar;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    // Assert
                    let calculationWidgetDebugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget'));
                    expect(calculationWidgetDebugElement).toBeNull(
                        "Found the calculation widget element, but it shouldn't be rendered.");
                    resolve();
                });
        });
    });

    it('should not show the quote reference number when it has not been configured', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget h3.quote-reference'));
                    expect(debugElement).toBeNull(
                        "Found the calculation widget quote reference, however it should not be shown since "
                        + "it has not been configured to be shown.");
                    resolve();
                });
        });
    });

    it('should show the quote reference number when configured to do so', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.theme.includeQuoteReferenceInSidebar = true;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(CalculationState.PremiumEstimate, null);
                    applicationService.quoteReference = 'ABCDEF';
                    applicationService.applicationState = ApplicationStatus.Incomplete;
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement
                        .query(By.css('calculation-widget h3.quote-reference .quote-reference-value'));
                    expect(debugElement != null)
                        .toBeTruthy("Did not find the calculation widget quote reference value.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('ABCDEF');
                    resolve();
                });
        });
    });

    it('should show the default quote reference label when none is configured', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.theme.includeQuoteReferenceInSidebar = true;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(CalculationState.PremiumEstimate, null);
                    applicationService.quoteReference = 'ABCDEF';
                    applicationService.applicationState = ApplicationStatus.Incomplete;
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget h3.quote-reference .quote-reference-label'));
                    expect(debugElement != null)
                        .toBeTruthy("Did not find the calculation widget quote reference label.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('Quote ref: ');
                    resolve();
                });
        });
    });

    it('should show the quote reference label that is configured', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.theme.includeQuoteReferenceInSidebar = true;
        response.form.textElements[0] = {
            category: "Sidebar",
            name: "Quote Reference Label",
            text: "Your quote reference is:",
        };
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(CalculationState.PremiumEstimate, null);
                    applicationService.quoteReference = 'ABCDEF';
                    applicationService.applicationState = ApplicationStatus.Incomplete;
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget h3.quote-reference .quote-reference-label'));
                    expect(debugElement != null)
                        .toBeTruthy("Did not find the calculation widget quote reference label.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('Your quote reference is:');
                    resolve();
                });
        });
    });

    it('should evauluate expressions in the quote reference label', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.theme.includeQuoteReferenceInSidebar = true;
        response.form.textElements[0] = {
            category: "Sidebar",
            name: "Quote Reference Label",
            text: "Your quote ref is:",
        };
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(CalculationState.PremiumEstimate, null);
                    applicationService.quoteReference = 'ABCDEF';
                    applicationService.applicationState = ApplicationStatus.Incomplete;
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget h3.quote-reference .quote-reference-label'));
                    expect(debugElement != null)
                        .toBeTruthy("Did not find the calculation widget quote reference label.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('Your quote ref is:');
                    resolve();
                });
        });
    });

    it('should show the summary heading if it is configured', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.textElements[0] = {
            category: "Sidebar",
            name: "Summary Label",
            text: "The finer details:",
        };
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(CalculationState.PremiumEstimate, null);
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert
                    let debugElement: DebugElement =
                        fixture.debugElement.query(By.css('calculation-widget .summary h4'));
                    expect(debugElement != null).toBeTruthy("Did not find the calculation widget summary heading.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('The finer details:');
                    resolve();
                });
        });
    });

    it('should not show the summary heading if it is not configured', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(CalculationState.PremiumEstimate, null);
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert
                    let debugSummaryPropertiesElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .summary'));
                    expect(debugSummaryPropertiesElement != null).toBeTruthy(
                        'For this test there needs to be at least one summary property shown.');
                    let debugElement: DebugElement =
                        fixture.debugElement.query(By.css('calculation-widget .summary h4'));
                    expect(debugElement != null).toBeTruthy(
                        "The summary heading was found but should not have been rendered.");
                    resolve();
                });
        });
    });

    it('should evaluate expressions the summary heading', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.textElements[0] = {
            category: "Sidebar",
            name: "Summary Label",
            text: "The %{ substring('finer', 0, 3) }% details:",
        };
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(CalculationState.PremiumEstimate, null);
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert
                    let debugElement: DebugElement =
                        fixture.debugElement.query(By.css('calculation-widget .summary h4'));
                    expect(debugElement != null).toBeTruthy("Did not find the calculation widget summary heading.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('The fin details:');
                    resolve();
                });
        });
    });

    it('should show summary items in order', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: any = createQuoteResult(CalculationState.PremiumEstimate, null);
                    quoteStatus.ratingSummaryItems = [
                        {
                            summaryLabel: "SL1",
                            value: "SV1",
                            summaryPositionExpression: "5 - 4",
                        },
                        {
                            summaryLabel: "SL3",
                            value: "SV3",
                            summaryPositionExpression: "1 + 2",
                        },
                        {
                            summaryLabel: "SL2",
                            value: "SV2",
                            summaryPositionExpression: "4 - 2",
                        },
                    ];
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert                    
                    let debugElement: DebugElement =
                        fixture.debugElement.query(By.css('calculation-widget .summary ul'));
                    expect(debugElement != null).toBeTruthy("The summary ul should have been rendered");
                    let debugSummaryPropertyElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .summary ul > li:nth-child(2) > .value'));
                    expect(debugSummaryPropertyElement).not.toBeNull("The 2nd li should have been found");
                    let element: HTMLElement = debugSummaryPropertyElement.nativeElement;
                    expect(element.innerText.trim()).toBe('SV2');
                    resolve();
                });
        });
    });

    it('should evaluate expressions in the summary property label', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: any = createQuoteResult(CalculationState.PremiumEstimate, null);
                    quoteStatus.ratingSummaryItems = [
                        {
                            summaryLabel: "SL%{ 9 - 8 }%",
                            value: "SV1",
                        },
                    ];
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert                    
                    let debugElement: DebugElement =
                        fixture.debugElement.query(By.css('calculation-widget .summary ul'));
                    expect(debugElement != null).toBeTruthy("The summary ul should have been rendered");
                    let debugSummaryPropertyElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .summary ul > li:first-child > .key'));
                    expect(debugSummaryPropertyElement != null).toBeTruthy(
                        "The li for the summary property should have been found");
                    let element: HTMLElement = debugSummaryPropertyElement.nativeElement;
                    expect(element.innerText.trim()).toBe('SL1:');
                    resolve();
                });
        });
    });

    it('should not show summary property if it\'s label is an empty string', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: any = createQuoteResult(CalculationState.PremiumEstimate, null);
                    quoteStatus.ratingSummaryItems = [
                        {
                            summaryLabel: "%{ 10 > 11 ? 'SL1': '' }%",
                            value: "SV1",
                        },
                        {
                            summaryLabel: "%{ 11 > 10 ? 'SL2': '' }%",
                            value: "SV2",
                        },
                    ];
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert                    
                    let debugElement: DebugElement =
                        fixture.debugElement.query(By.css('calculation-widget .summary ul'));
                    expect(debugElement != null).toBeTruthy("The summary ul should have been rendered");
                    let debugSummaryPropertyElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .summary ul > li > .key'));
                    expect(debugSummaryPropertyElement != null).toBeTruthy(
                        "The li for the summary property should have been found");
                    let element: HTMLElement = debugSummaryPropertyElement.nativeElement;
                    expect(element.innerText.trim()).toBe('SL2:');
                    resolve();
                });
        });
    });

    it('should set the relevant header, price label and message for an approved quote', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    quoteResult.trigger = {
                        name: 'tools5k',
                        type: 'endorsment',
                        header: null,
                        title: null,
                        message: null,
                        displayPrice: true,
                        reviewerExplanation: '',
                    };
                    applicationService.quoteState = QuoteState.Approved;
                    quoteResult.quoteState = QuoteState.Approved;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    // Assert                    
                    let debugHeaderElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .header-text'));
                    expect(debugHeaderElement != null).toBeTruthy("The header text should have been rendered");
                    expect(debugHeaderElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.approvedHeader.text);

                    let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .price-label'));
                    expect(debugPriceLabelElement != null).toBeTruthy("The price label should have been rendered");
                    expect(debugPriceLabelElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.approvedLabel.text);

                    let debugMessageElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .message'));
                    expect(debugMessageElement != null).toBeTruthy("The message should have been rendered");
                    expect(debugMessageElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.approvedMessage.text);

                    resolve();
                });
        });
    });

    it('should set the relevant header, price label and message for a quote approved with no triggers (auto approved)',
        async () => {
            // Arrange
            fixture = TestBed.createComponent(TestHostComponent);
            sut = fixture.componentInstance;
            eventService = TestBed.inject<EventService>(EventService);
            calculationService = TestBed.inject<CalculationService>(CalculationService);
            applicationService = TestBed.inject<ApplicationService>(ApplicationService);

            // Act
            let response: any = _.cloneDeep(formConfig);
            response['status'] = 'success';
            let configProcessorService: ConfigProcessorService
                = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
            sut.ready = true;
            fixture.detectChanges();

            return new Promise((resolve: any, reject: any): void => {
                eventService.webFormLoadedSubject
                    .pipe(filter((loaded: boolean) => loaded == true))
                    .subscribe(async () => {
                        let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                        quoteResult.trigger = null;
                        applicationService.quoteState = QuoteState.Approved;
                        quoteResult.quoteState = QuoteState.Approved;
                        eventService.quoteResultSubject.next(quoteResult);
                        fixture.detectChanges();

                        // Assert                    
                        let debugHeaderElement: DebugElement = fixture.debugElement.query(
                            By.css('calculation-widget .header-text'));
                        expect(debugHeaderElement != null).toBeTruthy("The header text should have been rendered");
                        expect(debugHeaderElement.nativeElement.innerText)
                            .toBe(defaultTextElements.textElements.sidebar.autoApprovedHeader.text);

                        let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                            By.css('calculation-widget .price-label'));
                        expect(debugPriceLabelElement != null).toBeTruthy("The price label should have been rendered");
                        expect(debugPriceLabelElement.nativeElement.innerText)
                            .toBe(defaultTextElements.textElements.sidebar.autoApprovedLabel.text);

                        let debugMessageElement: DebugElement = fixture.debugElement.query(
                            By.css('calculation-widget .message'));
                        expect(debugMessageElement != null).toBeTruthy("The message should have been rendered");
                        expect(debugMessageElement.nativeElement.innerText)
                            .toBe(defaultTextElements.textElements.sidebar.autoApprovedMessage.text);

                        resolve();
                    });
            });
        });

    it('should set the relevant header, price label and message from the calculation state if the quote is nascent, '
        + 'there are no triggers and the payable amount is not 0 or negative.', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    quoteResult.trigger = null;
                    quoteResult.amountPayable = "123.45";
                    quoteResult.calculationState = CalculationState.PremiumEstimate;
                    applicationService.quoteState = QuoteState.Nascent;
                    quoteResult.quoteState = QuoteState.Nascent;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    // Assert                    
                    let debugHeaderElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .header-text'));
                    expect(debugHeaderElement != null).toBeTruthy("The header text should have been rendered");
                    expect(debugHeaderElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.premiumEstimateHeader.text);

                    let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .price-label'));
                    expect(debugPriceLabelElement != null).toBeTruthy("The price label should have been rendered");
                    expect(debugPriceLabelElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.premiumEstimateLabel.text);

                    let debugMessageElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .message'));
                    expect(debugMessageElement != null).toBeTruthy("The message should have been rendered");
                    expect(debugMessageElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.premiumEstimateMessage.text);

                    resolve();
                });
        });
    });

    it('should set the relevant header, price label and message, '
        + 'and always show the price if an adjustment quote is being reviewed.', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    quoteResult.trigger = null;
                    quoteResult.amountPayable = "-123.45";
                    quoteResult.calculationState = CalculationState.BindingQuote;
                    applicationService.quoteState = QuoteState.Review;
                    quoteResult.quoteState = QuoteState.Review;
                    applicationService.mode = ApplicationMode.Review;
                    applicationService.quoteType = QuoteType.Adjustment;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    // Assert                    
                    let debugHeaderElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .header-text'));
                    expect(debugHeaderElement != null).toBeTruthy("The header text should have been rendered");
                    expect(debugHeaderElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebarAdjustment.reviewHeader.text);

                    let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .price-label'));
                    expect(debugPriceLabelElement != null).toBeTruthy("The price label should have been rendered");
                    expect(debugPriceLabelElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebarAdjustment.refundLabel.text);

                    let debugMessageElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .message'));
                    expect(debugMessageElement != null).toBeTruthy("The message should have been rendered");
                    expect(debugMessageElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.reviewMessage.text);

                    resolve();
                });
        });
    });

    it('should set the relevant header, price label and message from the calculation state if the quote is incomplete, '
        + 'there are no triggers and the payable amount is not 0 or negative.', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    quoteResult.trigger = null;
                    quoteResult.amountPayable = "123.45";
                    quoteResult.calculationState = CalculationState.Incomplete;
                    applicationService.quoteState = QuoteState.Incomplete;
                    quoteResult.quoteState = QuoteState.Incomplete;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    // Assert                    
                    let debugHeaderElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .header-text'));
                    expect(debugHeaderElement != null).toBeTruthy("The header text should have been rendered");
                    expect(debugHeaderElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.incompleteHeader.text);

                    let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .price-label'));
                    expect(debugPriceLabelElement != null).toBeTruthy("The price label should have been rendered");
                    expect(debugPriceLabelElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.incompleteLabel.text);

                    let debugMessageElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .message'));
                    expect(debugMessageElement != null).toBeTruthy("The message should have been rendered");
                    expect(debugMessageElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebar.incompleteMessage.text);

                    resolve();
                });
        });
    });

    it('should set the price label to refundLabel from textElements if the quote is incomplete, '
        + 'there are no triggers and the payable amount is negative.', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    quoteResult.trigger = null;
                    quoteResult.amountPayable = "-123.45";
                    quoteResult.calculationState = CalculationState.PremiumComplete;
                    applicationService.quoteState = QuoteState.Incomplete;
                    quoteResult.quoteState = QuoteState.Incomplete;
                    applicationService.quoteType = QuoteType.Adjustment;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    // Assert                    
                    let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .price-label'));
                    expect(debugPriceLabelElement != null).toBeTruthy("The price label should have been rendered");
                    expect(debugPriceLabelElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebarAdjustment.refundLabel.text);

                    resolve();
                });
        });
    });

    it('should set the price label to noPaymentLabel from textElements if the quote is nascent, '
        + 'there are no triggers and the payable amount is exactly zero.', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    quoteResult.trigger = null;
                    quoteResult.amountPayable = "0";
                    quoteResult.calculationState = CalculationState.PremiumComplete;
                    applicationService.quoteState = QuoteState.Incomplete;
                    quoteResult.quoteState = QuoteState.Incomplete;
                    applicationService.quoteType = QuoteType.Adjustment;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    // Assert                    
                    let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .price-label'));
                    expect(debugPriceLabelElement != null).toBeTruthy("The price label should have been rendered");
                    expect(debugPriceLabelElement.nativeElement.innerText)
                        .toBe(defaultTextElements.textElements.sidebarAdjustment.noPaymentLabel.text);

                    resolve();
                });
        });
    });

    it('should render html within the messageText so that product developers have control laying out and styling it.',
        async () => {
            // Arrange
            fixture = TestBed.createComponent(TestHostComponent);
            sut = fixture.componentInstance;
            eventService = TestBed.inject<EventService>(EventService);
            calculationService = TestBed.inject<CalculationService>(CalculationService);
            applicationService = TestBed.inject<ApplicationService>(ApplicationService);

            // Act
            let response: any = _.cloneDeep(formConfig);
            response['status'] = 'success';
            let messageHtml: string = '<div><ul><li>this is a list item</li></ul></div>';
            response.form.textElements[0] = {
                category: "Sidebar Purchase",
                name: "Premium Estimate Message",
                text: messageHtml,
            };
            let configProcessorService: ConfigProcessorService
                = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
            sut.ready = true;
            fixture.detectChanges();

            return new Promise((resolve: any, reject: any): void => {
                eventService.webFormLoadedSubject
                    .pipe(filter((loaded: boolean) => loaded == true))
                    .subscribe(async () => {
                        let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                        quoteResult.trigger = null;
                        quoteResult.amountPayable = "123.45";
                        quoteResult.calculationState = CalculationState.PremiumEstimate;
                        applicationService.quoteState = QuoteState.Incomplete;
                        quoteResult.quoteState = QuoteState.Incomplete;
                        applicationService.quoteType = QuoteType.NewBusiness;
                        eventService.quoteResultSubject.next(quoteResult);
                        fixture.detectChanges();

                        // Assert                    
                        let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                            By.css('calculation-widget .message'));
                        expect(debugPriceLabelElement != null).toBeTruthy("The message should have been rendered");
                        expect(debugPriceLabelElement.nativeElement.innerHTML)
                            .toBe(messageHtml);

                        resolve();
                    });
            });
        });

    it('should not show the price if the trigger specifies it not to be shown', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(
                        CalculationState.PremiumComplete,
                        TriggerState.SoftReferral);
                    quoteStatus.trigger = <TriggerDisplayConfig>{
                        name: 'toolsCover',
                        type: 'review',
                        header: defaultTextElements.textElements.sidebar.reviewTriggeredHeader.text,
                        title: defaultTextElements.textElements.sidebar.reviewTriggeredLabel.text,
                        message: defaultTextElements.textElements.sidebar.reviewTriggeredMessageDefault.text,
                        displayPrice: false,
                    };
                    applicationService.applicationState = ApplicationStatus.Incomplete;
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert                    
                    let debugPriceElement: DebugElement =
                        fixture.debugElement.query(By.css('calculation-widget .price'));
                    expect(debugPriceElement == null)
                        .toBeTruthy("The price should not have been rendered, but it was.");
                    resolve();
                });
        });
    });

    it('should show the price if the trigger specifies it be shown', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(
                        CalculationState.PremiumEstimate,
                        TriggerState.SoftReferral);
                    quoteStatus.trigger = <TriggerDisplayConfig>{
                        name: 'toolsCover',
                        type: 'review',
                        header: defaultTextElements.textElements.sidebar.reviewTriggeredHeader.text,
                        title: defaultTextElements.textElements.sidebar.reviewTriggeredLabel.text,
                        message: defaultTextElements.textElements.sidebar.reviewTriggeredMessageDefault.text,
                        displayPrice: true,
                    };
                    applicationService.applicationState = ApplicationStatus.Incomplete;
                    let priceWidgetInstance: PriceWidget
                        = fixture.debugElement.query(By.directive(PriceWidget))?.componentInstance;
                    await priceWidgetInstance.onCalculationResult(quoteStatus).then(() => {
                        fixture.detectChanges();

                        // Assert
                        let debugPriceElement: DebugElement =
                            fixture.debugElement.query(By.css('calculation-widget .price'));
                        expect(debugPriceElement != null).toBeTruthy("The price should have been rendered");
                        expect(debugPriceElement.nativeElement).not.toBeNull();
                        resolve();
                    });
                });
        });
    });

    it('should show the trigger messages when a review trigger is active', () => {

        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: any = createQuoteResult(
                        CalculationState.PremiumComplete,
                        TriggerState.Review);
                    quoteResult.trigger = <TriggerDisplayConfig>{
                        name: 'toolsCover',
                        type: 'review',
                        header: 'The custom trigger header',
                        title: 'The custom trigger title',
                        message: 'The custom trigger message',
                        displayPrice: false,
                    };
                    quoteResult.triggers = [quoteResult.trigger];
                    applicationService.applicationState = ApplicationStatus.Review;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    // Assert                    
                    let debugHeaderElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .header-text'));
                    expect(debugHeaderElement != null).toBeTruthy("The header text should have been rendered");
                    expect(debugHeaderElement.nativeElement.innerText)
                        .toBe(quoteResult.trigger.header);

                    let debugPriceLabelElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .price-label'));
                    expect(debugPriceLabelElement != null).toBeTruthy("The price label should have been rendered");
                    expect(debugPriceLabelElement.nativeElement.innerText)
                        .toBe(quoteResult.trigger.title);

                    let debugMessageElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .message'));
                    expect(debugMessageElement != null).toBeTruthy("The message should have been rendered");
                    expect(debugMessageElement.nativeElement.innerText)
                        .toBe(quoteResult.trigger.message);

                    resolve();
                });
        });
    });

    it('should not show the price if the calculation result has state "incomplete"', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteStatus: QuoteResult = createQuoteResult(CalculationState.Incomplete, null);
                    applicationService.applicationState = ApplicationStatus.Incomplete;
                    eventService.quoteResultSubject.next(quoteStatus);
                    fixture.detectChanges();

                    // Assert                    
                    let debugPriceElement: DebugElement =
                        fixture.debugElement.query(By.css('calculation-widget .price'));
                    expect(debugPriceElement == null)
                        .toBeTruthy("The price should not have been rendered, but it was.");
                    resolve();
                });
        });
    });

    it('should show the price if the quote has a trigger which says not to display the price, '
        + 'but the quote was approved.', async (done: any) => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(
                        CalculationState.BindingQuote,
                        TriggerState.Endorsement);
                    quoteResult.trigger = {
                        name: 'otherActivity',
                        type: 'endorsment',
                        header: 'Test header',
                        title: 'Quote for referral',
                        message: 'Test message for endorsment referral',
                        displayPrice: false,
                        reviewerExplanation: 'Test description',
                    };
                    quoteResult.quoteState = QuoteState.Approved;
                    applicationService.applicationState = ApplicationStatus.Approved;

                    let priceWidgetInstance: PriceWidget
                        = fixture.debugElement.query(By.directive(PriceWidget))?.componentInstance;

                    await priceWidgetInstance.onCalculationResult(quoteResult).then(() => {
                        fixture.detectChanges();

                        // Assert
                        const debugPriceElement: DebugElement =
                            fixture.debugElement.query(By.css('calculation-widget .price'));
                        expect(debugPriceElement != null)
                            .toBeTruthy("The price should have been rendered, but it was not.");
                        resolve();
                    });

                    done();
                });
        }).catch((error: any) => {
            // eslint-disable-next-line no-undef
            fail(error);
        });
    }, 10000);

    it('when includeQuoteReferenceInSidebar is changed to false, the quote reference disappears '
        + 'from the sidebar', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.theme.includeQuoteReferenceInSidebar = true;
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.includeQuoteReferenceInSidebar = false;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    applicationService.applicationState = ApplicationStatus.Approved;
                    applicationService.quoteState = QuoteState.Approved;
                    quoteResult.quoteState = QuoteState.Approved;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('aside .quote-reference'));
                    expect(debugElement != null).toBeTruthy('The quote reference should be rendered initially');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('aside .quote-reference'));
                    expect(debugElement == null).toBeTruthy('The quote reference should stop being rendered');
                    resolve();
                });
        });
    });

    it('when includeQuoteReferenceInSidebar is changed to true, the quote reference appears '
        + 'in the sidebar', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.theme.includeQuoteReferenceInSidebar = false;
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.includeQuoteReferenceInSidebar = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    applicationService.applicationState = ApplicationStatus.Approved;
                    applicationService.quoteState = QuoteState.Approved;
                    quoteResult.quoteState = QuoteState.Approved;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('aside .quote-reference'));
                    expect(debugElement == null).toBeTruthy('The quote reference should not be rendered initially');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('aside .quote-reference'));
                    expect(debugElement != null).toBeTruthy('The quote reference should start being rendered');
                    resolve();
                });
        });
    });

    it('When ShowPaymentOptionsInSidebar is set to false, the payment options are no longer shown '
        + 'in the sidebar', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.theme.showPaymentOptionsInSidebar = true;
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.showPaymentOptionsInSidebar = false;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    applicationService.applicationState = ApplicationStatus.Approved;
                    applicationService.quoteState = QuoteState.Approved;
                    quoteResult.quoteState = QuoteState.Approved;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .body questions-widget'));
                    let paymentStyle: CSSStyleDeclaration = getComputedStyle(debugElement.nativeElement);
                    expect(paymentStyle.display).toBe('block');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('calculation-widget .body questions-widget'));
                    paymentStyle = getComputedStyle(debugElement.nativeElement);
                    expect(paymentStyle.display).toBe('none');
                    resolve();
                });
        });
    });

    it('When ShowPaymentOptionsInSidebar is set to true, the payment options are now shown '
        + 'in the sidebar', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.theme.showPaymentOptionsInSidebar = false;
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.showPaymentOptionsInSidebar = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let quoteResult: QuoteResult = createQuoteResult(CalculationState.PremiumComplete, null);
                    applicationService.applicationState = ApplicationStatus.Approved;
                    applicationService.quoteState = QuoteState.Approved;
                    quoteResult.quoteState = QuoteState.Approved;
                    eventService.quoteResultSubject.next(quoteResult);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('calculation-widget .body questions-widget'));
                    let paymentStyle: CSSStyleDeclaration = getComputedStyle(debugElement.nativeElement);
                    expect(paymentStyle.display).toBe('none');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('calculation-widget .body questions-widget'));
                    paymentStyle = getComputedStyle(debugElement.nativeElement);
                    expect(paymentStyle.display).toBe('block');
                    resolve();
                });
        });
    });

});
