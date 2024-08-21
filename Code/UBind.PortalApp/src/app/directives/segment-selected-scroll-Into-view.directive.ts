import { Directive, HostListener, ElementRef, AfterViewInit, Input } from "@angular/core";

/**
 * When a segment is selected, either with the mouse or on first load,
 * it scrolls the segment into view.
 */
@Directive({
    selector: '[segmentSelectedScrollIntoView]',
})
export class SegmentSelectedScrollIntoViewDirective implements AfterViewInit {

    @Input() public segmentSelectedScrollIntoView: string = '';

    public constructor(public element: ElementRef) { }

    public ngAfterViewInit(): void {
        const segment: string = this.segmentSelectedScrollIntoView;
        this.scrollToSelectedSegment(segment);
    }

    @HostListener('ionChange', ['$event'])
    public onSegmentSelected(): void {
        let segment: any = (event.target as any).value;
        this.scrollToSelectedSegment(segment);
    }

    @HostListener('window:resize', ['$event'])
    public onWindowResize(): void {
        const segment: string = this.segmentSelectedScrollIntoView;
        this.scrollToSelectedSegment(segment);
    }

    private scrollToSelectedSegment(segment: string): void {
        const children: HTMLCollection = this.element.nativeElement.children;
        const selectedElement: any = Array.from(children).filter((c: any) => c.value === segment)[0];
        if (selectedElement) {
            setTimeout(() => {
                selectedElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'nearest',
                    inline: 'center',
                });
            }, 300);
        }
    }
}
