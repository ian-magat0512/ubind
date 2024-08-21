import { StatusBar } from '@ionic-native/status-bar';
import { SplashScreen } from '@ionic-native/splash-screen';

/**
 * Export platform mock class
 */
export class PlatformMock {
    public ready(): Promise<string> {
        return new Promise((resolve: any): void => {
            resolve('READY');
        });
    }

    public getQueryParam(): boolean {
        return true;
    }

    public registerBackButtonAction(fn: Function, priority?: number): Function {
        return ((): boolean => true);
    }

    public hasFocus(ele: HTMLElement): boolean {
        return true;
    }

    public doc(): HTMLDocument {
        return document;
    }

    public is(): boolean {
        return true;
    }

    public getElementComputedStyle(container: any): any {
        return {
            paddingLeft: '10',
            paddingTop: '10',
            paddingRight: '10',
            paddingBottom: '10',
        };
    }

    public onResize(callback: any): any {
        return callback;
    }

    public registerListener(ele: any, eventName: string, callback: any): Function {
        return ((): boolean => true);
    }

    public win(): Window {
        return window;
    }

    public raf(callback: any): number {
        return 1;
    }

    public timeout(callback: any, timer: number): any {
        return setTimeout(callback, timer);
    }

    public cancelTimeout(id: any): void {
        // do nothing
    }

    public getActiveElement(): any {
        return document['activeElement'];
    }
}

/**
 * Export Nav mock class
 */
export class NavMock {

    public pop(): any {
        return new Promise((resolve: Function): void => {
            resolve();
        });
    }

    public push(): any {
        return new Promise((resolve: Function): void => {
            resolve();
        });
    }

    public getActive(): any {
        return {
            'instance': {
                'model': 'something',
            },
        };
    }

    public setRoot(): any {
        return true;
    }

    public registerChildNav(nav: any): void {
        return;
    }

}

/**
 * Export deep linker mock class
 */
export class DeepLinkerMock {

}
