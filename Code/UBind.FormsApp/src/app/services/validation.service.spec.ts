import { ValidationService } from './validation.service';
import { FormControl } from '@angular/forms';
import { ExpressionMethodService } from '../expressions/expression-method.service';
import { WorkflowStatusService } from './workflow-status.service';
import { SingleLineTextField } from '@app/components/fields/single-line-text/single-line-text.field';
import { FormService } from './form.service';
import { ConfigService } from './config.service';
import { EventEmitter, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { EventService } from './event.service';
import { ConfigProcessorService } from './config-processor.service';
import { sharedConfig } from '@app/app.module.shared';
import { MessageService } from './message.service';
import { ConfigurationOperation } from '@app/operations/configuration.operation';
import { EvaluateService } from './evaluate.service';
import { CalculationService } from './calculation.service';
import { WorkflowService } from './workflow.service';
import { AttachmentService } from './attachment.service';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { ApplicationService } from './application.service';
import { AlertService } from './alert.service';
import { WindowScrollService } from './window-scroll.service';
import { BroadcastService } from './broadcast.service';
import { CssProcessorService } from './css-processor.service';
import { OperationFactory } from '../operations/operation.factory';
import { WebhookService } from './webhook.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { PolicyOperation } from '@app/operations/policy.operation';
import { UserService } from './user.service';
import { ResumeApplicationService } from './resume-application.service';
import { AbnPipe } from '@app/pipes/abn.pipe';
import { BsbPipe } from '@app/pipes/bsb.pipe';
import { CreditCardNumberPipe } from '@app/pipes/credit-card-number.pipe';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { TimePipe } from '@app/pipes/time.pipe';
import { PhoneNumberPipe } from '@app/pipes/phone-number.pipe';
import { NumberPlatePipe } from '@app/pipes/number-plate.pipe';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NgSelectModule } from '@ng-select/ng-select';
import { QuestionsWidget } from '@app/components/widgets/questions/questions.widget';
import { FieldMetadataService } from './field-metadata.service';
import { ToolTipService } from './tooltip.service';
import { FakeToolTipService } from './fakes/fake-tooltip.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from './notification.service';
import { ConfigurationV2Processor } from './configuration-v2-processor';
import { EncryptionService } from './encryption.service';
import { FieldEventLogRegistry } from './debug/field-event-log-registry';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from './operation-status.service';
import { OperationInstructionService } from './operation-instruction.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from './api.service';
import { LoggerService } from './logger.service';

/* global spyOn */

