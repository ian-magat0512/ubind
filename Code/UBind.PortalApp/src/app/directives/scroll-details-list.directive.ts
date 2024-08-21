import { Directive, HostListener, ElementRef, AfterViewChecked } from '@angular/core';
import { LayoutManagerService } from '@app/services/layout-manager.service';

/**
 * Export scroll details directives class
 * Scroll details to display the dynamic scroll bar.
 */
@Directive({
    selector: '[appScrollDetails]',
})
export class ScrollDetailsDirective implements AfterViewChecked {
    public constructor(
        public elementRef: ElementRef,
        public layoutManager: LayoutManagerService,
    ) {

    }
    public ngAfterViewChecked(): void {
        this.onResize(null);
    }

    @HostListener('window:resize', ['$event'])
    public onResize(event: any): void {
        let el: any = this.elementRef.nativeElement;
        // Initial style attributes
        el.style.position = "absolute";
        el.style.width = "100%";
        el.style.overflowY = "auto";

        // Dynamic style attributes
        let masterContainer: HTMLElement = document.getElementById("masterViewContainer");
        let masterDisplay: string = masterContainer.style["display"];
        let isEnvironmentButton: any = document.getElementsByClassName("environment-control");
        let isDetailInMasterContainer: boolean = masterContainer.innerHTML.indexOf("app-entity-details-list") > -1;
        let appDetailsListEditForm: Element = document.getElementsByTagName('app-detail-list-item-edit-form')[0];
        let isDetailListItemEditForm: boolean =
            appDetailsListEditForm ?
                !(appDetailsListEditForm.parentElement.classList.length > 2) :
                false;
        let isMasterDisplayInitial: boolean = masterDisplay == "initial";
        let detailPaddingBottomHeight: string = isEnvironmentButton.length > 0 ? "50px" : "0px";
        el.style.height = isDetailListItemEditForm
            ? isMasterDisplayInitial ? "calc(100vh - 112px)" : "calc(100vh - 50px)"
            : isMasterDisplayInitial ? "calc(100vh - 112px)" : "100%";

        if (isDetailInMasterContainer) {
            el.style.paddingBottom = "50px";
            el.style.height = masterContainer.contains(el)
                ? "calc(100vh - 50px)"
                : this.layoutManager.splitPaneVisible
                    ? "calc(100vh - 112px)"
                    : "calc(100vh - 50px)";
        } else {
            el.style.paddingBottom = detailPaddingBottomHeight;
        }

        if (this.isScrollbar || !isMasterDisplayInitial) {
            el.style.bottom = isMasterDisplayInitial ? "" : "0px";
        }
        el.style.top = isMasterDisplayInitial ? "" : "0px";
    }
    private isScrollbar(): boolean {
        let el: any = this.elementRef.nativeElement;
        return el.nativeElement.scrollHeight > el.clientHeight;
    }
}
