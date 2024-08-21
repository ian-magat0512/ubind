import { QuoteType } from "@app/models/quote-type.enum";
import { SubscriptionLike, Subject } from "rxjs";
import { AppConfigService } from "@app/services/app-config.service";
import {
    OnDestroy, ElementRef, Injector, Renderer2,
    ChangeDetectorRef, OnInit, HostListener, Directive,
} from "@angular/core";
import { AppConfig } from "@app/models/app-config";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { DomSanitizer, SafeResourceUrl } from "@angular/platform-browser";
import { ProblemDetails } from "@app/models/problem-details";
import { ErrorHandlerService } from "@app/services/error-handler.service";
import { EventService } from "@app/services/event.service";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { SplitLayoutManager } from "@app/models/split-layout-manager";
import { RouteHelper } from "@app/helpers/route.helper";
import { BrowserDetectionService } from "@app/services/browser-detection.service";
import { AuthenticationService } from "@app/services/authentication.service";

declare let window: Window;

/**
 * Export forms app page abstract class
 * Also to load the webFormApp iframe and binding the ubindJs
 */
@Directive({ selector: '[appForms]' })
export abstract class FormsAppPage extends DetailPage implements OnInit, OnDestroy, SplitLayoutManager {

    protected formsAppBaseUrl: string = '';
    protected formsAppStrictReferrer: any;
    public formAppUrl: SafeResourceUrl;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public readonly defaultCancelButtonLabel: string = "Cancel";
    public readonly defaultSuccessButtonLabel: string = "Complete";
    public closeButtonLabel: string = this.defaultCancelButtonLabel;
    protected title: string = '';
    protected action: string = '';
    protected complete: boolean = false;
    public isIos: boolean;
    protected messageListener: () => void;
    public isDisposed: boolean = true;
    protected frameContainerElement: HTMLElement;
    protected iframeElement: HTMLElement;

    // injection attributes:
    public tenantAlias: string;
    public organisationId: string;
    public organisationAlias: string;
    public productAlias: string;
    public environment: string;
    public quoteId: string;
    public quoteTypeAsString: string;
    public mode: string;
    public version: string;
    public isTestData: string;
    public claimId: string;
    public formType: string;
    public debug: boolean;
    public debugLevel: number;
    public productRelease: string;
    public policyId: string;

    /*
     * the ID of the organisation of the portal which the user first logged into
     */
    protected portalOrganisationId: string;

    /**
     * the alias of the organisation of the portal which the user first logged into
     */
    protected portalOrganisationAlias: string;

    private customDomain: string;

