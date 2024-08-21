import { Injectable } from "@angular/core";
import { ParentFrameMessageService } from "./parent-frame-message.service";

/**
 * Service to redirect a user elsewhere.
 * This is done intelligently taking into account whether the portal is framed or not.
 */
@Injectable({
    providedIn: 'root',
})
export class RedirectService {

    public constructor(
        private parentFrameMessageService: ParentFrameMessageService,
    ) {}

    public redirectToUrl(url: string): void {
        if (window.self !== window.top) {
            this.parentFrameMessageService.sendRedirectMessage(url);
        } else {
            window.location.href = url;
        }
    }

    public getBaseHref(): string {
        const baseEls: HTMLCollectionOf<HTMLBaseElement> = document.getElementsByTagName('base');
        let baseEl: HTMLBaseElement;
        if (baseEls && baseEls.length > 0) {
            baseEl = baseEls[0];
            return baseEl.href;
        }

        console.error("Unable to get the base href tag from index.html");
        return '/portal/';
    }
}
