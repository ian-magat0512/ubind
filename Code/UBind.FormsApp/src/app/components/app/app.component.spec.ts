import { Component, CUSTOM_ELEMENTS_SCHEMA, DebugElement, ViewChild } from "@angular/core";
import { ComponentFixture, TestBed } from "@angular/core/testing";
import { NoopAnimationsModule } from "@angular/platform-browser/animations";
import { sharedConfig } from "@app/app.module.shared";
import { ExpressionDependencies } from "@app/expressions/expression-dependencies";
import { ConfigurationOperation } from "@app/operations/configuration.operation";
import { AlertService } from "@app/services/alert.service";
import { ApplicationStartupService } from "@app/services/application-startup.service";
import { ApplicationService } from "@app/services/application.service";
import { BroadcastService } from "@app/services/broadcast.service";
import { BrowserDetectionService } from "@app/services/browser-detection.service";
import { ConfigProcessorService } from "@app/services/config-processor.service";
import { ConfigService } from "@app/services/config.service";
import { CssProcessorService } from "@app/services/css-processor.service";
import { EventService } from "@app/services/event.service";
import { MessageService } from "@app/services/message.service";
import { NotificationService } from "@app/services/notification.service";
import { UserService } from "@app/services/user.service";
import { WorkflowService } from "@app/services/workflow.service";
import { NgSelectModule } from "@ng-select/ng-select";
import { AppComponent } from "./app.component";
import * as _ from 'lodash-es';
import formConfig from './app-components.test-form-config.json';
import { ExpressionMethodService } from "@app/expressions/expression-method.service";
import { AttachmentOperation } from "@app/operations/attachment.operation";
import { CalculationOperation } from "@app/operations/calculation.operation";
import { FakeOperationFactory } from "@app/operations/fakes/fake-operation-factory";
import { OperationFactory } from "@app/operations/operation.factory";
import { PolicyOperation } from "@app/operations/policy.operation";
import { AttachmentService } from "@app/services/attachment.service";
import { CalculationService } from "@app/services/calculation.service";
import { EncryptionService } from "@app/services/encryption.service";
import { EvaluateService } from "@app/services/evaluate.service";
import { FakeToolTipService } from "@app/services/fakes/fake-tooltip.service";
import { FormService } from "@app/services/form.service";
import { ResumeApplicationService } from "@app/services/resume-application.service";
import { ToolTipService } from "@app/services/tooltip.service";
import { ValidationService } from "@app/services/validation.service";
import { WebhookService } from "@app/services/webhook.service";
import { ScrollArguments, WindowScrollService } from "@app/services/window-scroll.service";
import { Alert } from "@app/models/alert";
import { CurrencyPipe } from "@angular/common";
import { AbnPipe } from "@app/pipes/abn.pipe";
import { BsbPipe } from "@app/pipes/bsb.pipe";
import { CreditCardNumberPipe } from "@app/pipes/credit-card-number.pipe";
import { CssIdentifierPipe } from "@app/pipes/css-identifier.pipe";
import { NumberPlatePipe } from "@app/pipes/number-plate.pipe";
import { PhoneNumberPipe } from "@app/pipes/phone-number.pipe";
import { TimePipe } from "@app/pipes/time.pipe";
import { ConfigurationV2Processor } from "@app/services/configuration-v2-processor";
import { OperationInstructionService } from "@app/services/operation-instruction.service";
import { OperationStatusService } from "@app/services/operation-status.service";
import { RevealGroupTrackingService } from "@app/services/reveal-group-tracking.service";
import { WorkflowStatusService } from "@app/services/workflow-status.service";
import { filter } from "rxjs/operators";
import { ApiService } from "@app/services/api.service";
import { ClaimApiService } from "@app/services/api/claim-api-service";
import { AppContextApiService } from "@app/services/api/app-context-api.service";
import { QuoteApiService } from "@app/services/api/quote-api.service";
import { ApplicationLoadService } from "@app/services/application-load-service";
import { ContextEntityService } from "@app/services/context-entity.service";
import { UnifiedFormModelService } from "@app/services/unified-form-model.service";
import { By } from "@angular/platform-browser";
import { WebFormComponent } from "../web-form/web-form";
import { AppEventService } from "@app/services/app-event.service";
import { WorkflowStepOperation } from "@app/operations/workflow-step.operation";
import { Subject, Observable, BehaviorSubject, of } from "rxjs";
import { FakeContextEntityService } from "@app/services/fakes/fake-context-entity.service";
import { OptionSetChangePublisher } from "@app/services/option-set-change-publisher";
import { DeploymentEnvironment } from "@app/models/deployment-environment";
import { QuoteState } from "@app/models/quote-state.enum";
import { CalculationState, TriggerState } from "@app/models/calculation-result-state";
import { QuoteResult } from "@app/models/quote-result";
import { SourceRatingSummaryItem } from "@app/models/source-rating-summary-item";
import { UrlService } from "@app/services/url-service";
import { FormsAppContextModel } from "@app/models/forms-app-context.model";

