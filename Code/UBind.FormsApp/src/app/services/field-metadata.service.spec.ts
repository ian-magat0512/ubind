import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfigurationOperation } from '@app/operations/configuration.operation';
import { ConfigProcessorService } from './config-processor.service';
import { EvaluateService } from './evaluate.service';
import { EventService } from './event.service';
import { MessageService } from './message.service';
import { sharedConfig } from '../app.module.shared';
import { ConfigService } from './config.service';
import { WindowScrollService } from './window-scroll.service';
import { FormService } from './form.service';
import { ApplicationService } from './application.service';
import { CalculationService } from './calculation.service';
import { WorkflowService } from './workflow.service';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { ExpressionMethodService } from '@app/expressions/expression-method.service';
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
import { WorkflowStatusService } from './workflow-status.service';
import { CssProcessorService } from './css-processor.service';
import { BroadcastService } from './broadcast.service';
import { AlertService } from './alert.service';
import { AttachmentService } from './attachment.service';
import { Subject } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import * as _ from 'lodash-es';
import formConfig from './field-metadata-service.test-form-config.json';
import { FieldMetadataService } from './field-metadata.service';
import { ExpressionInputSubjectService } from '@app/expressions/expression-input-subject.service';
import { NotificationService } from './notification.service';
import { ConfigurationV2Processor } from './configuration-v2-processor';
import { ValidationService } from './validation.service';
import { EncryptionService } from './encryption.service';
import { BrowserDetectionService } from './browser-detection.service';
import { OperationInstructionService } from './operation-instruction.service';
import { OperationStatusService } from './operation-status.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from './api.service';
import { LoggerService } from './logger.service';

/* global spyOn */

describe('FieldMetadataService', () => {
    let fixture: ComponentFixture<TestHostComponent>;

    /**
     * The test host component is a component we can get angular to load
     * for testing purposes.
     */
    @Component({
        selector: `host-component`,
        template: `<web-form></web-form>`,
    })
    class TestHostComponent {
    }

    let webhookServiceStub: object = {
        getActiveWebhookCount: (): number => 0,
        inProgressSubject: new Subject<boolean>(),
        webhookFieldInProgressSubject: new Subject<boolean>(),
    };

    let attachmentOperationStub: object = {
        operationInProgress: false,
        inProgressSubject: new Subject<boolean>(),
    };

    let encryptionServiceStub: any = {
        loadPublicKey: (): void => {},
    };

    beforeEach(async () => {
        TestBed.configureTestingModule({
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
                { provide: WorkflowService, useValue: {} },
                ConfigService,
                FormService,
                AttachmentService,
                { provide: CalculationOperation, useValue: {} },
                ApplicationService,
                { provide: AlertService, useValue: {} },
                { provide: WindowScrollService, useValue: {} },
                { provide: BroadcastService, useValue: {} },
                { provide: CssProcessorService, useValue: {} },
                ApplicationService,
                ExpressionMethodService,
                { provide: OperationFactory, useValue: {} },
                { provide: WebhookService, useValue: webhookServiceStub },
                { provide: AttachmentOperation, useValue: attachmentOperationStub },
                { provide: PolicyOperation, useValue: {} },
                UserService,
                ResumeApplicationService,
                { provide: OperationFactory, useValue: {} },
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
                ExpressionInputSubjectService,
                NotificationService,
                ConfigurationV2Processor,
                ValidationService,
                { provide: EncryptionService, useValue: encryptionServiceStub },
                BrowserDetectionService,
                OperationStatusService,
                ApiService,
                LoggerService,
                OperationInstructionService,
            ],
            imports: [
                NoopAnimationsModule,
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

    it('should get me the fieldPaths by tag', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        let response: object = _.cloneDeep(formConfig);
        response['status'] = 'success';
        configProcessorService.onConfigurationResponse(response);
        let fieldMetadataService: FieldMetadataService = TestBed.inject<FieldMetadataService>(FieldMetadataService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let eventService: EventService = TestBed.inject<EventService>(EventService);
        eventService.fieldPathAddedSubject.next('step1Field');
        expressionInputSubjectService.getFieldValueSubject('step1Field', 'something');

        // Act
        let fieldPaths: Set<string> = fieldMetadataService.getFieldPathsWithTag('disclosure');

        // Assert
        expect(fieldPaths).toContain('step1Field');
    });

    it('should return a fieldPath from a repeating question set when we get the fieldPaths by tag', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        configProcessorService.onConfigurationResponse(response);
        let fieldMetadataService: FieldMetadataService = TestBed.inject<FieldMetadataService>(FieldMetadataService);
        let expressionInputSubjectService: ExpressionInputSubjectService
            = TestBed.inject<ExpressionInputSubjectService>(ExpressionInputSubjectService);
        let eventService: EventService = TestBed.inject<EventService>(EventService);
        eventService.fieldPathAddedSubject.next('testRepeating[0].firstName');
        expressionInputSubjectService.getFieldValueSubject('testRepeating[0].firstName', 'John');

        // Act
        let fieldPaths: Set<string> = fieldMetadataService.getFieldPathsWithTag('disclosure');

        // Assert
        expect(fieldPaths).toContain('testRepeating[0].firstName');
    });

});
