/* eslint-disable max-classes-per-file */
import { AfterViewInit, Component, EventEmitter, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ConfigurationOperation } from '@app/operations/configuration.operation';
import { ConfigProcessorService } from '@app/services/config-processor.service';
import { EvaluateService } from '@app/services/evaluate.service';
import { EventService } from '@app/services/event.service';
import { MessageService } from '@app/services/message.service';
import { sharedConfig } from '@app/app.module.shared';
import { CalculationService } from '@app/services/calculation.service';
import { WorkflowService } from '@app/services/workflow.service';
import { WorkflowNavigation } from '@app/models/workflow-navigation';
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
import formConfig from './dynamic-widget.test-form-config.json';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { DynamicWidget } from './dynamic.widget';
import { ToolTipService } from '@app/services/tooltip.service';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { WorkflowDestinationService } from '@app/services/workflow-destination.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { TransitionState } from '@app/models/transition-state.enum';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { MaskPipe } from 'ngx-mask';

/* global spyOn */
/* global jasmine */

/**
 * Fake workflow service class
 */
class FakeWorkflowService {
    public navigate: EventEmitter<any> = new EventEmitter<any>();
    public currentDestination: WorkflowDestination = { stepName: "step1" };
    public currentNavigation: WorkflowNavigation = new WorkflowNavigation(null, { stepName: "step1" });
    public initialised: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(true);
    public actionAborted: EventEmitter<any> = new EventEmitter<any>();
    public actionCompleted: EventEmitter<any> = new EventEmitter<any>();
    public quoteLoadedSubject: Subject<boolean> = new Subject<boolean>();
    public loadedCustomerHasUserSubject: Subject<boolean> = new Subject<boolean>();
    public navigateToSubject: Subject<string> = new Subject<string>();
    public completedNavigationIn(): void { }
}

