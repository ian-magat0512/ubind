import { InjectionContext } from './injection-context';
import { Injection } from './injection';
import { WebFormInjection } from './web-form-injection';
import { PortalInjection } from './portal-injection';
import './html-element-extensions';
import { WebFormEmbedOptions } from './models/web-form-embed-options';
import { PortalEmbedOptions } from './models/portal-embed-options';
import { InterFrameMessage } from './models/inter-frame-message';
import { UrlHelper } from './helpers/url.helper';

let _uBindInjector: UBindInjector;

/**
 * The uBind injector embeds web forms and portals into web pages and apps
 */
class UBindInjector {

    private injectionContext: InjectionContext = new InjectionContext();
    private injections: Map<string, Injection> = new Map<string, Injection>();

    // Google Tag Manager related
    protected dataLayer: any;
    protected tagManagerIsEnabled: boolean = false;
    protected trackedFormModelValues: Array<string>;
    protected trackedCalculationValues: Array<string>;

    public constructor() {
        let scripts: HTMLCollectionOf<HTMLScriptElement> = document.getElementsByTagName('script');
        for (const script of Array.from(scripts)
            .filter((s: HTMLScriptElement) => s.src && s.src.indexOf('/assets/ubind.js') != -1)
        ) {
            this.injectionContext.uBindAppHost = script.src.substring(0, script.src.indexOf('/', 8));
            this.injectionContext.uBindAssetUrl = `${this.injectionContext.uBindAppHost}/assets/`;
        }

        this.injectMaterialPreloaderStylesheet();
        window.addEventListener(
            "message",
            (event: any) => _uBindInjector.handleMessageEvent(event),
            false);
        window.addEventListener(
            "scroll",
            (event: any) => _uBindInjector.handleScrollEvent(event),
            false);
        window.addEventListener(
            "resize",
            (event: any) => _uBindInjector.handleResizeEvent(event),
            false);
    }

    public detectAndInject() {
        let element: HTMLElement = document.getElementById('ubindFrameContainer');
        if (element) {
            element.addEventListener(
                "scroll",
                (event: any) => _uBindInjector.handleScrollEvent(event),
                false);
        }
        this.startWhenDocumentIsReady();
        this.checkInsertDeviceViewPort();
        this.removeToken();
    }

    private injectMaterialPreloaderStylesheet() {
        let head: HTMLHeadElement = document.head;
        let link: HTMLLinkElement = document.createElement("link");
        let linkIE: HTMLLinkElement = document.createElement("link");

        link.type = "text/css";
        link.rel = "stylesheet";
        link.href = this.injectionContext.uBindAssetUrl + 'material-preloader.css';

        if (this.injectionContext.isIE11) {
            linkIE.type = "text/css";
            linkIE.rel = "stylesheet";
            linkIE.href = this.injectionContext.uBindAssetUrl + 'material-preloader-ie11.css';
            head.appendChild(linkIE);
        }

        head.appendChild(link);
    }

    private checkInsertDeviceViewPort() {
        const meta: HTMLMetaElement = document.createElement('meta');
        const metaVieport: HTMLElement = document.head.querySelector("[name=viewport]");
        const canInject: boolean = !metaVieport || metaVieport.tagName != "meta";
        if (canInject) {
            meta.setAttribute('name', 'viewport');
            meta.setAttribute('injected', 'true');
            meta.content = "width=device-width, initial-scale=1.0";
            document.getElementsByTagName('head')[0].appendChild(meta);
        }
    }

    private handleCloseApp(data: any): void {
        let injection: Injection = this.injections.get(data.frameId);
        injection.handleCloseApp(data);

        // wait half a second before removing, so that animations can complete
        setTimeout(() => {
            this.removeInjectedIframe(data.frameId);
        }, 500);
    }

    public removeInjectedIframe(frameId: string) {
        this.injections.delete(frameId);
        let iframeElement: HTMLElement = document.getElementById(frameId);
        if (iframeElement) {
            iframeElement.remove();
        }
    }

