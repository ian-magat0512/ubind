import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { Permission, PermissionDataModel } from '@app/helpers/permissions.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { RoleApiService } from '@app/services/api/role-api.service';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { RoleViewModel } from '@app/viewmodels/role.viewmodel';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { RoleResourceModel } from '@app/resource-models/role.resource-model';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { UserViewModel } from '@app/viewmodels/user.viewmodel';
import { HttpErrorResponse } from '@angular/common/http';
import { UserApiService } from '@app/services/api/user-api.service';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { EntityType } from '@app/models/entity-type.enum';
import { PortalExtensionsService } from '@app/services/portal-extensions.service';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { ActionButton } from '@app/models/action-button';
import { PermissionService } from '@app/services/permission.service';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { PageType } from '@app/models/page-type.enum';

/**
 * Export detail role page component class.
 * This class manage displaying the role details.
 */
@Component({
    selector: 'app-detail-role',
    templateUrl: './detail-role.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-role.page.scss',
        '../../../../assets/css/scrollbar-div.css',
        '../../../../assets/css/scrollbar-detail.css',
    ],
    styles: [scrollbarStyle],
})
export class DetailRolePage extends DetailPage implements OnInit, OnDestroy {
    public role: RoleViewModel;
    private roleResourceModel: RoleResourceModel;
    public segment: string = 'Details';
    public permission: typeof Permission = Permission;
    public isMutual: boolean;
    private roleId: string;
    public detailsListItems: Array<DetailsListItem>;
    public isLoadingUsers: boolean = true;
    public usersErrorMessage: string;
    public canShowUsers: boolean = false;
    public users: Array<UserResourceModel>;
    public userViewModels: Array<UserViewModel>;
    public defaultUserImgPath: string = 'assets/imgs/default-user.svg';
    public defaultUserImgFilter: string
        = 'invert(52%) sepia(0%) saturate(0%) hue-rotate(153deg) brightness(88%) contrast(90%)';
    public permissionModel: PermissionDataModel;

