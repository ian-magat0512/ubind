import { FormService } from '../services/form.service';
import { CalculationService } from '../services/calculation.service';
import { ApplicationService } from '../services/application.service';
import { AlertService } from '../services/alert.service';
import { BroadcastService } from '../services/broadcast.service';
import { ConfigurationOperation } from '../operations/configuration.operation';
import { ConfigProcessorService } from '../services/config-processor.service';
import { ConfigService } from '../services/config.service';
import { WorkflowService } from '../services/workflow.service';
import { MessageService } from '../services/message.service';
import { UserService } from '../services/user.service';
import { EventService } from '../services/event.service';
import { WorkflowStatusService } from '../services/workflow-status.service';
import { CUSTOM_ELEMENTS_SCHEMA, Directive } from '@angular/core';
import { Subject } from 'rxjs';
import { TestBed } from '@angular/core/testing';
import { ValidationService } from '../services/validation.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { ExpressionMethodService } from '@app/expressions/expression-method.service';
import { BehaviorSubject, Observable } from 'rxjs';
import { sharedConfig } from '@app/app.module.shared';
import { EvaluateService } from '../services/evaluate.service';
import { AttachmentService } from '../services/attachment.service';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { WindowScrollService } from '../services/window-scroll.service';
import { CssProcessorService } from '../services/css-processor.service';
import { OperationFactory } from '../operations/operation.factory';
import { WebhookService } from '../services/webhook.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { PolicyOperation } from '@app/operations/policy.operation';
import { ResumeApplicationService } from '../services/resume-application.service';
import { AbnPipe } from '@app/pipes/abn.pipe';
import { BsbPipe } from '@app/pipes/bsb.pipe';
import { CreditCardNumberPipe } from '@app/pipes/credit-card-number.pipe';
import { CurrencyPipe } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NumberPlatePipe } from '@app/pipes/number-plate.pipe';
import { PhoneNumberPipe } from '@app/pipes/phone-number.pipe';
import { TimePipe } from '@app/pipes/time.pipe';
import { NgSelectModule } from '@ng-select/ng-select';
import { AppEventService } from '../services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import formConfig from './workflow-helper.test-form-config.json';
import * as _ from 'lodash-es';
import { FakeToolTipService } from '../services/fakes/fake-tooltip.service';
import { ToolTipService } from '../services/tooltip.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from '../services/notification.service';
import { ConfigurationV2Processor } from '../services/configuration-v2-processor';
import { Errors } from '@app/models/errors';
import { EncryptionService } from '../services/encryption.service';
import { WorkflowHelper } from './workflow.helper';
import { WorkflowDestination } from '@app/models/workflow-destination';

/* global spyOn */

/**
 * fake operation service class
 */
@Directive()

class FakeOperationFactory {

    public operations: any = {
        'calculation': {
            result: new Subject<any>(),
        },
    };

    // eslint-disable-next-line no-unused-vars
    public getStatus(operation: string): any {
        return 'success';
    }

    // eslint-disable-next-line no-unused-vars
    public execute(operationName: string, param: any): Observable<any> {
        return new BehaviorSubject<any>({
            workflowStep: 'step4',
        });
    }
}

describe('WorkflowHelper', () => {
    let workflowHelper: WorkflowHelper;

    let alertServiceStub: any = {
        updateSubject: new Subject<Alert>(),
        visibleSubject: new Subject<boolean>(),
        alert: (alert: Alert): void => {},
    };

    let broadcastServiceStub: any = {
        // eslint-disable-next-line no-unused-vars
        on: (key: any): Subject<any> => new Subject<any>(),
    };

    let operationFactoryStub: any = {
        // eslint-disable-next-line no-unused-vars
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

    let workflowStepOperationStub: any = {
        execute: (): Observable<any> => {
            return new BehaviorSubject<any>({ 'status': 'success' }).asObservable();
        },
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
                { provide: EvaluateService, useClass: EvaluateService },
                { provide: EventService, useClass: EventService },
                { provide: AppEventService, useClass: AppEventService },
                { provide: CalculationService, useClass: CalculationService },
                { provide: WorkflowService, useClass: WorkflowService },
                { provide: WorkflowStepOperation, useValue: {} },
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
                { provide: WorkflowStepOperation, useValue: workflowStepOperationStub },
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
                WorkflowHelper,
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

    it('it should set the starting step to 0 if the first article is visible', () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1['startScreen'] = true;

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowHelper = TestBed.inject<WorkflowHelper>(WorkflowHelper);

        // Act
        let destination: WorkflowDestination = workflowHelper.getStartingDestination();

        // Assert
        expect(destination.stepName).toBe('step1');
        expect(destination.articleIndex).toBe(0);
    });

    it('skips articles that are hidden when determining the starting destination', () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step2['startScreen'] = true;

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowHelper = TestBed.inject<WorkflowHelper>(WorkflowHelper);

        // Act
        let destination: WorkflowDestination = workflowHelper.getStartingDestination();

        // Assert
        expect(destination.stepName).toBe('step2');
        expect(destination.articleIndex).toBe(2);
    });

    it('skips articles when all of their article elements are hidden', () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step3['startScreen'] = true;

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowHelper = TestBed.inject<WorkflowHelper>(WorkflowHelper);

        // Act
        let destination: WorkflowDestination = workflowHelper.getStartingDestination();

        // Assert
        expect(destination.stepName).toBe('step3');
        expect(destination.articleIndex).toBe(2);
        expect(destination.articleElementIndex).toBe(1);
    });

    it('throws an error when no article element is visible', () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step4['startScreen'] = true;

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowHelper = TestBed.inject<WorkflowHelper>(WorkflowHelper);

        // Act
        let action: () => WorkflowDestination = (): WorkflowDestination => workflowHelper.getStartingDestination();

        // Assert
        expect(action).toThrow(Errors.Workflow.StartingDestinationHidden('step4'));
    });
});
