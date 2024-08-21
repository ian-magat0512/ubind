import { Component, OnInit, Input, HostListener, AfterViewInit } from '@angular/core';
import { Subject } from 'rxjs';
import { throttleTime } from 'rxjs/operators';
import { ViewChild, ElementRef } from '@angular/core';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export entity details list item components class
 * This class is to the entity detailst items that
 * manage the actions.
 */
@Component({
    selector: 'app-entity-details-list-item',
    templateUrl: './entity-details-list-item.component.html',
    styleUrls: ['./entity-details-list-item.component.scss'],
    styles: ['.title {overflow: hidden; text-overflow: ellipsis; ' +
        'display: -webkit-box; -webkit-box-orient: vertical; line-height: 28px;}'],

})
export class EntityDetailsListItemComponent implements OnInit, AfterViewInit {
    @Input() public detailsItem: DetailsListItem;
    @Input() public showIcon: false;
    @Input() public isCardView: false;
    @Input() public isAdjustClass: false;
    @Input() public truncateDescription: false;
    @ViewChild("title") public title: ElementRef;
    public itemWithoutDescription: boolean;
    public itemWithDescription: boolean;
    public labelCssClass: string;
    private resizeSubject: Subject<void> = new Subject<void>();
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor() {
        this.handleResizeEvents();
    }

    public ngOnInit(): void {
        if (!this.showIcon) {
            this.detailsItem.Icon = "";
        }

        if (this.isAdjustClass) {
            this.adjustItemClass();
        }

    }

    public ngAfterViewInit(): void {
        this.setWrapText();
    }

    private setWrapText(): void {
        // the css include the related properties to make the line-clamp work.
        this.title.nativeElement.style.cssText = `
            -webkit-line-clamp: ${this.detailsItem.lineCount > 0 ? this.detailsItem.lineCount : 'unset'};
            display: -webkit-box;
            white-space: normal;
            -webkit-box-orient: vertical;
            overflow: hidden;
            `;
    }

    @HostListener('window:resize', ['$event'])
    public onResize(event: any): void {
        this.resizeSubject.next();
    }

    private handleResizeEvents(): void {
        this.resizeSubject.pipe(throttleTime(10)).subscribe(() => {
            if (this.isAdjustClass) {
                this.adjustItemClass();
            }
        });
    }

    public itemOnClick(): void {
        if (this.detailsItem.HasLink || this.detailsItem.isListViewOnly) {
            this.detailsItem.onClick(event, this.detailsItem);
        }
    }

    public actionIconOnClick($event: Event): void {
        this.detailsItem.ActionIcon.onClick($event);
    }

    public actionIconsOnClick($event: Event, index: number): void {
        this.detailsItem.ActionIcons[index].onClick($event);
    }

    private adjustItemClass(): void {
        if (this.isCardView) {
            this.itemWithoutDescription = this.detailsItem.Icon == 'none' &&
                !this.detailsItem.Description ? true : false;
            this.itemWithDescription = this.detailsItem.Icon == 'none' &&
                this.detailsItem.Description ? true : false;
        } else {
            this.itemWithoutDescription = false;
            this.itemWithDescription = false;
        }
    }
}
