import { Component, ElementRef, Injector, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AlertController, LoadingController } from '@ionic/angular';
import { finalize, takeUntil } from 'rxjs/operators';
import { QuoteExpiryUpdateType } from '@app/resource-models/product.resource-model';
import { NavProxyService } from '@app/services/nav-proxy.service';
import { RouteHelper } from '@app/helpers/route.helper';
import { AppConfigService } from '@app/services/app-config.service';
import { LayoutManagerService } from '@app/services/layout-manager.service';
import { AppConfig } from '@app/models/app-config';
import { EventService } from '@app/services/event.service';
import { Subject } from 'rxjs';
import { DetailPage } from '../../master-detail/detail.page';
import { SharedAlertService } from '@app/services/shared-alert.service';
import { CustomValidators } from '@app/helpers/custom-validators';
import { RefundSettingsModel } from '@app/models/refund-settings.model';
import { ProductFeatureApiService } from '@app/services/api/product-feature-api.service';
import { ProductFeatureSetting } from '@app/models/product-feature-setting';
import { TenantService } from '@app/services/tenant.service';
import { ProductFeatureSettingService } from '@app/services/product-feature-service';
import { RefundRule } from '@app/models/refund-rule.enum';
import { PeriodWhichNoClaimsMade } from '@app/models/refund-settings-period.enum';
import { scrollbarStyle } from '@assets/scrollbar';

/**
 * Export cancellation setting page component class.
 * TODO: This class is used to set cancellation setting.
 */
@Component({
    selector: 'refund-settings',
    templateUrl: './refund-settings.page.html',
    styleUrls: [
        './refund-settings.page.scss',
        '../../../../assets/css/form-toolbar.scss',
    ],
    styles: [scrollbarStyle],
})
export class RefundSettingsPage extends DetailPage implements OnInit, OnDestroy {
    public form: FormGroup;
    public formHasError: boolean = false;
    public model: ProductFeatureSetting;
    public productAlias: string;
    private tenantId: string;
    private defaultLastNumberOfYearsWhichNoClaimsMade: number = 5;
    public tenantAlias: string;
    public quoteExpiryUpdateType: QuoteExpiryUpdateType;
    public selectedQuoteExpiryUpdateType: QuoteExpiryUpdateType;
    public newEnabled: boolean = false;
    public isLoading: boolean = false;
    public isRefundAreProvidedIfNoClaimsWereMade: boolean = false;
    public isLastNumberOfYearsSelected: boolean = false;
    public periodWhichNoClaimsMade: any = PeriodWhichNoClaimsMade;
    public refundRule: any = RefundRule;

