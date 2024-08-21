import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { RoleResourceModel } from '@app/resource-models/role.resource-model';
import { Permission } from '@app/helpers/permissions.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { RoleApiService } from '@app/services/api/role-api.service';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { RolePermissionViewModel, RoleViewModel } from '@app/viewmodels/role.viewmodel';
import { contentAnimation } from '@assets/animations';
import { scrollbarStyle } from '@assets/scrollbar';
import { Subject, SubscriptionLike } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { DetailsListItem } from '@app/models/details-list/details-list-item';
import { AuthenticationService } from '@app/services/authentication.service';
import { ActionButton } from '@app/models/action-button';
import { IconLibrary } from '@app/models/icon-library.enum';
import { ActionButtonHelper } from '@app/helpers/action-button.helper';

/**
 * Export detail role permission page component class.
 * This class manage displaying the role permission details.
 */
@Component({
    selector: 'app-detail-role-permission',
    templateUrl: './detail-role-permission.page.html',
    animations: [contentAnimation],
    styleUrls: [
        '../../../../assets/css/scrollbar-form-no-header.css',
        '../../../../assets/css/form-toolbar.scss',
        './detail-role-permission.page.scss',
    ],
    styles: [scrollbarStyle],
})
export class DetailRolePermissionPage extends DetailPage implements OnInit, OnDestroy {
    public detailSubscription: SubscriptionLike;
    public permission: typeof Permission = Permission;
    private roleId: string;
    public role: RoleViewModel;
    public rolePermission: RolePermissionViewModel;
    public detailsListItems: Array<DetailsListItem>;
    public actionButtonList: Array<ActionButton>;

    public constructor(
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        protected eventService: EventService,
        private sharedAlertService: SharedAlertService,
        private sharedLoaderService: SharedLoaderService,
        private roleApiService: RoleApiService,
        public layoutManager: LayoutManagerService,
        elementRef: ElementRef,
        injector: Injector,
        protected authService: AuthenticationService,
    ) {
        super(eventService, elementRef, injector);
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.roleId = this.routeHelper.getParam('roleId');
        this.load();
        this.initializeActionButtonList();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    private load(): void {
        this.isLoading = true;
        const permissionType: string = this.routeHelper.getParam('permissionType');
        this.roleApiService.getById(this.roleId)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            )
            .subscribe(
                (data: RoleResourceModel) => {
                    this.role = new RoleViewModel(data);
                    this.rolePermission = this.role.permissions.find(
                        (item: RolePermissionViewModel) => item.type === permissionType,
                    );
                    if (this.rolePermission) {
                        this.rolePermission.description = this.authService.isMutualTenant() ?
                            this.rolePermission.description.replace('Policies', 'Protections') :
                            this.rolePermission.description;
                    }

                    this.detailsListItems = this.rolePermission.createDetailsList();
                },
                (err: any) => {
                    this.errorMessage = 'There was a problem loading the permission details.';
                },
            );
    }

    public userDidTapEditButton(): void {
        this.unsubscribe();
        this.navProxy.navigateForward(['role', this.role.id, 'permission', this.rolePermission.type, 'edit']);
    }

    public async userDidTapDeleteButton(): Promise<void> {
        if (!(await this.confirmDelete())) {
            return;
        }

        await this.sharedLoaderService.presentWait();
        this.roleApiService.retractPermission(this.role.id, this.rolePermission.type)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(() => {
                this.sharedAlertService.showToast(`${this.authService.isMutualTenant() ?
                    this.rolePermission.description.replace('Policies', 'Protections') :
                    this.rolePermission.description} permission was removed from ${this.role.name} role`);
                this.returnToPrevious();
            });
    }

    private confirmDelete(): Promise<boolean> {
        return new Promise((resolve: any, reject: any): any => {
            this.sharedAlertService.showWithActionHandler({
                header: 'Confirm delete permission',
                subHeader: 'Are you sure you want to delete this permission? This action cannot be undone.',
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

    public returnToPrevious(): void {
        this.unsubscribe();
        this.navProxy.navigateBack(['role', this.roleId], true, { queryParams: { segment: 'Permissions' } });
    }

    private unsubscribe(): void {
        if (this.detailSubscription) {
            this.detailSubscription.unsubscribe();
        }
    }

    private initializeActionButtonList(): void {
        let actionButtonList: Array<ActionButton> = [];
        actionButtonList.push(ActionButton.createActionButton(
            "Edit",
            "pencil",
            IconLibrary.AngularMaterial,
            false,
            "Edit Role Permission",
            true,
            (): void => {
                return this.userDidTapEditButton();
            },
        ));

        actionButtonList.push(ActionButton.createActionButton(
            "Delete",
            "trash",
            IconLibrary.IonicV4,
            false,
            "Delete Role Permission",
            true,
            (): Promise<void> => {
                return this.userDidTapDeleteButton();
            },
        ));

        this.actionButtonList = ActionButtonHelper.sortActionButtons(actionButtonList);
    }
}
