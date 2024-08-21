import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { contentAnimation } from '@assets/animations';
import { EmailApiService } from '@app/services/api/email-api.service';
import { LoadDataService } from '@app/services/load-data.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { NavigationExtras } from '@angular/router';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { Permission } from '@app/helpers/permissions.helper';
import { StringHelper } from '@app/helpers/string.helper';
import { MapHelper } from '@app/helpers/map.helper';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { MessageService } from '@app/services/message.service';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { RouteHelper } from '@app/helpers/route.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { QueryRequestHelper } from '@app/helpers';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Export list email page component class
 * This class manage of displaying the email details in the list.
 */
@Component({
    selector: 'app-list-message',
    templateUrl: './list-message.page.html',
    styleUrls: [
        './list-message.page.scss',
        '../../../../assets/css/scrollbar-segment.css',
    ],
    animations: [contentAnimation],
    styles: [scrollbarStyle],
})
export class ListMessagePage {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<MessageViewModel, MessageResourceModel>;

    public title: string = 'Messages';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = ['Customer', 'Client', 'User'];
    public defaultSegment: string = 'Customer';
    public filterStatuses: Array<string> = ['Customer', 'Client', 'User'];
    public sortOptions: SortOption = {
        sortBy: [SortAndFilterBy.SentDate],
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending,
        ],
    };
    public filterByDates: Array<string> = [SortAndFilterBy.SentDate];
    public viewModelConstructor: typeof MessageViewModel = MessageViewModel;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        public navProxy: NavProxyService,
        public emailApiService: EmailApiService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        private stringHelper: StringHelper,
        public messageService: MessageService,
        private routeHelper: RouteHelper,
    ) { }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {

        let paramsArr: Array<string> = [];

        if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'client',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )) {
            paramsArr.push('admin');
        }
        if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'customer',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )) {
            paramsArr.push('customer');
        }
        if (MapHelper.containsEntryWithValue(
            params,
            'status',
            'user',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        )) {
            paramsArr.push('user');
        }

        params.set('status', paramsArr);

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

    public itemSelected(item: MessageViewModel): void {
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        let navigationExtras: NavigationExtras = { queryParams: { type: item.type } };
        this.navProxy.navigateForward(pathSegments, true, navigationExtras);
    }

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.SentDate,
            SortAndFilterByFieldName.CreatedDate,
        );
        return sortAndFilter;
    }

    public getUserFilterChips(): void {
        let dateData: Array<any> = QueryRequestHelper.constructDateFilters(this.listComponent.filterSelections);

        let navigationExtras: NavigationExtras = {
            state: {
                filterTitle: 'Filter & Sort ' + this.listComponent.entityTypeName + 's',
                statusTitle: "Status",
                entityTypeName: this.listComponent.entityTypeName,
                statusList: QueryRequestHelper.constructStringFilters(
                    this.filterStatuses,
                    this.listComponent.filterSelections,
                ),
                filterByDates: this.filterByDates,
                dateIsBefore: dateData['before'],
                dateIsAfter: dateData['after'],
                testData: QueryRequestHelper.getTestDataFilter(this.listComponent.filterSelections),
                sortOptions: this.sortOptions,
                selectedSortOption: QueryRequestHelper.constructSortFilters(this.listComponent.filterSelections),
                selectedFilterByDate: QueryRequestHelper.constructFilterByDate(this.listComponent.filterSelections),
            },
        };
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        if (pathSegments.includes('email')) {
            pathSegments.pop();
        }
        pathSegments.pop();
        pathSegments.push('filter');
        this.navProxy.navigateForward(pathSegments, true, navigationExtras);
    }
}
