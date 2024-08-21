import { InjectionContext } from './injection-context';
import { Injection } from './injection';
import { InterFrameMessage } from './models/inter-frame-message';
import { WebFormEmbedOptions } from './models/web-form-embed-options';
import * as iFrameResize from 'iframe-resizer/js/iframeResizer';
import { IFrameOptions, IFrameResizedData } from 'iframe-resizer';
import { scrollbarStyle } from './scrollbar-style';
import { ScrollToElementInfo } from './models/scroll-to-element-info';

/**
 * Represents the embedding of a web form into a page.
 */
export class WebFormInjection extends Injection {

    private scrollStartPosition: number;
    private scrollEndPosition: number;
    private ubindFrameContainer: HTMLElement;
    private originalBodyOverflow: string;
    private originalBodyOverflowX: string;
    private originalBodyOverflowY: string;
    private closeButtonEl: HTMLDivElement;
    private contentEl: HTMLDivElement;

    public constructor(
        containingElement: HTMLElement,
        private embedOptions: WebFormEmbedOptions,
        parentUrl: string,
        private isLoadedWithinPortal: boolean,
        private portalOrganisation: string | null,
        injectionContext: InjectionContext
    ) {
        super(
            containingElement,
            containingElement,
            embedOptions.tenant,
            embedOptions.portal,
            embedOptions.organisation,
            embedOptions.environment,
            parentUrl,
            injectionContext,
        );
        this.init();
    }

    protected init(): void {
        if (this.embedOptions.modalPopup) {
            this.containingElement.classList.add('ubind-modal');

            this.originalBodyOverflow = window.document.body.style.overflow;
            this.originalBodyOverflowX = window.document.body.style.overflowX;
            this.originalBodyOverflowY = window.document.body.style.overflowY;
            window.document.body.style.overflow = 'hidden';

            // write in the necessary divs and css
            this.layoutWebFormModal();

            // ensure we listen to the scroll events so we can reposition the sidebar
            this.rootElement.addEventListener(
                "scroll",
                (event: any) => this.handleScrollEvent(event),
                false);

            // if maximumHeight is used, the scrolling will happen on the content element
            this.contentEl.addEventListener(
                "scroll",
                (event: any) => this.handleScrollEvent(event),
                false);
        }
        super.init();
    }

