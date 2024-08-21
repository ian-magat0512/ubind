import { Component, Injector, ElementRef, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { SafeUrl } from '@angular/platform-browser';
import { Subject, Subscription, SubscriptionLike, Observable } from 'rxjs';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AppConfigService } from '@app/services/app-config.service';
import { UserApiService } from '@app/services/api/user-api.service';
import { CustomerApiService } from '@app/services/api/customer-api.service';
import { FeatureSettingService } from '@app/services/feature-setting.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { contentAnimation } from '@assets/animations';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserService } from '@app/services/user.service';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { AuthenticationService } from '@app/services/authentication.service';
import { ProfilePicUrlPipe } from '@app/pipes/profile-pic-url.pipe';
import { RouteHelper } from '@app/helpers/route.helper';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';
import { RoleResourceModel, RolePermissionResourceModel } from '@app/resource-models/role.resource-model';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { UserViewModel } from '@app/viewmodels/user.viewmodel';
import { EntityType } from '@app/models/entity-type.enum';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { filter, finalize, takeUntil, last } from 'rxjs/operators';
import { AppConfig } from '@app/models/app-config';
import { PersonPopOverStatus, PopoverPersonComponent } from '@app/components/popover-person/popover-person.component';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';
import { PopoverRoleComponent } from '@app/components/popover-role/popover.role.component';
import { RolePermissionViewModel } from '@app/viewmodels/role.viewmodel';
import { PermissionService } from '@app/services/permission.service';
import { DefaultRoleName } from '@app/models/default-role-name.enum';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { UserStatus } from '@app/models';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';
import { OrganisationModel } from '@app/models/organisation.model';
import { CustomerViewModel } from '@app/viewmodels/customer.viewmodel';

/**
 * Export Detail user page component class.
 * This class manage displaying of users details.
 */
