/* eslint-disable no-unused-vars */
import { ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, DebugElement, Directive } from '@angular/core';
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
import * as _ from 'lodash-es';
import formConfig from './actions-widget.test-form-config.json';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { EncryptionService } from '@app/services/encryption.service';
import { AppEventService } from '@app/services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { filter } from 'rxjs/operators';
import { By } from '@angular/platform-browser';
import { FormType } from '@app/models/form-type.enum';
import { ActionsWidget } from './actions.widget';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { ContextEntityService } from '@app/services/context-entity.service';
import { FakeContextEntityService } from '@app/services/fakes/fake-context-entity.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from '@app/services/operation-status.service';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */
/* global jasmine */

describe('ActionsWidget', () => {
    let sut: ActionsWidget;
    let fixture: ComponentFixture<ActionsWidget>;
    let eventService: EventService;
    let applicationService: ApplicationService;
    let workflowService: WorkflowService;
    let validationService: ValidationService;
    let expressionDependencies: ExpressionDependencies;

    let alertServiceStub: any = {
        updateSubject: new Subject<Alert>(),
        visibleSubject: new Subject<boolean>(),
        alert: (alert: Alert): void => {},
    };

    let operationStub: any = {
        execute: (): Observable<any> => new BehaviorSubject<object>({}),
    };

    let appEventServiceStub: any = {
        createEvent: (): void => {},
    };

    let broadcastServiceStub: any = {
        on: (key: any): Subject<any> => new Subject<any>(),
    };

    let operationFactoryStub: any = {
        getStatus: (operation: any): string => 'success',
        execute: (operationName: string, args: any): Observable<any> => new Subject<any>().asObservable(),
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

    let windowScrollServiceStub: any = {
        scrollElementIntoView: (): void => {
            eventService = TestBed.inject<EventService>(EventService);
            eventService.scrollingFinishedSubject.next(true);
        },
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
        changeDetection: ChangeDetectionStrategy.OnPush,
    })
    class TestHostComponent {
        public ready: boolean = false;
    }

    beforeEach(async () => TestBed.configureTestingModule({
        declarations: [
            TestHostComponent,
            ActionsWidget,
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
            { provide: WindowScrollService, useValue: windowScrollServiceStub },
            { provide: BroadcastService, useValue: broadcastServiceStub },
            { provide: CssProcessorService, useValue: {} },
            ValidationService,
            ExpressionMethodService,
            { provide: WebhookService, useValue: webhookServiceStub },
            { provide: AttachmentOperation, useValue: attachmentOperationStub },
            { provide: PolicyOperation, useValue: policyOperationStub },
            UserService,
            ResumeApplicationService,
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
            { provide: EncryptionService, useValue: encryptionServiceStub },
            WorkflowService,
            { provide: AppEventService, useValue: appEventServiceStub },
            { provide: WorkflowStepOperation, useValue: operationStub },
            NotificationService,
            ConfigurationV2Processor,
            BrowserDetectionService,
            RevealGroupTrackingService,
            { provide: ContextEntityService, useClass: FakeContextEntityService },
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
            'https://localhost:44366',
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
    }));

    afterEach(() => {
        if (fixture) {
            fixture.destroy();
        }
    });

    it('when an action button is added, it appears', async () => {
        // Arrange
        fixture = TestBed.createComponent(ActionsWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(formConfig);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primary: true,
            },
        };
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('ubind-action-widget'));
        expect(debugElement == null)
            .toBeTruthy('The action button should not be there yet.');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();

        // Assert
        debugElement = fixture.debugElement.query(By.css('ubind-action-widget'));
        expect(debugElement != null)
            .toBeTruthy('The action button should have been rendered.');
    });

    it('when an action button is removed, it disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(ActionsWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primary: true,
            },
        };
        let response2: any = _.cloneDeep(formConfig);
        response2['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            let debugElement: DebugElement
                = fixture.debugElement.query(By.css('ubind-action-widget'));
            expect(debugElement != null)
                .toBeTruthy('The action button should be there from the start.');

            // Act
            configProcessorService.onConfigurationResponse(response2);
            fixture.detectChanges();

            // Assert
            debugElement = fixture.debugElement.query(By.css('ubind-action-widget'));
            expect(debugElement == null)
                .toBeTruthy('The action button should have been removed.');
            resolve();
        });
    });

});