    /* eslint-disable no-fallthrough */
    public handleMessageEvent(event: any): void {
        if (event.origin === this.injectionContext.uBindAppHost) {
            switch (event.data.messageType) {
                case 'frameLoading':
                    this.handleFrameLoadingEvent(event.data);
                    break;
                case 'appLoad':
                    this.handleAppLoadEvent(event.data);
                    break;
                case 'configurationLoaded':
                    this.handleConfigurationLoadedEvent(event.data);
                    break;
                case 'webFormLoad':
                    this.handleWebFormLoadEvent(event.data);
                    break;
                case 'scrollToElement':
                    this.handleScrollToElementEvent(event.data);
                    break;
                case 'closeApp':
                    this.handleCloseApp(event.data);
                    break;
                case 'appEvent':
                    this.handleAppEvent(event.data);
                    break;
                case 'saveInitiated':
                case 'policyInitiated':
                case 'closeButtonLabel':
                case 'authenticationError':
                case 'quoteStateChanged':
                case 'claimStateChanged':
                    if (window.top != window.self) {
                        window.parent.postMessage(event.data, event.origin);
                    }
                    break;
                case 'displayMessage':
                    this.handleDisplayMessageEvent(event.data);
                    break;
                case 'redirect':
                    this.handleRedirect(event.data);
                case 'urlChanged':
                    this.handleUrlChangedEvent(event.data);
                    break;
                default:
                    break;
            }
        }
    }

    public handleAppEvent(data: any): void {
        let injection: Injection = this.injections.get(data.frameId);
        if (injection != null) {
            const newEvent: CustomEvent = new CustomEvent(data.payload.eventType, { detail: data.payload.data });
            injection.rootElement.dispatchEvent(newEvent);
        } else {
            console.log('Received an appEvent for ubind injection iframe with id \''
                + data.frameId + '\' however no ubind injection iframe with that id was found.');
        }
        this.forwardEventToGoogleTagManager(data);
    }

    public forwardEventToGoogleTagManager(data: any) {
        if (this.tagManagerIsEnabled) {
            const tagManagerData: any = {
                'event': 'uBind.' + data['payload']['eventType'],
            };
            if (data['payload']['eventData']) {
                tagManagerData['uBindEventData'] = data['payload']['eventData'];
            }
            if (data['payload']['formModel']) {
                tagManagerData['uBindFormModel'] = data['payload']['formModel'];
            }
            if (data['payload']['calculation']) {
                tagManagerData['uBindCalculation'] = data['payload']['calculation'];
            }
            this.dataLayer.push(tagManagerData);
        }
    }

    public handleScrollEvent(event: any): void {
        this.injections.forEach((injection: Injection) => injection.handleScrollEvent(event));
    }

    public handleResizeEvent(event: any): void {
        this.injections.forEach((injection: Injection) => injection.handleResizeEvent(event));
    }

    private handleRedirect(data: any): void {
        if (data.payload.url) {
            window.location.href = data.payload.url;
        }
    }

    private handleDisplayMessageEvent(data: any): void {
        let injection: Injection = this.injections.get(data.frameId);
        if (injection != null) {
            injection.displayMessage(data.payload.message, data.payload.severity ? data.payload.severity : 3);
        } else {
            console.log('Received a display message event for ubind injection iframe with id \''
                + data.frameId + '\' however no ubind injection iframe with that id was found.');
        }
    }

    private handleUrlChangedEvent(data: any): void {
        let injection: Injection = this.injections.get(data.frameId);
        if (injection != null && injection instanceof PortalInjection) {
            (<PortalInjection>injection).updateUrlPathQueryParameter(data.payload);
        } else {
            console.log('Received a url changed event for ubind injection iframe with id \''
                + data.frameId + '\' however no ubind injection iframe with that id was found.');
        }
    }

    private startWhenDocumentIsReady(): void {
        if (document.readyState != 'complete') {
            setTimeout(
                () => {
                    this.startWhenDocumentIsReady();
                },
                this.injectionContext.checkReadyIntervalMs);
        } else {
            this.setupTagManager();
            this.createInjections();
        }
    }

