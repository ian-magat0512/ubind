import {
    Component, OnInit, Input, OnDestroy, AfterContentChecked,
    ChangeDetectionStrategy, ChangeDetectorRef, SimpleChanges, OnChanges,
} from '@angular/core';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { DetailsListItemCard } from '@app/models/details-list/details-list-item-card';
import { fromEvent, Observable, Subject } from 'rxjs';
import { filter, takeUntil, throttleTime } from 'rxjs/operators';
import { RouteHelper } from '@app/helpers/route.helper';
import { EnumHelper } from '@app/helpers/enum.helper';
import { Permission, PermissionDataModel } from '@app/helpers';
import { PermissionService } from '@app/services/permission.service';
import { DetailsListItemActionIcon } from '@app/models/details-list/details-list-item-action-icon';
import { EventService } from '@app/services/event.service';

/**
 * Represents a card or a section of groups of properties.
 */
interface Card {
    title: string;
    groups: Array<Group>;
    largeBottomMargin: boolean;
    smallBottomMargin: boolean;
    allowedPermissions: Permission | Array<Permission>;
}

/**
 * A group containing properties to render.
 */
interface Group {
    name: string;
    items: Array<DetailsListItem>;
    allowedPermissions: Permission | Array<Permission>;
}

/**
 * Export entity details list component class
 * This class component is the entity details
 * categorizing per group entity.
 */
