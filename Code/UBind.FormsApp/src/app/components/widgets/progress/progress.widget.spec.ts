import { ConfigurationOperation } from '../../../operations/configuration.operation';
import { ApplicationService } from '../../../services/application.service';
import { BroadcastService } from '../../../services/broadcast.service';
import { ConfigProcessorService } from '../../../services/config-processor.service';
import { ConfigService } from '../../../services/config.service';
import { FormService } from '../../../services/form.service';
import { CalculationService } from '../../../services/calculation.service';
import { UserService } from '../../../services/user.service';
import { AlertService } from '../../../services/alert.service';
import { MessageService } from '../../../services/message.service';
import { WorkflowService } from '@app/services/workflow.service';
import { WorkflowNavigation } from '@app/models/workflow-navigation';
import { ProgressWidget } from './progress.widget';
import { EventService } from '@app/services/event.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, ViewChild, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { sharedConfig } from '@app/app.module.shared';
import { EvaluateService } from '@app/services/evaluate.service';
import { AttachmentService } from '@app/services/attachment.service';
import { ValidationService } from '@app/services/validation.service';
import { ExpressionMethodService } from '@app/expressions/expression-method.service';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { CssProcessorService } from '@app/services/css-processor.service';
import { OperationFactory } from '@app/operations/operation.factory';
import { WebhookService } from '@app/services/webhook.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { PolicyOperation } from '@app/operations/policy.operation';
import { AbnPipe } from '@app/pipes/abn.pipe';
import { BsbPipe } from '@app/pipes/bsb.pipe';
import { CreditCardNumberPipe } from '@app/pipes/credit-card-number.pipe';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { TimePipe } from '@app/pipes/time.pipe';
import { PhoneNumberPipe } from '@app/pipes/phone-number.pipe';
import { NumberPlatePipe } from '@app/pipes/number-plate.pipe';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NgSelectModule } from '@ng-select/ng-select';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import * as _ from 'lodash-es';
import formConfig from './progress-widget.test-form-config.json';
import { By } from '@angular/platform-browser';
import { ExpressionInputSubjectService } from '@app/expressions/expression-input-subject.service';
import { filter } from 'rxjs/operators';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { WebFormComponent } from '@app/components/web-form/web-form';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { AppEventService } from '@app/services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationStatusService } from '@app/services/operation-status.service';
import { LoggerService } from '@app/services/logger.service';
import { ApiService } from '@app/services/api.service';

/* global spyOn */

declare const viewport: any;

