import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { EntityListComponent } from '@app/components/entity-list/entity-list.component';
import { RoleResourceModel } from '@app/resource-models/role.resource-model';
import { Permission } from '@app/helpers';
import { RoleApiService } from '@app/services/api/role-api.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { RoleViewModel } from '@app/viewmodels/role.viewmodel';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { AuthenticationService } from '@app/services/authentication.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { EntityType } from '@app/models/entity-type.enum';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButton } from '@app/models/action-button';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '@app/services/app-config.service';

/**
 * Export list role page component class.
 * This class manage displaying the role details in the list.
 */
@Component({
    selector: 'app-list-role',
    templateUrl: './list-role.page.html',
    styleUrls: ['./list-role.page.scss'],
    animations: [contentAnimation],
    styles: [scrollbarStyle],
})
export class ListRolePage implements OnInit, AfterViewInit, OnDestroy {

    @ViewChild(EntityListComponent, { static: true })
    public listComponent: EntityListComponent<RoleViewModel, RoleResourceModel>;

    public title: string = 'Roles';
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.RoleName]),
        sortOrder: [SortDirection.Descending, SortDirection.Ascending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public permission: typeof Permission = Permission;
    public viewModelConstructor: typeof RoleViewModel = RoleViewModel;

    private entityTypes: typeof EntityType = EntityType;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    public actions: Array<ActionButtonPopover> = [];
    public hasActionsIncludedInMenu: boolean = false;
    public additionalActionButtonList: Array<ActionButton> = [];
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    private environment: DeploymentEnvironment;
    private destroyed: Subject<void> = new Subject<void>();

    public constructor(
        public navProxy: NavProxyService,
        public roleService: RoleApiService,
        private authService: AuthenticationService,
        private sharedPopoverService: SharedPopoverService,
        private portalExtensionService: PortalExtensionsService,
        private permissionService: PermissionService,
        appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.pipe(takeUntil(this.destroyed)).subscribe((appConfig: AppConfig) => {
            this.environment = <DeploymentEnvironment>appConfig.portal.environment;
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.initialiseAdditionalActionButtons();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public createRoleButtonClick(): void {
        this.listComponent.selectedId = null;
        this.navProxy.navigateForward(['role', 'create']);
    }

    public ngAfterViewInit(): void {
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Role,
                PageType.List,
            );
    }

    private generatePopoverLinks(): void {
        // Add portal page trigger actions
        this.actions = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.Role,
            PageType.List,
        );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        let filters: Map<string, string | Array<string>> = await this.listComponent.getListQueryHttpParams();
        filters.set('tenantId', this.authService.tenantId);
        filters.set('environment', this.environment);
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action && command.data.action.portalPageTrigger) {
                this.portalExtensionService.executePortalPageTrigger(
                    command.data.action.portalPageTrigger,
                    this.entityTypes.Role,
                    PageType.List,
                    null,
                    null,
                    filters,
                );
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverViewComponent,
                cssClass: 'custom-popover-list more-button-top-popover-positioning',
                componentProps: {
                    actions: this.actions,
                },
                event: event,
            },
            'Roles option popover',
            popoverDismissAction,
        );
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
            SortAndFilterBy.RoleName,
            SortAndFilterByFieldName.Name,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.LastModifiedDate,
        );
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }

    public initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.permissionService.hasOneOfPermissions([
            Permission.ManageRoles,
            Permission.ManageRolesForAllOrganisations])) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create Role",
                true,
                (): void => {
                    return this.createRoleButtonClick();
                },
                1,
            ));
        }
        this.additionalActionButtonList = additionalActionButtonList;
    }
}
