import { OnInit, OnDestroy, ViewChild, Directive } from '@angular/core';
import { SegmentedEntityListComponent } from '@app/components/entity-list/segmented-entity-list.component';
import { UserViewModel } from '@app/viewmodels/user.viewmodel';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { SubscriptionLike } from 'rxjs';
import { Permission, StringHelper } from '@app/helpers';
import { SortOption } from '@app/components/filter/sort-option';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';
import { MapHelper } from '@app/helpers/map.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { NavigationExtras } from '@angular/router';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserType } from '@app/models/user-type.enum';
import { AdditionalPropertyDefinitionService } from '@app/services/additional-property-definition.service';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { EntityType } from '@app/models/entity-type.enum';
import { UserService } from '@app/services/user.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { PermissionService } from '@app/services/permission.service';
import { SortFilterHelper } from '@app/helpers/sort-filter.helper';
import { SortAndFilterBy, SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { AppConfig } from '@app/models/app-config';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { RouteHelper } from '@app/helpers/route.helper';
import { Errors } from '@app/models/errors';

/**
 * Base class for list user page.
 */
@Directive({ selector: '[appCommonListUser]' })
export abstract class CommonListUserPage implements OnInit, OnDestroy {

    @ViewChild(SegmentedEntityListComponent, { static: true })
    public listComponent: SegmentedEntityListComponent<UserViewModel, UserResourceModel>;

    public title: string = 'Users';
    public permission: typeof Permission = Permission;
    public segments: Array<string> = ['New', 'Invited', 'Active', 'Disabled'];
    public defaultSegment: string = 'Active';
    public filterStatuses: Array<string> = ['New', 'Invited', 'Active', 'Disabled'];
    public sortOptions: SortOption = {
        sortBy: SortFilterHelper.getEntitySortAndFilter([SortAndFilterBy.UserName]),
        sortOrder: [SortDirection.Descending, SortDirection.Ascending],
    };
    public filterByDates: Array<string> = SortFilterHelper.getEntitySortAndFilter();
    public viewModelConstructor: typeof UserViewModel = UserViewModel;
    public defaultUserImgPath: string = 'assets/imgs/default-user.svg';
    public defaultUserImgFilter: string
        = 'invert(52%) sepia(0%) saturate(0%) hue-rotate(153deg) brightness(88%) contrast(90%)';

    private subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public canCreateUser: boolean = false;
    public additionalActionButtonList: Array<ActionButton> = [];
    private tenantId: string;

    public constructor(
        protected appConfigService: AppConfigService,
        protected navProxy: NavProxyService,
        protected authService: AuthenticationService,
        protected stringHelper: StringHelper,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        protected userService: UserService,
        protected sharedAlertService: SharedAlertService,
        protected permissionService: PermissionService,
        protected routeHelper: RouteHelper,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantId = appConfig.portal.tenantId;
        });
    }

    public ngOnInit(): void {
        this.initialiseAdditionalActionButtons();
    }

    public ngOnDestroy(): void {
        this.subscriptions.forEach((s: SubscriptionLike) => s.unsubscribe());
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

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultUserImgPath, this.defaultUserImgFilter);
    }

    public didSelectCreate(): void {
        this.listComponent.selectedId = null;
        this.additionalPropertyDefinitionService.verifyIfUserIsAllowedToProceed(
            this.tenantId,
            this.authService.userType,
            AdditionalPropertyDefinitionContextType.Organisation,
            EntityType.User,
            this.authService.userOrganisationId,
            this.authService.tenantId,
            true,
            this.permissionService.hasPermission(this.permission.EditAdditionalPropertyValues),
            () => {
                let params: NavigationExtras;
                this.navProxy.navigateForward(['user', 'create'], true, params);
            },
            () => {
                this.sharedAlertService.showWithOk(
                    'You cannot create a new user',
                    'Because users in this context have at least one required additional property,'
                    + ' and you do not have permission to edit additional properties, you are unable '
                    + 'to create new users. For assistance, please speak to your administrator.',
                );
            },
        );
    }

    public getDefaultHttpParams(): Map<string, string | Array<string>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        if (this.authService.isMasterUser() && !this.routeHelper.hasPathSegment('organisation')) {
            // the user is a master user listing master users
            params.set('userTypes', [UserType.Master]);
            params.set('organisation', this.authService.userOrganisationId);
        } else if (this.authService.isAgent()
            || (this.authService.isMasterUser() && this.routeHelper.hasPathSegment('organisation'))
        ) {
            // the user is a agent user listing agent users, or a master user listing agent users
            params.set('userTypes', [UserType.Client]);
            const contextOrganisationid: string
                = this.routeHelper.getParam('organisationId') || this.authService.userOrganisationId;
            params.set('organisation', contextOrganisationid);
        } else {
            // the user is a a customer, so they shouldn't be listing users
            throw Errors.User.AccessDenied("You are not allowed to list users.");
        }
        return params;
    }

    private getSortAndFilters(): Map<string, string> {
        let sortAndFilter: Map<string, string> = new Map<string, string>();
        sortAndFilter.set(
            SortAndFilterBy.UserName,
            SortAndFilterByFieldName.FullName,
        );
        sortAndFilter.set(
            SortAndFilterBy.LastModifiedDate,
            SortAndFilterByFieldName.LastModifiedDate,
        );
        return SortFilterHelper.getEntitySortAndFiltersMap(sortAndFilter);
    }

    public initialiseAdditionalActionButtons(): void {
        let additionalActionButtonList: Array<ActionButton> = [];
        if (this.canCreateUser ||
            this.permissionService.hasOneOfPermissions([
                Permission.ManageUsers,
                Permission.ManageUsersForOtherOrganisations])) {
            additionalActionButtonList.push(ActionButton.createActionButton(
                "Create",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Create User",
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