@Component({
    selector: 'app-entity-details-list',
    templateUrl: './entity-details-list.component.html',
    animations: [contentAnimation],
    styleUrls: ['./entity-details-list.component.scss'],
    styles: [
        scrollbarStyle,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EntityDetailsListComponent implements OnInit, OnDestroy, AfterContentChecked, OnChanges {

    @Input() public detailsListItems: Array<DetailsListItem>;
    @Input() public hasItemLines: boolean;
    @Input() public hasGroupLines: boolean;
    @Input() public isListViewOnly: boolean;
    @Input() public truncateDescription: boolean = false;

    public groupCategories: Array<DetailsListItemCard>;
    public itemGroups: Array<Group> = [];
    public columns: number = 1;
    public columnClass: string;
    public isCardView: boolean = false;
    public containerClass: string;
    private containerWidth: number;
    public cards: Array<Card> = [];
    public resizeObservable$: Observable<Event>;
    private resizeSubject: Subject<Event> = new Subject<Event>();
    private destroyed: Subject<void> = new Subject<void>();
    public detailsListClasses: any = {
        detailsListTwoColumns: 'details-list-650w',
        detailsListThreeColumns: 'details-list-950w',
        detailsListFourColumns: 'details-list-1250w',
    };
    public doesTruncateDescription: boolean = this.truncateDescription;

    public constructor(
        private changeDetectorRef: ChangeDetectorRef,
        private routeHelper: RouteHelper,
        private permissionService: PermissionService,
        private eventService: EventService,
    ) {
        window.onkeyup = (): void => {
            this.onResize();
        };
    }

    public ngOnInit(): void {
        this.initItems();
        this.onResize();
        this.resizeObservable$ = fromEvent(window, 'resize');
        this.resizeObservable$
            .pipe(takeUntil(this.destroyed))
            .subscribe(this.resizeSubject);
        this.handleResizeEvents();

        this.eventService.detailViewDataChangedSubject$
            .pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.changeDetectorRef.markForCheck();
            });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public ngOnChanges(changes: SimpleChanges): void {
        if (changes.detailsListItems) {
            this.initItems();
        }
    }

    public ngAfterContentChecked(): void {
        this.resizeSubject.next(null);
    }

    private handleResizeEvents(): void {
        this.resizeSubject
            .pipe(
                throttleTime(10),
                filter(() => !this.routeHelper.navigationInProgress),
            ).subscribe(() => this.onResize());
    }

    private onResize(): void {
        let previousCardView: boolean = this.isCardView;
        let previousColumns: number = this.columns;
        this.recalculateLayout();
        if (this.isCardView != previousCardView || this.columns != previousColumns) {
            this.changeDetectorRef.markForCheck();
        }
    }

    private recalculateLayout(): void {
        const containerWidth: number = this.getCurrentContainerWidth();
        if (containerWidth !== this.containerWidth) {
            this.containerWidth = containerWidth;
            this.isCardView = containerWidth >= 650;
            if (this.isCardView) {
                if (containerWidth >= 650 && containerWidth < 950 && this.columns !== 2) {
                    this.columns = 2;
                    this.columnClass = this.detailsListClasses.detailsListTwoColumns;
                } else if (containerWidth >= 950 && containerWidth < 1250 && this.columns !== 3) {
                    this.columns = 3;
                    this.columnClass = this.detailsListClasses.detailsListThreeColumns;
                } else if (containerWidth >= 1250 && this.columns !== 4) {
                    this.columns = 4;
                    this.columnClass = this.detailsListClasses.detailsListFourColumns;
                }
            } else {
                this.columns = 1;
                this.columnClass = null;
            }
            this.determineBottomMarginForAllCards();
        }
    }

    private getCurrentContainerWidth(): number {
        const el: HTMLIonRouterOutletElement = document.getElementsByTagName('ion-router-outlet')[0];
        if (el) {
            return el.clientWidth;
        }
    }

    private initItems(): void {
        const _this: this = this;
        if (this.detailsListItems) {
            this.detailsListItems = this.detailsListItems.filter((f: DetailsListItem) => f.DisplayValue);
            this.detailsListItems.forEach((item: DetailsListItem) => {
                const groupIndex: number = _this.itemGroups.findIndex((group: Group) => group.name === item.GroupName);
                if (groupIndex <= -1) {
                    _this.itemGroups.push({
                        name: item.GroupName,
                        items: _this.getItemsByGroup(item.GroupName),
                        allowedPermissions:
                            item.allowedPermissions ? item.allowedPermissions : EnumHelper.toEnumArray(Permission),
                    });
                } else {
                    _this.itemGroups[groupIndex].items = _this.getItemsByGroup(item.GroupName);
                }

                if (item.Card) {
                    const categoryIndex: number = _this.cards.findIndex((category: Card) =>
                        category.title === item.Card.Description);
                    if (categoryIndex <= -1) {
                        _this.cards.push({
                            title: item.Card.Description,
                            groups: _this.getGroupsByCategoryName(item.Card.Name),
                            smallBottomMargin: false,
                            largeBottomMargin: false,
                            allowedPermissions:
                                item.allowedPermissions ? item.allowedPermissions : EnumHelper.toEnumArray(Permission),
                        });
                    } else {
                        _this.cards[categoryIndex].title = item.Card.Description;
                        _this.cards[categoryIndex].groups = _this.getGroupsByCategoryName(
                            item.Card.Name);
                    }
                }
            });
        }

        this.removeActionIfHasNoPermissions();
    }

    private getGroupsByCategoryName(categoryName: string): Array<Group> {
        const groups: Array<Group> = [];
        const _this: this = this;
        const detailsListItems: Array<DetailsListItem> = this.detailsListItems.filter((f: DetailsListItem) =>
            f.Card.Name === categoryName);
        detailsListItems.forEach((item: DetailsListItem) => {
            const i: number = groups.findIndex((g: Group) => g.name === item.GroupName);
            if (i <= -1) {
                groups.push({
                    name: item.GroupName,
                    items: _this.getItemsByGroup(item.GroupName),
                    allowedPermissions:
                        item.allowedPermissions ? item.allowedPermissions : EnumHelper.toEnumArray(Permission),
                });
            }
        });

        return groups;
    }

    private getItemsByGroup(group: string): Array<DetailsListItem> {
        const items: Array<DetailsListItem> = this.detailsListItems.filter(
            (i: DetailsListItem) => i.GroupName === group && i.DisplayValue);
        return items;
    }

    private determineBottomMarginForAllCards(): void {
        for (let i: number = 0; i < this.cards.length; i++) {
            if (i === this.cards.length - 1 && this.columns > 2) {
                this.cards[i].largeBottomMargin = false;
                this.cards[i].smallBottomMargin = false;
            } else if (i === 1 && this.columns === 4) {
                this.cards[i].largeBottomMargin = false;
                this.cards[i].smallBottomMargin = true;
            } else {
                this.cards[i].largeBottomMargin = false;
                this.cards[i].smallBottomMargin = false;
            }
        }
    }

    // checks if the action is clickable if the user has the permission to do so.
    private removeActionIfHasNoPermissions(): void {
        if (this.detailsListItems) {
            this.detailsListItems.forEach((item: DetailsListItem) => {
                if (item.relatedEntityType
                    && (item.ActionIcon
                        || (item.ActionIcons && item.ActionIcons.length > 0))) {
                    let permissionModel: PermissionDataModel = {
                        organisationId: item.relatedEntityOrganisationId,
                        ownerUserId: item.relatedEntityOwnerId,
                        customerId: item.relatedEntityCustomerId,
                    };

                    let hasPermission: boolean =
                        this.permissionService.hasElevatedPermissionsOfTheRelatedEntity(
                            item.relatedEntityType,
                            permissionModel);

                    if (item.ActionIcons
                        && item.ActionIcons.length > 0) {
                        // check permissions of each action icon, remove them if not allowed to access
                        item.ActionIcons = item.ActionIcons.filter((actionIcon: DetailsListItemActionIcon) => {
                            if (actionIcon.mustHavePermissions) {
                                if (!this.permissionService.hasOneOfPermissions(actionIcon.mustHavePermissions)) {
                                    return false;
                                }
                            }
                            if (actionIcon.mustHaveOneOfPermissions) {
                                if (!this.permissionService.hasOneOfPermissions(actionIcon.mustHaveOneOfPermissions)) {
                                    return false;
                                }
                            }
                            if (actionIcon.mustHaveOneOfEachSetOfPermissions) {
                                if (!this.permissionService.hasOneOfEachSetOfPermissions(
                                    actionIcon.mustHaveOneOfEachSetOfPermissions)) {
                                    return false;
                                }
                            }
                            return true;
                        });
                    } else {
                        if (!hasPermission) {
                            item.ActionIcon = null;
                        }
                    }
                }
            });
        }
    }
}
