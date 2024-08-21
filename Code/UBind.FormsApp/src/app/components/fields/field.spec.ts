/* eslint-disable max-len */
import { EncryptionService } from '@app/services/encryption.service';
import { ChangeDetectionStrategy, Component, DebugElement, ViewChild, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
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
import { filter } from 'rxjs/operators';
import { By } from '@angular/platform-browser';
import * as _ from 'lodash-es';
import formConfig from './field.test-form-config.json';
import formConfig2 from './field.test-form-config2.json';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { AppEventService } from '@app/services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import { WebFormComponent } from '../web-form/web-form';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { SafeHtmlPipe } from '@app/pipes/safe-html.pipe';
import { WorkflowRole } from '@app/models/workflow-role.enum';
import { FieldType } from '@app/models/field-type.enum';
import { FieldDataType } from '@app/models/field-data-type.enum';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationStatusService } from '@app/services/operation-status.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */
/* global jasmine */

describe('Field', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;
    let calculationService: CalculationService;

    let operationStub: any = {
        execute: (): Observable<any> => {
            return new BehaviorSubject<object>({});
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
        @ViewChild(WebFormComponent)
        public webFormComponent: WebFormComponent;

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
                { provide: AlertService, useValue: alertServiceStub },
                WindowScrollService,
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
                ApplicationService,
                AbnPipe,
                BsbPipe,
                CreditCardNumberPipe,
                CurrencyPipe,
                CssIdentifierPipe,
                TimePipe,
                PhoneNumberPipe,
                NumberPlatePipe,
                SafeHtmlPipe,
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

    it('should trigger the calculation of it\'s value on first render when autoTriggerCalculatedValue '
        + 'is not set', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response: any = _.cloneDeep(formConfig);
        delete response.form.questionSets[0].fields[0].autoTriggerCalculatedValue;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let field: HTMLInputElement = fixture.debugElement.query(By.css('#testField1')).nativeElement;
                    expect(field.value).toBe('step1');
                    resolve();
                });
        });
    });

    it('should not trigger the calculation of it\'s value on first render when autoTriggerCalculatedValue '
        + 'is false', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response: any = _.cloneDeep(formConfig);
        response.form.questionSets[0].fields[0].autoTriggerCalculatedValue = false;
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let field: HTMLInputElement = fixture.debugElement.query(By.css('#testField1')).nativeElement;
                    expect(field.value).toBe('');
                    resolve();
                });
        });
    });

    it('should not trigger the calculation of it\'s value when rendered the second time because the form control has '
        + 'an existing value', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        TestBed.inject<WorkflowStatusService>(WorkflowStatusService);
        sut.ready = true;

        return new Promise((resolve: any, reject: any): void => {
            eventService.appLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    fixture.detectChanges();
                    setTimeout(() => {
                        let field1: HTMLInputElement = fixture.debugElement.query(By.css('#testField1')).nativeElement;
                        expect(field1.value).toBe('step1');
                    }, 60);

                    // navigate to step 2
                    workflowService.navigateTo({ stepName: 'step2' });
                    fixture.detectChanges();
                    sut.webFormComponent.transitioningOut = true;
                    sut.webFormComponent.onCompletedTransition();
                    sut.webFormComponent.transitioningIn = true;

                    fixture.detectChanges();
                    await fixture.whenStable();
                    setTimeout(() => {
                        // to know for sure we are on step 2, let's check that test field 2 exists
                        expect(applicationService.currentWorkflowDestination.stepName).toBe('step2');
                        let field2: HTMLInputElement =
                        fixture.debugElement.query(By.css('#testField2')).nativeElement;
                        expect(field2.value).toBe('hello');

                        // check that field1's value has not changed
                        let sameField: HTMLInputElement =
                            fixture.debugElement.query(By.css('#testField1')).nativeElement;
                        expect(sameField.value).toBe('step1');
                        resolve();
                    }, 60);
                });
        });
    }, 10000);

    it('should not clear the field\'s value if calculatedValueExpression is not set, and calculatedTriggerExpression '
        + ' is set', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response: any = _.cloneDeep(formConfig2);
        response.form.questionSets[0].fields[0].defaultValueExpression = "'hello'";
        response.form.questionSets[0].fields[0].calculatedValueTriggerExpression = 'testField2';
        response.form.questionSets[0].fields[0].calculatedValueConditionExpression = "testField2 != ''";
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let field1: HTMLInputElement = fixture.debugElement.query(By.css('#testField1')).nativeElement;
                    expect(field1.value).toBe('hello');
                    let field2: HTMLInputElement = fixture.debugElement.query(By.css('#testField2')).nativeElement;
                    expect(field2.value).toBe('');

                    // Act
                    field2.value = 'anything';
                    field2.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    field2.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    // Assert
                    let result: any = fixture.debugElement.query(By.css('#testField1')).nativeElement.value;
                    expect(result).toBe('hello');

                    resolve();
                });
        });
    });

    it('When a field\'s bootstrap columns change, the field\'s layout changes ', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].bootstrapColumnsExtraSmall = 6;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let field1: HTMLElement
                        = fixture.debugElement.query(By.css('formly-group formly-field:first-child')).nativeElement;
                    expect(field1.classList.contains('col-xs-12'))
                        .toBeTruthy('the field should have the class col-xs-12');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    field1 = fixture.debugElement.query(By.css('formly-group formly-field:first-child')).nativeElement;
                    expect(field1.classList.contains('col-xs-12'))
                        .toBeFalsy('the field should not have the class col-xs-12');
                    expect(field1.classList.contains('col-xs-6'))
                        .toBeTruthy('the field should have the class col-xs-6');
                    resolve();
                });
        });
    });

    it('When a field\'s width is set, its width becomes constrained', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].widgetCssWidth = '200px';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    expect(debugElement == null)
                        .toBeTruthy('the container wrapper should not have been rendered.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    expect(debugElement != null)
                        .toBeTruthy('a container wrapper should have been rendered');
                    resolve();
                });
        });
    });

    it('When a field\'s width is changed, its width updates', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].widgetCssWidth = '400px';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].widgetCssWidth = '200px';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    let wrapperEl: HTMLElement = debugElement.nativeElement;
                    expect(wrapperEl.getAttribute('style')).toBe('width: 400px;');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    wrapperEl = debugElement.nativeElement;
                    expect(wrapperEl.getAttribute('style')).toBe('width: 200px;');
                    resolve();
                });
        });
    });

    it('When a field\'s width is removed, its width is no longer constrained', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].widgetCssWidth = '400px';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].widgetCssWidth;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    expect(debugElement != null)
                        .toBeTruthy('the container wrapper should have been rendered.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    expect(debugElement == null)
                        .toBeTruthy('the container wrapper should have been removed');
                    resolve();
                });
        });
    });

    it('when a field\'s label is added, it starts showing a label', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        delete response1.form.questionSets[0].fields[0].label;
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].label = 'My test label';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child label'));
                    expect(debugElement == null)
                        .toBeTruthy('the label should not have been rendered.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child label'));
                    expect(debugElement != null)
                        .toBeTruthy('a label should have been rendered');
                    resolve();
                });
        });
    });

    it('When a field\'s label is updated, its label changes', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].label = 'Initial label';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].label = 'Updated label';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child label'));
                    let labelEl: HTMLElement = debugElement.nativeElement;
                    expect(labelEl.innerHTML).toBe('Initial label');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child label'));
                    labelEl = debugElement.nativeElement;
                    expect(labelEl.innerHTML).toBe('Updated label');
                    resolve();
                });
        });
    });

    it('when a field\'s label is removed, it stops showing a label', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].label = 'My Test Label';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].label;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child label'));
                    expect(debugElement != null)
                        .toBeTruthy('the label should have been rendered.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child label'));
                    expect(debugElement == null)
                        .toBeTruthy('a label should have been removed');
                    resolve();
                });
        });
    });

    it('when a field\'s question is added, it starts showing a question', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].question = 'My question';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child question-wrapper'));
                    expect(debugElement == null)
                        .toBeTruthy('the field should not have any question text');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child question-wrapper .question'));
                    let questionEl: HTMLElement = debugElement.nativeElement;
                    expect(questionEl.innerText).toBe('My question');
                    resolve();
                });
        });
    });

    it('When a field\'s question is updated, its question changes', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].question = 'Initial question';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].question = 'Updated question';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child question-wrapper .question'));
                    let labelEl: HTMLElement = debugElement.nativeElement;
                    expect(labelEl.innerHTML).toBe('Initial question');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child question-Wrapper .question'));
                    labelEl = debugElement.nativeElement;
                    expect(labelEl.innerHTML).toBe('Updated question');
                    resolve();
                });
        });
    });

    it('when a field\'s question is removed, it stops showing a question', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].question = 'My question';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].question;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child question-wrapper'));
                    expect(debugElement != null)
                        .toBeTruthy('the field should have an initial question wrapper');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child question-wrapper'));
                    expect(debugElement == null)
                        .toBeTruthy('the field\'s question-wrapper should have been removed');
                    resolve();
                });
        });
    });

    it('When a field\'s hideCondition is added, it hides when the condition is fulfilled', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].hideConditionExpression = "true";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child hide-wrapper.no-layout'));
                    expect(debugElement == null)
                        .toBeTruthy('the field should initially not be hidden');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child hide-wrapper.no-layout'));
                    expect(debugElement != null)
                        .toBeTruthy('the field should have become hidden');
                    resolve();
                });
        });
    });

    it('when a field\'s hideCondition is removed, it no longer hides', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].hideConditionExpression = "true";
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].hideConditionExpression;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child hide-wrapper.no-layout'));
                    expect(debugElement != null)
                        .toBeTruthy('the field should initially be hidden');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child hide-wrapper.no-layout'));
                    expect(debugElement == null)
                        .toBeTruthy('the field should have become not hidden');
                    resolve();
                });
        });
    });

    it('when a field has a validation rule added, it starts validating in accordance with the rule', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.pop();
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].validationRules
            = "customExpression('fieldValue > \\'50000\\'', 'You must declare a turnover of greater than $50k')";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let field1: HTMLInputElement = fixture.debugElement.query(By.css('#testField1')).nativeElement;
                    field1.value = '10000';
                    field1.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:first-child formly-wrapper-validation-messages'));
                    expect(debugElement != null).toBeTruthy('There should always be a validation wrapper');
                    expect(field1.classList.contains('ng-invalid')).toBeFalsy('The field should initially be valid');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    field1 = fixture.debugElement.query(By.css('#testField1')).nativeElement;
                    changeEvent1 = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    expect(field1.classList.contains('ng-invalid')).toBeTruthy('The field should show as invalid');
                    resolve();
                });
        });
    });

    it('when a field has a validation rule removed, it stops validating in accordance with that rule', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields[0].validationRules
            = "customExpression('fieldValue > \\'50000\\'', 'You must declare a turnover of greater than $50k')";
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].validationRules;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let field1: HTMLInputElement = fixture.debugElement.query(By.css('#testField1')).nativeElement;
                    field1.value = '10000';
                    field1.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:first-child formly-wrapper-validation-messages'));
                    expect(debugElement != null).toBeTruthy('There should always be a validation wrapper');
                    expect(field1.classList.contains('ng-invalid')).toBeTruthy('The field should initially be invalid');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    field1 = fixture.debugElement.query(By.css('#testField1')).nativeElement;
                    changeEvent1 = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    expect(field1.classList.contains('ng-invalid')).toBeFalsy('The field should no longer be invalid');
                    resolve();
                });
        });
    });

    it('when a field has a heading2 value set, it starts showing that as a h2', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].heading2 = 'My Heading2';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child content-wrapper'));
                    expect(debugElement == null)
                        .toBeTruthy('the content-wrapper should not have been rendered.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child content-wrapper h2'));
                    expect(debugElement != null)
                        .toBeTruthy('the h2 should have been rendered');
                    let h2Element: HTMLElement = debugElement.nativeElement;
                    expect(h2Element.innerText).toBe('My Heading2');
                    resolve();
                });
        });
    });

    it('when a field has a heading2 value removed, it stops showing a h2', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields[0].heading2 = 'My Heading2';
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].heading2;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child content-wrapper h2'));
                    expect(debugElement != null)
                        .toBeTruthy('the h2 should have been rendered');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child content-wrapper'));
                    expect(debugElement == null)
                        .toBeTruthy('the content-wrapper should have been removed');
                    resolve();
                });
        });
    });

    it('when a field has a heading2 value updated, it shows the updated text', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields[0].heading2 = 'My Heading2';
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].heading2 = 'My Updated Heading2';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child content-wrapper h2'));
                    expect(debugElement != null)
                        .toBeTruthy('the h2 should have been rendered');
                    let h2Element: HTMLElement = debugElement.nativeElement;
                    expect(h2Element.innerText).toBe('My Heading2');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child content-wrapper h2'));
                    h2Element = debugElement.nativeElement;
                    expect(h2Element.innerText).toBe('My Updated Heading2');
                    resolve();
                });
        });
    });

    it('when a field has a placeholder added, it starts showing that placeholder if the field is empty', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].placeholder = 'My placeholder text';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    expect(fieldEl.attributes.getNamedItem('placeholder') == null)
                        .toBeTruthy('the field should not have a placeholder');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    fieldEl = debugElement.nativeElement;
                    let placeholderAttr: Attr = fieldEl.attributes.getNamedItem('placeholder');
                    expect(placeholderAttr != null)
                        .toBeTruthy('the field input element should have a placeholder attribute');
                    expect(placeholderAttr.value).toBe('My placeholder text');
                    resolve();
                });
        });
    });

    it('When a field has its placeholder updated, it updates the placeholder if it\'s shown', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].placeholder = 'My initial placeholder';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].placeholder = 'My updated placeholder';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    let placeholderAttr: Attr = fieldEl.attributes.getNamedItem('placeholder');
                    expect(placeholderAttr != null)
                        .toBeTruthy('the field input element should have a placeholder attribute to start with');
                    expect(placeholderAttr.value).toBe('My initial placeholder');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    fieldEl = debugElement.nativeElement;
                    placeholderAttr = fieldEl.attributes.getNamedItem('placeholder');
                    expect(placeholderAttr.value).toBe('My updated placeholder');
                    resolve();
                });
        });
    });

    it('when a field has a placeholder removed, it stops showing that placeholder', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields[0].placeholder = 'My placeholder text';
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].placeholder;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    let placeholderAttr: Attr = fieldEl.attributes.getNamedItem('placeholder');
                    expect(placeholderAttr != null)
                        .toBeTruthy('the field input element should initially have a placeholder attribute');
                    expect(placeholderAttr.value).toBe('My placeholder text');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    fieldEl = debugElement.nativeElement;
                    expect(fieldEl.attributes.getNamedItem('placeholder') == null)
                        .toBeTruthy('the field should no longer have a placeholder');

                    resolve();
                });
        });
    });

    it('when a field has a tooltip added, the tooltip icon appears', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].tooltip = 'My tooltip text';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child ubind-tooltip-widget-ng'));
                    expect(debugElement == null)
                        .toBeTruthy('the field should not have a tooltip icon');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:first-child ubind-tooltip-widget-ng'));
                    expect(debugElement != null)
                        .toBeTruthy('the field should now have a tooltip icon');
                    resolve();
                });
        });
    });

    /**
     * We cannot include this test in normal test runs. It needs to be run individually. For a full explanation, see:
     * https://stackoverflow.com/questions/73849002/unit-tests-of-angular-elements-registered-with-the-customelementregistry-as-a-h
     */
    xit('when a field has a tooltip updated, it shows the updated tooltip text', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields[0].tooltip = 'My initial tooltip text';
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].tooltip = 'My updated tooltip text';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child ubind-tooltip-widget'));
                    let tooltipEl: HTMLElement = debugElement.nativeElement;
                    expect(tooltipEl.innerHTML).toBe('My initial tooltip text');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:first-child ubind-tooltip-widget'));
                    tooltipEl = debugElement.nativeElement;
                    expect(tooltipEl.innerHTML).toBe('My updated tooltip text');
                    resolve();
                });
        });
    });

    it('when a field has a tooltip removed, the tooltip icon disappears', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields[0].tooltip = 'My tooltip text';
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].tooltip;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child ubind-tooltip-widget-ng'));
                    expect(debugElement != null)
                        .toBeTruthy('the field should initially have a tooltip icon');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:first-child ubind-tooltip-widget'));
                    expect(debugElement == null)
                        .toBeTruthy('the field\'s tooltip icon should no longer be present');
                    resolve();
                });
        });
    });

    it('when a field has a calculated value added, the fields value updates', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        delete response1.form.questionSets[0].fields[0].defaultValueExpression;
        delete response1.form.questionSets[0].fields[0].calculatedValueTriggerExpression;
        delete response1.form.questionSets[0].fields[0].calculatedValueConditionExpression;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].calculatedValueExpression = "testField2";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#testField2'));
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'The calculated value';
                    fieldEl2.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent1);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    expect(fieldEl.value).toBe('');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    setTimeout(() => {
                        debugElement = fixture.debugElement.query(By.css('#testField1'));
                        fieldEl = debugElement.nativeElement;
                        expect(fieldEl.value).toBe("The calculated value");
                        resolve();
                    }, 60);
                });
        });
    });

    it('when a field has a calculated value removed, the fields value no longer updates', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        delete response1.form.questionSets[0].fields[0].defaultValueExpression;
        delete response1.form.questionSets[0].fields[0].calculatedValueTriggerExpression;
        delete response1.form.questionSets[0].fields[0].calculatedValueConditionExpression;
        response1.form.questionSets[0].fields[0].calculatedValueExpression = "testField2";
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].calculatedValueExpression;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#testField2'));
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'The calculated value';
                    fieldEl2.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent1);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    setTimeout(() => {
                        expect(fieldEl.value).toBe("The calculated value");

                        // Act
                        configProcessorService.onConfigurationResponse(response2);
                        fixture.detectChanges();

                        // Assert
                        debugElement2 = fixture.debugElement.query(By.css('#testField2'));
                        fieldEl2 = debugElement2.nativeElement;
                        fieldEl2.value = 'The new calculated value';
                        fieldEl2.dispatchEvent(new Event('input'));
                        changeEvent1 = new Event('change', { bubbles: true, cancelable: false });
                        fieldEl2.dispatchEvent(changeEvent1);
                        fixture.detectChanges();

                        debugElement = fixture.debugElement.query(By.css('#testField1'));
                        fieldEl = debugElement.nativeElement;
                        expect(fieldEl.value).toBe("The calculated value");
                        resolve();
                    }, 60);
                });
        });
    });

    it('when a field has a calculated trigger added, the fields value updates when the trigger '
        + 'evaluates to something different', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        delete response1.form.questionSets[0].fields[0].defaultValueExpression;
        delete response1.form.questionSets[0].fields[0].calculatedValueTriggerExpression;
        delete response1.form.questionSets[0].fields[0].calculatedValueConditionExpression;
        response1.form.questionSets[0].fields[0].calculatedValueExpression = "'The calculated value' + testField3";
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.push({
            "$type": "single-line-text",
            "sensitive": false,
            "required": false,
            "affectsPremium": false,
            "affectsTriggers": false,
            "requiredForCalculations": false,
            "private": false,
            "displayable": true,
            "canChangeWhenApproved": false,
            "resetForNewQuotes": false,
            "resetForNewRenewalQuotes": false,
            "resetForNewAdjustmentQuotes": false,
            "resetForNewCancellationQuotes": false,
            "resetForNewPurchaseQuotes": false,
            "startsNewRow": false,
            "bootstrapColumnsExtraSmall": 12,
            "label": "Test Field 3",
            "name": "Test Field 3",
            "key": "testField3",
            "questionSetKey": "testQuestionSet1",
            "dataType": "text",
            "workflowRole": "none",
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].calculatedValueTriggerExpression = "testField2";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#testField2'));
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'I changed';
                    fieldEl2.dispatchEvent(new Event('input'));
                    let changeEvent2: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    expect(fieldEl.value).toBe(
                        'The calculated value',
                        'since testField3 has no value it should just be \'The calculated value\'');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    let debugElement3: DebugElement = fixture.debugElement.query(By.css('#testField3'));
                    let fieldEl3: HTMLInputElement = debugElement3.nativeElement;
                    fieldEl3.value = 'XXXX';
                    fieldEl3.dispatchEvent(new Event('input'));
                    let changeEvent3: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl3.dispatchEvent(changeEvent3);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    fieldEl = debugElement.nativeElement;
                    expect(fieldEl.value).toBe(
                        'The calculated value',
                        'since testField2 has not changed yet it should just be \'The calculated value\'');

                    debugElement2 = fixture.debugElement.query(By.css('#testField2'));
                    fieldEl2 = debugElement2.nativeElement;
                    fieldEl2.value = 'I changed again';
                    fieldEl2.dispatchEvent(new Event('input'));
                    changeEvent2 = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);
                    fixture.detectChanges();
                    setTimeout(() => {
                        debugElement = fixture.debugElement.query(By.css('#testField1'));
                        fieldEl = debugElement.nativeElement;
                        expect(fieldEl.value).toBe(
                            "The calculated valueXXXX",
                            "since testField2 has changed, it should have caused the calculated value to update");
                        resolve();
                    }, 60);
                });
        });
    });

    it('when a field has a calculated condition added, the fields value only updates once '
        + 'the condition evaluates to true', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        delete response1.form.questionSets[0].fields[0].defaultValueExpression;
        delete response1.form.questionSets[0].fields[0].calculatedValueTriggerExpression;
        delete response1.form.questionSets[0].fields[0].calculatedValueConditionExpression;
        response1.form.questionSets[0].fields[0].calculatedValueExpression = "'The calculated value' + testField3";
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.push({
            "$type": "single-line-text",
            "sensitive": false,
            "required": false,
            "affectsPremium": false,
            "affectsTriggers": false,
            "requiredForCalculations": false,
            "private": false,
            "displayable": true,
            "canChangeWhenApproved": false,
            "resetForNewQuotes": false,
            "resetForNewRenewalQuotes": false,
            "resetForNewAdjustmentQuotes": false,
            "resetForNewCancellationQuotes": false,
            "resetForNewPurchaseQuotes": false,
            "startsNewRow": false,
            "bootstrapColumnsExtraSmall": 12,
            "label": "Test Field 3",
            "name": "Test Field 3",
            "key": "testField3",
            "questionSetKey": "testQuestionSet1",
            "dataType": "text",
            "workflowRole": "none",
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].calculatedValueConditionExpression = "testField2 == 'YYYY'";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#testField2'));
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'I changed';
                    fieldEl2.dispatchEvent(new Event('input'));
                    let changeEvent2: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    expect(fieldEl.value).toBe(
                        'The calculated value',
                        'since testField3 has no value it should just be \'The calculated value\'');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    let debugElement3: DebugElement = fixture.debugElement.query(By.css('#testField3'));
                    let fieldEl3: HTMLInputElement = debugElement3.nativeElement;
                    fieldEl3.value = 'XXXX';
                    fieldEl3.dispatchEvent(new Event('input'));
                    let changeEvent3: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl3.dispatchEvent(changeEvent3);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    fieldEl = debugElement.nativeElement;
                    expect(fieldEl.value).toBe(
                        'The calculated value',
                        'since testField2 does not equal \'YYYY\', it should just be \'The calculated value\'');

                    debugElement2 = fixture.debugElement.query(By.css('#testField2'));
                    fieldEl2 = debugElement2.nativeElement;
                    fieldEl2.value = 'YYYY';
                    fieldEl2.dispatchEvent(new Event('input'));
                    changeEvent2 = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);

                    debugElement3 = fixture.debugElement.query(By.css('#testField3'));
                    fieldEl3 = debugElement3.nativeElement;
                    fieldEl3.value = 'ZZZZ';
                    fieldEl3.dispatchEvent(new Event('input'));
                    changeEvent3 = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl3.dispatchEvent(changeEvent3);
                    fixture.detectChanges();
                    setTimeout(() => {
                        debugElement = fixture.debugElement.query(By.css('#testField1'));
                        fieldEl = debugElement.nativeElement;
                        expect(fieldEl.value).toBe(
                            "The calculated valueZZZZ",
                            "since testField2 has changed to 'YYYY', the calculated value should have updated");
                        resolve();
                    }, 60);
                });
        });
    });

    it('when a field has a disabled condition added, the field becomes disabled', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].disabledConditionExpression = 'testField2 == \'YYYY\'';
        response2.form.questionSets[0].fields[0].calculatedValueConditionExpression = "testField2 != 'YYYY'";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#testField2'));
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'YYYY';
                    fieldEl2.dispatchEvent(new Event('input'));
                    let changeEvent2: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);
                    fixture.detectChanges();

                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    expect(fieldEl.attributes.getNamedItem('disabled') == null)
                        .toBeTruthy('the field should not be disabled');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    setTimeout(() => {
                        debugElement = fixture.debugElement.query(By.css('#testField1'));
                        fieldEl = debugElement.nativeElement;
                        expect(fieldEl.attributes.getNamedItem('disabled') != null)
                            .toBeTruthy('the field should become disabled');
                        resolve();
                    }, 60);
                });
        });
    });

    it('when a field has a disabled condition removed, the field becomes enabled', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].disabledConditionExpression = 'testField2 == \'YYYY\'';
        response1.form.questionSets[0].fields[0].calculatedValueConditionExpression = "testField2 != 'YYYY'";
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].disabledConditionExpression;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#testField2'));
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'YYYY';
                    fieldEl2.dispatchEvent(new Event('input'));
                    let changeEvent2: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);
                    fixture.detectChanges();

                    setTimeout(() => {
                        let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                        let fieldEl: HTMLInputElement = debugElement.nativeElement;
                        expect(fieldEl.attributes.getNamedItem('disabled') != null)
                            .toBeTruthy('the field should initially be disabled');

                        // Act
                        configProcessorService.onConfigurationResponse(response2);
                        fixture.detectChanges();

                        // Assert
                        setTimeout(() => {
                            debugElement = fixture.debugElement.query(By.css('#testField1'));
                            fieldEl = debugElement.nativeElement;
                            expect(fieldEl.attributes.getNamedItem('disabled') == null)
                                .toBeTruthy('the field should become enabled');
                            resolve();
                        }, 60);

                    }, 60);
                });
        });
    });

    it('when a field has affectsPremium added, changing the field value will '
        + 'start triggering a calculation', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let calculationSpy: jasmine.Spy = spyOn(calculationService, "generateQuoteRequest");
        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].affectsPremium = false;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].affectsPremium = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'ABC';
                    fieldEl2.dispatchEvent(new Event('input'));
                    let changeEvent2: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);
                    fixture.detectChanges();
                    expect(calculationSpy).not.toHaveBeenCalled();

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    expect(calculationSpy).toHaveBeenCalled();
                    resolve();
                });
        });
    });

    it('when a field has affectsPremium removed, changing the field value will no longer'
        + 'trigger a calculation', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let calculationSpy: jasmine.Spy = spyOn(calculationService, "generateQuoteRequest");
        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].affectsPremium = true;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].affectsPremium = false;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'ABC';
                    fieldEl2.dispatchEvent(new Event('input'));
                    let changeEvent2: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);
                    fixture.detectChanges();
                    expect(calculationSpy).toHaveBeenCalled();
                    calculationSpy.calls.reset();

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    debugElement2 = fixture.debugElement.query(By.css('#testField1'));
                    fieldEl2 = debugElement2.nativeElement;
                    fieldEl2.value = 'DEF';
                    fieldEl2.dispatchEvent(new Event('input'));
                    changeEvent2 = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent2);
                    fixture.detectChanges();

                    // Assert
                    expect(calculationSpy).not.toHaveBeenCalled();
                    resolve();
                });
        });
    });

    it('When a field has requiredForCalculations set, clearing the field value and '
        + 'changing another field will no longer trigger the calculation', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let performCalculationSpy: jasmine.Spy = spyOn<any>(calculationService, "triggerCalculation");
        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].affectsPremium = true;
        response1.form.questionSets[0].fields[1].required = true;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[1].requiredForCalculations = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = 'ABC';
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    expect(performCalculationSpy).toHaveBeenCalled();
                    performCalculationSpy.calls.reset();

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    fieldEl = debugElement.nativeElement;
                    fieldEl.value = 'DEF';
                    fieldEl.dispatchEvent(new Event('input'));
                    changeEvent = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    // Assert
                    expect(performCalculationSpy).not.toHaveBeenCalled();
                    performCalculationSpy.calls.reset();
                    resolve();
                });
        });
    });

    it('When a field has requiredForCalculations removed, clearing the field value and '
        + 'changing another field will start triggering the calculation', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let performCalculationSpy: jasmine.Spy = spyOn<any>(calculationService, "triggerCalculation");
        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].affectsPremium = true;
        response1.form.questionSets[0].fields[1].required = true;
        response1.form.questionSets[0].fields[1].requiredForCalculations = true;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[1].requiredForCalculations;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = 'ABC';
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    expect(performCalculationSpy).not.toHaveBeenCalled();

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    fieldEl = debugElement.nativeElement;
                    fieldEl.value = 'DEF';
                    fieldEl.dispatchEvent(new Event('input'));
                    changeEvent = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    // Assert
                    expect(performCalculationSpy).toHaveBeenCalled();
                    performCalculationSpy.calls.reset();
                    resolve();
                });
        });
    });

    it('it should trigger calculation on initialization when all field marked as required for calculation are all valid'
        + ' and there is also field marked as affectsPremium',
    async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let performCalculationSpy: jasmine.Spy = spyOn<any>(calculationService, "triggerCalculation");
        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].required = true;
        response1.form.questionSets[0].fields[0].requiredForCalculations = true;
        response1.form.questionSets[0].fields[1].affectsPremium = true;
        response1.form.questionSets[0].fields[1].required = true;
        response1.form.questionSets[0].fields[1].requiredForCalculations = true;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[1].requiredForCalculations;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = 'ABC';
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('#testField2'));
                    let fieldE2: HTMLInputElement = debugElement.nativeElement;
                    fieldE2.value = 'ABC';
                    fieldE2.dispatchEvent(new Event('input'));
                    changeEvent = new Event('change', { bubbles: true, cancelable: false });
                    fieldE2.dispatchEvent(changeEvent);
                    fixture.detectChanges();


                    expect(performCalculationSpy).toHaveBeenCalled();
                    resolve();
                });
        });

    });

    it('it should not trigger calculation on initialization when there is an invalid'
        + 'field marked as required for calculation,'
    ,async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let performCalculationSpy: jasmine.Spy = spyOn<any>(calculationService, "triggerCalculation");
        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].required = true;
        response1.form.questionSets[0].fields[0].requiredForCalculations = false;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[1].requiredForCalculations;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();
        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let emptyValue: string = '';
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = emptyValue;
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    expect(performCalculationSpy).not.toHaveBeenCalled();
                    resolve();
                });
        });

    });


    it('When a field has the workflow role "customerEmail" added, its value will start being used '
        + 'when creating a customer', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].workflowRole = WorkflowRole.CustomerEmail;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();
        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = 'testemail@customer.com';
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    let formValues: object = formService.getValues();
                    let value: any = formService.getValueForWorkflowRole(WorkflowRole.CustomerEmail, formValues);
                    expect(value).toBeUndefined();

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    value = formService.getValueForWorkflowRole(WorkflowRole.CustomerEmail, formValues);
                    expect(value).toBe('testemail@customer.com');
                    resolve();
                });
        });
    });

    it('When a field has private set to true, its value will no longer be included in the payload', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].private = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();
        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = 'testemail@customer.com';
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    let formValues: object = formService.getValues(true, true, false, false);
                    expect(formValues['testField1']).toBe('testemail@customer.com');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    formValues = formService.getValues(true, true, false, false);
                    expect(formValues['testField1']).toBeUndefined();
                    resolve();
                });
        });
    });

    it('When a field has private set to false, its value will start being included in the payload', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let formService: FormService = TestBed.inject<FormService>(FormService);
        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].private = true;
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].private = false;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#testField1'));
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = 'testemail@customer.com';
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    let formValues: object = formService.getValues(true, true, false, false);
                    expect(formValues['testField1']).toBeUndefined();

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    formValues = formService.getValues(true, true, false, false);
                    expect(formValues['testField1']).toBe('testemail@customer.com');
                    resolve();
                });
        });
    });

    it('when a field has containerClass set, the class will be added to the field\'s container', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].containerClass = 'my-test-class';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    expect(debugElement == null)
                        .toBeTruthy('the container wrapper should not have been rendered.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    expect(debugElement != null)
                        .toBeTruthy('a container wrapper should have been rendered');
                    let containerEl: HTMLElement = debugElement.nativeElement;
                    expect(containerEl.classList.contains('my-test-class'))
                        .toBeTruthy('the wrapper should have the css class');
                    resolve();
                });
        });
    });

    it('when a field has containerCss set, the style will be added to the field\'s container', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].containerCss = 'border: 1px solid green;';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    expect(debugElement == null)
                        .toBeTruthy('the container wrapper should not have been rendered.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child container-wrapper'));
                    expect(debugElement != null)
                        .toBeTruthy('a container wrapper should have been rendered');
                    let containerEl: HTMLElement = debugElement.nativeElement;
                    expect(containerEl.style.border).toBe('1px solid green');
                    resolve();
                });
        });
    });

    it('when a field has a tag added, and that tag is used to generate a summary table, '
        + 'that field will start being included in the summary table', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].defaultValueExpression = "'testField1Value'";
        response1.form.questionSets[0].fields[1].defaultValueExpression = "'testField2Value'";
        response1.form.questionSets[0].fields[0].tags = [ 'testTag' ];
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.push({
            "$type": "content",
            "sensitive": false,
            "required": false,
            "affectsPremium": false,
            "affectsTriggers": false,
            "requiredForCalculations": false,
            "private": false,
            "displayable": true,
            "canChangeWhenApproved": false,
            "resetForNewQuotes": false,
            "resetForNewRenewalQuotes": false,
            "resetForNewAdjustmentQuotes": false,
            "resetForNewCancellationQuotes": false,
            "resetForNewPurchaseQuotes": false,
            "startsNewRow": false,
            "bootstrapColumnsExtraSmall": 12,
            "label": "Test Field 3",
            "name": "Test Field 3",
            "key": "testField3",
            "questionSetKey": "testQuestionSet1",
            "dataType": "text",
            "workflowRole": "none",
            "html": "%{ generateSummaryTableOfFieldsWithTag('testTag') }%",
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[1].tags = [ 'testTag' ];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child content-wrapper div.html'));
                    expect(debugElement.nativeElement.innerHTML).toContain('testField1Value');
                    expect(debugElement.nativeElement.innerHTML).not.toContain('testField2Value');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child content-wrapper div.html'));
                    expect(debugElement.nativeElement.innerHTML).toContain('testField2Value');
                    resolve();
                });
        });
    });

    it('when a field has a tag removed, and that tag is used to generate a summary table, '
        + 'that field will stop being included in the summary table', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].defaultValueExpression = "'testField1Value'";
        response1.form.questionSets[0].fields[1].defaultValueExpression = "'testField2Value'";
        response1.form.questionSets[0].fields[0].tags = [ 'testTag' ];
        response1.form.questionSets[0].fields[1].tags = [ 'testTag' ];
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.push({
            "$type": "content",
            "sensitive": false,
            "required": false,
            "affectsPremium": false,
            "affectsTriggers": false,
            "requiredForCalculations": false,
            "private": false,
            "displayable": true,
            "canChangeWhenApproved": false,
            "resetForNewQuotes": false,
            "resetForNewRenewalQuotes": false,
            "resetForNewAdjustmentQuotes": false,
            "resetForNewCancellationQuotes": false,
            "resetForNewPurchaseQuotes": false,
            "startsNewRow": false,
            "bootstrapColumnsExtraSmall": 12,
            "label": "Test Field 3",
            "name": "Test Field 3",
            "key": "testField3",
            "questionSetKey": "testQuestionSet1",
            "dataType": "text",
            "workflowRole": "none",
            "html": "%{ generateSummaryTableOfFieldsWithTag('testTag') }%",
        });
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[1].tags;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child content-wrapper div.html'));
                    expect(debugElement.nativeElement.innerHTML).toContain('testField1Value');
                    expect(debugElement.nativeElement.innerHTML).toContain('testField2Value');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child content-wrapper div.html'));
                    expect(debugElement.nativeElement.innerHTML).not.toContain('testField2Value');
                    resolve();
                });
        });
    });

    it('when a new field is added which has a tag, and that tag is used to generate a summary table, '
        + 'that field will start being included in the summary table', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].defaultValueExpression = "'testField1Value'";
        response1.form.questionSets[0].fields[1].defaultValueExpression = "'testField2Value'";
        response1.form.questionSets[0].fields[0].tags = [ 'testTag' ];
        response1.form.questionSets[0].fields[1].tags = [ 'testTag' ];
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.push({
            "$type": "content",
            "sensitive": false,
            "required": false,
            "affectsPremium": false,
            "affectsTriggers": false,
            "requiredForCalculations": false,
            "private": false,
            "displayable": true,
            "canChangeWhenApproved": false,
            "resetForNewQuotes": false,
            "resetForNewRenewalQuotes": false,
            "resetForNewAdjustmentQuotes": false,
            "resetForNewCancellationQuotes": false,
            "resetForNewPurchaseQuotes": false,
            "startsNewRow": false,
            "bootstrapColumnsExtraSmall": 12,
            "label": "Test Field 3",
            "name": "Test Field 3",
            "key": "testField3",
            "questionSetKey": "testQuestionSet1",
            "dataType": "text",
            "workflowRole": "none",
            "html": "%{ generateSummaryTableOfFieldsWithTag('testTag') }%",
        });
        let response2: any = _.cloneDeep(response1);
        response1.form.questionSets[0].fields.splice(1, 1);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child content-wrapper div.html'));
                    expect(debugElement.nativeElement.innerHTML).toContain('testField1Value');
                    expect(debugElement.nativeElement.innerHTML).not.toContain('testField2Value');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child content-wrapper div.html'));
                    expect(debugElement.nativeElement.innerHTML).toContain('testField2Value');
                    resolve();
                });
        });
    });

    it('when a field is removed which has a tag, and that tag is used to generate a summary table, '
        + 'that field will no longer be included in the summary table', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1.form.questionSets[0].fields[0].defaultValueExpression = "'testField1Value'";
        response1.form.questionSets[0].fields[1].defaultValueExpression = "'testField2Value'";
        response1.form.questionSets[0].fields[0].tags = [ 'testTag' ];
        response1.form.questionSets[0].fields[1].tags = [ 'testTag' ];
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.push({
            "$type": "content",
            "sensitive": false,
            "required": false,
            "affectsPremium": false,
            "affectsTriggers": false,
            "requiredForCalculations": false,
            "private": false,
            "displayable": true,
            "canChangeWhenApproved": false,
            "resetForNewQuotes": false,
            "resetForNewRenewalQuotes": false,
            "resetForNewAdjustmentQuotes": false,
            "resetForNewCancellationQuotes": false,
            "resetForNewPurchaseQuotes": false,
            "startsNewRow": false,
            "bootstrapColumnsExtraSmall": 12,
            "label": "Test Field 3",
            "name": "Test Field 3",
            "key": "testField3",
            "questionSetKey": "testQuestionSet1",
            "dataType": "text",
            "workflowRole": "none",
            "html": "%{ generateSummaryTableOfFieldsWithTag('testTag') }%",
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields.splice(1, 1);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child content-wrapper div.html'));
                    expect(debugElement.nativeElement.innerHTML).toContain('testField1Value');
                    expect(debugElement.nativeElement.innerHTML).toContain('testField2Value');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child content-wrapper div.html'));
                    expect(debugElement.nativeElement.innerHTML).not.toContain('testField2Value');
                    resolve();
                });
        });
    });

    it('when a field is changed from a Text field to a Currency Field, a dollar symbol appears', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].$type = FieldType.Currency;
        response2.form.questionSets[0].fields[0].dataType = FieldDataType.Currency;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child .fa-usd'));
                    expect(debugElement == null)
                        .toBeTruthy('the field initially should not have a dollar sign');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child .fa-usd'));
                    expect(debugElement != null)
                        .toBeTruthy('the field should now have a dollar sign');
                    resolve();
                });
        });
    });

    it('when a field is changed from a Text field to a Date Picker Field, '
        + 'the date picker becomes available', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].$type = FieldType.DatePicker;
        response2.form.questionSets[0].fields[0].dataType = FieldDataType.Date;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child datepicker-field'));
                    expect(debugElement == null)
                        .toBeTruthy('the field initially should not have a date picker');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(
                            By.css('formly-group formly-field:first-child datepicker-field'));
                    expect(debugElement != null)
                        .toBeTruthy('the field should now have a date picker');
                    resolve();
                });
        });
    });

    it('when a field\'s iconLeft is added, the icon is now shown in the left of the field', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].iconLeft = 'fas fa-car';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugEl: DebugElement = fixture.debugElement.query(By.css(
                        '#anchor-testField1 addons-wrapper > .input-group-addon .fa-car'));
                    expect(debugEl).toBeNull('initially there should be no car icon in the left of the field');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-testField1 addons-wrapper > .input-group-addon .fa-car'));
                    expect(debugEl).not.toBeNull('The car icon should have been added');
                    resolve();
                });
        });
    });

});
