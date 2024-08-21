import { Component } from '@angular/core';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { loaderAnimation } from '../../../assets/animations';

/**
 * Export SVG loader component class.
 * TODO: Write a better class header: svg loader.
 */
@Component({
    selector: 'svg-loader',
    template: `
  <div class="custom-loader" [@loaderAnimation] [class.ios]="isIos">
      <svg class="custom-loader-md" width="40" height="40">
          <circle cx="20" cy="20" r="15"></circle>
      </svg>
      <div class="custom-loader-ios">
          <div class="bar1"></div>
          <div class="bar2"></div>
          <div class="bar3"></div>
          <div class="bar4"></div>
          <div class="bar5"></div>
          <div class="bar6"></div>
          <div class="bar7"></div>
          <div class="bar8"></div>
          <div class="bar9"></div>
          <div class="bar10"></div>
          <div class="bar11"></div>
          <div class="bar12"></div>
      </div>
  </div>`,
    animations: [loaderAnimation],
    styleUrls: ['./svg-loader.component.scss'],
})
export class SvgLoaderComponent {

    public isIos: boolean;

    public constructor(browserDetectionService: BrowserDetectionService) {
        this.isIos = browserDetectionService.isIos;
    }
}