describe('ProgressWidget', () => {
    let sut: ProgressWidget;
    let fixture: ComponentFixture<ProgressWidget>;
    let eventService: EventService;
    let expressionInputSubjectService: ExpressionInputSubjectService;
    let applicationService: ApplicationService;
    let workflowService: WorkflowService;

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

    let operationStub: any = {
        execute: (): Observable<any> => {
            return new BehaviorSubject<object>({});
        },
    };

    let windowScrollServiceStub: any = {
        scrollElementIntoView: (): void => {},
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
                { provide: ConfigurationOperation, useValue: {} },
                WorkflowService,
                { provide: CalculationOperation, useValue: {} },
                { provide: AlertService, useValue: alertServiceStub },
                { provide: WindowScrollService, useValue: windowScrollServiceStub },
                { provide: BroadcastService, useValue: broadcastServiceStub },
                { provide: CssProcessorService, useValue: {} },
                { provide: OperationFactory, useValue: operationFactoryStub },
                { provide: WebhookService, useValue: webhookServiceStub },
                { provide: AttachmentOperation, useValue: attachmentOperationStub },
                { provide: PolicyOperation, useValue: policyOperationStub },
                { provide: OperationFactory, useClass: FakeOperationFactory },
                { provide: ToolTipService, useClass: FakeToolTipService },
                FormService,
                ExpressionMethodService,
                ValidationService,
                ApplicationService,
                AttachmentService,
                ConfigService,
                EvaluateService,
                ConfigProcessorService,
                MessageService,
                EventService,
                CalculationService,
                UserService,
                ResumeApplicationService,
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
                { provide: EncryptionService, useValue: encryptionServiceStub },
                BrowserDetectionService,
                AppEventService,
                { provide: WorkflowStepOperation, useValue: operationStub },
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
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            workflowService.currentNavigation = new WorkflowNavigation(null, { stepName: 'step1' });
            eventService = TestBed.inject<EventService>(EventService);
            eventService.sectionWidgetCompletedViewInitSubject = new BehaviorSubject<void>(null);
        });
    });

    afterEach(() => {
        viewport.reset();
    });

    it('steps which do not define a tab index should not show up in the progress widget', () => {
        // Arrange
        fixture = TestBed.createComponent(ProgressWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBetweenMs = 0;

        // Act
        let response: any = _.cloneDeep(formConfig);
        delete response.form.workflowConfiguration.step2.tabIndexExpression;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        eventService.webFormLoadedSubject.next(true);
        sut.ngOnInit();

        // Assert        
        expect(sut.progressSteps.filter((step: any) => step.render == true).length).toBe(5);
    });

    it('steps which define a tab index expression that evaluates '
        + 'to null or empty should not show up in the progress widget', () => {
        // Arrange
        fixture = TestBed.createComponent(ProgressWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBetweenMs = 0;

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.workflowConfiguration.step2.tabIndexExpression = "";
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        eventService.webFormLoadedSubject.next(true);
        sut.ngOnInit();

        // Assert        
        expect(sut.progressSteps.filter((step: any) => step.render == true).length).toBe(5);
    });

    it('steps which do not define a tab label object should not show up in the progress widget', () => {
        // Arrange
        fixture = TestBed.createComponent(ProgressWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBetweenMs = 0;

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.textElements.splice(1, 1);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        eventService.webFormLoadedSubject.next(true);
        sut.ngOnInit();

        // Assert        
        expect(sut.progressSteps.filter((step: any) => step.render == true).length).toBe(5);
    });

    it('steps which do not define a tab label text property should not show up in the progress widget', () => {
        // Arrange
        fixture = TestBed.createComponent(ProgressWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBetweenMs = 0;

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.textElements.splice(1, 1);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        eventService.webFormLoadedSubject.next(true);
        sut.ngOnInit();

        // Assert        
        expect(sut.progressSteps.filter((step: any) => step.render == true).length).toBe(5);
    });

    it('steps which define a tab label which evaluates to null or empty should not show up in the progress widget',
        () => {
            // Arrange
            fixture = TestBed.createComponent(ProgressWidget);
            sut = fixture.componentInstance;
            sut.transitionDelayBetweenMs = 0;

            // Act
            let response: any = _.cloneDeep(formConfig);
            response.form.textElements[1].text = "%{ '' }%";
            response['status'] = 'success';
            let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
            eventService.webFormLoadedSubject.next(true);
            sut.ngOnInit();

            // Assert
            expect(sut.progressSteps.filter((step: any) => step.render == true).length).toBe(5);
        });

    it('steps are initially ordered by their tab index', () => {
        // Arrange
        fixture = TestBed.createComponent(ProgressWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBetweenMs = 0;

        // Act
        let response: any = _.cloneDeep(formConfig);
        response.form.workflowConfiguration.step1.tabIndex = 6;
        response.form.workflowConfiguration.step6.tabIndex = 1;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        eventService.webFormLoadedSubject.next(true);
        sut.ngOnInit();

        // Assert
        expect(sut.progressSteps[0].title).toBe("Step 6 tab label");
        expect(sut.progressSteps[5].title).toBe("Step 1 tab label");
    });

    it('steps are re-ordered by their tab index when a tabIndexExpression result changes', () => {
        // Arrange
        fixture = TestBed.createComponent(ProgressWidget);
        sut = fixture.componentInstance;
        sut.transitionDelayBetweenMs = 0;
        expressionInputSubjectService = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let response: any = _.cloneDeep(formConfig);
        delete response.form.workflowConfiguration.step1.tabIndex;
        response.form.workflowConfiguration.step1['tabIndexExpression'] = "test1";
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        eventService.webFormLoadedSubject.next(true);
        sut.ngOnInit();
        expect(sut.progressSteps[0].title).toBe("Step 1 tab label");
        expect(sut.progressSteps[5].title).toBe("Step 6 tab label");

        // Act
        expressionInputSubjectService.getFieldValueSubject('test1', 7);

        // Assert
        expect(sut.progressSteps[0].title).toBe("Step 2 tab label");
        expect(sut.progressSteps[5].title).toBe("Step 1 tab label");
    });

    it('renders the correct number of steps even when a step is not renderable', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);

        // Act
        delete response.form.workflowConfiguration.step2.tabIndexExpression;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        applicationService.currentWorkflowDestination = { stepName: 'step4' };
        viewport.set(1000);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();
                    setTimeout(() => {
                        let navDebugElement: any = hostFixture.debugElement.query(By.css('progress-widget > nav'));
                        expect(navDebugElement != null).toBeTruthy('could not find the nav element');

                        // Assert
                        let navElement: HTMLElement = navDebugElement.nativeElement;
                        expect(navElement.children.length).toBe(5);
                        resolve();
                    }, 60);
                });
        });
    });

    it('renders the correct classes on steps as the workflow step changes', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        workflowService.skipWorkflowAnimations = true;

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        viewport.set(1000);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();
                    let step1DebugElement: any = hostFixture.debugElement.query(
                        By.css('progress-widget > nav > div:first-child'));
                    expect(step1DebugElement).not.toBeNull();
                    let step1Element: HTMLElement = step1DebugElement.nativeElement;
                    expect(step1Element.innerText).toBe('Step 1 tab label');
                    expect(step1Element.classList).toContain('first');
                    expect(step1Element.classList).not.toContain('last');
                    expect(step1Element.classList).toContain('has-next');
                    expect(step1Element.classList).not.toContain('has-previous');
                    expect(step1Element.classList).toContain('active');
                    expect(step1Element.classList).not.toContain('inactive');
                    expect(step1Element.classList).not.toContain('next-active');
                    expect(step1Element.classList).not.toContain('previous-active');
                    expect(step1Element.classList).toContain('next-inactive');
                    expect(step1Element.classList).not.toContain('previous-inactive');
                    expect(step1Element.classList).not.toContain('future');
                    expect(step1Element.classList).not.toContain('past');

                    let step2DebugElement: any = hostFixture.debugElement.query(
                        By.css('progress-widget > nav > div:nth-child(2)'));
                    expect(step2DebugElement).not.toBeNull();
                    let step2Element: HTMLElement = step2DebugElement.nativeElement;
                    expect(step2Element.innerText).toBe('Step 2 tab label');
                    expect(step2Element.classList).not.toContain('first');
                    expect(step2Element.classList).not.toContain('last');
                    expect(step2Element.classList).toContain('has-next');
                    expect(step2Element.classList).toContain('has-previous');
                    expect(step2Element.classList).not.toContain('active');
                    expect(step2Element.classList).toContain('inactive');
                    expect(step2Element.classList).not.toContain('next-active');
                    expect(step2Element.classList).toContain('previous-active');
                    expect(step2Element.classList).toContain('next-inactive');
                    expect(step2Element.classList).not.toContain('previous-inactive');
                    expect(step2Element.classList).toContain('future');
                    expect(step2Element.classList).not.toContain('past');

                    // Act
                    workflowService.navigateTo({ stepName: 'step2' });
                    hostFixture.detectChanges();

                    // Assert
                    setTimeout(() => {
                        hostFixture.detectChanges();
                        expect(step1Element.classList).toContain('first');
                        expect(step1Element.classList).not.toContain('last');
                        expect(step1Element.classList).toContain('has-next');
                        expect(step1Element.classList).not.toContain('has-previous');
                        expect(step1Element.classList).not.toContain('active');
                        expect(step1Element.classList).toContain('inactive');
                        expect(step1Element.classList).toContain('next-active');
                        expect(step1Element.classList).not.toContain('previous-active');
                        expect(step1Element.classList).not.toContain('next-inactive');
                        expect(step1Element.classList).not.toContain('previous-inactive');
                        expect(step1Element.classList).not.toContain('future');
                        expect(step1Element.classList).toContain('past');

                        expect(step2Element.classList).not.toContain('first');
                        expect(step2Element.classList).not.toContain('last');
                        expect(step2Element.classList).toContain('has-next');
                        expect(step2Element.classList).toContain('has-previous');
                        expect(step2Element.classList).toContain('active');
                        expect(step2Element.classList).not.toContain('inactive');
                        expect(step2Element.classList).not.toContain('next-active');
                        expect(step2Element.classList).not.toContain('previous-active');
                        expect(step2Element.classList).toContain('next-inactive');
                        expect(step2Element.classList).toContain('previous-inactive');
                        expect(step2Element.classList).not.toContain('future');
                        expect(step2Element.classList).not.toContain('past');

                        resolve();
                    }, 50);
                });
        });
    });

    it('renders the truncated classes on steps beside truncated steps', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        workflowService.skipWorkflowAnimations = true;

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        viewport.set(500);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();
                    let step1DebugElement: any = hostFixture.debugElement.query(
                        By.css('progress-widget > nav > div:first-child'));
                    expect(step1DebugElement).not.toBeNull();
                    let step1Element: HTMLElement = step1DebugElement.nativeElement;
                    expect(step1Element.classList).not.toContain('previous-truncated');
                    expect(step1Element.classList).not.toContain('next-truncated');

                    let step2DebugElement: any = hostFixture.debugElement.query(
                        By.css('progress-widget > nav > div:nth-child(2)'));
                    expect(step2DebugElement).not.toBeNull();
                    let step2Element: HTMLElement = step2DebugElement.nativeElement;
                    expect(step2Element.classList).not.toContain('previous-truncated');
                    expect(step2Element.classList).not.toContain('next-truncated');

                    let step3DebugElement: any = hostFixture.debugElement.query(
                        By.css('progress-widget > nav > div:nth-child(3)'));
                    expect(step3DebugElement).not.toBeNull();
                    let step3Element: HTMLElement = step3DebugElement.nativeElement;
                    expect(step3Element.classList).not.toContain('previous-truncated');
                    expect(step3Element.classList).toContain('next-truncated');

                    // Act
                    workflowService.navigateTo({ stepName: 'step3' });
                    hostFixture.detectChanges();

                    // Assert
                    setTimeout(() => {
                        hostFixture.detectChanges();
                        expect(step2Element.classList).toContain('previous-truncated');
                        expect(step2Element.classList).not.toContain('next-truncated');

                        resolve();
                    }, 50);
                });
        });
    });

    it('renders the right steps truncated indicator when steps to the right are not shown', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        viewport.set(500);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();

                    // Assert
                    let debugElement: any = hostFixture.debugElement.query(
                        By.css('progress-widget .steps-truncated-right'));
                    expect(debugElement != null).toBeTruthy("could not find the .steps-truncated-right element");
                    resolve();
                });
        });
    });

    it('renders the left steps truncated indicator when steps to the left are not shown', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'step6' };
        viewport.set(500);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();

                    // Assert
                    let navDebugElement: any =
                        hostFixture.debugElement.query(By.css('progress-widget .steps-truncated-left'));
                    expect(navDebugElement).toBeTruthy();
                    let navElement: HTMLElement = navDebugElement.nativeElement;
                    expect(navElement).not.toBeNull();
                    resolve();
                });
        });
    });

    it('does not render the progress widget at all if the settings say not to use it', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response.form.theme.showProgressWidget = false;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(() => {
                    hostFixture.detectChanges();

                    // Assert
                    let navDebugElement: any = hostFixture.debugElement.query(By.css('progress-widget > nav'));
                    expect(navDebugElement == null).toBeTruthy();
                    resolve();
                });
        });
    });

    it('ensures each step is at least the size of minimumProgressStepWidthPixels specified in settings', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response.form.theme.minimumProgressStepWidthPixels = 500;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        viewport.set(1000);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();

                    // Assert
                    let debugElement: any = hostFixture.debugElement.query(
                        By.css('progress-widget > nav div:first-child'));
                    expect(debugElement).not.toBeNull();
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.clientWidth).toBeGreaterThanOrEqual(500);
                    resolve();
                });
        });
    });

    it('does not render the right steps truncated indicator when there are no renderable steps', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        delete response.form.workflowConfiguration.step1.tabIndex;
        response.form.workflowConfiguration.step2.tabIndexExpression = 'false';
        response.form.workflowConfiguration.step3.tabIndexExpression = 'false';
        delete response.form.workflowConfiguration.step4.tabIndex;
        delete response.form.workflowConfiguration.step5.tabIndex;
        delete response.form.workflowConfiguration.step6.tabIndex;
        response.form.workflowConfiguration.step1.startScreenExpression = 'false';
        response.form.workflowConfiguration.step2.startScreenExpression = 'false';
        response.form.workflowConfiguration.step3.startScreenExpression = 'false';
        response.form.workflowConfiguration.step4.startScreenExpression = 'true';
        response.form.workflowConfiguration.step5.startScreenExpression = 'false';
        response.form.workflowConfiguration.step6.startScreenExpression = 'false';
        response.form.workflowConfiguration['step7'] = {
            startScreenExpression: 'false',
        };
        response.form.workflowConfiguration['step8'] = {
            startScreenExpression: 'false',
        };
        response.form.workflowConfiguration['step9'] = {
            startScreenExpression: 'false',
        };
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'step4' };
        viewport.set(500);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();

                    // Assert
                    let navDebugElement: any =
                        hostFixture.debugElement.query(By.css('progress-widget .steps-truncated-right'));
                    expect(navDebugElement == null).toBeTruthy();
                    resolve();
                });
        });
    });

    it('does not render the left steps truncated indicator when there are no renderable steps', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        delete response.form.workflowConfiguration.step1.tabIndex;
        response.form.workflowConfiguration.step2.tabIndexExpression = 'false';
        response.form.workflowConfiguration.step3.tabIndexExpression = 'false';
        delete response.form.workflowConfiguration.step4.tabIndex;
        delete response.form.workflowConfiguration.step5.tabIndex;
        delete response.form.workflowConfiguration.step6.tabIndex;
        response.form.workflowConfiguration.step1.startScreenExpression = 'false';
        response.form.workflowConfiguration.step2.startScreenExpression = 'false';
        response.form.workflowConfiguration.step3.startScreenExpression = 'false';
        response.form.workflowConfiguration.step4.startScreenExpression = 'true';
        response.form.workflowConfiguration.step5.startScreenExpression = 'false';
        response.form.workflowConfiguration.step6.startScreenExpression = 'false';
        response.form.workflowConfiguration['step7'] = {
            startScreenExpression: 'false',
        };
        response.form.workflowConfiguration['step8'] = {
            startScreenExpression: 'false',
        };
        response.form.workflowConfiguration['step9'] = {
            startScreenExpression: 'false',
        };
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);

        // Act
        workflowService.currentNavigation = new WorkflowNavigation(null, { stepName: 'step4' });
        applicationService.currentWorkflowDestination = { stepName: 'step4' };
        viewport.set(500);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();

                    // Assert
                    let navDebugElement: any =
                        hostFixture.debugElement.query(By.css('progress-widget .steps-truncated-left'));
                    expect(navDebugElement == null).toBeTruthy();
                    resolve();
                });
        });
    });

    it('does not render the entire progress widget if the active step is not renderable', async () => {
        // Arrange
        fixture = TestBed.createComponent(ProgressWidget);
        sut = fixture.componentInstance;
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step3.tabIndexExpression = 'null';
        response.form.workflowConfiguration.step1.startScreenExpression = 'false';
        response.form.workflowConfiguration.step2.startScreenExpression = 'false';
        response.form.workflowConfiguration.step3.startScreenExpression = 'true';
        response.form.workflowConfiguration.step4.startScreenExpression = 'false';
        response.form.workflowConfiguration.step5.startScreenExpression = 'false';
        response.form.workflowConfiguration.step6.startScreenExpression = 'false';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        eventService.webFormLoadedSubject.next(true);
        fixture.detectChanges();
        // Act
        workflowService.navigateTo({ stepName: 'step3' });
        fixture.detectChanges();
        viewport.set(500);
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            setTimeout(() => {
                expect(sut.visibleState).toBe('hidden');
                resolve();
            }, 110);
        });
    });

    it('when MinimumProgressStepWidthPixels is changed, the new minimum width is applied', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response1: any = _.cloneDeep(formConfig);
        response1.form.theme.minimumProgressStepWidthPixels = 300;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.minimumProgressStepWidthPixels = 500;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        viewport.set(1000);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();

                    let debugElement: any = hostFixture.debugElement.query(
                        By.css('progress-widget > nav div:first-child'));
                    expect(debugElement).not.toBeNull();
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.clientWidth).toBeGreaterThanOrEqual(300);

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    hostFixture.detectChanges();

                    // Assert
                    debugElement = hostFixture.debugElement.query(
                        By.css('progress-widget > nav div:first-child'));
                    expect(debugElement).not.toBeNull();
                    element = debugElement.nativeElement;
                    expect(element.clientWidth).toBeGreaterThanOrEqual(500);

                    resolve();
                });
        });
    });

    it('when ShowProgressWidget is set to false, the progress widget disappears', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response1: any = _.cloneDeep(formConfig);
        response1.form.theme.showProgressWidget = true;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.showProgressWidget = false;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        viewport.set(1000);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();

                    let debugElement: any = hostFixture.debugElement.query(
                        By.css('#progress-bar'));
                    expect(debugElement != null).toBeTruthy('The progress widget should be rendered from the start');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    hostFixture.detectChanges();

                    // Assert
                    debugElement = hostFixture.debugElement.query(
                        By.css('#progress-bar'));
                    expect(debugElement == null).toBeTruthy('The progress widget should no longer be rendered');
                    resolve();
                });
        });
    });

    it('when ShowProgressWidget is set to true, the progress widget appears', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response1: any = _.cloneDeep(formConfig);
        response1.form.theme.showProgressWidget = false;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.showProgressWidget = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);

        // Act
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        viewport.set(1000);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();

                    let debugElement: any = hostFixture.debugElement.query(
                        By.css('#progress-bar'));
                    expect(debugElement == null).toBeTruthy('The progress widget should not be rendered initially');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    hostFixture.detectChanges();

                    // Assert
                    debugElement = hostFixture.debugElement.query(
                        By.css('#progress-bar'));
                    expect(debugElement != null).toBeTruthy('The progress widget should now be rendered');
                    resolve();
                });
        });
    });

});
