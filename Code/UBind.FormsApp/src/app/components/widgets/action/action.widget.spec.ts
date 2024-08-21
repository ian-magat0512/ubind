/* eslint-disable max-classes-per-file */
/* eslint-disable max-len */
/* eslint-disable no-unused-vars */
// eslint-disable-next-line max-classes-per-file
import { ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA, DebugElement, Directive, ViewChild } from '@angular/core';
import { ComponentFixture, discardPeriodicTasks, fakeAsync, TestBed, tick } from '@angular/core/testing';
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
import formConfig from './action-widget.test-form-config.json';
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
import { ActionWidget } from './action.widget';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { AngularElementsService } from '@app/services/angular-elements.service';
import { ContextEntityService } from '@app/services/context-entity.service';
import { FakeContextEntityService } from '@app/services/fakes/fake-context-entity.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationInstruction } from '@app/models/operation-instruction';
import { ObservableArguments } from '@app/expressions/expression';
import { MaskPipe } from 'ngx-mask';

/* global spyOn */
/* global jasmine */


/**
 * Test Host component class
 */
@Component({
    selector: `app-test-host-component`,
    template: `
        <ubind-action-widget-ng #actionWidget action-name="testAction" data-test-variable="99"></ubind-action-widget-ng>
        `,
})
class TestActionWidgetHostComponent {
    @ViewChild(ActionWidget)
    public actionWidget: ActionWidget;
}

