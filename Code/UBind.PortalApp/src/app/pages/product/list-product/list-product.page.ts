import { Component, OnInit, ViewChild } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ProductApiService } from '@app/services/api/product-api.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { ProductViewModel } from '@app/viewmodels/product.viewmodel';
import { ProductResourceModel } from '@app/resource-models/product.resource-model';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { Permission } from '@app/helpers/permissions.helper';
import { TenantService } from '@app/services/tenant.service';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { AuthenticationService } from '@app/services/authentication.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { AdditionalPropertyDefinitionService } from '@app/services/additional-property-definition.service';
import { EntityType } from '@app/models/entity-type.enum';
import { PermissionService } from '@app/services/permission.service';
import { ProductService } from '../../../services/product.service';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list product page component class.
 * TODO: Write a better class header: displaying of product in the list.
 */
@Component({
    selector: 'app-list-product',
    templateUrl: './list-product.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListProductPage implements OnInit {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<ProductViewModel, ProductResourceModel>;

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
    public viewModelConstructor: typeof ProductViewModel = ProductViewModel;
    protected tenantId: string;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        protected tenantService: TenantService,
        public productApiService: ProductApiService,
        public layoutManager: LayoutManagerService,
        public appConfigService: AppConfigService,
        private authService: AuthenticationService,
        private sharedAlertService: SharedAlertService,
        private additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        private permissionService: PermissionService,
        private productService: ProductService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
        });
    }

    public ngOnInit(): void {
        this.initialiseAdditionalActionButtons();
    }

    public async getSelectedId(): Promise<string> {
        const productAlias: string = this.routeHelper.getParam('productAlias');
        if (productAlias) {
            return await this.productService.getProductIdFromAlias(this.tenantId, productAlias);
        }
        return null;
    }

    public createNewProduct(): void {
        this.additionalPropertyDefinitionService.verifyIfUserIsAllowedToProceed(
            this.tenantId,
            this.authService.userType,
            AdditionalPropertyDefinitionContextType.Tenant,
            EntityType.Product,
            this.tenantId,
            "",
            false,
            this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues),
            () => {
                this.navProxy.navigate(['product', 'create']);
            },
            () => {
                this.sharedAlertService.showWithOk(
                    'You cannot create a new product',
                    'Because products in this context have at least one required additional property,'
                    + ' and you do not have permission to edit additional properties, you are unable '
                    + 'to create new products. For assistance, please speak to your administrator.',
                );
            },
        );
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
