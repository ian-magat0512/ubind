import { ChangeDetectionStrategy, Component, DebugElement, EventEmitter, CUSTOM_ELEMENTS_SCHEMA,
} from '@angular/core';
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
import formConfig from './field-addons.test-form-config.json';
import { ToolTipService } from '@app/services/tooltip.service';
import { FakeToolTipService } from '@app/services/fakes/fake-tooltip.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { AngularElementsService } from '@app/services/angular-elements.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from '@app/services/operation-instruction.service';
import { OperationStatusService } from '@app/services/operation-status.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from '@app/services/api.service';
import { LoggerService } from '@app/services/logger.service';

/* global spyOn */

describe('Field Addons', () => {
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
        changeDetection: ChangeDetectionStrategy.OnPush,
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
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                { provide: EvaluateService, useClass: EvaluateService },
                { provide: EventService, useClass: EventService },
                { provide: CalculationService, useClass: CalculationService },
                { provide: WorkflowService, useValue: workflowServiceStub },
                { provide: ConfigService, useClass: ConfigService },
                FormService,
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
                TimePipe,
                PhoneNumberPipe,
                NumberPlatePipe,
                CssIdentifierPipe,
                ExpressionDependencies,
                WorkflowStatusService,
                NotificationService,
                ConfigurationV2Processor,
                { provide: EncryptionService, useValue: encryptionServiceStub },
                BrowserDetectionService,
                RevealGroupTrackingService,
                AngularElementsService,
                ApiService,
                LoggerService,
                OperationInstructionService,
                OperationStatusService,
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

            let angularElementsService: AngularElementsService = TestBed.inject(AngularElementsService);
            angularElementsService.registerAngularComponentsAsAngularElements();
        });
    });

    afterEach((): void => {
        fixture.destroy();
    });

    it('renders icons or text in fields as expected', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);
        let debugEl: DebugElement = null;

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
                    await fixture.whenRenderingDone();

                    // textWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-textWithIcons addons-wrapper > .input-group-addon .fa-address-card'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-address-card icon at the left of the textWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-textWithIcons addons-wrapper > .input-group-addon .fa-adjust'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-adjust icon at the right of the textWithIcons field");

                    // textWithText
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-textWithText addons-wrapper > .input-group-addon:first-child'));
                    expect(debugEl.nativeElement.innerText).toContain(
                        'Left',
                        "Did not find the Left text on textWithText");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-textWithText addons-wrapper > .input-group-addon:last-child'));
                    expect(debugEl.nativeElement.innerText).toContain(
                        'Right',
                        "Did not find the Right text on textWithText");

                    // selectWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-selectWithIcons addons-wrapper > .input-group-addon .fa-address-card'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-address-card icon at the left of the selectWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-selectWithIcons addons-wrapper > .input-group-addon .fa-adjust'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-adjust icon at the right of the selectWithIcons field");

                    // selectWithText
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-selectWithText addons-wrapper > .input-group-addon:first-child'));
                    expect(debugEl.nativeElement.innerText).toContain(
                        'L',
                        "Did not find the Left text on selectWithText");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-selectWithText addons-wrapper > .input-group-addon:last-child'));
                    expect(debugEl.nativeElement.innerText).toContain(
                        'R',
                        "Did not find the Right text on selectWithText");

                    // searchSelectWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-searchSelectWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-address-card'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-address-card icon at the left of the searchSelectWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-searchSelectWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-adjust'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-adjust icon at the right of the searchSelectWithIcons field");

                    // searchSelectWithText
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-searchSelectWithText addons-wrapper > .input-group-addon:first-child'));
                    expect(debugEl.nativeElement.innerText).toContain(
                        'L',
                        "Did not find the Left text on searchSelectWithText");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-searchSelectWithText addons-wrapper > '
                        + '.input-group-addon:last-child'));
                    expect(debugEl.nativeElement.innerText).toContain(
                        'R',
                        "Did not find the Right text on searchSelectWithText");

                    // searchSelectDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-searchSelectDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-search'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-search icon at the left of the searchSelectDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-searchSelectDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'search-select-field'.toUpperCase(),
                        "found something on the right of searchSelectDefault when it shouldn't have anything");

                    // currencyWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-currencyWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-shopping-cart'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-shopping-cart icon at the left of the currencyWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-currencyWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-credit-card-front'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-credit-card-front icon at the right of the currencyWithIcons field");

                    // currencyDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-currencyDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-usd'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-usd icon at the left of the currencyDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-currencyDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'currency-field'.toUpperCase(),
                        "found something on the right of currencyDefault when it shouldn't have anything");

                    // currencyWithCode
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-currencyWithCode addons-wrapper > '
                        + '.input-group-addon:first-child'));
                    expect(debugEl.nativeElement.innerText).toContain(
                        'PHP',
                        "Did not find the Left text on currencyWithCode");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-currencyWithCode addons-wrapper > '
                        + '.input-group-addon:first-child span'));
                    expect(debugEl.nativeElement.classList.contains('currency-code')).toBeTruthy(
                        "Did not find css class currency-code on currencyWithCode:"
                        + Array.from(debugEl.nativeElement.classList).join(','));
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-currencyWithCode addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'currency-field'.toUpperCase(),
                        "found something on the right of currencyWithCode when it shouldn't have anything");

                    // currencyWithLeftText
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-currencyWithLeftText addons-wrapper > '
                        + '.input-group-addon:first-child'));
                    expect(debugEl.nativeElement.innerText).toContain(
                        'AAA',
                        "Did not find the Left text on currencyWithLeftText");

                    // passwordWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-passwordWithIcons addons-wrapper > '
                        + '.input-group-addon .fa-fingerprint'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-fingerprint icon at the left of the passwordWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-passwordWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-key-skeleton'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-key-skeleton icon at the right of the passwordWithIcons field");

                    // passwordDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-passwordDefault addons-wrapper > *:first-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'password-field'.toUpperCase(),
                        "found something on the left of passwordDefault when it shouldn't have anything");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-passwordDefault addons-wrapper > .input-group-addon:last-child '
                        + '.fa-key'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-key icon at the right of the passwordDefault field");

                    // percentWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-percentWithIcons addons-wrapper > '
                        + '.input-group-addon .fa-pizza'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-pizza icon at the left of the percentWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-percentWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-link'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-link icon at the right of the percentWithIcons field");

                    // percentDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-percentDefault addons-wrapper > *:first-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the left of percentDefault when it shouldn't have anything");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-percentDefault addons-wrapper > .input-group-addon:last-child '
                        + '.fa-percent'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-percent icon at the right of the percentDefault field");

                    // nameWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-nameWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-calculator'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-calculator icon at the left of the nameWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-nameWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-bullseye'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-bullseye icon at the right of the nameWithIcons field");

                    // nameDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-nameDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-user'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-user icon at the left of the nameDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-nameDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the right of nameDefault when it shouldn't have anything");

                    // dateWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-dateWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-calculator'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-calculator icon at the left of the dateWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-dateWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-bullseye'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-bullseye icon at the right of the dateWithIcons field");

                    // dateDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-dateDefault addons-wrapper > .input-group-addon:first-child '
                        + '.glyphicon-calendar'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the glyphicon-calendar icon at the left of the dateDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-dateDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'datepicker-field'.toUpperCase(),
                        "found something on the right of dateDefault when it shouldn't have anything");

                    // timeWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-timeWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-cogs'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-cogs icon at the left of the timeWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-timeWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-dashboard'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-dashboard icon at the right of the timeWithIcons field");

                    // timeDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-timeDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-clock'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-clock icon at the left of the timeDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-timeDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the right of timeDefault when it shouldn't have anything");

                    // emailWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-emailWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-cogs'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-cogs icon at the left of the emailWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-emailWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-dashboard'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-dashboard icon at the right of the emailWithIcons field");

                    // emailDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-emailDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-envelope'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-envelope icon at the left of the emailDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-emailDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the right of emailDefault when it shouldn't have anything");

                    // phoneWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-phoneWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-cogs'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-cogs icon at the left of the phoneWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-phoneWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-dashboard'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-dashboard icon at the right of the phoneWithIcons field");

                    // phoneDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-phoneDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-phone'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-phone icon at the left of the phoneDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-phoneDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the right of phoneDefault when it shouldn't have anything");

                    // postcodeWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-postcodeWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-cogs'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-cogs icon at the left of the postcodeWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-postcodeWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-dashboard'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-dashboard icon at the right of the postcodeWithIcons field");

                    // postcodeDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-postcodeDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-map-marker'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-map-marker icon at the left of the postcodeDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-postcodeDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the right of postcodeDefault when it shouldn't have anything");

                    // abnWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-abnWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-cogs'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-cogs icon at the left of the abnWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-abnWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-dashboard'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-dashboard icon at the right of the abnWithIcons field");

                    // abnDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-abnDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-certificate'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-certificate icon at the left of the abnDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-abnDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the right of abnDefault when it shouldn't have anything");

                    // numberPlateWithIcons
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-numberPlateWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-cogs'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-cogs icon at the left of the numberPlateWithIcons field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-numberPlateWithIcons addons-wrapper > .input-group-addon '
                        + '.fa-dashboard'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-dashboard icon at the right of the numberPlateWithIcons field");

                    // numberPlateDefault
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-numberPlateDefault addons-wrapper > .input-group-addon:first-child '
                        + '.fa-rectangle-wide'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-rectangle-wide icon at the left of the numberPlateDefault field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-numberPlateDefault addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the right of numberPlateDefault when it shouldn't have anything");

                    // numberPlateWithRightIcon
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-numberPlateWithRightIcon addons-wrapper > .input-group-addon '
                        + '.fa-rectangle-wide'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-rectangle-wide icon at the left of the numberPlateWithRightIcon field");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-numberPlateWithRightIcon addons-wrapper > .input-group-addon '
                        + '.fa-dashboard'));
                    expect(debugEl).not.toBeNull(
                        "Did not find the fa-dashboard icon at the right of the numberPlateWithRightIcon field");

                    // numberPlateWithIconCleared
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-numberPlateWithIconCleared addons-wrapper > *:first-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the left of numberPlateWithIconCleared when it shouldn't have anything");
                    debugEl = fixture.debugElement.query(By.css(
                        '#anchor-numberPlateWithIconCleared addons-wrapper > *:last-child'));
                    expect(debugEl.nativeElement.tagName.toUpperCase()).toBe(
                        'input-field'.toUpperCase(),
                        "found something on the right of numberPlateWithIconCleared when it shouldn't have anything");

                    resolve();
                });
        });
    });
});
