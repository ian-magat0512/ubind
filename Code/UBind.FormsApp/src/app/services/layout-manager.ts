import { Injectable } from "@angular/core";
import { WebFormEmbedOptions } from "@app/models/web-form-embed-options";

/**
 * Loads iframe resizer to ensure iframes maintain the correct width and height.
 */
@Injectable({
    providedIn: 'root',
})
export class LayoutManager {

    public applyLayoutOptions(embedOptions: WebFormEmbedOptions) {
        let css: string = '';
        if (embedOptions.minimumHeight) {
            document.body.style.minHeight = embedOptions.minimumHeight;
        }
        if (embedOptions.paddingSm || embedOptions.paddingMd || embedOptions.paddingLg
            || embedOptions.paddingXl || embedOptions.paddingXxl
        ) {
            if (embedOptions.modalPopup) {
                document.body.classList.add('modal-popup');
            }
            if (embedOptions.paddingXs) {
                css += `
                    .layout-manager app {
                        padding: ${embedOptions.paddingXs};
                    }\n`;
            }
            if (embedOptions.paddingSm) {
                css += this.getCssForAppPadding(576, embedOptions.paddingSm);
            }
            if (embedOptions.paddingMd) {
                css += this.getCssForAppPadding(768, embedOptions.paddingMd);
            }
            if (embedOptions.paddingLg) {
                css += this.getCssForAppPadding(992, embedOptions.paddingLg);
            }
            if (embedOptions.paddingXl) {
                css += this.getCssForAppPadding(1200, embedOptions.paddingXl);
            }
            if (embedOptions.paddingXxl) {
                css += this.getCssForAppPadding(1400, embedOptions.paddingXxl);
            }
        }
        css += this.getCssForAccentColours(embedOptions);
        this.applyLayoutCss(css);
    }

    private applyLayoutCss(css: string): void {
        // ensure we have a targeting class
        document.body.classList.add("layout-manager");

        // remove the old css first if it exists
        let el: HTMLStyleElement = <HTMLStyleElement>document.getElementById('embedded-layout-styles');
        if (el) {
            el.remove();
        }

        // insert new css
        let head: HTMLHeadElement = document.head || document.getElementsByTagName('head')[0];
        el = document.createElement('style');
        el.id = 'embedded-layout-styles';
        el.appendChild(document.createTextNode(css));
        head.appendChild(el);
    }

    private getCssForAppPadding(breakPointPixels: number, padding: string, minMaxWidth: string = 'min-width'): string {
        return `
            @media (${minMaxWidth}: ${breakPointPixels}px) {
                .layout-manager app {
                    padding: ${padding};
                }
            }\n`;
    }

    private getCssForAccentColours(embedOptions: WebFormEmbedOptions): string {
        let css: string = ':root, :root body * {\n';
        if (embedOptions.accentColor1) {
            css += `    --accent-color-1: ${embedOptions.accentColor1} !important;\n`;
        }
        if (embedOptions.accentColor2) {
            css += `    --accent-color-2: ${embedOptions.accentColor2} !important;\n`;
        }
        if (embedOptions.accentColor3) {
            css += `    --accent-color-3: ${embedOptions.accentColor3} !important;\n`;
        }
        if (embedOptions.accentColor4) {
            css += `    --accent-color-4: ${embedOptions.accentColor4} !important;\n`;
        }
        css += '}\n';
        return css;
    }
}
