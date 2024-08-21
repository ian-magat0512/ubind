/* eslint-disable max-classes-per-file */
import { FormService } from './form.service';
import { CalculationService } from './calculation.service';
import { ApplicationService } from './application.service';
import { AlertService } from './alert.service';
import { BroadcastService } from './broadcast.service';
import { ConfigurationOperation } from '../operations/configuration.operation';
import { ConfigProcessorService } from './config-processor.service';
import { ConfigService } from './config.service';
import { WorkflowService } from './workflow.service';
import { MessageService } from './message.service';
import { UserService } from './user.service';
import { EventService } from './event.service';
import { WorkflowStatusService } from './workflow-status.service';
import { CUSTOM_ELEMENTS_SCHEMA, Directive } from '@angular/core';
import { Subject } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { ValidationService } from './validation.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { ExpressionMethodService } from '@app/expressions/expression-method.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { sharedConfig } from '@app/app.module.shared';
import { EvaluateService } from './evaluate.service';
import { AttachmentService } from './attachment.service';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { WindowScrollService } from './window-scroll.service';
import { CssProcessorService } from './css-processor.service';
import { OperationFactory } from '../operations/operation.factory';
import { WebhookService } from './webhook.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { PolicyOperation } from '@app/operations/policy.operation';
import { ResumeApplicationService } from './resume-application.service';
import { AbnPipe } from '@app/pipes/abn.pipe';
import { BsbPipe } from '@app/pipes/bsb.pipe';
import { CreditCardNumberPipe } from '@app/pipes/credit-card-number.pipe';
import { CurrencyPipe } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NumberPlatePipe } from '@app/pipes/number-plate.pipe';
import { PhoneNumberPipe } from '@app/pipes/phone-number.pipe';
import { TimePipe } from '@app/pipes/time.pipe';
import { NgSelectModule } from '@ng-select/ng-select';
import { AppEventService } from './app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import { ApiService } from './api.service';
import { QuoteApiService } from './api/quote-api.service';
import { ApplicationLoadService } from './application-load-service';
import { FakeToolTipService } from './fakes/fake-tooltip.service';
import { ToolTipService } from './tooltip.service';
import { ClaimApiService } from './api/claim-api-service';
import { EncryptionService } from './encryption.service';
import { Alert } from '@app/models/alert';
import { Operation } from '@app/operations/operation';
import { OperationInstructionService } from './operation-instruction.service';
import { OperationStatusService } from './operation-status.service';

/* global spyOn */

/**
 * Fake operation service class
 */
@Directive()

/**
 * Fake operation factory class
 */
@Directive()
class FakeOperationFactory {
    public fakeOperation: FakeOperation = new FakeOperation();

    public create(operationName: string): Operation {
        return this.fakeOperation as Operation;
    }
}

/**
 * An operation that does nothing.
 */
class FakeOperation {
    public execute(args: any = {}, operationId: number = Date.now()): Observable<any> {
        return new BehaviorSubject<any>({
            workflowStep: 'step4',
        }).asObservable();
    }
}

describe('ApplicationLoadService', () => {
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
                AppEventService,
                CalculationService,
                WorkflowService,
                { provide: WorkflowStepOperation, useValue: {} },
                ConfigService,
                FormService,
                AttachmentService,
                { provide: CalculationOperation, useValue: {} },
                ApplicationService,
                { provide: AlertService, useValue: alertServiceStub },
                { provide: WindowScrollService, useValue: {} },
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
                ApplicationLoadService,
                ApiService,
                QuoteApiService,
                ClaimApiService,
                AbnPipe,
                BsbPipe,
                CreditCardNumberPipe,
                CurrencyPipe,
                TimePipe,
                PhoneNumberPipe,
                NumberPlatePipe,
                ExpressionDependencies,
                WorkflowStatusService,
                { provide: EncryptionService, useValue: encryptionServiceStub },
                OperationStatusService,
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

    it('adds the current workflow step to the history when loading a quote', async () => {
        // Arrange
        let applicationLoadService: ApplicationLoadService
            = TestBed.inject<ApplicationLoadService>(ApplicationLoadService);
        let workflowStatusService: WorkflowStatusService = TestBed.inject<WorkflowStatusService>(WorkflowStatusService);

        // Act
        await applicationLoadService.loadQuote('test123');

        // Assert
        expect(workflowStatusService.workflowStepHistory[0]).toBe('step4');
    });

    it('sets the current workflow step when loading a quote', async () => {
        // Arrange
        let applicationLoadService: ApplicationLoadService
            = TestBed.inject<ApplicationLoadService>(ApplicationLoadService);
        let applicationService: ApplicationService = TestBed.inject<ApplicationService>(ApplicationService);

        // Act
        await applicationLoadService.loadQuote('test123');

        // Assert
        expect(applicationService.currentWorkflowDestination.stepName).toBe('step4');
    });
});
