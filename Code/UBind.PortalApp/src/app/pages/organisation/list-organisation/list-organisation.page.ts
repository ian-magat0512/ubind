import { Component, OnInit, OnDestroy, ViewChild } from "@angular/core";
import { SegmentedEntityListComponent } from "@app/components/entity-list/segmented-entity-list.component";
import { OrganisationResourceModel } from "@app/resource-models/organisation/organisation.resource-model";
import { Permission } from "@app/helpers";
import { OrganisationApiService } from "@app/services/api/organisation-api.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { OrganisationViewModel } from "@app/viewmodels/organisation.viewmodel";
import { contentAnimation } from "@assets/animations";
import { scrollbarStyle } from "@assets/scrollbar";
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { Subject, SubscriptionLike } from "rxjs";
import { EventService } from "@app/services/event.service";
import { RouteHelper } from "@app/helpers/route.helper";
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { AuthenticationService } from "@app/services/authentication.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { AdditionalPropertyDefinitionService } from "@app/services/additional-property-definition.service";
import { AdditionalPropertyDefinitionContextType } from "@app/models/additional-property-context-type.enum";
import { EntityType } from "@app/models/entity-type.enum";
import { PermissionService } from "@app/services/permission.service";
import { SortFilterHelper } from "@app/helpers/sort-filter.helper";
import { SortAndFilterBy, SortAndFilterByFieldName } from "@app/models/sort-filter-by.enum";
import { IconLibrary } from "@app/models/icon-library.enum";
import { ActionButton } from "@app/models/action-button";
import { AppConfigService } from "@app/services/app-config.service";
import { AppConfig } from "@app/models/app-config";
import { ActivatedRoute, Params } from "@angular/router";
import { takeUntil } from "rxjs/operators";

/**
 * Export list organisation page component class
 * TODO: Write a better class header: displaying of the organisation in the list.
 */
@Component({
    selector: 'app-list-organisation',
    templateUrl: './list-organisation.page.html',
    animations: [contentAnimation],
    styleUrls: ['../../../../assets/css/scrollbar-segment.css'],
    styles: [scrollbarStyle],
})
export class ListOrganisationPage implements OnInit, OnDestroy {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<OrganisationViewModel, OrganisationResourceModel>;

    public title: string = 'Organisations';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = ['Active', 'Disabled'];
    public defaultSegment: string = 'Active';
    public filterStatuses: Array<string> = ['Active', 'Disabled'];
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.OrganisationName], true),
        sortOrder: [
            SortDirection.Ascending,
            SortDirection.Descending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public viewModelConstructor: typeof OrganisationViewModel = OrganisationViewModel;
    private organisationId: string;
    private tenantId: string;
    private pathTenantAlias: string;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public additionalActionButtonList: Array<ActionButton> = [];
    public iconLibrary: typeof IconLibrary = IconLibrary;
    private destroyed: Subject<void> = new Subject<void>();

    public constructor(
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        public organisationApiService: OrganisationApiService,
        private routeHelper: RouteHelper,
        private eventService: EventService,
        private authenticationService: AuthenticationService,
        private sharedAlertService: SharedAlertService,
        private additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        private permissionService: PermissionService,
        private appConfigService: AppConfigService,
        private route: ActivatedRoute,
    ) {
        this.handleOrganisationDeleted();
        this.onOrganisationUpdatedChangeDefault();
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
        });
    }

    public ngOnInit(): void {
        this.route.params.pipe(takeUntil(this.destroyed)).subscribe((params: Params) => {
            this.pathTenantAlias = params['tenantAlias'];
        });
        this.initialiseAdditionalActionButtons();
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public didSelectCreate(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('organisation');
        pathSegments.push('create');
        this.listComponent.selectedId = null;
        this.additionalPropertyDefinitionService.verifyIfUserIsAllowedToProceed(
            this.tenantId,
            this.authenticationService.userType,
            AdditionalPropertyDefinitionContextType.Tenant,
            EntityType.Organisation,
            this.authenticationService.tenantId,
            "",
            false,
            this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues),
            () => {
                this.navProxy.navigateForward(pathSegments);
            },
            () => {
                this.sharedAlertService.showWithOk(
                    'You cannot create a new organisation',
                    'Because organisations in this context have at least one required additional property,'
                    + ' and you do not have permission to edit additional properties, you are unable '
                    + 'to create new organisations. For assistance, please speak to your administrator.',
                );
            },
        );
    }

    public async getSelectedId(): Promise<string> {
        if (this.organisationId) {
            return this.organisationId;
        } else {
            let organisationId: string = this.routeHelper.getParam('organisationId');
            if (organisationId) {
                return organisationId;
            }
            return null;
        }
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (this.pathTenantAlias) {
            params.set('tenant', this.pathTenantAlias);
        }
        return params;
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

    private handleOrganisationDeleted(): void {
        this.subscriptions.push(
            this.eventService.organisationStateChangedSubject$.subscribe(() => this.listComponent.load()),
        );
    }

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.OrganisationName,
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
        additionalActionButtonList.push(ActionButton.createActionButton(
            "Create",
            "plus",
            IconLibrary.AngularMaterial,
            false,
            "Create Organisation",
            true,
            (): void => {
                return this.didSelectCreate();
            },
            1,
        ));
        this.additionalActionButtonList = additionalActionButtonList;
    }

    public onOrganisationUpdatedChangeDefault(): void {
        this.eventService.getEntityUpdatedSubject('Organisation').pipe(takeUntil(this.destroyed))
            .subscribe((resourceModel: OrganisationResourceModel) => {
                if (resourceModel.isDefault) {
                    // find the current default and make it non default
                    let organisations: Array<OrganisationViewModel> = this.listComponent.repository.boundList;
                    let index: number = organisations.findIndex((organisation: OrganisationViewModel) => {
                        return organisation.isDefault;
                    });
                    if (index > -1) {
                        let viewModel: OrganisationViewModel = organisations[index];
                        if (viewModel.id !== resourceModel.id) {
                            viewModel.isDefault = false;
                        }
                    }
                }
            });
    }
}