/* global spyOn */

// disabled due to intermittent failures. To be fixed in UB-10955
xdescribe('Appcomponent', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let alertServiceStub: any;
    let broadcastServiceStub: any;
    let operationFactoryStub: any;
    let webhookServiceStub: any;
    let attachmentOperationStub: any;
    let policyOperationStub: any;
    let encryptionServiceStub: any;
    let eventService: EventService;
    let applicationStartupService: ApplicationStartupService;
    let appContextApiServiceStub: any;
    let userServiceStub: any;
    let appEventServiceStub: any;
    let operationStub: any;
    let workflowService: WorkflowService;
    let optionSetChangePublisherStub: any;
    let quoteApiServiceStub: any;
    let claimApiServiceStub: any;
    let applicationService: any;
    let windowScrollServiceStub: any;
    let urlParams: URLSearchParams;
    let appContextServiceStub: any;

    const createQuoteResult =  (
        calculationState: CalculationState,
        triggerState: TriggerState,
    ): QuoteResult => {
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
    };

    /**
     * Test host component class
     */
    @Component({
        selector: `host-component`,
        template: `
        <app *ngIf="isFormTypeQuote && ready"></app>
        <app *ngIf="!isFormTypeQuote && ready"></app>`,
    })
    class TestHostComponent {
        @ViewChild(AppComponent)
        public appComponent: AppComponent;
        public ready: boolean = false;
        public isFormTypeQuote: boolean = true;
    }

    beforeEach(async () => {
        alertServiceStub = {
            updateSubject: new Subject<Alert>(),
            visibleSubject: new Subject<boolean>(),
            alert: (alert: Alert): void => {},
        };
        broadcastServiceStub = {
            on: (key: any): Subject<any> => new Subject<any>(),
        };
        operationFactoryStub = {
            getStatus: (operation: any): string => 'success',
        };
        webhookServiceStub = {
            getActiveWebhookCount: (): number => 0,
            inProgressSubject: new Subject<boolean>(),
            webhookFieldInProgressSubject: new Subject<boolean>(),
        };
        attachmentOperationStub = {
            operationInProgress: false,
            inProgressSubject: new Subject<boolean>(),
        };
        policyOperationStub = {
            getQuoteType: (): string => 'newBusiness',
        };
        encryptionServiceStub = {
            loadPublicKey: (): void => { },
        };
        userServiceStub = {
            retrieveLoggedInUserData: (): void => { },
        };
        appEventServiceStub = {
            createEvent: (): void => { },
        };
        operationStub = {
            execute: (): Observable<any> => {
                return new BehaviorSubject<object>({});
            },
        };
        optionSetChangePublisherStub = {
            onConfigurationLoaded: (): void => { },
            onConfigurationUpdated: (): void => { },
        };

        appContextApiServiceStub = {
            getFormsAppContext: (
                tenant: string,
                product: string,
                organisation?: string,
                portal?: string,
                quoteId?: string,
            ): Observable<FormsAppContextModel> => {
                return new Observable((observer: any): void => {
                    observer.next({});
                    observer.complete();
                });
            },
        };
        quoteApiServiceStub = {
            createNewBusinessQuote: (
                tenantId: string,
                organisationAlias: string,
                portalId: string,
                productId: string,
                environment: DeploymentEnvironment,
                isTestData: boolean,
            ): any => { },
            getQuoteState: (quoteId: string): any => {
                return QuoteState.Nascent.toString();
            },
        };
        claimApiServiceStub = {
            createNewClaim: (
                tenantId: string,
                organisationAlias: string,
                productId: string,
                environment: string,
                isTestData: boolean): any => { },
            getQuoteState: (quoteId: string): any => {
                return QuoteState.Nascent.toString();
            },
        };

        windowScrollServiceStub = {
            scrollElementIntoView: (elementRef: HTMLElement, args?: ScrollArguments): void => { },
        };

        appContextServiceStub = {
            getFormsAppContext: (
                tenant: string,
                organisation: string,
                portal: string,
                quoteId: string): any => of({}),
        };

        await TestBed.configureTestingModule({
            declarations: [
                TestHostComponent,
                AppComponent,
                WebFormComponent,
                ...sharedConfig.declarations,
            ],
            providers: [
                // { provide: ElementRef, useValue: mockElementRef },
                { provide: EncryptionService, useValue: encryptionServiceStub },
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                { provide: EvaluateService, useClass: EvaluateService },
                { provide: EventService, useClass: EventService },
                { provide: CalculationService, useClass: CalculationService },
                { provide: WorkflowService, useClass: WorkflowService },
                { provide: ConfigService, useClass: ConfigService },
                { provide: FormService, useClass: FormService },
                { provide: AttachmentService, useClass: AttachmentService },
                { provide: CalculationOperation, useValue: {} },
                { provide: ApplicationService, useClass: ApplicationService },
                { provide: AlertService, useValue: alertServiceStub },
                { provide: WindowScrollService, useValue: windowScrollServiceStub },
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
                { provide: ApplicationStartupService, useClass: ApplicationStartupService },
                { provide: UserService, useValue: userServiceStub },
                { provide: QuoteApiService, useValue: quoteApiServiceStub },
                { provide: ClaimApiService, useValue: claimApiServiceStub },
                { provide: OptionSetChangePublisher, useValue: optionSetChangePublisherStub },
                { provide: ContextEntityService, useClass: FakeContextEntityService },
                { provide: AppContextApiService, useValue: appContextApiServiceStub },
                { provide: AppEventService, useValue: appEventServiceStub },
                { provide: WorkflowStepOperation, useValue: operationStub },
                { provide: AppContextApiService, useValue: appContextServiceStub },
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
                OperationInstructionService,
                ApiService,
                UnifiedFormModelService,
                ConfigurationOperation,
                ApplicationLoadService,
                UrlService,
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
            urlParams = new URLSearchParams();
            urlParams.set('tenant', 'test-tenant');
            urlParams.set('organisation', 'test-organisation');
            urlParams.set('product', 'test-product');
            urlParams.set('environment', 'development');
            urlParams.set('formType', 'quote');
            let urlService: UrlService = TestBed.inject<UrlService>(UrlService);
            spyOn(urlService, 'getQueryStringParams').and.returnValue(urlParams);

            let messageService: MessageService = TestBed.inject<MessageService>(MessageService);
            spyOn(messageService, 'sendMessage'); // make it do nothing.
            eventService = TestBed.inject<EventService>(EventService);
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            applicationService = TestBed.inject<ApplicationService>(ApplicationService);
            spyOn(<any>workflowService, "executeOperations");
            applicationStartupService = TestBed.inject<ApplicationStartupService>(ApplicationStartupService);
            spyOn(applicationStartupService, 'fetchAndProcessConfiguration').and.callFake(async () => {
                configProcessorService.onConfigurationResponse(response);
            });
            let response: any = _.cloneDeep(formConfig);
            response['status'] = 'success';
            let configProcessorService: ConfigProcessorService
                = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
        });
    });
    afterEach(() => {
        fixture.destroy();
    });

    // intermittently fails on build server. To be fixed in UB-10955
    xit('should have claim state classes on the app element', async () => {
        // Arrange
        urlParams.set('formType', 'claim');
        fixture = TestBed.createComponent(TestHostComponent);
        applicationStartupService.initialise(urlParams);
        sut = fixture.componentInstance;
        sut.isFormTypeQuote = false;
        fixture.detectChanges();
        sut.ready = true;
        fixture.detectChanges();
        await fixture.whenStable();
        workflowService.initialise();
        return new Promise((resolve: any, reject: any): void => {
            eventService.appLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(() => {
                    setTimeout(async () => {
                        fixture.detectChanges();
                        await fixture.whenStable();
                        setTimeout(() => {
                            // Act
                            const appComponent: DebugElement =
                                fixture.debugElement.query(By.directive(AppComponent));
                            const classes: string = Object.keys(appComponent.classes).join(' ');

                            // Assert
                            expect(classes.includes('claim-state-nascent')).toBeTrue();
                            expect(classes.includes('calculation-state-incomplete')).toBeTrue();
                            expect(classes.includes('workflow-step-step1')).toBeTrue();
                            expect(classes.includes('no-trigger')).toBeTrue();
                            resolve();
                        }, 50);
                    }, 50);
                });
        });
    }, 10000);

    // intermittently fails on build server. To be fixed in UB-10955
    xit('should have state classes on the app element', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        applicationStartupService.initialise(urlParams);
        sut = fixture.componentInstance;
        sut.ready = true;
        fixture.detectChanges();
        workflowService.initialise();
        return new Promise((resolve: any, reject: any): void => {
            eventService.appLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(() => {
                    setTimeout(async () => {
                        fixture.detectChanges();
                        await fixture.whenStable();
                        setTimeout(() => {
                            // Act
                            const appComponent: DebugElement =
                                fixture.debugElement.query(By.directive(AppComponent));
                            const classes: string = Object.keys(appComponent.classes).join(' ');

                            // Assert
                            expect(classes.includes('quote-state-nascent')).toBeTrue();
                            expect(classes.includes('calculation-state-incomplete')).toBeTrue();
                            expect(classes.includes('workflow-step-step1')).toBeTrue();
                            expect(classes.includes('no-trigger')).toBeTrue();
                            resolve();
                        }, 50);
                    }, 50);
                });
        });
    }, 10000);

    // intermittently fails on build server. To be fixed in UB-10955
    xit('should update workflow-step class on workflow navigation', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        applicationStartupService.initialise(urlParams);
        sut = fixture.componentInstance;
        sut.ready = true;
        fixture.detectChanges();
        workflowService.initialise();
        return new Promise((resolve: any, reject: any): void => {
            eventService.appLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(() => {
                    setTimeout(async () => {
                        fixture.detectChanges();
                        await fixture.whenStable();
                        workflowService.navigateTo({ stepName: 'step2' });
                        workflowService.completedNavigationOut();
                        workflowService.completedNavigationIn();
                        setTimeout(async () => {
                            expect(workflowService.currentDestination.stepName).toBe('step2');
                            fixture.detectChanges();
                            await fixture.whenStable();

                            // Act
                            const appComponent: DebugElement =
                                fixture.debugElement.query(By.directive(AppComponent));
                            const classes: string = Object.keys(appComponent.classes).join(' ');

                            // Assert
                            expect(classes.includes('workflow-step-step2')).toBeTrue();
                            resolve();
                        }, 50);
                    }, 50);
                });
        });
    }, 10000);

    // intermittently fails on build server. To be fixed in UB-10955
    xit('should update quote-state class on QuoteResult.', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        applicationStartupService.initialise(urlParams);
        sut = fixture.componentInstance;
        sut.ready = true;
        fixture.detectChanges();
        workflowService.initialise();
        return new Promise((resolve: any, reject: any): void => {
            eventService.appLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(() => {
                    setTimeout(async () => {
                        fixture.detectChanges();
                        await fixture.whenStable();
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
                        workflowService.navigateTo({ stepName: 'step2' });
                        workflowService.completedNavigationOut();
                        workflowService.completedNavigationIn();
                        fixture.detectChanges();
                        await fixture.whenStable();

                        setTimeout(() => {
                            // Act
                            const appComponent: DebugElement =
                                fixture.debugElement.query(By.directive(AppComponent));
                            const classes: string = Object.keys(appComponent.classes).join(' ');

                            // Assert
                            expect(classes.includes('quote-state-approved')).toBeTrue();
                            resolve();
                        }, 50);
                    }, 50);
                });
        });
    }, 10000);

    // intermittently fails on build server. To be fixed in UB-10955
    xit('should update calculation-state class on CalculationState changed.', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        applicationStartupService.initialise(urlParams);
        sut = fixture.componentInstance;
        sut.ready = true;
        fixture.detectChanges();
        workflowService.initialise();
        return new Promise((resolve: any, reject: any): void => {
            eventService.appLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(() => {
                    setTimeout(async () => {
                        fixture.detectChanges();
                        await fixture.whenStable();
                        applicationService.quoteState = QuoteState.Complete;
                        applicationService.latestQuoteResult = <any>{
                            quoteState: QuoteState.Complete,
                            calculationState: CalculationState.PremiumComplete,
                            ratingSummaryItems: new Array<SourceRatingSummaryItem>(),
                        };
                        workflowService.navigateTo({ stepName: 'step2' });
                        workflowService.completedNavigationOut();
                        workflowService.completedNavigationIn();
                        fixture.detectChanges();
                        await fixture.whenStable();
                        setTimeout(() => {
                            // Act
                            const appComponent: DebugElement =
                                fixture.debugElement.query(By.directive(AppComponent));
                            const classes: string = Object.keys(appComponent.classes).join(' ');

                            // Assert
                            expect(classes.includes('quote-state-complete')).toBeTrue();
                            expect(classes.includes('calculation-state-premiumComplete')).toBeTrue();
                            resolve();
                        }, 50);
                    }, 50);
                });
        });
    }, 10000);
});
