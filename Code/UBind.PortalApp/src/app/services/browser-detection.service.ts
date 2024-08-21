import { Injectable } from '@angular/core';

/**
 * Export browser detection service class.
 * TODO: Write a better class header: browser detection functions.
 */
@Injectable({
    providedIn: 'root',
})
export class BrowserDetectionService {

    private _isIos: boolean;

    public constructor() {
        this.determineIfIos();
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

    public get isMobile(): boolean {
        let isMobileDevice: boolean =
            /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i
                .test(navigator.userAgent);
        return isMobileDevice;
    }
}