    protected buildIframeUrl(): string {
        let url: string = '';
        const portal: string = this.portal ?? this.injectionContext.findGetParameter('portal');
        const formType: string = this.embedOptions.formType ?? this.injectionContext.findGetParameter("formType");
        const quoteId: string = this.embedOptions.quoteId ?? this.injectionContext.findGetParameter("quoteId");
        const policyId: string = this.embedOptions.policyId ?? this.injectionContext.findGetParameter("policyId");
        const claimId: string = this.embedOptions.claimId ?? this.injectionContext.findGetParameter("claimId");
        const mode: string = this.embedOptions.mode ?? this.injectionContext.findGetParameter("mode");
        const quoteVersion: number
            = this.embedOptions.quoteVersion ?? Number.parseInt(this.injectionContext.findGetParameter("version"), 10);
        const quoteType: string = this.embedOptions.quoteType ?? this.injectionContext.findGetParameter("quoteType");
        const sidebarOffsetConfiguration: string = this.embedOptions.sidebarOffset
            ?? this.injectionContext.findGetParameter("sidebarOffset");
        const testMode: boolean = this.embedOptions.isTestData != null
            ? this.embedOptions.isTestData
            : this.injectionContext.findGetParameter("testData") != null
                ? this.injectionContext.findGetParameter("testData").toUpperCase() == 'TRUE'
                : false;
        const debug: boolean = this.embedOptions.debug != null
            ? this.embedOptions.debug
            : this.injectionContext.findGetParameter("debug") != null
                ? this.injectionContext.findGetParameter("debug").toUpperCase() == 'TRUE'
                : false;
        const debugLevel: number = this.embedOptions.debugLevel
            || Number.parseInt(this.injectionContext.findGetParameter('debugLevel'), 10);
        const isLoadedWithinPortal: boolean = this.isLoadedWithinPortal != null
            ? this.isLoadedWithinPortal
            : this.injectionContext.findGetParameter("isLoadedWithinPortal") != null
                ? this.injectionContext.findGetParameter("isLoadedWithinPortal").toUpperCase() == 'TRUE'
                : false;
        const productRelease: string = this.embedOptions.productRelease
            ?? this.injectionContext.findGetParameter("productRelease");
        return this.injectionContext.uBindAppHost + this.injectionContext.uBindAppPath
            + '?tenant=' + this.tenant
            + (this.organisation ? '&organisation=' + this.organisation : '')
            + (this.portalOrganisation ? '&portalOrganisation=' + this.portalOrganisation : '')
            + (portal ? '&portal=' + portal : '')
            + '&product=' + this.embedOptions.product
            + '&environment=' + this.environment
            + '&frameId=' + this.getIframeId()
            + (formType ? '&formType=' + formType : '')
            + (quoteId ? '&quoteId=' + quoteId : '')
            + (policyId ? '&policyId=' + policyId : '')
            + (claimId ? '&claimId=' + claimId : '')
            + (quoteVersion ? '&version=' + quoteVersion : '')
            + (quoteType ? url += '&quoteType=' + quoteType : '')
            + (sidebarOffsetConfiguration ? url += '&sidebarOffset=' + sidebarOffsetConfiguration : '')
            + (mode ? '&mode=' + mode : '')
            + (testMode ? '&testMode=' + testMode : '')
            + (debug ? '&debug=' + debug : '')
            + (debugLevel ? '&debugLevel=' + debugLevel : '')
            + (isLoadedWithinPortal ? '&isLoadedWithinPortal=' + isLoadedWithinPortal : '')
            + (productRelease ? '&productRelease=' + productRelease : '');
    }

    public handleScrollEvent(event: any): void {
        this.handleScrollAndResizeEvent(event, 'scroll');
    }

    public handleResizeEvent(event: any): void {
        this.handleScrollAndResizeEvent(event, 'resize');
        if (this.embedOptions.modalPopup) {
            this.adjustPopupWidth();
            this.adjustPopupOffset();
        }
    }

    public get product(): string {
        return this.embedOptions.product;
    }

    public get mode(): string {
        return this.embedOptions.mode;
    }

    public get formType(): string {
        return this.embedOptions.formType;
    }

    public getIframeId(): string {
        return `ubind-webform-iframe---${this.tenant}${this.organisation ? '---' + this.organisation : ''}`
            + `---${this.product}---${this.environment}${this.embedOptions.formType ? '---' + this.formType : ''}`;
    }

    public getIframeTitle(): string {
        return this.embedOptions.title || `${this.tenant} ${this.embedOptions.product}`;
    }

    public handleAppLoadEvent(data: InterFrameMessage): void {
        const payload: any = data?.payload;
        if (payload?.status == 'failure') {
            super.handleAppLoadEvent(data);
        }
        this.embedOptions = this.applyDefaults(this.embedOptions);
        const cleanEmbedOptions: WebFormEmbedOptions = this.cleanEmbedOptions(this.embedOptions);
        this.sendMessage('embedOptions', cleanEmbedOptions);
    }

    private applyDefaults(embedOptions: WebFormEmbedOptions): WebFormEmbedOptions {
        if (embedOptions.modalPopup) {
            embedOptions.paddingXs = embedOptions.paddingXs ?? "10px";
            embedOptions.paddingSm = embedOptions.paddingSm ?? "15px";
            embedOptions.paddingMd = embedOptions.paddingMd ?? "20px";
        }
        return this.embedOptions;
    }

    /**
     * Since WebFormEmbedOptions can contain a javascript function, we need to clean it and make sure it's
     * only JSON before sending it as a postMessage, otherwise it will never be received.
     */
    private cleanEmbedOptions(embedOptions: WebFormEmbedOptions): WebFormEmbedOptions {
        return JSON.parse(JSON.stringify(embedOptions));
    }

