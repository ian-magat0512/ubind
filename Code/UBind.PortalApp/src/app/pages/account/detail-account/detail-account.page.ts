import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SafeUrl } from '@angular/platform-browser';
import { Subject, SubscriptionLike } from 'rxjs';
import { contentAnimation } from '@assets/animations';
import { AccountApiService } from '@app/services/api/account-api.service';
import { UserViewModel } from '@app/viewmodels/user.viewmodel';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission } from '@app/helpers/permissions.helper';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { EventService } from '@app/services/event.service';
import { filter, finalize, takeUntil } from 'rxjs/operators';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { IonicHelper } from '@app/helpers/ionic.helper';
import { DefaultImgHelper } from '@app/helpers/default-img-helper';
import { AuthenticationService } from '@app/services/authentication.service';
import { HttpErrorResponse } from '@angular/common/http';
import { RoleResourceModel, RolePermissionResourceModel } from '@app/resource-models/role.resource-model';
import { UserApiService } from '@app/services/api/user-api.service';
import { RolePermissionViewModel } from '@app/viewmodels/role.viewmodel';
import { AdditionalPropertyDefinitionService } from '@app/services/additional-property-definition.service';
import { PermissionService } from '@app/services/permission.service';
import { ActionButton } from '@app/models/action-button';
import { PopoverViewComponent } from '@app/components/popover-view/popover-view.component';
import { SharedPopoverService } from '@app/services/shared-popover.service';
import { ActionButtonPopover } from '@app/models/action-button-popover';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';
import { PopoverCommand } from '@app/models/popover-command';
import { UserService } from '@app/services/user.service';

/**
 * Export detail account page component class
 * TODO: Write a better class header: details of the account page.
 */
@Component({
    selector: 'app-detail-account',
    templateUrl: './detail-account.page.html',
    animations: [contentAnimation],
    styleUrls: [
        './detail-account.page.css',
    ],
    styles: [
        scrollbarStyle,
    ],
})
export class DetailAccountPage implements OnInit, AfterViewInit, OnDestroy {
    public title: string;
    public userId: string;
    protected user: UserResourceModel;
    protected userViewModel: UserViewModel;
    public roles: Array<RoleResourceModel>;
    public returnValue: any;
    public profilePictureUrl: SafeUrl;
    protected subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();
    public accountSubscription: SubscriptionLike;
    public segment: string = 'Detail';
    public permission: typeof Permission = Permission;
    public isLoading: boolean = true;
    public errorMessage: string;
    public accountDetailsListItems: Array<DetailsListItem>;
    private canViewAdditionalPropertyValues: boolean = false;
    public defaultImgPath: string = 'assets/imgs/default-user.svg';
    public isCustomer: boolean;
    public isLoadingPermissions: boolean = false;
    public permissionsErrorMessage: string;
    private destroyed: Subject<void>;
    public permissions: Array<RolePermissionViewModel>;
    public actionButtonList: Array<ActionButton>;
    public actions: Array<ActionButtonPopover> = [];
    public flipMoreIcon: boolean = false;
    public iconLibrary: typeof IconLibrary = IconLibrary;

    public constructor(
        public navProxy: NavProxyService,
        private route: ActivatedRoute,
        private accountApiService: AccountApiService,
        protected userApiService: UserApiService,
        private eventService: EventService,
        public layoutManager: LayoutManagerService,
        public userPath: UserTypePathHelper,
        public authenticationService: AuthenticationService,
        protected additionalPropertyDefinitionService: AdditionalPropertyDefinitionService,
        private permissionService: PermissionService,
        protected sharedPopoverService: SharedPopoverService,
        protected userService: UserService,
    ) {
        this.title = this.route.snapshot.paramMap.get('title') || 'My Account';
    }

    public ngAfterViewInit(): void {
        IonicHelper.initIonSegmentButtons();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.isCustomer = this.authenticationService.isCustomer();
        this.userId = this.authenticationService.userId;
        this.canViewAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.ViewAdditionalPropertyValues);
        this.load();
        this.initializeActionButtonList();
        this.eventService.getEntityUpdatedSubject('User')
            .pipe(
                filter((user: UserResourceModel) => user.id == this.userId),
                takeUntil(this.destroyed),
            )
            .subscribe((user: UserResourceModel) => {
                if (this.layoutManager.splitPaneEnabled) {
                    this.load();
                } else {
                    this.user = user;
                    this.userViewModel = new UserViewModel(user);
                    this.initializeAccountDetailsListItems();
                }
            });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async load(): Promise<void> {
        this.isLoading = true;
        let promises: Array<Promise<void>> = new Array<Promise<void>>();
        promises.push(this.loadAccount());
        await Promise.all(promises).then(
            () => {
                this.isLoading = false;
            },
            (err: any) => {
                this.isLoading = false;
                throw err;
            },
        );
    }

