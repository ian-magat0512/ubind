import { Component, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
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
import * as _ from 'lodash-es';
import formConfig from './font-manager.test-form-config.json';
import { ToolTipService } from '@app/services/tooltip.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { FontManager } from './font-manager';
import { BrowserDetectionService } from './browser-detection.service';
import { RevealGroupTrackingService } from './reveal-group-tracking.service';
import { AngularElementsService } from './angular-elements.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';
import { OperationInstructionService } from './operation-instruction.service';
import { OperationStatusService } from './operation-status.service';
import { MaskPipe } from 'ngx-mask';
import { ApiService } from './api.service';
import { LoggerService } from './logger.service';

/* global spyOn */

describe('FontManager', () => {
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
                { provide: MaskPipe, useClass: MaskPipe },
                ToolTipService,
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
                FontManager,
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
        })
            .compileComponents().then(() => {
                let messageService: MessageService = TestBed.inject<MessageService>(MessageService);
                spyOn(messageService, 'sendMessage'); // make it do nothing.
                TestBed.inject<FontManager>(FontManager);

                let angularElementsService: AngularElementsService = TestBed.inject(AngularElementsService);
                angularElementsService.registerAngularComponentsAsAngularElements();
            });
    });

    afterEach(() => {
        fixture.destroy();
        let linkEl: HTMLElement = window.document.getElementById('google-fonts-stylesheet');
        if (linkEl) {
            linkEl.remove();
        }
    });

    it('when a google font is added, it\'s loaded', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.googleFonts.push({
            usage: 'headings',
            family: 'Roboto Slab',
            weight: '400',
        });
        let response3: any = _.cloneDeep(response2);
        response3.form.theme.googleFonts.push({
            usage: 'bodyText',
            family: 'Chivo',
            weight: '300',
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
                    let linkEl: HTMLElement = window.document.getElementById('google-fonts-stylesheet');
                    expect(linkEl == null).toBeTruthy('There should be no google fonts stylesheet yet');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    linkEl = window.document.getElementById('google-fonts-stylesheet');
                    expect(linkEl.getAttribute('href'))
                        .toBe('https://fonts.googleapis.com/css?family=Roboto+Slab:400');

                    // Act
                    configProcessorService.onConfigurationResponse(response3);
                    fixture.detectChanges();

                    linkEl = window.document.getElementById('google-fonts-stylesheet');
                    expect(linkEl.getAttribute('href'))
                        .toBe('https://fonts.googleapis.com/css?family=Roboto+Slab:400|Chivo:300');
                    resolve();
                });
        });
    });

    it('when a google font is removed, it\'s unloaded', async () => {
        // Arrange
        fixture = TestBed.createComponent(TestHostComponent);
        sut = fixture.componentInstance;
        eventService = TestBed.inject<EventService>(EventService);

        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        response1.form.theme.googleFonts.push({
            usage: 'headings',
            family: 'Roboto Slab',
            weight: '400',
        });
        response1.form.theme.googleFonts.push({
            usage: 'bodyText',
            family: 'Chivo',
            weight: '300',
        });
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.googleFonts.pop();
        let response3: any = _.cloneDeep(response2);
        response3.form.theme.googleFonts.pop();
        expect(response3.form.theme.googleFonts.length).toBe(0);
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        sut.ready = true;
        fixture.detectChanges();

        return new Promise((resolve: any, reject: any): void => {
            eventService.webFormLoadedSubject
                .pipe(filter((loaded: boolean) => loaded == true))
                .subscribe(async () => {
                    let linkEl: HTMLElement = window.document.getElementById('google-fonts-stylesheet');
                    expect(linkEl.getAttribute('href'))
                        .toBe('https://fonts.googleapis.com/css?family=Roboto+Slab:400|Chivo:300');

                    // Act
                    configProcessorService.onConfigurationResponse(response2);
                    fixture.detectChanges();

                    // Assert
                    linkEl = window.document.getElementById('google-fonts-stylesheet');
                    expect(linkEl.getAttribute('href'))
                        .toBe('https://fonts.googleapis.com/css?family=Roboto+Slab:400');

                    // Act
                    configProcessorService.onConfigurationResponse(response3);
                    fixture.detectChanges();

                    linkEl = window.document.getElementById('google-fonts-stylesheet');
                    expect(linkEl == null)
                        .toBeTruthy('There should be no google fonts stylesheet any more');
                    resolve();
                });
        });
    });

});
