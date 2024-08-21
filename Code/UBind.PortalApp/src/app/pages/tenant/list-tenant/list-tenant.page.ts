import { Component, OnInit, ViewChild } from '@angular/core';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission } from '@app/helpers/permissions.helper';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { TenantViewModel } from '@app/viewmodels/tenant.viewmodel';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { SegmentedEntityListComponent } from '../../../components/entity-list/segmented-entity-list.component';
import { RouteHelper } from '../../../helpers/route.helper';
import { TenantService } from '@app/services/tenant.service';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list tenant page component class.
 * This class manage displaying the tenant in the list.
 */
@Component({
    selector: 'app-list-tenant',
    templateUrl: './list-tenant.page.html',
    animations: [contentAnimation],
    styleUrls: ['../../../../assets/css/scrollbar-segment.css'],
    styles: [scrollbarStyle],
})
export class ListTenantPage implements OnInit {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<TenantViewModel, TenantResourceModel>;

    public title: string = 'Tenants';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = ['Active', 'Disabled'];
    public defaultSegment: string = 'Active';
    public filterStatuses: Array<string> = ['Active', 'Disabled'];
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.TenantName], true),
        sortOrder: [SortDirection.Ascending, SortDirection.Descending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public viewModelConstructor: typeof TenantViewModel = TenantViewModel;
    private tenantId: string;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        public tenantApiService: TenantApiService,
        public layoutManager: LayoutManagerService,
        private routeHelper: RouteHelper,
        private tenantService: TenantService,
        private permissionService: PermissionService,
    ) {
    }

    public ngOnInit(): void {
        this.initialiseAdditionalActionButtons();
    }

    public userDidTapAddButton(): void {
        this.navProxy.navigateForward(['tenant', 'create']);
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

    public itemSelected(tenant: TenantViewModel): void {
        this.navProxy.navigateForward(['tenant', tenant.alias]);
    }

    public async getSelectedId(): Promise<string> {
        if (this.tenantId) {
            return this.tenantId;
        } else {
            let tenantAlias: string = this.routeHelper.getParam('tenantAlias');
            if (tenantAlias) {
                this.tenantId = await this.tenantService.getTenantIdFromAlias(tenantAlias);
                return this.tenantId;
            } else {
                return null;
            }
        }
    }

    private initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasPermission(Permission.ManageTenants)) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Tenant",
                true,
                (): void => {
                    return this.userDidTapAddButton();
                },
                1,
            ));
        }
        this.additionalActionButtonList = additionalActionButtonList;
    }

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.TenantName,
            SortAndFilterByFieldName.DetailsName,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.DetailsLastModifiedDate,
        );
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }
}
