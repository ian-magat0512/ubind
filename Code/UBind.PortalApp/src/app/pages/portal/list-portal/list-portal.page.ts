import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { PortalDetailResourceModel, PortalResourceModel } from '@app/resource-models/portal/portal.resource-model';
import { Permission, QueryRequestHelper } from '@app/helpers';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { SegmentedEntityListComponent } from '../../../components/entity-list/segmented-entity-list.component';
import { PortalViewModel } from '../../../viewmodels/portal.viewmodel';
import { RouteHelper } from '../../../helpers/route.helper';
import { TenantResourceModel } from '../../../resource-models/tenant.resource-model';
import { TenantApiService } from '../../../services/api/tenant-api.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { AdditionalPropertyDefinitionService } from '@app/services/additional-property-definition.service';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { EntityType } from '@app/models/entity-type.enum';
import { ReplaySubject, Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { ActivatedRoute, NavigationExtras, Params } from '@angular/router';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import { OrganisationResourceModel } from '@app/resource-models/organisation/organisation.resource-model';
import { PermissionService } from '@app/services/permission.service';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';
import { RoutePaths } from '@app/helpers/route-path';
import { EventService } from '@app/services/event.service';
import { OrganisationModel } from '@app/models/organisation.model';

/**
 * Export list portal page component class
 * TODO: Write a better class header: displaying of portal details in list.
 */
@Component({
    selector: 'app-list-portal-page',
    templateUrl: './list-portal.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-segment.css',
    ],
    styles: [scrollbarStyle],
})
export class ListPortalPage implements OnInit, OnDestroy {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<PortalViewModel, PortalResourceModel>;

