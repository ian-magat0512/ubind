import { Component } from '@angular/core';
import { loaderAnimation } from '@assets/animations';

/**
 * Export loader component class
 * This component class is to display the loader.
 */
@Component({
    selector: 'loader',
    template: `
  <div class="custom-loader" [ngClass]="ionicPlatformStyleMode" [@loaderAnimation]>
      <svg class="custom-loader-md" width="40" height="40" fill-opacity="0.0">
          <circle cx="20" cy="20" r="15" fill-opacity="0.0"></circle>
      </svg>

      <div class="custom-loader-ios">
            <div  *ngFor="let item of [].constructor(12); let i =index" class="bar{{i+1}}"></div>
      </div>
  </div>`,
    animations: [loaderAnimation],
    styleUrls: ['./loader.component.scss'],
})
export class LoaderComponent {

    public ionicPlatformStyleMode: string;

    public constructor() {
        this.findIonicPlatformStyleMode();
    }

    /**
     * Finds the Ionic "mode" (see https://ionicframework.com/docs/theming/platform-styles)
     * from the top level html tag, so it can be added as a class to the first div. 
     * This is so we can use local scss rather than having have global css just to access 
     * the class of the html tag.
     */
    private findIonicPlatformStyleMode(): void {
        let htmlElement: HTMLElement = document.getElementsByTagName('html').item(0);
        this.ionicPlatformStyleMode = htmlElement.getAttribute('mode');
    }
}
