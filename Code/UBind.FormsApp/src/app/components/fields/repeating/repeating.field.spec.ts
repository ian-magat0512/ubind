import { ChangeDetectionStrategy, Component, DebugElement, EventEmitter, CUSTOM_ELEMENTS_SCHEMA,
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
import formConfig from './repeating.test-form-config.json';
import { ExpressionInputSubjectService } from '@app/expressions/expression-input-subject.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from '@app/services/operation-status.service';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */

describe('RepeatingField', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;
    let applicationService: ApplicationService;

    let workflowServiceStub: any = {
        navigate: new EventEmitter<any>(),
        currentDestination: { stepName: "purchaseQuote" },
        currentNavigation: new WorkflowNavigation(null, { stepName: "purchaseQuote" }),
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
                CreditCardNumberPipe,
                CurrencyPipe,
                CssIdentifierPipe,
                TimePipe,
                PhoneNumberPipe,
                NumberPlatePipe,
                ExpressionDependencies,
                WorkflowStatusService,
                ExpressionInputSubjectService,
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
        });
    });

    afterEach(() => {
        fixture.destroy();
    });

    it('should update the field values of repeating question fields when repeating question instances are re-ordered '
        + 'due to a removal so that fieldPaths update', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.questionSets[0].fields[0].maximumQuantityExpression = 10;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    let field1: any = fixture.debugElement.query(By.css('#testRepeating0-firstName')).nativeElement;
                    field1.value = 'John';
                    field1.dispatchEvent(new Event('input'));
                    let changeEvent1: Event = new Event('change', { bubbles: true, cancelable: false });
                    field1.dispatchEvent(changeEvent1);
                    fixture.detectChanges();

                    let addButtonDebugElement: DebugElement
                    = fixture.debugElement.query(By.css('#testRepeating-add-item-button'));
                    expect(addButtonDebugElement).not.toBeNull('The add button should have been rendered.');
                    addButtonDebugElement.triggerEventHandler('click', null);
                    fixture.detectChanges();
                    // wait for the animation
                    setTimeout(async () => {
                        fixture.detectChanges();

                        let removeButtonDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('#testRepeating1-remove-item-button'));
                        expect(removeButtonDebugElement).not.toBeNull('The remove button should have been rendered.');

                        expressionInputSubjectService.getFieldValueSubject('testRepeating[1].firstName', '');
                        let result: string = '';
                        expressionInputSubjectService.getFieldValueObservable('testRepeating[1].firstName')
                            .subscribe((value: any) => result = value);

                        let field2: HTMLInputElement
                            = fixture.debugElement.query(By.css('#testRepeating1-firstName')).nativeElement;
                        field2.value = 'Mary';
                        field2.dispatchEvent(new Event('input'));
                        let changeEvent2: Event = new Event('change', { bubbles: true, cancelable: false });
                        field2.dispatchEvent(changeEvent2);
                        fixture.detectChanges();
                        expect(result).toBe('Mary');

                        addButtonDebugElement.triggerEventHandler('click', null);
                        fixture.detectChanges();
                        // wait for the animation
                        setTimeout(async () => {
                            fixture.detectChanges();

                            let field3: HTMLInputElement
                            = fixture.debugElement.query(By.css('#testRepeating2-firstName')).nativeElement;
                            field3.value = 'Zoe';
                            field3.dispatchEvent(new Event('input'));
                            let changeEvent3: Event = new Event('change', { bubbles: true, cancelable: false });
                            field3.dispatchEvent(changeEvent3);
                            fixture.detectChanges();

                            // Act
                            removeButtonDebugElement
                            = fixture.debugElement.query(By.css('#testRepeating1-remove-item-button'));
                            removeButtonDebugElement.triggerEventHandler('click', null);
                            fixture.detectChanges();
                            // wait for the animation out of the item
                            setTimeout(async () => {
                                fixture.detectChanges();
                                // wait for the query list change to notify
                                setTimeout(async () => {
                                    fixture.detectChanges();
                                    expect(result).toBe('Zoe');
                                    resolve();
                                }, 10);
                            }, 10);
                        }, 10);
                    }, 10);
                });
        });
    });

    it('when a repeating questions min qty is increased, more repeating instances are shown '
        + 'to meet the minimum', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].maximumQuantityExpression = 10;
        response1.form.questionSets[0].fields[0].minimumQuantityExpression = 1;
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].minimumQuantityExpression = 2;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    let field1DebugElement: DebugElement
                    = fixture.debugElement.query(By.css('#testRepeating0-firstName'));
                    expect(field1DebugElement != null).toBeTruthy('There should be an initial repeating instance.');
                    let field2DebugElement: DebugElement
                    = fixture.debugElement.query(By.css('#testRepeating1-firstName'));
                    expect(field2DebugElement == null)
                        .toBeTruthy('There should not be a 2nd initial repeating instance.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    field2DebugElement = fixture.debugElement.query(By.css('#testRepeating1-firstName'));
                    expect(field2DebugElement != null)
                        .toBeTruthy('The second repeating instance should have been added.');

                    resolve();
                });
        });
    });

    it('when a repeating questions max qty is decreased, repeating instances are removed to not exceed the maxium'
        + 'to meet the minimum', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].maximumQuantityExpression = 10;
        response1.form.questionSets[0].fields[0].minimumQuantityExpression = 2;
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].minimumQuantityExpression = 1;
        response2.form.questionSets[0].fields[0].maximumQuantityExpression = 1;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    let field2DebugElement: DebugElement
                    = fixture.debugElement.query(By.css('#testRepeating1-firstName'));
                    expect(field2DebugElement != null).toBeTruthy('There should be two initial repeating instances');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    field2DebugElement = fixture.debugElement.query(By.css('#testRepeating1-firstName'));
                    expect(field2DebugElement == null)
                        .toBeTruthy('The second repeating instance should have been removed.');

                    resolve();
                });
        });
    });

    it('When the text or icon for a repeating field add or remove button is updated in textElements, '
        + 'it updates', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].maximumQuantityExpression = 10;
        let response2: any = _.cloneDeep(response1);
        response2.form.questionSets[0].fields[0].addRepeatingInstanceButtonLabel = 'Add another dude';
        response2.form.textElements.push({
            "category": "Form Elements",
            "name": "Repeating Add Item Button",
            "text": "Add another Driver",
            "icon": "fas fa-city",
        });
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        applicationService.currentWorkflowDestination = { stepName: 'purchaseDetails' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('#testRepeating-add-item-button'));
                    let buttonEl: HTMLButtonElement = debugElement.nativeElement;
                    expect(buttonEl.innerText).toBe('Add another Test Repeating');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('#testRepeating-add-item-button'));
                    buttonEl = debugElement.nativeElement;
                    expect(buttonEl.innerText.trim()).toBe('Add another dude');
                    let buttonDebugElement: DebugElement
                    = fixture.debugElement.query(By.css('#testRepeating-add-item-button .btn-icon > span'));
                    expect(buttonDebugElement != null).toBeTruthy('The icon should have been rendered.');
                    let spanEl: HTMLSpanElement = buttonDebugElement.nativeElement;
                    expect(spanEl.classList.contains('fa-city'));

                    resolve();
                });
        });
    });

});
