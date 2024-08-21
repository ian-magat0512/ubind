import { Component, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { AlertController } from '@ionic/angular';
import { finalize, takeUntil } from 'rxjs/operators';
import { scrollbarStyle } from '@assets/scrollbar';
import { Permission, StringHelper } from '@app/helpers';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { PortalApiService } from '@app/services/api/portal-api.service';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { PortalResourceModel, PortalRequestModel } from '@app/resource-models/portal/portal.resource-model';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { FormHelper } from '@app/helpers/form.helper';
import { TenantService } from '@app/services/tenant.service';
import { PortalDetailViewModel } from '@app/viewmodels/portal-detail.viewmodel';
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { AdditionalPropertyDefinitionApiService }
    from '@app/services/api/additional-property-definition-api.service';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyDefinition, AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { AdditionalPropertyValueUpsertResourceModel } from '@app/resource-models/additional-property.resource-model';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { Subject, Subscription } from 'rxjs';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { ActivatedRoute, Params } from '@angular/router';
import { PermissionService } from '@app/services/permission.service';
import { PortalUserType } from '@app/models/portal-user-type.enum';
import { AuthenticationService } from '@app/services/authentication.service';
import { OrganisationModel } from '@app/models/organisation.model';

/**
 * Export create/edit portal page component class
 * This class manage for creating and editing of portal.
 */
@Component({
    selector: 'app-create-edit-portal',
    templateUrl: './create-edit-portal.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditPortalPage extends CreateEditPage<PortalResourceModel> implements OnInit, OnDestroy {
    public isEdit: boolean;
    public subjectName: string = "Portal";
    public tenantId: string;
    public tenantAlias: string;
    private portalId: string;
    private pathOrganisationId: string;
    private defaultOrganisationId: string;
    private performingUserOrganisationId: string;
    private portalAdditionalPropertyValueFields: Array<AdditionalPropertyValue> = [];
    private entityType: EntityType = EntityType.Portal;
    private canEditAdditionalPropertyValues: boolean;

    public constructor(
        protected alertCtrl: AlertController,
        protected eventService: EventService,
        protected formBuilder: FormBuilder,
        protected portalApiService: PortalApiService,
        private routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        private sharedAlert: SharedAlertService,
        private tenantApiService: TenantApiService,
        public layoutManager: LayoutManagerService,
        protected tenantService: TenantService,
        protected sharedLoaderService: SharedLoaderService,
        elementRef: ElementRef,
        injector: Injector,
        public formHelper: FormHelper,
        private additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        private additionalPropertyValueService: AdditionalPropertyValueService,
        protected appConfigService: AppConfigService,
        private route: ActivatedRoute,
        private permissionService: PermissionService,
        private authenticationService: AuthenticationService,
    ) {
        super(eventService, elementRef, injector, formHelper);
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.tenantAlias = appConfig.portal.tenantAlias;
        });
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async ngOnInit(): Promise<void> {
        this.portalId = this.routeHelper.getParam('portalId') || null;
        this.isEdit = this.portalId != null;
        super.ngOnInit();
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        let tenantAliasFromUrl: string = this.routeHelper.getParam('tenantAlias');
        this.tenantAlias =
            tenantAliasFromUrl
                ? tenantAliasFromUrl
                : this.tenantAlias;
        if (!this.tenantId) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        }        this.destroyed = new Subject<void>();
        this.eventService.performingUserOrganisationSubject$.pipe(takeUntil(this.destroyed))
            .subscribe((organisation: OrganisationModel) => {
                this.performingUserOrganisationId = organisation.id;
            });
        this.route.params.pipe(takeUntil(this.destroyed)).subscribe((params: Params) => {
            this.pathOrganisationId = params['organisationId'];
        });
        if (tenantAliasFromUrl) {
            await this.loadTenantFromAlias(this.tenantAlias);
        }
        await this.initializeAdditionalPropertyInputFields();

        if (this.isEdit) {
            this.load();
        } else {
            this.model = {
                id: null,
                name: '',
                alias: '',
                title: '',
                stylesheetUrl: '',
                styles: '',
                deleted: false,
                disabled: false,
                isDefault: false,
                createdDateTime: '',
                createdTicksSinceEpoch: 0,
                lastModifiedDateTime: '',
                lastModifiedTicksSinceEpoch: 0,
                tenantId: this.tenantId,
                tenantName: null,
                userType: null,
                productionUrl: null,
                stagingUrl: null,
                developmentUrl: null,
                organisationId: this.getContextOrganisationId(),
                additionalPropertyValues: [],
            };

            this.detailList = PortalDetailViewModel.createDetailsListForEdit(
                this.portalAdditionalPropertyValueFields, this.canEditAdditionalPropertyValues, this.isEdit);
            this.isLoading = false;
            this.form = this.buildForm();
            if (this.canEditAdditionalPropertyValues) {
                let formValue: any = AdditionalPropertiesHelper.setFormValue(
                    [],
                    this.portalAdditionalPropertyValueFields,
                    [],
                );
                this.form.patchValue(formValue);
            }
            this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                this.portalAdditionalPropertyValueFields,
                this.form,
                "",
                this.tenantId,
                this.detailList,
            );
        }
    }

    private async initializeAdditionalPropertyInputFields(): Promise<void> {
        const additionalProperties: Array<AdditionalPropertyDefinition> = await this.additionalPropertiesService
            .getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextId(
                this.tenantId,
                AdditionalPropertyDefinitionContextType.Tenant,
                this.entityType,
                this.tenantId,
            )
            .toPromise();
        this.portalAdditionalPropertyValueFields =
            AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForCreateForm(additionalProperties);
    }

    protected buildForm(): FormGroup {
        let controls: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        const form: FormGroup = this.formBuilder.group(controls);
        return form;
    }

    private async loadTenantFromAlias(tenantAlias: string): Promise<void> {
        return this.tenantApiService.get(tenantAlias)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed))
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

    public async load(): Promise<void> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set("tenant", this.tenantId);
        params.set("useCache", 'false');
        this.portalApiService.getById(this.portalId, params)
            .pipe(finalize(() => this.isLoading = false))
            .subscribe(
                (data: PortalResourceModel) => {
                    this.model = data;
                    data.additionalPropertyValues = AdditionalPropertiesHelper
                        .generateAdditionalPropertyInputFieldsForEdit(
                            this.portalAdditionalPropertyValueFields, data.additionalPropertyValues);
                    this.detailList = PortalDetailViewModel.createDetailsListForEdit(
                        data.additionalPropertyValues, this.canEditAdditionalPropertyValues, this.isEdit);
                    this.form = this.buildForm();
                    this.setFormValue(data);
                    this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                        data.additionalPropertyValues,
                        this.form,
                        this.portalId,
                        this.tenantId,
                        this.detailList);
                },
                (err: any) => {
                    this.errorMessage = 'There was a problem loading the portal details';
                    throw err;
                });
    }

    private setFormValue(portalResourceModel: PortalResourceModel): void {
        let formValue: any = {
            name: this.model.name,
            alias: this.model.alias,
            title: this.model.title,
            userType: this.model.userType.toLowerCase(),
        };
        if (this.canEditAdditionalPropertyValues) {
            formValue = AdditionalPropertiesHelper.setFormValue(
                formValue,
                this.portalAdditionalPropertyValueFields,
                portalResourceModel.additionalPropertyValues,
            );
        }
        this.form.patchValue(formValue);
    }

    public async create(value: any): Promise<void> {
        if (this.canEditAdditionalPropertyValues
            && !await this.additionalPropertyValueService.validateAdditionalPropertyValues(
                this.portalAdditionalPropertyValueFields,
                null,
                this.form,
                value,
                this.tenantId)) {
            return;
        }

        this.model.name = value.name;
        this.model.alias = value.alias;
        this.model.title = value.title;
        this.model.tenantId = this.tenantId;
        this.model.userType = <PortalUserType>StringHelper.camelCase(value.userType);
        this.model.organisationId = this.getContextOrganisationId();
        this.model.deleted = false;
        this.model.disabled = false;
        this.model.createdDateTime = '';
        this.model.lastModifiedDateTime = '';
        this.populatePropertiesBeforePersisting(value, false);
        await this.sharedLoaderService.presentWithDelay();

        let requestModel: PortalRequestModel = new PortalRequestModel(this.model);
        const subscription: Subscription = this.portalApiService.create(requestModel)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe((portal: PortalResourceModel) => {
                this.eventService.getEntityCreatedSubject('Portal').next(portal);
                this.sharedAlert.showToast(`Portal ${portal.name} was created`);
                this.goToPortal(portal.id);
            });
    }

    public async update(value: any): Promise<void> {
        if (this.canEditAdditionalPropertyValues
            && !await this.additionalPropertyValueService.validateAdditionalPropertyValues(
                this.portalAdditionalPropertyValueFields,
                this.model.id,
                this.form,
                value,
                this.tenantId)) {
            return;
        }
        this.model.name = value.name;
        this.model.alias = value.alias;
        this.model.title = value.title;
        this.model.userType = <PortalUserType>StringHelper.camelCase(value.userType);

        this.populatePropertiesBeforePersisting(value, true);

        await this.sharedLoaderService.presentWithDelay();

        let requestModel: PortalRequestModel = new PortalRequestModel(this.model);
        const subscription: Subscription = this.portalApiService.update(this.model.id, requestModel)
            .pipe(finalize(() => {
                this.sharedLoaderService.dismiss();
                subscription.unsubscribe();
            }))
            .subscribe((portal: PortalResourceModel) => {
                this.eventService.getEntityUpdatedSubject('Portal').next(portal);
                this.sharedAlert.showToast(`Portal details for ${portal.name} were saved`);
                this.goToPortal(portal.id);
            });
    }

    private populatePropertiesBeforePersisting(value: any, isUpdate: boolean): void {
        let properties: Array<AdditionalPropertyValueUpsertResourceModel> = [];
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        if ((this.canEditAdditionalPropertyValues && isUpdate) || !isUpdate) {
            properties = AdditionalPropertiesHelper.buildProperties(
                this.portalAdditionalPropertyValueFields, value);
        }

        if (properties.length > 0) {
            this.model = { ...this.model, properties: properties };
        }
    }

    public goToPortal(portalId: string): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        if (!this.isEdit) {
            pathSegments.push(portalId);
        }
        this.navProxy.navigateForward(pathSegments);
    }

    public returnToPrevious(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('portal');
        if (this.model && this.model.id) {
            pathSegments.push(this.model.id);
            this.navProxy.navigateForward(pathSegments);
        } else {
            if (this.pathOrganisationId) {
                pathSegments.pop();
                this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Portals' } });
            } else {
                this.navProxy.navigateBack(pathSegments);
            }
        }
    }

    private getContextOrganisationId(): string {
        return this.pathOrganisationId || this.defaultOrganisationId || this.performingUserOrganisationId;
    }
}