// disabled intermittently failing tests. To be fixed in UB-10955
xdescribe('ActionWidget', () => {
    let sut: ActionWidget;
    let fixture: ComponentFixture<ActionWidget>;
    let eventService: EventService;
    let applicationService: ApplicationService;
    let workflowService: WorkflowService;
    let validationService: ValidationService;
    let expressionDependencies: ExpressionDependencies;
    let fakeOperationFactory: FakeOperationFactory = new FakeOperationFactory();

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
    class TestWebFormHostComponent {
        public ready: boolean = false;
    }

    beforeEach(async () => TestBed.configureTestingModule({
        declarations: [
            TestWebFormHostComponent,
            TestActionWidgetHostComponent,
            ActionWidget,
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
            { provide: OperationFactory, useValue: fakeOperationFactory },
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
            AngularElementsService,
            { provide: ContextEntityService, useClass: FakeContextEntityService },
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
        let angularElementsService: AngularElementsService = TestBed.inject(AngularElementsService);
        angularElementsService.registerAngularComponentsAsAngularElements();
    }));

    afterEach(() => {
        if (fixture) {
            fixture.destroy();
        }
    });

    it('When an action button\'s destination step is changed, clicking the button navigates to the new step',
        fakeAsync(() => {
        // Arrange
            fixture = TestBed.createComponent(ActionWidget);
            sut = fixture.componentInstance;
            sut.location = 'formFooter';
            sut.name = 'enquiry';
            eventService = TestBed.inject<EventService>(EventService);
            let onActionClickSpy: jasmine.Spy = spyOn(sut, 'onActionClick').and.callThrough();
            let response1: any = _.cloneDeep(formConfig);
            response1['status'] = 'success';
            response1.form.workflowConfiguration.step1.actions = {
                enquiry: {
                    primary: true,
                    destinationStep: "step2",
                },
            };
            let response2: any = _.cloneDeep(response1);
            response2.form.workflowConfiguration.step1.actions.enquiry.destinationStep = 'step3';
            let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response1);
            workflowService = TestBed.inject<WorkflowService>(WorkflowService);
            workflowService.initialise();
            fixture.detectChanges();
            tick();

            let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
            expect(debugElement != null)
                .toBeTruthy('The action button should be there from the start.');

            // Act
            configProcessorService.onConfigurationResponse(response2);
            fixture.detectChanges();
            tick();

            // Assert
            debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
            debugElement.triggerEventHandler('click', null);
            tick();
            expect(onActionClickSpy).toHaveBeenCalled();
            expect(workflowService.currentNavigation.to.stepName).toBe('step3');
            discardPeriodicTasks();
        }));

    it('When an action button\'s destination step expression is changed, clicking the button navigates '
        + 'to the new step', fakeAsync(() => {
        // Arrange
        fixture = TestBed.createComponent(ActionWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        sut.name = 'enquiry';
        eventService = TestBed.inject<EventService>(EventService);
        let onActionClickSpy: jasmine.Spy = spyOn(sut, 'onActionClick').and.callThrough();
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primary: true,
                destinationStepExpression: "'step' + '2'",
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.destinationStepExpression = "'step' + '3'";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement != null)
            .toBeTruthy('The action button should be there from the start.');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();

        // Assert
        debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        debugElement.triggerEventHandler('click', null);
        tick();
        expect(onActionClickSpy).toHaveBeenCalled();
        expect(workflowService.currentNavigation.to.stepName).toBe('step3');
        discardPeriodicTasks();
    }));

    /**
     * We cannot include this test in normal test runs. It needs to be run individually. For a full explanation, see:
     * https://stackoverflow.com/questions/73849002/unit-tests-of-angular-elements-registered-with-the-customelementregistry-as-a-h
     */
    xit('when an action button\'s requiresValidQuestionSets property is changed to add a question set to it, '
        + 'it requires the new question set to also be valid before navigating', () => {
        // Arrange
        let hostFixture: ComponentFixture<TestWebFormHostComponent> = TestBed.createComponent(TestWebFormHostComponent);
        let hostComponent: TestWebFormHostComponent = hostFixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles = [{
            heading: 'My article',
            elements: [
                {
                    type: "questions",
                    name: "ratingPrimary",
                },
                {
                    type: "questions",
                    name: "ratingSecondary",
                },
            ],
        }];
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primary: true,
                destinationStep: "step2",
                requiresValidQuestionSets: ['ratingPrimary'],
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.requiresValidQuestionSets.push('ratingSecondary');
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let formService: FormService = TestBed.inject<FormService>(FormService);
        spyOn(formService, 'scrollToFirstInvalidField').and.returnValue(true);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        hostComponent.ready = true;
        hostFixture.detectChanges();
        let original: (widgetPosition: any) => Array<string>
            = (<any>workflowService).getQuestionSetPathsRequiredToBeValid;
        let paths: Array<string>;
        let fnSpy: jasmine.Spy = spyOn<any>(workflowService, "getQuestionSetPathsRequiredToBeValid")
            .and.callFake((widgetPosition: any) => {
                paths = original.bind(workflowService)(widgetPosition);
                return paths;
            });

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = hostFixture.debugElement.query(By.css('#formFooter-enquiry'));
                    expect(debugElement != null)
                        .toBeTruthy('The action button should be there from the start.');

                    let debugElement1: DebugElement = hostFixture.debugElement.query(By.css('#test1'));
                    expect(debugElement1 != null).toBeTruthy('#test1 should have been rendered');
                    let fieldEl1: HTMLInputElement = debugElement1.nativeElement;
                    fieldEl1.value = 'something';
                    fieldEl1.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl1.dispatchEvent(changeEvent);
                    hostFixture.detectChanges();

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    hostFixture.detectChanges();
                    await hostFixture.whenStable();

                    // Assert
                    debugElement = hostFixture.debugElement.query(By.css('#formFooter-enquiry'));
                    debugElement.triggerEventHandler('click', null);
                    hostFixture.detectChanges();
                    await hostFixture.whenStable();
                    expect(paths).toContain('ratingSecondary');
                    resolve();
                });
        });
    });

    /**
     * We cannot include this test in normal test runs. It needs to be run individually. For a full explanation, see:
     * https://stackoverflow.com/questions/73849002/unit-tests-of-angular-elements-registered-with-the-customelementregistry-as-a-h
     */
    xit('when an action button\'s requiresValidQuestionSetsExpression property is changed to add a question set to it, '
        + 'it requires the new question set to also be valid before navigating', () => {
        // Arrange
        let hostFixture: ComponentFixture<TestWebFormHostComponent> = TestBed.createComponent(TestWebFormHostComponent);
        let hostComponent: TestWebFormHostComponent = hostFixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles = [{
            heading: 'My article',
            elements: [
                {
                    type: "questions",
                    name: "ratingPrimary",
                },
                {
                    type: "questions",
                    name: "ratingSecondary",
                },
            ],
        }];
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primary: true,
                destinationStep: "step2",
                requiresValidQuestionSetsExpression: "['ratingPrimary']",
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.requiresValidQuestionSetsExpression
            = "getVisibleQuestionSets()";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let formService: FormService = TestBed.inject<FormService>(FormService);
        spyOn(formService, 'scrollToFirstInvalidField').and.returnValue(true);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        hostComponent.ready = true;
        hostFixture.detectChanges();
        let original: (widgetPosition: any) => Array<string>
            = (<any>workflowService).getQuestionSetPathsRequiredToBeValid;
        let paths: Array<string>;
        let fnSpy: jasmine.Spy = spyOn<any>(workflowService, "getQuestionSetPathsRequiredToBeValid")
            .and.callFake((widgetPosition: any) => {
                paths = original.bind(workflowService)(widgetPosition);
                return paths;
            });

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = hostFixture.debugElement.query(By.css('#formFooter-enquiry'));
                    expect(debugElement != null)
                        .toBeTruthy('The action button should be there from the start.');

                    let debugElement1: DebugElement = hostFixture.debugElement.query(By.css('#test1'));
                    expect(debugElement1 != null).toBeTruthy('#test1 should have been rendered');
                    let fieldEl1: HTMLInputElement = debugElement1.nativeElement;
                    fieldEl1.value = 'something';
                    fieldEl1.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl1.dispatchEvent(changeEvent);
                    hostFixture.detectChanges();

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    hostFixture.detectChanges();
                    await hostFixture.whenStable();

                    // Assert
                    debugElement = hostFixture.debugElement.query(By.css('#formFooter-enquiry'));
                    debugElement.triggerEventHandler('click', null);
                    hostFixture.detectChanges();
                    setTimeout(async () => {
                        await hostFixture.whenStable();
                        expect(paths).toContain('ratingSecondary');
                        resolve();
                    }, 1000);
                });
        });
    });

    it('when an action button with no operations listed has the formUpdate operation added, when it\'s clicked '
        + 'it now calls the formUpdate operation ', fakeAsync(() => {
        // Arrange
        fixture = TestBed.createComponent(ActionWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        sut.name = 'enquiry';
        eventService = TestBed.inject<EventService>(EventService);
        let onActionClickSpy: jasmine.Spy = spyOn(sut, 'onActionClick').and.callThrough();
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primary: true,
                destinationStep: "step2",
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.operations = ['formUpdate'];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let operationInstructionService: OperationInstructionService
            = TestBed.inject<OperationInstructionService>(OperationInstructionService);
        let lastInstructions: Array<OperationInstruction>;
        spyOn(operationInstructionService, 'executeAll').and
            .callFake(async (instructions: Array<OperationInstruction>) => {
                lastInstructions = instructions;
            });
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement != null)
            .toBeTruthy('The action button should be there from the start.');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();

        // Assert
        debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        debugElement.triggerEventHandler('click', null);
        tick();
        expect(onActionClickSpy).toHaveBeenCalled();
        const index: number = lastInstructions.findIndex((instruction: OperationInstruction) => {
            return instruction.name == 'formUpdate';
        });
        expect(index).not.toBe(-1);
        discardPeriodicTasks();
    }));

    it('when an action button has the formUpdate operation removed, when it\'s clicked it no longer calls the '
        + 'formUpdate operation', fakeAsync(() => {
        // Arrange
        fixture = TestBed.createComponent(ActionWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        sut.name = 'enquiry';
        eventService = TestBed.inject<EventService>(EventService);
        let onActionClickSpy: jasmine.Spy = spyOn(sut, 'onActionClick').and.callThrough();
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primary: true,
                destinationStep: "step2",
                operations: ['formUpdate', 'customer'],
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.operations = ['customer'];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        let operationInstructionService: OperationInstructionService
            = TestBed.inject<OperationInstructionService>(OperationInstructionService);
        let lastInstructions: Array<OperationInstruction>;
        spyOn(operationInstructionService, 'executeAll').and
            .callFake(async (instructions: Array<OperationInstruction>) => {
                lastInstructions = instructions;
            });
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement != null)
            .toBeTruthy('The action button should be there from the start.');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();

        // Assert
        debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        debugElement.triggerEventHandler('click', null);
        tick();
        expect(onActionClickSpy).toHaveBeenCalled();
        const index: number = lastInstructions.findIndex((instruction: OperationInstruction) => {
            return instruction.name == 'formUpdate';
        });
        expect(index).toBe(-1);
        discardPeriodicTasks();
    }));

    it('When an action button\'s primaryExpression is changed to to resolve to false, its appearance becomes '
        + 'subdued ', () => {
        // Arrange
        fixture = TestBed.createComponent(ActionWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        sut.name = 'enquiry';
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primaryExpression: 'true',
                destinationStep: "step2",
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.primaryExpression = 'false';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement != null)
            .toBeTruthy('The action button should be there from the start.');
        let buttonEl: HTMLButtonElement = debugElement.nativeElement;
        expect(buttonEl.classList.contains('btn-primary'))
            .toBeTruthy('the btn-primary css class should be present from the start');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();

        // Assert
        debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        buttonEl = debugElement.nativeElement;
        expect(buttonEl.classList.contains('btn-primary'))
            .toBeFalsy('the btn-primary css class should not be present');
        expect(buttonEl.classList.contains('btn-secondary'))
            .toBeTruthy('the btn-secondary css class should be present');
    });

    it('When an action button\'s primaryExpression is changed to to resolve to true, its appearance becomes '
        + 'emboldened ', () => {
        // Arrange
        fixture = TestBed.createComponent(ActionWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        sut.name = 'enquiry';
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primaryExpression: 'false',
                destinationStep: "step2",
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.primaryExpression = 'true';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement != null)
            .toBeTruthy('The action button should be there from the start.');
        let buttonEl: HTMLButtonElement = debugElement.nativeElement;
        expect(buttonEl.classList.contains('btn-primary'))
            .toBeFalsy('the btn-primary css class should not be present from the start');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();

        // Assert
        debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        buttonEl = debugElement.nativeElement;
        expect(buttonEl.classList.contains('btn-primary'))
            .toBeTruthy('the btn-primary css class should be present');
        expect(buttonEl.classList.contains('btn-secondary'))
            .toBeFalsy('the btn-secondary css class should not be present');
    });

    it('When an action button\'s changeApplicationMode is set, clicking the button changes the '
        + 'application mode', fakeAsync(() => {
        // Arrange
        fixture = TestBed.createComponent(ActionWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        sut.name = 'enquiry';
        eventService = TestBed.inject<EventService>(EventService);
        let onActionClickSpy: jasmine.Spy = spyOn(sut, 'onActionClick').and.callThrough();
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primaryExpression: 'false',
                destinationStep: "step1",
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.changeApplicationModeTo = 'endorse';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement != null)
            .toBeTruthy('The action button should be there from the start.');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();
        debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        debugElement.triggerEventHandler('click', null);
        tick();

        // Assert
        expect(onActionClickSpy).toHaveBeenCalled();
        workflowService.completedNavigationOut();
        expect(applicationService.mode).toBe('endorse');
        discardPeriodicTasks();
    }));

    it('when an action button\'s hiddenExpression is set to evaluate to true, it disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(ActionWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        sut.name = 'enquiry';
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primaryExpression: 'false',
                destinationStep: "step1",
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.hiddenExpression = 'true';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement != null)
            .toBeTruthy('The action button should be there from the start.');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();

        // Assert
        debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement == null)
            .toBeTruthy('The action button should have become hidden');
    });

    it('when an action button\'s hiddenExpression is set to evaluate to false, it appears', () => {
        // Arrange
        fixture = TestBed.createComponent(ActionWidget);
        sut = fixture.componentInstance;
        sut.location = 'formFooter';
        sut.name = 'enquiry';
        eventService = TestBed.inject<EventService>(EventService);
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            enquiry: {
                primaryExpression: 'false',
                destinationStep: "step1",
                hiddenExpression: "'true'",
            },
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.actions.enquiry.hiddenExpression = 'false';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        fixture.detectChanges();

        let debugElement: DebugElement
            = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement == null)
            .toBeTruthy('The action button should be hidden from the start.');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        fixture.detectChanges();

        // Assert
        debugElement = fixture.debugElement.query(By.css('#formFooter-enquiry'));
        expect(debugElement != null)
            .toBeTruthy('The action button should have appeared.');
    });

    it('an action buttons data attributes are read and added to the observable arguments for expressions', fakeAsync(() => {
        // Arrange
        let hostFixture: ComponentFixture<TestActionWidgetHostComponent>;
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.actions = {
            testAction: {
                primaryExpression: 'false',
                destinationStep: "step1",
            },
        };
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        workflowService = TestBed.inject<WorkflowService>(WorkflowService);
        workflowService.initialise();
        hostFixture = TestBed.createComponent(TestActionWidgetHostComponent);

        // Act
        hostFixture.detectChanges();

        // Wait for ngAfterViewInit to be called
        tick(20);
        hostFixture.detectChanges(); // Rebind after ngAfterViewInit        

        // Assert
        const actionWidget: ActionWidget = hostFixture.componentInstance.actionWidget;
        expect(actionWidget).not.toBeNull();
        const expressionObservableArguments: ObservableArguments = (<any>actionWidget).expressionObservableArguments;
        const observable: Observable<any> = expressionObservableArguments.testVariable;
        observable.subscribe((value: any) => {
            expect(value).toBe('99');
        });
        discardPeriodicTasks();
    }));

});