    public handleConfigurationLoadedEvent(data: InterFrameMessage): void {
        const payload: any = data?.payload;
        if (payload?.status == 'failure') {
            super.handleAppLoadEvent(data);
        }
    }

    public handleWebFormLoadEvent(data: InterFrameMessage): void {
        const payload: any = data?.payload;
        if (payload?.status == 'success') {
            this.initiateIframeResizer();
        }
        super.handleAppLoadEvent(data);
    }

    public handleDebuggingMode(): void {
        const data: any = {
            'messageType': 'debug',
        };
        this.getIframeWindow().postMessage(data, this.injectionContext.uBindAppHost);
    }

    public handleScrollToElementEvent(data: any): void {
        if (this.embedOptions.modalPopup) {
            this.scrollPopupToElement(data.payload);
        } else {
            this.scrollWindowToElement(data.payload);
        }
    }

    protected getScrollOffset(): number {
        if (this.embedOptions.modalPopup) {
            if (this.embedOptions.scrollInsidePopup) {
                return this.containingElement.scrollTop;
            } else {
                return this.rootElement.scrollTop;
            }
        }
        return super.getScrollOffset();
    }

    private scrollPopupToElement(info: ScrollToElementInfo): void {
        const elementToScroll: HTMLElement = this.embedOptions.scrollInsidePopup
            ? this.containingElement
            : this.rootElement;
        const documentHeight: number = this.contentEl.scrollHeight;
        const viewportHeight: number = this.contentEl.offsetHeight;
        let scrollStartPosition: number = this.getScrollOffset();
        let scrollEndPosition: number = this.calculateScrollEndPosition(
            scrollStartPosition,
            info.elementPositionPixels,
            info.scrollMarginPixels,
            info.visibleContentStartPixels,
            info.elementHeightPixels,
            viewportHeight,
            documentHeight);
        this.scrollElementTo(
            elementToScroll,
            scrollStartPosition,
            scrollEndPosition,
            info.notifyWhenScrollingFinished,
            info.behaviour);
    }

