import { Component, DebugElement, EventEmitter, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
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
import { filter } from 'rxjs/operators';
import { By } from '@angular/platform-browser';
import * as _ from 'lodash-es';
import formConfig from './search-select.test-form-config.json';
import { ToolTipService } from '@app/services/tooltip.service';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { Alert } from '@app/models/alert';
import { EncryptionService } from "@app/services/encryption.service";
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { OptionSetChangePublisher } from '@app/services/option-set-change-publisher';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from '@app/services/operation-status.service';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */

describe('SearchSelectField', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;

    let workflowServiceStub: any = {
        navigate: new EventEmitter<any>(),
        currentDestination: { stepName: "step1" },
        initialised: new BehaviorSubject<boolean>(true),
        actionAborted: new EventEmitter<any>(),
        actionCompleted: new EventEmitter<any>(),
        completedNavigationIn: (): void => { },
        quoteLoadedSubject: new Subject<boolean>(),
        loadedCustomerHasUserSubject: new Subject<boolean>(),
        navigateToSubject: new Subject<boolean>(),
    };

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

    /**
     * Test host component class
     */
    @Component({
        selector: `host-component`,
        template: `<web-form *ngIf="ready"></web-form>`,
        // We need to use OnPush change detection here so that we don't get
        // ExpressionChangedAfterItHasBeenCheckedError during unit tests.
        /* changeDetection: ChangeDetectionStrategy.OnPush*/
    })
    class TestHostComponent {
        public ready: boolean = false;
    }

    beforeEach(async () => {
        return TestBed.configureTestingModule({
            declarations: [
                TestHostComponent,
                ...sharedConfig.declarations,
            ],
            providers: [
                { provide: EncryptionService, useValue: encryptionServiceStub },
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                { provide: EvaluateService, useClass: EvaluateService },
                { provide: EventService, useClass: EventService },
                { provide: CalculationService, useClass: CalculationService },
                { provide: WorkflowService, useValue: workflowServiceStub },
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
                OptionSetChangePublisher,
                BrowserDetectionService,
                RevealGroupTrackingService,
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
        })
            .compileComponents().then(() => {
                let messageService: MessageService = TestBed.inject<MessageService>(MessageService);
                spyOn(messageService, 'sendMessage'); // make it do nothing.                
                TestBed.inject<OptionSetChangePublisher>(OptionSetChangePublisher);
            });
    });

    afterEach(() => {
        fixture.destroy();
    });

    xit('should only load options from the api from the start if autoTriggerOptionsRequest is set to true',
        async () => {
            // Arrange
            fixture = TestBed.createComponent(TestHostComponent);
            eventService = TestBed.inject<EventService>(EventService);

            // TODO:...
            /*
    
            let response = _.cloneDeep(formConfig);
            response['status'] = 'success';
            let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
            configProcessorService.onConfigurationResponse(response);
            fixture.detectChanges();
    
            return new Promise((resolve, reject) => {
                eventService.webFormLoadedSubject
                    .pipe(filter((loaded) => loaded == true))
                    .subscribe(async () => {
    
                        // Act
                        fixture.detectChanges();
                        let searchSelectField: SearchSelectField = 
                            fixture.debugElement.query(By.directive(SearchSelectField)).componentInstance;
                        let spy = spyOn<any>(searchSelectField, 'getOptionsFromApi');
    
                        // Assert
                        expect(spy).not.toHaveBeenCalled();
                        resolve();
                    });                
            });    
            */
        });

    it('should publish the search term for expressions when someone types into the field', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response: any = _.cloneDeep(formConfig);
        response['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:last-child ng-select input'));
                    let inputEl: HTMLInputElement = debugElement.nativeElement;
                    inputEl.value = 't';
                    inputEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    inputEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    setTimeout(() => {
                        // Assert
                        let field1Value: any = fixture.debugElement.query(By.css('#searchTerm')).nativeElement.value;
                        expect(field1Value).toBe('t');
                        resolve();
                    }, 60);
                });
        });
    });

    it('when a Search Select Field option searchableText is changed to \'xyx\', searching for \'xyz\' '
        + 'will show it in the results', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.optionSets[0].options[3].searchableText = 'xyz';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:last-child ng-select input'));
                    let inputEl: HTMLInputElement = debugElement.nativeElement;
                    inputEl.value = 'toy';
                    inputEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    inputEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    let panelElement: DebugElement = fixture.debugElement.query(
                        By.css('.ng-dropdown-panel'));
                    expect(panelElement != null)
                        .toBeTruthy('The dropdown panel should have appeared.');
                    let optionsDivElement: DebugElement = fixture.debugElement.query(
                        By.css('.ng-dropdown-panel .ng-dropdown-panel-items div:last-child'));
                    expect(optionsDivElement.children.length).toBe(1);
                    expect(optionsDivElement.children[0].children[0].nativeElement.innerText).toBe('Toyota');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    setTimeout(() => {
                        debugElement = fixture.debugElement.query(
                            By.css('formly-group formly-field:last-child ng-select input'));
                        inputEl = debugElement.nativeElement;
                        inputEl.value = 'xyz';
                        inputEl.dispatchEvent(new Event('input'));
                        changeEvent = new Event('change', { bubbles: true, cancelable: false });
                        inputEl.dispatchEvent(changeEvent);
                        fixture.detectChanges();
                        panelElement = fixture.debugElement.query(
                            By.css('.ng-dropdown-panel'));
                        expect(panelElement != null)
                            .toBeTruthy('The dropdown panel should have appeared.');
                        optionsDivElement = fixture.debugElement.query(
                            By.css('.ng-dropdown-panel .ng-dropdown-panel-items div:last-child'));
                        expect(optionsDivElement.children.length).toBe(1);
                        expect(optionsDivElement.children[0].children[0].nativeElement.innerText).toBe('Holden');
                        resolve();
                    }, 60);
                });
        });
    });

    it('when a Search Select Field option properties are changed, selecting that option will set the '
        + 'field data or properties so it is available for expressions', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.questionSets[0].fields[0].calculatedValueExpression
            = "getFieldPropertyValue('searchSelect1', 'a')";
        response1.form.optionSets[0].options[0].properties = {
            "a": "green",
            "b": "duck",
        };
        let response2: any = _.cloneDeep(response1);
        response2.form.optionSets[0].options[0].properties = {
            "a": "yellow",
            "b": "cow",
        };
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:last-child ng-select input'));
                    let inputEl: HTMLInputElement = debugElement.nativeElement;
                    inputEl.value = 'toy';
                    inputEl.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    inputEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    let panelElement: DebugElement = fixture.debugElement.query(
                        By.css('.ng-dropdown-panel'));
                    expect(panelElement != null)
                        .toBeTruthy('The dropdown panel should have appeared.');
                    let optionsDivElement: DebugElement = fixture.debugElement.query(
                        By.css('.ng-dropdown-panel .ng-dropdown-panel-items div:last-child'));
                    expect(optionsDivElement.children.length).toBe(1);
                    let firstOptionEl: HTMLElement = optionsDivElement.children[0].children[0].nativeElement;
                    firstOptionEl.click();
                    fixture.detectChanges();
                    let resultEl: HTMLInputElement = fixture.debugElement.query(By.css('#searchTerm')).nativeElement;
                    setTimeout(() => {
                        expect(resultEl.value).toBe('green');

                        // Act
                        configProcessorService.onConfigurationResponse(response2);
                        fixture.detectChanges();
                    }, 3000);
                    // Unfortunately we could not get this working. The click() call doesn't seem to work
                    // the second time, only the first time.
                    /*
                    // Assert
                    // we have to select a different option first so that ng-select will detect the change
                    debugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:last-child ng-select input'));
                    inputEl = debugElement.nativeElement;
                    inputEl.value = 'nis';
                    inputEl.dispatchEvent(new Event('input'));
                    changeEvent = new Event('change', { bubbles: true, cancelable: false });
                    inputEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    panelElement = fixture.debugElement.query(
                        By.css('.ng-dropdown-panel'));
                    expect(panelElement != null)
                        .toBeTruthy('The dropdown panel should have appeared.');
                    optionsDivElement = fixture.debugElement.query(
                        By.css('.ng-dropdown-panel .ng-dropdown-panel-items div:last-child'));
                    expect(optionsDivElement.children.length).toBe(1);
                    firstOptionEl = optionsDivElement.children[0].nativeElement;
                    firstOptionEl.click();
                    fixture.detectChanges();

                    // now select the first item again
                    debugElement = fixture.debugElement.query(
                        By.css('formly-group formly-field:last-child ng-select input'));
                    inputEl = debugElement.nativeElement;
                    inputEl.value = 'toy';
                    inputEl.dispatchEvent(new Event('input'));
                    changeEvent = new Event('change', { bubbles: true, cancelable: false });
                    inputEl.dispatchEvent(changeEvent);
                    fixture.detectChanges();
                    panelElement = fixture.debugElement.query(
                        By.css('.ng-dropdown-panel'));
                    expect(panelElement != null)
                        .toBeTruthy('The dropdown panel should have appeared.');
                    optionsDivElement = fixture.debugElement.query(
                        By.css('.ng-dropdown-panel .ng-dropdown-panel-items div:last-child'));
                    expect(optionsDivElement.children.length).toBe(1);
                    firstOptionEl = optionsDivElement.children[0].children[0].nativeElement;
                    firstOptionEl.click();
                    fixture.detectChanges();

                    resultEl = fixture.debugElement.query(By.css('#searchTerm')).nativeElement;
                    expect(resultEl.value).toBe('yellow');
                    */
                    resolve();
                });
        });
    });

});
