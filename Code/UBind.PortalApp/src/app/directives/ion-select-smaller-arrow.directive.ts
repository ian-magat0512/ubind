/**
 * Author: Sam Roose
 */
import { Directive, ElementRef } from '@angular/core';

/**
 * Changes the arrow on a select/drop-list to make it smaller.
 */
@Directive({
    selector: '[ionSelectSmallerArrow]',
})
export class IonSelectSmallerArrowDirective {

    private observer: MutationObserver;
    private isIE11: boolean = !((window as any).ActiveXObject) && "ActiveXObject" in window;

    public constructor(private el: ElementRef) {

        const node: any = this.el.nativeElement;

        this.observer = new MutationObserver((mutations: Array<MutationRecord>): void => {
            this.removeArrow();
        });

        this.observer.observe(node, {
            childList: true,
        });
    }

    private removeArrow(): void {

        if (!this.isIE11) {
            if (this.el.nativeElement.shadowRoot.querySelector('.select-icon') === null) {
                return;
            }
            this.el.nativeElement.shadowRoot.querySelector('.select-icon')
                .setAttribute(
                    'style',
                    'width: 17px !important;height: 17px !important;',
                );
            this.el.nativeElement.shadowRoot.querySelector('.select-icon .select-icon-inner')
                .setAttribute(
                    'style',
                    'border-top: 4px solid; border-right: 4px solid transparent; border-left: 4px solid transparent;',
                );
        } else {
            if (this.el.nativeElement.querySelector('.select-icon') === null) {
                return;
            }
            this.el.nativeElement.querySelector('.select-icon').
                setAttribute(
                    'style',
                    'width: 17px !important;height: 17px !important;',
                );
            this.el.nativeElement.querySelector('.select-icon .select-icon-inner')
                .setAttribute(
                    'style',
                    'border-top: 4px solid; border-right: 4px solid transparent; border-left: 4px solid transparent;',
                );
        }
        this.observer.disconnect();
    }
}