    public constructor(
        protected appConfigService: AppConfigService,
        public layoutManager: LayoutManagerService,
        public sanitizer: DomSanitizer,
        protected errorHandlerService: ErrorHandlerService,
        public eventService: EventService,
        protected elementRef: ElementRef,
        protected render: Renderer2,
        injector: Injector,
        protected changeDetectorRef: ChangeDetectorRef,
        protected routeHelper: RouteHelper,
        private browserDetectionService: BrowserDetectionService,
        private authenticationService: AuthenticationService,
    ) {
        super(eventService, elementRef, injector);
        this.subscriptions.push(this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.formsAppBaseUrl = appConfig.formsApp.baseUrl;
            let thirdSlashPosition: any = this.getSubstringPosition(this.formsAppBaseUrl, '/', 3);
            this.formsAppStrictReferrer = thirdSlashPosition
                ? this.formsAppStrictReferrer = this.formsAppBaseUrl.substring(0, thirdSlashPosition)
                : this.formsAppBaseUrl;
            this.tenantAlias = appConfig.portal.tenantAlias;
            this.environment = appConfig.portal.environment;
            this.customDomain = `https://${appConfig.portal.customDomain}`;
            this.portalOrganisationAlias = appConfig.portal.organisationAlias;
            this.portalOrganisationId = appConfig.portal.organisationId;
        }));
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.isDisposed = false;
        this.isIos = this.browserDetectionService.isIos;
        this.organisationId = this.authenticationService.userOrganisationId;
        this.organisationAlias = this.authenticationService.userOrganisationAlias;
    }

    public ngOnDestroy(): any {
        this.isDisposed = true;
        if (this.messageListener) {
            this.messageListener();
        }
        this.destroyed.next();
        this.destroyed.complete();
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
    }

    private getSubstringPosition(string: string, subString: string, startingIndex: any): any {
        return string.split(subString, startingIndex).join(subString).length;
    }

    protected loadFormsApp(mode: string = null, quoteType: QuoteType = null, version: string = null): void {
        if (quoteType) {
            this.quoteTypeAsString = quoteType.toString();
        }

        this.mode = mode;
        this.version = version;
        this.messageListener = this.render.listen(window, 'message', (event: any) => this.receiveMessage(event));
        let debugQueryParam: string = this.routeHelper.getParam('debug');
        this.debug = debugQueryParam != null && debugQueryParam.toUpperCase() == 'TRUE';
        let debugLevelQueryParam: number = parseInt(this.routeHelper.getParam('debugLevel'), 10);
        if (!isNaN(debugLevelQueryParam)) {
            this.debugLevel = debugLevelQueryParam;
        }

        // It's necessary to call the change detector here otherwise 
        // the html doesn't get updated with the latest productAlias
        // so on the second quote edit it will come through as null.
        this.changeDetectorRef.detectChanges();

        this.injectUbindJs();
        this.frameContainerElement = this.elementRef.nativeElement.querySelector('#ubindFrameContainer');
        this.frameContainerElement.addEventListener('touchmove', (event: TouchEvent) => {
            event.stopPropagation();
        });
        this.listenForScrollToMessages();
    }

    protected injectUbindJs(): void {
        if (this.isUBindJsAlreadyInjected()) {
            window['_uBindInjector'].detectAndInject();
        } else {
            let head: HTMLHeadElement = document.getElementsByTagName("head")[0];
            let js: HTMLScriptElement = document.createElement("script");
            js.type = "text/javascript";
            js.src = '../../../../assets/ubind.js';
            head.appendChild(js);
        }
    }

    private isUBindJsAlreadyInjected(): boolean {
        let scripts: any = document.getElementsByTagName('script');
        for (let i in scripts) {
            if (scripts[i].src && scripts[i].src.indexOf('/assets/ubind.js') != -1) {
                return true;
            }
        }
        return false;
    }

    protected listenForScrollToMessages(): void {
        let eventMethod: string = window.addEventListener ? 'addEventListener' : 'attachEvent';
        let eventer: any = window[eventMethod];
        let messageEvent: string = eventMethod === 'attachEvent' ? 'onmessage' : 'message';

        eventer(messageEvent, (e: any) => {
            if (e.data.messageType == 'frameContainerScrollTo') {
                this.frameContainerElement.scrollTop = e.data.payload.newScrollPosition;
                let iframe: HTMLIFrameElement = <HTMLIFrameElement>document.getElementsByClassName('ubind-iframe')[0];
                iframe.contentWindow.postMessage({
                    'messageType': 'sidebarTrigger',
                    'payload': {
                        showSidebar: true,
                    },
                }, this.formsAppBaseUrl);
            }
        });
    }

    public shouldShowSplit(): boolean {
        let contentWidth: number = this.layoutManager.getContentWidth();
        let halfContentWidth: number = Number.parseInt((contentWidth / 2).toString(), 10);
        let minimumWidth: number = 829;
        return halfContentWidth > minimumWidth;
    }

    public receiveMessage(event: any): void {
        if (event && event.origin === this.formsAppStrictReferrer && event.data.messageType == 'authenticationError') {
            const problemDetails: ProblemDetails = ProblemDetails.fromJson(event.data.payload);
            this.errorHandlerService.handleError(problemDetails);
        }

        if (event && ([this.formsAppStrictReferrer, this.customDomain].includes(event.origin))
            && event.data.messageType == 'closeApp') {
            this.complete = true;
            this.closeButtonClicked();
        }

        if (event && event.origin === this.formsAppStrictReferrer && event.data.messageType == 'closeButtonLabel') {
            this.closeButtonLabel = event.data.payload;
        }

        if (event && event.origin === this.formsAppStrictReferrer &&
            event.data.messageType == 'customerUpdated') {
            this.eventService.customerUpdated();
        }

        if (event && event.origin === this.formsAppStrictReferrer && event.data.messageType == 'quoteStepChanged') {
            this.eventService.quoteStepChanged(event.data.payload);
        }

        if (event && event.origin === this.formsAppStrictReferrer && event.data.messageType == 'quoteStateChanged') {
            this.eventService.quoteStateChanged(event.data.payload);
        }

        if (event && event.origin === this.formsAppStrictReferrer && event.data.messageType == 'claimStateChanged') {
            this.eventService.claimStateChanged(event.data.payload);
        }

        this.closeButtonLabel = this.closeButtonLabel || (this.complete ?
            this.defaultSuccessButtonLabel : this.defaultCancelButtonLabel);
    }

    private removeInjectedIframe(): void {
        this.iframeElement = this.frameContainerElement && this.frameContainerElement.querySelector('iframe');
        if (this.iframeElement && window['_uBindInjector']) {
            window['_uBindInjector'].removeInjectedIframe(this.iframeElement.id);
        }
    }

    public userDidTapCloseButton(): void {
        this.removeInjectedIframe();
        this.closeButtonClicked();
    }

    public userDidTapBackButton(): void {
        this.removeInjectedIframe();
        this.goBackButtonClicked();
    }

    public abstract closeButtonClicked(): void;

    public abstract goBackButtonClicked(): void;

    @HostListener('window:resize', ['$event'])
    public async onResize(): Promise<void> {
        this.updateMinimumHeight();
    }

    private updateMinimumHeight(): void {
        // tell the iframe we want it to have a new minimum height
        let minimumHeightPixels: number = (<HTMLElement> this.elementRef.nativeElement).clientHeight;

        // subtract the standard portal padding so we don't get a scrollbar
        minimumHeightPixels -= 91.5;

        let iframe: HTMLIFrameElement = <HTMLIFrameElement>document.getElementsByClassName('ubind-iframe')[0];
        iframe.contentWindow.postMessage({
            'messageType': 'appMinimumHeight',
            'payload': {
                minimumHeight: minimumHeightPixels + 'px',
            },
        }, this.formsAppBaseUrl);
    }

}
