import { Injectable } from "@angular/core";
import { filter } from "rxjs/operators";
import { ConfigService } from "./config.service";
import { EventService } from "./event.service";

/**
 * Generates some html with icon classes so they are preloaded, so that they don't load when fields are rendered.
 */
@Injectable({
    providedIn: 'root',
})
export class IconPreloader {

    public constructor(
        private configService: ConfigService,
        eventService: EventService,
    ) {
        eventService.loadedConfiguration.pipe(filter((processed: any) => processed))
            .subscribe((processed: any) => this.preloadIcons());
    }

    private preloadIcons(): void {
        if (this.configService.configuration?.icons?.length) {
            let body: HTMLHeadElement = document.body || document.getElementsByTagName('body')[0];
            let div: HTMLDivElement = document.createElement('div');
            div.className = 'preloaded-icons';
            for (let icon of this.configService.configuration.icons) {
                let iconElement: HTMLElement = document.createElement('i');
                iconElement.className = icon;
                div.appendChild(iconElement);
            }
            body.appendChild(div);
        }
    }
}
