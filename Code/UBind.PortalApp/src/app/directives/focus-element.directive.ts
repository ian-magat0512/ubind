import { Directive, ElementRef, OnInit } from '@angular/core';

/**
 * Forces an input element to take the input focus the moment it is rendered
 */
@Directive({
    selector: '[FocusOnShow]',
})
export class FocusOnShowDirective implements OnInit {

    public constructor(private el: ElementRef) {
        if (!el.nativeElement['focus']) {
            throw new Error('Element does not accept focus.');
        }
    }

    public ngOnInit(): void {
        const input: HTMLInputElement = this.el.nativeElement as HTMLInputElement;
        input.focus();
        input.select();
    }
}