    private setupTagManager(): void {
        if (window['dataLayer'] && window['dataLayer']['push']) {
            this.tagManagerIsEnabled = true;
            this.dataLayer = window['dataLayer'];
            if (window['uBindTagManagerTrackProperties']) {
                this.trackedFormModelValues = window['uBindTagManagerTrackProperties']['formFields'] || [];
                this.trackedCalculationValues = window['uBindTagManagerTrackProperties']['calculation'] || [];
            }
        }
    }

    private createInjections(): void {
        this.createWebFormInjections();
        this.createPortalInjections();
        document.addEventListener('keydown', this.onXKeydownRevealIFrame.bind(this));
    }

    private createWebFormInjections(): void {
        const elements: HTMLCollectionOf<Element> = document.getElementsByClassName('ubind-product');
        for (const element of Array.from(elements) as Array<HTMLElement>) {
            const autoEmbed: boolean = element.getBooleanAttributeValue('data-auto-embed', true);
            if (autoEmbed) {
                this.loadWebFormElement(element);
            }
        }
    }

    /**
     * 
     * @param element the html element (e.g. div) where the form should be injected,
     * and which contains the relevant data parameters
     * @param formModel 
     * @returns the id unique to this injection
     */
    public loadWebFormElement(element: HTMLElement, embedOptions?: WebFormEmbedOptions): string {
        if (!embedOptions) {
            embedOptions = <WebFormEmbedOptions>{};
        }

        if (!element.id) {
            element.id = 'ubind-product';
        }

        const parentUrl: string = this.getParentUrl();
        const isLoadedWithinPortal: boolean = element.getBooleanAttributeValue('data-isLoadedWithinPortal', false);

        // The embed options on the tag are only used where they weren't passed in as embed options using javascript.
        // The javascript options takes precedence over tag attributes.
        embedOptions.title = embedOptions.title ?? element.getStringAttributeValue('data-title');
        embedOptions.environment = embedOptions.environment ?? element.getStringAttributeValue('data-environment');
        embedOptions.formType = embedOptions.formType ?? element.getStringAttributeValue('data-formType');
        embedOptions.quoteId = embedOptions.quoteId ?? element.getStringAttributeValue('data-quoteId');
        embedOptions.policyId = embedOptions.policyId ?? element.getStringAttributeValue('data-policyId');
        embedOptions.claimId = embedOptions.claimId ?? element.getStringAttributeValue('data-claimId');
        embedOptions.portal = embedOptions.portal
            ?? element.getStringAttributeValue('data-portal')
            ?? element.getStringAttributeValue('data-portal-id');
        embedOptions.quoteType = embedOptions.quoteType ?? element.getStringAttributeValue('data-quoteType');
        embedOptions.sidebarOffset = embedOptions.sidebarOffset
            ?? element.getStringAttributeValue('data-sidebar-offset');
        embedOptions.mode = embedOptions.mode ?? element.getStringAttributeValue('data-mode');
        embedOptions.isTestData = embedOptions.isTestData ?? element.getBooleanAttributeValue('data-istestdata', false);
        embedOptions.debug = embedOptions.debug ?? element.getBooleanAttributeValue('data-debug', false);
        embedOptions.debugLevel = embedOptions.debugLevel ?? element.getNumberAttributeValue('data-debug-level');
        embedOptions.autoResize = embedOptions.autoResize
            ?? element.getBooleanAttributeValue('data-iframeresizer-enabled', true);
        embedOptions.productRelease = embedOptions.productRelease
            ?? element.getStringAttributeValue('data-productRelease');
        let quoteVersionStr: string = element.getStringAttributeValue('data-version');
        if (quoteVersionStr) {
            embedOptions.quoteVersion = Number.parseInt(quoteVersionStr, 10);
        }

        embedOptions.tenant = embedOptions.tenant
            ?? element.getStringAttributeValue('data-tenant')
            ?? element.getStringAttributeValue('data-tenant-id');
        embedOptions.product = embedOptions.product
            ?? element.getStringAttributeValue('data-product')
            ?? element.getStringAttributeValue('data-product-id');
        embedOptions.organisation = embedOptions.organisation
            ?? element.getStringAttributeValue('data-organisation');
        const portalOrganisation: string | null = embedOptions.portalOrganisation
            ?? element.getStringAttributeValue('data-portalOrganisation');

        if (embedOptions.tenant == null && embedOptions.product == 'DepositAssure-Concierge') {
            embedOptions.tenant = 'deposit-assure';
            embedOptions.product = 'concierge';
        }

        embedOptions.loaderBackground
            = embedOptions.loaderBackground ?? element.getStringAttributeValue('data-loader-background', undefined);
        embedOptions.accentColor1
            = embedOptions.accentColor1 ?? element.getStringAttributeValue('data-accent-color-1', undefined);
        embedOptions.accentColor2
            = embedOptions.accentColor2 ?? element.getStringAttributeValue('data-accent-color-2', undefined);
        embedOptions.accentColor3
            = embedOptions.accentColor3 ?? element.getStringAttributeValue('data-accent-color-3', undefined);
        embedOptions.accentColor4
            = embedOptions.accentColor4 ?? element.getStringAttributeValue('data-accent-color-4', undefined);
        embedOptions.modalPopup
            = embedOptions.modalPopup ?? element.getBooleanAttributeValue('data-modal-popup', false);
        embedOptions.modalZIndex
            = embedOptions.modalZIndex ?? element.getNumberAttributeValue('data-modal-z-index', undefined);
        embedOptions.minimumHeight
            = embedOptions.minimumHeight ?? element.getStringAttributeValue('data-minimum-height', undefined);
        embedOptions.succeededRedirectUrl
            = embedOptions.succeededRedirectUrl
            ?? element.getStringAttributeValue('data-succeeded-redirect-url', undefined);
        embedOptions.abortedRedirectUrl
            = embedOptions.abortedRedirectUrl
            ?? element.getStringAttributeValue('data-aborted-redirect-url', undefined);
        embedOptions.scrollInsidePopup
            = embedOptions.scrollInsidePopup ?? element.getBooleanAttributeValue('data-scroll-inside-popup', undefined);
        embedOptions.modalBackdrop
            = embedOptions.modalBackdrop ?? element.getBooleanAttributeValue('data-modal-backdrop', undefined);
        embedOptions.width
            = embedOptions.width ?? element.getStringAttributeValue('data-width', undefined);
        embedOptions.minimumWidth
            = embedOptions.minimumWidth ?? element.getStringAttributeValue('data-minimum-width', undefined);
        embedOptions.fullWidthBelowPixels
            = embedOptions.fullWidthBelowPixels
            ?? element.getNumberAttributeValue('data-full-width-below-pixels', undefined);
        embedOptions.maximumWidth
            = embedOptions.maximumWidth ?? element.getStringAttributeValue('data-maximum-width', undefined);
        embedOptions.maximumHeight
            = embedOptions.maximumHeight ?? element.getStringAttributeValue('data-maximum-height', undefined);
        embedOptions.borderRadius
            = embedOptions.borderRadius ?? element.getStringAttributeValue('data-border-radius', undefined);
        embedOptions.paddingXs
            = embedOptions.paddingXs ?? element.getStringAttributeValue('data-padding-xs', undefined);
        embedOptions.paddingSm
            = embedOptions.paddingSm ?? element.getStringAttributeValue('data-padding-sm', undefined);
        embedOptions.paddingMd
            = embedOptions.paddingMd ?? element.getStringAttributeValue('data-padding-md', undefined);
        embedOptions.paddingLg
            = embedOptions.paddingLg ?? element.getStringAttributeValue('data-padding-lg', undefined);
        embedOptions.paddingXl
            = embedOptions.paddingXl ?? element.getStringAttributeValue('data-padding-xl', undefined);
        embedOptions.paddingXxl
            = embedOptions.paddingXxl ?? element.getStringAttributeValue('data-padding-xxl', undefined);

        const seedFormDataJson: string = element.getStringAttributeValue('data-seed-form-data', undefined);
        if (embedOptions.seedFormData == null && seedFormDataJson) {
            embedOptions.seedFormData = JSON.parse(decodeURIComponent(seedFormDataJson));
        }
        const overwriteFormDataJson: string = element.getStringAttributeValue('data-overwrite-form-data', undefined);
        if (embedOptions.overwriteFormData == null && overwriteFormDataJson) {
            embedOptions.overwriteFormData = JSON.parse(decodeURIComponent(overwriteFormDataJson));
        }

        const succeededCallbackFuncName: string = element.getStringAttributeValue('data-succeeded-callback', undefined);
        if (embedOptions.succeededCallback == null && succeededCallbackFuncName) {
            embedOptions.succeededCallback = window[succeededCallbackFuncName];
        }
        const abortedCallbackFuncName: string = element.getStringAttributeValue('data-aborted-callback', undefined);
        if (embedOptions.abortedCallback == null && abortedCallbackFuncName) {
            embedOptions.abortedCallback = window[abortedCallbackFuncName];
        }

        const injection: WebFormInjection = new WebFormInjection(
            element,
            embedOptions,
            parentUrl,
            isLoadedWithinPortal,
            portalOrganisation,
            this.injectionContext);
        const frameId: string = injection.getIframeId();
        this.injections.set(frameId, injection);
        element.setAttribute('data-injection-id', frameId);
        return frameId;
    }