describe('ValidationService', () => {
    let validationService: ValidationService;
    let formService: FormService;
    let workflowService: WorkflowService;
    let expressionDependencies: ExpressionDependencies;
    let calculationService: CalculationService;
    let applicationService: ApplicationService;
    let fieldMetadataService: FieldMetadataService;
    let eventService: EventService;
    let fieldEventLogRegistry: FieldEventLogRegistry;
    let maskPipe: MaskPipe;


    let workflowServiceStub: any = {
        navigate: new EventEmitter<any>(),
        currentDestination: { stepName: "purchaseQuote" },
        initialised: new BehaviorSubject<boolean>(true),
        actionAborted: new EventEmitter<any>(),
        actionCompleted: new EventEmitter<any>(),
        completedNavigationIn: (): void => { },
        quoteLoadedSubject: new Subject<boolean>(),
        loadedCustomerHasUserSubject: new Subject<boolean>(),
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

    beforeEach(async () => {
        return TestBed.configureTestingModule({
            declarations: [
                ...sharedConfig.declarations,
            ],
            providers: [
                { provide: EncryptionService, useValue: encryptionServiceStub },
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                EvaluateService,
                { provide: EventService, useClass: EventService },
                { provide: CalculationService, useClass: CalculationService },
                { provide: WorkflowService, useValue: workflowServiceStub },
                { provide: ConfigService, useClass: ConfigService },
                { provide: FormService, useClass: FormService },
                { provide: AttachmentService, useClass: AttachmentService },
                { provide: CalculationOperation, useValue: {} },
                ApplicationService,
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
                TimePipe,
                PhoneNumberPipe,
                NumberPlatePipe,
                ExpressionDependencies,
                WorkflowStatusService,
                FieldMetadataService,
                NotificationService,
                ConfigurationV2Processor,
                FieldEventLogRegistry,
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

            validationService = TestBed.inject<ValidationService>(ValidationService);
            formService = TestBed.inject<FormService>(FormService);
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            expressionDependencies = TestBed.inject<ExpressionDependencies>(ExpressionDependencies);
            calculationService = TestBed.inject<CalculationService>(CalculationService);
            applicationService = TestBed.inject<ApplicationService>(ApplicationService);
            fieldMetadataService = TestBed.inject<FieldMetadataService>(FieldMetadataService);
            eventService = TestBed.inject<EventService>(EventService);
            fieldEventLogRegistry = TestBed.inject<FieldEventLogRegistry>(FieldEventLogRegistry);
            maskPipe = TestBed.inject<MaskPipe>(MaskPipe);
        });
    });

    it('should validate numbers', () => {
        // check actual number
        const fc: FormControl = new FormControl('9999', validationService.isNumber());
        fc['field'] = new SingleLineTextField(formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        fc['formControl'] = fc;

        expect(fc.valid).toBeTruthy();

        fc.setValue('9.9');
        expect(fc.valid).toBeTruthy();

        // check incorrect numbers
        fc.setValue(' 99');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('99 ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue(' 99 ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('9 9');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('9,9');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('  ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue(' ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('asdf');
        expect(fc.invalid).toBeTruthy();
    });

    it('should validate names', () => {
        let validationService: any = TestBed.inject<ValidationService>(ValidationService);
        let formService: any = TestBed.inject<FormService>(FormService);
        // check valid names
        const fc: FormControl = new FormControl(
            'Valid Name',
            validationService.isName());
        fc['field'] = new SingleLineTextField(formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        fc['formControl'] = fc;

        expect(fc.valid).toBeTruthy();

        fc.setValue('Valid Name Too');
        expect(fc.valid).toBeTruthy();

        fc.setValue('al so');
        expect(fc.valid).toBeTruthy();

        fc.setValue('ValidNameAsWell');
        expect(fc.valid).toBeTruthy();

        // check invalid names
        fc.setValue(' Invalid Name');
        expect(fc.invalid).toBeFalsy();

        fc.setValue('Invalid Name ');
        expect(fc.invalid).toBeFalsy();

        fc.setValue(' Invalid Name ');
        expect(fc.invalid).toBeFalsy();

        fc.setValue(' ');
        expect(fc.invalid).toBeFalsy();

        fc.setValue('  ');
        expect(fc.invalid).toBeFalsy();

        fc.setValue('  ');
        expect(fc.invalid).toBeFalsy();
    });

    it('should validate full names', () => {
        let validationService: any = TestBed.inject<ValidationService>(ValidationService);
        let formService: any = TestBed.inject<FormService>(FormService);
        // check valid full names
        const fc: FormControl = new FormControl(
            'Valid Full Name',
            validationService.isFullName());
        fc['field'] = new SingleLineTextField(formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        fc['formControl'] = fc;

        expect(fc.valid).toBeTruthy();

        fc.setValue('Valid Full Name Too');
        expect(fc.valid).toBeTruthy();

        fc.setValue('al so');
        expect(fc.valid).toBeTruthy();

        fc.setValue('');
        expect(fc.valid).toBeTruthy();

        // check invalid full names
        fc.setValue('InvalidFullName');
        expect(fc.invalid).toBeTruthy();

        fc.setValue(' Invalid Full Name');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('Invalid Full Name ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue(' Invalid Full Name ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue(' ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('  ');
        expect(fc.invalid).toBeTruthy();
    });

    it('should validate required fields', () => {
        let validationService: any = TestBed.inject<ValidationService>(ValidationService);
        let formService: any = TestBed.inject<FormService>(FormService);
        // required: true, hidden: false
        let fc: FormControl = new FormControl(
            'required field has value',
            validationService.required(true, false));
        let field: SingleLineTextField = new SingleLineTextField(
            formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        field.parentQuestionsWidget = <QuestionsWidget><any>{
            fieldPath: '',
            isHidden: (): boolean => {
                return false;
            },
        };
        field['templateOptions'] = {};
        Object.defineProperty(field, 'to', { value: {} });
        Object.defineProperty(field, 'key', { value: 'testField' });
        fc['field'] = field;
        fc['formControl'] = fc;

        expect(fc.valid).toBeTruthy();

        // check invalid required value
        fc.setValue('');
        expect(fc.invalid).toBeTruthy();

        fc.setValue(' ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('  ');
        expect(fc.invalid).toBeTruthy();

        // Test for numeric/currency data types
        fc.setValue(1);
        expect(fc.valid).toBeTruthy();

        fc.setValue(0);
        expect(fc.valid).toBeTruthy();

        // required: true, hidden: true
        fc = new FormControl('required field has value', validationService.required(true, true));
        field = new SingleLineTextField(
            formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        field.parentQuestionsWidget = <QuestionsWidget><any>{
            fieldPath: '',
            isHidden: (): boolean => {
                return false;
            },
        };
        field.hidden = true;
        field['templateOptions'] = {};
        Object.defineProperty(field, 'to', { value: {} });
        Object.defineProperty(field, 'key', { value: 'testField' });
        fc['field'] = field;
        fc['formControl'] = fc;
        expect(fc.valid).toBeTruthy();

        // check invalid required value
        fc.setValue('');
        expect(fc.valid).toBeTruthy();

        fc.setValue(' ');
        expect(fc.valid).toBeTruthy();

        fc.setValue('  ');
        expect(fc.valid).toBeTruthy();

        // Test for numeric/currency data types
        fc.setValue(1);
        expect(fc.valid).toBeTruthy();

        fc.setValue(0);
        expect(fc.valid).toBeTruthy();


        // required: false, hidden: false
        fc = new FormControl('', validationService.required(false, false));
        field = new SingleLineTextField(
            formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        field.parentQuestionsWidget = <QuestionsWidget><any>{
            fieldPath: '',
            isHidden: (): boolean => {
                return false;
            },
        };
        field['templateOptions'] = {};
        Object.defineProperty(field, 'to', { value: {} });
        Object.defineProperty(field, 'key', { value: 'testField' });
        fc['field'] = field;
        fc['formControl'] = fc;
        expect(fc.valid).toBeTruthy();

        fc.setValue(' ');
        expect(fc.valid).toBeTruthy();

        fc.setValue('  ');
        expect(fc.valid).toBeTruthy();

        // Test for numeric/currency data types
        fc.setValue(1);
        expect(fc.valid).toBeTruthy();

        fc.setValue(0);
        expect(fc.valid).toBeTruthy();

        // required: false, hidden: true
        fc = new FormControl('required field has value', validationService.required(false, true));
        field = new SingleLineTextField(
            formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        field.parentQuestionsWidget = <QuestionsWidget><any>{
            fieldPath: '',
            isHidden: (): boolean => {
                return false;
            },
        };
        field.hidden = true;
        field['templateOptions'] = {};
        Object.defineProperty(field, 'to', { value: {} });
        Object.defineProperty(field, 'key', { value: 'testField' });
        fc['field'] = field;
        fc['formControl'] = fc;
        expect(fc.valid).toBeTruthy();

        fc.setValue('');
        expect(fc.valid).toBeTruthy();

        fc.setValue(' ');
        expect(fc.valid).toBeTruthy();

        fc.setValue('  ');
        expect(fc.valid).toBeTruthy();

        // Test for numeric/currency data types
        fc.setValue(1);
        expect(fc.valid).toBeTruthy();

        fc.setValue(0);
        expect(fc.valid).toBeTruthy();
    });

    it('should validate element tags', () => {
        let validationService: any = TestBed.inject<ValidationService>(ValidationService);
        let formService: any = TestBed.inject<FormService>(FormService);
        const fc: FormControl = new FormControl('<svg url="">', validationService.elementTagValidator);
        fc['field'] = new SingleLineTextField(
            formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        fc['formControl'] = fc;
        expect(fc.invalid).toBeTruthy();
    });

    it('should validate plate number', () => {
        const fc: FormControl = new FormControl('123', validationService.isNumberPlate());
        fc['field'] = new SingleLineTextField(formService,
            workflowService,
            validationService,
            expressionDependencies,
            calculationService,
            applicationService,
            fieldMetadataService,
            eventService,
            fieldEventLogRegistry,
            maskPipe);
        fc['formControl'] = fc;

        expect(fc.valid).toBeTruthy();

        fc.setValue('ABCD123ef');
        expect(fc.valid).toBeTruthy();

        fc.setValue('ABC-D45');
        expect(fc.valid).toBeTruthy();

        // check incorrect numbers
        fc.setValue('ABCD5244ZZ');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('AUb-45z');
        expect(fc.invalid).toBeTruthy();

        fc.setValue('DK-00-NA');
        expect(fc.invalid).toBeTruthy();

        fc.setValue(' ');
        expect(fc.invalid).toBeTruthy();
    });


});
