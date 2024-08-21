import { Component, OnInit, ViewChild } from "@angular/core";
import { EntityListComponent } from "@app/components/entity-list/entity-list.component";
import { RouteHelper } from "@app/helpers/route.helper";
import { EntityType } from "@app/models/entity-type.enum";
import { DataTableDefinitionResourceModel } from "@app/resource-models/data-table-definition.resource-model";
import { OrganisationResourceModel } from "@app/resource-models/organisation/organisation.resource-model";
import { TenantResourceModel } from "@app/resource-models/tenant.resource-model";
import { OrganisationApiService } from "@app/services/api/organisation-api.service";
import { DataTableDefinitionService } from "@app/services/data-table-definition.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { ProductService } from "@app/services/product.service";
import { TenantService } from "@app/services/tenant.service";
import { DataTableDefinitionViewModel } from "@app/viewmodels/data-table-definition.viewmodel";
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { Permission } from '@app/helpers/permissions.helper';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';
import { PermissionService } from '@app/services/permission.service';

/**
 * Component for data table entity list in the master pane.
 */
@Component({
    selector: 'app-list-data-table-definition',
    templateUrl: './list-data-table-definition.page.html',
})
export class ListDataTableDefinitionPage implements OnInit {
    @ViewChild(EntityListComponent)
    public listComponent: EntityListComponent<DataTableDefinitionViewModel, DataTableDefinitionResourceModel>;

    public title: string = 'Data Tables';
    public viewModelConstructor: typeof DataTableDefinitionViewModel = DataTableDefinitionViewModel;
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.DataTableName]),
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;
    private entityType: string;
    private entityId: string;
    private tenantAlias: string;

    public constructor(
        private routeHelper: RouteHelper,
        private tenantService: TenantService,
        private productService: ProductService,
        private organisationApiService: OrganisationApiService,
        private navProxy: NavProxyService,
        public dataTableDefinitionService: DataTableDefinitionService,
        private permissionService: PermissionService,
    ) {
    }

    public ngOnInit(): void {
        this.initialiseAdditionalActionButtons();
    }

    public async loadDataTableDefinitions(): Promise<void> {
        this.tenantAlias = this.routeHelper.getContextTenantAlias();
        const productAlias: string = this.routeHelper.getParam('productAlias');
        const organisationId: string = this.routeHelper.getParam('organisationId');
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.tenantAlias);
        const productId: string =
            productAlias ? await this.productService.getProductIdFromAlias(this.tenantAlias, productAlias) : '';
        const productName: string =
            productAlias ? await this.productService.getProductNameFromAlias(this.tenantAlias, productAlias) : '';
        const tenant: TenantResourceModel =
            !!this.tenantAlias && !(!!productAlias || !!organisationId)
                ? await this.tenantService.getTenantFromAlias(this.tenantAlias) : null;
        const organisation: OrganisationResourceModel =
            organisationId ? await this.organisationApiService.getById(organisationId, params).toPromise() : null;

        if (productAlias) {
            this.entityType = EntityType.Product;
            this.entityId = productId;
            this.title += ` for ${productName}`;
        }

        if (organisationId) {
            this.entityType = EntityType.Organisation;
            this.entityId = organisationId;
            this.title += ` for ${organisation.name}`;
        }

        if (!!this.tenantAlias && !(!!productAlias || !!organisationId)) {
            this.entityType = EntityType.Tenant;
            this.entityId = tenant.id;
            this.title += ` for ${tenant.name}`;
        }
    }

    public async getDefaultHttpParams(): Promise<Map<string, string | Array<string>>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        await this.loadDataTableDefinitions();
        params.set('tenant', this.tenantAlias);
        params.set('entityType', this.entityType);
        params.set('entityId', this.entityId);
        return params;
    }

    public itemSelected(dataTableDefinition: DataTableDefinitionResourceModel): void {
        if (dataTableDefinition) {
            const pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
            pathSegments.push(dataTableDefinition.id);
            this.navProxy.navigate(pathSegments);
        }
    }

    public getSelectedId(): string {
        return this.routeHelper.getParam('dataTableDefinitionId');
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        const sortBy: any = params.get(SortFilterHelper.sortBy);
        if (sortBy) {
            params = SortFilterHelper.setSortAndFilterByParam(
                params,
                SortFilterHelper.sortBy,
                sortBy,
                this.getSortAndFilters());
        }

        const dateFilteringPropertyName: any = params.get(SortFilterHelper.dateFilteringPropertyName);
        if (dateFilteringPropertyName) {
            params = SortFilterHelper.setSortAndFilterByParam(
                params,
                SortFilterHelper.dateFilteringPropertyName,
                dateFilteringPropertyName,
                this.getSortAndFilters());
        }
    }

    public createNewDataTable(): void {
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push('create');
        this.navProxy.navigate(pathSegments);
    }

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.DataTableName,
            SortAndFilterByFieldName.DetailsName);
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.DetailsLastModifiedDate);
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }

    private initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasPermission(Permission.ManageDataTables)) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "add",
                IconLibrary.IonicV4,
                false,
                "Create Data Table",
                true,
                (): void => {
                    return this.createNewDataTable();
                },
                1,
            ));
        }
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
