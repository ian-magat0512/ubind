import { Injectable, EventEmitter, Output } from '@angular/core';
import { UrlHelper } from '@app/helpers';
import { EventService } from './event.service';

/**
 * Sends and receives messages to/from the parent frame, if any.
 */
@Injectable({ providedIn: 'root' })
export class ParentFrameMessageService {

    @Output() public modelUpdate: EventEmitter<any> = new EventEmitter<any>();
    @Output() public windowResize: EventEmitter<any> = new EventEmitter<any>();

    private parentUrl: string;
    private frameId: string;
    private originOfParent: string;

    public constructor(
        private eventService: EventService,
    ) {
        window.addEventListener('message', (event: any) => {
            this.receiveMessage(event);
        }, false);

        const el: HTMLBodyElement = document.getElementsByTagName('body')[0];
        this.parentUrl = decodeURI(this.findGetParameter('data-parent-url'));
        this.frameId = this.findGetParameter('frameId') || el.getAttribute('data-frame-id');
        if (this.parentUrl && this.parentUrl != "null") {
            this.originOfParent = new URL(this.parentUrl).origin;
        }

        this.eventService.routeChangedSubject$.subscribe((url: string) => {
            this.sendPathMessageToParentFrame(url);
        });
    }

    public sendMessage(messageType: string, payload: any): void {
        try {
            let data: any = {
                'messageType': messageType,
                'frameId': this.frameId,
                'payload': payload,
            };
            window.parent.postMessage(data, this.originOfParent);
        } catch (err) {
        }
    }

    public sendDisplayMessage(messageToDisplay: string, severity: number = 4, status: string = 'failure'): void {
        const payload: any = {
            'status': status,
            'message': messageToDisplay,
            'severity': severity,
        };
        this.sendMessage('displayMessage', payload);
    }

    public sendPathMessageToParentFrame(path: string): void {
        if (window.self !== window.top) {
            path = UrlHelper.extractPathUrl(path);
            const payload: any = {
                path: path,
            };
            this.sendMessage('urlChanged', payload);
        }
    }

    private receiveMessage(event: any): void {
        if (event.data.messageType === 'updateModel' ||
            event.data.type === 'updateModel') {
        // TODO: remove 'type reference' and update script on PromoInABox
            this.modelUpdate.emit(event.data);
        }
        if (event.data.messageType === 'resize') {
            this.windowResize.emit(event.data);
        }
    }

    public getParentUrl(): string {
        return this.parentUrl;
    }

    public findGetParameter(parameterName: any): string {
        let result: any = null;
        let tmp: Array<any> = [];
        location.search
            .substr(1)
            .split('&')
            .forEach((item: string) => {
                tmp = item.split('=');
                if (tmp[0] === parameterName) {
                    result = decodeURIComponent(tmp[1]);
                }
            });
        return result;
    }

    public sendRedirectMessage(url: string): void {
        const payload: any = {
            'url': url,
        };
        this.sendMessage('redirect', payload);
    }
}
