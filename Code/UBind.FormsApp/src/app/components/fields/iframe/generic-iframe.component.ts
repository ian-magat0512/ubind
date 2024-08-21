
import { takeUntil } from 'rxjs/operators';
import { Component, OnDestroy, Input, OnInit } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { FormService } from '@app/services/form.service';
import { MessageService } from '@app/services/message.service';
import { ConfigService } from '@app/services/config.service';
import { ApplicationService } from '@app/services/application.service';
import { Iframeable } from './iframeable';
import { Subject } from 'rxjs';
import * as _ from 'lodash-es';

/**
 * Export generic Iframe component class.
 * TODO: Write a better class header: generic Iframe functions.
 */
@Component({
    selector: 'generic-iframe',
    templateUrl: './generic-iframe.component.html',
})

export class GenericIframeComponent implements Iframeable, OnInit, OnDestroy {
    @Input() public facade: any;
    @Input() public url: SafeResourceUrl;

    protected destroyed: Subject<void> = new Subject<void>();
    protected checkReadyIntervalMs: number = 100;

    protected iframeId: string;
    protected iframes: any;

    public isCustom: boolean = false;

    public isLoading: boolean = false;
    public errorMessage: string;

    // Iframeresizer
    protected iFrameResizerOptions: any = {
        checkOrigin: false,
        heightCalculationMethod: 'lowestElement',
        log: false, // Enable console logging,
        scrolling: false,
    };

    protected messageSubscription: any;

    public constructor(
        protected sanitizer: DomSanitizer,
        protected messageService: MessageService,
        public applicationService: ApplicationService,
        protected configService: ConfigService,
        protected formService: FormService) {
    }

    public ngOnInit(): void {
        this.iframeId = this.facade.key + 'Iframe';
        this.initiateIframeResizer();
        this.messageSubscription = this.messageService.modelUpdate.pipe(
            takeUntil(this.destroyed))
            .subscribe((data: any) => {
                this.onModelUpdate(data);
            });
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
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
        this.closeIframe();
        if (this.messageSubscription) {
            this.messageSubscription.unsubscribe();
        }
    }

    protected onModelUpdate(data: any): void {
        if (data.fieldKey == this.facade.key) {
            for (let key in data.values) {
                this.updateFormControlFieldValue(key, data.values[key]);
            }
            this.setFacadeValue(data.values);
        }
    }

    protected setFacadeValue(value: any): void {
        if (_.isObject(value)) {
            value = JSON.stringify(value);
        }
        if (value != this.facade.field.formControl.value) {
            this.facade.setValue(value);
            this.facade.onChange();
        }
    }

    protected updateFormControlFieldValue(key: string, value: any): void {
        this.facade.parentQuestionsWidget.setFieldValue(key, value);
    }

    protected closeIframe(): void {
        if (this.iframes) {
            for (const iframe of this.iframes) {
                if (iframe.id == this.iframeId) {
                    iframe.iFrameResizer.close();
                    return;
                }
            }
        }
    }
}
