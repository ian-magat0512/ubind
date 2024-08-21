import { Component, OnInit, ViewChild } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { ProductViewModel } from '@app/viewmodels/product.viewmodel';
import { Permission } from '@app/helpers/permissions.helper';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { finalize } from 'rxjs/operators';
import { TenantService } from '@app/services/tenant.service';
import { ReplaySubject } from 'rxjs';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list tenant product page component class.
 * This class manage displaying the tenant product in the list.
 */
@Component({
    selector: 'app-list-tenant-product',
    templateUrl: './list-tenant-product.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListTenantProductPage implements OnInit {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<ProductViewModel, ProductResourceModel>;

    private tenant: TenantResourceModel;
    private tenantIdSubject: ReplaySubject<string> = new ReplaySubject<string>(1);
    public title: string = 'Products';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = ['Active', 'Disabled'];
    public defaultSegment: string = 'Active';
    public filterStatuses: Array<string> = ['Active', 'Disabled'];
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.ProductName]),
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public viewModelConstructor: any = ProductViewModel;
    protected tenantId: string;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        protected tenantService: TenantService,
        public productApiService: ProductApiService,
        protected tenantApiService: TenantApiService,
        public layoutManager: LayoutManagerService,
        private permissionService: PermissionService,
    ) {
    }

    public ngOnInit(): void {
        const tenantAlias: string = this.routeHelper.getParam('tenantAlias');
        let subscription: any = this.tenantApiService.get(tenantAlias)
            .pipe(finalize(() => subscription.unsubscribe()))
            .subscribe((tenant: TenantResourceModel) => {
                this.tenant = tenant;
                this.title = tenant.name + ' Products';
                this.tenantIdSubject.next(tenant.id);
            });
        this.initialiseAdditionalActionButtons();
    }

    public itemSelected(product: ProductViewModel): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('product');
        pathSegments.push(product.alias);
        this.navProxy.navigateForward(pathSegments);
    }

    public createNewProduct(): void {
        this.navProxy.navigate(['tenant', this.tenant.alias, 'product', 'create']);
    }

    public async getDefaultHttpParams(): Promise<Map<string, string | Array<string>>> {
        return new Promise<any>((resolve: any): any => {
            let subscription: any = this.tenantIdSubject
                .pipe(finalize(() => subscription.unsubscribe()))
                .subscribe((tenantId: string) => {
                    let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
                    params.set('tenant', tenantId);
                    resolve(params);
                });
        });
    }

    public async getSelectedId(): Promise<string> {
        let productAlias: string = this.routeHelper.getParam('productAlias');
        if (productAlias) {
            if (!this.tenantId) {
                let tenantAlias: string = this.routeHelper.getParam('tenantAlias');
                this.tenantId = await this.tenantService.getTenantIdFromAlias(tenantAlias);
            }

            return (await this.productApiService
                .getByAlias(productAlias, this.tenantId).toPromise()).id;
        } else {
            return null;
        }
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        const sortBy: any = params.get(SortFilterHelper.sortBy);
        if (sortBy) {
            params = SortFilterHelper.setSortAndFilterByParam(
                params,
                SortFilterHelper.sortBy,
                sortBy,
                this.getSortAndFilters(),
            );
        }

        const dateFilteringPropertyName: any = params.get(SortFilterHelper.dateFilteringPropertyName);
        if (dateFilteringPropertyName) {
            params = SortFilterHelper.setSortAndFilterByParam(
                params,
                SortFilterHelper.dateFilteringPropertyName,
                dateFilteringPropertyName,
                this.getSortAndFilters(),
            );
        }
    }

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.ProductName,
            SortAndFilterByFieldName.DetailsName,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.DetailsLastModifiedDate,
        );
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }

    private initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasPermission(Permission.ManageProducts)) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Product",
                true,
                (): void => {
                    return this.createNewProduct();
                },
                1,
            ));
        }
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
