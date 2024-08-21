import { AfterViewInit, Component, EventEmitter, Output } from '@angular/core';

/**
 * This component is responsible for rendering a loader animation, specifically
 * one which is an Animated PNG (APNG). The reason we want to use an animated PNG
 * rather than an SVG loader with css transitions, is that when the PC is under
 * heavy load, the SVG loader can pause, or the transitions get out of sync,
 * causing the revealed portion of the circle to be so small or in rare cases,
 * hidden altogether. The APNG is rendered inside a different thread of the computer
 * so it has been found to be less likely to be interrupted or paused during these times.
 * Under heavy load it can still be impacted, however the impact is generally less
 * severe than other technologies for animated loaders.
 * 
 * This component is typically used to display a loader during DOM updates, which
 * are very CPU intensive and tend to lock up the browser.
 */
@Component({
    selector: 'apng-loader',
    template: `<div class="apng-loader"></div>`,
    styleUrls: ['./apng-loader.component.scss'],
})

export class ApngLoaderComponent implements AfterViewInit {
    @Output() public rendered: EventEmitter<any> = new EventEmitter();

    public ngAfterViewInit(): void {
        this.rendered.emit(null);
    }
}
