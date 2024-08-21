import { Component, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Permission } from '@app/helpers';
import { AlertController } from '@ionic/angular';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ProductCreateRequestResourceModel, ProductResourceModel } from '@app/resource-models/product.resource-model';
import { ProductApiService } from '@app/services/api/product-api.service';
import { scrollbarStyle } from '@assets/scrollbar';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { EventService } from '@app/services/event.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ProductStatus } from '@app/models/product-status.enum';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { Errors } from '@app/models/errors';
import { FormHelper } from '@app/helpers/form.helper';
import { TenantService } from '@app/services/tenant.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { ProductViewModel } from '@app/viewmodels/product.viewmodel';
import { CreateEditPage } from '@app/pages/master-create/create-edit.page';
import { DetailsListFormItem } from '@app/models/details-list/details-list-form-item';
import { AdditionalPropertyValue, AdditionalPropertyDefinition } from '@app/models/additional-property-item-view.model';
import { Subject } from 'rxjs';
import { AdditionalPropertiesHelper } from '@app/helpers/additional-properties.helper';
import { UserService } from '@app/services/user.service';
import { AdditionalPropertyDefinitionApiService } from '@app/services/api/additional-property-definition-api.service';
import { AdditionalPropertyDefinitionContextType } from '@app/models/additional-property-context-type.enum';
import { EntityType } from '@app/models/entity-type.enum';
import { AdditionalPropertyValueUpsertResourceModel } from '@app/resource-models/additional-property.resource-model';
import { AdditionalPropertyValueService } from '@app/services/additional-property-value.service';
import { ProductService } from '@app/services/product.service';
import { PermissionService } from '@app/services/permission.service';

/**
 * Export create/edit product page component class
 * TODO: Write a better class header: creation and editing of product.
 */
