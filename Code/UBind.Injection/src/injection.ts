import { InjectionContext } from './injection-context';

/**
 * Represents something to be embedded and loaded.
 */
export abstract class Injection {

    protected loaderElement: HTMLDivElement;
    protected iframeContainer: HTMLDivElement;
    protected iframeElement: HTMLIFrameElement;
    protected messageElement: HTMLDivElement;
    // eslint-disable-next-line no-undef
    private appSlowLoadTimer: NodeJS.Timeout;
    // eslint-disable-next-line no-undef
    private iframeSlowLoadTimer: NodeJS.Timeout;
    private hasIframeStartedLoading: boolean = false;
    protected iframeLoaded: boolean = false;
    protected loaded: boolean = false;
    protected errored: boolean = false;
    private currentMessage: string;
    private currentMessageSeverity: number = 0;
    protected loaderHidden: boolean = false;

    public constructor(
        public rootElement: HTMLElement,
        protected containingElement: HTMLElement,
        protected tenant: string,
        protected portal: string,
        protected organisation: string,
        protected environment: string,
        protected parentUrl: string,
        protected injectionContext: InjectionContext,
    ) {
    }

    protected init(): void {
        this.createLoaderElement();
        this.createMessageElement();
        this.createIframeSlowLoadTimer();
        this.createAppSlowLoadTimer();
        this.createIframeContainer();
        this.createIframeElement();
        this.createNoLoadTimer();
    }

    protected createIframeElement(): void {
        this.iframeElement = document.createElement('iframe');
        this.iframeElement.title = this.getIframeTitle();
        this.iframeElement.style.height = '100%';
        this.iframeElement.style.transition = 'height 200ms ease-out';
        this.iframeElement.src = this.buildIframeUrl();
        this.iframeElement.id = this.getIframeId();
        this.iframeElement.className = 'ubind-iframe';
        this.iframeElement.frameBorder = '0';
        this.iframeElement.scrolling = "yes";
        this.iframeElement.style.width = '100%';
        this.iframeElement.allow = 'clipboard-write';
        this.iframeElement.onload = this.onIframeLoad.bind(this);
        this.iframeElement.setAttribute('referrerpolicy', 'no-referrer-when-downgrade');
        if (this.injectionContext.isIos || this.injectionContext.isAndroid) {
            let data: any = {
                'messageType': 'iosAndAndroidTrigger',
                'payload': {},
            };
            const originOfChild: string = this.injectionContext.uBindAppHost;
            this.iframeElement.onload = () => {
                this.getIframeWindow().postMessage(data, originOfChild);
            };
            this.iframeElement.onload.bind(this);
        }
        this.iframeContainer.appendChild(this.iframeElement);
    }

    public onIframeLoad(): void {
        this.iframeLoaded = true;
    }

    public abstract getIframeId(): string;

    public abstract getIframeTitle(): string;

    protected createLoaderElement(fadeIn: boolean = true): void {
        this.loaderElement = document.createElement('div');
        this.loaderElement.style.width = '40px';
        this.loaderElement.style.margin = 'auto';
        if (fadeIn) {
            this.loaderElement.style.opacity = '0';
            this.loaderElement.style.transition = 'opacity 1.5s ease-in';
        }
        let circle: string = this.injectionContext.isIE11
            ? `<circle class="ie" cx="20" cy="20" r="15" fill-opacity="0.0"></circle>`
            : `<circle cx="20" cy="20" r="15" fill-opacity="0.0"></circle>`;
        this.loaderElement.innerHTML = `
           <div class="material-preloader">
                <svg class="custom-loader-md" width="40" height="40" fill-opacity="0.0">
                    ${circle}
                </svg>
            </div>`;
        this.containingElement.appendChild(this.loaderElement);

        if (fadeIn) {
            setTimeout(() => this.loaderElement.style.opacity = '1', 0);
        }
    }

    protected changeLoaderToCross(): void {
        let cross: string = `<path stroke="#909090" stroke-width="2.4" d="m2.4,2.4 10.2,10.2m0-10.2-10.2,10.2" />`;
        this.loaderElement.innerHTML = `
           <div class="material-preloader failure-symbol" style="height: 40px; width: 40px;">
                <svg viewbox="0 0 15 15" fill-opacity="0.0">
                    ${cross}
                </svg>
            </div>`;
    }

    protected createIframeContainer(): void {
        this.iframeContainer = document.createElement('div');
        this.iframeContainer.style.display = 'none';
        this.containingElement.appendChild(this.iframeContainer);
    }

