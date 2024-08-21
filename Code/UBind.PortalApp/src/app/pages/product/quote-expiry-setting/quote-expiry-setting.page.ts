import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AlertController, LoadingController } from '@ionic/angular';
import { finalize, takeUntil } from 'rxjs/operators';
import {
    QuoteExpiryUpdateType, ProductResourceModel, ProductUpdateRequestResourceModel,
} from '@app/resource-models/product.resource-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { ProductApiService } from '@app/services/api/product-api.service';
import { AppConfigService } from '@app/services/app-config.service';
import { SharedLoaderService } from '@app/services/shared-loader.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { AppConfig } from '@app/models/app-config';
import { EventService } from '@app/services/event.service';
import { Subject } from 'rxjs';
import { DetailPage } from '../../master-detail/detail.page';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { CustomValidators } from '@app/helpers/custom-validators';

/**
 * Export product quote expiry setting page component class.
 * TODO: Write a better class header: setting up the quote expiry.
 */
@Component({
    selector: 'app-quote-expiry-setting',
    templateUrl: './quote-expiry-setting.page.html',
    styleUrls: [
        './quote-expiry-setting.page.scss',
        '../../../../assets/css/form-toolbar.scss',
        '../../../../assets/css/edit-form.scss',
    ],
})
export class ProductQuoteExpirySettingsPage extends DetailPage implements OnInit, OnDestroy {
    public form: FormGroup;
    public formHasError: boolean = false;
    public model: ProductResourceModel;
    public productAlias: string;
    public tenantAlias: string;
    public quoteExpiryUpdateType: QuoteExpiryUpdateType;
    public selectedQuoteExpiryUpdateType: QuoteExpiryUpdateType;
    public newEnabled: boolean = false;
    public isLoading: boolean = false;

    public constructor(
        private routeHelper: RouteHelper,
        protected loadCtrl: LoadingController,
        protected alertCtrl: AlertController,
        private sharedAlertService: SharedAlertService,
        protected productService: ProductApiService,
        public navProxy: NavProxyService,
        private formBuilder: FormBuilder,
        private sharedLoaderService: SharedLoaderService,
        public layoutManager: LayoutManagerService,
        protected appConfigService: AppConfigService,
        eventService: EventService,
        elementRef: ElementRef,
        public injector: Injector,
    ) {
        super(eventService, elementRef, injector);
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantAlias = appConfig.portal.tenantAlias;
            }
        });
        this.buildForm();
        this.selectedQuoteExpiryUpdateType = QuoteExpiryUpdateType.UpdateNone;
    }

    public ngOnInit(): void {
        this.destroyed = new Subject<void>();
        this.tenantAlias = this.routeHelper.getParam('tenantAlias');
        this.productAlias = this.routeHelper.getParam('productAlias');
        this.loadModel();
    }

    public ngOnDestroy(): void {
        this.destroyed.next();
        this.destroyed.complete();
    }

    public async loadModel(): Promise<void> {
        this.isLoading = true;
        this.productService.getByAlias(this.productAlias, this.tenantAlias)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            ).subscribe(
                (dt: ProductResourceModel) => {
                    this.model = dt;
                    this.newEnabled = this.model.quoteExpirySettings.enabled;
                    this.form.setValue({
                        expiryDays: this.model.quoteExpirySettings.expiryDays,
                        enabled: this.model.quoteExpirySettings.enabled,
                    });
                });
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.form.value.expiryDays != this.model.quoteExpirySettings.expiryDays ||
            this.newEnabled != this.model.quoteExpirySettings.enabled) {
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
        if (!this.newEnabled && !this.model.quoteExpirySettings.enabled) {
            return this.returnToSettings();
        }
        if (!this.form.valid && this.newEnabled) {
            this.formHasError = true;
            return;
        }
        this.formHasError = false;
        if (this.model && this.model.id) {
            this.update(value);
        }
    }

    protected buildForm(): void {
        this.form = this.formBuilder.group({
            expiryDays: ['', [
                CustomValidators.wholeNumber,
                Validators.min(1),
                Validators.max(365),
                Validators.required]],
            enabled: [''],
        });
    }

    public quoteExpiryActiveStateChange(event: any): void {
        const enabled: any = event.detail.checked;
        this.newEnabled = enabled;
        if (enabled) {
            this.selectedQuoteExpiryUpdateType = QuoteExpiryUpdateType.UpdateAllWithoutExpiryOnly;
        } else {
            this.selectedQuoteExpiryUpdateType = QuoteExpiryUpdateType.UpdateNone;
        }
    }

    public applyUpdateSelection(event: any): void {
        this.selectedQuoteExpiryUpdateType = event.value;
    }

    private async update(value: any): Promise<void> {
        // deep clone.
        let model: ProductUpdateRequestResourceModel = new ProductUpdateRequestResourceModel(this.model);
        model.quoteExpirySettings.enabled = this.newEnabled;
        model.quoteExpirySettings.expiryDays = this.newEnabled
            ? value.expiryDays
            : model.quoteExpirySettings.expiryDays;
        model.quoteExpirySettings.updateType = this.newEnabled ?
            this.selectedQuoteExpiryUpdateType : QuoteExpiryUpdateType.UpdateNone;
        let loadMessage: string = this.selectedQuoteExpiryUpdateType == QuoteExpiryUpdateType.UpdateNone
            ? 'Please wait...'
            : 'Updating quote expiry dates...';

        await this.sharedLoaderService.present(loadMessage);
        this.productService.update(this.productAlias, model)
            .pipe(finalize(() => this.sharedLoaderService.dismiss()))
            .subscribe(
                (res: ProductResourceModel) => {
                    const data: any = {
                        enabled: model.quoteExpirySettings.enabled,
                        expiryDays: model.quoteExpirySettings.expiryDays,
                    };
                    // notify change
                    this.eventService.getEntityUpdatedSubject('quote-expiry-settings').next(data);
                    this.returnToSettings();
                });
    }

    private returnToSettings(): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateBack([
                'product',
                this.productAlias],
            true,
            {
                queryParams: {
                    segment: 'Settings',
                },
            });
        } else {
            this.navProxy.navigateBack([
                'tenant',
                this.tenantAlias,
                'product',
                this.productAlias],
            true,
            {
                queryParams: {
                    segment: 'Settings',
                },
            });
        }
    }
}
