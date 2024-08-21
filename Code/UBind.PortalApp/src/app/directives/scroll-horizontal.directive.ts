import { Directive, HostListener, Output, EventEmitter, ElementRef } from '@angular/core';

/**
 * Export scroll horizontal directives class
 * This class host the event listener for mouse wheel.
 */
@Directive({
    selector: '[scrollHorizontal]',
})
export class ScrollHorizontalDirective {

    @Output() public mouseWheelUp: EventEmitter<any> = new EventEmitter();
    @Output() public mouseWheelDown: EventEmitter<any> = new EventEmitter();

    public constructor(
        public elementRef: ElementRef,
    ) {

    }

    @HostListener('mousewheel', ['$event'])
    public onMouseWheelChrome(event: any): void {
        this.mouseWheelFunc(event);
    }

    @HostListener('DOMMouseScroll', ['$event'])
    public onMouseWheelFirefox(event: any): void {
        this.mouseWheelFunc(event);
    }

    @HostListener('onmousewheel', ['$event'])
    public onMouseWheelIE(event: any): void {
        this.mouseWheelFunc(event);
    }

    public mouseWheelFunc(event: any): void {

        let el: any = this.elementRef.nativeElement;

        if (event.deltaY > 0) {
            el.scrollLeft += 100;
        } else {
            el.scrollLeft -= 100;
        }

        // For IE
        event.returnValue = false;

        // For Chrome and Firefox
        if (event.preventDefault) {
            event.preventDefault();
        }
    }
}