    /**
     * Loads a web form in a popup modal.
     * @param embedOptions The options
     * @returns the id unique to this injection
     */
    public loadWebForm(embedOptions: WebFormEmbedOptions): HTMLDivElement {
        let divEl: HTMLDivElement = document.createElement('div');
        document.body.appendChild(divEl);
        embedOptions.modalPopup = true;
        this.loadWebFormElement(divEl, embedOptions);
        return divEl;
    }

    private createPortalInjections(): void {
        const elements: HTMLCollectionOf<Element> = document.getElementsByClassName('ubind-portal');
        // eslint-disable-next-line @typescript-eslint/prefer-for-of
        for (let i: number = 0; i < elements.length; i++) {
            const element: HTMLElement = <HTMLElement>elements[i];
            const autoEmbed: boolean = element.getBooleanAttributeValue('data-auto-embed', true);
            if (autoEmbed) {
                this.loadPortalElement(element);
            }
        }
    }

    public loadPortalElement(element: HTMLElement, embedOptions?: PortalEmbedOptions): string {
        if (!embedOptions) {
            embedOptions = <PortalEmbedOptions>{};
        }

        if (!element.id) {
            element.id = 'ubind-portal';
        }

        if (embedOptions.fullScreen == null && embedOptions.fullScreen == undefined) {
            embedOptions.fullScreen = true;
        }

        embedOptions.tenant = embedOptions.tenant
            ?? element.getStringAttributeValue('data-tenant')
            ?? element.getStringAttributeValue('data-tenant-id');
        embedOptions.organisation = embedOptions.organisation ?? element.getStringAttributeValue('data-organisation');
        embedOptions.environment = embedOptions.environment ?? element.getStringAttributeValue('data-environment');
        embedOptions.portal = embedOptions.portal
            ?? element.getStringAttributeValue('data-portal')
            ?? element.getStringAttributeValue('data-portal-id');
        embedOptions.path = embedOptions.path ?? this.getPortalPath(element);
        this.overrideEnvironmentBasedOnPath(embedOptions);
        const parentUrl: string = this.getParentUrl();
        const injection: PortalInjection = new PortalInjection(
            element, embedOptions, parentUrl, this.injectionContext);
        const frameId: string = injection.getIframeId();
        this.injections.set(frameId, injection);
        element.setAttribute('data-injection-id', frameId);
        return frameId;
    }