    public fadeOutEffect() {
        const iframeContainer: HTMLDivElement = this.iframeContainer;
        const loaderElement: HTMLDivElement = this.loaderElement;
        const fadeTarget: HTMLDivElement = iframeContainer;
        // eslint-disable-next-line no-undef
        const fadeEffect: NodeJS.Timeout = setInterval(() => {
            if (!fadeTarget.style.opacity) {
                fadeTarget.style.opacity = '1';
            }
            const opacity: number = Number.parseFloat(fadeTarget.style.opacity);
            if (opacity > 0) {
                fadeTarget.style.opacity = (opacity - 0.1).toString();
            } else {
                iframeContainer.style.visibility = 'hidden';
                loaderElement.style.display = 'none';
                this.rootElement.style.display = 'none';
                clearInterval(fadeEffect);
                this.slideUpEffect(iframeContainer);
            }
        }, 15);
    }

    private slideUpEffect(iframeContainer: HTMLElement) {
        const elementTarget: HTMLElement = iframeContainer;
        elementTarget.style.height = elementTarget.offsetHeight + 'px';
        // eslint-disable-next-line no-undef
        const slideUpEffect: NodeJS.Timeout = setInterval(() => {
            if (elementTarget.offsetHeight > 80) {
                elementTarget.style.height = (elementTarget.offsetHeight - 50) + 'px';
            } else {
                clearInterval(slideUpEffect);
            }
        }, 15);
    }

    protected abstract buildIframeUrl(): string;

    public handleFrameLoadingEvent(data: any): void {
        this.hasIframeStartedLoading = true;
    }

    public handleAppLoadEvent(data: any): void {
        const payload: any = data.payload;
        if (payload.status == 'success') {
            this.loaded = true;
        } else if (payload.status == 'failure') {
            this.loaded = true;
            this.displayMessage(payload.message);
            this.iframeLoaded = true;
            this.hideLoader();
        } else {
            if (payload.message != null) {
                this.displayMessage(payload.message);
                console.error(data.frameId + ' failed to initialise: ' + payload.message);
            } else {
                console.error(data.frameId + ' failed to initialise.');
            }
        }
    }

    public hideLoader() {
        this.loaderHidden = true;
        this.loaderElement.style.display = 'none';
    }

    public revealIframe(): void {
        setTimeout(
            () => {
                let iframeContainer: HTMLDivElement = this.iframeContainer;
                iframeContainer.style.display = 'block';
                let loaderElement: HTMLDivElement = this.loaderElement;
                loaderElement.style.height = '0px';
                loaderElement.style.display = 'none';
                this.hideMessageElement();
                clearTimeout(this.appSlowLoadTimer);
            }, 100);
    }

    public handleCloseApp(data?: any): void {
        // no default implementation. Override to implement.
    }

    protected closeEmbeddedFrame(data: any): void {
        this.fadeOutEffect();
        if (data.payload.message) {
            this.loaderElement.style.display = 'block';
            this.displayMessage(data.payload.message, data.payload.severity ? data.payload.severity : 3);
        }
    }

    /*
     * Creates an element which can display a message, for example, when there's an error,
     * or when it's taking a long time to load
     */
    private createMessageElement(): void {
        let messageElement: HTMLDivElement = document.createElement('div');
        messageElement.style.fontFamily = 'sans-serif';
        messageElement.style.fontSize = '14px';
        messageElement.style.visibility = 'hidden';
        messageElement.style.textAlign = 'center';
        messageElement.style.color = 'rgb(144, 144, 144)';
        messageElement.id = 'message_-_' + this.getIframeId();
        messageElement.style.paddingBottom = '15px';
        messageElement.style.paddingLeft = '15px';
        messageElement.style.paddingRight = '15px';
        this.messageElement = messageElement;
        this.containingElement.appendChild(messageElement);
    }

    private createIframeSlowLoadTimer(): void {
        this.iframeSlowLoadTimer = setTimeout(() => {
            if (!this.hasIframeStartedLoading) {
                this.displayMessage("We couldn't load the uBind iFrame. It's possible that this website is not in the "
                    + "allowed list of deployment targets for this product. Please check the security restrictions "
                    + "in the product configuration, or get in touch with our support team.", 2);
            }
        }, this.injectionContext.iframeStartedLoadingThresholdMs);
    }

    private createAppSlowLoadTimer(): void {
        this.appSlowLoadTimer = setTimeout(() => {
            if (this.messageElement.style.visibility != 'visible') {
                this.displayMessage('Loading, please wait...', 1);
            }
        }, this.injectionContext.slowLoadingThresholdMs);
    }

