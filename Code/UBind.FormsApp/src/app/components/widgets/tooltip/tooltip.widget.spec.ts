/* eslint-disable max-len */
/* eslint-disable max-classes-per-file */
import { Component, DebugElement, EventEmitter, Renderer2, CUSTOM_ELEMENTS_SCHEMA, ChangeDetectionStrategy,
} from '@angular/core';
import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
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
import { By } from '@angular/platform-browser';
import * as _ from 'lodash-es';
import formConfig from '../../wrappers/tooltip/tooltip.test-form-config.json';
import { ToolTipService } from '@app/services/tooltip.service';
import { Alert } from '@app/models/alert';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationV2Processor } from '@app/services/configuration-v2-processor';
import { EncryptionService } from '@app/services/encryption.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { OptionSetChangePublisher } from '@app/services/option-set-change-publisher';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { RevealGroupTrackingService } from '@app/services/reveal-group-tracking.service';
import { TooltipWidget } from './tooltip.widget';
import { AngularElementsService } from '@app/services/angular-elements.service';
import { FakeOperationFactory } from '@app/operations/fakes/fake-operation-factory';

/* global spyOn */

/**
 * For testing
 */
@Component({
    template: `
        <ubind-tooltip-widget 
            [content]="contentAttribute"
            [show-icon]="showIconAttribute"
            [icon]="iconAttribute"
            [icon-position]="iconPositionAttribute">
            {{ projectedContent }}
        </ubind-tooltip-widget>`,
    // We need to use OnPush change detection here so that we don't get
    // ExpressionChangedAfterItHasBeenCheckedError during unit tests.
    changeDetection: ChangeDetectionStrategy.OnPush,
})
class TestHostComponent {

    public projectedContent: string = null;
    public contentAttribute: string = null;
    public showIconAttribute: string = null;
    public iconAttribute: string = null;
    public iconPositionAttribute: string = null;
}
/**
 * We cannot include these tests in normal test runs. They need to be run individually. For a full explanation, see:
 * https://stackoverflow.com/questions/73849002/unit-tests-of-angular-elements-registered-with-the-customelementregistry-as-a-h
 */
