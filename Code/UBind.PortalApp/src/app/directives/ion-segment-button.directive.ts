import { Directive, ElementRef, AfterViewInit } from '@angular/core';

/**
 *
 */
@Directive({
    selector: '[ionSegmentButtonEvent]',
})
export class IonSegmentButtonDirective implements AfterViewInit {

    public constructor(private elRef: ElementRef) { }

    public ngAfterViewInit(): void {
        this.elRef.nativeElement.addEventListener('keydown', (event: KeyboardEvent) => {
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                this.elRef.nativeElement.click();
            }
        });

        this.elRef.nativeElement.addEventListener('focus', (event: FocusEvent) => {
            event.preventDefault();
            this.elRef.nativeElement.classList.add('segment-button-focused');
            this.elRef.nativeElement.focus();
        });
    }
}
