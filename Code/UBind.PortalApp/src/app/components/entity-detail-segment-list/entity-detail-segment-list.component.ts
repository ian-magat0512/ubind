import {
    Component, OnInit, Input, TemplateRef, ChangeDetectorRef,
    OnDestroy, Output, EventEmitter } from "@angular/core";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { LoadDataService } from "@app/services/load-data.service";
import { RepositoryRegistry } from "@app/repositories/repository-registry";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { contentAnimation } from "@assets/animations";
import { scrollbarStyle } from "@assets/scrollbar";
import { Pager } from "@app/helpers";
import { Observable, SubscriptionLike } from "rxjs";
import { finalize } from "rxjs/operators";

/** 
 * Export Entity detail segment list component class.
 * This class manage the entity component of the detail segmet list.
 */
@Component({
    selector: 'app-entity-detail-segment-list',
    templateUrl: './entity-detail-segment-list.component.html',
    animations: [contentAnimation],
    styleUrls: [
        './entity-detail-segment-list.component.scss',
        '../../../assets/css/scrollbar-list.scss',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class EntityDetailSegmentListComponent implements OnInit, OnDestroy {

    @Input() public entityTypeNamePlural: string;
    @Input() public itemTemplate: TemplateRef<HTMLElement>;
    @Input() public itemSelectedHandler: (item: any) => void;
    @Input() public getSegmentListCallback: (params?: Map<string, string | Array<string>>) => Observable<Array<any>>;
    @Input() public viewModelConstructor: any;
    @Output() public itemsFirstLoadedEvent: EventEmitter<boolean | undefined> = new EventEmitter<boolean | undefined>();

    public headers: Array<string> = [];
    public itemList: Array<any> = [];
    public pager: Pager = new Pager();
    public hasLoaded: boolean = false;
    public isLoadingItems: boolean = false;
    public errorMessage: string = '';
    private infiniteScrollSubscription: SubscriptionLike;
    private subscription: SubscriptionLike;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected navProxy: NavProxyService,
        protected loadDataService: LoadDataService,
        protected repositoryRegistry: RepositoryRegistry,
        public layoutManager: LayoutManagerService,
    ) {
    }

    public ngOnInit(): void {
        this.load();
    }

    public ngOnDestroy(): void {
        this.unsubscribeSubscriptions();
    }

    public reload(): void {
        this.hasLoaded = false;
        this.itemList = [];
        this.headers = [];
        this.pager.resetPage();
        this.load();
    }

    public load(): void {
        this.isLoadingItems = true;
        this.unsubscribeSubscriptions();
        let params: Map<string, string | Array<string>> = this.pager.getPagingQueryParameters();
        try {
            this.subscription = this.getSegmentListCallback(params)
                .pipe(finalize(() => {
                    this.subscription.unsubscribe();
                    this.isLoadingItems = false;
                }))
                .subscribe((data: any) => {
                    if (!this.pager.checkScrollIfEnabled(data?.length)) {
                        this.pager.deactivateInfiniteScroll();
                    }

                    this.populateData(data);
                    this.hasLoaded = true;
                    if (this.itemsFirstLoadedEvent) {
                        this.itemsFirstLoadedEvent.emit(data?.length > 0);
                    }
                });
        } catch (error: any) {
            this.isLoadingItems = false;
            this.pager.deactivateInfiniteScroll();
            this.errorMessage = `There was a problem loading the list of ${this.entityTypeNamePlural.toLowerCase()}`;
            if (this.itemsFirstLoadedEvent) {
                this.itemsFirstLoadedEvent.emit(undefined);
            }
            throw error;
        }
    }

    public addMoreData(event: any): void {
        if (event) {
            event.stopPropagation();
        }

        if (this.infiniteScrollSubscription && !this.infiniteScrollSubscription.closed) {
            return;
        }

        if (this.pager.checkListHeight(event, this.infiniteScrollSubscription) && !this.isLoadingItems) {
            this.pager.nextPage();
            this.loadMoreData();
        }
    }

    public itemSelected(item: any): void {
        if (this.itemSelectedHandler) {
            this.itemSelectedHandler(item);
        }
    }

    private loadMoreData(): void {
        this.isLoadingItems = true;
        let params: Map<string, string | Array<string>> = this.pager.getPagingQueryParameters();
        try {
            this.infiniteScrollSubscription = this.getSegmentListCallback(params)
                .pipe(finalize(() => {
                    this.isLoadingItems = false;
                    this.infiniteScrollSubscription.unsubscribe();
                }))
                .subscribe((data: any) => {
                    if (!this.pager.checkScrollIfEnabled(data?.length)) {
                        this.pager.deactivateInfiniteScroll();
                    }

                    this.populateData(data);
                });
        } catch (error: any) {
            this.pager.deactivateInfiniteScroll();
            this.isLoadingItems = false;
            throw error;
        }
    }

    private populateData(data: Array<any>): void {
        if (data?.length > 0) {
            let newList: Array<any> = [];
            data.forEach((item: any) => {
                newList.push(new this.viewModelConstructor(item));
            });

            if (this.itemList.length > 0) {
                this.itemList = [...this.itemList, ...newList];
            } else {
                this.itemList = newList;
            }

            this.groupByData();
        }
    }

    private groupByData(): void {
        let groupByValue: Array<string> = this.itemList.map((item: any) => item.groupByValue);
        this.headers = groupByValue.length > 0 ? Array.from(new Set(groupByValue)) : [];
    }

    private unsubscribeSubscriptions(): void {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
        if (this.infiniteScrollSubscription) {
            this.infiniteScrollSubscription.unsubscribe();
        }
    }
}
