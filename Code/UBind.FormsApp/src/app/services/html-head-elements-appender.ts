import { Injectable } from "@angular/core";
import { WorkingConfiguration } from "@app/models/configuration/working-configuration";
import { ConfigService } from "./config.service";
import { EventService } from "./event.service";

/**
 * Takes custom html snippets from the settings section of the configuration, and adds them to 
 * the html head element.
 */
@Injectable({
    providedIn: 'root',
})
export class HtmlHeadElementsAppender {

    private fontAwesomeKitId: string;

    public constructor(
        private configService: ConfigService,
        eventService: EventService,
    ) {
        eventService.loadedConfiguration.subscribe((config: WorkingConfiguration) => this.applyHtmlHeadElements());
        eventService.updatedConfiguration.subscribe((config: WorkingConfiguration) => this.applyHtmlHeadElements());
    }

    private applyHtmlHeadElements(): void {
        const oldFontAwesomeKitId: string = this.fontAwesomeKitId;
        let newFontAwesomeKitId: string = this.configService.theme?.fontAwesomeKitId;
        if (oldFontAwesomeKitId && !newFontAwesomeKitId) {
            // remove the existing fontawesome script tag
            let scriptEl: HTMLScriptElement = <HTMLScriptElement>document.getElementById('fontawesome-script-element');
            scriptEl.remove();
        } else if (oldFontAwesomeKitId && newFontAwesomeKitId) {
            // update the existing fontawesome script tag
            let scriptEl: HTMLScriptElement = <HTMLScriptElement>document.getElementById('fontawesome-script-element');
            scriptEl.src = `https://kit.fontawesome.com/${newFontAwesomeKitId}.js`;
        } else if (newFontAwesomeKitId) {
            // add the font awesome script
            let head: HTMLHeadElement = document.head || document.getElementsByTagName('head')[0];
            let script: HTMLScriptElement = document.createElement('script');
            script.type = 'text/javascript';
            script.src = `https://kit.fontawesome.com/${newFontAwesomeKitId}.js`;
            script.id = 'fontawesome-script-element';
            script.crossOrigin = 'anonymous';
            head.appendChild(script);
        }
        this.fontAwesomeKitId = newFontAwesomeKitId;
    }
}
