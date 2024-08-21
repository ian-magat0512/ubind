import { Injectable, NgZone } from '@angular/core';
import { ConfigService } from './config.service';
import { MessageService } from './message.service';
import * as _ from 'lodash-es';
import { debounceTime, filter, take } from 'rxjs/operators';
import { EventService } from './event.service';
import { SidebarOffsetService } from './sidebar-offset.service';
import { CssIdentifierPipe } from '@app/pipes/css-identifier.pipe';
import { BrowserDetectionService } from "./browser-detection.service";
import { Inject } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { ScrollToElementInfo } from '@app/models/scroll-to-element-info';

/**
 * Possible argments to a scroll request
 */
export interface ScrollArguments {
    scrollMargin?: number;
    behaviour?: ScrollBehavior;
    shake?: boolean;
    notifyWhenScrollingFinished?: boolean;
}

/**
 * One of the things this class does is load from the config (settings from the workbook)
 * Which say what distance from the top of the page the sidebar should be offset by.
 * This allows the sidebar to sit below things like a permanent header overlay on the website
 * it happens to be injected into.
 * 
 * This is not an ideal solution but more of a workaround and last resort, because really, 
 * we shouldn't be solving the problem like this. A uBind form should work well no matter
 * where it's injected.
 */
@Injectable()
export class WindowScrollService {

    protected isScrolling: boolean = false;
    protected duration: number;
    protected startTime: number;
    protected startPosition: number;
    protected endPosition: number;
    protected easing: string;
    protected shake: boolean;
    protected elementRef: any;
    protected parentFramePosition: number;

    protected shakeClassName: string = 'shake';
    protected shakeDuration: number = 2000; // ms

    protected defaultScrollArgs: ScrollArguments = {
        scrollMargin: 30, // px
        behaviour: 'smooth',
        shake: false,
        notifyWhenScrollingFinished: false,
    };
    protected scrollDurationMultiplier: number = 1;

    protected scrollOffsetTop: any = {};

    public constructor(
        protected configService: ConfigService,
        protected messageService: MessageService,
        private ngZone: NgZone,
        private eventService: EventService,
        private sidebarOffsetService: SidebarOffsetService,
        private cssIdentifierPipe: CssIdentifierPipe,
        private browserDetectionService: BrowserDetectionService,
        @Inject(DOCUMENT) private document: Document,
    ) {
        configService.configurationReadySubject.pipe(filter((ready: boolean) => ready))
            .subscribe(() => {
                this.scrollDurationMultiplier = this.configService.theme
                    ? this.configService.theme.scrollDurationMultiplier || 1
                    : 1;
            });
        this.scrollFocusedElementIntoViewWhenFormElementHidden();
    }

    public scrollElementIntoView(elementRef: HTMLElement, args?: ScrollArguments): void {
        if (!this.isScrolling) {
            this.ngZone.runOutsideAngular(() => {
                args = _.merge(_.clone(this.defaultScrollArgs), args);
                if (args.shake) {
                    args.notifyWhenScrollingFinished = true;
                }

                // can't scroll to a hidden element
                while (elementRef.tagName != 'BODY' && elementRef.offsetParent == null) {
                    elementRef = elementRef.parentElement;
                }

                let elementPosition: number = this.getElementPosition(elementRef);
                if (isNaN(elementPosition)) return;
                let elementHeight: number = elementRef.clientHeight;
                let visibleContentStartPixels: number = this.sidebarOffsetService.getOffsetToVisibleContent();
                let payload: ScrollToElementInfo = {
                    elementPositionPixels: elementPosition,
                    elementHeightPixels: elementHeight,
                    visibleContentStartPixels: visibleContentStartPixels,
                    behaviour: args.behaviour,
                    scrollMarginPixels: args.scrollMargin,
                    startTimeMillis: Date.now(),
                    notifyWhenScrollingFinished: args.notifyWhenScrollingFinished,
                };
                this.messageService.sendMessage('scrollToElement', payload);
                this.shake = args.shake;
                if (this.shake) {
                    this.eventService.scrollingFinished$
                        .pipe(
                            take(1),
                        )
                        .subscribe((finished: boolean) => {
                            this.shakeElement(elementRef);
                            this.focusElement(elementRef);
                        });
                }
            });
        }
    }

    private focusElement(elementRef: HTMLElement): void {
        let element: HTMLElement = elementRef.querySelector(
            '.tabbable, input:not([type="hidden"], [type="radio"]), select, button, textarea');
        if (element) {
            element.focus();
        }
    }

    private shakeElement(elementRef: any): void {
        elementRef.className = elementRef.className + ' ' + this.shakeClassName;
        setTimeout(
            () => {
                this.unshakeElement(elementRef);

            },
            this.shakeDuration);
    }

    private unshakeElement(elementRef: any): void {
        if (elementRef.className.substring(elementRef.className.length - (this.shakeClassName.length + 1),
            elementRef.className.length) == (' ' + this.shakeClassName)) {
            elementRef.className = elementRef.className.substring(0,
                elementRef.className.length - (this.shakeClassName.length + 1));

        }
    }

    public getElementPosition(elementRef: HTMLElement): number {
        let yPos: number = 0;
        while (elementRef) {
            if (elementRef.tagName == 'BODY') {
                yPos += (elementRef.offsetTop + elementRef.clientTop);
            } else {
                yPos += (elementRef.offsetTop - elementRef.scrollTop + elementRef.clientTop);
            }
            elementRef = <HTMLElement>elementRef.offsetParent;
        }
        return yPos;
    }

    public getInvalidNativeElementByFieldPath(fieldPath: string): any {
        let elements: any = document.getElementsByClassName('field-anchor');
        for (let i in elements) {
            let el: Element = elements[i];
            if (el.getAttribute) {
                let elementId: string = el.getAttribute('id');
                const htmlId: string = this.cssIdentifierPipe.transform('anchor-' + fieldPath);
                if (elementId && elementId.indexOf(htmlId) == 0) {
                    if (el.getElementsByClassName('ng-invalid').length > 0) {
                        let labelWrapperSubElements: any = el.getElementsByTagName('label-wrapper');
                        if (labelWrapperSubElements.length) {
                            return labelWrapperSubElements[0];
                        } else {
                            return el;
                        }
                    }
                }
            }
        }
        return null;
    }

    private scrollFocusedElementIntoViewWhenFormElementHidden(): void {
        this.eventService.formElementHiddenChangeSubject.pipe(
            debounceTime(100),
        ).subscribe(() => {
            if (this.document.activeElement.tagName != 'BODY') {
                this.scrollElementIntoView(<HTMLElement>(this.document.activeElement));
            }
        });
    }
}
