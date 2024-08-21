import { EventEmitter, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { PolicyOperation } from '@app/operations/policy.operation';
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
import formConfig from './trigger-processing.service.test-form-config.json';
import calculationResponse from './trigger-processing-service.test-calculation-response.json';
import defaultTextElements from './form-configuration/default-text-elements.json';
import { TriggerProcessingService } from './trigger-processing.service';
import { FormType } from '@app/models/form-type.enum';
import { QuoteResult } from '@app/models/quote-result';
import { FakeToolTipService } from './fakes/fake-tooltip.service';
import { ToolTipService } from './tooltip.service';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { Alert } from '@app/models/alert';
import { NotificationService } from './notification.service';
import { ConfigurationV2Processor } from './configuration-v2-processor';
import { EncryptionService } from './encryption.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from './operation-status.service';
import { OperationInstructionService } from './operation-instruction.service';
import { QuoteResultProcessor } from './quote-result-processor';
import { ApiService } from './api.service';
import { LoggerService } from './logger.service';

/* global spyOn */

/**
 * Fake workflow service class
 */
class FakeWorkflowService {
    public navigate: EventEmitter<any> = new EventEmitter<any>();
    public currentDestination: WorkflowDestination = { stepName: "step1" };
    public initialised: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
    public actionAborted: EventEmitter<any> = new EventEmitter<any>();
    public actionCompleted: EventEmitter<any> = new EventEmitter<any>();
    public quoteLoadedSubject: Subject<boolean> = new Subject<boolean>();
    public loadedCustomerHasUserSubject: Subject<boolean> = new Subject<boolean>();
    public navigateToSubject: Subject<string> = new Subject<string>();
    public completedNavigationIn(): void { }
}

describe('TriggerProcessingService', () => {
    let calculationService: CalculationService;
    let applicationService: ApplicationService;
    // eslint-disable-next-line no-unused-vars
    let quoteResultProcessor: QuoteResultProcessor;

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
                { provide: WorkflowService, useClass: FakeWorkflowService },
                AbnPipe,
                BsbPipe,
                CreditCardNumberPipe,
                CurrencyPipe,
                TimePipe,
                PhoneNumberPipe,
                NumberPlatePipe,
                ExpressionDependencies,
                WorkflowStatusService,
                NotificationService,
                ConfigurationV2Processor,
                { provide: EncryptionService, useValue: encryptionServiceStub },
                TriggerProcessingService,
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
                'https://localhost',
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
            quoteResultProcessor = TestBed.inject<QuoteResultProcessor>(QuoteResultProcessor);
        });
    });

    it('should append the MessageAppendix to the trigger message', async () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);
        configResponse.triggers.push({
            name: "Tools Cover 8k",
            key: "toolsCover8k",
            typeName: "Endorsment",
            type: "endorsement",
            customerMessage: 'My trigger message.',
        });
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        expect(quoteResult.trigger.message).toBe(
            'My trigger message.<span class="message-appendix"> '
            + defaultTextElements.textElements.sidebar.endorsementTriggeredMessageAppendix.text
            + '</span>');
    });

    it('should generate the correct message when each trigger has a message, header and label', () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);
        configResponse.triggers.push({
            name: "Tools Cover 8k",
            key: "toolsCover8k",
            typeName: "Endorsment",
            type: "endorsement",
            customerMessage: 'New format trigger message.',
        });
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        expect(quoteResult.trigger.message).toBe(
            'New format trigger message.<span class="message-appendix"> '
                + defaultTextElements.textElements.sidebar.endorsementTriggeredMessageAppendix.text
                + '</span>');
    });

    it('should use the header text defined in the trigger config', () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);

        configResponse.triggers.push({
            name: "Tools Cover 8k",
            key: "toolsCover8k",
            typeName: "Endorsment",
            type: "endorsement",
            customerSummary: 'My trigger header.',
            customerMessage: 'New format trigger message.',
        });
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        expect(quoteResult.trigger.header).toBe('My trigger header.');
    });

    it('should use the title/label text defined in the trigger config', () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);
        configResponse.triggers.push({
            name: "Tools Cover 8k",
            key: "toolsCover8k",
            typeName: "Endorsment",
            type: "endorsement",
            customerTitle: 'My trigger title.',
            customerMessage: 'New format trigger message.',
        });
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        expect(quoteResult.trigger.title).toBe('My trigger title.');
    });

    it('should evaluate expressions in the custom trigger message', () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);
        configResponse.triggers.push({
            name: "Tools Cover 8k",
            key: "toolsCover8k",
            typeName: "Endorsment",
            type: "endorsement",
            customerMessage: 'My trigger %{ 4 * 5 }%.',
        });
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        expect(quoteResult.trigger.message).toBe(
            'My trigger 20.<span class="message-appendix"> '
            + defaultTextElements.textElements.sidebar.endorsementTriggeredMessageAppendix.text
            + '</span>');
    });

    it('should evaluate expressions in the custom trigger header', () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);
        configResponse.triggers.push({
            name: "Tools Cover 8k",
            key: "toolsCover8k",
            typeName: "Endorsment",
            type: "endorsement",
            customerSummary: 'My trigger %{ 4 * 5 }%.',
            customerMessage: 'Something',
        });
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        expect(quoteResult.trigger.header).toBe('My trigger 20.');
    });

    it('should evaluate expressions in the custom trigger title', () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);
        configResponse.triggers.push({
            name: "Tools Cover 8k",
            key: "toolsCover8k",
            typeName: "Endorsment",
            type: "endorsement",
            customerTitle: 'My trigger %{ 4 * 5 }%.',
            customerMessage: 'Something',
        });
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        expect(quoteResult.trigger.title).toBe('My trigger 20.');
    });

    it('should use the trigger header from textElements when there is no custom trigger header set', () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        expect(quoteResult.trigger.header)
            .toBe(defaultTextElements.textElements.sidebar.endorsementTriggeredHeader.text);
    });

    it('should ensure the active trigger is the one with the highest precedence', () => {
        // Arrange
        calculationService = TestBed.inject<CalculationService>(CalculationService);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let configResponse: any = _.cloneDeep(formConfig);
        configResponse['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(configResponse);

        // Act
        let calcResponse: any = _.cloneDeep(calculationResponse);
        calcResponse.calculationResult.triggers.endorsement.toolsCover8k = true;
        calcResponse.calculationResult.triggers.decline.turnover150k = true;
        calculationService.processCalculationResponse(calcResponse);

        // Assert
        let quoteResult: QuoteResult = applicationService.latestQuoteResult;
        // since decline is a higher order of precedence than endorsement, it should be the active trigger.
        expect(quoteResult.trigger.type).toBe('decline');
    });

});
