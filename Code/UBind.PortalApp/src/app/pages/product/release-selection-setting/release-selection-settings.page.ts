import { Component, ElementRef, Injector, OnDestroy, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { RouteHelper } from "@app/helpers/route.helper";
import { QuoteTypeNames } from "@app/models";
import { DetailPage } from "@app/pages/master-detail/detail.page";
import { ProductReleaseSettingsResourceModel } from "@app/resource-models/product-release-settings.resource-model";
import { DeploymentApiService } from "@app/services/api/deployment-api.service";
import { ProductApiService } from "@app/services/api/product-api.service";
import { EventService } from "@app/services/event.service";
import { LayoutManagerService } from "@app/services/layout-manager.service";
import { NavProxyService } from "@app/services/nav-proxy.service";
import { SharedAlertService } from "@app/services/shared-alert.service";
import { SharedLoaderService } from "@app/services/shared-loader.service";
import { Subject } from "rxjs";
import { finalize, takeUntil } from "rxjs/operators";
import { titleCase } from "title-case";

/**
 * Export tenant password expiry settings page component class.
 * This class manage setting up the tenant password expiry.
 */
@Component({
    selector: 'app-release-selection-settings',
    templateUrl: './release-selection-settings.page.html',
    styleUrls: [
        './release-selection-settings.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
})
export class ReleaseSelectionSettingsPage extends DetailPage implements OnInit, OnDestroy {
    public releaseSelectionSettingsForm: FormGroup;
    public errorMessage: string;
    public quoteType: string;
    public isUseDefaultConfig: boolean;
    private tenantAlias: string;
    private productAlias: string;
    private productReleaseSettings: ProductReleaseSettingsResourceModel;

    public constructor(
        public layoutManager: LayoutManagerService,
        public routeHelper: RouteHelper,
        public navProxy: NavProxyService,
        private formBuilder: FormBuilder,
        eventService: EventService,
        elementRef: ElementRef,
        injector: Injector,
        private productApiService: ProductApiService,
        protected sharedLoaderService: SharedLoaderService,
        protected sharedAlertService: SharedAlertService,
        private deploymentApiService: DeploymentApiService,
    ) {
        super(eventService, elementRef, injector);
        this.buildForm();
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();

        this.tenantAlias = this.routeHelper.getParam('tenantAlias')
            || this.routeHelper.getParam('portalTenantAlias');
        this.productAlias = this.routeHelper.getParam('productAlias');
        this.quoteType = this.routeHelper.getParam('quoteType');
        this.load();
    }

    public ngOnDestroy(): void {
        if (this.destroyed) {
            this.destroyed.next();
            this.destroyed.complete();
        }
    }

    protected buildForm(): void {
        this.releaseSelectionSettingsForm = this.formBuilder.group({
            ProductReleaseSelection: [null, [Validators.required]],
        });
    }

    public load(): void {
        this.isLoading = true;
        this.productApiService
            .getProductReleaseSettings(this.tenantAlias, this.productAlias)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false))
            .subscribe((productReleaseSettings: ProductReleaseSettingsResourceModel) => {
                this.productReleaseSettings = productReleaseSettings;
                let releaseSelectionValue: number = 0;
                if (this.quoteType == QuoteTypeNames.Adjustment.toLowerCase()) {
                    releaseSelectionValue = productReleaseSettings.doesAdjustmentUseDefaultProductRelease ? 1 : 0;
                } else if (this.quoteType == QuoteTypeNames.Cancellation.toLowerCase()) {
                    releaseSelectionValue = productReleaseSettings.doesCancellationUseDefaultProductRelease ? 1 : 0;
                }
                this.releaseSelectionSettingsForm.patchValue({
                    ProductReleaseSelection: releaseSelectionValue,
                });
            });
    }

    public async userDidTapCloseButton(value: any): Promise<void> {
        let valueChanged: boolean = this.quoteType == QuoteTypeNames.Adjustment.toLowerCase()
            ? (value.ProductReleaseSelection == 1) !=
            this.productReleaseSettings.doesAdjustmentUseDefaultProductRelease
            : (value.ProductReleaseSelection == 1) !=
            this.productReleaseSettings.doesCancellationUseDefaultProductRelease;

        if (valueChanged) {
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
        this.updateReleaseSelectionSettings(value);
    }

    public async updateReleaseSelectionSettings(value: any): Promise<void> {
        let adjustmentConfig: boolean = this.quoteType == QuoteTypeNames.Adjustment.toLowerCase()
            ? value.ProductReleaseSelection == 1
            : this.productReleaseSettings.doesAdjustmentUseDefaultProductRelease;
        let cancellationConfig: boolean = this.quoteType == QuoteTypeNames.Cancellation.toLowerCase()
            ? value.ProductReleaseSelection == 1
            : this.productReleaseSettings.doesCancellationUseDefaultProductRelease;
        const model: ProductReleaseSettingsResourceModel = {
            tenant: this.tenantAlias,
            doesAdjustmentUseDefaultProductRelease: adjustmentConfig,
            doesCancellationUseDefaultProductRelease: cancellationConfig,
        };

        await this.sharedLoaderService.presentWait();
        this.productApiService.updateProductReleaseSettings(this.productAlias, model)
            .pipe(
                finalize(() => this.sharedLoaderService.dismiss()),
                takeUntil(this.destroyed))
            .subscribe((res: any) => {
                let message: string;
                if (value.ProductReleaseSelection == 0) {
                    message = `${titleCase(this.quoteType)} quotes in the staging and production `
                        + `environments will now use the product release associated with the applicable policy period`;
                } else {
                    message = `${titleCase(this.quoteType)} quotes will use the default product release for the `
                        + `applicable environment`;
                }
                this.sharedAlertService.showToast(message);
                this.returnToSettings();
            });
    }

    private returnToSettings(): void {
        let pathSegments: Array<string> = this.routeHelper.getPathSegments();
        pathSegments.pop();
        this.navProxy.navigateBack(pathSegments,
            true, {
                queryParams: {
                    segment: 'Settings',
                },
            },
        );
    }
}
