import { Component, OnInit, ViewChild } from '@angular/core';
import { EmailApiService } from '@app/services/api/email-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { NavigationExtras } from '@angular/router';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Permission } from '@app/helpers/permissions.helper';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { MessageService } from '@app/services/message.service';
import { MessageResourceModel } from '@app/resource-models/message.resource-model';
import { MessageViewModel } from '@app/viewmodels/message.viewmodel';
import { RouteHelper } from '@app/helpers/route.helper';
import { SortAndFilterBy } from '@app/models/sort-filter-by.enum';
import { QueryRequestHelper } from '@app/helpers/query-request.helper';
import { EntityListComponent } from '../entity-list/entity-list.component';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { RoutePaths } from '@app/helpers/route-path';
import { IconLibrary } from '@app/models/icon-library.enum';

/**
 * Base entity list message page
 * This class component is all the base entity list message functions
 */
@Component({
    template: '',
})
export abstract class BaseEntityListMessagePage implements OnInit {
    @ViewChild(EntityListComponent)
    public listComponent: EntityListComponent<MessageViewModel, MessageResourceModel>;

    public title: string;
    public permission: typeof Permission = Permission;
    private entityId: string;
    public viewModelConstructor: typeof MessageViewModel = MessageViewModel;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public sortOptions: SortOption = {
        sortBy: [SortAndFilterBy.SentDate],
        sortOrder: [SortDirection.Descending, SortDirection.Ascending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();

    public constructor(
        protected routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        protected emailApiService: EmailApiService,
        protected userPath: UserTypePathHelper,
        public messageService: MessageService,
    ) { }

    public ngOnInit(): void {
        this.entityId = this.routeHelper.getParam(this.getEntityIdParamName());
        this.loadEntityName(this.entityId);
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        let paramsArr: Array<string> = [];
        params.set('status', paramsArr);
        params.set('entityType', this.getEntityType());
        params.set('entityId', this.entityId);
        return params;
    }

    public itemSelected(item: MessageViewModel): void {
        this.entityId = this.routeHelper.getParam(this.getEntityIdParamName());
        let pathSegments: Array<string> = this.routeHelper.getModulePathSegments();
        pathSegments.push(this.entityId);
        pathSegments.push("message");
        pathSegments.push(item.type);
        pathSegments.push(item.id);
        this.navProxy.navigateForward(pathSegments, true);
    }

    public getUserFilterChips(): void {
        let dateData: Array<any> = QueryRequestHelper.constructDateFilters(this.listComponent.filterSelections);
        let navigationExtras: NavigationExtras = {
            state: {
                filterTitle: 'Filter & Sort ' + this.listComponent.entityTypeName + 's',
                statusTitle: "Status",
                entityTypeName: this.listComponent.entityTypeName,
                statusList: QueryRequestHelper.constructStringFilters(
                    this.listComponent.filterStatuses,
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
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil(RoutePaths.message);
        pathSegments.push('filter');
        this.navProxy.navigateForward(pathSegments, true, navigationExtras);
    }

    protected abstract getEntityType(): string;
    protected abstract getEntityIdParamName(): string;
    protected abstract loadEntityName(entityId: string): void;
}
