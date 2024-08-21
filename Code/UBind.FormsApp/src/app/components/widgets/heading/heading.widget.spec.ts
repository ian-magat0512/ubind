import { Component, DebugElement, ViewChild, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
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
import { filter } from 'rxjs/operators';
import { By } from '@angular/platform-browser';
import * as _ from 'lodash-es';
import formConfig from './heading-widget.test-form-config.json';
import { ToolTipService } from '@app/services/tooltip.service';
import { WebFormComponent } from '@app/components/web-form/web-form';
import { AppEventService } from '@app/services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
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
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */

describe('HeadingWidget', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;
    let workflowService: WorkflowService;

    let operationStub: any = {
        execute: (): Observable<any> => {
            return new BehaviorSubject<any>({ 'status': 'success' }).asObservable();
        },
    };

    let appEventServiceStub: any = {
        createEvent: (): void => {},
    };

    let alertServiceStub: any = {
        updateSubject: new Subject<Alert>(),
        visibleSubject: new Subject<boolean>(),
        alert: (alert: Alert): void => {},
    };

    let broadcastServiceStub: any = {
        on: (key: any): Subject<any> => new Subject<any>(),
    };

    let toolTipServiceStub: any = {
        toolTipChangedSubject: new Subject<any>(),
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
        template: `<web-form></web-form>`,
    })
    class TestHostComponent {
        @ViewChild(WebFormComponent)
        public webFormComponent: WebFormComponent;
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
                { provide: ToolTipService, useValue: toolTipServiceStub },
                UserService,
                ResumeApplicationService,
                { provide: OperationFactory, useClass: FakeOperationFactory },
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
                WorkflowService,
                { provide: AppEventService, useValue: appEventServiceStub },
                { provide: WorkflowStepOperation, useValue: operationStub },
                NotificationService,
                ConfigurationV2Processor,
                { provide: EncryptionService, useValue: encryptionServiceStub },
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
        });
    });

    afterEach(() => {
        fixture.destroy();
    });

    it('should appear on the page when it first loads', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.currentNavigation = new WorkflowNavigation(null, { stepName: 'step1' });

        // Act
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    // Assert
                    let headingWidgetDebugElement: DebugElement = fixture.debugElement.query(By.css('heading-widget'));
                    expect(headingWidgetDebugElement).toBeTruthy("Could not find the heading widget element.");
                    let h1DebugElement: DebugElement = fixture.debugElement.query(By.css('heading-widget > h1'));
                    expect(h1DebugElement).toBeTruthy("Could not find the heading widget h1 element.");
                    let h1Element: HTMLElement = h1DebugElement.nativeElement;
                    expect(h1Element.innerText).toBe('Step 1 Top Heading - The Quick');
                    resolve();
                });
        });
    });

    it('should change when the step changes', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.currentNavigation = new WorkflowNavigation(null, { stepName: 'step1' });

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    fixture.detectChanges();
                    let headingWidgetDebugElement: DebugElement = fixture.debugElement.query(By.css('heading-widget'));
                    expect(headingWidgetDebugElement).toBeTruthy("Could not find the heading widget element.");
                    let h1DebugElement: DebugElement = fixture.debugElement.query(By.css('heading-widget > h1'));
                    expect(h1DebugElement).toBeTruthy("Could not find the heading widget h1 element.");
                    let h1Element: HTMLElement = h1DebugElement.nativeElement;
                    expect(h1Element.innerText).toBe('Step 1 Top Heading - The Quick');

                    // navigate to step 2
                    workflowService.navigateTo({ stepName: 'step2' });
                    fixture.detectChanges();
                    sut.webFormComponent.transitioningOut = true;
                    sut.webFormComponent.onCompletedTransition();
                    sut.webFormComponent.transitioningIn = true;

                    fixture.detectChanges();
                    await fixture.whenRenderingDone();

                    // Assert
                    headingWidgetDebugElement = fixture.debugElement.query(By.css('heading-widget'));
                    expect(headingWidgetDebugElement).toBeTruthy("Could not find the heading widget element.");
                    h1DebugElement = fixture.debugElement.query(By.css('heading-widget > h1'));
                    expect(h1DebugElement).toBeTruthy("Could not find the heading widget h1 element.");
                    h1Element = h1DebugElement.nativeElement;
                    expect(h1Element.innerText).toBe('Step 2 Top Heading - Brown Fox');
                    resolve();
                });
        });
    });

    it('should evaluate embedded expressions within the text', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.currentNavigation = new WorkflowNavigation(null, { stepName: 'step1' });

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.textElements[0].text
            = "I called your mum %{ 5 + 5 }% times";
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    fixture.detectChanges();

                    // Assert
                    let headingWidgetDebugElement: DebugElement = fixture.debugElement.query(By.css('heading-widget'));
                    expect(headingWidgetDebugElement).toBeTruthy("Could not find the heading widget element.");
                    let h1DebugElement: DebugElement = fixture.debugElement.query(By.css('heading-widget > h1'));
                    expect(h1DebugElement).toBeTruthy("Could not find the heading widget h1 element.");
                    let h1Element: HTMLElement = h1DebugElement.nativeElement;
                    expect(h1Element.innerText).toBe('I called your mum 10 times');
                    resolve();
                });
        });
    });

    it('when ShowTopHeading is set to false, the top heading disappears', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.currentNavigation = new WorkflowNavigation(null, { stepName: 'step1' });
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.theme.showTopHeading = true;
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.showTopHeading = false;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    fixture.detectChanges();

                    let h1DebugElement: DebugElement = fixture.debugElement.query(By.css('heading-widget > h1'));
                    expect(h1DebugElement != null).toBeTruthy("The heading should be displayed from the start");

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();
                    await fixture.whenRenderingDone();

                    // Assert
                    h1DebugElement = fixture.debugElement.query(By.css('heading-widget > h1'));
                    expect(h1DebugElement == null).toBeTruthy("The heading should no longer be displayed");
                    resolve();
                });
        });
    });

    it('when ShowTopHeading is set to true, the top heading appears', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.currentNavigation = new WorkflowNavigation(null, { stepName: 'step1' });
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.theme.showTopHeading = false;
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.showTopHeading = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    fixture.detectChanges();

                    let h1DebugElement: DebugElement = fixture.debugElement.query(By.css('heading-widget > h1'));
                    expect(h1DebugElement == null).toBeTruthy("The heading should not be displayed from the start");

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();
                    await fixture.whenRenderingDone();

                    // Assert
                    h1DebugElement = fixture.debugElement.query(By.css('heading-widget > h1'));
                    expect(h1DebugElement != null).toBeTruthy("The heading should now be displayed");
                    resolve();
                });
        });
    });

});