    public title: string = 'Portals';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = ['Active', 'Disabled'];
    public defaultSegment: string = 'Active';
    public filterStatuses: Array<string> = ['Active', 'Disabled'];
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.PortalName]),
        sortOrder: [SortDirection.Descending, SortDirection.Ascending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public viewModelConstructor: any = PortalViewModel;
    public tenantAlias: string;
    public tenantId: string;
    private tenantIdSubject: ReplaySubject<string> = new ReplaySubject<string>(1);
    private destroyed: Subject<void> = new Subject<void>();
    private organisationName: string;

    private pathOrganisationId: string;
    private portalOrganisationId: string;
    private defaultOrganisationId: string;
    private performingUserOrganisationId: string;
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        private routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public portalApiService: PortalApiService,
        protected tenantApiService: TenantApiService,
        private organisationApiService: OrganisationApiService,
        public layoutManager: LayoutManagerService,
        private authService: AuthenticationService,
        private sharedAlert: SharedAlertService,
        private additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        public appConfigService: AppConfigService,
        private route: ActivatedRoute,
        private permissionService: PermissionService,
        private eventService: EventService,
    ) {
        this.onPortalUpdatedReloadPortals();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.eventService.performingUserOrganisationSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((organisation: OrganisationModel) => {
                this.performingUserOrganisationId = organisation.id;
            });
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
            this.tenantAlias = appConfig.portal.tenantAlias;
            this.organisationName = appConfig.portal.organisationName;
        });
        this.route.params.pipe(takeUntil(this.destroyed)).subscribe((params: Params) => {
            this.pathOrganisationId = params['organisationId'];
        });
        this.loadTenant();
        this.initialiseAdditionalActionButtons();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async loadTenant(): Promise<void> {
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');
        if (this.tenantAlias &&
            this.permissionService.hasPermission(Permission.ViewTenants)) {
            await this.loadTenantFromAlias(this.tenantAlias);
        } else {
            let portalId: string = this.routeHelper.getParam('portalId');
            if (portalId && !this.pathOrganisationId) {
                await this.loadPortalOrganisationId(this.tenantId, portalId);
            }
        }

        if (this.permissionService.hasPermission(Permission.ViewOrganisations)) {
            await this.loadOrganisationName(this.getContextOrganisationId());
        }

        this.tenantIdSubject.next(this.tenantId);
    }

    private async loadTenantFromAlias(tenantAlias: string): Promise<void> {
        return this.tenantApiService.get(tenantAlias)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((tenant: TenantResourceModel) => {
                this.tenantId = tenant.id;
                this.defaultOrganisationId = tenant.defaultOrganisationId;
                this.title = tenant.name + ' Portals';
            });
    }

    private async loadPortalOrganisationId(tenantId: string, portalId: string): Promise<void> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenantId', tenantId);
        return this.portalApiService.getById(portalId, params)
            .pipe(takeUntil(this.destroyed)).toPromise().then((resourceModel: PortalDetailResourceModel) => {
                this.portalOrganisationId = resourceModel.organisationId;
            });
    }

    private async loadOrganisationName(organisationId: string): Promise<void> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        return this.organisationApiService.getById(organisationId, params)
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((organisation: OrganisationResourceModel) => {
                this.organisationName = organisation.name;
                this.title = this.organisationName + ' Portals';
            });
    }

    public async getDefaultHttpParams(): Promise<Map<string, string | Array<string>>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        return new Promise<any>((resolve: any): any => {
            let subscription: any = this.tenantIdSubject
                .pipe(finalize(() => subscription.unsubscribe()))
                .subscribe((tenantId: string) => {
                    params.set('tenant', tenantId);
                    params.set('organisation', this.getContextOrganisationId());
                    resolve(params);
                });
        });
    }

    public createNewPortal(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('portal');
        pathSegments.push('create');
        this.listComponent.selectedId = null;
        this.additionalPropertyDefinitionService.verifyIfUserIsAllowedToProceed(
            this.tenantId,
            this.authService.userType,
            AdditionalPropertyDefinitionContextType.Tenant,
            EntityType.Portal,
            this.tenantId,
            "",
            false,
            this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues),
            () => {
                this.navProxy.navigate(pathSegments);
            },
            () => {
                this.sharedAlert.showWithOk(
                    'You cannot create a new portal',
                    'Because portals in this context have at least one required additional property,'
                    + ' and you do not have permission to edit additional properties, you are unable '
                    + 'to create new portals. For assistance, please speak to your administrator.',
                );
            },
        );
    }

    private getContextOrganisationId(): string {
        return this.pathOrganisationId || this.portalOrganisationId
            || this.defaultOrganisationId || this.performingUserOrganisationId;
    }

    private getSelectedId(): string {
        return this.routeHelper.getParam('portalId');
    }

    public getUserFilterSelection(): void {
        let dateData: Array<any> = QueryRequestHelper.constructDateFilters(this.listComponent.filterSelections);
        let portalId: string = this.getSelectedId();

        let navigationExtras: NavigationExtras = {
            state: {
                filterTitle: 'Filter & Sort Portals',
                statusTitle: 'Status',
                entityTypeName: this.listComponent.entityTypeName,
                statusList: QueryRequestHelper.constructStringFilters(
                    this.filterStatuses,
                    this.listComponent.filterSelections,
                ),
                filterByDates: this.filterByDates,
                dateIsBefore: dateData['before'],
                dateIsAfter: dateData['after'],
                selectedId: portalId,
                sortOptions: this.sortOptions,
                selectedSortOption: QueryRequestHelper.constructSortFilters(this.listComponent.filterSelections),
                selectedDateFilteringPropertyName: QueryRequestHelper.constructFilterByDate(
                    this.listComponent.filterSelections,
                ),
            },
        };
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments = this.routeHelper.getPathSegmentsUpUntil(RoutePaths.portal);
        pathSegments.push('filter');
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
                this.getSortAndFilters());
        }
    }

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.PortalName,
            SortAndFilterByFieldName.DetailsName,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.DetailsLastModifiedDate,
        );
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }

    public initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasPermission(Permission.ManagePortals)) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Portal",
                true,
                (): void => {
                    return this.createNewPortal();
                },
                1,
            ));
        }
        this.additionalActionButtonList = additionalActionButtonList;
    }

    public onPortalUpdatedReloadPortals(): void {
        this.eventService.getEntityUpdatedSubject('Portal').pipe(takeUntil(this.destroyed))
            .subscribe((resourceModel: PortalDetailResourceModel) => {
                if (resourceModel) { // will be null if we navigate away whilst loading
                    if (this.listComponent
                        && this.listComponent.repository
                        && this.listComponent.repository.boundList
                    ) {
                        // find the portal with the same ID in the data list
                        let portals: Array<PortalViewModel> = this.listComponent.repository.boundList;
                        let index: number = portals.findIndex((p: PortalViewModel) => p.id === resourceModel.id);
                        if (index > -1) {
                            let viewModel: PortalViewModel = portals[index];
                            if (viewModel.isDefault != resourceModel.isDefault) {
                                // if the default portal has changed, reload the list
                                this.listComponent.load();
                            }
                        }
                    }
                }
            });
    }
}
