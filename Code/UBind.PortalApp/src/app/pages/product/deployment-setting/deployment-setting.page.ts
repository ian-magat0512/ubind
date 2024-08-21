import { Component, ViewChild, Injector, ElementRef, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { ProductDeploymentSettingApiService } from '@app/services/api/product-deployment-setting.api.service';
import { ProductDeploymentSettingResourceModel } from '@app/resource-models/product-deployment-setting.resource-model';
import { UrlHelper, StringHelper } from '@app/helpers';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { RouteHelper } from '../../../helpers/route.helper';
import { DetailPage } from '@app/pages/master-detail/detail.page';
import { EventService } from '@app/services/event.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { finalize, takeUntil } from 'rxjs/operators';
import { TenantApiService } from '@app/services/api/tenant-api.service';
import { FormHelper } from '@app/helpers/form.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { TenantService } from '@app/services/tenant.service';
import { ProductService } from '@app/services/product.service';
import { Subject } from 'rxjs';
import { SharedAlertService } from '@app/services/shared-alert.service';

/**
 * Export product deployment setting page component class
 * This class manage of setting up the deployment.
 */
@Component({
    selector: 'app-deployment-setting',
    templateUrl: './deployment-setting.page.html',
    styleUrls: [
        '../../../../assets/css/edit-form.scss',
    ],
})
export class ProductDeploymentSettingsPage extends DetailPage implements OnInit, OnDestroy {
    public deploymentSettingForm: FormGroup;
    public formHasError: boolean = false;
    public model: ProductDeploymentSettingResourceModel;
    private tenantId: string;
    private tenantAlias: string;
    private productAlias: string;
    public environment: string;
    public headingEnvironment: string;

    @ViewChild('urlsIonicTextArea') public urlsIonicTextArea: any;

    public constructor(
        protected sharedLoaderService: SharedLoaderService,
        protected sharedAlertService: SharedAlertService,
        protected tenantApiService: TenantApiService,
        protected productDeploymentSettingService: ProductDeploymentSettingApiService,
        public navProxy: NavProxyService,
        private routeHelper: RouteHelper,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
        private formHelper: FormHelper,
        protected appConfigService: AppConfigService,
        protected tenantService: TenantService,
        protected productService: ProductService,
    ) {
        super(eventService, elementRef, injector);
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantId = appConfig.portal.tenantId;
            }
        });
        this.buildForm();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.environment = this.routeHelper.getParam("environment");
        this.tenantAlias = this.routeHelper.getParam("tenantAlias");
        this.productAlias = this.routeHelper.getParam("productAlias");
        this.headingEnvironment = StringHelper.capitalizeFirstLetter(this.environment);
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

        this.productDeploymentSettingService.getById(this.tenantId, this.productAlias)
            .pipe(
                finalize(() => this.isLoading = false),
                takeUntil(this.destroyed),
            )
            .subscribe(
                (dt: ProductDeploymentSettingResourceModel) => {
                    this.model = dt;
                    this.formHasError = false;
                    this.deploymentSettingForm.setValue({
                        urls: dt[this.environment].join('\n'),
                    });

                    setTimeout(() => {
                        this.urlsIonicTextArea.setFocus();
                    }, 100);
                },
                (err: any) => {
                    this.errorMessage = 'There was an error loading the product deployment settings';
                    throw err;
                },
            );
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.model[this.environment].join('\n') != this.deploymentSettingForm.value.urls) {
            if (!await this.formHelper.confirmExitWithUnsavedChanges()) {
                return;
            }
        }
        this.returnToSettings();
    }

    public userDidTapSaveButton(value: any): void {
        if (!this.deploymentSettingForm.valid) {
            this.formHasError = true;
            return;
        }
        this.formHasError = false;
        if (this.model) {
            this.update(value);
        }
    }

    protected buildForm(): void {
        this.deploymentSettingForm = this.formBuilder.group({
            urls: ['', [Validators.minLength(0), Validators.maxLength(99999)]],
        });
    }

    private async update(value: any): Promise<void> {
        const model: ProductDeploymentSettingResourceModel = this.model;
        let urls: Array<any> = this.iterateInputThenToArray(value.urls);
        if (!urls) {
            return;
        }
        model[this.environment] = urls;
        await this.sharedLoaderService.presentWait();
        this.productDeploymentSettingService.update(this.tenantId, this.productAlias, model)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe((res: ProductDeploymentSettingResourceModel) => {
                let model: any = {
                    environment: this.environment,
                    urls: urls,
                };
                // notify change
                this.eventService.getEntityUpdatedSubject('deployment-settings').next(model);

                // show snackbar
                let message: string = 'Deployment targets for the ' + this.headingEnvironment +
                    ' environment were successfully updated.';
                this.sharedAlertService.showToast(message);
                this.returnToSettings();
            });
    }

    private iterateInputThenToArray(str: string): Array<any> {
        let tmpUrls: Array<string> = str.split('\n');
        let urls: Array<any> = [];
        for (let tmpUrl of tmpUrls) {
            let urlTrimmed: string = tmpUrl.trim();
            if (urlTrimmed) {
                if (UrlHelper.validateUrl(urlTrimmed)) {
                    urls.push(urlTrimmed);
                } else {
                    this.errorMessage = urlTrimmed + " is not a valid hostname.";
                    this.formHasError = true;
                    return null;
                }
            }
        }
        return urls;
    }

    private returnToSettings(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop(); // edit
        pathSegments.pop(); // environment
        pathSegments.pop(); // deployment-settings
        this.navProxy.navigateBack(pathSegments, true, { queryParams: { segment: 'Settings' } });
    }
}
