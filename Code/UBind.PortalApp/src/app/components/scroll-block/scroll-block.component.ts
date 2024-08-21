import { Component, Input } from '@angular/core';
import { loaderAnimation } from '@assets/animations';

/**
 * Export scroll block component class
 * This is the component for the scroll.
 */
@Component({
    selector: 'app-scroll-block',
    templateUrl: './scroll-block.component.html',
    animations: [loaderAnimation],
    styleUrls: ['./scroll-block.component.scss'],
})
export class ScrollBlockComponent {

    @Input() public isLoadMoreDataEnabled: boolean;
    @Input() public infiniteScrollIsLoading: boolean;
    public ionicPlatformStyleMode: string;

    public constructor() {
        this.findIonicPlatformStyleMode();
    }

    private findIonicPlatformStyleMode(): void {
        let htmlElement: HTMLElement = document.getElementsByTagName('html').item(0);
        this.ionicPlatformStyleMode = htmlElement.getAttribute('mode');
    }
}
