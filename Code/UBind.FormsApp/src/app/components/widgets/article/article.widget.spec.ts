/* eslint-disable max-classes-per-file */
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
import { filter } from 'rxjs/operators';
import { By } from '@angular/platform-browser';
import * as _ from 'lodash-es';
import formConfig1 from './article-widget.test-form-config1.json';
import formConfig2 from './article-widget.test-form-config2.json';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { ToolTipService } from '@app/services/tooltip.service';
import { WorkflowDestination } from '@app/models/workflow-destination';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationStatusService } from '@app/services/operation-status.service';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */

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

describe('ArticleWidget', () => {
    let sut: TestHostComponent;
    let fixture: ComponentFixture<TestHostComponent>;
    let eventService: EventService;
    let validationService: ValidationService;

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
        }).compileComponents().then(() => {
            let messageService: MessageService = TestBed.inject<MessageService>(MessageService);
            spyOn(messageService, 'sendMessage'); // make it do nothing.
        });
    });

    afterEach(() => {
        fixture.destroy();
    });

    it('heading should appear on the page when set', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig1);
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

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('article-widget > article > h2.article-heading'));
                    expect(debugElement).not.toBeNull("Could not find the article widget heading element.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('My step 1 first article heading');
                    resolve();
                });
        });
    });

    it('heading should not appear on the page when not set', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig1);
        delete response.form.workflowConfiguration.step1.articles[0].heading;
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

                    // Assert
                    let debugElement: DebugElement =
                        fixture.debugElement.query(By.css('article-widget > article > h2'));
                    expect(debugElement).toBeNull("h2 element was found when it shouldn't have been rendered.");
                    resolve();
                });
        });
    });

    it('text should appear on the page when set', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig1);
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

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('article-widget > article > p.article-text'));
                    expect(debugElement).not.toBeNull("Could not find the article widget text element.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('My step 1 first article text');
                    resolve();
                });
        });
    });

    it('text should not appear on the page when not set', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig1);
        delete response.form.workflowConfiguration.step1.articles[0].text;
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

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('article-widget > article > p.article-text'));
                    expect(debugElement).toBeNull(
                        "p.article-text element was found when it shouldn't have been rendered.");
                    resolve();
                });
        });
    });

    it('should evaluate embedded expressions within the heading', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig1);
        response.form.textElements[0].text
            = "I called your mum %{ 5 + 5 }% times";
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
                    fixture.detectChanges();

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('article-widget > article > h2.article-heading'));
                    expect(debugElement).not.toBeNull("Could not find the article widget heading element.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('I called your mum 10 times');
                    resolve();
                });
        });
    });

    it('should evaluate embedded expressions within the text', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        // Act
        let response: any = _.cloneDeep(formConfig1);
        response.form.textElements[1].text
            = "I called your mum %{ 5 + 5 }% times";
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
                    fixture.detectChanges();

                    // Assert
                    let debugElement: DebugElement = fixture.debugElement.query(
                        By.css('article-widget > article > p.article-text'));
                    expect(debugElement).not.toBeNull("Could not find the article widget message element.");
                    let element: HTMLElement = debugElement.nativeElement;
                    expect(element.innerText).toBe('I called your mum 10 times');
                    resolve();
                });
        });
    });

    // This test triggers causes ExpressionChangedAfterItHasBeenCheckedError due to a bug in Reactive Forms
    // See: https://github.com/angular/angular/issues/23657
    // Weirdly, it only happens under Karma unit tests.
    // The issue is related to validation. If field validation is disabled until after the initialisation has finished
    // then it doesn't trigger the error. To get around the issue, in this test we manually control the validation.
    it('should show itself when it\'s hidden expression uses questionSetsAreValid', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response: any = _.cloneDeep(formConfig2);
        response['status'] = 'success';
        response.form.workflowConfiguration.step1.articles[1]['hidden']
            = "questionSetsAreValid(['step1QuestionSet1']) != true";
        // response.workflow.step1.articles[1]['hidden'] = "true";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response);
        sut.ready = true;
        fixture.detectChanges();

        // make the field invalid:
        validationReturnValue = { required: true };
        let field: any = fixture.debugElement.query(By.css('#step1Field1')).nativeElement;
        field.dispatchEvent(new Event('input'));
        let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
        field.dispatchEvent(changeEvent);
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let questionSet2DebugElement: DebugElement
                        = fixture.debugElement.query(By.css('questions-widget.step1QuestionSet2'));
                    expect(questionSet2DebugElement == null)
                        .toBeTruthy('step1QuestionSet2 should not be rendered yet.');

                    // Act
                    let field: any = fixture.debugElement.query(By.css('#step1Field1')).nativeElement;
                    field.value = 'Johnny Bravo';
                    validationReturnValue = null;
                    field.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    field.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    // Assert
                    questionSet2DebugElement
                        = fixture.debugElement.query(By.css('questions-widget.step1QuestionSet2'));
                    expect(questionSet2DebugElement != null).toBeTruthy(
                        'step1QuestionSet2 should be rendered since step1QuestionSet1 is valid.');
                    resolve();
                });
        });
    });

    it('when an article heading is changed, it updates', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "heading": "A second article heading",
            "text": "My second article text",
            "cssClasses": [ 'my-second-article' ],
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.articles[1].heading = "My updated heading";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let headingDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article h2.article-heading'));
                    expect(headingDebugElement != null)
                        .toBeTruthy('The second article should have been rendered');
                    let el: HTMLElement = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('A second article heading');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    headingDebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article h2.article-heading'));
                    el = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('My updated heading');
                    resolve();
                });
        });
    });

    it('when an article heading is removed, it disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "heading": "A second article heading",
            "text": "My second article text",
            "cssClasses": [ 'my-second-article' ],
        });
        let response2: any = _.cloneDeep(response1);
        delete response2.form.workflowConfiguration.step1.articles[1].heading;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let headingDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article h2.article-heading'));
                    expect(headingDebugElement != null)
                        .toBeTruthy('The second article should have been rendered');
                    let el: HTMLElement = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('A second article heading');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    headingDebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article h2.article-heading'));
                    expect(headingDebugElement == null).toBeTruthy('The heading should have been removed.');
                    resolve();
                });
        });
    });

    it('when an article heading is added, it appears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "text": "My second article text",
            "cssClasses": [ 'my-second-article' ],
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.articles[1].heading = 'My added heading';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let headingDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article h2.article-heading'));
                    expect(headingDebugElement == null).toBeTruthy('The heading should not exist yet.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    headingDebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article h2.article-heading'));
                    expect(headingDebugElement != null).toBeTruthy('The heading should have been added.');
                    resolve();
                });
        });
    });

    it('when the text element for an article heading is changed, it updates', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "heading": "myHeading",
            "cssClasses": [ 'my-second-article' ],
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.textElements[0].text = 'My updated text element heading';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let headingDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article h2.article-heading'));
                    expect(headingDebugElement != null)
                        .toBeTruthy('The second article should have been rendered');
                    let el: HTMLElement = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('My step 1 first article heading');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    headingDebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article h2.article-heading'));
                    el = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('My updated text element heading');
                    resolve();
                });
        });
    });

    it('when an article\'s text is changed, it updates', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "heading": "A second article heading",
            "text": "My second article text",
            "cssClasses": [ 'my-second-article' ],
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.articles[1].text = "My updated text";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let headingDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article p.article-text'));
                    expect(headingDebugElement != null)
                        .toBeTruthy('The second article should have been rendered');
                    let el: HTMLElement = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('My second article text');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    headingDebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article p.article-text'));
                    el = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('My updated text');
                    resolve();
                });
        });
    });

    it('when an article\'s text is removed, it disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "heading": "A second article heading",
            "text": "My second article text",
            "cssClasses": [ 'my-second-article' ],
        });
        let response2: any = _.cloneDeep(response1);
        delete response2.form.workflowConfiguration.step1.articles[1].text;
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let headingDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article p.article-text'));
                    expect(headingDebugElement != null)
                        .toBeTruthy('The second article should have been rendered');
                    let el: HTMLElement = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('My second article text');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    headingDebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article p.article-text'));
                    expect(headingDebugElement == null).toBeTruthy('The text should have been removed.');
                    resolve();
                });
        });
    });

    it('when an article\'s text is added, it appears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "heading": "My second article heading",
            "cssClasses": [ 'my-second-article' ],
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.articles[1].text = 'My added text';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let headingDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article p.article-text'));
                    expect(headingDebugElement == null).toBeTruthy('The text should not exist yet.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    headingDebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article p.article-text'));
                    expect(headingDebugElement != null).toBeTruthy('The text should have been added.');
                    resolve();
                });
        });
    });

    it('when the text element for an article\'s text is changed, it updates', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "text": "myText",
            "cssClasses": [ 'my-second-article' ],
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.textElements[1].text = 'My updated text element text';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let headingDebugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article p.article-text'));
                    expect(headingDebugElement != null)
                        .toBeTruthy('The second article should have been rendered');
                    let el: HTMLElement = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('My step 1 first article text');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    headingDebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article p.article-text'));
                    el = headingDebugElement.nativeElement;
                    expect(el.innerText).toBe('My updated text element text');
                    resolve();
                });
        });
    });

    it('when an article element of type "content" is added, it appears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "text": "myText",
            "cssClasses": [ 'my-second-article' ],
            "elements" : [],
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.workflowConfiguration.step1.articles[1].elements.push({
            "type": "content",
            "name": "My additional content",
        });
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article content-widget'));
                    expect(debugElement == null)
                        .toBeTruthy('There should be no content widget in the second article yet.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(By.css('article.my-second-article content-widget .content-block'));
                    let el: HTMLElement = debugElement.nativeElement;
                    expect(el.innerText).toBe('My additional content');
                    resolve();
                });
        });
    });

    it('when an article element of type "content" is removed, it disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "text": "myText",
            "cssClasses": [ 'my-second-article' ],
            "elements" : [
                {
                    "type": "content",
                    "name": "My additional content",
                },
            ],
        });
        let response2: any = _.cloneDeep(formConfig1);
        response2['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article content-widget'));
                    expect(debugElement != null)
                        .toBeTruthy('There be an existing content widget');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(By.css('article.my-second-article content-widget'));
                    expect(debugElement == null).toBeTruthy('The content article element should have been removed.');
                    resolve();
                });
        });
    });

    it('when the text element for an article element of type "content" is changed, it updates', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles.push({
            "text": "myText",
            "cssClasses": [ 'my-second-article' ],
            "elements" : [
                {
                    "type": "content",
                    "name": "myContent",
                },
            ],
        });
        response1.form.textElements.push({
            category: "Workflow",
            subcategory: "Step 1",
            name: "My Content",
            text: "My text element content",
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.textElements[2].text = 'My updated text element content';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-second-article content-widget .content-block'));
                    expect(debugElement != null)
                        .toBeTruthy('There be an existing content widget');
                    let el: HTMLElement = debugElement.nativeElement;
                    expect(el.innerText).toBe('My text element content');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(By.css('article.my-second-article content-widget .content-block'));
                    el = debugElement.nativeElement;
                    expect(el.innerText).toBe('My updated text element content');
                    resolve();
                });
        });
    });

    it('when an article element of type "questions" is added, it appears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles[0].elements = [];
        let response2: any = _.cloneDeep(formConfig1);
        response2['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet'));
                    expect(debugElement == null)
                        .toBeTruthy('There should be no questions widget yet');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                        = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet'));
                    expect(debugElement != null)
                        .toBeTruthy('The question set should have been added');
                    resolve();
                });
        });
    });

    it('when an article element of type "questions" is removed, it disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig1);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(formConfig1);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.articles[0].elements = [];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet'));
                    expect(debugElement != null)
                        .toBeTruthy('There should be a questions widget to start with.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement
                    = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet'));
                    expect(debugElement == null)
                        .toBeTruthy('The question set should have been removed.');
                    resolve();
                });
        });
    });

    it('when an article element of type "questions" has a hiddenExpression added, it starts hiding '
        + 'when the hiddenExpression evaluates to true', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(formConfig2);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.articles[1].elements[0].hiddenExpression = "step1Field1 == 'hide'";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet2'));
                    expect(debugElement != null)
                        .toBeTruthy('The question set step1QuestionSet2 should have been rendered.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    let debugElement1: DebugElement = fixture.debugElement.query(By.css('#step1Field1'));
                    expect(debugElement1 != null).toBeTruthy('step1Field1 should have been rendered');
                    let fieldEl1: HTMLInputElement = debugElement1.nativeElement;
                    fieldEl1.value = 'hide';
                    fieldEl1.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl1.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet2'));
                    expect(debugElement == null)
                        .toBeTruthy('The question set should have been hidden.');
                    resolve();
                });
        });
    });

    it('when an article element of type "questions" has its hiddenExpression removed whilst the question set '
        + 'is hidden, the question set appears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles[1].elements[0].hiddenExpression = "step1Field1 == 'hide'";
        let response2: any = _.cloneDeep(formConfig2);
        response2['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet2'));
                    expect(debugElement != null)
                        .toBeTruthy('The question set step1QuestionSet2 should have been rendered.');

                    let debugElement1: DebugElement = fixture.debugElement.query(By.css('#step1Field1'));
                    expect(debugElement1 != null).toBeTruthy('step1Field1 should have been rendered');
                    let fieldEl1: HTMLInputElement = debugElement1.nativeElement;
                    fieldEl1.value = 'hide';
                    fieldEl1.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl1.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet2'));
                    expect(debugElement == null)
                        .toBeTruthy('The question set should have been hidden.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet2'));
                    expect(debugElement != null)
                        .toBeTruthy('The question set should have been shown.');
                    resolve();
                });
        });
    });

    it('when an article element of type "questions" has its hiddenExpression updated when the question set '
        + 'is hidden, the question set disappears', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles[1].elements[0].hiddenExpression = "step1Field1 == 'asdf'";
        let response2: any = _.cloneDeep(formConfig2);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.articles[1].elements[0].hiddenExpression = "step1Field1 == 'hide'";
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet2'));
                    expect(debugElement != null)
                        .toBeTruthy('The question set step1QuestionSet2 should have been rendered.');

                    let debugElement1: DebugElement = fixture.debugElement.query(By.css('#step1Field1'));
                    expect(debugElement1 != null).toBeTruthy('step1Field1 should have been rendered');
                    let fieldEl1: HTMLInputElement = debugElement1.nativeElement;
                    fieldEl1.value = 'hide';
                    fieldEl1.dispatchEvent(new Event('input'));
                    let changeEvent: Event = new Event('change', { bubbles: true, cancelable: false });
                    fieldEl1.dispatchEvent(changeEvent);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet2'));
                    expect(debugElement != null)
                        .toBeTruthy('The question set should not have been hidden.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    debugElement = fixture.debugElement.query(By.css('article questions-widget.step1QuestionSet2'));
                    expect(debugElement == null)
                        .toBeTruthy('The question set should have been hidden.');
                    resolve();
                });
        });
    });

    it('when an article has cssClasses added, the cssClasses are rendered on the html tag', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(formConfig2);
        response2['status'] = 'success';
        response2.form.workflowConfiguration.step1.articles[0].cssClasses = [ 'my-article-css-class'];
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-article-css-class'));
                    expect(debugElement == null)
                        .toBeTruthy('The article should not have a css class yet.');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('article.my-article-css-class'));
                    expect(debugElement != null)
                        .toBeTruthy('The article should have the css class on it');
                    resolve();
                });
        });
    });

    it('when an article has cssClasses removed, the cssClasses are removed from the html tag', () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        validationService = TestBed.inject<ValidationService>(ValidationService);
        let validationReturnValue: any = null;
        spyOn(validationService, "required").and.returnValue((control: any) => {
            return validationReturnValue;
        });

        let response1: any = _.cloneDeep(formConfig2);
        response1['status'] = 'success';
        response1.form.workflowConfiguration.step1.articles[0].cssClasses = [ 'my-article-css-class'];
        let response2: any = _.cloneDeep(formConfig2);
        response2['status'] = 'success';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let debugElement: DebugElement
                        = fixture.debugElement.query(By.css('article.my-article-css-class'));
                    expect(debugElement != null)
                        .toBeTruthy('The article should have the css class on it');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    debugElement = fixture.debugElement.query(By.css('article.my-article-css-class'));
                    expect(debugElement == null)
                        .toBeTruthy('The article should not have a css class any more.');
                    resolve();
                });
        });
    });
});
