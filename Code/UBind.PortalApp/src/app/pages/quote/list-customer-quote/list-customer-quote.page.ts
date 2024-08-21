import { Component, ChangeDetectorRef, ViewChild, OnInit, OnDestroy } from '@angular/core';
import { scrollbarStyle } from '@assets/scrollbar';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { QuoteApiService } from '@app/services/api/quote-api.service';
import { BroadcastService } from '@app/services/broadcast.service';
import { LoadDataService } from '@app/services/load-data.service';
import { Permission, StringHelper } from '@app/helpers';
import { contentAnimation } from '@assets/animations';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { QuoteResourceModel } from '@app/resource-models/quote.resource-model';
import { CustomerQuoteViewModel } from '@app/viewmodels/customer-quote.viewmodel';
import { AlertInput } from '@ionic/core';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { QuoteService } from '@app/services/quote.service';
import { Subject } from 'rxjs';
import { EventService } from '@app/services/event.service';
import { QuoteStateChangedModel } from '@app/models/quote-state-changed.model';
import { MapHelper } from '@app/helpers/map.helper';
import { ProductApiService } from '@app/services/api/product-api.service';
import { AppConfigService } from '@app/services/app-config.service';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { PermissionService } from '@app/services/permission.service';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { QueryRequestHelper } from '@app/helpers/query-request.helper';
import { QuoteTypeFilter } from '@app/models/quote-type-filter.enum';
import { NavigationExtras } from '@angular/router';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { takeUntil } from 'rxjs/operators';
import { AppConfig } from '@app/models/app-config';
import { ProductBaseEntityListPage } from '@app/pages/product/product-base-entity-list/product-base-entity-list.page';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { QuoteStatus } from '@app/models/quote-status-filter';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list customer quote page component class.
 * TODO: Write a better class header: displaying of customer quote in the list.
 */
@Component({
    selector: 'app-list-customer-quote',
    templateUrl: './list-customer-quote.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListCustomerQuotePage extends ProductBaseEntityListPage implements OnInit, OnDestroy {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<CustomerQuoteViewModel, QuoteResourceModel>;
    public tenantAlias: string;
    public title: string = 'My Quotes';
    public permission: typeof Permission = Permission;
    public hasAtLeastOneEnabledProduct: boolean;
    protected inputsList: Array<AlertInput> = [];
    public segments: Array<string> = ['Incomplete', 'Referred', 'Complete'];
    public defaultSegment: string = 'Incomplete';
    public filterStatuses: Array<string> = ['Incomplete', 'Referred', 'Complete'];
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([
            SortAndFilterBy.ExpiryDate,
            SortAndFilterBy.CustomerName,
        ]),
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending,
        ],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.ExpiryDate]);
    public quoteTypes: Array<string> = [
        QuoteTypeFilter.NewBusiness,
        QuoteTypeFilter.Adjustment,
        QuoteTypeFilter.Renewal,
        QuoteTypeFilter.Cancellation];
    public viewModelConstructor: typeof CustomerQuoteViewModel = CustomerQuoteViewModel;

    public hasNewBusinessProductFeature: boolean;
    public hasManageQuotesPermission: boolean;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public quoteApiService: QuoteApiService,
        protected quoteService: QuoteService,
        protected broadcastService: BroadcastService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        protected userPath: UserTypePathHelper,
        protected eventService: EventService,
        protected productApiService: ProductApiService,
        private appConfigService: AppConfigService,
        private productFeatureService: ProductFeatureSettingService,
        private permissionService: PermissionService,
        private stringHelper: StringHelper,
    ) {
        super(productApiService, eventService);
        this.appConfigService.appConfigSubject.pipe(takeUntil(this.destroyed)).subscribe((appConfig: AppConfig) => {
            this.tenantAlias = appConfig.portal.tenantAlias;
            this.tenantId = appConfig.portal.tenantId;
        });
        this.hasManageQuotesPermission = this.permissionService.hasPermission(Permission.ManageQuotes);
    }

    public async ngOnInit(): Promise<void> {
        this.destroyed = new Subject<void>();
        this.loadProducts();
        this.handleQuoteStateChangedEvents();
        this.initialiseAdditionalActionButtons();
        this.hasNewBusinessProductFeature = await this.productFeatureService.anyProductHasNewBusinessQuoteFeature();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    protected handleProductsWasLoaded(): void {
        this.handleProductFilterUpdateEvent(this.listComponent.filterSelections);
        this.includeExpirySegment();
    }

    private includeExpirySegment(): void {
        // retrieve products of the tenant to retrieve quote expiry settings.
        let hasExpiry: boolean = this.products.findIndex((p: ProductResourceModel) =>
            p.quoteExpirySettings.enabled) > -1;
        if (hasExpiry) {
            this.segments.push(QuoteStatus.Expired);
            this.filterStatuses.push(QuoteStatus.Expired);
        }
    }

    private handleQuoteStateChangedEvents(): void {
        this.eventService.quoteStateChangedSubject$
            .pipe(takeUntil(this.destroyed))
            .subscribe((data: QuoteStateChangedModel) => {
                this.quoteApiService.getById(data.quoteId)
                    .pipe(takeUntil(this.destroyed))
                    .subscribe((quote: QuoteResourceModel) => {
                        if (data.previousQuoteState == 'nascent' && data.newQuoteState != 'nascent') {
                            this.listComponent.onItemCreated(quote);
                        } else {
                            this.listComponent.onItemUpdated(quote);
                        }
                    });
            });
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        let statusArr: Array<string> = [];
        if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'incomplete',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )) {
            statusArr.push('incomplete');
            statusArr.push('approved');
        }
        if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'complete',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )) {
            statusArr.push('complete');
            statusArr.push('declined');
        }
        if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'referred',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )) {
            statusArr.push('referred');
            statusArr.push('endorsement');
            statusArr.push('review');
        }
        if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'expired',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )) {
            statusArr.push('expired');
        }
        params.set('status', statusArr);

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

    public async createNewQuote(): Promise<void> {
        this.quoteService.createQuoteBySelectingProduct();
    }

    public getSelectedId(): string {
        return this.routeHelper.getParam('quoteId');
    }

    public getUserFilterChips(): void {

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
        this.navProxy.navigateForward(['my-quotes', 'filter'], true, navigationExtras);
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
            1,
        ));
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
