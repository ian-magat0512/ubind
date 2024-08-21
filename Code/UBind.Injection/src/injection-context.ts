/**
 * Holds and retreives contextual information about an injection and how to inject it.
 */
export class InjectionContext {
    public isIos: boolean;
    public isAndroid: boolean = navigator.userAgent.match(/android/i)
        && navigator.userAgent.match(/android/i).length > 0;
    public isIE11: boolean = !((window as any).ActiveXObject) && "ActiveXObject" in window;
    public uBindAppHost: string;
    public uBindAssetUrl: string;
    public uBindAppPath: string = '/index.html';
    public slowLoadingThresholdMs: number = 5000;
    public checkReadyIntervalMs: number = 100;
    public noLoadThresholdMs: number = 40000;
    public iframeStartedLoadingThresholdMs: number = 15000;

    // Mobile keyboard related
    protected mobileDeviceBreakpoint: number = 1024; // px

    // Iframeresizer
    public iFrameResizerOptions: any = {
        log: false, // Enable console logging 
        autoResize: true,

        // We will calculate the height of the element with the attribute "data-iframe-height".
        // This is fast and accurate. We had problems with the height of the body which seems to expand
        // under certain conditions for certain products, which leaves a lot of whitespace at the bottom
        heightCalculationMethod: 'taggedElement',
    };

    public constructor() {
        this.determineIfIos();
    }

    public findGetParameter(parameterName: string, locationSearch?: string): string {
        let result: string = null;
        let tmp: Array<string> = [];
        const locationSearchToUse: string = locationSearch || window.location.search;
        locationSearchToUse
            .substr(1)
            .split("&")
            .forEach((item: string) => {
                tmp = item.split("=");
                if (tmp[0] === parameterName) {
                    result = decodeURIComponent(tmp[1]);
                }
            });
        return result;
    }

    public getElementPosition(elementRef: HTMLElement): number {
        let yPos: number = 0;
        while (elementRef) {
            if (elementRef.tagName == 'BODY') {
                yPos += (elementRef.offsetTop + elementRef.clientTop);
            } else {
                yPos += (elementRef.offsetTop - elementRef.scrollTop + elementRef.clientTop);
            }
            elementRef = <HTMLElement>elementRef.offsetParent;
        }
        return yPos;
    }

    public mobileKeyboardIsVisible(event: any): boolean {
        const viewportWidth: number = event.target.innerWidth;
        const viewportHeight: number = event.target.innerHeight;
        if (viewportWidth < this.mobileDeviceBreakpoint && viewportHeight < this.mobileDeviceBreakpoint) {
            return true;
        } else {
            return false;
        }
    }

    private determineIfIos(): void {
        // implementation from https://stackoverflow.com/a/62094756/9883553
        let iosQuirkPresent: () => boolean = () => {
            let audio: HTMLAudioElement = new Audio();
            let originalVolume: number = audio.volume;
            audio.volume = 0.5;
            let result: boolean = audio.volume === 1;   // volume cannot be changed from "1" on iOS 12 and below
            audio.volume = originalVolume;
            return result;
        };
        let isIDeviceUserAgent: boolean = /iPad|iPhone|iPod/.test(navigator.userAgent);
        const isAppleDevice: boolean = navigator.userAgent.includes('Macintosh');
        const isTouchScreen: boolean = navigator.maxTouchPoints >= 1;   // true for iOS 13 (and hopefully beyond)
        this.isIos = isIDeviceUserAgent || (isAppleDevice && (isTouchScreen || iosQuirkPresent()));
    }
}
