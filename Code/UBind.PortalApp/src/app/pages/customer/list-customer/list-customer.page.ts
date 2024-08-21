import { Component, OnInit, ChangeDetectorRef, ViewChild, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { BroadcastService } from '@app/services/broadcast.service';
import { LoadDataService } from '@app/services/load-data.service';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { Permission, StringHelper } from '@app/helpers';
import { scrollbarStyle } from '@assets/scrollbar';
import { contentAnimation } from '@assets/animations';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { CustomerViewModel } from '@app/viewmodels/customer.viewmodel';
import { MapHelper } from '@app/helpers/map.helper';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { EntityType } from '@app/models/entity-type.enum';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { AdditionalPropertyDefinitionService } from '@app/services/additional-property-definition.service';
import { PermissionService } from '@app/services/permission.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EntityViewModel } from '@app/viewmodels/entity.viewmodel';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { EventService } from '@app/services/event.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';
import { AppConfig } from '@app/models/app-config';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

/**
 * Export list customer page component class
 * TODO: Write a better class header: listing of customer details.
 */
@Component({
    selector: 'app-list-customer',
    templateUrl: './list-customer.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListCustomerPage implements OnInit, OnDestroy {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<CustomerViewModel, CustomerResourceModel>;

    public title: string = 'Customers';
    public permission: typeof Permission = Permission;
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.CustomerName]),
        sortOrder: [
            SortDirection.Descending,
            SortDirection.Ascending,
        ],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public viewModelConstructor: typeof CustomerViewModel = CustomerViewModel;
    public defaultUserImgPath: string = 'assets/imgs/person-md.svg';
    public defaultUserImgFilter: string =
        'invert(52%) sepia(0%) saturate(0%) hue-rotate(153deg) brightness(88%) contrast(90%)';
    public additionalActionButtonList: Array<ActionButton> = [];
    private tenantId: string;
    private destroyed: Subject<void>;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        protected changeDetectorRef: ChangeDetectorRef,
        protected route: ActivatedRoute,
        public navProxy: NavProxyService,
        protected appConfigService: AppConfigService,
        public customerApiService: CustomerApiService,
        protected authService: AuthenticationService,
        protected broadcastService: BroadcastService,
        protected loadDataService: LoadDataService,
        public layoutManager: LayoutManagerService,
        private stringHelper: StringHelper,
        private permissionService: PermissionService,
        private sharedAlertService: SharedAlertService,
        private additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        private routeHelper: RouteHelper,
        private eventService: EventService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        const customerId: string
            = this.routeHelper.getParam('customerId') || this.route.snapshot.paramMap.get('customerId');
        if (customerId) {
            this.listComponent.selectedId = customerId;
        }

        this.initialiseAdditionalActionButtons();

        this.eventService.customerUpdatedSubject$
            .pipe(takeUntil(this.destroyed))
            .subscribe(() => {
                this.listComponent.toggleReload();
            });
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public didSelectCreate(): void {
        this.listComponent.selectedId = null;
        this.additionalPropertyDefinitionService.verifyIfUserIsAllowedToProceed(
            this.tenantId,
            this.authService.userType,
            AdditionalPropertyDefinitionContextType.Organisation,
            EntityType.Customer,
            this.authService.userOrganisationId,
            this.authService.tenantId,
            true,
            this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues),
            () => {
                this.navProxy.navigateForward(['customer', 'create']);
            },
            () => {
                this.sharedAlertService.showWithOk(
                    'You cannot create a new customer',
                    'Because customers in this context have at least one required additional property,'
                    + ' and you do not have permission to edit additional properties, you are unable '
                    + 'to create new customers. For assistance, please speak to your administrator.',
                );
            },
        );
    }

    public onListQueryParamsGenerated(params: Map<string, string | Array<string>>): void {
        MapHelper.replaceEntryValue(
            params,
            'status',
            'disabled',
            'deactivated',
            this.stringHelper.equalsIgnoreCase.bind(this.stringHelper),
        );

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

    public itemSelected(item: EntityViewModel): void {
        this.navProxy.navigateForward(['customer', item.id], true, { queryParams: { segment: 'Details' } });
    }

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultUserImgPath, this.defaultUserImgFilter);
    }

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.CustomerName,
            SortAndFilterByFieldName.PersonDisplayName,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.LastModifiedDate,
        );
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }

    private initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasOneOfPermissions([
            Permission.ManageCustomers,
            Permission.ManageAllCustomers,
            Permission.ManageAllCustomersForAllOrganisations])) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Customer",
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
