import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { EmailApiService } from '@app/services/api/email-api.service';
import { LoadDataService } from '@app/services/load-data.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ActivatedRoute, NavigationExtras } from '@angular/router';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { Permission } from '@app/helpers/permissions.helper';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { MessageService } from '@app/services/message.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export list master email page component class
 * TODO: Write a better class header: displaying of master email in the list.
 */
@Component({
    selector: 'app-list-master-message',
    templateUrl: './list-master-message.page.html',
    styleUrls: [
        './list-master-message.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    animations: [contentAnimation],
    styles: [scrollbarStyle],
})
export class ListMasterMessagePage {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<MessageViewModel, MessageResourceModel>;

    public title: string = 'Messages';
    public sortOptions: SortOption = {
        sortBy: [SortAndFilterBy.SentDate],
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending,
        ],
    };
    public filterByDates: Array<string> = [SortAndFilterBy.SentDate];
    public permission: typeof Permission = Permission;
    public viewModelConstructor: typeof MessageViewModel = MessageViewModel;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        public navProxy: NavProxyService,
        private route: ActivatedRoute,
        public emailApiService: EmailApiService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        public messageService: MessageService,
        private routeHelper: RouteHelper,
    ) {
    }

    public itemSelected(item: MessageViewModel): void {
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        let navigationExtras: NavigationExtras = { queryParams: { type: item.type } };
        this.navProxy.navigateForward(pathSegments, true, navigationExtras);
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
            SortAndFilterBy.SentDate,
            SortAndFilterByFieldName.CreatedDate,
        );
        return sortAndFilter;
    }
}
