import { ApplicationStartupService } from '@app/services/application-startup.service';
import { ElementRef, Component, OnInit, HostListener, AfterViewInit, HostBinding, OnDestroy } from '@angular/core';
import { Observable, Subscription, fromEvent, Subject } from 'rxjs';
import { AlertService } from '../../services/alert.service';
import { Alert } from '../../models/alert';
import { BroadcastService } from '../../services/broadcast.service';
import { Meta } from '@angular/platform-browser';
import { ConfigProcessorService } from '@app/services/config-processor.service';
import { filter } from 'rxjs/operators';
import { WorkflowService } from '@app/services/workflow.service';
import { ConfigService } from '@app/services/config.service';
import { StyleSheetManager } from '@app/services/style-sheet-manager';
import { ApplicationService } from '@app/services/application.service';
import { HtmlHeadElementsAppender } from '@app/services/html-head-elements-appender';
import { FontManager } from '@app/services/font-manager';
import { IframeManager } from '@app/services/iframe-manager';
import { CssProcessorService } from '@app/services/css-processor.service';
import { IconPreloader } from '@app/services/icon-preloader';
import { NotificationService } from '@app/services/notification.service';
import { ConfigurationOperation } from '@app/operations/configuration.operation';
import { OptionSetChangePublisher } from '@app/services/option-set-change-publisher';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { EventService } from '@app/services/event.service';
import { MessageService } from '@app/services/message.service';
import { LayoutManager } from '@app/services/layout-manager';
import { AngularElementsService } from '@app/services/angular-elements.service';
import { Errors } from '@app/models/errors';
import { QuoteResultProcessor } from '@app/services/quote-result-processor';
import { ClaimResultProcessor } from '@app/services/claim-result-processor';
import { ExpressionDependencies } from '@app/expressions/expression-dependencies';
import { FormType } from '@app/models/form-type.enum';
import { Expression } from '@app/expressions/expression';
import { takeUntil } from 'rxjs/operators';
import { UrlService } from '@app/services/url-service';
import { QuoteType } from '@app/models/quote-type.enum';
import { EnumHelper } from '@app/helpers/enum.helper';
/**
 * Export App component class.
 * TODO: Write a better class header: App components.
 */