xdescribe('TooltipWidget', () => {
    let hostFixture: ComponentFixture<TestHostComponent>;

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

    beforeEach(async () => {
        return TestBed.configureTestingModule({
            declarations: [
                TestHostComponent,
                TooltipWidget,
                ...sharedConfig.declarations,
            ],
            providers: [
                { provide: EncryptionService, useValue: encryptionServiceStub },
                { provide: ConfigProcessorService, useClass: ConfigProcessorService },
                { provide: MessageService, useClass: MessageService },
                { provide: ConfigurationOperation, useValue: {} },
                { provide: EvaluateService, useClass: EvaluateService },
                EventService,
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
                OptionSetChangePublisher,
                BrowserDetectionService,
                RevealGroupTrackingService,
                Renderer2,
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

            hostFixture = TestBed.createComponent(TestHostComponent);

            let angularElementsService: AngularElementsService = TestBed.inject(AngularElementsService);
            angularElementsService.registerAngularComponentsAsAngularElements();
        });
    });

    afterEach(() => {
        hostFixture.destroy();
    });

    it('when the tooltip icon is changed in the theme, the new tooltip icon is displayed', async () => {
        // Arrange
        let response1: any = _.cloneDeep(formConfig);
        response1['status'] = 'success';
        let response2: any = _.cloneDeep(response1);
        response2.form.theme.tooltipIcon = 'fa fa-laptop';
        let configProcessorService: ConfigProcessorService
            = TestBed.inject<ConfigProcessorService>(ConfigProcessorService);
        configProcessorService.onConfigurationResponse(response1);
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        let debugElement: DebugElement = hostFixture.debugElement.query(By.css('.tooltip-anchor i'));
        let aEl: HTMLElement = debugElement.nativeElement;
        expect(aEl.classList.contains('fa-question-circle'))
            .toBeTruthy('The tooltip icon should initially be fa-question-circle');

        // Act
        configProcessorService.onConfigurationResponse(response2);
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        // Assert
        debugElement = hostFixture.debugElement.query(
            By.css('.tooltip-anchor i'));
        aEl = debugElement.nativeElement;
        expect(aEl.classList.contains('fa-laptop'))
            .toBeTruthy('The tooltip icon should become fa-laptop');
    });

    it('when the content is projected and no content attribute is set, '
        + 'the projected content appears on the tooltip', async () => {
        // Arrange
        let sut: TestHostComponent = hostFixture.componentInstance;
        sut.projectedContent = "This is the projected content";

        // Act
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        // Assert
        let debugElement: DebugElement = hostFixture.debugElement
            .query(By.css('.tooltip-content-container .popover-content'));
        expect(debugElement != null).toBeTruthy('The popover content div should be found');
        expect(debugElement.nativeElement.innerHTML).toContain("This is the projected content");
    });


    it('when the content attribute is set, that\'s what appears on the tooltip', async () => {
        // Arrange
        let el: HTMLElement = hostFixture.nativeElement.getElementsByTagName('ubind-tooltip-widget')[0];
        let testString: string = "This was set on the content attribute.";
        el.setAttribute('content', testString);

        // Act
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        return new Promise((resolve: any, reject: any): void => {
            setTimeout(() => {
                // Assert
                let debugElement: DebugElement = hostFixture.debugElement
                    .query(By.css('.tooltip-content-container .popover-content'));
                expect(debugElement != null).toBeTruthy('The popover content div should be found');
                expect(debugElement.nativeElement.innerHTML).toContain(testString);
                resolve();
            }, 0);

        });
    });

    it('when the content is projected and the content attribute is set, '
        + 'the projected content is the anchor text', async () => {
        // Arrange
        let testContent: string = "This was set on the content attribute.";
        let testAnchor: string = "This is anchor text";
        let sut: TestHostComponent = hostFixture.componentInstance;
        sut.projectedContent = testAnchor;
        let el: HTMLElement = hostFixture.nativeElement.getElementsByTagName('ubind-tooltip-widget')[0];
        el.setAttribute('content', testContent);

        // Act
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        return new Promise((resolve: any, reject: any): void => {
            setTimeout(() => {
                // Assert
                let debugAnchor: DebugElement = hostFixture.debugElement
                    .query(By.css('.tooltip-anchor'));
                expect(debugAnchor != null).toBeTruthy('The popover anchor should be found');
                expect(debugAnchor.nativeElement.innerHTML).toContain(testAnchor);

                let debugElement: DebugElement = hostFixture.debugElement
                    .query(By.css('.tooltip-content-container .popover-content'));
                expect(debugElement != null).toBeTruthy('The popover content div should be found');
                expect(debugElement.nativeElement.innerHTML).toContain(testContent);
                resolve();
            }, 0);

        });
    });

    it('when the content is projected and the content attribute is set, '
        + 'no icon is shown by default', async () => {
        // Arrange
        let testContent: string = "This was set on the content attribute.";
        let testAnchor: string = "This is anchor text";
        let sut: TestHostComponent = hostFixture.componentInstance;
        sut.projectedContent = testAnchor;
        let el: HTMLElement = hostFixture.nativeElement.getElementsByTagName('ubind-tooltip-widget')[0];
        el.setAttribute('content', testContent);

        // Act
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        return new Promise((resolve: any, reject: any): void => {
            setTimeout(() => {
                // Assert
                let debugAnchor: DebugElement = hostFixture.debugElement
                    .query(By.css('.tooltip-anchor .tooltip-icon'));
                expect(debugAnchor == null).toBeTruthy('The icon element should not be found');
                resolve();
            }, 0);

        });
    });

    it('when the content is projected and the content attribute is set, '
        + 'an icon is shown after the term if the show-icon attribute is set', async () => {
        // Arrange
        let testContent: string = "This was set on the content attribute.";
        let testAnchor: string = "This is anchor text";
        let sut: TestHostComponent = hostFixture.componentInstance;
        sut.projectedContent = testAnchor;
        let el: HTMLElement = hostFixture.nativeElement.getElementsByTagName('ubind-tooltip-widget')[0];
        el.setAttribute('content', testContent);
        el.setAttribute('show-icon', 'true');

        // Act
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        return new Promise((resolve: any, reject: any): void => {
            setTimeout(() => {
                // Assert
                let debugAnchorIcon: DebugElement = hostFixture.debugElement
                    .query(By.css('.tooltip-anchor .tooltip-icon'));
                expect(debugAnchorIcon != null).toBeTruthy('The icon element should be found');

                let debugAnchor: DebugElement = hostFixture.debugElement.query(By.css('.tooltip-anchor'));
                const innerHtml: string = debugAnchor.nativeElement.innerHTML;
                const toolTipIconPos: number = innerHtml.indexOf('tooltip-icon');
                const termPos: number = innerHtml.indexOf(testAnchor);
                expect(toolTipIconPos).toBeGreaterThan(termPos);

                resolve();
            }, 0);

        });
    });

    it('when the content is projected and the content attribute is set, '
        + 'an icon is shown before the term if the icon-position attribute is set to "before"', fakeAsync(() => {
        // Arrange
        let testContent: string = "This was set on the content attribute.";
        let testAnchor: string = "This is anchor text";
        let sut: TestHostComponent = hostFixture.componentInstance;
        sut.projectedContent = testAnchor;
        let el: HTMLElement = hostFixture.nativeElement.getElementsByTagName('ubind-tooltip-widget')[0];
        el.setAttribute('content', testContent);
        el.setAttribute('icon-position', 'before');

        // Act
        hostFixture.detectChanges();
        tick();

        // Assert
        let debugAnchorIcon: DebugElement = hostFixture.debugElement
            .query(By.css('.tooltip-anchor .tooltip-icon'));
        expect(debugAnchorIcon != null).toBeTruthy('The icon element should be found');

        let debugAnchor: DebugElement = hostFixture.debugElement.query(By.css('.tooltip-anchor'));
        const innerHtml: string = debugAnchor.nativeElement.innerHTML;
        const toolTipIconPos: number = innerHtml.indexOf('tooltip-icon');
        const termPos: number = innerHtml.indexOf(testAnchor);
        expect(toolTipIconPos).toBeLessThan(termPos);
    }));

    it('when the content is projected and the content attribute is set, '
        + 'an icon is shown after the term if a custom icon is specified', async () => {
        // Arrange
        let testContent: string = "This was set on the content attribute.";
        let testAnchor: string = "This is anchor text";
        let sut: TestHostComponent = hostFixture.componentInstance;
        sut.projectedContent = testAnchor;
        let el: HTMLElement = hostFixture.nativeElement.getElementsByTagName('ubind-tooltip-widget')[0];
        el.setAttribute('content', testContent);
        el.setAttribute('icon', 'fa-solid fa-car');

        // Act
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        return new Promise((resolve: any, reject: any): void => {
            setTimeout(() => {
                let debugAnchorIcon: DebugElement = hostFixture.debugElement
                    .query(By.css('.tooltip-anchor .tooltip-icon'));
                expect(debugAnchorIcon != null).toBeTruthy('The icon element should be found');

                let debugAnchor: DebugElement = hostFixture.debugElement.query(By.css('.tooltip-anchor'));
                const innerHtml: string = debugAnchor.nativeElement.innerHTML;
                const toolTipIconPos: number = innerHtml.indexOf('tooltip-icon');
                const termPos: number = innerHtml.indexOf(testAnchor);
                expect(toolTipIconPos).toBeGreaterThan(termPos);

                resolve();
            }, 0);

        });
    });

    it('when the icon attribute is set, the custom icon is used', async () => {
        // Arrange
        let sut: TestHostComponent = hostFixture.componentInstance;
        sut.projectedContent = "This is the projected content";

        let el: HTMLElement = hostFixture.nativeElement.getElementsByTagName('ubind-tooltip-widget')[0];
        el.setAttribute('icon', 'fa-solid fa-car');

        // Act
        hostFixture.detectChanges();
        await hostFixture.whenRenderingDone();

        return new Promise((resolve: any, reject: any): void => {
            setTimeout(() => {
                let debugAnchorIcon: DebugElement = hostFixture.debugElement
                    .query(By.css('.tooltip-anchor .tooltip-icon'));
                expect(debugAnchorIcon != null).toBeTruthy('The icon element should be found');
                const outerHtml: string = debugAnchorIcon.nativeElement.outerHTML;
                expect(outerHtml).toContain('fa-car');
                resolve();
            }, 0);

        });
    });

});