    public loadPortal(embedOptions?: PortalEmbedOptions): HTMLDivElement {
        let divEl: HTMLDivElement = document.createElement('div');
        divEl.id = 'ubind-portal';
        document.body.appendChild(divEl);
        this.loadPortalElement(divEl, embedOptions);
        return divEl;
    }

    private overrideEnvironmentBasedOnPath(embedOptions: PortalEmbedOptions) {
        if (embedOptions.path) {
            let map: Map<string, string> = UrlHelper.gatherQueryParamsFromIncompletePath(embedOptions.path);
            let environment: string = map.get("environment");
            if (environment) {
                embedOptions.environment = environment;
            }
        }
    }

    private getPortalPath(element: HTMLElement): string {
        let path: string = element.getStringAttributeValue('data-path');
        if (!path) {
            const currentUrl: string = window.location.search;
            path = currentUrl.includes('?path') ? currentUrl.split('path=')[1] : currentUrl;
            if (!path.includes('?')) {
                path = path.replace('&', '?');
            }
        }
        if (path && path.startsWith('/')) {
            path = path.substring(1);
        }
        return path;
    }

    private handleFrameLoadingEvent(data: InterFrameMessage): void {
        let injection: Injection = this.injections.get(data.frameId);
        if (injection != null) {
            injection.handleFrameLoadingEvent(data);
        } else {
            console.error('Received a frame loading event for ubind injection iframe with id \''
                + data.frameId + '\' however no ubind injection iframe with that id was found.');
        }
    }

