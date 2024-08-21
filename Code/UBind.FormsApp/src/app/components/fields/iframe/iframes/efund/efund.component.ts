import { Component, OnDestroy, OnInit } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { ApplicationService } from '@app/services/application.service';
import { BrowserDetectionService } from '@app/services/browser-detection.service';
import { ConfigService } from '@app/services/config.service';
import { FormService } from '@app/services/form.service';
import { MessageService } from '@app/services/message.service';
import { GenericIframeComponent } from '../../generic-iframe.component';
import { Iframeable } from '../../iframeable';

/**
 * Export Efund component class.
 * TODO: Write a better class header: efund payment function.
 */
@Component({
    selector: 'app-principal-finance',
    templateUrl: '../../generic-iframe.component.html',
})
export class EfundComponent extends GenericIframeComponent implements Iframeable, OnDestroy, OnInit {

    public isLoading: boolean = true;
    public errorMessage: string;

    public constructor(
        protected sanitizer: DomSanitizer,
        protected messageService: MessageService,
        public applicationService: ApplicationService,
        protected configService: ConfigService,
        protected formService: FormService,
        protected browserDetector: BrowserDetectionService,
    ) {
        super(sanitizer, messageService, applicationService, configService, formService);
    }

    public ngOnInit(): void {
        this.iFrameResizerOptions.scrolling = true;
        super.ngOnInit();
    }

    public ngOnDestroy(): void {
        super.ngOnDestroy();
    }

    protected initiateIframeResizer(): void {
        if (window['iFrameResize'] == null || document.getElementById(this.iframeId) == null) {
            setTimeout(
                () => {
                    this.initiateIframeResizer();
                },
                this.checkReadyIntervalMs);
        } else {
            this.iframes = window['iFrameResize'](this.iFrameResizerOptions, '#' + this.iframeId);
        }
        if (this.iframes && this.iframes[0]) {
            this.iframes[0].style.height = '1500px';
        }

        // We're adding a delay here because arteva needs login so we don't want the login screen flashing.
        setTimeout(() => this.isLoading = false, 5000);
    }
}