    private entityTypes: typeof EntityType = EntityType;
    protected portalPageTriggers: Array<PortalPageTriggerResourceModel>;
    protected actions: Array<ActionButtonPopover> = [];
    protected hasActionsIncludedInMenu: boolean = false;
    public actionButtonList: Array<ActionButton>;
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        protected eventService: EventService,
        private sharedAlertService: SharedAlertService,
        private sharedLoaderService: SharedLoaderService,
        private roleApiService: RoleApiService,
        public layoutManager: LayoutManagerService,
        private userApiService: UserApiService,
        elementRef: ElementRef,
        injector: Injector,
        protected authService: AuthenticationService,
        private portalExtensionService: PortalExtensionsService,
        protected sharedPopoverService: SharedPopoverService,
        protected permissionService: PermissionService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.roleId = this.routeHelper.getParam('roleId');
        this.segment = this.routeHelper.getParam('segment') || this.segment;
        let loadRolePromise: Promise<void> = this.loadRole();
        let preparePortalExtensionsPromise: Promise<void> = this.preparePortalExtensions();
        Promise.all([loadRolePromise, preparePortalExtensionsPromise]).then(() => {
            this.initializeDetailsListItems();

            if (this.segment != 'Details') {
                this.segmentChanged(null);
            }

            this.generatePopoverLinks();
        });
        this.eventService.getEntityUpdatedSubject('Role').subscribe((role: RoleResourceModel) => {
            if (role.id == this.roleResourceModel.id) {
                this.loadRole();
            }
        });
        this.isMutual = this.authService.isMutualTenant();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    private async loadRole(): Promise<void> {
        this.isLoading = true;
        return this.roleApiService.getById(this.roleId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false))
            .toPromise().then((data: RoleResourceModel) => {
                if (!data) {
                    return;
                }

                this.roleResourceModel = data;
                this.permissionModel = {
                    organisationId: this.roleResourceModel.organisationId,
                    ownerUserId: null,
                    customerId: null,
                };
                this.role = new RoleViewModel(data);
                this.canShowUsers = data.name != 'Customer';
            }, (err: any) => {
                this.errorMessage = 'There was a problem loading the role';
                throw err;
            });
    }

    private async preparePortalExtensions(): Promise<void> {
        this.portalPageTriggers =
            await this.portalExtensionService.getEnabledPortalPageTriggers(
                this.authService,
                this.entityTypes.Role,
                PageType.Display,
                this.segment);
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
            this.entityTypes.Role,
            PageType.Display,
            this.segment,
            this.roleId,
        );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): void => {
            this.flipMoreIcon = false;
            if (command && command.data && command.data.action && command.data.action.portalPageTrigger) {
                this.portalExtensionService.executePortalPageTrigger(
                    command.data.action.portalPageTrigger,
                    this.entityTypes.Role,
                    PageType.Display,
                    this.segment,
                    this.roleId,
                );
            }
        };

        await this.sharedPopoverService.show(
            {
                component: PopoverViewComponent,
                cssClass: 'custom-popover more-button-top-popover-positioning',
                componentProps: {
                    actions: this.actions,
                },
                event: event,
            },
            'Role option popover',
            popoverDismissAction);
    }

    private initializeDetailsListItems(): void {
        if (this.role) {
            this.detailsListItems = this.role.createDetailsList();
        }
    }

    public segmentChanged($event: any): void {
        if ($event != null && $event.detail.value != this.segment) {
            this.segment = $event.detail.value;
        }

        switch (this.segment) {
            case 'Users':
                this.loadUsers();
                break;
            default:
        }
        this.preparePortalExtensions().then(() => this.generatePopoverLinks());
        this.navProxy.updateSegmentQueryStringParameter(
            'segment',
            this.segment != 'Details' ? this.segment : null);
    }

    private async loadUsers(): Promise<void> {
        this.isLoadingUsers = true;
        this.userApiService.getUsersWithRole(this.role.name)
            .pipe(
                finalize(() => this.isLoadingUsers = false),
                takeUntil(this.destroyed))
            .subscribe(
                (users: Array<UserResourceModel>) => {
                    let userViewModels: Array<UserViewModel> = new Array<UserViewModel>();
                    for (let user of users) {
                        userViewModels.push(new UserViewModel(user));
                    }
                    this.userViewModels = userViewModels;
                    this.users = users;
                },
                (err: HttpErrorResponse) => {
                    this.usersErrorMessage = 'There was a problem loading the users for this role';
                });
    }

    public goBack(): void {
        this.navProxy.navigateBack(['role', 'list']);
    }

    public userDidTapEditButton(): void {
        this.navProxy.navigateForward(['role', this.role.id, 'edit']);
    }

    public async userDidTapDeleteButton(): Promise<void> {
        if (!(await this.confirmDelete())) {
            return;
        }

        await this.sharedLoaderService.presentWait();
        this.roleApiService.delete(this.role.id)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(() => {
                this.roleResourceModel.isDeleted = true;
                this.eventService.getEntityDeletedSubject('Role').next(this.roleResourceModel);
                this.sharedAlertService.showToast(`${this.role.name} role was deleted`);
                this.goBack();
            });
    }

    private confirmDelete(): Promise<boolean> {
        return new Promise((resolve: any, reject: any): any => {
            this.sharedAlertService.showWithActionHandler({
                header: 'Confirm delete role',
                subHeader: 'Are you sure you want to delete this role? This action cannot be undone.',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            resolve(false);
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            resolve(true);
                        },
                    },
                ],
            });
        });
    }

    public userDidSelectItem(user: UserViewModel): void {
        if (user) {
            this.navProxy.navigateForward(['user', user.id]);
        }
    }

    public userDidTapAddButton(): void {
        switch (this.segment) {
            case 'Permissions':
                this.navProxy.navigateForward(['role', this.roleId, 'permission']);
                break;
            default:
        }
    }

    public userDidTapListItem(item: string): void {
        switch (this.segment) {
            case 'Permissions':
                this.navProxy.navigateForward(['role', this.roleId, 'permission', item]);
                break;
            default:
        }
    }

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultUserImgPath, this.defaultUserImgFilter);
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        if (this.permissionModel == null) {
            return;
        }

        const hasManageRole: boolean = this.permissionService.hasElevatedPermissionsViaModel(
            Permission.ManageRoles,
            this.permissionModel);

        if (hasManageRole
            && this.segment == 'Details'
            && this.role
            && this.role.isRenamable) {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                "Edit Role",
                true,
                (): void => {
                    return this.userDidTapEditButton();
                }));

            actionButtonList.push(ActionButton.createActionButton(
                "Delete",
                "trash",
                IconLibrary.IonicV4,
                false,
                "Delete Role",
                true,
                (): Promise<void> => {
                    return this.userDidTapDeleteButton();
                }));
        }

        if (hasManageRole
            && this.segment == 'Permissions'
            && this.role
            && this.role.arePermissionsEditable) {
            actionButtonList.push(ActionButton.createActionButton(
                "Add Permission",
                "plus",
                IconLibrary.AngularMaterial,
                false,
                "Add Permission",
                true,
                (): void => {
                    return this.userDidTapAddButton();
                }));
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
                            this.entityTypes.Role,
                            PageType.Display,
                            this.segment,
                            this.roleId);
                    }));
            }
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
