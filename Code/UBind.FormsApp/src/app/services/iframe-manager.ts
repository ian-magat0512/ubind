import { Injectable } from "@angular/core";
import { filter } from "rxjs/operators";
import { EventService } from "./event.service";

/**
 * Loads iframe resizer to ensure iframes maintain the correct width and height.
 */
@Injectable({
    providedIn: 'root',
})
export class IframeManager {

    public constructor(
        eventService: EventService,
    ) {
        eventService.loadedConfiguration.pipe(filter((processed: any) => processed))
            .subscribe((processed: any) => this.loadIframeResizer());
    }

    private loadIframeResizer(): void {
        const head: HTMLHeadElement = document.head || document.querySelector('head');
        let el: HTMLScriptElement = document.createElement('script');
        el.type = 'application/javascript';
        el.src = '/assets/iframeResizer.contentWindow.min.js';
        head.appendChild(el);

        // the following may be needed for when we inject a premium funding / payment iframe within the webForm.
        el = document.createElement('script');
        el.type = 'application/javascript';
        el.src = '/assets/iframeResizer.js';
        head.appendChild(el);
    }
}
