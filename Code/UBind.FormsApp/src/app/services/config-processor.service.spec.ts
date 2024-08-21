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
import formConfig from './config-processor-service.test-form-config.json';
import defaultTextElements from './form-configuration/default-text-elements.json';
import { NotificationService } from './notification.service';
import { ConfigurationV2Processor } from './configuration-v2-processor';
import { ValidationService } from './validation.service';
import { EncryptionService } from './encryption.service';
import { BrowserDetectionService } from './browser-detection.service';
import { OperationStatusService } from './operation-status.service';
import { OperationInstructionService } from './operation-instruction.service';
import { ApiService } from './api.service';
import { LoggerService } from './logger.service';

/* global spyOn */

describe('ConfigurationProcessorService', () => {
    let fixture: ComponentFixture<TestHostComponent>;

    /**
     * Test host component class
     */
    @Component({
        selector: `host-component`,
        template: `<web-form></web-form>`,
    })
    class TestHostComponent {
    }

    let webhookServiceStub: any = {
        getActiveWebhookCount: (): number => 0,
        inProgressSubject: new Subject<boolean>(),
        webhookFieldInProgressSubject: new Subject<boolean>(),
    };

    let attachmentOperationStub: any = {
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

    it('should let me load the quote form using the config from my json file', () => {
        // Arrange        
        fixture = TestBed.createComponent(TestHostComponent);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let act: () => void = (): void => configProcessorService.onConfigurationResponse(response);
        expect(act).not.toThrow();
    });

    it('should load the default text elements where none has been provided', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let bindingQuoteHeaderIndex: number = response.form.textElements.findIndex(
            (textElement: any) => textElement.name == "Binding Quote Header");
        response.form.textElements.splice(bindingQuoteHeaderIndex, 1);
        configProcessorService.onConfigurationResponse(response);
        let configService: ConfigService = TestBed.inject<ConfigService>(ConfigService);
        expect(configService.textElements.sidebar.bindingQuoteHeader.text)
            .toBe(defaultTextElements.textElements.sidebar.bindingQuoteHeader.text);
    });

    it('should not overwrite text elements with the default text elements', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let bindingQuoteHeaderIndex: number = response.form.textElements.findIndex(
            (textElement: any) => textElement.name == "Binding Quote Header");
        response.form.textElements[bindingQuoteHeaderIndex].text = 'My custom binding quote header';
        configProcessorService.onConfigurationResponse(response);
        let configService: ConfigService = TestBed.inject<ConfigService>(ConfigService);
        expect(configService.textElements.sidebar.bindingQuoteHeader.text).toBe('My custom binding quote header');
    });
});
