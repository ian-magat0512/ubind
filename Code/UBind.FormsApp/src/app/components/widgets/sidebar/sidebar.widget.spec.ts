import { ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, DebugElement } from '@angular/core';
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
import { BehaviorSubject, Observable, Subject } from 'rxjs';
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
import * as _ from 'lodash-es';
import formConfig from './sidebar-widget.test-form-config.json';
import { Aside, SidebarWidget } from './sidebar.widget';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { EncryptionService } from '@app/services/encryption.service';
import { AppEventService } from '@app/services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { filter } from 'rxjs/operators';
import { By } from '@angular/platform-browser';
import { FormType } from '@app/models/form-type.enum';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationStatusService } from '@app/services/operation-status.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */
declare const viewport: any;

describe('SidebarWidget', () => {
    let sut: SidebarWidget;
    let hostComponent: TestHostComponent;
    let fixture: ComponentFixture<SidebarWidget>;
    let hostFixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;
    let applicationService: ApplicationService;
    let workflowService: WorkflowService;

    let alertServiceStub: any = {
        updateSubject: new Subject<Alert>(),
        visibleSubject: new Subject<boolean>(),
        alert: (alert: Alert): void => {},
    };

    let operationStub: any = {
        execute: (): Observable<any> => {
            return new BehaviorSubject<object>({});
        },
    };

    let appEventServiceStub: any = {
        createEvent: (): void => {},
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

    let windowScrollServiceStub: any = {
        scrollElementIntoView: (): void => {
            eventService = TestBed.inject<EventService>(EventService);
            eventService.scrollingFinishedSubject.next(true);
        },
    };

    let encryptionServiceStub: any = {
        loadPublicKey: (): void => {},
    };

    /**
     * Test host component class
     */
    @Component({
        selector: `host-component`,
        template: `<web-form *ngIf="ready"></web-form>`,
        // We need to use OnPush change detection here so that we don't get
        // ExpressionChangedAfterItHasBeenCheckedError during unit tests.
        changeDetection: ChangeDetectionStrategy.OnPush,
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
                ConfigProcessorService,
                MessageService,
                { provide: ConfigurationOperation, useValue: {} },
                EvaluateService,
                EventService,
                CalculationService,
                ConfigService,
                FormService,
                AttachmentService,
                { provide: CalculationOperation, useValue: {} },
                ApplicationService,
                { provide: AlertService, useValue: alertServiceStub },
                { provide: WindowScrollService, useValue: windowScrollServiceStub },
                { provide: BroadcastService, useValue: broadcastServiceStub },
                { provide: CssProcessorService, useValue: {} },
                ValidationService,
                ExpressionMethodService,
                { provide: OperationFactory, useValue: operationFactoryStub },
                { provide: WebhookService, useValue: webhookServiceStub },
                { provide: AttachmentOperation, useValue: attachmentOperationStub },
                { provide: PolicyOperation, useValue: policyOperationStub },
                UserService,
                ResumeApplicationService,
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
                { provide: EncryptionService, useValue: encryptionServiceStub },
                WorkflowService,
                { provide: AppEventService, useValue: appEventServiceStub },
                { provide: WorkflowStepOperation, useValue: operationStub },
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
                null,
                'test-organisation-alias',
                null,
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
        if (fixture) {
            fixture.destroy();
        }
        if (hostFixture) {
            hostFixture.destroy();
        }
        viewport.reset();
    });

    it('should not destroy and re-create the calculation widget when the order of sidebar elements changes',
        async () => {
            // Arrange
            fixture = TestBed.createComponent(SidebarWidget);
            sut = fixture.componentInstance;
            sut.transitionDelayBeforeMs = 0;
            sut.transitionDelayBetweenMs = 0;
            eventService = TestBed.inject<EventService>(EventService);
            applicationService = TestBed.inject<ApplicationService>(ApplicationService);
            let sideBarAfterAnimateOutSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateOut');
            let response: any = _.cloneDeep(formConfig);
            response.form.workflowConfiguration.step1 = {
                "startScreenExpression": "true",
                "tabIndex": 1,
                "sidebar": [
                    {
                        "type": "calculation",
                    },
                    {
                        "type": "aside",
                        "name": "testAside",
                    },
                ],
            };
            response.form.workflowConfiguration.step2 = {
                "tabIndexExpression": "2",
                "sidebar": [
                    {
                        "type": "aside",
                        "name": "testAside",
                    },
                    {
                        "type": "calculation",
                    },
                ],
            };
            response['status'] = 'success';
            let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            workflowService.initialise();
            fixture.detectChanges();
            let calculationWidgetAside: Aside = sut.asides.filter((aside: Aside) => {
                return aside.type == 'calculation';
            })[0];
            expect(sideBarAfterAnimateOutSpy).not.toHaveBeenCalled();

            return new Promise((resolve: any, reject: any): void => {
                // Act
                sideBarAfterAnimateOutSpy.calls.reset();
                workflowService.navigateTo({ stepName: 'step2' });
                fixture.detectChanges();
                setTimeout(async () => {
                    fixture.detectChanges();
                    // Theres a 400ms transition animation so wait for it
                    await fixture.whenRenderingDone();
                    fixture.detectChanges();
                    // Theres a 400ms transition animation so wait for it
                    await fixture.whenRenderingDone();
                    fixture.detectChanges();
                    await fixture.whenRenderingDone();
                    expect(sideBarAfterAnimateOutSpy).toHaveBeenCalled();
                    fixture.detectChanges();
                    // Assert
                    let calculationWidgetAside2: Aside = sut.asides.filter((aside: Aside) => {
                        return aside.type == 'calculation';
                    })[0];
                    expect(calculationWidgetAside == calculationWidgetAside2)
                        .toBeTruthy('The calculation widget aside should be the same instance.');
                    resolve();
                }, 0);
            });
        });

    it('when a sidebar with a calculation widget is added, the calculation widget appears', () => {
        // Arrange
        fixture = TestBed.createComponent(SidebarWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBeforeMs = 0;
        sut.transitionDelayBetweenMs = 0;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.sidebar = [];
        let response2: any = _.cloneDeep(formConfig);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.sidebar = [
            {
                type: "calculation",
            },
        ];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            let debugElement: DebugElement
                = fixture.debugElement.query(By.css('calculation-widget'));
            expect(debugElement == null)
                .toBeTruthy('The calculation widget should not be there yet.');

            // Act
            configProcessorService.onConfigurationResponse(response2);
            fixture.detectChanges();

            // Assert
            debugElement = fixture.debugElement.query(By.css('calculation-widget'));
            expect(debugElement != null)
                .toBeTruthy('The calculation widget should have been shown.');
            resolve();
        });
    });

    it('when the calculation widget is removed, the calculation widget disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(SidebarWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBeforeMs = 0;
        sut.transitionDelayBetweenMs = 0;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.sidebar = [
            {
                type: "calculation",
            },
        ];
        let response2: any = _.cloneDeep(formConfig);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.sidebar = [];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            let debugElement: DebugElement
                = fixture.debugElement.query(By.css('calculation-widget'));
            expect(debugElement != null)
                .toBeTruthy('The calculation widget should be there to start with.');

            // Act
            configProcessorService.onConfigurationResponse(response2);
            fixture.detectChanges();

            // Assert
            debugElement = fixture.debugElement.query(By.css('calculation-widget'));
            expect(debugElement == null)
                .toBeTruthy('The calculation widget should have been removed.');
            resolve();
        });
    });

    it('when an aside is added, the aside appears', () => {
        // Arrange
        fixture = TestBed.createComponent(SidebarWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBeforeMs = 0;
        sut.transitionDelayBetweenMs = 0;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.sidebar = [];
        let response2: any = _.cloneDeep(formConfig);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.sidebar = [
            {
                type: "aside",
                name: "testAside",
            },
        ];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            let debugElement: DebugElement
                = fixture.debugElement.query(By.css('#testAside'));
            expect(debugElement == null)
                .toBeTruthy('The aside should not be there yet.');

            // Act
            configProcessorService.onConfigurationResponse(response2);
            fixture.detectChanges();

            // Assert
            debugElement = fixture.debugElement.query(By.css('#testAside'));
            expect(debugElement != null)
                .toBeTruthy('The aside should have been shown.');
            resolve();
        });
    });

    it('when the aside is removed, the aside disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(SidebarWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBeforeMs = 0;
        sut.transitionDelayBetweenMs = 0;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.sidebar = [
            {
                type: "aside",
                name: "testAside",
            },
        ];
        let response2: any = _.cloneDeep(formConfig);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.sidebar = [];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            let debugElement: DebugElement
                = fixture.debugElement.query(By.css('#testAside'));
            expect(debugElement != null)
                .toBeTruthy('The aside should be there to start with.');

            // Act
            configProcessorService.onConfigurationResponse(response2);
            fixture.detectChanges();

            // Assert
            debugElement = fixture.debugElement.query(By.css('#testAside'));
            expect(debugElement == null)
                .toBeTruthy('The aside should have been removed.');
            resolve();
        });
    });

    it('when one of the textElements for an aside is changed, the aside content updates', () => {
        // Arrange
        fixture = TestBed.createComponent(SidebarWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBeforeMs = 0;
        sut.transitionDelayBetweenMs = 0;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.sidebar = [
            {
                type: "aside",
                name: "testAside",
            },
        ];
        response1.form.textElements.push({
            category: "Sidebar Panels",
            name: "Test Aside Header",
            text: "This is the original test aside header text",
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.textElements.pop();
        response2.form.textElements.push({
            category: "Sidebar Panels",
            name: "Test Aside Header",
            text: "This is the updated test aside header text",
        });
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            let debugElement: DebugElement
                = fixture.debugElement.query(By.css('#testAside .header .table .cell'));
            let el: HTMLElement = debugElement.nativeElement;
            expect(el.innerText)
                .toBe('This is the original test aside header text');

            // Act
            configProcessorService.onConfigurationResponse(response2);
            fixture.detectChanges();

            // Assert
            debugElement = fixture.debugElement.query(By.css('#testAside .header .table .cell'));
            el = debugElement.nativeElement;
            expect(el.innerText)
                .toBe('This is the updated test aside header text');
            resolve();
        });
    });

    it('when SidebarWidthPixels is changed, the new width is applied to the sidebar', () => {
        // Arrange
        hostFixture = TestBed.createComponent(TestHostComponent);
        hostComponent = hostFixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.sidebarWidthPixels = 500;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        viewport.set(1000);
        hostComponent.ready = true;
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();
                    await hostFixture.whenStable();
                    let debugElement: DebugElement
                        = hostFixture.debugElement.query(By.css('#sidebar-column-ubind'));
                    let computedStyle: CSSStyleDeclaration = getComputedStyle(debugElement.nativeElement);
                    let widthPixels: number = Number.parseInt(computedStyle.width.replace(/px/, ''), 10);
                    expect(widthPixels).toBeGreaterThanOrEqual(180);
                    expect(widthPixels).toBeLessThanOrEqual(300);

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    hostFixture.detectChanges();

                    // Assert
                    debugElement = hostFixture.debugElement.query(By.css('#sidebar-column-ubind'));
                    computedStyle = getComputedStyle(debugElement.nativeElement);
                    widthPixels = Number.parseInt(computedStyle.width.replace(/px/, ''), 10);
                    expect(widthPixels).toBeGreaterThanOrEqual(500);
                    expect(widthPixels).toBeLessThanOrEqual(620);
                    resolve();
                });
        });
    });

});
