// eslint-disable-next-line max-classes-per-file
import { EncryptionService } from '@app/services/encryption.service';
import { ChangeDetectionStrategy, Component, DebugElement, ViewChild, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
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
import formConfig from './toggle-field.test-form-config.json';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { AppEventService } from '@app/services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import { WebFormComponent } from '@app/components/web-form/web-form';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { SafeHtmlPipe } from '@app/pipes/safe-html.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { Expression } from '@app/expressions/expression';
import { ExpressionInputSubjectService } from '@app/expressions/expression-input-subject.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationStatusService } from '@app/services/operation-status.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */

describe('ToggleField', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;
    let workflowService: WorkflowService;

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
                ExpressionInputSubjectService,
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

    it('when a toggle field has an icon added, the icon appears', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].icon = 'fas fa-car';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
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
                            By.css('formly-group formly-field:first-child toggle-field .btn-icon'));
                    expect(debugElement == null)
                        .toBeTruthy('the toggle field should initially not have an icon');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:first-child toggle-field .btn-icon'));
                    expect(debugElement != null)
                        .toBeTruthy('the toggle field should now have an icon');
                    resolve();
                });
        });
    });

    it('when a toggle field has an icon updated, the new icon shows instead', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].icon = 'fas fa-car';
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].icon = 'fas fa-city';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
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
                            By.css('formly-group formly-field:first-child toggle-field .btn-icon .fa-car'));
                    expect(debugElement != null)
                        .toBeTruthy('the toggle field should initially have the fa-car icon');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:first-child toggle-field .btn-icon .fa-city'));
                    expect(debugElement != null)
                        .toBeTruthy('the toggle field should now have the fa-city icon');
                    resolve();
                });
        });
    });

    it('when a toggle field has an icon removed, the icon disappears', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].icon = 'fas fa-car';
        let response2: any = _.cloneDeep(response1);
        delete response2.form.questionSets[0].fields[0].icon;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
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
                            By.css('formly-group formly-field:first-child toggle-field .btn-icon'));
                    expect(debugElement != null)
                        .toBeTruthy('the toggle field should initially have an icon');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:first-child toggle-field .btn-icon'));
                    expect(debugElement == null)
                        .toBeTruthy('the toggle field should no longer have an icon');
                    resolve();
                });
        });
    });

    it('when a toggle field is hidden, its value for expressions is set to an empty string, '
        + 'but restored when unhidden', async () => {
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        // response.form.workflowConfiguration.step1.articles[0].elements[0].hiddenExpression = "testValue == 'hide'";
        response.form.questionSets[0].fields[0].hideConditionExpression = "testValue == 'hide'";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
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
                        = fixture.debugElement.query(By.css('#testField1'));
                    expect(debugElement != null)
                        .toBeTruthy('the toggle field should initially be shown');
                    let checkBoxEl: HTMLInputElement = debugElement.nativeElement;
                    expect(checkBoxEl.checked).toBeFalsy();
                    checkBoxEl.click();
                    fixture.detectChanges();
                    expect(checkBoxEl.checked).toBeTruthy();

                    let expressionValue: any = null;
                    let expression: Expression = new Expression(
                        'testField1',
                        expressionDependencies,
                        'test expression');
                    expression.nextResultObservable.subscribe((value: any) => {
                        expressionValue = value;
                    });
                    expression.triggerEvaluation();
                    expect(expressionValue).toBe(true);

                    checkBoxEl.click();
                    fixture.detectChanges();
                    expect(checkBoxEl.checked).toBeFalsy();
                    expect(expressionValue).toBe(false);

                    // Act
                    eventService.fieldPathAddedSubject.next('testValue');
                    expressionDependencies.expressionInputSubjectService.getFieldValueSubject('testValue', 'hide');
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    expect(expressionValue).toBe('');

                    // Act
                    eventService.fieldPathAddedSubject.next('testValue');
                    expressionDependencies.expressionInputSubjectService.getFieldValueSubject('testValue', 'show');
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    expect(expressionValue).toBeFalse();
                    resolve();
                });
        });
    });

    it('when a toggle field is in a question set that becomes hidden, '
        + 'its value for expressions is set to an empty string, but restored when unhidden', async () => {
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[0].elements[0].hiddenExpression = "testValue == 'hide'";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
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
                        = fixture.debugElement.query(By.css('#testField1'));
                    expect(debugElement != null)
                        .toBeTruthy('the toggle field should initially be shown');
                    let checkBoxEl: HTMLInputElement = debugElement.nativeElement;
                    expect(checkBoxEl.checked).toBeFalsy();
                    checkBoxEl.click();
                    fixture.detectChanges();
                    expect(checkBoxEl.checked).toBeTruthy();

                    let expressionValue: any = null;
                    let expression: Expression = new Expression(
                        'testField1',
                        expressionDependencies,
                        'test expression');
                    expression.nextResultObservable.subscribe((value: any) => {
                        expressionValue = value;
                    });
                    expression.triggerEvaluation();
                    expect(expressionValue).toBe(true);

                    checkBoxEl.click();
                    fixture.detectChanges();
                    expect(checkBoxEl.checked).toBeFalsy();
                    expect(expressionValue).toBe(false);

                    // Act
                    eventService.fieldPathAddedSubject.next('testValue');
                    expressionDependencies.expressionInputSubjectService.getFieldValueSubject('testValue', 'hide');
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    expect(expressionValue).toBe('');

                    // Act
                    eventService.fieldPathAddedSubject.next('testValue');
                    expressionDependencies.expressionInputSubjectService.getFieldValueSubject('testValue', 'show');
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    expect(expressionValue).toBeFalse();
                    resolve();
                });
        });
    });

    it('when a toggle field is within an article that becomes hidden, '
        + 'its value for expressions is set to an empty string, but restored when unhidden', async () => {
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let expressionDependencies: ExpressionDependencies
            = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[0].hiddenExpression = "testValue == 'hide'";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let articleDebug: DebugElement = fixture.debugElement.query(By.css('article-widget'));
                    expect(articleDebug != null).toBeTruthy('the article widget should be rendered');
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('#testField1'));
                    expect(debugElement != null)
                        .toBeTruthy('the toggle field should initially be shown');
                    let checkBoxEl: HTMLInputElement = debugElement.nativeElement;
                    expect(checkBoxEl.checked).toBeFalsy();
                    checkBoxEl.click();
                    fixture.detectChanges();
                    expect(checkBoxEl.checked).toBeTruthy();

                    let expressionValue: any = null;
                    let expression: Expression = new Expression(
                        'testField1',
                        expressionDependencies,
                        'test expression');
                    expression.nextResultObservable.subscribe((value: any) => {
                        expressionValue = value;
                    });
                    expression.triggerEvaluation();
                    expect(expressionValue).toBe(true);

                    checkBoxEl.click();
                    fixture.detectChanges();
                    expect(checkBoxEl.checked).toBeFalsy();
                    expect(expressionValue).toBe(false);

                    // Act
                    eventService.fieldPathAddedSubject.next('testValue');
                    expressionDependencies.expressionInputSubjectService.getFieldValueSubject('testValue', 'hide');
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    expect(expressionValue).toBe('');

                    // Act
                    eventService.fieldPathAddedSubject.next('testValue');
                    expressionDependencies.expressionInputSubjectService.getFieldValueSubject('testValue', 'show');
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testField1'));
                    expect(expressionValue).toBeFalse();
                    resolve();
                });
        });
    });

});