@Component({
    selector: 'app-detail-user',
    templateUrl: './detail-user.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-user.page.scss',
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailUserPage extends DetailPage implements OnInit, AfterViewInit, OnDestroy {
    public imageBaseUrl: string = '';

    public title: string;
    public segment: string;
    public profilePictureUrl: SafeUrl;
    public roles: Array<RoleResourceModel>;
    public userDetailsListItems: Array<DetailsListItem>;
    public userId: string;
    public entityTypes: typeof EntityType = EntityType;

    protected user: UserResourceModel;
    protected userViewModel: UserViewModel;
    protected defaultImgPath: string = 'assets/imgs/default-user.svg';
    public isLoadingRoles: boolean = false;
    public rolesErrorMessage: string;
    public isLoadingPermissions: boolean = false;
    public permissionsErrorMessage: string;
    public isLoadingEmails: boolean = false;
    public emailsErrorMessage: string;
    public hasAvailableRolesPromise: Promise<boolean>;

    public permission: typeof Permission = Permission;
    public subscription: SubscriptionLike;

    private tenantAlias: string;
    private subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    private defaultImgFilter: string
        = 'invert(52%) sepia(0%) saturate(0%) hue-rotate(153deg) brightness(88%) contrast(90%)';

    protected organisationId: string;
    protected performingUserOrganisationId: string;
    public shouldDisplayNavigationBarItems: boolean = false;
    public canModifyUser: boolean;
    public canGoBack: boolean;
    public permissions: Array<RolePermissionViewModel>;
    private canViewAdditionalPropertyValues: boolean = false;
    public hasEmailFeature: boolean;
    public permissionModel: PermissionDataModel;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;
    public customerTypeViewModel: typeof CustomerViewModel = CustomerViewModel;

    public constructor(
        public layoutManager: LayoutManagerService,
        public eventService: EventService,
        public elementRef: ElementRef,
        public injector: Injector,
        protected appConfigService: AppConfigService,
        protected navProxy: NavProxyService,
        protected userService: UserService,
        protected userApiService: UserApiService,
        protected customerApiService: CustomerApiService,
        protected featureSettingService: FeatureSettingService,
        protected authenticationService: AuthenticationService,
        protected sharedLoaderService: SharedLoaderService,
        protected sharedPopoverService: SharedPopoverService,
        protected sharedAlert: SharedAlertService,
        protected profilePicUrlPipe: ProfilePicUrlPipe,
        protected route: ActivatedRoute,
        protected routeHelper: RouteHelper,
        protected permissionService: PermissionService,
        private portalExtensionService: PortalExtensionsService,
    ) {
        super(eventService, elementRef, injector);

        this.subscriptions.push(this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantAlias = appConfig.portal.tenantAlias;
        }));
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.canGoBack = this.routeHelper.getPathSegments()[1] != 'user';
        this.canViewAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.ViewAdditionalPropertyValues);
        this.eventService.performingUserOrganisationSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((organisation: OrganisationModel) => {
                this.performingUserOrganisationId = organisation.id;
            });
        this.segment = this.route.snapshot.queryParamMap.get('previous') || 'Detail';
        this.route.params.pipe(takeUntil(this.destroyed)).subscribe((params: Params) => {
            this.organisationId = params['organisationId'];
            this.userId = params['userId'];
            this.load();
        });
        this.listenForUserUpdates();
    }

    public ngAfterViewInit(): void {
        this.loadAvailableRoles();
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async load(): Promise<void> {
        this.isLoading = true;

        let promises: Array<Promise<void>> = new Array<Promise<void>>();
        promises.push(this.loadUser());
        promises.push(this.loadUserRoles());
        await Promise.all(promises).then(
            () => {
                this.isLoading = false;
            },
            (err: any) => {
                this.isLoading = false;
                throw err;
            },
        )
            .finally(
                () => {
                    this.establishPermissions();
                    this.generatePopoverLinks();
                },
            );
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authenticationService,
                this.entityTypes.User,
                PageType.Display,
                this.segment,
            );
    }

    private generatePopoverLinks(): void {
        // Add portal page trigger actions
        this.actions = this.portalExtensionService.getActionButtonPopovers(this.portalPageTriggers);
        this.hasActionsIncludedInMenu = this.actions.filter((x: ActionButtonPopover) => x.includeInMenu).length > 0;
        this.initializeActionButtonList();
    }

    protected executePortalPageTrigger(trigger: PortalPageTriggerResourceModel): void {
        this.portalExtensionService.executePortalPageTrigger(
            trigger,
            this.entityTypes.User,
            PageType.Display,
            this.segment,
            this.userId,
        );
    }

    protected async loadUser(): Promise<void> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.routeHelper.getContextTenantAlias());
        if (this.organisationId) {
            params.set('organisation', this.organisationId);
        }
        return this.userApiService
            .getById(this.userId, params)
            .pipe(
                filter(
                    (user: UserResourceModel) => user != null && user != undefined),
                takeUntil(this.destroyed),
                last())
            .toPromise().then((user: UserResourceModel) => {
                this.user = user;
                this.userViewModel = new UserViewModel(user);
                this.permissionModel = {
                    organisationId: this.user.organisationId,
                    ownerUserId: this.user.ownerId,
                    customerId: this.user.customerId,
                };
                this.userDetailsListItems = this.userViewModel.createDetailsList(
                    this.userService,
                    this.navProxy,
                    this.sharedPopoverService,
                    this.permissionService,
                    this.authenticationService.isCustomer(),
                    this.canViewAdditionalPropertyValues);
                this.title = this.userViewModel.fullName;
                if (this.userViewModel.profilePictureId) {
                    this.profilePictureUrl =
                        this.profilePicUrlPipe.transform(this.userViewModel.profilePictureId, this.defaultImgPath);
                }
            },
            (err: any) => {
                // Needed to be paired with last() rxjs function, throws error when return is undefined
                // when destroying or canceling the api request
                if (err.name != 'EmptyError') {
                    throw err;
                }
            });
    }

    protected async loadUserRoles(): Promise<void> {
        if (this.isLoadingRoles) {
        // We're already loading roles, let's just wait for that one to complete
            return Promise.resolve();
        }

        this.isLoadingRoles = true;
        return this.userApiService.getUserRoles(this.userId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoadingRoles = false),
            )
            .toPromise().then((roles: Array<RoleResourceModel>) => {
                this.roles = roles;
            },
            (err: any) => {
                this.rolesErrorMessage = 'There was a problem loading the roles';
                throw err;
            });
    }

    public userDidSelectCustomer(customerViewModel: CustomerViewModel): void {
        this.navProxy.navigate(['customer', customerViewModel.id]);
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
        }
        switch (this.segment) {
            case 'Detail':
                break;
            case 'Picture':
                break;
            case 'Customers':
                break;
            case 'Permissions':
                this.loadPermissions();
                break;
            default:
                break;
        }

        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        this.navProxy.updateSegmentQueryStringParameter(
            'previous',
            this.segment != 'Details' ? this.segment : null,
        );
    }

    public async showMenu(event: any): Promise<void> {
        if (!this.userViewModel) {
            return;
        }
        this.flipMoreIcon = true;
        const popoverDismissAction = async (command: PopoverCommand): Promise<any> => {
            this.flipMoreIcon = false;
            if (!(command && command.data)) {
                // no option was selected.
                return;
            }
            switch (command.data.action.actionName) {
                case PersonPopOverStatus.Activate: {
                    await this.userService.sendActivation(this.userViewModel);
                    break;
                }
                case PersonPopOverStatus.ResendActivate: {
                    await this.userService.resendActivation(this.user);
                    break;
                }
                case PersonPopOverStatus.Disable: {
                    this.isLoading = true;
                    await this.userService.disableAccount(this.userViewModel).then((user: UserResourceModel) => {
                        this.userViewModel = new UserViewModel(user);
                        this.userDetailsListItems = this.userViewModel.createDetailsList(
                            this.userService,
                            this.navProxy,
                            this.sharedPopoverService,
                            this.permissionService,
                            this.authenticationService.isCustomer(),
                            this.canViewAdditionalPropertyValues);
                        this.isLoading = false;
                    },
                    (err: any) => {
                        this.isLoading = false;
                        this.errorMessage = 'There was a problem disabling the user account';
                        throw err;
                    });
                    break;
                }
                case PersonPopOverStatus.Enable: {
                    this.isLoading = true;
                    await this.userService.enableAccount(this.userViewModel).then((user: UserResourceModel) => {
                        this.userViewModel = new UserViewModel(user);
                        this.userDetailsListItems = this.userViewModel.createDetailsList(
                            this.userService,
                            this.navProxy,
                            this.sharedPopoverService,
                            this.permissionService,
                            this.authenticationService.isCustomer(),
                            this.canViewAdditionalPropertyValues);
                        this.isLoading = false;
                    },
                    (err: any) => {
                        this.isLoading = false;
                        this.errorMessage = 'There was a problem enabling the user account';
                        throw err;
                    });
                    break;
                }
                case PersonPopOverStatus.Edit: {
                    this.userDidTapEditButton();
                    break;
                }
                case PersonPopOverStatus.AssignRole: {
                    this.didSelectAssignRole();
                    break;
                }
                default:
                    if (command.data && command.data.action) {
                        this.executePortalPageTrigger(command.data.action.portalPageTrigger);
                    }
                    break;
            }
        };

        const isDefaultOptionsEnabled: boolean =
            this.canModifyUser && (this.segment == 'Detail' || this.segment == 'Picture');
        await this.sharedPopoverService.show(
            {
                component: PopoverPersonComponent,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                componentProps: {
                    isDefaultOptionsEnabled: isDefaultOptionsEnabled,
                    actions: this.actions,
                    segment: this.segment,
                    entityType: this.segment == 'Detail' ? EntityType.User : 'Profile Picture',
                    shouldShowPopOverEdit: this.canModifyUser
                        && (this.segment == 'Detail'
                            || this.segment == 'Picture'),
                    shouldShowPopOverNewStatus: this.userViewModel.status === UserStatus.New,
                    shouldShowPopOverResendStatus: this.userViewModel.status === UserStatus.Invited,
                    shouldShowPopOverDisableStatus: this.userViewModel.status !== UserStatus.Deactivated &&
                        this.userViewModel.status !== UserStatus.Disabled,
                    shouldShowPopOverEnableStatus: this.userViewModel.status === UserStatus.New ||
                        this.userViewModel.status === UserStatus.Deactivated ||
                        this.userViewModel.status === UserStatus.Disabled,
                    shouldShowPopOverAssignRole: this.canModifyUser && this.segment === 'Roles',
                },
                event: event,
            },
            'User option popover',
            popoverDismissAction);
    }

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultImgPath, this.defaultImgFilter);
    }

    protected listenForUserUpdates(): void {
        this.eventService.getEntityUpdatedSubject('User')
            .pipe(
                takeUntil(this.destroyed),
                filter((user: UserResourceModel) => user.id == this.user.id))
            .subscribe((user: UserResourceModel) => {
                this.userViewModel = new UserViewModel(user);
                this.load();
            });
    }

    public getSegmentCustomerList(params?: Map<string, string | Array<string>>,
    ): Observable<Array<CustomerResourceModel>> {
        return this.customerApiService.getCustomersByOwner(this.userId, params);
    }

    public async loadPermissions(): Promise<void> {
        if (this.isLoadingPermissions) {
        // We're already loading permissions, let's just wait for that one to complete
            return Promise.resolve();
        }
        this.isLoadingPermissions = true;
        return this.userApiService.getEffectivePermissions(this.userId, this.routeHelper.getContextTenantAlias())
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoadingPermissions = false))
            .toPromise().then((permissionResourceModels: Array<RolePermissionResourceModel>) => {
                let permissionViewModels: Array<RolePermissionViewModel> = new Array<RolePermissionViewModel>();
                for (let permissionResourceModel of permissionResourceModels) {
                    permissionViewModels.push(new RolePermissionViewModel(permissionResourceModel));
                }
                this.permissions = permissionViewModels;
            },
            (err: any) => {
                this.permissionsErrorMessage = 'There was a problem loading the permissions';
                throw err;
            });
    }

    public async didSelectAssignRole(): Promise<void> {
        this.sharedLoaderService.presentWait().then(() => {
            this.hasAvailableRolesPromise.then((hasAvailableRolesToBeAssigned: boolean) => {
                this.sharedLoaderService.dismiss();
                if (!hasAvailableRolesToBeAssigned) {
                    this.sharedAlert.showWithOk(
                        'All available roles assigned',
                        'This user has already been assigned all available roles.');
                    return;
                }
                let pathSegments: Array<string> = this.routeHelper.getPathSegments();
                pathSegments.push('role', 'assign');
                this.navProxy.navigateForward(pathSegments);
            });
        });
    }

    public loadAvailableRoles(): void {
        this.hasAvailableRolesPromise = new Promise(async (resolve: any): Promise<any> => {
            this.userApiService.getAvailableRoles(this.userId, this.routeHelper.getContextTenantAlias())
                .pipe(takeUntil(this.destroyed))
                .subscribe((availableRoles: Array<RoleResourceModel>) => {
                    resolve(availableRoles.length > 0);
                });
        });
    }

    public async userDidTapEditButton(): Promise<void> {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        if (this.segment === 'Detail') {
            pathSegments.push('edit');
        } else if (this.segment === 'Picture') {
            pathSegments.push('picture', 'upload');
        } else {
            throw new Error("The user tapped the edit button on a segment which it has not been implemented on.");
        }
        this.navProxy.navigate(pathSegments);
    }

    public goBack(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        if (this.organisationId) {
            pathSegments.pop();
            this.navProxy.navigate(pathSegments, { queryParams: { segment: 'Users' } });
        } else {
            pathSegments.push('list');
            this.navProxy.navigate(pathSegments, { queryParams: { segment: this.userViewModel.segment } });
        }
    }

    public async didSelectRoleActionButton(role: any): Promise<void> {
        if (!this.userViewModel) {
            return;
        }

        const popoverDismissAction = (command: PopoverCommand): void => {
            if (!(command && command.data)) {
                // nothing was selected
                return;
            }
            if (command.data.action.actionName === 'un-assign') {
                this.removeRoleFromUser(role);
            } else if (command.data && command.data.action) {
                this.executePortalPageTrigger(command.data.action.portalPageTrigger);
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverRoleComponent,
                cssClass: 'custom-popover',
                componentProps: {
                    actions: this.actions,
                },
                event,
            },
            'User role option popover',
            popoverDismissAction);
    }

    private async removeRoleFromUser(role: any): Promise<void> {
        await this.sharedLoaderService.presentWithDelay();
        const subscription: Subscription = this.userApiService.unassignRoleFromUser(
            this.userViewModel.id,
            role.id,
            this.routeHelper.getContextTenantAlias())
            .pipe(
                finalize(() => {
                    this.sharedLoaderService.dismiss();
                    this.loadAvailableRoles();
                    subscription.unsubscribe();
                }))
            .subscribe(
                () => {
                    this.roles = this.roles = this.roles.filter((r: RoleResourceModel) => r.id !== role.id);
                    this.sharedAlert.showToast(
                        `${role.name} role was un-assigned from user ${this.userViewModel.fullName}`);
                    this.eventService.getEntityUpdatedSubject('User').next(this.user);
                });
    }

    private establishPermissions(): void {
        if (this.user) {
            this.canModifyUser = this.permissionService.hasElevatedPermissions(
                Permission.ManageUsers,
                this.user.organisationId,
                this.user.ownerId,
                this.user.customerId);

            let hasTenantAdminRole: boolean = this.roles
                .map((x: RoleResourceModel) => x.name)
                .includes(DefaultRoleName.TenantAdmin);

            if (hasTenantAdminRole) {
                this.canModifyUser = this.canModifyUser
                    && this.permissionService.hasPermission(Permission.ManageTenantAdminUsers);
            }
        }
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (this.canModifyUser && this.segment === 'Roles') {
            actionButtonList.push(ActionButton.createActionButton(
                "Assign",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Assign Role",
                true,
                (): Promise<void> => {
                    return this.didSelectAssignRole();
                },
            ));
        }

        let entityType: string = this.segment == 'Detail' ? "User" : "Profile Picture";
        if (this.canModifyUser && (this.segment == 'Detail' || this.segment == 'Picture')) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                `Edit ${entityType}`,
                true,
                (): Promise<void> => {
                    return this.userDidTapEditButton();
                },
            ));
        }

        for (let action of this.actions) {
            if (action.actionButtonLabel) {
                actionButtonList.push(ActionButton.createActionButton(
                    action.actionButtonLabel ? action.actionButtonLabel : action.actionName,
                    action.actionIcon,
                    IconLibrary.IonicV4,
                    action.actionButtonPrimary,
                    action.actionName,
                    action.actionButtonLabel ? true : false,
                    (): Promise<void> => {
                        return this.portalExtensionService.executePortalPageTrigger(
                            action.portalPageTrigger,
                            this.entityTypes.User,
                            PageType.Display,
                            this.segment,
                            this.userId);
                    },
                ));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