    public async loadAccount(): Promise<void> {
        return this.accountApiService.get()
            .pipe(
                takeUntil(this.destroyed),
            )
            .toPromise()
            .then(
                (user: UserResourceModel) => {
                    this.user = user;
                    this.userViewModel = new UserViewModel(user);
                    this.initializeAccountDetailsListItems();
                    this.generatePopoverLinks();
                },
                (err: HttpErrorResponse) => {
                    this.errorMessage = "There was a problem loading your account details";
                    throw err;
                },
            );
    }

    protected async loadUserRoles(): Promise<void> {
        return this.accountApiService
            .getUserRoles()
            .pipe(takeUntil(this.destroyed))
            .toPromise()
            .then((roles: Array<RoleResourceModel>) => {
                this.roles = roles;
            });
    }

    private initializeAccountDetailsListItems(): void {
        this.accountDetailsListItems = this.userViewModel.createDetailsList(
            this.userService,
            this.navProxy,
            this.sharedPopoverService,
            this.permissionService,
            this.authenticationService.isCustomer(),
            this.canViewAdditionalPropertyValues,
            true,
        );
    }

    public userDidTapEditButton(): void {
        if (this.segment === 'Detail') {
            this.navProxy.navigateForward([this.userPath.account, 'edit']);
        } else {
            this.navProxy.navigateForward([this.userPath.account, 'picture', 'upload']);
        }
    }

    public setDefaultImg(event: any): void {
        DefaultImgHelper.setImageSrcAndFilter(event.target, this.defaultImgPath, null);
    }

    public segmentChanged($event: any): void {
        if ($event.detail.value != this.segment) {
            this.segment = $event.detail.value;
            this.generatePopoverLinks();
            this.initializeActionButtonList();
        }
        switch (this.segment) {
            case 'Roles':
                this.loadUserRoles();
                break;
            case 'Detail':
                break;
            case 'Picture':
                break;
            case 'Permissions':
                this.loadPermissions();
                break;
            default:
                break;
        }
    }

    public async loadPermissions(): Promise<void> {
        if (this.isLoadingPermissions) {
        // we're already loading permissions, let's just wait for that one to complete.                
            return Promise.resolve();
        }
        this.isLoadingPermissions = true;
        return this.accountApiService.getUserPermissions()
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoadingPermissions = false),
            )
            .toPromise().then(
                (permissionResourceModels: Array<RolePermissionResourceModel>) => {
                    let permissionViewModels: Array<RolePermissionViewModel> = new Array<RolePermissionViewModel>();
                    for (let permissionResourceModel of permissionResourceModels) {
                        permissionViewModels.push(new RolePermissionViewModel(permissionResourceModel));
                    }
                    this.permissions = permissionViewModels;
                },
                (err: any) => {
                    this.permissionsErrorMessage = 'There was a problem loading the permissions';
                    throw err;
                },
            );
    }

    public async presentPopover(event: any): Promise<void> {
        this.flipMoreIcon = true;
        const popoverDismissAction = (command: PopoverCommand): any => {
            this.flipMoreIcon = false;
            if (!(command && command.data && command.data.action)) {
                // nothing was selected
                return;
            }
            const actionName: string = command.data.action.actionName;
            if (actionName == 'Edit Account' || actionName == 'Edit Profile Picture') {
                this.userDidTapEditButton();
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
            'My Account option popover',
            popoverDismissAction,
        );
    }

    private generatePopoverLinks(): void {
        this.actions = [];
        this.actions.push({
            actionName: this.segment == 'Detail' ? 'Edit Account' : 'Edit Profile Picture',
            actionIcon: "pencil",
            iconLibrary: IconLibrary.AngularMaterial,
            actionButtonLabel: "",
            actionButtonPrimary: false,
            includeInMenu: true,
        });
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        let entityType: string = this.segment == 'Detail' ? "Account" : "Profile Picture";
        if (this.segment == 'Detail' || this.segment == 'Picture') {
            actionButtonList.push(ActionButton.createActionButton(
                "Edit",
                "pencil",
                IconLibrary.AngularMaterial,
                false,
                `Edit ${entityType}`,
                true,
                (): void => {
                    return this.userDidTapEditButton();
                },
            ));
        }

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }

}
