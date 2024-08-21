import { Component, Injector, ElementRef, OnInit, OnDestroy } from '@angular/core';
import { scrollbarStyle } from '@assets/scrollbar';
import { EventService } from '@app/services/event.service';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { FormHelper } from '@app/helpers/form.helper';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { OrganisationApiService } from '@app/services/api/organisation-api.service';
import {
    UpsertOrganisationResourceModel, OrganisationResourceModel,
} from '@app/resource-models/organisation/organisation.resource-model';
import { RouteHelper } from '@app/helpers/route.helper';
import { finalize, takeUntil, debounceTime, filter } from 'rxjs/operators';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { OrganisationViewModel } from '../../../viewmodels/organisation.viewmodel';
import { ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { Permission } from '@app/helpers';
import { CreateEditPage } from '../../master-create/create-edit.page';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { AdditionalPropertyDefinition, AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { UserService } from '@app/services/user.service';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { AdditionalPropertyValueUpsertResourceModel } from '@app/resource-models/additional-property.resource-model';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { PermissionService } from '@app/services/permission.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { AuthenticationService } from '@app/services/authentication.service';
import {
    OrganisationLinkedIdentity,
    OrganisationLinkedIdentityUpsertModel,
} from '@app/resource-models/organisation/organisation-linked-identity.resource-model';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { TenantApiService } from '@app/services/api/tenant-api.service';

/**
 * Export create/edit organisation page component class
 * This class manage for creating and editing of organisation page.
 */
@Component({
    selector: 'app-create-organisation',
    templateUrl: './create-edit-organisation.page.html',
    styleUrls: [
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditOrganisationPage
    extends CreateEditPage<OrganisationViewModel> implements OnInit, OnDestroy {
    public isEdit: boolean;

    public subjectName: string = 'Organisation';
    protected organisationId: string;
    public canEditAdditionalPropertyValues: boolean;
    private contextTenantAlias: string;
    private organisationAdditionalPropertyInputFields: Array<AdditionalPropertyValue>;
    private tenantId: string;
    private organisationResourceModel: OrganisationResourceModel;
    private linkedIdentities: Array<OrganisationLinkedIdentity>;
    private defaultOrganisationId: string;

    public constructor(
        protected formBuilder: FormBuilder,
        protected organisationApiService: OrganisationApiService,
        private route: ActivatedRoute,
        private navProxy: NavProxyService,
        public formHelper: FormHelper,
        private sharedLoaderService: SharedLoaderService,
        private routeHelper: RouteHelper,
        private sharedAlertService: SharedAlertService,
        private eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        public userService: UserService,
        public additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        private additionalPropertyValueService: AdditionalPropertyValueService,
        private permissionService: PermissionService,
        appConfigService: AppConfigService,
        private authenticationService: AuthenticationService,
        private tenantApiService: TenantApiService,
    ) {
        super(eventService, elementRef, injector, formHelper);

        this.destroyed = new Subject<void>();
        appConfigService.appConfigSubject
            .pipe(takeUntil(this.destroyed))
            .subscribe((appConfig: AppConfig) => {
                this.tenantId = appConfig.portal.tenantId;
            });
    }

    public async ngOnInit(): Promise<void> {
        this.contextTenantAlias = this.routeHelper.getContextTenantAlias();
        this.organisationId = this.route.snapshot.paramMap.get('organisationId');
        this.isEdit = this.organisationId != null;
        super.ngOnInit();
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        await this.loadTenantFromAlias(this.contextTenantAlias);
        await this.initializeAdditionalPropertyInputFields();
        if (this.isEdit) {
            this.load();
        } else {
            await this.loadLinkedIdentities();
            this.detailList =
                OrganisationViewModel.createDetailsListForEdit(
                    this.organisationAdditionalPropertyInputFields,
                    this.canEditAdditionalPropertyValues,
                    this.linkedIdentities);
            this.form = this.buildForm();
            if (this.organisationAdditionalPropertyInputFields.length > 0
                && this.canEditAdditionalPropertyValues) {
                let defaultValues: any = [];
                this.organisationAdditionalPropertyInputFields.forEach((item: AdditionalPropertyValue) => {
                    defaultValues[AdditionalPropertiesHelper.generateControlId(
                        item.additionalPropertyDefinitionModel.id)] =
                        item.value;
                });
                this.form.patchValue(defaultValues);
                this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                    this.organisationAdditionalPropertyInputFields,
                    this.form,
                    "",
                    this.tenantId,
                    this.detailList);
            }
            this.watchNameValueChangeForUniqueness();
            this.watchAliasValueChangeForUniqueness();
            this.isLoading = false;
        }
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.unsubscribe();
    }

    public async load(): Promise<void> {
        try {
            await this.sharedLoaderService.presentWait();
            let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
            params.set('tenant', this.contextTenantAlias);
            await this.organisationApiService.getById(this.organisationId, params)
                .pipe(
                    finalize(() => this.isLoading = false),
                    takeUntil(this.destroyed),
                )
                .toPromise()
                .then((organisationResourceModel: OrganisationResourceModel) => {
                    this.organisationResourceModel = organisationResourceModel;
                    organisationResourceModel.additionalPropertyValues =
                        AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForEdit(
                            this.organisationAdditionalPropertyInputFields,
                            organisationResourceModel.additionalPropertyValues);
                });

            await this.loadLinkedIdentities();
            this.detailList = OrganisationViewModel.createDetailsListForEdit(
                this.organisationResourceModel.additionalPropertyValues,
                this.canEditAdditionalPropertyValues,
                this.linkedIdentities);
            this.form = this.buildForm();
            this.setFormValue(this.organisationResourceModel);
            this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                this.organisationAdditionalPropertyInputFields,
                this.form,
                this.organisationId,
                this.tenantId,
                this.detailList);
            this.watchNameValueChangeForUniqueness();
            this.watchAliasValueChangeForUniqueness();
        } finally {
            this.sharedLoaderService.dismiss();
        }
    }

    public async create(value: any): Promise<void> {
        if (this.canEditAdditionalPropertyValues
            && !await this.additionalPropertyValueService.validateAdditionalPropertyValues(
                this.organisationAdditionalPropertyInputFields,
                "",
                this.form,
                value,
                this.tenantId)
        ) {
            return;
        }
        let organisationModel: UpsertOrganisationResourceModel = {
            name: value.name,
            alias: value.alias,
            tenant: this.contextTenantAlias,
            managingOrganisation: this.getDefaultManagingOrganisationId(),
            linkedIdentities: this.convertLinkedIdentityFormValuesToLinkedIdentityUpsertModels(value.linkedIdentities),
        };
        let properties: Array<AdditionalPropertyValueUpsertResourceModel> = AdditionalPropertiesHelper.buildProperties(
            this.organisationAdditionalPropertyInputFields, value);

        if (properties.length > 0) {
            organisationModel = { ...organisationModel, properties };
        }

        this.sharedLoaderService.presentWait().then(() => {
            this.organisationApiService.create(organisationModel)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed))
                .subscribe((organisationResourceModel: OrganisationResourceModel) => {
                    this.eventService.getEntityCreatedSubject('Organisation').next(organisationResourceModel);
                    this.sharedAlertService.showToast(`Organisation ${organisationResourceModel.name} was created`);
                    this.organisationId = organisationResourceModel.id;
                    this.returnToPrevious();
                });
        });
    }

    public async update(value: any): Promise<void> {
        if (this.canEditAdditionalPropertyValues
            && !await this.additionalPropertyValueService.validateAdditionalPropertyValues(
                this.organisationAdditionalPropertyInputFields,
                this.organisationId,
                this.form,
                value,
                this.tenantId)) {
            return;
        }
        let organisationModel: UpsertOrganisationResourceModel = {
            alias: value.alias,
            name: value.name,
            tenant: this.organisationResourceModel.tenantId,
            managingOrganisation: this.organisationResourceModel.managingOrganisationId,
            linkedIdentities: this.convertLinkedIdentityFormValuesToLinkedIdentityUpsertModels(value.linkedIdentities),
        };
        let properties: Array<AdditionalPropertyValueUpsertResourceModel> = [];
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        if (this.canEditAdditionalPropertyValues) {
            properties = AdditionalPropertiesHelper.buildProperties(
                this.organisationAdditionalPropertyInputFields, value);
        }

        if (properties.length > 0) {
            organisationModel = { ...organisationModel, properties };
        }

        this.sharedLoaderService.presentWait().then(() => {
            this.organisationApiService.update(this.organisationId, organisationModel)
                .pipe(
                    finalize(() => this.sharedLoaderService.dismiss()),
                    takeUntil(this.destroyed))
                .subscribe((organisationResourceModel: OrganisationResourceModel) => {
                    this.eventService.getEntityUpdatedSubject('Organisation').next(organisationResourceModel);
                    this.sharedAlertService
                        .showToast(`Organisation ${organisationResourceModel.name} successfully updated`);
                    this.returnToPrevious();
                });
        });
    }

    private convertLinkedIdentityFormValuesToLinkedIdentityUpsertModels(
        uniqueIds: Array<{ uniqueId: string }>,
    ): Array<OrganisationLinkedIdentityUpsertModel> {
        if (uniqueIds == null) {
            return null;
        }
        const upsertModels: Array<OrganisationLinkedIdentityUpsertModel> = [];
        for (let i: number = 0; i < uniqueIds.length; i++) {
            const uniqueId: string = uniqueIds[i].uniqueId;
            upsertModels.push({
                authenticationMethodId: this.linkedIdentities[i].authenticationMethodId,
                uniqueId: uniqueId,
            });
        }
        return upsertModels;
    }

    private async loadTenantFromAlias(tenantAlias: string): Promise<void> {
        this.isLoading = true;
        return this.tenantApiService.get(tenantAlias)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed),
            )
            .toPromise()
            .then((tenant: TenantResourceModel) => {
                this.tenantId = tenant.id;
                this.defaultOrganisationId = tenant.defaultOrganisationId;
            },
            (err: any) => {
                this.errorMessage = "There was a problem loading the tenant details";
                throw err;
            });
    }

    /**
     * Loads the linked identities for this organisation. If it's a new organisation, it will still load
     * them but they will be blank (ie no unique ID set)
     */
    private async loadLinkedIdentities(): Promise<void> {
        this.isLoading = true;
        if (this.isEdit) {
            await this.organisationApiService.getLinkedIdentities(
                this.organisationId,
                this.routeHelper.getContextTenantAlias(),
                true)
                .pipe(takeUntil(this.destroyed), finalize(() => this.isLoading = false))
                .toPromise()
                .then((linkedIdentities: Array<OrganisationLinkedIdentity>) => {
                    this.linkedIdentities = linkedIdentities;
                    this.organisationResourceModel.linkedIdentities = linkedIdentities;
                });
        } else {
            await this.organisationApiService.getPotentialLinkedIdentities(
                this.getDefaultManagingOrganisationId(),
                this.routeHelper.getContextTenantAlias())
                .pipe(takeUntil(this.destroyed), finalize(() => this.isLoading = false))
                .toPromise()
                .then((linkedIdentities: Array<OrganisationLinkedIdentity>) => {
                    this.linkedIdentities = linkedIdentities;
                });
        }
    }

    public returnToPrevious(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('organisation');
        if (this.organisationId) {
            pathSegments.push(this.organisationId);
        } else {
            pathSegments.push('list');
        }
        this.navProxy.navigateBack(pathSegments);
    }

    private setFormValue(organisationResourceModel: OrganisationResourceModel): void {
        let formValue: any = {
            alias: organisationResourceModel.alias,
            name: organisationResourceModel.name,
            linkedIdentities: new Array<{ uniqueId: string }>(),
        };

        for (let linkedIdentities of this.linkedIdentities) {
            formValue.linkedIdentities.push({ uniqueId: linkedIdentities.uniqueId });
        }

        if (this.canEditAdditionalPropertyValues) {
            formValue = AdditionalPropertiesHelper.setFormValue(
                formValue,
                this.organisationAdditionalPropertyInputFields,
                organisationResourceModel.additionalPropertyValues);
        }
        this.form.patchValue(formValue);
    }

    protected buildForm(): FormGroup {
        let controls: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        const form: FormGroup = this.formBuilder.group(controls);
        return form;
    }

    protected watchNameValueChangeForUniqueness(): void {
        const control: AbstractControl = this.form.get('name');
        control.valueChanges
            .pipe(
                debounceTime(500),
                takeUntil(this.destroyed),
                filter((value: any) => value != null))
            .subscribe((value: any) => {
                if (control.valid) {
                    const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
                    params.set('names', [value.trim()]);
                    params.set('tenant', this.contextTenantAlias);
                    this.organisationApiService.getList(params)
                        .pipe(takeUntil(this.destroyed))
                        .subscribe((data: Array<OrganisationResourceModel>) => {
                            if (data.length === 0) {
                                return;
                            }
                            if (this.isEdit && data.filter((org: OrganisationResourceModel) =>
                                org.id !== this.organisationId).length === 0) {
                                return null;
                            }

                            control.markAsTouched({ onlySelf: true });
                            control.setErrors({ uniqueness: true });
                        });
                }
            });
    }

    protected watchAliasValueChangeForUniqueness(): void {
        const control: AbstractControl = this.form.get('alias');
        control.valueChanges
            .pipe(
                debounceTime(500),
                takeUntil(this.destroyed),
                filter((value: any) => value != null))
            .subscribe((value: any) => {
                if (control.valid) {
                    const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
                    params.set('aliases', [value.trim()]);
                    params.set('tenant', this.contextTenantAlias);
                    this.organisationApiService.getList(params)
                        .pipe(takeUntil(this.destroyed))
                        .subscribe((data: Array<OrganisationResourceModel>) => {
                            if (data.length === 0) {
                                return;
                            }
                            if (this.isEdit && data.filter((org: OrganisationResourceModel) =>
                                org.id !== this.organisationId).length === 0) {
                                return null;
                            }
                            control.markAsTouched({ onlySelf: true });
                            control.setErrors({ uniqueness: true });
                        });
                }
            });
    }

    private async initializeAdditionalPropertyInputFields(): Promise<void> {
        let additionalPropertyDefinitions: Array<AdditionalPropertyDefinition> = this.organisationId ?
            await this.additionalPropertiesService
                .getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextIdAndParentContextId(
                    this.routeHelper.getContextTenantAlias(),
                    AdditionalPropertyDefinitionContextType.Organisation,
                    EntityType.Organisation,
                    this.organisationId,
                    this.contextTenantAlias,
                    true)
                .toPromise() :
            await this.additionalPropertiesService
                .getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextId(
                    this.routeHelper.getContextTenantAlias(),
                    AdditionalPropertyDefinitionContextType.Tenant,
                    EntityType.Organisation,
                    this.contextTenantAlias)
                .toPromise();
        this.organisationAdditionalPropertyInputFields =
            AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForCreateForm(
                additionalPropertyDefinitions);
    }

    private getDefaultManagingOrganisationId(): string {
        if (this.authenticationService.isMasterUser()) {
            return this.organisationId || this.defaultOrganisationId;
        } else {
            return this.authenticationService.userOrganisationId;
        }
    }
}