    private createNoLoadTimer(): void {
        setTimeout(() => {
            if (!this.iframeLoaded && this.hasIframeStartedLoading) {
                console.error("The iframe didn't load after quite some time. Please check the embed parameters to "
                    + "ensure you have specified the correct tenant, product and environment.");
                this.displayMessage(`We're having trouble loading your embedded experience. If you're using a `
                    + 'slow internet connection, you can continue waiting, otherwise please get in touch with '
                    + `customer support so we can work out what's going wrong for you.`, 1);
            }
        }, this.injectionContext.noLoadThresholdMs);
    }

    /**
     * Displays a message in the message area.
     * @param message The mssage to be displayed
     * @param severity The severity of the message. Messages with higher severity will overwrite those with lower
     * severity. A message with lower severity will be ignored if a message with higher severity is already being shown.
     */
    public displayMessage(message: string, severity: number = 1): void {
        if (severity >= this.currentMessageSeverity) {
            let debug: boolean = this.injectionContext.findGetParameter('debug')?.toLowerCase() == 'true';
            if (debug) {
                message += ' Press d to reveal debug tools.';
            }
            this.messageElement.innerHTML = message;
            this.messageElement.style.visibility = 'visible';
            this.currentMessage = message;
            this.currentMessageSeverity = severity;
        } else {
            if (this.currentMessageSeverity > 0) {
                console.log('Did not display message "' + message + '" with severity "' + severity + '" because the '
                    + 'message "' + this.currentMessage + '" exists with a higher severity ('
                    + this.currentMessageSeverity + ').');
            }
        }

        if (severity > 1) {
            this.changeLoaderToCross();
        }
    }

    private hideMessageElement(): void {
        if (this.messageElement) {
            this.messageElement.style.height = '0px';
            this.messageElement.style.visibility = 'hidden';
            this.messageElement.style.display = 'none';
        }
    }

    public handleScrollEvent(event: any): void {
        let iframePosition: number = this.getIframePosition();
        let scrollOffset: number = this.getScrollOffset();
        let payload: any = {
            'verticalScrollAmountPixels': scrollOffset - iframePosition,
        };
        this.sendMessage('scroll', payload);
    }

    protected getIframePosition(): number {
        let iframePosition: number = this.injectionContext.getElementPosition(this.iframeElement);
        let ubindFrameContainer: HTMLElement = document.getElementById('ubindFrameContainer');
        if (ubindFrameContainer) {
            let frameContainerStart: number = ubindFrameContainer.getBoundingClientRect().y;
            iframePosition -= frameContainerStart;
        }
        return iframePosition;
    }

    public sendMessage(messageType: string, payload: any): void {
        try {
            const data: any = {
                'messageType': messageType,
                'payload': payload,
            };
            const originOfChild: string = this.injectionContext.uBindAppHost;
            this.getIframeWindow().postMessage(data, originOfChild);
        } catch (err) {
            console.log('Error posting message to child frame: ' + err);
        }
    }

    public handleResizeEvent(event: any): void {
        const payload: any = {
            'verticalScrollAmountPixels': this.getScrollOffset(),
            'mobileKeyboardVisible': this.injectionContext.mobileKeyboardIsVisible(event),
        };
        this.sendMessage('resize', payload);
    }

    protected getScrollOffset(): number {
        let ubindFrameContainer: HTMLElement = document.getElementById('ubindFrameContainer');
        return ubindFrameContainer ? ubindFrameContainer.scrollTop : window.scrollY;
    }

    protected getIframeWindow() {
        let doc: Document = window.document;
        if (this.iframeElement.contentWindow) {
            return this.iframeElement.contentWindow;
        }
        if ((<any>this.iframeElement).window) {
            return (<any>this.iframeElement).window;
        }
        if (!doc && this.iframeElement.contentDocument) {
            doc = this.iframeElement.contentDocument;
        }
        if (!doc && (<any>this.iframeElement).document) {
            doc = (<any>this.iframeElement).document;
        }
        if (doc && doc.defaultView) {
            return doc.defaultView;
        }
        if (doc && (<any>doc).parentWindow) {
            return (<any>doc).parentWindow;
        }

        console.log('Unable to get the window object of the injected iframe.');
        this.displayMessage("Unable to get the window object of the injected iframe. You may be using an unsupported "
            + "browser. Please contact customer support.", 2);
        this.changeLoaderToCross();

        return undefined;
    }
}
