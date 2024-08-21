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
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
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
import formConfig from './workflow-service.test-form-config.json';
import * as _ from 'lodash-es';
import { ProblemDetails } from '@app/models/problem-details';
import { FakeToolTipService } from './fakes/fake-tooltip.service';
import { ToolTipService } from './tooltip.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from './notification.service';
import { ConfigurationV2Processor } from './configuration-v2-processor';
import { Errors } from '@app/models/errors';
import { EncryptionService } from './encryption.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from './operation-status.service';
import { OperationInstructionService } from './operation-instruction.service';
import { ApiService } from './api.service';
import { LoggerService } from './logger.service';

/* global spyOn */

describe('WorkflowService', () => {
    let workflowService: WorkflowService;

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

    it('it should not evaluate destinationStep as an expression', async () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1['actions'] = {
            enquiry: {
                operations: ["formUpdate"],
                destinationStep: '\'step\' + \'2\'',
            },
        };

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        // stop operations from happening
        spyOn(<any>workflowService, "executeOperations");

        // Act
        // TODO: Can't get this to work as a separately defined action.
        // let action: () => Promise<void> = (): Promise<void> => workflowService.queueAction('enquiry', 'sidebar');

        // Assert
        // eslint-disable-next-line no-undef
        await expectAsync(
            workflowService.queueAction('enquiry', 'sidebar'),
        ).toBeRejectedWith(Errors.Workflow.StepNotFound('\'step\' + \'2\''));
    });

    it('it should evaluate destinationStepExpression as an expression', async () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1['actions'] = {
            enquiry: {
                operations: ["formUpdate"],
                destinationStepExpression: '\'step\' + \'2\'',
            },
        };

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        // stop operations from happening
        spyOn(<any>workflowService, "executeOperations");

        // Act
        await workflowService.queueAction('enquiry', 'sidebar');

        // Assert
        let step: string = workflowService.currentNavigation.to.stepName;
        expect(step).toBe('step2');
    });

    it('throws an error when no start screen expression is true', () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.startScreenExpression = "1 > 2";
        response.form.workflowConfiguration.step2.startScreenExpression = "3 > 4";
        response.form.workflowConfiguration.step3.startScreenExpression = "1 > 2";
        response.form.workflowConfiguration.step4.startScreenExpression = "3 > 4";
        delete response.form.workflowConfiguration.step5.startScreenExpression;
        response.form.workflowConfiguration.step6.startScreenExpression = "3 > 4";

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(response);
        let caughtError: ProblemDetails;
        try {
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            workflowService.initialise();
        } catch (err) {
            caughtError = err;
        }

        expect(caughtError).not.toBeNull();
        expect(caughtError.code).toBe('product.configuration.error');
    });

    it('determines the correct starting step', () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.startScreenExpression = "1 > 2";
        response.form.workflowConfiguration.step2.startScreenExpression = "3 > 4";
        response.form.workflowConfiguration.step3.startScreenExpression = "1 < 99";
        response.form.workflowConfiguration.step4.startScreenExpression = "3 > 4";
        delete response.form.workflowConfiguration.step5.startScreenExpression;
        response.form.workflowConfiguration.step6.startScreenExpression = "3 > 4";

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        (<any>configProcessorService).onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        expect(workflowService.currentDestination.stepName).toBe('step3');
    });

    it('determines the correct destination step when there is a destinationStepExpression', async () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1['actions'] = {
            enquiry: {
                operations: ["formUpdate"],
                destinationStepExpression: '\'step\' + \'2\'',
            },
        };

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        // stop operations from happening
        spyOn(<any>workflowService, "executeOperations");

        // Act
        await workflowService.queueAction('enquiry', 'sidebar');

        // Assert
        let step: string = workflowService.currentNavigation.to.stepName;
        expect(step).toBe('step2');
    });

    it('determines the correct destination step when there is a destinationStep', async () => {
        // Arrange
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1['actions'] = {
            enquiry: {
                operations: ["formUpdate"],
                destinationStep: 'step2',
            },
        };

        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        // stop operations from happening
        spyOn(<any>workflowService, "executeOperations");

        // Act
        await workflowService.queueAction('enquiry', 'sidebar');

        // Assert
        let step: string = workflowService.currentNavigation.to.stepName;
        expect(step).toBe('step2');
    });

    it('uses the destinationStepExpression when there is both a destinationStep and destinationStepExpression',
        async () => {
            // Arrange
            let response: any = _.cloneDeep(formConfig);
            response['status'] = 'success';
            response.form.workflowConfiguration.step1['actions'] = {
                enquiry: {
                    operations: ["formUpdate"],
                    destinationStep: 'step2',
                    destinationStepExpression: "'step' + 3",
                },
            };

            let configProcessorService: ConfigProcessorService
                = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            workflowService.initialise();
            // stop operations from happening
            spyOn(<any>workflowService, "executeOperations");

            // Act
            await workflowService.queueAction('enquiry', 'sidebar');

            // Assert
            let step: string = workflowService.currentNavigation.to.stepName;
            expect(step).toBe('step3');
        });

    it('when the current workflow step is deleted, it uses the startScreenExpression and loads the first page.',
        () => {
        // Arrange
            let response: any = _.cloneDeep(formConfig);
            response['status'] = 'success';
            delete response.form.workflowConfiguration.step2.startScreenExpression;
            delete response.form.workflowConfiguration.step3.startScreenExpression;
            delete response.form.workflowConfiguration.step4.startScreenExpression;
            delete response.form.workflowConfiguration.step5.startScreenExpression;
            delete response.form.workflowConfiguration.step6.startScreenExpression;
            let response2: any = _.cloneDeep(response);
            let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            workflowService.initialise();
            workflowService.navigateTo({ stepName: 'step2' });
            workflowService.completedNavigationOut();
            workflowService.completedNavigationIn();
            expect(workflowService.currentDestination.stepName).toBe('step2');

            // stop operations from happening
            spyOn(<any>workflowService, "executeOperations");

            // Act
            delete response2.form.workflowConfiguration.step2;
            configProcessorService.onConfigurationResponse(response2);

            // Assert
            let step: string = workflowService.currentNavigation.to.stepName;
            expect(step).toBe('step1');
        });

});
