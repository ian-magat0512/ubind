/* eslint-disable max-classes-per-file */
import { ConfigurationOperation } from '../../../operations/configuration.operation';
import { ApplicationService } from '../../../services/application.service';
import { BroadcastService } from '../../../services/broadcast.service';
import { ConfigProcessorService } from '../../../services/config-processor.service';
import { ConfigService } from '../../../services/config.service';
import { FormService } from '../../../services/form.service';
import { CalculationService } from '../../../services/calculation.service';
import { UserService } from '../../../services/user.service';
import { AlertService } from '../../../services/alert.service';
import { MessageService } from '../../../services/message.service';
import { WorkflowService } from '@app/services/workflow.service';
import { WorkflowNavigation } from '@app/models/workflow-navigation';
import { EventService } from '@app/services/event.service';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { TestBed } from '@angular/core/testing';
import { Component, ViewChild, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { sharedConfig } from '@app/app.module.shared';
import { EvaluateService } from '@app/services/evaluate.service';
import { AttachmentService } from '@app/services/attachment.service';
import { ValidationService } from '@app/services/validation.service';
import { ExpressionMethodService } from '@app/expressions/expression-method.service';
import { ResumeApplicationService } from '@app/services/resume-application.service';
import { CalculationOperation } from '@app/operations/calculation.operation';
import { WindowScrollService } from '@app/services/window-scroll.service';
import { CssProcessorService } from '@app/services/css-processor.service';
import { OperationFactory } from '@app/operations/operation.factory';
import { WebhookService } from '@app/services/webhook.service';
import { AttachmentOperation } from '@app/operations/attachment.operation';
import { PolicyOperation } from '@app/operations/policy.operation';
import { AbnPipe } from '@app/pipes/abn.pipe';
import { BsbPipe } from '@app/pipes/bsb.pipe';
import { CreditCardNumberPipe } from '@app/pipes/credit-card-number.pipe';
import { CurrencyPipe } from '@app/pipes/currency.pipe';
import { TimePipe } from '@app/pipes/time.pipe';
import { PhoneNumberPipe } from '@app/pipes/phone-number.pipe';
import { NumberPlatePipe } from '@app/pipes/number-plate.pipe';
import { WorkflowStatusService } from '@app/services/workflow-status.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NgSelectModule } from '@ng-select/ng-select';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import * as _ from 'lodash-es';
import formConfig from './header-widget.test-form-config.json';
import { By } from '@angular/platform-browser';
import { filter } from 'rxjs/operators';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { WebFormComponent } from '@app/components/web-form/web-form';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { AppEventService } from '@app/services/app-event.service';
import { WorkflowStepOperation } from '@app/operations/workflow-step.operation';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { AngularElementsService } from '@app/services/angular-elements.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationStatusService } from '@app/services/operation-status.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */

declare const viewport: any;

describe('HeaderWidget', () => {
    let eventService: EventService;
    let applicationService: ApplicationService;
    let workflowService: WorkflowService;

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

    let operationStub: any = {
        execute: (): Observable<any> => {
            return new BehaviorSubject<object>({});
        },
    };

    let windowScrollServiceStub: any = {
        scrollElementIntoView: (): void => {},
    };

    /**
     * Test host component class
     */
    @Component({
        selector: `host-component`,
        template: `<ubind-header-widget class="page-footer"></ubind-header-widget>
                   <web-form></web-form>`,
    })
    class TestHostComponent {
        @ViewChild(WebFormComponent)
        public webFormComponent: WebFormComponent;
    }

    beforeEach(async () => {
        return TestBed.configureTestingModule({
            declarations: [
                TestHostComponent,
                ...sharedConfig.declarations,
            ],
            providers: [
                { provide: ConfigurationOperation, useValue: {} },
                WorkflowService,
                { provide: CalculationOperation, useValue: {} },
                { provide: AlertService, useValue: alertServiceStub },
                { provide: WindowScrollService, useValue: windowScrollServiceStub },
                { provide: BroadcastService, useValue: broadcastServiceStub },
                { provide: CssProcessorService, useValue: {} },
                { provide: OperationFactory, useValue: operationFactoryStub },
                { provide: WebhookService, useValue: webhookServiceStub },
                { provide: AttachmentOperation, useValue: attachmentOperationStub },
                { provide: PolicyOperation, useValue: policyOperationStub },
                { provide: OperationFactory, useClass: FakeOperationFactory },
                { provide: ToolTipService, useClass: FakeToolTipService },
                { provide: MaskPipe, useClass: MaskPipe },
                FormService,
                ExpressionMethodService,
                ValidationService,
                ApplicationService,
                AttachmentService,
                ConfigService,
                EvaluateService,
                ConfigProcessorService,
                MessageService,
                EventService,
                CalculationService,
                UserService,
                ResumeApplicationService,
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
                NotificationService,
                ConfigurationV2Processor,
                { provide: EncryptionService, useValue: encryptionServiceStub },
                BrowserDetectionService,
                AppEventService,
                { provide: WorkflowStepOperation, useValue: operationStub },
                RevealGroupTrackingService,
                AngularElementsService,
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
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            workflowService.currentNavigation = new WorkflowNavigation(null, { stepName: 'step1' });
            eventService = TestBed.inject<EventService>(EventService);
            eventService.sectionWidgetCompletedViewInitSubject = new BehaviorSubject<void>(null);
            let angularElementsService: AngularElementsService = TestBed.inject(AngularElementsService);
            angularElementsService.registerAngularComponentsAsAngularElements();
        });
    });

    it('removes the header when the next step doesn\'t define one', async () => {
        // Arrange
        let hostFixture: any = TestBed.createComponent(TestHostComponent);
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        workflowService.skipWorkflowAnimations = true;

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        applicationService.currentWorkflowDestination = { stepName: 'step1' };
        viewport.set(1000);
        hostFixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    hostFixture.detectChanges();
                    let step1DebugElement: any = hostFixture.debugElement.query(
                        By.css('.step1-content1'));
                    expect(step1DebugElement).not.toBeNull();
                    let step1Element: HTMLElement = step1DebugElement.nativeElement;
                    expect(step1Element.innerText).toContain('Some content');

                    // Act
                    workflowService.navigateTo({ stepName: 'step2' });
                    hostFixture.detectChanges();

                    // Assert
                    setTimeout(() => {
                        eventService.readyForNextStepSubject.next();
                        hostFixture.detectChanges();
                        let step2DebugElement: any = hostFixture.debugElement.query(
                            By.css('.step1-content1'));
                        expect(step2DebugElement).toBeNull();
                        resolve();
                    }, 50);
                });
        });
    });

});
