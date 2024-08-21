import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { Permission } from '@app/helpers';
import { ReportApiService } from '@app/services/api/report-api.service';
import { ReportResourceModel } from '@app/resource-models/report.resource-model';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { ReportViewModel } from '@app/viewmodels/report.viewmodel';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { EntityType } from '@app/models/entity-type.enum';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';

/**
 * Export list report page component class.
 * This class manage displaying the report in the list.
 */
@Component({
    selector: 'app-list-report',
    templateUrl: './list-report.page.html',
    styleUrls: ['./list-report.page.scss'],
})
export class ListReportPage implements OnInit {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<ReportViewModel, ReportResourceModel>;

    public title: string = 'Reports';
    public permission: typeof Permission = Permission;
    public filterStatuses: Array<string> = ['Deleted'];
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.ReportName]),
        sortOrder: [SortDirection.Descending, SortDirection.Ascending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public viewModelConstructor: typeof ReportViewModel = ReportViewModel;
    public portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    public entityTypes: typeof EntityType = EntityType;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        public reportApiService: ReportApiService,
        public layoutManager: LayoutManagerService,
        protected route: ActivatedRoute,
        protected authService: AuthenticationService,
        protected portalExtensionService: PortalExtensionsService,
        private permissionService: PermissionService,
    ) {
    }

    public ngOnInit(): void {
        this.initialiseAdditionalActionButtons();
    }

    public createReportButtonClick(): void {
        this.navProxy.navigateForward(['report', 'create']);
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
            SortAndFilterBy.ReportName,
            SortAndFilterByFieldName.Name,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.LastModifiedDate,
        );
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }

    private initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasPermission(Permission.ManageReports)) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Report",
                true,
                (): void => {
                    return this.createReportButtonClick();
                },
                1,
            ));
        }
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
