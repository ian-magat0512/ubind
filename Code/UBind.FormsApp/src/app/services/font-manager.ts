import { Injectable } from "@angular/core";
import { GoogleFont } from "@app/models/configuration/settings";
import { WorkingConfiguration } from "@app/models/configuration/working-configuration";
import { ConfigService } from "./config.service";
import { EventService } from "./event.service";

/**
 * Loads fonts such as google fonts for use within the form
 */
@Injectable({
    providedIn: 'root',
})
export class FontManager {

    private baseUrl: string = 'https://fonts.googleapis.com/css?family=';
    private fontsUrl: string;

    public constructor(
        private configService: ConfigService,
        eventService: EventService,
    ) {
        eventService.loadedConfiguration.subscribe((config: WorkingConfiguration) => this.applyGoogleFonts());
        eventService.updatedConfiguration.subscribe((config: WorkingConfiguration) => this.applyGoogleFonts());
    }

    private applyGoogleFonts(): void {
        const oldFontsUrl: string = this.fontsUrl;
        let newFontsUrl: string = this.generateFontsUrl();
        if (oldFontsUrl == newFontsUrl) {
            // nothing has changed
            return;
        }
        if (oldFontsUrl && !newFontsUrl) {
            // delete the existing link tag, since all google fonts have been removed
            let linkEl: HTMLLinkElement = <HTMLLinkElement>document.getElementById('google-fonts-stylesheet');
            linkEl.remove();
        } else if (oldFontsUrl && newFontsUrl) {
            // update the existing link href since the google fonts have been updated
            let linkEl: HTMLLinkElement = <HTMLLinkElement>document.getElementById('google-fonts-stylesheet');
            linkEl.href = newFontsUrl;
        } else if (newFontsUrl) {
            // add the stylesheet for google fonts
            let head: HTMLHeadElement = document.head || document.getElementsByTagName('head')[0];
            let el: HTMLLinkElement = document.createElement('link');
            el.id = 'google-fonts-stylesheet';
            el.rel = 'preload';
            el.rel = 'stylesheet';
            el.href = newFontsUrl;
            head.appendChild(el);
        }
        this.fontsUrl = newFontsUrl;
    }

    private generateFontsUrl(): string {
        if (this.configService.theme?.googleFonts?.length) {
            let url: string = this.baseUrl;
            const families: Set<string> = this.getDistinctFamilies();
            let firstFamily: boolean = true;
            for (let family of families) {
                const weights: Set<string> = this.getWeightsForFamily(family);
                url += firstFamily ? '' : '|';
                url += family.replace(/ /g, '+');
                url += ':' + Array.from(weights).join(',');
                firstFamily = false;
            }
            return url;
        } else {
            return null;
        }
    }

    private getDistinctFamilies(): Set<string> {
        let families: Set<string> = new Set<string>();
        if (this.configService.theme?.googleFonts) {
            this.configService.theme.googleFonts.forEach((font: GoogleFont) => families.add(font.family));
        }
        return families;
    }

    private getWeightsForFamily(family: string): Set<string> {
        let weights: Set<string> = new Set<string>();
        if (this.configService.theme?.googleFonts) {
            this.configService.theme.googleFonts.forEach((font: GoogleFont) => {
                if (font.family == family) {
                    weights.add(font.weight);
                }
            });
        }
        return weights;
    }
}
