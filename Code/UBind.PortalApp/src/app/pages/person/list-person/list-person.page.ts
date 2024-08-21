import { Component, ChangeDetectorRef, ViewChild, OnInit, OnDestroy } from "@angular/core";
import { scrollbarStyle } from '@assets/scrollbar';
import { contentAnimation } from '@assets/animations';
import { NavProxyService } from "@app/services/nav-proxy.service";
import { ActivatedRoute, Router } from "@angular/router";
import { PersonApiService } from "@app/services/api/person-api.service";
import { LoadDataService } from "@app/services/load-data.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { EntityListComponent } from "@app/components/entity-list/entity-list.component";
import { PersonViewModel } from "@app/viewmodels";
import { PersonResourceModel } from "@app/models";
import { RouteHelper } from "@app/helpers/route.helper";
import { EventService } from "@app/services/event.service";
import { finalize, takeUntil } from "rxjs/operators";
import { Subject, Subscription } from "rxjs";
import { CustomerApiService } from "@app/services/api/customer-api.service";
import { Permission } from "@app/helpers";
import { EntityViewModel } from "@app/viewmodels/entity.viewmodel";
import { SortOption } from "@app/components/filter/sort-option";
import { SortDirection } from "@app/viewmodels/sorted-entity.viewmodel";
import { SortFilterHelper } from "@app/helpers/sort-filter.helper";
import { SortAndFilterBy, SortAndFilterByFieldName } from "@app/models/sort-filter-by.enum";
import { IconLibrary } from "@app/models/icon-library.enum";
import { PermissionService } from "@app/services/permission.service";
import { ActionButton } from "@app/models/action-button";

/**
 * Export list person page class.
 * This class is for listing of people.
 */
@Component({
    selector: 'app-list-person',
    templateUrl: './list-person.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
        './list-person.page.scss',
    ],
    styles: [scrollbarStyle],
})
export class ListPersonPage implements OnInit, OnDestroy {
    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<PersonViewModel, PersonResourceModel>;

    public permission: typeof Permission = Permission;
    public viewModelConstructor: typeof PersonViewModel = PersonViewModel;
    public title: string = 'People';
    public primaryPersonId: string;
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.CustomerName]),
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending,
        ],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    private customerId: string;
    private customerName: string = '';
    protected destroyed: Subject<void>;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        protected changeDetectorRef: ChangeDetectorRef,
        private routeHelper: RouteHelper,
        protected router: Router,
        protected route: ActivatedRoute,
        public personService: PersonApiService,
        public customerService: CustomerApiService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        protected eventService: EventService,
        private permissionService: PermissionService,
    ) {
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.customerId = this.routeHelper.getParam('customerId');
        if (this.customerId) {
            this.loadPrimaryPerson();
        }
        this.listenForPersonUpdates();
        this.initialiseAdditionalActionButtons();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("customerId", this.customerId);
        return params;
    }

    public itemSelected(item: EntityViewModel): void {
        this.navProxy.navigateForward(['customer', this.customerId, 'person', item.id]);
    }

    public didSelectCreate(): void {
        this.navProxy.navigateForward(['customer', this.customerId, 'person', 'create']);
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
            SortAndFilterBy.CustomerName,
            SortAndFilterByFieldName.DisplayName,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.LastModifiedDate,
        );
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }

    private async loadPrimaryPerson(): Promise<void> {
        return new Promise((resolve: any, reject: any): void => {
            const subscription: Subscription = this.customerService.getPrimaryPerson(this.customerId)
                .pipe(finalize(() => subscription.unsubscribe()))
                .subscribe((data: PersonResourceModel) => {
                    let model: PersonViewModel = new PersonViewModel(data);
                    this.primaryPersonId = model.id;
                    this.customerName = model.fullName;
                    this.title = `${this.customerName} People`;
                    resolve();
                });
        });
    }

    private listenForPersonUpdates(): void {
        this.eventService.getEntityUpdatedSubject('Person')
            .pipe(takeUntil(this.destroyed))
            .subscribe((data: any) => {
                this.listComponent.load();
                let fromEvent: string = data['fromEvent'];
                if (fromEvent !== 'deleted' && fromEvent === 'setToPrimary') {
                    this.primaryPersonId = data.id;
                    this.customerName = data.fullName || data.name;
                    this.title = `${this.customerName} People`;
                } else {
                    this.ngOnDestroy();
                }
            });
    }

    private initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasPermission(Permission.ManageCustomers)) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Person",
                true,
                (): void => {
                    return this.didSelectCreate();
                },
                1,
            ));
        }
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