@Component({
    selector: 'app-create-edit-product',
    templateUrl: './create-edit-product.page.html',
    styleUrls: [
        '../../../../assets/css/scrollbar-form.css',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class CreateEditProductPage extends CreateEditPage<ProductViewModel> implements OnInit, OnDestroy {
    public isEdit: boolean;
    public subjectName: string = 'Product';
    public canEditAdditionalPropertyValues: boolean;
    private tenantId: string;
    private productAlias: string;
    private tenantAlias: string;
    private productAdditionalPropertyInputFields: Array<AdditionalPropertyValue>;

    public constructor(
        public eventService: EventService,
        protected sharedLoaderService: SharedLoaderService,
        protected alertCtrl: AlertController,
        protected tenantService: TenantService,
        protected productApiService: ProductApiService,
        private formBuilder: FormBuilder,
        private routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        public layoutManager: LayoutManagerService,
        private sharedLoader: SharedLoaderService,
        protected sharedAlertService: SharedAlertService,
        public formHelper: FormHelper,
        elementRef: ElementRef,
        injector: Injector,
        private appConfigService: AppConfigService,
        public userService: UserService,
        public additionalPropertiesService: AdditionalPropertyDefinitionApiService,
        public additionalPropertyValueService: AdditionalPropertyValueService,
        private productService: ProductService,
        private permissionService: PermissionService,
    ) {
        super(eventService, elementRef, injector, formHelper);
    }

    public async ngOnInit(): Promise<void> {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        this.isEdit = pathSegments[pathSegments.length - 1] === 'edit';
        super.ngOnInit();
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        this.tenantAlias = this.routeHelper.getParam('tenantAlias') || this.routeHelper.getParam('portalTenantAlias');
        if (!this.tenantId && this.tenantAlias) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        }
        this.productAlias = this.routeHelper.getParam('productAlias');
        this.destroyed = new Subject<void>();
        await this.load();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.unsubscribe();
    }

    protected buildForm(): FormGroup {
        let controls: any = [];
        this.detailList.forEach((item: DetailsListFormItem) => {
            controls[item.Alias] = item.FormControl;
        });
        const form: FormGroup = this.formBuilder.group(controls);
        return form;
    }

    public async load(): Promise<void> {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantId = appConfig.portal.tenantId;
            }
        });

        if (!this.tenantId) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        }

        await this.initializeAdditionalPropertyInputFields();

        if (!this.productAlias) {
            this.detailList = ProductViewModel.createDetailsListForEdit(
                this.productAdditionalPropertyInputFields,
                this.canEditAdditionalPropertyValues);
            this.isLoading = false;
            this.form = this.buildForm();
            if (this.productAdditionalPropertyInputFields.length > 0
                && this.canEditAdditionalPropertyValues) {
                let defaultValues: any = [];
                this.productAdditionalPropertyInputFields.forEach((item: AdditionalPropertyValue) => {
                    defaultValues[AdditionalPropertiesHelper.generateControlId(
                        item.additionalPropertyDefinitionModel.id)] = item.value;
                });
                this.form.patchValue(defaultValues);
                this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                    this.productAdditionalPropertyInputFields,
                    this.form,
                    "",
                    this.tenantId,
                    this.detailList);
            }
        } else {
            let productId: string = await this.productService.getProductIdFromAlias(
                this.tenantAlias, this.productAlias);
            this.productApiService.getByAlias(this.productAlias, this.tenantId)
                .pipe(
                    finalize(() => this.isLoading = false),
                    takeUntil(this.destroyed))
                .subscribe(
                    (product: ProductResourceModel) => {
                        product.additionalPropertyValues =
                            AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForEdit(
                                this.productAdditionalPropertyInputFields,
                                product.additionalPropertyValues,
                            );
                        this.detailList = ProductViewModel.createDetailsListForEdit(
                            product.additionalPropertyValues,
                            this.canEditAdditionalPropertyValues);
                        this.model = new ProductViewModel(product);
                        this.form = this.buildForm();
                        this.setFormValue(product);
                        this.additionalPropertyValueService.registerUniqueAdditionalPropertyFieldsOnValueChanges(
                            this.productAdditionalPropertyInputFields,
                            this.form,
                            productId,
                            this.tenantId,
                            this.detailList);
                    },
                    (err: any) => {
                        this.errorMessage = 'There was a problem loading the product details';
                        throw err;
                    });
        }
    }

    public returnToPrevious(): void {
        const pathSegments: Array<string> = this.routeHelper.getPathSegments();
        if (pathSegments[pathSegments.length - 1] === 'edit') {
            pathSegments.pop();
            this.navProxy.navigate(pathSegments);
        } else {
            const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
            if (pathSegmentsAfter[0] === 'product') {
                this.navProxy.navigateBack(['product', 'list']);
            } else {
                const navigationExtras: any = { queryParams: { segment: 'Products' } };
                this.navProxy.navigateBack(['tenant', this.tenantAlias], true, navigationExtras);
            }
        }
    }

    public async create(value: any): Promise<void> {
        if (this.canEditAdditionalPropertyValues
            && !await this.additionalPropertyValueService.validateAdditionalPropertyValues(
                this.productAdditionalPropertyInputFields,
                "",
                this.form,
                value,
                this.tenantId)) {
            return;
        }
        const tenantId: string = this.model ? this.model.tenantId : this.tenantId;

        let productModel: ProductCreateRequestResourceModel = {
            alias: value.alias,
            name: value.name,
            tenant: tenantId,
        };
        let properties: Array<AdditionalPropertyValueUpsertResourceModel> = AdditionalPropertiesHelper.buildProperties(
            this.productAdditionalPropertyInputFields, value);

        if (properties.length > 0) {
            productModel = { ...productModel, properties: properties };
        }

        await this.sharedLoader.presentWithDelay();
        this.productApiService.create(productModel)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.sharedLoader.dismiss()))
            .subscribe((product: ProductResourceModel) => {
                this.eventService.getEntityCreatedSubject('Product').next(product);
                this.sharedAlertService.showToast(`Product ${product.name} was created`);
                this.checkUntilInitialised(product);
                this.navigateToProduct(product.alias);
            });
    }

    public async update(value: any): Promise<void> {
        let productId: string = !this.model
            ? await this.productService.getProductIdFromAlias(this.tenantAlias, this.model.alias)
            : this.model.id;

        if (this.canEditAdditionalPropertyValues
            && !await this.additionalPropertyValueService.validateAdditionalPropertyValues(
                this.productAdditionalPropertyInputFields,
                productId,
                this.form,
                value,
                this.tenantId)) {
            return;
        }
        let model: any = {
            id: this.model.id,
            alias: value.alias,
            name: value.name,
            disabled: this.model.disabled,
            deleted: this.model.deleted,
            tenantId: this.model.tenantId,
            tenant: this.model.tenantId,
            quoteExpirySettings: this.model.quoteExpirySettings,
        };
        let properties: Array<AdditionalPropertyValueUpsertResourceModel> = [];
        this.canEditAdditionalPropertyValues
            = this.permissionService.hasPermission(Permission.EditAdditionalPropertyValues);
        if (this.canEditAdditionalPropertyValues) {
            properties = AdditionalPropertiesHelper.buildProperties(
                this.productAdditionalPropertyInputFields, value);
        }

        if (properties.length > 0) {
            model = { ...model, properties: properties };
        }

        await this.sharedLoader.presentWithDelay();
        this.productApiService.update(this.model.id, model)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.sharedLoader.dismiss()))
            .subscribe((product: ProductResourceModel) => {
                this.eventService.getEntityUpdatedSubject('Product').next(product);
                this.sharedAlertService.showToast(`Product details for ${product.name} were saved`);
                this.navigateToProduct(product.alias);
            });
    }

    private setFormValue(productResourceModel: ProductResourceModel): void {
        let formValue: any = {
            name: productResourceModel.name,
            alias: productResourceModel.alias,
        };
        if (this.canEditAdditionalPropertyValues) {
            formValue = AdditionalPropertiesHelper.setFormValue(
                formValue,
                this.productAdditionalPropertyInputFields,
                productResourceModel.additionalPropertyValues);
        }
        this.form.patchValue(formValue);
    }

    private navigateToProduct(productAlias: string): void {
        this.navProxy.navigateBackOne();
        const pathSegments: Array<string> = this.routeHelper.getPathSegmentsUpUntil('product');
        pathSegments.push(productAlias);
        this.navProxy.navigateForward(pathSegments);
    }

    private async checkUntilInitialised(product: ProductResourceModel): Promise<void> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', product.tenantId);
        setTimeout(() => {
            this.productApiService.getById(product.id, params)
                .subscribe((updatedProduct: ProductResourceModel) => {
                    switch (updatedProduct.status) {
                        case ProductStatus.Initialising:
                            this.checkUntilInitialised(product);
                            return;
                        case ProductStatus.Initialised:
                            this.sharedAlertService.showToast('The new product "' + updatedProduct.name +
                                '" has finished initialising');
                            break;
                        case ProductStatus.InitialisationFailed:
                            this.eventService.getEntityUpdatedSubject('Product').next(updatedProduct);
                            throw Errors.General.Unexpected(
                                'The new product "' + updatedProduct.name +
                                '" has encountered an issue while initializing.');
                        default:
                            throw Errors.General.Unexpected(
                                'When waiting for a new product to initialise, an unknown status of '
                                + '"' + updatedProduct.status + '" was received.');
                    }
                    this.eventService.getEntityUpdatedSubject('Product').next(updatedProduct);
                });
        }, 1000);
    }

    private async initializeAdditionalPropertyInputFields(): Promise<void> {
        let productAdditionalProperties: Array<AdditionalPropertyDefinition> = !this.productAlias ?
            await this.additionalPropertiesService
                .getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextId(
                    this.tenantId,
                    AdditionalPropertyDefinitionContextType.Tenant,
                    EntityType.Product,
                    this.tenantAlias)
                .toPromise()
            : await this.additionalPropertiesService
                .getAdditionalPropertyDefinitionsByContextTypeAndEntityTypeAndContextIdAndParentContextId(
                    this.tenantId,
                    AdditionalPropertyDefinitionContextType.Product,
                    EntityType.Product,
                    this.productAlias,
                    this.tenantAlias,
                    true)
                .toPromise();
        this.productAdditionalPropertyInputFields =
            AdditionalPropertiesHelper.generateAdditionalPropertyInputFieldsForCreateForm(productAdditionalProperties);
    }
}