describe('DynamicWidget', () => {
    let sut: TestWidget;
    let fixture: ComponentFixture<TestWidget>;
    let applicationService: ApplicationService;
    let workflowService: WorkflowService;

    let alertServiceStub: any = {
        updateSubject: new Subject<Alert>(),
        visibleSubject: new Subject<boolean>(),
        alert: (alert: Alert): void => {},
    };

    let broadcastServiceStub: any = {
        on: (): Subject<any> => new Subject<any>(),
    };

    let operationFactoryStub: any = {
        getStatus: (): string => 'success',
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

    /**
     * Test widget component class
     */
    @Component({
        selector: `test-widget`,
        template: `<h1 [@testAnimation]="transitionState" `
            + `(@testAnimation.done)="onAnimationDone($event)">Test Widget</h1>`,
        animations: [
            trigger('testAnimation', [
                state(TransitionState.Left, style({ transform: 'opacity: 0' })),
                state(TransitionState.None, style({ transform: 'opacity: 1' })),
                state(TransitionState.Right, style({ transform: 'opacity: 0' })),
                transition(`* => ${TransitionState.None}`, animate(10)),
                transition(`${TransitionState.None} => *`, animate(10)),
            ]),
        ],
    })
    class TestWidget extends DynamicWidget implements OnInit, AfterViewInit {

        public ngAfterViewInitCalled: boolean = false;

        public constructor(
            workflowService: WorkflowService,
            configService: ConfigService,
            workflowDestinationService: WorkflowDestinationService,
        ) {
            super(workflowService, configService, workflowDestinationService);
        }

        public ngOnInit(): void {
            super.ngOnInit();
        }

        public ngAfterViewInit(): void {
            this.ngAfterViewInitCalled = true;
            super.ngAfterViewInit();
        }

        protected async onChangeStepBeforeAnimateOut(
            previousDestination: WorkflowDestination,
            nextDestination: WorkflowDestination,
        ): Promise<void> {
            // console.log(`TestWidget::onChangeStepBeforeAnimateOut`);
        }

        protected async onChangeStepAfterAnimateOut(
            previousDestination: WorkflowDestination,
            nextDestination: WorkflowDestination,
        ): Promise<void> {
            // console.log(`TestWidget::onChangeStepAfterAnimateOut`);
        }

        protected async onChangeStepBeforeAnimateIn(
            previousDestination: WorkflowDestination,
            nextDestination: WorkflowDestination,
        ): Promise<void> {
            // console.log(`TestWidget::onChangeStepBeforeAnimateIn`);
        }

        protected async onChangeStepAfterAnimateIn(
            previousDestination: WorkflowDestination,
            nextDestination: WorkflowDestination,
        ): Promise<void> {
            // console.log(`TestWidget::onChangeStepAfterAnimateIn`);
        }
    }

    beforeEach(async () => {
        return TestBed.configureTestingModule({
            declarations: [
                TestWidget,
                ...sharedConfig.declarations,
            ],
            providers: [
                ConfigProcessorService,
                MessageService,
                { provide: ConfigurationOperation, useValue: {} },
                EvaluateService,
                EventService,
                CalculationService,
                { provide: WorkflowService, useClass: FakeWorkflowService },
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
                NotificationService,
                ConfigurationV2Processor,
                { provide: EncryptionService, useValue: encryptionServiceStub },
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

    afterEach(() => {
        fixture.destroy();
    });

    it('should call onChangeStepBeforeAnimateIn on first load', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestWidget);
        sut = fixture.componentInstance;
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        let spy: any = spyOn<any>(sut, 'onChangeStepBeforeAnimateIn');
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            // because it uses setTimeout, we must
            setTimeout(() => {
                expect(spy).toHaveBeenCalled();
                resolve();
            }, 10);
        });
    });

    it('should call ngAfterViewInit before onCompletedTransition', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestWidget);
        sut = fixture.componentInstance;
        let onCompletedTransitionSpy: jasmine.Spy = spyOn<any>(sut, 'onCompletedTransition').and.callThrough();
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            // because it uses setTimeout, we must
            setTimeout(async () => {
                fixture.detectChanges();
                expect(sut.ngAfterViewInitCalled).toBeTruthy('ngAfterViewInit should have been called');
                expect(onCompletedTransitionSpy).not.toHaveBeenCalled();

                // Act - run the animation
                fixture.detectChanges();
                await fixture.whenRenderingDone();
                expect(onCompletedTransitionSpy).toHaveBeenCalled();
                resolve();
            }, 10);
        });
    });

    it('should call onChangeStepAfterAnimateIn on first load once the animation completes', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestWidget);
        sut = fixture.componentInstance;
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        let onCompletedTransitionSpy: any = spyOn<any>(sut, 'onCompletedTransition').and.callThrough();
        let afterAnimateInSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateIn');
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            // because it uses setTimeout, we must
            setTimeout(async () => {
                fixture.detectChanges();
                expect(sut.ngAfterViewInitCalled).toBeTruthy('ngAfterViewInit should have been called');
                expect(onCompletedTransitionSpy).not.toHaveBeenCalled();
                expect(afterAnimateInSpy).not.toHaveBeenCalled();

                // Act - run the animation
                fixture.detectChanges();
                await fixture.whenRenderingDone();
                expect(onCompletedTransitionSpy).toHaveBeenCalled();
                expect(afterAnimateInSpy).toHaveBeenCalled();
                resolve();
            }, 10);
        });
    });

    it('should call onChangeStepBeforeAnimateOut when the workflow step changes', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestWidget);
        sut = fixture.componentInstance;
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        let onCompletedTransitionSpy: any = spyOn<any>(sut, 'onCompletedTransition').and.callThrough();
        let afterAnimateInSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateIn');
        let beforeAnimateOutSpy: any = spyOn<any>(sut, 'onChangeStepBeforeAnimateOut');
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            // because it uses setTimeout, we must
            setTimeout(async () => {
                expect(sut.ngAfterViewInitCalled).toBeTruthy('ngAfterViewInit should have been called');
                expect(onCompletedTransitionSpy).not.toHaveBeenCalled();
                expect(afterAnimateInSpy).not.toHaveBeenCalled();
                fixture.detectChanges();
                await fixture.whenRenderingDone();
                expect(onCompletedTransitionSpy).toHaveBeenCalled();
                expect(afterAnimateInSpy).toHaveBeenCalled();
                expect(beforeAnimateOutSpy).not.toHaveBeenCalled();

                // Act
                workflowService.currentNavigation = new WorkflowNavigation(
                    { stepName: 'step1' },
                    { stepName: 'step2' });
                applicationService.currentWorkflowDestination = { stepName: 'step2' };
                workflowService.navigateToSubject.next({ stepName: 'step2' });
                setTimeout(async () => {
                    // Assert
                    expect(beforeAnimateOutSpy).toHaveBeenCalled();
                    resolve();
                }, 20);
            }, 10);
        });
    });

    it('should call onChangeStepAfterAnimateOut when the workflow step changes', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestWidget);
        sut = fixture.componentInstance;
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        let onCompletedTransitionSpy: any = spyOn<any>(sut, 'onCompletedTransition').and.callThrough();
        let afterAnimateInSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateIn');
        let beforeAnimateOutSpy: any = spyOn<any>(sut, 'onChangeStepBeforeAnimateOut');
        let afterAnimateOutSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateOut');
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            // because it uses setTimeout, we must
            setTimeout(async () => {
                fixture.detectChanges();
                expect(sut.ngAfterViewInitCalled).toBeTruthy('ngAfterViewInit should have been called');
                expect(onCompletedTransitionSpy).not.toHaveBeenCalled();
                expect(afterAnimateInSpy).not.toHaveBeenCalled();
                fixture.detectChanges();
                await fixture.whenRenderingDone();
                expect(onCompletedTransitionSpy).toHaveBeenCalled();
                expect(afterAnimateInSpy).toHaveBeenCalled();
                expect(beforeAnimateOutSpy).not.toHaveBeenCalled();

                // Act
                workflowService.currentNavigation = new WorkflowNavigation(
                    { stepName: 'step1' },
                    { stepName: 'step2' });
                applicationService.currentWorkflowDestination = { stepName: 'step2' };
                workflowService.navigateToSubject.next({ stepName: 'step2' });
                setTimeout(async () => {
                    // Assert
                    expect(beforeAnimateOutSpy).toHaveBeenCalled();
                    expect(afterAnimateOutSpy).not.toHaveBeenCalled();
                    onCompletedTransitionSpy.calls.reset();
                    fixture.detectChanges();
                    await fixture.whenRenderingDone();
                    expect(onCompletedTransitionSpy).toHaveBeenCalled();
                    expect(afterAnimateOutSpy).toHaveBeenCalled();
                    resolve();
                }, 20);
            }, 10);
        });
    });

    it('should call onChangeStepBeforeAnimateIn when the workflow step changes', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestWidget);
        sut = fixture.componentInstance;
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        let onCompletedTransitionSpy: any = spyOn<any>(sut, 'onCompletedTransition').and.callThrough();
        let afterAnimateInSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateIn');
        let beforeAnimateOutSpy: any = spyOn<any>(sut, 'onChangeStepBeforeAnimateOut');
        let afterAnimateOutSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateOut');
        let beforeAnimateInSpy: any = spyOn<any>(sut, 'onChangeStepBeforeAnimateIn');
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            // because it uses setTimeout, we must
            setTimeout(async () => {
                expect(sut.ngAfterViewInitCalled).toBeTruthy('ngAfterViewInit should have been called');
                expect(onCompletedTransitionSpy).not.toHaveBeenCalled();
                expect(afterAnimateInSpy).not.toHaveBeenCalled();
                fixture.detectChanges();
                await fixture.whenRenderingDone();
                expect(onCompletedTransitionSpy).toHaveBeenCalled();
                expect(afterAnimateInSpy).toHaveBeenCalled();
                expect(beforeAnimateOutSpy).not.toHaveBeenCalled();

                // Act
                beforeAnimateInSpy.calls.reset();
                workflowService.currentNavigation = new WorkflowNavigation(
                    { stepName: 'step1' },
                    { stepName: 'step2' });
                applicationService.currentWorkflowDestination = { stepName: 'step2' };
                workflowService.navigateToSubject.next({ stepName: 'step2' });
                setTimeout(async () => {
                    // Assert
                    expect(beforeAnimateOutSpy).toHaveBeenCalled();
                    expect(afterAnimateOutSpy).not.toHaveBeenCalled();
                    expect(beforeAnimateInSpy).not.toHaveBeenCalled();
                    onCompletedTransitionSpy.calls.reset();
                    fixture.detectChanges();
                    await fixture.whenRenderingDone();
                    setTimeout(() => {
                        expect(onCompletedTransitionSpy).toHaveBeenCalled();
                        expect(afterAnimateOutSpy).toHaveBeenCalled();
                        expect(beforeAnimateInSpy).toHaveBeenCalled();
                        resolve();
                    }, 10);
                }, 10);
            }, 10);
        });
    });

    it('should call onChangeStepAfterAnimateIn when the workflow step changes', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestWidget);
        sut = fixture.componentInstance;
        applicationService = TestBed.inject<ApplicationService>(ApplicationService);
        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        let onCompletedTransitionSpy: any = spyOn<any>(sut, 'onCompletedTransition').and.callThrough();
        let afterAnimateInSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateIn');
        let beforeAnimateOutSpy: any = spyOn<any>(sut, 'onChangeStepBeforeAnimateOut');
        let afterAnimateOutSpy: any = spyOn<any>(sut, 'onChangeStepAfterAnimateOut');
        let beforeAnimateInSpy: any = spyOn<any>(sut, 'onChangeStepBeforeAnimateIn');
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            // because it uses setTimeout, we must
            setTimeout(async () => {
                expect(sut.ngAfterViewInitCalled).toBeTruthy('ngAfterViewInit should have been called');
                expect(onCompletedTransitionSpy).not.toHaveBeenCalled();
                expect(afterAnimateInSpy).not.toHaveBeenCalled();
                fixture.detectChanges();
                await fixture.whenRenderingDone();
                expect(onCompletedTransitionSpy).toHaveBeenCalled();
                expect(afterAnimateInSpy).toHaveBeenCalled();
                expect(beforeAnimateOutSpy).not.toHaveBeenCalled();

                // Act
                beforeAnimateInSpy.calls.reset();
                afterAnimateInSpy.calls.reset();
                workflowService.currentNavigation = new WorkflowNavigation(
                    { stepName: 'step1' },
                    { stepName: 'step2' });
                applicationService.currentWorkflowDestination = { stepName: 'step2' };
                workflowService.navigateToSubject.next({ stepName: 'step2' });
                setTimeout(async () => {
                    // Assert
                    expect(beforeAnimateOutSpy).toHaveBeenCalled();
                    expect(afterAnimateOutSpy).not.toHaveBeenCalled();
                    expect(beforeAnimateInSpy).not.toHaveBeenCalled();
                    onCompletedTransitionSpy.calls.reset();
                    fixture.detectChanges();
                    await fixture.whenRenderingDone();
                    setTimeout(async () => {
                        expect(onCompletedTransitionSpy).toHaveBeenCalled();
                        expect(afterAnimateOutSpy).toHaveBeenCalled();
                        expect(beforeAnimateInSpy).toHaveBeenCalled();
                        await fixture.whenRenderingDone();
                        expect(afterAnimateInSpy).not.toHaveBeenCalled();
                        fixture.detectChanges();
                        await fixture.whenRenderingDone();
                        expect(afterAnimateInSpy).toHaveBeenCalled();
                        resolve();
                    }, 50);
                }, 50);
            }, 50);
        });
    });
});
