import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from "@angular/core";
import { IonicHelper } from "@app/helpers/ionic.helper";
import { ActionButton } from "@app/models/action-button";
import { IconLibrary } from "@app/models/icon-library.enum";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { Subject } from "rxjs";
import { takeUntil } from "rxjs/operators";

/**
 * This is to construct the list of action and more buttons for entity detail page.
 * Which means this is a component that render the action button on entity detail pages and the setting of each buttons.
 */
@Component({
    selector: 'app-entity-action-button-list',
    templateUrl: './entity-action-button-list.component.html',
    styleUrls: [
        './entity-action-button-list.component.scss',
    ],
})
export class EntityActionButtonListComponent implements OnInit, OnDestroy, OnChanges {

    @Input() public actionButtonList: Array<ActionButton> = [];
    @Input() public getMoreButtonCallback: (event: any) => void;
    @Input() public canShowMore: boolean = false;
    @Input() public flipMoreIcon: boolean = false;
    @Input() public isForMasterView: boolean = false;
    @Input() public overrideTooltipPosition: string = "";

    public minPaneWidth: number = 400;
    public paneWidth: number = 0;
    public prevPaneWidth: number = 0;
    public actionButtonsCount: number = 0;
    public actionButtonCollapsedWidth: number = 40;
    public actionButtonExpandedWidth: number = 120;
    public actionButtonIndex: number = 0;
    public canShowMoreButton: boolean;
    public tooltipPosition: string;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public setAriaLabel: any = IonicHelper.setAriaLabel;
    private destroyed: Subject<void>;

    public constructor(
        public layoutManager: LayoutManagerService,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.tooltipPosition = this.overrideTooltipPosition ? this.overrideTooltipPosition : 'above';
        this.initializeSettings();

        this.layoutManager.splitPaneEnabledSubject.pipe(
            takeUntil(this.destroyed),
        ).subscribe((splitActive: boolean) => {
            setTimeout(() => {
                this.initializeSettings();
            }, 0);
        });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public ngOnChanges(simpleChanges: SimpleChanges): void {
        this.setPaneWidth();
        if ((Object.prototype.hasOwnProperty.call(simpleChanges, 'actionButtonList')
            && !simpleChanges.actionButtonList.firstChange)
            || this.prevPaneWidth !== this.paneWidth
        ) {
            // this will re compute the settings of each action button.
            this.initializeSettings();
            this.prevPaneWidth = this.paneWidth;
        }
    }

    public showMore(event: any): void {
        if (this.getMoreButtonCallback) {
            return this.getMoreButtonCallback(event);
        }
    }

    public onClick($event: Event, index: number): void {
        this.actionButtonList[index].onClick($event);
    }

    private initializeSettings(): void {
        if (this.actionButtonList) {
            this.actionButtonsCount = this.actionButtonList.length;
            this.setActionButtonSetting();
        }

        this.setMoreButtonSetting();
    }

    private setMoreButtonSetting(): void {
        this.setPaneWidth();
        let currentPaneWidth: number = (this.minPaneWidth
            + (this.actionButtonsCount
                * this.actionButtonCollapsedWidth));
        this.canShowMoreButton = this.paneWidth >= currentPaneWidth;
    }

    private setActionButtonSetting(): void {
        this.setPaneWidth();
        if (!this.actionButtonList) {
            return;
        }

        for (let index: number = 0; index < this.actionButtonList.length; index++) {
            let actionBtn: number = (this.minPaneWidth
                + (this.actionButtonsCount
                    * this.actionButtonCollapsedWidth)
                + (this.actionButtonExpandedWidth
                    - this.actionButtonCollapsedWidth)
                * (index));

            let showActionLabel: boolean = this.paneWidth >= actionBtn;
            this.actionButtonList[index].HasActionLabel = showActionLabel;
        }
    }

    private setPaneWidth(): void {
        this.minPaneWidth = this.isForMasterView ? 350 : 400;
        this.paneWidth = this.isForMasterView
            ? !this.layoutManager.splitPaneVisible
                ? this.layoutManager.getContentWidth()
                : this.layoutManager.getMasterViewComponentWidth()
            : !this.layoutManager.splitPaneVisible
                ? this.layoutManager.getContentWidth()
                : this.layoutManager.getContentWidth()
                - this.layoutManager.getMasterViewComponentWidth();
    }
}
