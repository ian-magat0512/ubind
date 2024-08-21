import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { RoleResourceModel } from '@app/resource-models/role.resource-model';
import { FormHelper } from '@app/helpers/form.helper';
import { RouteHelper } from '@app/helpers/route.helper';
import { RoleApiService } from '@app/services/api/role-api.service';
import { EventService } from '@app/services/event.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { RoleViewModel } from '@app/viewmodels/role.viewmodel';
import { scrollbarStyle } from '@assets/scrollbar';
import { debounceTime, finalize, filter, takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { CreateEditPage } from "@app/pages/master-create/create-edit.page";
import { AuthenticationService } from '@app/services/authentication.service';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';

/**
 * Export create/edit role page component class.
 * TODO: Write a better class header: creation and editing of role.
 */
@Component({
    selector: 'app-create-edit-role',
    templateUrl: './create-edit-role.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditRolePage extends CreateEditPage<RoleViewModel> implements OnInit, OnDestroy {
    public isEdit: boolean;
    public subjectName: string = "Role";
    public roleId: string;

    public constructor(
        protected formBuilder: FormBuilder,
        public formHelper: FormHelper,
        protected navProxy: NavProxyService,
        private sharedAlertService: SharedAlertService,
        protected sharedLoaderService: SharedLoaderService,
        public layoutManager: LayoutManagerService,
        protected routeHelper: RouteHelper,
        protected router: Router,
        public eventService: EventService,
        public elementRef: ElementRef,
        public injector: Injector,
        public roleApiService: RoleApiService,
        protected authService: AuthenticationService,
    ) {
        super(eventService, elementRef, injector, formHelper);
        this.form = this.buildForm();
    }

    public ngOnInit(): void {
        this.roleId = this.routeHelper.getParam('roleId');
        this.isEdit = this.roleId != null;
        super.ngOnInit();
        this.destroyed = new Subject<void>();
        this.isLoading = this.isEdit;
        if (this.isEdit) {
            this.load();
        } else {
            this.detailList = RoleViewModel.createDetailsListForEdit();
        }
        this.watchForNameChangesAndCheckIfUnique();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    protected buildForm(): FormGroup {
        this.detailList = RoleViewModel.createDetailsListForEdit();
        let controls: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        const form: FormGroup = this.formBuilder.group(controls);

        return form;
    }

    protected watchForNameChangesAndCheckIfUnique(): void {
        const control: AbstractControl = this.form.get('name');
        control.valueChanges
            .pipe(
                debounceTime(500),
                takeUntil(this.destroyed),
                filter((value: any) => value != null),
            )
            .subscribe((value: any) => {
                if (control.valid) {
                    const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
                    params.set('names', [value.trim()]);
                    this.roleApiService.getList(params)
                        .subscribe((data: Array<RoleResourceModel>) => {
                            if (data.length === 0) {
                                return;
                            }
                            if (this.isEdit && data.filter((role: RoleResourceModel) =>
                                role.id !== this.model.id).length === 0) {
                                return null;
                            }

                            control.markAsTouched({ onlySelf: true });
                            control.setErrors({ uniqueness: true });
                        });
                }
            });
    }

    public async load(): Promise<void> {
        await this.sharedLoaderService.presentWait();
        this.roleApiService.getById(this.roleId)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                this.isLoading = false;
            }))
            .subscribe(
                (role: RoleResourceModel) => {
                    this.model = new RoleViewModel(role);
                    this.detailList = RoleViewModel.createDetailsListForEdit();
                    this.form.controls.name.setValue(this.authService.isMutualTenant() ?
                        role.name.replace('Policies', 'Protections') : role.name);
                    this.form.controls.description.setValue(this.authService.isMutualTenant() ?
                        role.description.replace('Policies', 'Protections') : role.description);
                },
            );
    }

    public async create(value: any): Promise<void> {
        const role: RoleResourceModel = {
            id: '',
            name: value.name.trim(),
            description: value.description.trim(),
            type: null,
            isFixed: false,
            isPermanentRole: false,
            isDeletable: true,
            isRenamable: true,
            arePermissionsEditable: true,
            permissions: null,
            isDeleted: false,
            createdDateTime: null,
            lastModifiedDateTime: null,
            organisationId: this.authService.userOrganisationId,
            tenantId: this.authService.tenantId,
        };

        await this.sharedLoaderService.presentWait();
        this.roleApiService.create(role)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe((newRole: RoleResourceModel) => {
                this.eventService.getEntityCreatedSubject('Role').next(newRole);
                this.form.controls.name.reset();
                this.form.controls.description.reset();
                this.sharedAlertService.showToast(`${newRole.name} role was created`);
                this.model = new RoleViewModel(newRole);
                this.returnToDetailPage();
            });
    }

    public async update(value: any): Promise<void> {
        const role: RoleResourceModel = {
            id: this.model.id,
            name: value.name.trim(),
            description: this.authService.isMutualTenant() ?
                value.description.trim().replace('Protections', 'Policies') : value.description.trim(),
            type: this.model.type,
            isFixed: this.model.isFixed,
            isPermanentRole: this.model.isPermanentRole,
            isDeletable: this.model.isDeletable,
            isRenamable: this.model.isRenamable,
            arePermissionsEditable: this.model.arePermissionsEditable,
            permissions: this.model.permissions,
            isDeleted: false,
            createdDateTime: null,
            lastModifiedDateTime: null,
            organisationId: this.model.organisationId,
            tenantId: this.authService.tenantId,
        };

        await this.sharedLoaderService.presentWait();
        this.roleApiService.update(role.id, role)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe((newRole: RoleResourceModel) => {
                this.eventService.getEntityUpdatedSubject('Role').next(newRole);
                this.sharedAlertService.showToast(`${newRole.name} role was modified successfully`);
                this.returnToDetailPage();
            });
    }

    public returnToPrevious(): void {
        if (!this.isEdit) {
            this.returnToListPage();
        } else {
            this.returnToDetailPage();
        }
    }

    private returnToListPage(): void {
        this.navProxy.navigateBack(['role', 'list']);
    }

    private returnToDetailPage(): void {
        this.navProxy.navigateBack(['role', this.model.id]);
    }
}
