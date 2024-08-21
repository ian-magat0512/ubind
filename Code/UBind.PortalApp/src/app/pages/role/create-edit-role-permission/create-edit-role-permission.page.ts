import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { RoleResourceModel, RolePermissionResourceModel } from '@app/resource-models/role.resource-model';
import { FormHelper } from '@app/helpers/form.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { RoleApiService } from '@app/services/api/role-api.service';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { RoleType } from '@app/models/role-type.enum';
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { RoleViewModel } from '@app/viewmodels/role.viewmodel';
import { AuthenticationService } from '@app/services/authentication.service';
import { UserTypePathHelper } from '@app/helpers/user-type-path.helper';

/**
 * Export create/edit role permission page component class.
 * This class manage creation and editing role permissions.
 */
@Component({
    selector: 'app-create-edit-role-permission',
    templateUrl: './create-edit-role-permission.page.html',
    styleUrls: [
        '../../../../assets/css/form-toolbar.scss',
        './create-edit-role-permission.page.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditRolePermissionPage extends CreateEditPage<RoleResourceModel> implements OnInit, OnDestroy {
    public isEdit: boolean;
    public subjectName: string = "Permission";
    public availablePermissionsForRole: Array<RolePermissionResourceModel> = [];
    public permission: any;
    public description: string;
    private tenantPermissions: Array<RolePermissionResourceModel>;

    public constructor(
        private sharedAlertService: SharedAlertService,
        protected sharedLoaderService: SharedLoaderService,
        public navProxy: NavProxyService,
        protected formBuilder: FormBuilder,
        public formHelper: FormHelper,
        protected roleApiService: RoleApiService,
        protected eventService: EventService,
        protected router: Router,
        public layoutManager: LayoutManagerService,
        private routeHelper: RouteHelper,
        elementRef: ElementRef,
        injector: Injector,
        protected authService: AuthenticationService,
        private userPath: UserTypePathHelper,
    ) {
        super(eventService, elementRef, injector, formHelper);
        this.form = this.buildForm();
    }

    public ngOnInit(): void {
        this.permission = this.routeHelper.getParam('permissionType');
        this.isEdit = this.permission != null;
        super.ngOnInit();
        this.destroyed = new Subject<void>();
        this.subjectName = "Permission";
        if (!this.isEdit) {
            this.detailList = RoleViewModel.createDetailsListForPermissionEdit();
        }
        this.load();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    protected buildForm(): FormGroup {
        return this.formBuilder.group({
            type: ['', [Validators.required]],
            disabled: false,
        });
    }

    public load(): void {
        this.isLoading = true;
        const roleId: string = this.routeHelper.getParam('roleId');
        if (roleId) {
            this.roleApiService.getById(roleId)
                .pipe(takeUntil(this.destroyed))
                .subscribe(
                    (role: RoleResourceModel) => {
                        this.model = role;

                        this.loadAllPermissions(this.model.type);
                    }, (err: any) => {
                        this.errorMessage = 'There was a problem loading the role.';
                    });
        }
    }

    private loadAllPermissions(roleType: RoleType): void {
        this.roleApiService.getAllPermissions(roleType)
            .pipe(takeUntil(this.destroyed),
                finalize(() => this.isLoading = false))
            .subscribe(async (permissions: Array<RolePermissionResourceModel>) => {
                this.tenantPermissions = permissions;
                this.availablePermissionsForRole =
                    this.tenantPermissions.filter((item: RolePermissionResourceModel) => {
                        if (this.isEdit && item.type !== this.permission) {
                            return this.model.permissions.filter(
                                (inner: RolePermissionResourceModel) => inner.type === item.type).length === 0;
                        } else if (this.isEdit) {
                        // Let's make sure to include the current permission in the available selection
                            this.description = item.description;
                            return true;
                        }

                        // Create mode
                        return this.model.permissions.filter(
                            (inner: RolePermissionResourceModel) => inner.type === item.type).length === 0;
                    });

                if (this.availablePermissionsForRole.length === 0) {
                    await this.sharedAlertService.showWithActionHandler({
                        header: 'All available permissions assigned',
                        subHeader: 'This role has already been assigned all available permissions.',
                        buttons: [{ text: 'OK' }],
                    });
                    this.returnToPrevious();
                } else {
                    this.applyOptionsToField(this.availablePermissionsForRole);
                }

                if (this.isEdit) {
                    this.detailList = RoleViewModel.createDetailsListForPermissionEdit();
                    this.form.setValue({
                        type: this.permission.toString(),
                        disabled: false,
                    });
                }
            }, (err: any) => {
                this.errorMessage = 'There was a problem loading the permissions.';
            });
    }

    public update(value: any): void {
        this.roleApiService.updatePermission(this.model.id, this.permission, value.type)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(() => {
                const selectedPermission: RolePermissionResourceModel = this.tenantPermissions.find(
                    (i: RolePermissionResourceModel) => i.type === value.type);
                this.sharedAlertService.showToast(`${this.authService.isMutualTenant() ?
                    selectedPermission.description.replace('Policies', 'Protections') :
                    selectedPermission.description} permission was updated on ${this.model.name} role`);
                this.returnToPermissionDetailPage(selectedPermission.type);
            });
    }

    public create(value: any): void {
        this.roleApiService.assignPermission(this.model.id, value.type)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(() => {
                const selectedPermission: RolePermissionResourceModel = this.tenantPermissions.find(
                    (i: RolePermissionResourceModel) => i.type === value.type);
                this.sharedAlertService.showToast(`${this.authService.isMutualTenant() ?
                    selectedPermission.description.replace('Policies', 'Protections') :
                    selectedPermission.description} permission was added to ${this.model.name} role`);
                this.returnToPermissionDetailPage(selectedPermission.type);
            });
    }

    public returnToPrevious(): void {
        this.navProxy.navigateBack(['role', this.model.id, 'segment', 'Permissions']);
    }

    public returnToPermissionDetailPage(permission: string): void {
        this.navProxy.navigate([this.userPath.role, this.model.id, 'permission', permission]);
    }

    private applyOptionsToField(rolePermissions: Array<RolePermissionResourceModel>): void {
        let items: Array<any> = [];
        rolePermissions.forEach((permission: RolePermissionResourceModel) => {
            items.push({
                label: this.authService.isMutualTenant() ?
                    permission.description.replace('Policies', 'Protections') : permission.description,
                value: permission.type,
            });
        });

        this.fieldOptions.push({ name: "type", options: items, type: "option" });
    }
}
