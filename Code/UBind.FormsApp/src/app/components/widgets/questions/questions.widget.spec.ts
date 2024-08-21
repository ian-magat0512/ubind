/* eslint-disable max-classes-per-file */
import { ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, DebugElement, ViewChild,
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
import formConfig from './questions-widget.test-form-config.json';
import { UnifiedFormModelService } from '@app/services/unified-form-model.service';
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
import { SectionDisplayMode } from '@app/models/section-display-mode.enum';
import { AppEventService } from '@app/services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import { WebFormComponent } from '@app/components/web-form/web-form';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationStatusService } from '@app/services/operation-status.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */
/* global jasmine */

describe('QuestionsWidget', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;
    let calculationService: CalculationService;
    let validationService: ValidationService;

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

    let appEventServiceStub: any = {
        createEvent: (): void => {},
    };

    let operationStub: any = {
        execute: (): Observable<any> => {
            return new BehaviorSubject<object>({});
        },
    };

    let windowScrollServiceStub: any = {
        scrollElementIntoView: (): void => {
            eventService = TestBed.inject<EventService>(EventService);
            eventService.scrollingFinishedSubject.next(true);
        },
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
                { provide: EncryptionService, useValue: encryptionServiceStub },
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                { provide: EvaluateService, useClass: EvaluateService },
                { provide: EventService, useClass: EventService },
                CalculationService,
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
                WorkflowStatusService,
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
        });
    });

    afterEach(() => {
        fixture.destroy();
    });

    it('should trigger a calculation when the question set has affectsPremium set and '
        + 'the question set becomes valid', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[1]['hiddenExpression'] =
                "questionSetsAreValid(['step1QuestionSet1']) != true";
        response.form.workflowConfiguration.step1.articles[0].elements[0]['affectsPremium'] = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        // make the field invalid which makes the quesiton set invalid
        validationReturnValue = { required: true };
        changeFieldValue(fixture, '#step1Field1', '');

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    let questionSet2DebugElement: DebugElement
                            = fixture.debugElement.query(By.css('questions-widget.step1QuestionSet2'));
                    expect(questionSet2DebugElement == null).toBeTruthy(
                        'step1QuestionSet2 should not be rendered yet.');

                    calculationService = TestBed.inject<CalculationService>(CalculationService);
                    spyOn(calculationService, "generateQuoteRequest");

                    // Act
                    validationReturnValue = null;
                    changeFieldValue(fixture, '#step1Field1', 'Johnny Bravo');

                    // wait 50 ms for it to be triggered
                    setTimeout(() => {
                        // Assert
                        expect(calculationService.generateQuoteRequest).toHaveBeenCalled();
                        resolve();
                    }, 50);
                });
        });
    });

    it('should trigger a calculation when the question set has affectsTriggers set and '
        + 'the question set becomes valid', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[1]['hiddenExpression'] =
                "questionSetsAreValid(['step1QuestionSet1']) != true";
        response.form.workflowConfiguration.step1.articles[0].elements[0]['affectsTriggers'] = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        // make the field invalid which makes the quesiton set invalid
        validationReturnValue = { required: true };
        changeFieldValue(fixture, '#step1Field1', '');

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    let questionSet2DebugElement: DebugElement
                            = fixture.debugElement.query(By.css('questions-widget.step1QuestionSet2'));
                    expect(questionSet2DebugElement == null).toBeTruthy(
                        'step1QuestionSet2 should not be rendered yet.');

                    calculationService = TestBed.inject<CalculationService>(CalculationService);
                    spyOn(calculationService, "generateQuoteRequest");

                    // Act
                    validationReturnValue = null;
                    changeFieldValue(fixture, '#step1Field1', 'Johnny Bravo');

                    // wait 50 ms for it to be triggered
                    setTimeout(() => {
                        // Assert
                        expect(calculationService.generateQuoteRequest).toHaveBeenCalled();
                        resolve();
                    }, 50);
                });
        });
    });

    it('should trigger a calculation when the field has affectsPremium, '
        + 'and another question set has requiredForCalculation and is valid', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[1]['hidden']
                = "questionSetsAreValid(['step1QuestionSet1']) != true";
        response.form.workflowConfiguration.step1.articles[0].elements[0]['affectsPremium'] = true;
        response.form.workflowConfiguration.step1.articles[0].elements[0]['requiredForCalculation'] = true;
        response.form.questionSets[1].fields[0].affectsPremium = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        // make the field valid which makes the quesiton set invalid
        validationReturnValue = null;
        changeFieldValue(fixture, '#step1Field1', 'Johnny Bravo');

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    calculationService = TestBed.inject<CalculationService>(CalculationService);
                    spyOn(calculationService, "generateQuoteRequest");

                    // Act
                    validationReturnValue = null;
                    changeFieldValue(fixture, '#step1Field2', 'Johnny Bravo');

                    // wait 50 ms for it to be triggered
                    setTimeout(() => {
                        // Assert
                        expect(calculationService.generateQuoteRequest).toHaveBeenCalled();
                        resolve();
                    }, 50);
                });
        });
    });

    it('should not trigger a calculation when the field has affectsPremium, '
        + 'but another question set has requiredForCalculation and is not valid', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[0].elements[0]['affectsPremium'] = true;
        response.form.workflowConfiguration.step1.articles[0].elements[0]['requiredForCalculation'] = true;
        response.form.questionSets[1].fields[0].affectsPremium = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        // make the field invalid which makes the question set invalid
        validationReturnValue = { required: true };
        changeFieldValue(fixture, '#step1Field1', '');

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    calculationService = TestBed.inject<CalculationService>(CalculationService);
                    spyOn<any>(calculationService, "triggerCalculation");

                    // Act
                    validationReturnValue = null;
                    changeFieldValue(fixture, '#step1Field2', 'Johnny Bravo');

                    // wait 50 ms for it to be triggered
                    setTimeout(() => {
                        // Assert
                        expect(<any>calculationService['triggerCalculation']).not.toHaveBeenCalled();
                        resolve();
                    }, 50);
                });
        });
    });

    it('should trigger a calculation when the field has affectsTriggers, '
        + 'and another question set has requiredForCalculation and is valid', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[1]['hidden']
                = "questionSetsAreValid(['step1QuestionSet1']) != true";
        response.form.workflowConfiguration.step1.articles[0].elements[0]['requiredForCalculation'] = true;
        response.form.questionSets[1].fields[0].affectsTriggers = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        // make the field valid which makes the quesiton set invalid
        validationReturnValue = null;
        changeFieldValue(fixture, '#step1Field1', 'Johnny Bravo');

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    calculationService = TestBed.inject<CalculationService>(CalculationService);
                    spyOn(calculationService, "generateQuoteRequest");

                    // Act
                    validationReturnValue = null;
                    changeFieldValue(fixture, '#step1Field2', 'Johnny Bravo');

                    // wait 50 ms for it to be triggered
                    setTimeout(() => {
                        // Assert
                        expect(calculationService.generateQuoteRequest).toHaveBeenCalled();
                        resolve();
                    }, 50);
                });
        });
    });

    it('should not trigger a calculation when the field has affectsTriggers, '
        + 'but another question set has requiredForCalculation and is not valid', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[0].elements[0]['requiredForCalculation'] = true;
        response.form.questionSets[1].fields[0].affectsTriggers = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        // make the field invalid which makes the question set invalid
        validationReturnValue = { required: true };
        changeFieldValue(fixture, '#step1Field1', '');

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {

                    calculationService = TestBed.inject<CalculationService>(CalculationService);
                    spyOn<any>(calculationService, "triggerCalculation");

                    // Act
                    validationReturnValue = null;
                    changeFieldValue(fixture, '#step1Field2', 'Johnny Bravo');

                    // wait 50 ms for it to be triggered
                    setTimeout(() => {
                        // Assert
                        expect(<any>calculationService['triggerCalculation']).not.toHaveBeenCalled();
                        resolve();
                    }, 50);
                });
        });
    });

    it('should remove values from the strict form model when a question set is hidden', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);

        // disable the "required" validation since it causes 
        // ExpressionChangedAfterItWasChecked error due to ng-valid changing.
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    // Set a value in the second question set
                    changeFieldValue(fixture, '#step1Field2', 'Johnny Bravo');
                    fixture.detectChanges();

                    // Get it to save the form data into the unified form model
                    let formService: any = TestBed.inject<FormService>(FormService);
                    formService.getValues();

                    // Check that it saved it to the unified form model
                    let unifiedFormModelService: any = TestBed.inject<UnifiedFormModelService>(UnifiedFormModelService);
                    expect(unifiedFormModelService.strictFormModel.model['step1Field2']).toBe('Johnny Bravo');

                    // Act
                    // change the field value to hide which hides the second question set
                    changeFieldValue(fixture, '#step1Field1', 'hide1');
                    fixture.detectChanges();

                    // Assert
                    // Check that it removed it from the unified form model by setting it to an empty string
                    formService.getValues();
                    expect(unifiedFormModelService.strictFormModel.model['step1Field2']).toBe('');
                    resolve();
                });
        });
    });

    it('should remove values from the strict form model for a question set when it\'s parent article is hidden',
        async () => {
            // Arrange
            fixture = TestBed.createComponent(TestHostComponent);
            sut = fixture.componentInstance;
            eventService = TestBed.inject<EventService>(EventService);
            validationService = TestBed.inject<ValidationService>(ValidationService);

            // disable the "required" validation since it causes 
            // ExpressionChangedAfterItWasChecked error due to ng-valid changing.
            let validationReturnValue: any = null;
            spyOn(validationService, "required").and.returnValue((control: any) => {
                return validationReturnValue;
            });
            let response: any = _.cloneDeep(formConfig);
            response['status'] = 'success';
            let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
            let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
            workflowService.skipWorkflowAnimations = true;
            workflowService.initialise();
            sut.ready = true;
            fixture.detectChanges();

            return new Promise((resolve: any, reject: any): void => {
                eventService.webFormLoadedSubject
                    .pipe(filter((loaded: boolean) => loaded == true))
                    .subscribe(async () => {
                        // Set a value in the second question set
                        changeFieldValue(fixture, '#step1Field2', 'Johnny Bravo');
                        fixture.detectChanges();

                        // Get it to save the form data into the unified form model
                        let formService: any = TestBed.inject<FormService>(FormService);
                        formService.getValues();

                        // Check that it saved it to the unified form model
                        let unifiedFormModelService: UnifiedFormModelService =
                            TestBed.inject<UnifiedFormModelService>(UnifiedFormModelService);
                        expect(unifiedFormModelService.strictFormModel.model['step1Field2']).toBe('Johnny Bravo');

                        // Act
                        // change the field value to hide which hides the second question set
                        changeFieldValue(fixture, '#step1Field1', 'hide2');
                        fixture.detectChanges();

                        // Assert
                        // Check that it removed it from the unified form model by setting it to an empty string
                        formService.getValues();
                        expect(unifiedFormModelService.strictFormModel.model['step1Field2']).toBe('');
                        resolve();
                    });
            });
        });

    // eslint-disable-next-line prefer-arrow/prefer-arrow-functions
    function changeFieldValue(fixture: any, cssSelector: string, value: string): void {
        let field: any = fixture.debugElement.query(By.css(cssSelector)).nativeElement;
        field.value = value;
        field.dispatchEvent(new Event('input'));
        let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
        field.dispatchEvent(changeEvent);
        fixture.detectChanges();
    }

    // eslint-disable-next-line prefer-arrow/prefer-arrow-functions
    function isElementVisible(el: HTMLElement): boolean {
        return !!el.offsetParent;
    }

    it('when an article element of type "questions" has affectsPremium and affectsTriggers '
        + 'set to false it stops triggering calculations', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let generateQuoteRequestSpy: jasmine.Spy = spyOn(calculationService, "generateQuoteRequest");
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles[0].elements[0].affectsPremium = true;
        response1.form.workflowConfiguration.step1.articles[0].elements[0].affectsTriggers = true;
        let response2: any = _.cloneDeep(formConfig);
        response2['status'] = 'success';

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#step1Field1'));
                    expect(debugElement != null).toBeTruthy('step1Field1 should have been rendered');
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = 'Johnny Bravo';
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    // wait 60ms for the quote request to trigger
                    setTimeout(() => {
                        expect(generateQuoteRequestSpy).toHaveBeenCalled();
                        generateQuoteRequestSpy.calls.reset();

                        // Act
                        configProcessorService.onConfigurationResponse(response2);
                        fixture.detectChanges();
                        debugElement = fixture.debugElement.query(By.css('#step1Field1'));
                        fieldEl = debugElement.nativeElement;
                        fieldEl.value = 'Sally Thunder';
                        fieldEl.dispatchEvent(new Event('input'));
                        changeEvent = new Event('change', { bubbles: true, cancelable: false });
                        fieldEl.dispatchEvent(changeEvent);
                        fixture.detectChanges();

                        // Assert
                        // wait 60ms for the quote request to trigger
                        debugElement = fixture.debugElement.query(By.css('#step1Field1'));
                        fieldEl = debugElement.nativeElement;
                        expect(fieldEl.value).toBe('Sally Thunder');
                        expect(generateQuoteRequestSpy).not.toHaveBeenCalled();
                        resolve();
                    }, 60);
                });
        });
    });

    it('when an article element of type "questions" has affectsPremium and affectsTriggers '
        + 'set to true it starts triggering calculations', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let generateQuoteRequestSpy: jasmine.Spy = spyOn(calculationService, "generateQuoteRequest");
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles[0].elements[0].affectsPremium = true;
        response1.form.workflowConfiguration.step1.articles[0].elements[0].affectsTriggers = true;
        let response2: any = _.cloneDeep(formConfig);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.articles[0].elements[0].affectsPremium = true;
        response2.form.workflowConfiguration.step1.articles[0].elements[0].affectsTriggers = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(By.css('#step1Field1'));
                    expect(debugElement != null).toBeTruthy('step1Field1 should have been rendered');
                    let fieldEl: HTMLInputElement = debugElement.nativeElement;
                    fieldEl.value = 'Johnny Bravo';
                    fieldEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();


                    // wait 60ms for the quote request to trigger
                    setTimeout(() => {
                        expect(generateQuoteRequestSpy).toHaveBeenCalled();
                        generateQuoteRequestSpy.calls.reset();
                        // Act
                        configProcessorService.onConfigurationResponse(response2);
                        fixture.detectChanges();
                        debugElement = fixture.debugElement.query(By.css('#step1Field1'));
                        fieldEl = debugElement.nativeElement;
                        fieldEl.value = 'Sally Thunder';
                        fieldEl.dispatchEvent(new Event('input'));
                        changeEvent = new Event('change', { bubbles: true, cancelable: false });
                        fieldEl.dispatchEvent(changeEvent);
                        fixture.detectChanges();

                        // Assert
                        // wait 60ms for the quote request to trigger
                        setTimeout(() => {
                            debugElement = fixture.debugElement.query(By.css('#step1Field1'));
                            fieldEl = debugElement.nativeElement;
                            expect(fieldEl.value).toBe('Sally Thunder');
                            expect(generateQuoteRequestSpy).toHaveBeenCalled();
                            resolve();
                        },60);

                    }, 60);
                });
        });
    });

    it('when an article element has requiredForCalculation set, clearing a required field and changing another '
        + 'field in another question set will no longer trigger the calculation', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let performCalculationSpy: jasmine.Spy = spyOn<any>(calculationService, "triggerCalculation");

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].required = true;
        response1.form.workflowConfiguration.step1.articles[0].elements[0].affectsPremium = true;
        response1.form.workflowConfiguration.step1.articles[0].elements[0].affectsTriggers = true;
        response1.form.workflowConfiguration.step1.articles[1].elements[0].affectsPremium = true;
        response1.form.workflowConfiguration.step1.articles[1].elements[0].affectsTriggers = true;
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.articles[0].elements[0].requiredForCalculation = true;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement1: DebugElement = fixture.debugElement.query(By.css('#step1Field1'));
                    expect(debugElement1 != null).toBeTruthy('step1Field1 should have been rendered');
                    let fieldEl1: HTMLInputElement = debugElement1.nativeElement;
                    fieldEl1.value = 'Johnny Bravo';
                    fieldEl1.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl1.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    // wait 60ms for the quote request to trigger
                    setTimeout(() => {
                        expect(performCalculationSpy).toHaveBeenCalled();
                        performCalculationSpy.calls.reset();

                        let debugElement2: DebugElement = fixture.debugElement.query(By.css('#step1Field2'));
                        expect(debugElement2 != null).toBeTruthy('step1Field2 should have been rendered');
                        let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                        fieldEl2.value = 'Johnny Bravo';
                        fieldEl2.dispatchEvent(new Event('input'));
                        let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                        fieldEl2.dispatchEvent(changeEvent);
                        fixture.detectChanges();
                        // wait 60ms for the quote request to trigger
                        setTimeout(() => {
                            // both fields were valid so it should have now been called
                            expect(performCalculationSpy).toHaveBeenCalled();
                            performCalculationSpy.calls.reset();

                            // Act
                            configProcessorService.onConfigurationResponse(response2);
                            fixture.detectChanges();
                            debugElement1 = fixture.debugElement.query(By.css('#step1Field1'));
                            fieldEl1 = debugElement1.nativeElement;
                            fieldEl1.value = '';
                            fieldEl1.dispatchEvent(new Event('input'));
                            changeEvent = new Event('change', { bubbles: true, cancelable: false });
                            fieldEl1.dispatchEvent(changeEvent);
                            fixture.detectChanges();

                            // Assert
                            // wait 60ms for the quote request to trigger
                            setTimeout(() => {
                                debugElement1 = fixture.debugElement.query(By.css('#step1Field1'));
                                fieldEl1 = debugElement1.nativeElement;
                                expect(fieldEl1.value).toBe('');
                                expect(performCalculationSpy).not.toHaveBeenCalled();
                                resolve();
                            }, 60);
                        }, 60);
                    }, 60);
                });
        });
    });

    it('when an article element has requiredForCalculation removed, clearing a required field and changing another '
        + 'field in another question set starts triggering the calculation', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        let performCalculationSpy: jasmine.Spy = spyOn<any>(calculationService, "triggerCalculation");

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].required = true;
        response1.form.workflowConfiguration.step1.articles[0].elements[0].affectsPremium = true;
        response1.form.workflowConfiguration.step1.articles[0].elements[0].affectsTriggers = true;
        response1.form.workflowConfiguration.step1.articles[0].elements[0].requiredForCalculation = true;
        response1.form.workflowConfiguration.step1.articles[1].elements[0].affectsPremium = true;
        response1.form.workflowConfiguration.step1.articles[1].elements[0].affectsTriggers = true;
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.articles[0].elements[0].requiredForCalculation = false;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement1: DebugElement = fixture.debugElement.query(By.css('#step1Field1b'));
                    expect(debugElement1 != null).toBeTruthy('step1Field1 should have been rendered');
                    let fieldEl1: HTMLInputElement = debugElement1.nativeElement;
                    fieldEl1.value = 'Johnny Bravo';
                    fieldEl1.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl1.dispatchEvent(changeEvent);

                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#step1Field2'));
                    expect(debugElement2 != null).toBeTruthy('step1Field2 should have been rendered');
                    let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                    fieldEl2.value = 'Johnny Bravo';
                    fieldEl2.dispatchEvent(new Event('input'));
                    changeEvent = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl2.dispatchEvent(changeEvent);

                    fixture.detectChanges();

                    // wait 60ms for the quote request to trigger
                    setTimeout(() => {
                        // step1Field1's question set "step1QuestionSet1" has requiredForCalculations=true
                        // and no value was entered was entered for step1Field1, so it's invalid.
                        // Thus no calculation should have been performed here.
                        expect(performCalculationSpy).not.toHaveBeenCalled();
                        performCalculationSpy.calls.reset();

                        // Act
                        configProcessorService.onConfigurationResponse(response2);
                        fixture.detectChanges();

                        let debugElement2: DebugElement = fixture.debugElement.query(By.css('#step1Field2'));
                        expect(debugElement2 != null).toBeTruthy('step1Field2 should have been rendered');
                        let fieldEl2: HTMLInputElement = debugElement2.nativeElement;
                        fieldEl2.value = 'Johnny Bravo Updated';
                        fieldEl2.dispatchEvent(new Event('input'));
                        let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                        fieldEl2.dispatchEvent(changeEvent);
                        fixture.detectChanges();

                        // wait 60ms for the quote request to trigger
                        setTimeout(() => {
                            // Assert
                            debugElement2 = fixture.debugElement.query(By.css('#step1Field2'));
                            fieldEl2 = debugElement2.nativeElement;
                            expect(fieldEl2.value).toBe('Johnny Bravo Updated');

                            // step1Field1 is no longer required for calculations and question set "step1QuestionSet1"
                            // is no longer required to be valid for calculations to proceed.
                            // Therefore when step1Field2 was updated, the calculation should have proceeded.
                            expect(performCalculationSpy).toHaveBeenCalled();
                            resolve();
                        }, 60);
                    }, 60);
                });
        });
    });

    it('when displayMode is articleElement and reveal groups are used, navigating back shows the previous revealed '
        + 'groups', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let expressionMethodService: ExpressionMethodService
            = TestBed.inject<ExpressionMethodService>(ExpressionMethodService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.questionSets[0].fields[0].startsNewRevealGroup = true;
        response.form.questionSets[0].fields[0].required = true;
        response.form.questionSets[0].fields[1].startsNewRevealGroup = true;
        response.form.questionSets[0].fields[1].required = true;
        response.form.questionSets[1].fields[0].startsNewRevealGroup = true;
        response.form.questionSets[1].fields[0].required = true;
        response.form.workflowConfiguration.step1.displayMode = SectionDisplayMode.Article;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        let workflowService: WorkflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.skipWorkflowAnimations = true;
        workflowService.initialise();
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement1: DebugElement = fixture.debugElement.query(By.css('#step1Field1'));
                    expect(isElementVisible(debugElement1.nativeElement)).toBeTruthy('step1Field1 should be visible');
                    let debugElement2: DebugElement = fixture.debugElement.query(By.css('#step1Field1b'));
                    expect(isElementVisible(debugElement2.nativeElement))
                        .toBeFalsy('step1Field1b should not have be visible');
                    changeFieldValue(fixture, '#step1Field1', 'a');
                    debugElement2 = fixture.debugElement.query(By.css('#step1Field1b'));
                    expect(isElementVisible(debugElement2.nativeElement))
                        .toBeTruthy('step1Field1b should have been rendered');
                    changeFieldValue(fixture, '#step1Field1b', 'b');

                    // navigate to the next article element
                    let destination: WorkflowDestination = {
                        stepName: 'step1',
                        articleIndex: expressionMethodService.getNextArticleIndex(),
                    };
                    workflowService.navigateTo(destination);
                    fixture.detectChanges();
                    setTimeout(() => {
                        fixture.detectChanges();
                        let debugElement3: DebugElement = fixture.debugElement.query(By.css('#step1Field2'));
                        expect(isElementVisible(debugElement3.nativeElement))
                            .toBeTruthy('step1Field2 should have been rendered');

                        // navigate back to the previous article element
                        destination = {
                            stepName: 'step1',
                            articleIndex: expressionMethodService.getPreviousArticleIndex(),
                        };
                        workflowService.navigateTo(destination);
                        fixture.detectChanges();
                        setTimeout(() => {
                            expect(workflowService.currentDestination.articleIndex).toBe(0);
                            fixture.detectChanges();
                            debugElement1 = fixture.debugElement.query(By.css('#step1Field1'));
                            expect(debugElement1 != null).toBeTruthy('step1Field1 should have been rendered');
                            debugElement2 = fixture.debugElement.query(By.css('#step1Field1b'));
                            expect(debugElement2 != null).toBeTruthy('step1Field1b should have been rendered');
                            resolve();
                        }, 50);
                    }, 50);
                });
        });
    });
});