@Component({
    selector: 'app',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit, AfterViewInit, OnDestroy {

    public onlineEvent: Observable<Event>;
    public offlineEvent: Observable<Event>;
    public subscriptions$: Array<Subscription> = [];
    public isAllowed: boolean = false;
    public ready: boolean = false;
    private debugComboJustTriggered: boolean = false;
    private debugNumberPressed: number = 0;
    private sidebarBottomPixels: number = 0;
    private alertBottomPixels: number = 0;
    private destroyed: Subject<void> = new Subject<void>();

    @HostBinding('class') public classes: string;
    @HostBinding('class.is-loaded-within-portal') public isLoadedWithinPortal: boolean;
    @HostBinding('class.is-loaded-outside-portal') public isLoadedOutsidePortal: boolean;
    @HostBinding('class.mobile-width') public isMobileWidth: boolean;
    @HostBinding('class.wider-than-mobile') public isWiderThanMobile: boolean;
    @HostBinding('class.ios') public isIos: boolean = true;

    /**
     * This class at the top level will allow product developers to write more specific css to override anything
     * which is output by the system.
     */
    @HostBinding('class.override') public override: boolean = true;

    public constructor(
        protected configProcessor: ConfigProcessorService,
        protected configService: ConfigService,
        private elementRef: ElementRef,
        private appStartupService: ApplicationStartupService,
        protected alertService: AlertService,
        private notificationService: NotificationService,
        protected broadcast: BroadcastService,
        private meta: Meta,
        protected workflowService: WorkflowService,
        public applicationService: ApplicationService,
        private configurationOperation: ConfigurationOperation,
        private browserDetectionService: BrowserDetectionService,
        public eventService: EventService,
        private messageService: MessageService,
        private angularElementsService: AngularElementsService,
        private expressionDependencies: ExpressionDependencies,
        styleSheetManager: StyleSheetManager, /* DO NOT REMOVE */
        customHtmlHeadElementsManager: HtmlHeadElementsAppender, /* DO NOT REMOVE */
        cssProcessor: CssProcessorService, /* DO NOT REMOVE */
        fontManager: FontManager, /* DO NOT REMOVE */
        iframeManager: IframeManager, /* DO NOT REMOVE */
        layoutManager: LayoutManager, /* DO NOT REMOVE */
        iconPreloader: IconPreloader, /* DO NOT REMOVE */
        optionSetChangePublisher: OptionSetChangePublisher, /* DO NOT REMOVE */
        quoteResultProcessor: QuoteResultProcessor, /* DO NOT REMOVE */
        claimResultProcessor: ClaimResultProcessor, /* DO NOT REMOVE */
        urlService: UrlService,
    ) {
        let urlParams: URLSearchParams = urlService.getQueryStringParams();
        this.setupAppAttributes(urlParams);
        this.notifyInjectorThatAppHasLoaded();
        this.applicationService.configurationAvailableSubject.pipe(filter((available: boolean) => available))
            .subscribe((available: boolean) => {
                this.isLoadedWithinPortal = applicationService.isLoadedWithinPortal;
                this.isLoadedOutsidePortal = !applicationService.isLoadedWithinPortal;
            });
        this.appStartupService.initialise(urlParams);
        this.appStartupService.readySubject.pipe(filter((ready: boolean) => ready))
            .subscribe(
                (ready: boolean) => {
                    this.ready = ready;
                    this.setupStateExpression();
                },
                () => this.appStartupService.readySubject.unsubscribe());
    }

    private notifyInjectorThatAppHasLoaded(): void {
        this.messageService.sendMessage('appLoad');
    }

    public ngOnInit(): void {
        this.angularElementsService.registerAngularComponentsAsAngularElements();
        this.isMobileWidth = this.browserDetectionService.isMobileWidth();
        this.isIos = this.browserDetectionService.isIos;
        this.isWiderThanMobile = !this.isMobileWidth;
        this.onlineEvent = fromEvent(window, 'online');
        this.offlineEvent = fromEvent(window, 'offline');
        this.subscriptions$.push(this.onlineEvent.subscribe(() => {
            setTimeout(() => {
                this.alertService.hide();
            }, 500);
        }));

        this.subscriptions$.push(this.offlineEvent.subscribe(() => {
            this.alertService.alert(new Alert(
                `You're offline`,
                `We were unable to contact the server. Please check your internet connection and try again.`,
            ));
            setTimeout(() => this.broadcast.broadcast('Error500Event', {}), 500);
        }));
        this.applyMinimumHeight();
        this.onMinimumHeightUpdated();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    private setupAppAttributes(urlParams: URLSearchParams): void {
        let attributes: Array<string> = [
            'tenant',
            'portal',
            'organisation',
            'product',
            'environment',
            'formType',
            'quoteType',
            'mode',
            'isLoadedWithinPortal'];
        for (let attribute of attributes) {
            let value: any = urlParams.get(attribute);
            if (attribute == 'quoteType' && value != null) {
                let quoteType: QuoteType = EnumHelper.parseOrNull(QuoteType, value);
                value = quoteType ??
                    (!isNaN(Number(value)) ? EnumHelper.parseOrNull(QuoteType, Number(value)) : null);
            }
            if (attribute == 'isLoadedWithinPortal' && !value) {
                value = 'false';
            }
            if (value) {
                this.elementRef.nativeElement.setAttribute(attribute, value);
            }
        }
    }

    private setupStateExpression(): void {
        let expressionSource: string = 'hasActiveTrigger() + getActiveTriggerType() + getCurrentWorkflowStep()';
        if (this.applicationService.formType == FormType.Quote) {
            expressionSource += ' + getQuoteCalculationState() + getQuoteState()';
        } else {
            expressionSource += ' + getClaimCalculationState() + getClaimState()';
        }

        let stateExpression: Expression = new Expression(
            expressionSource,
            this.expressionDependencies,
            'app state expression');
        stateExpression.nextResultObservable
            .pipe(takeUntil(this.destroyed)).subscribe(() => this.generateStateClasses());
        stateExpression.triggerEvaluation();
    }

    private generateStateClasses(): void {
        let classes: Array<string> = new Array<string>();
        if (this.applicationService.formType == FormType.Quote && this.applicationService.latestQuoteResult) {
            classes.push(`calculation-state-${this.applicationService.latestQuoteResult.calculationState}`);
            let quoteState: string = this.expressionDependencies.expressionMethodService.getQuoteState();
            classes.push(`quote-state-${quoteState}`);
        } else if (this.applicationService.formType == FormType.Claim && this.applicationService.latestClaimResult) {
            let claimState: string = this.expressionDependencies.expressionMethodService.getClaimState();
            classes.push(`calculation-state-${this.applicationService.latestClaimResult.calculationState}`);
            classes.push(`claim-state-${claimState}`);
        }

        if (this.expressionDependencies.expressionMethodService.hasActiveTrigger()) {
            classes.push('has-trigger');
        } else {
            classes.push('no-trigger');
        }

        const activeTriggerType: string =
            this.expressionDependencies.expressionMethodService.getActiveTriggerType();
        if (activeTriggerType) {
            classes.push(`has-${activeTriggerType}-trigger`);
        }

        const currentWorkflowStep: string =
            this.expressionDependencies.expressionMethodService.getCurrentWorkflowStep();
        if (currentWorkflowStep) {
            classes.push(`workflow-step-${currentWorkflowStep}`);
        }

        this.classes = classes.join(' ');
    }

    private applyMinimumHeight(): void {
        if (this.applicationService.embedOptions?.minimumHeight) {
            this.elementRef.nativeElement.style.minHeight = this.applicationService.embedOptions.minimumHeight;
        }
    }

    private onMinimumHeightUpdated(): void {
        this.eventService.appMinimumHeightSubject.subscribe((minimumHeight: string) => {
            if (!this.applicationService.embedOptions) {
                this.applicationService.embedOptions = { minimumHeight: minimumHeight };
            } else {
                this.applicationService.embedOptions.minimumHeight = minimumHeight;
            }
            this.applyMinimumHeight();
        });

        this.eventService.sidebarBottomSubject.subscribe((sidebarBottomPixels: number) => {
            this.sidebarBottomPixels = sidebarBottomPixels;
            this.setLargestMiniumHeightPixels();
        });

        this.eventService.alertBottomSubject.subscribe((alertBottomPixels: number) => {
            this.alertBottomPixels = alertBottomPixels;
            this.setLargestMiniumHeightPixels();
        });
    }

    /**
     * Gets the larger of the passed value or the configured value for minimum height
     * @param defaultValue the value to use if the configured minimum height could not be parsed.
     */
    private setLargestMiniumHeightPixels(): void {
        let configuredMinHeightPixels: number = 0;
        const configuredMinHeight: string = this.applicationService.embedOptions?.minimumHeight;
        if (configuredMinHeight?.endsWith('px')) {
            configuredMinHeightPixels
                = Number.parseInt(configuredMinHeight.substring(0, configuredMinHeight.length - 2), 10);
        }
        let minimumHeightPixels: number
            = Math.max(configuredMinHeightPixels, this.alertBottomPixels, this.sidebarBottomPixels);
        if (minimumHeightPixels > 0) {
            this.elementRef.nativeElement.style.minHeight = minimumHeightPixels + 'px';
        } else {
            this.elementRef.nativeElement.style.minHeight = null;
        }
    }

    public ngAfterViewInit(): void {
        this.setMetaTags();
    }

    @HostListener("window:message", ['$event'])
    public onMessage(e: any): void {
        if (e.data.messageType == 'iosAndAndroidTrigger') {
            // add some extra length to the body for overscroll
            document.body.style.paddingBottom = '100px';
        } else if (e.data.messageType == 'debug') {
            if (!this.ready) {
                this.toggleDebug();
            }
        }
    }

    @HostListener('document:keyup.alt.control.d')
    public toggleDebug(): void {
        this.debugComboJustTriggered = true;
        this.notificationService.notify({
            message: 'Toggling debug. Please wait...',
            expireAfterMillis: 1000,
        });
        setTimeout(() => {
            this.applicationService.debug = !this.applicationService.debug;
            if (!this.applicationService.debug && this.debugNumberPressed) {
                this.applicationService.debug = true;
            }
            if (this.applicationService.debug) {
                this.applicationService.debugLevel = this.debugNumberPressed || 1;
            }
            this.notificationService.notify({
                message: 'Debug is now ' + (this.applicationService.debug
                    ? `ON, level ${this.applicationService.debugLevel}.`
                    : 'OFF'),
                expireAfterMillis: 3000,
            });
            this.debugNumberPressed = 0;
            this.debugComboJustTriggered = false;
        }, 1000);
    }

    @HostListener('document:keydown', ['$event'])
    public onKeyDown(event: KeyboardEvent): void {
        if (this.debugComboJustTriggered) {
            let numberPressed: number = parseInt(event.key, 10);
            if (!isNaN(numberPressed)) {
                this.debugNumberPressed = numberPressed;
            }
        }
    }

    private setMetaTags(): void {
        this.meta.addTag({
            name: 'format-detection',
            content: 'telephone=no',
        });
        this.meta.addTag({
            name: 'msapplication-tap-highlight',
            content: 'no',
        });
        this.meta.addTag({
            name: 'apple-mobile-web-app-capable',
            content: 'yes',
        });
        this.meta.addTag({
            name: 'apple-mobile-web-app-status-bar-style',
            content: 'black',
        });
        this.meta.addTag({
            name: 'X-UA-Compatible',
            content: 'IE=edge',
        });
    }

    @HostListener('document:keyup.alt.control.r')
    public onReloadConfiguration(): void {
        this.notificationService.notify({
            message: 'Reloading configuration, please wait...',
            expireUponNextNotification: true,
        });
        let params: any = {};

        if (this.applicationService.quoteId) {
            params['quoteId'] = this.applicationService.quoteId;
        }
        if (this.applicationService.productReleaseId || this.applicationService.productReleaseNumber) {
            params['productRelease']
                = this.applicationService.productReleaseId || this.applicationService.productReleaseNumber;
        }
        if (this.applicationService.quoteType) {
            params['quoteType'] = this.applicationService.quoteType;
        }
        if (this.applicationService.policyId) {
            params['policyId'] = this.applicationService.policyId;
        }

        this.configurationOperation.execute(params).toPromise()
            .then((response: any) => {
                this.configProcessor.onConfigurationResponse(response);
                this.notificationService.notify({
                    message: 'Configuration reloaded.',
                });
            });
    }

    @HostListener('document:keyup.alt.control.t')
    public onTestError(): void {
        throw Errors.General.Unexpected(
            'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec in sem urna. Quisque et placerat nunc. '
            + 'Pellentesque sollicitudin luctus semper. Duis convallis felis a eros commodo vehicula vitae nec ante. '
            + 'Etiam est lorem, vulputate nec mi vel, mollis efficitur tellus. Cras interdum justo quis nisi interdum '
            + 'finibus. Nam vestibulum massa eu lacus tempor, ac luctus nulla sollicitudin. Aliquam imperdiet porta '
            + 'efficitur. Proin ante leo, malesuada vel vehicula in, mattis sed enim. Duis rhoncus auctor ex non '
            + 'mollis.');
    }


    @HostListener("window:resize", ['$event'])
    protected onWindowResize(e: any): void {
        this.eventService.windowResizeSubject.next();
        this.isMobileWidth = this.browserDetectionService.isMobileWidth();
        this.isWiderThanMobile = !this.isMobileWidth;
        this.updateBodyClasses();
    }

    private updateBodyClasses(): void {
        if (this.isMobileWidth) {
            if (document.body.classList.contains('wider-than-mobile')) {
                document.body.classList.remove('wider-than-mobile');
            }
            if (!document.body.classList.contains('mobile-width')) {
                document.body.classList.add('mobile-width');
            }
        } else {
            if (!document.body.classList.contains('wider-than-mobile')) {
                document.body.classList.add('wider-than-mobile');
            }
            if (document.body.classList.contains('mobile-width')) {
                document.body.classList.remove('mobile-width');
            }
        }
    }
}
