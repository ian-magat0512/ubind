import { Injectable } from "@angular/core";

/**
 * Tools to help with ionic issues.
 */
@Injectable({
    providedIn: 'root',
})
export class IonicHelper {
    // Workaround ion-segment issue on tab selection via keyboard navigation
    public static initIonSegmentButtons(): void {
        const ionSegmentButtons: any = document.querySelectorAll('ion-segment-button');
        ionSegmentButtons.forEach((sel: HTMLIonSegmentButtonElement) => {
            sel.addEventListener('focus', (event: FocusEvent) => {
                let tab: HTMLElement = event.currentTarget as HTMLElement;
                tab.classList.add("segment-button-focused");
            });
            sel.addEventListener('focusout', (event: FocusEvent) => {
                let tab: HTMLElement = event.currentTarget as HTMLElement;
                tab.classList.remove("segment-button-focused");
            });
        });
    }

    public static initIonSelectDefaults(): void {
        const ionSelects: any = document.querySelectorAll('ion-select');
        ionSelects.forEach((sel: HTMLIonSelectElement) => {
            sel.setAttribute('aria-label', 'sample');
        });
    }

    public static setAriaLabel($event: any): void {
        if ($event.target.ariaLabel) {
            (<HTMLElement>$event.target.shadowRoot.activeElement).setAttribute("aria-label", $event.target.ariaLabel);
        }
    }
}