    public constructor(
        private routeHelper: RouteHelper,
        protected loadCtrl: LoadingController,
        protected alertCtrl: AlertController,
        private sharedAlertService: SharedAlertService,
        protected productFeatureApiService: ProductFeatureApiService,
        protected productFeatureSettingService: ProductFeatureSettingService,
        public navProxy: NavProxyService,
        private formBuilder: FormBuilder,
        public layoutManager: LayoutManagerService,
        protected appConfigService: AppConfigService,
        eventService: EventService,
        elementRef: ElementRef,
        protected tenantService: TenantService,
        public injector: Injector,
    ) {
        super(eventService, elementRef, injector);
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.tenantId = appConfig.portal.tenantId;
            }
        });

    }

    public ngOnInit(): void {
        this.buildForm();
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
        if (!this.tenantId) {
            this.tenantId = await this.tenantService.getTenantIdFromAlias(this.tenantAlias);
        }

        this.productFeatureApiService.getProductFeature(this.tenantId, this.productAlias)
            .pipe(
                takeUntil(this.destroyed),
                finalize(() => this.isLoading = false),
            ).subscribe(
                (productFeature: ProductFeatureSetting) => {
                    this.model = productFeature;
                    const refundPolicy: number = RefundRule[this.model.refundPolicy];
                    const periodWhichNoClaimsMade: number =
                        Number(PeriodWhichNoClaimsMade[this.model.periodWhichNoClaimsMade]);
                    this.isRefundAreProvidedIfNoClaimsWereMade =
                        refundPolicy == RefundRule.RefundsAreProvidedIfNoClaimsWereMade;
                    this.isLastNumberOfYearsSelected =
                        periodWhichNoClaimsMade == PeriodWhichNoClaimsMade.LastNumberOfYears;
                    this.form.patchValue({
                        RefundPolicy: refundPolicy,
                        PeriodWhichNoClaimsMade: !Number.isNaN(periodWhichNoClaimsMade) ?
                            periodWhichNoClaimsMade : PeriodWhichNoClaimsMade.CurrentPolicyPeriod,
                        LastNumberOfYearsWhichNoClaimsMade: this.model.lastNumberOfYearsWhichNoClaimsMade != null
                            ? this.model.lastNumberOfYearsWhichNoClaimsMade.toString() :
                            this.defaultLastNumberOfYearsWhichNoClaimsMade,
                    });
                });
    }

    public async userDidTapCloseButton(): Promise<void> {
        if (this.form.value.RefundPolicy != this.model.refundPolicy ||
            this.form.value.periodWhichNoClaimsMade != this.model.periodWhichNoClaimsMade ||
            this.form.value.lastNumberOfYearsWhichNoClaimsMade != this.model.lastNumberOfYearsWhichNoClaimsMade) {
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

    public userDidTapApplyButton(value: any): void {
        if (!this.form.valid) {
            this.formHasError = true;
            return;
        }
        this.formHasError = false;
        if (this.model) {
            this.update(value);
        }
    }

    protected buildForm(): void {
        this.form = this.formBuilder.group({
            RefundPolicy: [null, [Validators.required]],
            PeriodWhichNoClaimsMade: [null, []],
            LastNumberOfYearsWhichNoClaimsMade: [null,
                [Validators.required, CustomValidators.wholeNumber, Validators.min(1), Validators.max(100)]],
        });
    }

    public ruleSelectionChange(event: any): void {
        this.isRefundAreProvidedIfNoClaimsWereMade =
            event.value == RefundRule.RefundsAreProvidedIfNoClaimsWereMade;
    }

    public periodSelectionChange(event: any): void {
        this.isLastNumberOfYearsSelected = event.value == PeriodWhichNoClaimsMade.LastNumberOfYears;
        this.form.controls['LastNumberOfYearsWhichNoClaimsMade']
            .setValidators(this.isLastNumberOfYearsSelected ? [
                Validators.required,
                CustomValidators.wholeNumber,
                Validators.min(1),
                Validators.max(100),
            ] : []);
        this.form.get('LastNumberOfYearsWhichNoClaimsMade').updateValueAndValidity();
    }

    public applyUpdateSelection(event: any): void {
        this.selectedQuoteExpiryUpdateType = event.detail.value;
    }

    private async update(value: any): Promise<void> {
        let periodWhichNoClaimsMade: any = value.RefundPolicy ==
            RefundRule.RefundsAreProvidedIfNoClaimsWereMade ? value.PeriodWhichNoClaimsMade : '';

        let lastNumberOfYearsWhichNoClaimsMade: any =
            periodWhichNoClaimsMade == PeriodWhichNoClaimsMade.LastNumberOfYears ?
                value.LastNumberOfYearsWhichNoClaimsMade : '';

        let model: RefundSettingsModel = {
            refundPolicy: value.RefundPolicy,
            periodWhichNoClaimsMade: periodWhichNoClaimsMade,
            lastNumberOfYearsWhichNoClaimsMade: lastNumberOfYearsWhichNoClaimsMade,
        };

        this.productFeatureApiService.updateProductFeatureRefundSettings(
            this.tenantId,
            this.productAlias,
            model)
            .subscribe(
                (res: any) => {
                    this.sharedAlertService.showToast(
                        `Cancellation refund rules updated for ${this.productAlias}`);
                    this.returnToSettings();
                });
    }

    private returnToSettings(): void {
        const pathSegmentsAfter: Array<string> = this.routeHelper.getPathSegmentsAfter('path');
        if (pathSegmentsAfter[0] === 'product') {
            this.navProxy.navigateBack([
                'product',
                this.productAlias],
            true, {
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
