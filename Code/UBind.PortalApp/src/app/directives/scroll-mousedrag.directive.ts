import { Directive, HostListener, ElementRef } from '@angular/core';

/**
 * Export scroll mouse drag directives class
 * This class host the event listener for mouse movement.
 */
@Directive({
    selector: '[scrollMousedrag]',
})
export class ScrollMouseDragDirective {

    public isDown: boolean = false;
    public startX: number | undefined;
    public scrollLeft: number | undefined;
    public nativeElement: any;

    public constructor(
        public elementRef: ElementRef,
    ) {
        this.nativeElement = elementRef.nativeElement;
    }

    @HostListener('touchstart', ['$event'])
    @HostListener('mousedown', ['$event'])
    public onMouseDown(event: any): void {
        const mouseX: number = event.pageX || event.touches[0].pageX;
        this.isDown = true;
        this.startX = mouseX - this.nativeElement.offsetLeft;
        this.scrollLeft = this.nativeElement.scrollLeft;
    }

    @HostListener('touchend', ['$event'])
    @HostListener('mouseleave', ['$event'])
    public onMouseLeave(event: any): void {

        this.isDown = false;
    }

    @HostListener('mouseup', ['$event'])
    public onMouseUp(event: any): void {

        this.isDown = false;
    }

    @HostListener('touchmove', ['$event'])
    @HostListener('mousemove', ['$event'])
    public onMouseMove(event: any): void {

        if (!this.isDown) {
            return;
        }

        // For IE
        event.returnValue = false;

        // For Chrome and Firefox
        if (event.preventDefault) {
            event.preventDefault();
        }

        const pageX: number = event.pageX || event.touches[0].pageX;
        const offsetX: number = pageX - this.nativeElement.offsetLeft;
        const walk: number = (offsetX - this.startX) * 3; // Scroll-fast
        this.nativeElement.scrollLeft = this.scrollLeft - walk;
    }
}
