import { Component, ChangeDetectorRef, ViewChild, OnInit } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PolicyApiService } from '@app/services/api/policy-api.service';
import { Permission, PolicyHelper, QueryRequestHelper, StringHelper } from '@app/helpers';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { PolicyViewModel } from '@app/viewmodels/policy.viewmodel';
import { PolicyResourceModel } from '@app/resource-models/policy.resource-model';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { CustomerPolicyViewModel } from '@app/viewmodels/customer-policy.viewmodel';
import { MapHelper } from '@app/helpers/map.helper';
import { AuthenticationService } from '@app/services/authentication.service';
import { SortOption } from '@app/components/filter/sort-option';
import { ProductApiService } from '@app/services/api/product-api.service';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';
import { Subject } from 'rxjs';
import { NavigationExtras } from '@angular/router';
import { EventService } from '@app/services/event.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { ProductBaseEntityListPage } from '@app/pages/product/product-base-entity-list/product-base-entity-list.page';
import { CustomerPolicyStatus, PolicyStatus } from '@app/models';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export list customer policy page component class
 * This class manage of displaying the customer policy details in the list.
 */
@Component({
    selector: 'app-list-customer-policy',
    templateUrl: './list-customer-policy.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListCustomerPolicyPage extends ProductBaseEntityListPage implements OnInit {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<CustomerPolicyViewModel, PolicyResourceModel>;

    public title: string = 'My Policies';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = [
        CustomerPolicyStatus.Current,
        CustomerPolicyStatus.Inactive];
    public defaultSegment: string = CustomerPolicyStatus.Current;
    public filterStatuses: Array<string> = [CustomerPolicyStatus.Current, CustomerPolicyStatus.Inactive];
    public sortOptions: SortOption = PolicyHelper.sortOptions;
    public filterByDates: Array<string> = PolicyHelper.filterByDates;
    public viewModelConstructor: typeof PolicyViewModel = PolicyViewModel;
    public itemNamePlural: string = "policies";
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected routeHelper: RouteHelper,
        protected authService: AuthenticationService,
        public navProxy: NavProxyService,
        public policyApiService: PolicyApiService,
        public layoutManager: LayoutManagerService,
        private stringHelper: StringHelper,
        private appConfigService: AppConfigService,
        protected productApiService: ProductApiService,
        public eventService: EventService,
        protected userPath: UserTypePathHelper,
    ) {
        super(productApiService, eventService);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.title = this.authService.isMutualTenant() ? 'My Protections' : this.title;
        this.itemNamePlural = this.authService.isMutualTenant() ? "protections" : this.itemNamePlural;
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
        });
        this.loadProducts();
    }

    public getSelectedId(): string {
        return this.routeHelper.getParam('policyId');
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'current',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )
            && MapHelper.containsEntryWithValue(
                params,
                'status',
                'inactive',
                this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
            )) {
            params.set(
                'status',
                [PolicyStatus.Issued,
                    PolicyStatus.Active,
                    PolicyStatus.Expired,
                    PolicyStatus.Cancelled],
            );
        } else if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'active',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )
            || MapHelper.containsEntryWithValue(
                params,
                'status',
                'current',
                this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
            )) {
            params.set('status', [PolicyStatus.Issued, PolicyStatus.Active]);
        } else if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'inactive',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )
            || MapHelper.containsEntryWithValue(
                params,
                'status',
                'cancelled',
                this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
            )) {
            params.set('status', [PolicyStatus.Expired, PolicyStatus.Cancelled]);
        }

        const sortBy: any = params.get('sortBy');
        if (sortBy) {
            params = PolicyHelper.setSortAndFilterByParam(params, sortBy, 'sortBy');
        }

        const dateFilteringPropertyName: any = params.get('filterByDate');
        if (dateFilteringPropertyName) {
            params = PolicyHelper.setSortAndFilterByParam(params, dateFilteringPropertyName, 'filterByDate');
        }
    }

    public getUserFilterChips(): void {

        let dateData: Array<any> = QueryRequestHelper.constructDateFilters(this.listComponent.filterSelections);
        let policyId: string = this.getSelectedId();

        let navigationExtras: NavigationExtras = {
            state: {
                filterTitle: 'Filter & Sort Policies',
                statusTitle: 'Status',
                entityTypeName: this.listComponent.entityTypeName,
                isProductLoading: this.isProductLoading,
                productList: QueryRequestHelper.constructProductFilters(
                    this.filterProducts,
                    this.listComponent.filterSelections,
                ),
                statusList: QueryRequestHelper.constructStringFilters(
                    this.filterStatuses,
                    this.listComponent.filterSelections,
                ),
                filterByDates: this.filterByDates,
                dateIsBefore: dateData['before'],
                dateIsAfter: dateData['after'],
                selectedId: policyId,
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
                this.userPath.policy,
                'filter',
            ],
            true,
            navigationExtras,
        );

    }

    protected handleProductsWasLoaded(): void {
        this.handleProductFilterUpdateEvent(this.listComponent.filterSelections);
    }
}