    private scrollWindowToElement(info: ScrollToElementInfo): void {
        let iframePosition: number = this.getIframePosition();

        if (isNaN(iframePosition)) {
            console.error('Unable to get the iframe scrolling position, so we could not scroll to the element.');
            return;
        }

        this.ubindFrameContainer = document.getElementById('ubindFrameContainer');
        const body: HTMLElement = document.body;
        const html: HTMLElement = document.documentElement;

        let documentHeight: number;
        if (this.ubindFrameContainer) {
            documentHeight = Math.max(
                this.iframeElement.scrollHeight, this.iframeElement.offsetHeight, this.iframeElement.clientHeight);
        } else {
            documentHeight = Math.max(
                body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
        }

        let elementPosition: number = info.elementPositionPixels;
        const elementHeight: number = info.elementHeightPixels;
        const visibleContentStartPixels: number = info.visibleContentStartPixels;

        const viewportHeight: number = this.ubindFrameContainer
            ? this.ubindFrameContainer.parentElement.clientHeight
            : window.innerHeight - visibleContentStartPixels;

        const scrollMargin: number = info.scrollMarginPixels;
        elementPosition += iframePosition;
        let scrollStartPosition: number = this.getScrollOffset();
        let scrollEndPosition: number = this.calculateScrollEndPosition(
            scrollStartPosition,
            elementPosition,
            scrollMargin,
            visibleContentStartPixels,
            elementHeight,
            viewportHeight,
            documentHeight);
        this.scrollElementTo(
            window,
            scrollStartPosition,
            scrollEndPosition,
            info.notifyWhenScrollingFinished,
            info.behaviour);
    }

    private scrollElementTo(
        element: Window | HTMLElement,
        scrollStartPosition: number,
        scrollEndPosition: number,
        notifyWhenScrollingFinished: boolean,
        behavior: ScrollBehavior,
    ) {
        if (scrollStartPosition == scrollEndPosition) {
            if (notifyWhenScrollingFinished) {
                this.notifyChildFrameThatScrollingHasFinished(scrollEndPosition);
            }
            return;
        }
        this.scrollStartPosition = scrollStartPosition;
        this.scrollEndPosition = scrollEndPosition;
        element.scrollTo({
            left: 0,
            top: this.scrollEndPosition,
            behavior: behavior,
        });
        if (notifyWhenScrollingFinished) {
            if (element instanceof Window) {
                this.notifyChildFrameThatScrollingHasFinished(scrollEndPosition);
            } else {
                this.whenScrollReachesTargetNotifyScrollingFinished(this.scrollStartPosition, this.scrollEndPosition);
            }
        }
        this.notifyContainerOfNewScrollPosition(this.scrollEndPosition);
        this.scrollEndPosition = null;
    }

    private handleScrollAndResizeEvent(event: any, messageType: string) {
        let iframePosition: number = this.getIframePosition();
        let verticalScrollAmountPixels: number = 0;
        let scrollOffset: number = 0;
        if (this.embedOptions.modalPopup === true) {
            scrollOffset = this.contentEl.scrollTop;
            if (scrollOffset > 0) {
                verticalScrollAmountPixels = scrollOffset;
            } else {
                verticalScrollAmountPixels = scrollOffset - iframePosition;
            }
        } else {
            scrollOffset = this.getScrollOffset();
            verticalScrollAmountPixels = scrollOffset - iframePosition;
        }
        const payload: any = {
            'scrollOffset': scrollOffset,
            'verticalScrollAmountPixels': verticalScrollAmountPixels,
            'mobileKeyboardVisible': this.injectionContext.mobileKeyboardIsVisible(event),
        };
        this.sendMessage(messageType, payload);
    }

    private whenScrollReachesTargetNotifyScrollingFinished(
        scrollStartPosition: number, scrollEndPosition: number
    ): void {
        let scrollingUp: boolean = scrollEndPosition < scrollStartPosition;
        let count: number = 0;
        // eslint-disable-next-line no-undef
        let interval: NodeJS.Timeout = setInterval(() => {
            count++;
            if (count > 30) {
                console.error('Timed out waiting to scroll to the right position. Giving up.');
            }
            let currentOffset: number = this.getScrollOffset();
            if ((scrollingUp && currentOffset < scrollEndPosition + 20) ||
                (!scrollingUp && currentOffset > scrollEndPosition - 20) ||
                count > 30
            ) {
                clearInterval(interval);
                this.notifyChildFrameThatScrollingHasFinished(scrollEndPosition);
            }
        }, 150);
    }

    /**
     * 
     * @param currentScrollOffset The the number of pixels of the document that are hidden
     * @param targetElementStartingPosition The starting position of the element we want to scroll into view
     * @param scrollMargin The amount of space to show above the element when scrolling up.
     * E.g. overscroll up by this much
     * @param visibleContentStartPixels 
     * @param elementHeight 
     * @param viewportHeight 
     * @param documentHeight 
     * @returns 
     */
    private calculateScrollEndPosition(
        currentScrollOffset: number,
        targetElementStartingPosition: number,
        scrollMargin: number,
        visibleContentStartPixels: number,
        elementHeight: number,
        viewportHeight: number,
        documentHeight: number,
    ): number {
        let scrollEndPosition: number;
        // if element is above the viewport
        let targetElementStartingPositionWithTopMargin: number
            = Math.max(targetElementStartingPosition - scrollMargin, 0);
        if (targetElementStartingPositionWithTopMargin < currentScrollOffset) {
            // scroll up to element
            scrollEndPosition = targetElementStartingPositionWithTopMargin - visibleContentStartPixels;
            // make sure we don't scroll to before where the document starts though
            scrollEndPosition = Math.max(scrollEndPosition, 0);
            return scrollEndPosition;
        }
        // if element is below the viewport
        let bottomOfTargetElement: number = targetElementStartingPosition + elementHeight;
        let documentPositionAtBottomOfViewport: number = currentScrollOffset + viewportHeight;
        if (bottomOfTargetElement > documentPositionAtBottomOfViewport) {
            // if the whole element fits in the viewport
            let heightOfTargetElementWithTopAndBottomMargin: number = elementHeight + (2 * scrollMargin);
            if (heightOfTargetElementWithTopAndBottomMargin < viewportHeight) {
                // move the viewport down so element is just visible at the bottom of the viewport, with bottom margin
                let bottomOfTargetElementWithBottomMargin: number = bottomOfTargetElement + scrollMargin;
                scrollEndPosition = bottomOfTargetElementWithBottomMargin - viewportHeight - visibleContentStartPixels;
                // make sure we don't scroll to before where the document starts though
                scrollEndPosition = Math.max(scrollEndPosition, 0);
                // make sure we don't scroll past the height of the document though
                scrollEndPosition = Math.min(documentHeight, scrollEndPosition);
                return scrollEndPosition;
            } else {
                // move the viewport down so the top of the element is visible within the viewport
                scrollEndPosition = targetElementStartingPositionWithTopMargin - visibleContentStartPixels;
                // make sure we don't scroll to before where the document starts though
                scrollEndPosition = Math.max(scrollEndPosition, 0);
                return scrollEndPosition;
            }
        }

        // element is already in full view
        scrollEndPosition = currentScrollOffset;
        return scrollEndPosition;
    }

    private notifyContainerOfNewScrollPosition(newScrollPosition: number): void {
        const data: any = {
            'messageType': 'frameContainerScrollTo',
            'payload': {
                newScrollPosition: newScrollPosition,
            },
        };
        if (this.ubindFrameContainer && parent) {
            if (parent == window) {
                window.postMessage(data, this.injectionContext.uBindAppHost);
            } else {
                // when the parent is equal to `global` object which occur on  
                // embedding multiple iframe, navigate to first element which is the `window`.
                parent[0].postMessage(data, this.injectionContext.uBindAppHost);
            }
        }
        this.getIframeWindow().postMessage(data, this.injectionContext.uBindAppHost);

    }

    private notifyChildFrameThatScrollingHasFinished(scrollEndPosition: number): void {
        const data: any = {
            'messageType': 'scrollingFinished',
            'payload': {
                scrollEndPosition: scrollEndPosition,
            },
        };
        this.getIframeWindow().postMessage(data, this.injectionContext.uBindAppHost);
    }

    public initiateIframeResizer(): void {
        if (this.embedOptions.autoResize) {
            iFrameResize(this.getIframeResizerOptions(), `#${this.getIframeId()}`);
        }
        this.revealIframe();
        if (this.closeButtonEl) {
            this.closeButtonEl.style.display = 'none';
        }
    }

    protected getIframeResizerOptions(): IFrameOptions {
        let options: any = this.injectionContext.iFrameResizerOptions;

        // if we're inside a modal popup, we want to know when it's resized so we can reposition it.
        if (this.embedOptions.modalPopup) {
            options.onResized = this.onIframeResized.bind(this);
        }
        return options;
    }

    public handleCloseApp(data?: any): void {
        if (!this.isLoadedWithinPortal) {
            this.closeEmbeddedFrame(data);
        }

        if (data?.payload?.state == 'complete') {
            // trigger the succeded event
            const newEvent: CustomEvent = new CustomEvent('succeeded', { detail: data.payload.data });
            this.rootElement.dispatchEvent(newEvent);

            if (this.embedOptions.succeededCallback) {
                this.embedOptions.succeededCallback(data.payload.data);
            }
            if (this.embedOptions.succeededRedirectUrl) {
                window.location.href = this.embedOptions.succeededRedirectUrl;
            }
        } else {
            // trigger the aborted event
            const newEvent: CustomEvent = new CustomEvent('aborted', { detail: data?.payload?.data });
            this.rootElement.dispatchEvent(newEvent);

            if (this.embedOptions.abortedCallback) {
                this.embedOptions.abortedCallback(data?.payload?.data);
            }
            if (this.embedOptions.abortedRedirectUrl) {
                window.location.href = this.embedOptions.abortedRedirectUrl;
            }
        }
    }

    protected closeEmbeddedFrame(data?: any): void {
        if (data?.payload?.message && !this.embedOptions.modalPopup) {
            this.messageElement.style.display = 'block';
            this.displayMessage(data.payload.message, data.payload.severity ? data.payload.severity : 3);
        } else if (!this.embedOptions.modalPopup) {
            this.fadeOutEffect();
        }
        if (this.embedOptions.modalPopup) {
            const speed: string = '200ms';
            const cssBlock: string = `
                #${this.rootElement.id} .modal-content {
                    transition:
                        margin-top ${speed} ease-out,
                        width ${speed} ease-out,
                        min-width ${speed} ease-out,
                        min-height ${speed} ease-out,
                        max-width ${speed} ease-out,
                        max-height ${speed} ease-out,
                        opacity ${speed} ease-out;
                }
            `;
            let styleElement: HTMLStyleElement = document.createElement('style');
            styleElement.innerHTML = cssBlock;
            document.head.appendChild(styleElement);

            // hide the iframe content
            this.iframeContainer.style.display = 'none';

            // animate the box smaller.
            this.contentEl.style.height = '80px';
            this.contentEl.style.maxWidth = '80px';
            this.contentEl.style.maxHeight = '80px';
            this.contentEl.style.minHeight = '80px';
            this.contentEl.style.minWidth = '80px';
            this.contentEl.style.opacity = '0.8';

            // remove the background
            this.rootElement.style.backgroundColor = 'rgba(0, 0, 0, 0%)';

            // wait for animation out to complete
            setTimeout(() => {
                this.contentEl.style.display = 'none';
                window.document.body.style.overflow = this.originalBodyOverflow;
                window.document.body.style.overflowX = this.originalBodyOverflowX;
                window.document.body.style.overflowY = this.originalBodyOverflowY;

                // wait for the background to fade out before hiding the root element completely
                setTimeout(() => {
                    this.rootElement.style.display = 'none';
                }, 60);
            }, 210);
        }
    }

    private onIframeResized(messageData: IFrameResizedData): void {
        if (this.embedOptions.modalPopup) {
            this.adjustPopupOffset();
        }
    }

    /**
     * Makes the popup full height if the iframe is taller than the screen
     */
    private adjustPopupOffset(): void {
        // we need a 250 second delay because we need to wait for the css animation to finish.
        setTimeout(() => {
            let windowHeight: number = window.innerHeight;
            let offsetTop: number = this.contentEl.offsetTop;
            let bottomOfPopup: number = this.contentEl.offsetHeight + offsetTop;

            // we should try to leave 20px at the bottom
            if (offsetTop > 0) {
                if (bottomOfPopup >= windowHeight - 20) {
                    this.contentEl.style.marginTop = '0';
                    this.contentEl.classList.add('docked-top');
                }
            } else if (bottomOfPopup < windowHeight - 80) {
                this.contentEl.style.marginTop = '80px';
                this.contentEl.classList.remove('docked-top');
            }

            if (bottomOfPopup >= windowHeight) {
                this.contentEl.classList.add('docked-bottom');
            } else {
                this.contentEl.classList.remove('docked-bottom');
            }
        }, 250);
    }

    /**
     * Makes the popup full width if the iframe width is greater than the fullWidthBelowPixels threshold
     */
    private adjustPopupWidth(): void {
        let windowWidth: number = window.innerWidth;
        if (windowWidth < this.embedOptions.fullWidthBelowPixels) {
            this.contentEl.style.width = '100%';
            this.contentEl.classList.add('docked-left', 'docked-right');
        } else {
            this.contentEl.style.width = this.embedOptions.width;
            this.contentEl.classList.remove('docked-left', 'docked-right');
        }
    }

    /**
     * Generates HTML and css to layout the web form modal
     */
    private layoutWebFormModal(): void {

        this.rootElement = this.containingElement;

        if (this.embedOptions.scrollInsidePopup === undefined) {
            this.embedOptions.scrollInsidePopup = true;
        }
        if (this.embedOptions.width === undefined) {
            this.embedOptions.width = '80%';
        }
        if (this.embedOptions.minimumWidth === undefined) {
            this.embedOptions.minimumWidth = '320px';
        }
        if (this.embedOptions.minimumHeight === undefined) {
            this.embedOptions.minimumHeight = '115px';
        }
        if (this.embedOptions.maximumWidth === undefined) {
            this.embedOptions.maximumWidth = '1000px';
        }
        if (this.embedOptions.borderRadius === undefined) {
            this.embedOptions.borderRadius = '5px';
        }
        if (this.embedOptions.loaderBackground === undefined) {
            this.embedOptions.loaderBackground = 'white';
        }
        if (this.embedOptions.fullWidthBelowPixels === undefined) {
            this.embedOptions.fullWidthBelowPixels = 576;
        }
        if (this.embedOptions.modalZIndex === undefined) {
            this.embedOptions.modalZIndex = 10001;
        }

        // add the "custom" class to the root element so that it can be used to create more specific
        // selectors, so override styles
        this.rootElement.classList.add('custom');

        let cssBlock: string = `
            #${this.rootElement.id} {
                display: block;
                position: fixed;
                z-index: ${this.embedOptions.modalZIndex};
                left: 0;
                top: 0;
                width: 100%;
                height: 100%;
                overflow: auto;
                transition: background-color 250ms ease-in-out;
            }

            #${this.rootElement.id} .modal-content {
                background: ${this.embedOptions.loaderBackground};
                margin-left: auto;
                margin-right: auto;
                margin-top: 80px;
                width: 80px;
                min-width: 80px;
                min-height: 80px;
                max-width: ${this.embedOptions.maximumWidth};
                transition:
                    margin-top 500ms ease-in-out,
                    width 1s ease-in-out,
                    min-width 1s ease-in-out,
                    min-height 1s ease-in-out,
                    max-width 500ms ease-in-out,
                    max-height 500ms ease-in-out;
                box-shadow: 0 9px 46px 8px rgba(0, 0, 0, 14%),
                    0 11px 15px -7px rgba(0, 0, 0, 12%),
                    0 24px 38px  3px rgba(0, 0, 0, 20%);
            }

            #${this.rootElement.id} .modal-content, #${this.rootElement.id} .modal-content iframe {
                border-top-left-radius: ${this.embedOptions.borderRadius};
                border-top-right-radius: ${this.embedOptions.borderRadius};
                border-bottom-left-radius: ${this.embedOptions.borderRadius};
                border-bottom-right-radius: ${this.embedOptions.borderRadius};
            }

            #${this.rootElement.id} .modal-content.docked-top,
            #${this.rootElement.id} .modal-content.docked-top iframe {
                border-top-left-radius: 0;
                border-top-right-radius: 0;
            }

            #${this.rootElement.id} .modal-content.docked-bottom,
            #${this.rootElement.id} .modal-content.docked-bottom iframe {
                border-bottom-left-radius: 0;
                border-bottom-right-radius: 0;
            }

            #${this.rootElement.id} .modal-content.docked-left,
            #${this.rootElement.id} .modal-content.docked-left iframe {
                border-top-left-radius: 0;
                border-bottom-left-radius: 0;
            }

            #${this.rootElement.id} .modal-content.docked-right,
            #${this.rootElement.id} .modal-content.docked-right iframe {
                border-top-right-radius: 0;
                border-bottom-right-radius: 0;
            }
            
            ${scrollbarStyle}`;

        // the following is needed to reduce the additional height of the iframe so it doesn't show the loader
        // background. This is likely due to an inaccuracy of iframeresizer.
        cssBlock += `
            #${this.rootElement.id} .modal-content iframe {
                margin-bottom: -4px;
            }`;

        if (this.embedOptions.maximumHeight !== undefined) {
            cssBlock += `
                #${this.rootElement.id} .modal-content {
                    max-height: min(100%, ${this.embedOptions.maximumHeight});
                    overflow-y: auto;
                }`;
        } else if (this.embedOptions.scrollInsidePopup) {
            cssBlock += `
                #${this.rootElement.id} .modal-content {
                    max-height: 100%;
                    overflow-y: auto;
                }`;
        }

        let styleElement: HTMLStyleElement = document.createElement('style');
        styleElement.innerHTML = cssBlock;
        document.head.appendChild(styleElement);

        // write in the new content element
        this.contentEl = document.createElement('div');
        this.contentEl.classList.add('modal-content');
        this.rootElement.appendChild(this.contentEl);

        setTimeout(() => {
            this.adjustPopupWidth();
            this.contentEl.style.minWidth = this.embedOptions.minimumWidth;
            this.contentEl.style.minHeight = `min(100%, ${this.embedOptions.minimumHeight})`;

            if (this.embedOptions.modalBackdrop !== false) {
                this.rootElement.style.backgroundColor = 'rgba(0, 0, 0, 50%)';
            }

            // after the animation has completed we might need to get rid of the top margin
            setTimeout(() => this.adjustPopupOffset(), 800);
        }, 0);

        this.containingElement = this.contentEl;

        // create close button in the top right, so you can close the loader even whilst loading
        this.createCloseButton();
    }

    public createCloseButton(): void {
        this.containingElement.style.position = 'relative';
        this.closeButtonEl = document.createElement('div');
        this.closeButtonEl.addEventListener(
            "click",
            (event: any) => this.handleCloseApp(),
            false);
        this.closeButtonEl.classList.add('close-button-cross');
        this.closeButtonEl.style.width = '20px';
        this.closeButtonEl.style.height = '20px';
        this.closeButtonEl.style.position = 'absolute';
        this.closeButtonEl.style.right = '0';
        this.closeButtonEl.style.top = '0';
        this.closeButtonEl.style.padding = '10px';
        this.closeButtonEl.style.border = 'none';
        this.closeButtonEl.style.background = 'transparent';
        this.closeButtonEl.style.opacity = '0.0';
        this.closeButtonEl.style.transition = 'opacity 0.5s ease-in';
        // eslint-disable-next-line max-len
        let crossRounded: string = `<path d="M310.6 150.6c12.5-12.5 12.5-32.8 0-45.3s-32.8-12.5-45.3 0L160 210.7 54.6 105.4c-12.5-12.5-32.8-12.5-45.3 0s-12.5 32.8 0 45.3L114.7 256 9.4 361.4c-12.5 12.5-12.5 32.8 0 45.3s32.8 12.5 45.3 0L160 301.3 265.4 406.6c12.5 12.5 32.8 12.5 45.3 0s12.5-32.8 0-45.3L205.3 256 310.6 150.6z"/>`;
        this.closeButtonEl.innerHTML = `
            <div class="close-symbol" style="height: 20px; width: 20px;">
                <svg viewbox="0 90 320 340">
                    ${crossRounded}
                </svg>
            </div>`;
        this.containingElement.appendChild(this.closeButtonEl);

        let cssBlock: string = `
            .close-button-cross > .close-symbol > svg {
                fill: #d5d5d5;
            }
            .close-button-cross:hover > .close-symbol > svg {
                fill: #b0b0b0;
            }`;
        let styleElement: HTMLStyleElement = document.createElement('style');
        styleElement.innerHTML = cssBlock;
        document.head.appendChild(styleElement);

        setTimeout(() => {
            this.closeButtonEl.style.opacity = '1.0';
        }, 1000);
    }
}