    private handleAppLoadEvent(data: InterFrameMessage): void {
        document.removeEventListener('keydown', this.onXKeydownRevealIFrame);
        let injection: Injection = this.injections.get(data.frameId);
        if (injection != null) {
            injection.handleAppLoadEvent(data);
        } else {
            console.error('Received an app load event for ubind injection iframe with id \''
                + data.frameId + '\' however no ubind injection iframe with that id was found.');
        }
    }

    private handleConfigurationLoadedEvent(data: InterFrameMessage): void {
        let injection: Injection = this.injections.get(data.frameId);
        if (injection != null && this.isWebFormInjection(injection)) {
            (<WebFormInjection>injection).handleConfigurationLoadedEvent(data);
        } else {
            console.error('Received a configuration loaded event for ubind injection iframe with id \''
                + data.frameId + '\' however no ubind injection iframe with that id was found.');
        }
    }

    private handleWebFormLoadEvent(data: InterFrameMessage): void {
        let injection: Injection = this.injections.get(data.frameId);
        if (injection != null && this.isWebFormInjection(injection)) {
            (<WebFormInjection>injection).handleWebFormLoadEvent(data);
        } else {
            console.error('Received an app load event for ubind injection iframe with id \''
                + data.frameId + '\' however no ubind injection iframe with that id was found.');
        }
    }

    private isWebFormInjection(instance: Injection): instance is WebFormInjection {
        return instance['handleWebFormLoadEvent'] != null;
    }

    private handleScrollToElementEvent(data: any): void {
        let injection: Injection = this.injections.get(data.frameId);
        if (injection == null) {
            console.error('Received a scroll to element event for ubind injection iframe with id \''
                + data.frameId + '\' however no ubind injection iframe with that id was found.');
        } else if (injection instanceof WebFormInjection) {
            (<WebFormInjection>injection).handleScrollToElementEvent(data);
        } else {
            console.error('Received a scroll to element event for ubind injection iframe with id \''
                + data.frameId
                + '\' however that ubind iframe id refers to a non web form iframe (e.g. a  portal iframe).');
        }
    }

    private removeToken() {
        // is not in iframe
        if (window.self === window.top) {
            sessionStorage.removeItem('ubind.accessToken');
        }
    }

    private onXKeydownRevealIFrame(event: KeyboardEvent): void {
        if (event.key == 'd') {
            this.injections.forEach((injection: Injection) => {
                if (this.isWebFormInjection(injection)) {
                    (<WebFormInjection>injection).handleWebFormLoadEvent(<InterFrameMessage>{
                        payload: {
                            status: 'success',
                        },
                    });
                    (<WebFormInjection>injection).handleDebuggingMode();
                }
            });
        }
    }

    private getParentUrl(): string {
        let parentUrl: string = document.location.href;
        let queryStringStart: number = parentUrl.indexOf('?');
        if (queryStringStart > 0) {
            parentUrl = parentUrl.substring(0, queryStringStart);
        }
        return parentUrl;
    }
}

if (_uBindInjector == null) {
    _uBindInjector = new UBindInjector();
    window['_uBindInjector'] = _uBindInjector;
    _uBindInjector.detectAndInject();
}
