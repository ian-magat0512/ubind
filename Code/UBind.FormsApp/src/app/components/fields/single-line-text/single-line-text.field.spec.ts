import { ChangeDetectionStrategy, Component, EventEmitter, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
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
import { filter } from 'rxjs/operators';
import { By } from '@angular/platform-browser';
import * as _ from 'lodash-es';
import formConfig from './single-line-text.test-form-config.json';
import { Expression } from '@app/expressions/expression';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { MaskPipe, NgxMaskModule } from 'ngx-mask';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from '@app/services/operation-status.service';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */

describe('SingleLineTextField', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;

    let workflowServiceStub: any = {
        navigate: new EventEmitter<any>(),
        currentDestination: { stepName: "purchaseQuote" },
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
                { provide: EncryptionService, useValue: encryptionServiceStub },
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                { provide: EvaluateService, useClass: EvaluateService },
                { provide: EventService, useClass: EventService },
                { provide: CalculationService, useClass: CalculationService },
                { provide: WorkflowService, useValue: workflowServiceStub },
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
                MaskPipe,
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
                NgxMaskModule,
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

    it('should set it\'s value from a calculated expression which references another field', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

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
                    // Act
                    let field: any = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    field.value = 'Johnny Bravo';
                    field.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    field.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    setTimeout(() => {
                        // Assert
                        let test2Value: any = fixture.debugElement.query(By.css('#test2')).nativeElement.value;
                        expect(test2Value).toBe('Johnny Bravo copied');
                        resolve();
                    }, 60);
                });
        });
    });

    it('should parse the placeholder for expressions', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

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
                    fixture.detectChanges();

                    // Act
                    let field: HTMLInputElement = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    expect(field.placeholder).toBe('The number 7');
                    resolve();
                });
        });
    });

    it('should set it\'s value to an empty string for expressions '
        + 'when it becomes hidden by an expression', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

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
                    fixture.detectChanges();
                    let field3: HTMLInputElement = fixture.debugElement.query(By.css('#test3')).nativeElement;
                    field3.value = 'Johnny Bravo';
                    field3.dispatchEvent(new Event('input'));
                    let changeEvent3: Event = new Event('change', { bubbles: true, cancelable: false });
                    field3.dispatchEvent(changeEvent3);
                    fixture.detectChanges();
                    let expression: Expression = new Expression(
                        "test3",
                        TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
                        'testing');
                    expect(expression.evaluate()).toBe('Johnny Bravo');

                    // Act
                    let field1: any = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    field1.value = 'hide';
                    field1.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();

                    // Assert
                    expect(expression.evaluate()).toBe('');
                    resolve();
                });
        });
    });

    it('should restore it\s old value when unhidden by an expression', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

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
                    fixture.detectChanges();
                    let field3: HTMLInputElement = fixture.debugElement.query(By.css('#test3')).nativeElement;
                    field3.value = 'Johnny Bravo';
                    field3.dispatchEvent(new Event('input'));
                    let changeEvent3: Event = new Event('change', { bubbles: true, cancelable: false });
                    field3.dispatchEvent(changeEvent3);
                    fixture.detectChanges();
                    let expression: Expression = new Expression(
                        "test3",
                        TestBed.inject<ExpressionDependencies>(ExpressionDependencies),
                        'testing');
                    expect(expression.evaluate()).toBe('Johnny Bravo');

                    // Act
                    let field1: any = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    field1.value = 'hide';
                    field1.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    field1.value = 'asdf';
                    field1.dispatchEvent(new Event('input'));
                    let changeEvent2: Event = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent2);
                    fixture.detectChanges();

                    // Assert
                    field3 = fixture.debugElement.query(By.css('#test3')).nativeElement;
                    expect(field3.value).toBe('Johnny Bravo');
                    resolve();
                });
        });
    });

    it('When a field\'s required status is changed to not required, '
        + 'it can be left blank without validation triggering', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields[0].required = true;
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].required = false;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    fixture.detectChanges();
                    let field1: HTMLInputElement = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    field1.value = '';
                    field1.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    field1 = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    expect(field1.classList.contains('ng-invalid'))
                        .toBeTruthy('Touching the field should trigger it to show as invalid');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    field1 = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    changeEvent1 = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    expect(field1.classList.contains('ng-invalid'))
                        .toBeFalsy('After removing the required param, the field should no longer show as invalid');
                    resolve();
                });
        });
    });

    it('when a field\'s required status is changed to required, it cannot be left blank', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields.pop();
        response1.form.questionSets[0].fields[0].required = false;
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].required = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    fixture.detectChanges();
                    let field1: HTMLInputElement = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    field1.value = '';
                    field1.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    field1 = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    expect(field1.classList.contains('ng-invalid'))
                        .toBeFalsy('The field should start out as valid');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    field1 = fixture.debugElement.query(By.css('#test1')).nativeElement;
                    changeEvent1 = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();
                    expect(field1.classList.contains('ng-invalid'))
                        .toBeTruthy('After adding the required param, the field should now be marked invalid');
                    resolve();
                });
        });
    });

});
