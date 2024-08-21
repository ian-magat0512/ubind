import { Injectable } from '@angular/core';
import { ApplicationService } from './application.service';
import { ConfigService } from './config.service';
import { EventService } from './event.service';

/**
 * Detects the browser user agent, device types, and other things from the browser.
 */
@Injectable()
export class BrowserDetectionService {

    private _isIos: boolean;
    private viewportWidth: number;

    public constructor(
        private configService: ConfigService,
        private applicationService: ApplicationService,
        private eventService: EventService,
    ) {
        this.determineIfIos();
        this.listenForWindowResize();
    }

    public get currentBrowser(): string {
        if ((this.currentUserAgent.indexOf("opera") || this.currentUserAgent.indexOf('opr')) != -1) {
            return 'Opera';
        } else if (this.currentUserAgent.indexOf("chrome") != -1) {
            return 'Chrome';
        } else if (this.currentUserAgent.indexOf("safari") != -1) {
            return 'Safari';
        } else if (this.currentUserAgent.indexOf("firefox") != -1) {
            return 'Firefox';
        } else if ((this.currentUserAgent.indexOf("msie") != -1) || (!!this.documentNode == true)) {
            return 'IE';
        } else {
            return 'Unknown';
        }
    }

    public get currentUserAgent(): string {
        return navigator.userAgent.toLowerCase();
    }

    public get documentNode(): number {
        return document.DOCUMENT_NODE;
    }

    private determineIfIos(): void {
        // implementation from https://stackoverflow.com/a/62094756/9883553
        let iosQuirkPresent: () => boolean = (): boolean => {
            let audio: HTMLAudioElement = new Audio();
            let originalVolume: number = audio.volume;
            audio.volume = 0.5;
            let result: boolean = audio.volume === 1;   // volume cannot be changed from "1" on iOS 12 and below
            audio.volume = originalVolume;
            return result;
        };
        let isIDeviceUserAgent: boolean = /iPad|iPhone|iPod/.test(navigator.userAgent);
        let isAppleDevice: boolean = navigator.userAgent.includes('Macintosh');
        let isTouchScreen: boolean = navigator.maxTouchPoints >= 1;   // true for iOS 13 (and hopefully beyond)
        this._isIos = isIDeviceUserAgent || (isAppleDevice && (isTouchScreen || iosQuirkPresent()));
    }

    public get isIos(): boolean {
        return this._isIos;
    }

    private listenForWindowResize(): void {
        this.eventService.windowResizeSubject.subscribe(() => {
            if (this.applicationService.isLoadedWithinPortal) {
                // normally window.innerWidth includes the scrollbar width, however when we are loading it within the
                // portal, the scroll bar is in the parent frame, so we have to get the width of the container.
                let ubindFrameContainer: HTMLElement = window.parent.document.getElementById('ubindFrameContainer');
                this.viewportWidth = ubindFrameContainer.offsetWidth;
            } else {
                this.viewportWidth = window.innerWidth;
            }
        });
    }

    /**
     * Calculates whether we are in mobile size, according to the configured mobile breakpoint
     * If inside the portal, it takes into account the width of the scrollbar.
     */
    public isMobileWidth(): boolean {
        return this.viewportWidth <= this.configService.mobileBreakPointPixels;
    }
}
