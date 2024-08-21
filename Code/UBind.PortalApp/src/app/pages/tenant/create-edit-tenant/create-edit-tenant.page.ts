import { Component, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AlertController, LoadingController } from '@ionic/angular';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { FormHelper } from '@app/helpers/form.helper';
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { TenantViewModel } from '@app/viewmodels/tenant.viewmodel';
import { Subject } from 'rxjs';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { AdditionalPropertyDefinition, AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { UserService } from '@app/services/user.service';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyValueUpsertResourceModel } from '@app/resource-models/additional-property.resource-model';
import { Permission } from '@app/helpers';
import { PermissionService } from '@app/services/permission.service';

/**
 * Export create/edit tenant page component class.
 * This class manage creation and editing of tenants.
 */
@Component({
    selector: 'app-create-edit-tenant',
    templateUrl: './create-edit-tenant.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditTenantPage extends CreateEditPage<TenantResourceModel> implements OnInit, OnDestroy {
    public isEdit: boolean;
    public subjectName: string = "Tenant";
    public tenantAlias: string;
    public canEditAdditionalPropertyValues: boolean;

    private mode: string = '';

    private tenantAdditionalPropertyInputFields: Array<AdditionalPropertyValue>;

    public constructor(
        protected eventService: EventService,
        protected loadCtrl: LoadingController,
        protected alertCtrl: AlertController,
        protected tenantApiService: TenantApiService,
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        private sharedLoader: SharedLoaderService,
        private sharedAlert: SharedAlertService,
        elementRef: ElementRef,
        injector: Injector,
        public formHelper: FormHelper,
        private additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        public userService: UserService,
        private permissionService: PermissionService,
    ) {
        super(eventService, elementRef, injector, formHelper);
    }

    public async ngOnInit(): Promise<void> {
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');
        this.isEdit = this.tenantAlias != null;
        super.ngOnInit();
        this.destroyed = new Subject<void>();
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        this.mode = pathSegments[pathSegments.length - 1];
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        this.isLoading = this.isEdit;
        if (this.isEdit) {
            await this.initializeAdditionalPropertyInputFields();
            await this.load();
        } else {
            this.isLoading = this.isEdit;
            this.detailList = TenantViewModel.createDetailsListForEdit([], this.canEditAdditionalPropertyValues);
            this.form = this.buildForm();
        }
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    public async load(): Promise<void> {
        this.isLoading = true;
        this.tenantApiService.get(this.tenantAlias)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed))
            .subscribe(
                (tenant: TenantResourceModel) => {
                    tenant.additionalPropertyValues =
                        AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForEdit(
                            this.tenantAdditionalPropertyInputFields, tenant.additionalPropertyValues);
                    this.model = tenant;
                    this.detailList = TenantViewModel.createDetailsListForEdit(
                        tenant.additionalPropertyValues,
                        this.canEditAdditionalPropertyValues);
                    this.form = this.buildForm();
                    let tenantNameValues: any = {
                        name: this.model.name,
                        alias: this.model.alias,
                        customDomain: this.model.customDomain,
                        disabled: this.model.disabled,
                    };
                    if (this.tenantAdditionalPropertyInputFields.length > 0
                        && this.canEditAdditionalPropertyValues) {
                        tenant.additionalPropertyValues.forEach((apv: AdditionalPropertyValue) => {
                            tenantNameValues[
                                AdditionalPropertiesHelper.generateControlId(
                                    apv.additionalPropertyDefinitionModel.id)] = apv.value;
                        });
                    }
                    this.form.patchValue(tenantNameValues);
                },
                (err: any) => {
                    this.errorMessage = 'There was a problem loading the tenant details';
                    throw err;
                });
    }

    protected buildForm(): FormGroup {
        let controls: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        const form: FormGroup = this.formBuilder.group(controls);
        return form;
    }

    public async create(value: any): Promise<void> {
        await this.sharedLoader.presentWithDelay();
        this.tenantApiService.create(value)
            .pipe(
                finalize(() => this.sharedLoader.dismiss()),
                takeUntil(this.destroyed))
            .subscribe((tenant: TenantResourceModel) => {
                this.eventService.getEntityCreatedSubject('Tenant').next(tenant);
                this.sharedAlert.showToast(`Tenant ${tenant.name} was created`);
                this.navProxy.navigateBack(['tenant', tenant.alias], true);
            });
    }

    public async update(value: any): Promise<void> {
        let properties: Array<AdditionalPropertyValueUpsertResourceModel> = [];
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        if (this.canEditAdditionalPropertyValues) {
            properties = AdditionalPropertiesHelper.buildProperties(
                this.tenantAdditionalPropertyInputFields, value);
        }

        await this.sharedLoader.presentWithDelay();

        if (properties.length > 0) {
            value = { ...value, properties: properties };
        }

        value.disabled = this.model.disabled;
        value.deleted = this.model.deleted;
        this.tenantApiService.update(this.model.id, value)
            .pipe(
                finalize(() => this.sharedLoader.dismiss()),
                takeUntil(this.destroyed))
            .subscribe((tenant: TenantResourceModel) => {
                this.eventService.getEntityUpdatedSubject('Tenant').next(tenant);
                this.sharedAlert.showToast(`Tenant details for ${tenant.name} were saved`);
                this.navProxy.navigateBack(['tenant', tenant.alias], true);
            });
    }

    public returnToPrevious(): void {
        if (this.model) {
            this.navProxy.navigateBack(['tenant', this.model.alias], true);
        } else {
            this.navProxy.navigateBack(['tenant', 'list'], true);
        }
    }

    private async initializeAdditionalPropertyInputFields(): Promise<void> {
        const tenantAdditionalProperties: Array<AdditionalPropertyDefinition> = await this.additionalPropertiesService
            .getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextId(
                this.tenantAlias,
                AdditionalPropertyDefinitionContextType.Tenant,
                EntityType.Tenant,
                this.tenantAlias)
            .toPromise();
        this.tenantAdditionalPropertyInputFields =
            AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForCreateForm(tenantAdditionalProperties);
    }
}
