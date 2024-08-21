import { Component, ChangeDetectorRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { AlertInput } from '@ionic/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { LoadDataService } from '@app/services/load-data.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { Permission } from '@app/helpers/permissions.helper';
import { QuoteViewModel } from '@app/viewmodels/quote.viewmodel';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { EventService } from '@app/services/event.service';
import { Subject } from 'rxjs';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { QuoteService } from '@app/services/quote.service';
import { QuoteFilterComponent } from '@app/components/filter/quote-filter.component';
import { QueryRequestHelper } from '@app/helpers/query-request.helper';
import { FilterSelection } from '@app/viewmodels/filter-selection.viewmodel';
import { MapHelper } from '@app/helpers/map.helper';
import { QuoteStateChangedModel } from '@app/models/quote-state-changed.model';
import { QuoteTypeFilter } from '@app/models/quote-type-filter.enum';
import { QuoteStatus } from '@app/models/quote-status-filter';
import { NavigationExtras } from '@angular/router';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { AppConfigService } from '@app/services/app-config.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { takeUntil, filter } from 'rxjs/operators';
import { AppConfig } from '@app/models/app-config';
import { QuoteStepChangedModel } from '@app/models/quote-step-changed.model';
import { ProductBaseEntityListPage } from '@app/pages/product/product-base-entity-list/product-base-entity-list.page';
import { MatDialog } from '@angular/material/dialog';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list quote page component class.
 * Displaying the list of quotes by segments.
 */
@Component({
    selector: 'app-list-quote-page',
    templateUrl: './list-quote.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListQuotePage extends ProductBaseEntityListPage implements OnInit, OnDestroy {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<QuoteViewModel, QuoteResourceModel>;
    public tenantAlias: string;
    public title: string = 'Quotes';
    public anyProductHasNewBusinessFeature: boolean;
    public hasManageQuotes: boolean;
    public isCreateNewQuoteEnabled: boolean;
    public permission: typeof Permission = Permission;
    protected inputsList: Array<AlertInput> = [];
    public segments: Array<string> = [
        QuoteStatus.Incomplete,
        QuoteStatus.Review,
        QuoteStatus.Endorsement,
        QuoteStatus.Approved,
        QuoteStatus.Declined,
        QuoteStatus.Complete,
    ];
    public filterStatuses: Array<string> = [
        QuoteStatus.Incomplete,
        QuoteStatus.Review,
        QuoteStatus.Endorsement,
        QuoteStatus.Approved,
        QuoteStatus.Declined,
        QuoteStatus.Complete,
    ];

    public defaultSegment: string = 'Incomplete';
    public quoteTypes: Array<string> = [
        QuoteTypeFilter.NewBusiness,
        QuoteTypeFilter.Adjustment,
        QuoteTypeFilter.Renewal,
        QuoteTypeFilter.Cancellation];
    public sortOptions: SortOption = {
        sortBy: [
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterBy.CreatedDate,
            SortAndFilterBy.ExpiryDate,
            SortAndFilterBy.CustomerName,
        ],
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending,
        ],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.ExpiryDate]);
    public viewModelConstructor: typeof QuoteViewModel = QuoteViewModel;
    public filterComponentConstructor: typeof QuoteFilterComponent = QuoteFilterComponent;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public quoteApiService: QuoteApiService,
        private quoteService: QuoteService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        protected eventService: EventService,
        protected userPath: UserTypePathHelper,
        protected productApiService: ProductApiService,
        private appConfigService: AppConfigService,
        public dialog: MatDialog,
    ) {
        super(productApiService, eventService);
        this.appConfigService.appConfigSubject.pipe(takeUntil(this.destroyed)).subscribe((appConfig: AppConfig) => {
            this.tenantAlias = appConfig.portal.tenantAlias;
            this.tenantId = appConfig.portal.tenantId;
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.loadProducts();
        this.handleQuoteStateChangedEvents();
        this.handleQuoteStepChangedEvents();
        this.initialiseAdditionalActionButtons();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    protected getQuoteTypeParameters(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        const quoteTypesFilterChips: Array<FilterSelection> = this.listComponent.filterSelections.filter(
            (filterSelection: FilterSelection) => filterSelection.propertyName == 'quoteTypes',
        );

        if (quoteTypesFilterChips.length > 0) {
            quoteTypesFilterChips.forEach((filterSelection: FilterSelection) => {
                MapHelper.add(params, 'quoteTypes', filterSelection.value);
            });
        }
        return params;
    }

    protected handleProductsWasLoaded(): void {
        this.handleProductFilterUpdateEvent(this.listComponent.filterSelections);
        this.includeExpirySegment();
    }

    public getSelectedId(): string {
        return this.routeHelper.getParam('quoteId');
    }

    private includeExpirySegment(): void {
        // retrieve products of the tenant to retrieve quote expiry settings.
        let hasExpiry: boolean = this.products.findIndex((p: ProductResourceModel) =>
            p.quoteExpirySettings.enabled) > -1;
        if (hasExpiry && !this.segments.includes(QuoteStatus.Expired)) {
            this.segments.push(QuoteStatus.Expired);
        }

        if (hasExpiry && !this.filterStatuses.includes(QuoteStatus.Expired)) {
            this.filterStatuses.push(QuoteStatus.Expired);
        }
    }

    private handleQuoteStateChangedEvents(): void {
        this.eventService.quoteStateChangedSubject$
            .pipe(takeUntil(this.destroyed))
            .subscribe(
                (data: QuoteStateChangedModel) => {
                    this.quoteApiService.getById(data.quoteId)
                        .pipe(takeUntil(this.destroyed))
                        .subscribe((quote: QuoteResourceModel) => {
                            if ((data.previousQuoteState === 'nascent' && data.newQuoteState !== 'nascent') ||
                                (data.previousQuoteState === 'incomplete' && data.newQuoteState === 'incomplete')) {
                                this.listComponent.onItemCreated(quote);
                            } else {
                                this.listComponent.onItemUpdated(quote);
                            }
                        });
                },
            );
    }

    private handleQuoteStepChangedEvents(): void {
        this.eventService.quoteStepChangedSubject$
            .pipe(
                filter((data: QuoteStepChangedModel) => data.quoteId != null),
                takeUntil(this.destroyed))
            .subscribe(
                (data: QuoteStepChangedModel) => {
                    if (data.quoteId) {
                        this.quoteApiService.getById(data.quoteId).subscribe((quote: QuoteResourceModel) => {
                            this.listComponent.onItemUpdated(quote);
                        });
                    }
                });
    }

    public async createNewQuote(): Promise<void> {
        this.quoteService.createQuoteBySelectingProduct();
    }

    public async getDefaultHttpParamsCallback(): Promise<Map<string, string | Array<string>>> {
        return this.getQuoteTypeParameters();
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        const sortBy: any = params.get(SortFilterHelper.sortBy);
        if (sortBy) {
            params = SortFilterHelper.setQuoteSortAndFilterByParam(params, sortBy, SortFilterHelper.sortBy);
        }

        const dateFilteringPropertyName: any = params.get(SortFilterHelper.dateFilteringPropertyName);
        if (dateFilteringPropertyName) {
            params = SortFilterHelper.setQuoteSortAndFilterByParam(
                params,
                dateFilteringPropertyName,
                SortFilterHelper.dateFilteringPropertyName,
            );
        }
    }

    public async getUserFilterChips(): Promise<void> {
        let dateData: Array<any> = QueryRequestHelper.constructDateFilters(this.listComponent.filterSelections);
        let quoteId: string = this.getSelectedId();

        let navigationExtras: NavigationExtras = {
            state: {
                filterTitle: 'Filter & Sort ' + this.listComponent.entityTypeName + 's',
                statusTitle: 'Status',
                entityTypeName: this.listComponent.entityTypeName,
                isProductLoading: this.isProductLoading,
                productList: QueryRequestHelper.constructProductFilters(
                    this.filterProducts,
                    this.listComponent.filterSelections,
                ),
                quoteTypesList: QueryRequestHelper.constructStringFilters(
                    this.quoteTypes,
                    this.listComponent.filterSelections,
                ),
                statusList: QueryRequestHelper.constructStringFilters(
                    this.filterStatuses,
                    this.listComponent.filterSelections,
                ),
                filterByDates: this.filterByDates,
                dateIsBefore: dateData['before'],
                dateIsAfter: dateData['after'],
                selectedId: quoteId,
                testData: QueryRequestHelper.getTestDataFilter(this.listComponent.filterSelections),
                sortOptions: this.sortOptions,
                selectedSortOption: QueryRequestHelper.constructSortFilters(this.listComponent.filterSelections),
                selectedDateFilteringPropertyName: QueryRequestHelper.constructFilterByDate(
                    this.listComponent.filterSelections,
                ),
            },
        };
        this.navProxy.navigateForward(
            [
                this.listComponent.entityTypeName.toLowerCase(),
                'filter',
            ],
            true,
            navigationExtras,
        );
    }

    private initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        additionalActionButtonList.push(ActionButton.createActionButton(
            "Create",
            "plus",
            IconLibrary.AngularMaterial,
            true,
            "Create Quote",
            true,
            (): Promise<void> => {
                return this.createNewQuote();
            },
        ));
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
