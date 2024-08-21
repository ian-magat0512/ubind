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
import { Observable } from 'rxjs';
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
import * as _ from 'lodash-es';
import { ApiService } from './api.service';
import { QuoteApiService } from './api/quote-api.service';
import { ApplicationLoadService } from './application-load-service';
import { ApplicationStartupService } from './application-startup.service';
import formConfig from './application-startup-service.test-form-config.json';
import { UnifiedFormModelService } from './unified-form-model.service';
import { AuthenticationService } from './authentication.service';
import { FakeToolTipService } from './fakes/fake-tooltip.service';
import { ToolTipService } from './tooltip.service';
import { AppContextApiService } from './api/app-context-api.service';
import { ClaimApiService } from './api/claim-api-service';
import { Alert } from '@app/models/alert';
import { NotificationService } from './notification.service';
import { ConfigurationV2Processor } from './configuration-v2-processor';
import { EncryptionService } from './encryption.service';
import { ProblemDetails } from '@app/models/problem-details';
import { FakeContextEntityService } from '@app/services/fakes/fake-context-entity.service';
import { ContextEntityService } from './context-entity.service';
import { OperationInstructionService } from './operation-instruction.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from './operation-status.service';
import { FormsAppContextModel } from '@app/models/forms-app-context.model';
import { LoggerService } from './logger.service';

/* global spyOn */

