import { Component, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ProductDeploymentSettingApiService } from '@app/services/api/product-deployment-setting.api.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { AppConfigService } from '@app/services/app-config.service';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { Subject } from 'rxjs';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { ProductFeatureSetting } from '@app/models/product-feature-setting';
import { ProductFeatureApiService } from '@app/services/api/product-feature-api.service';
import { ProductFeatureRenewalSetting } from '@app/models/product-feature-renewal-setting';
import { AppConfig } from '@app/models/app-config';
import { CustomValidators } from '@app/helpers/custom-validators';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { DateHelper } from '@app/helpers/date.helper';

/**
 * Export transaction type component class
 * This class is used to manage transaction type additional setting
 */
@Component({
    selector: 'app-deployment-setting',
    templateUrl: './renewal-transaction-type.page.html',
    styleUrls: [
        '../../../../assets/css/form-toolbar.scss',
        '../../../../assets/css/edit-form.scss',
        './renewal-transaction-type.page.scss',
    ],
})
export class TransactionTypePage extends DetailPage implements OnInit, OnDestroy {
    public productFeatureSetting: ProductFeatureSetting;
    public form: FormGroup;
    public formHasError: boolean = false;
    private tenantAlias: string;
    private productAlias: string;
    private tenantId: string;
    public headingEnvironment: string;
    public allowRenewalAfterExpiry: boolean;
    public expiredPolicyRenewalPeriodDays: number;

    public constructor(
        protected sharedLoaderService: SharedLoaderService,
        protected sharedAlertService: SharedAlertService,
        protected tenantApiService: TenantApiService,
        protected productDeploymentSettingService: ProductDeploymentSettingApiService,
        private productFeatureService: ProductFeatureSettingService,
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        private productFeatureApiService: ProductFeatureApiService,
        protected appConfigService: AppConfigService,
        protected tenantService: TenantService,
        protected productService: ProductService,
    ) {
        super(eventService, elementRef, injector);
        this.buildForm();
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantId = appConfig.portal.tenantId;
            }
        });
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');
        this.productAlias = this.routeHelper.getParam("productAlias");
        this.loadModel();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async loadModel(): Promise<void> {
        if (!this.tenantId) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        }

        this.productFeatureApiService.getProductFeature(this.tenantId, this.productAlias)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed),
            )
            .subscribe(
                (dt: ProductFeatureSetting) => {
                    this.productFeatureSetting = dt;
                    this.allowRenewalAfterExpiry = this.productFeatureSetting.allowRenewalAfterExpiry;
                    this.form.setValue({
                        allowRenewalAfterExpiry: this.productFeatureSetting.allowRenewalAfterExpiry,
                        expiredPolicyRenewalPeriodDays:
                        DateHelper.secondToDays(this.productFeatureSetting.expiredPolicyRenewalPeriodSeconds),
                    });
                    this.formHasError = false;
                },
                (err: any) => {
                    this.errorMessage = 'There was an error loading the renewal transaction type.';
                    throw err;
                },
            );
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.allowRenewalAfterExpiry != this.productFeatureSetting.allowRenewalAfterExpiry ||
            this.form.value.expiredPolicyRenewalPeriodDays !=
            DateHelper.secondToDays(this.productFeatureSetting.expiredPolicyRenewalPeriodSeconds)) {
            await this.sharedAlertService.showWithActionHandler({
                header: 'Unsaved Changes',
                subHeader: 'You have unsaved changes, are you sure you wish '
                    + 'to close the current view without saving them?',
                buttons: [
                    {
                        text: 'No',
                        handler: (): any => {
                            return;
                        },
                    },
                    {
                        text: 'Yes',
                        handler: (): any => {
                            this.returnToSettings();
                        },
                    },
                ],
            });
        } else {
            this.returnToSettings();
        }
    }

    public userDidTapSaveButton(value: any): void {
        if (!this.form.valid && this.allowRenewalAfterExpiry) {
            this.formHasError = true;
            return;
        }

        if (this.form.get('expiredPolicyRenewalPeriodDays').errors) {
            value.expiredPolicyRenewalPeriodDays = 0;
        }

        this.formHasError = false;
        if (this.productFeatureSetting) {
            this.update(value);
        }
    }

    private async update(value: any): Promise<void> {
        let model: ProductFeatureRenewalSetting = new ProductFeatureRenewalSetting();
        model.expiredPolicyRenewalPeriodSeconds = DateHelper.daysToSeconds(value.expiredPolicyRenewalPeriodDays);
        model.allowRenewalAfterExpiry = this.allowRenewalAfterExpiry;
        this.productFeatureApiService.updateProductFeatureRenewalSetting(this.tenantId, this.productAlias, model)
            .subscribe(
                () => {
                    this.productFeatureService.clearProductFeatureSettings();
                    this.returnToSettings();
                },
            );
    }

    protected buildForm(): void {
        this.form = this.formBuilder.group({
            allowRenewalAfterExpiry: [''],
            expiredPolicyRenewalPeriodDays: ['',
                [Validators.required,
                    CustomValidators.wholeNumber,
                    Validators.min(1),
                    Validators.max(365)]],
        });
    }

    public toggleAllowRenewalAfterExpiry(event: any): void {
        const enabled: any = event.detail.checked;
        this.allowRenewalAfterExpiry = enabled;
    }

    private returnToSettings(): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateBack(['product', this.productAlias], true, { queryParams: { segment: 'Settings' } });
        } else {
            this.navProxy.navigateBack(
                ['tenant',
                    this.tenantAlias,
                    'product',
                    this.productAlias],
                true,
                {
                    queryParams: {
                        segment: 'Settings',
                    },
                },
            );
        }
    }
}
