import { Injectable, EventEmitter, Output, Directive } from '@angular/core';
import { InterFrameMessage } from '@app/models/inter-frame-message';
import { WebFormEmbedOptions } from '@app/models/web-form-embed-options';
import { ApplicationService } from './application.service';
import { EventService } from './event.service';
import { LayoutManager } from './layout-manager';
import { UnifiedFormModelService } from './unified-form-model.service';

/**
 * Export message service class.
 * TODO: Write a better class header: message service functions.
 */
@Directive()
@Injectable()

export class MessageService {

    @Output() public modelUpdate: EventEmitter<any> = new EventEmitter<any>();
    @Output() public windowResize: EventEmitter<any> = new EventEmitter<any>();

    public constructor(
        private eventService: EventService,
        private unifiedFormModelService: UnifiedFormModelService,
        private layoutManager: LayoutManager,
        private applicationService: ApplicationService,
    ) {
        window.addEventListener('message', (event: any) => {
            this.receiveMessage(event);
        }, false);
    }

    public sendMessage(messageType: string, payload?: any): void {
        try {
            let data: InterFrameMessage = {
                'messageType': messageType,
                'frameId': this.findGetParameter('frameId'),
                'payload': payload,
            };

            let postDomainSlashPosition: number = document.referrer.indexOf('/', 8);
            let originOfParent: string = postDomainSlashPosition == -1
                ? document.referrer
                : document.referrer.substring(0, postDomainSlashPosition);
            window.parent.postMessage(data, originOfParent);
        } catch (err) {
        }
    }

    private receiveMessage(event: any): void {
        if (event.data.messageType == 'updateModel' || event.data.type == 'updateModel') {
            // TODO: remove 'type reference' and update script on PromoInABox
            this.modelUpdate.emit(event.data);
        } else if (event.data.messageType == 'resize') {
            this.windowResize.emit(event.data);
        } else if (event.data.messageType == 'scrollingFinished') {
            this.eventService.scrollingFinishedSubject.next(true);
        } else if (event.data.messageType == 'embedOptions') {
            const embedOptions: WebFormEmbedOptions = event.data.payload;
            this.applicationService.embedOptions = embedOptions;
            const seedFormData: any = embedOptions.seedFormData;
            if (seedFormData) {
                this.unifiedFormModelService.apply(seedFormData);
            }
            const overwriteFormData: any = embedOptions.overwriteFormData;
            if (overwriteFormData) {
                this.unifiedFormModelService.overwriteFormData = overwriteFormData;
            }
            this.layoutManager.applyLayoutOptions(embedOptions);
        } else if (event.data.messageType == 'appMinimumHeight') {
            this.eventService.appMinimumHeightSubject.next(event.data.payload.minimumHeight);
        }
    }

    private findGetParameter(parameterName: string): string {
        let result: string = null;
        let tmp: Array<any> = [];
        location.search
            .substr(1)
            .split('&')
            .forEach((item: string) => {
                tmp = item.split('=');
                if (tmp[0] == parameterName) {
                    result = decodeURIComponent(tmp[1]);
                }
            });
        return result;
    }
}