describe('ApplicationStartupService', () => {
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

    let appContextApiServiceStub: any = {
        getFormsAppContext: (
            tenant: string,
            product: string,
            organisation?: string,
            portal?: string,
            quoteId?: string,
        ): Observable<FormsAppContextModel> => {
            return new Observable((observer: any): void => {
                observer.next({});
                observer.complete();
            });
        },
    };

    let encryptionServiceStub: any = {
        loadPublicKey: (): void => {},
    };

    let workflowStepOperationStub: any = {
        execute: (): Observable<any> => {
            return new Observable((observer: any): void => {
                observer.next({});
                observer.complete();
            });
        },
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
                { provide: WorkflowStepOperation, useValue: workflowStepOperationStub },
                ConfigService,
                FormService,
                AttachmentService,
                { provide: CalculationOperation, useValue: workflowStepOperationStub },
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
                UnifiedFormModelService,
                ApplicationStartupService,
                AuthenticationService,
                { provide: AppContextApiService, useValue: appContextApiServiceStub },
                NotificationService,
                ConfigurationV2Processor,
                { provide: EncryptionService, useValue: encryptionServiceStub },
                { provide: ContextEntityService, useClass: FakeContextEntityService },
                OperationStatusService,
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

    it('loads formModel.json from configuration if this is a new quote', async () => {
        // Arrange
        let sut: ApplicationStartupService = TestBed.inject<ApplicationStartupService>(ApplicationStartupService);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        let unifiedFormModelService: UnifiedFormModelService
            = TestBed.inject<UnifiedFormModelService>(UnifiedFormModelService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        spyOn(sut, 'fetchAndProcessConfiguration').and.callFake(async () => {
            configProcessorService.onConfigurationResponse(response);
        });
        spyOn<any>(sut, 'createNewQuote');

        let urlParams: URLSearchParams = new URLSearchParams();
        urlParams.set('tenant', 'test-tenant');
        urlParams.set('product', 'test-product');
        urlParams.set('environment', 'staging');

        // Act
        await sut.initialise(urlParams);

        // Assert
        expect(unifiedFormModelService.workingFormModel.model['testField']).toBe("testValue");
    });

    it('creating a renewal quote requires a policy ID', async () => {
        // Arrange
        const quoteType: string = 'renewal';
        let sut: ApplicationStartupService = TestBed.inject<ApplicationStartupService>(ApplicationStartupService);
        let urlParams: URLSearchParams = new URLSearchParams();
        urlParams.set('tenant', 'test-tenant');
        urlParams.set('product', 'test-product');
        urlParams.set('environment', 'staging');
        urlParams.set('quoteType', quoteType);

        // Act
        let caughtError: ProblemDetails = null;
        try {
            await sut.initialise(urlParams);
        } catch(err: any) {
            caughtError = err;
        }

        // Assert
        expect(caughtError).not.toBeNull();
        expect(caughtError.code).toBe('policy.modification.requested.without.policy.id');
    });

    it('creating an adjustment quote requires a policy ID', async () => {
        // Arrange
        const quoteType: string = 'adjustment';
        let sut: ApplicationStartupService = TestBed.inject<ApplicationStartupService>(ApplicationStartupService);
        let urlParams: URLSearchParams = new URLSearchParams();
        urlParams.set('tenant', 'test-tenant');
        urlParams.set('product', 'test-product');
        urlParams.set('environment', 'staging');
        urlParams.set('quoteType', quoteType);

        // Act
        let caughtError: ProblemDetails = null;
        try {
            await sut.initialise(urlParams);
        } catch(err: any) {
            caughtError = err;
        }

        // Assert
        expect(caughtError).not.toBeNull();
        expect(caughtError.code).toBe('policy.modification.requested.without.policy.id');
    });

    it('creating a cancellation quote requires a policy ID', async () => {
        // Arrange
        const quoteType: string = 'cancellation';
        let sut: ApplicationStartupService = TestBed.inject<ApplicationStartupService>(ApplicationStartupService);
        let urlParams: URLSearchParams = new URLSearchParams();
        urlParams.set('tenant', 'test-tenant');
        urlParams.set('product', 'test-product');
        urlParams.set('environment', 'staging');
        urlParams.set('quoteType', quoteType);

        // Act
        let caughtError: ProblemDetails = null;
        try {
            await sut.initialise(urlParams);
        } catch(err: any) {
            caughtError = err;
        }

        // Assert
        expect(caughtError).not.toBeNull();
        expect(caughtError.code).toBe('policy.modification.requested.without.policy.id');
    });

    it('application mode edit requires a quote ID, or both policyID and quoteType', async () => {
        // Arrange
        let sut: ApplicationStartupService = TestBed.inject<ApplicationStartupService>(ApplicationStartupService);
        let urlParams: URLSearchParams = new URLSearchParams();
        urlParams.set('tenant', 'test-tenant');
        urlParams.set('product', 'test-product');
        urlParams.set('environment', 'staging');
        urlParams.set('mode', 'edit');

        // Act
        let caughtError: ProblemDetails = null;
        try {
            await sut.initialise(urlParams);
        } catch(err: any) {
            caughtError = err;
        }

        // Assert
        expect(caughtError).not.toBeNull();
        expect(caughtError.code).toBe('application.mode.modify.without.existing.quote.or.policy');
    });

    it('application mode edit with policyID also requires quoteType', async () => {
        // Arrange
        let sut: ApplicationStartupService = TestBed.inject<ApplicationStartupService>(ApplicationStartupService);
        let urlParams: URLSearchParams = new URLSearchParams();
        urlParams.set('tenant', 'test-tenant');
        urlParams.set('product', 'test-product');
        urlParams.set('environment', 'staging');
        urlParams.set('mode', 'edit');
        urlParams.set('policyId', '1234-abcde');

        // Act
        let caughtError: ProblemDetails = null;
        try {
            await sut.initialise(urlParams);
        } catch(err: any) {
            caughtError = err;
        }

        // Assert
        expect(caughtError).not.toBeNull();
        expect(caughtError.code).toBe('application.mode.modify.without.existing.quote.or.policy');
    });

    it('application mode edit with renewal quoteType requires a policy ID', async () => {
        // Arrange
        const quoteType: string = 'renewal';
        let sut: ApplicationStartupService = TestBed.inject<ApplicationStartupService>(ApplicationStartupService);
        let urlParams: URLSearchParams = new URLSearchParams();
        urlParams.set('tenant', 'test-tenant');
        urlParams.set('product', 'test-product');
        urlParams.set('environment', 'staging');
        urlParams.set('mode', 'edit');
        urlParams.set('quoteType', quoteType);

        // Act
        let caughtError: ProblemDetails = null;
        try {
            await sut.initialise(urlParams);
        } catch(err: any) {
            caughtError = err;
        }

        // Assert
        expect(caughtError).not.toBeNull();
        expect(caughtError.code).toBe('policy.modification.requested.without.policy.id');
    });
});
