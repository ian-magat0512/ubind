import { Injectable } from "@angular/core";
import { AppConfig } from "@app/models/app-config";
import { ProductFeatureSetting } from "@app/models/product-feature-setting";
import { ApiService } from "./api.service";
import { ProductFeatureApiService } from "./api/product-feature-api.service";
import { AppConfigService } from "./app-config.service";
import { SharedAlertService } from "./shared-alert.service";
import { DateHelper } from "@app/helpers/date.helper";

/**
 * Export Product feature setting service class.
 * This class manage the setting of the product feature service.
 * 
 * It caches the value for 60 seconds and loads lazily.
 */
@Injectable({ providedIn: 'root' })
export class ProductFeatureSettingService {

    private static maximumAgeSeconds: number = 60;
    private productFeatureSettings: Array<ProductFeatureSetting>;
    private lastLoadedTimestamp: number = 0;
    private tenantId: string;

    public constructor(
        private productFeatureApiService: ProductFeatureApiService,
        private sharedAlertService: SharedAlertService,
        public apiService: ApiService,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (appConfig.portal.tenantName) {
                this.tenantId = appConfig.portal.tenantId;
                this.productFeatureSettings = null;
            }
        });
    }

    private async loadProductFeatureSettings(): Promise<void> {
        this.productFeatureSettings = await this.productFeatureApiService.getTenantProductFeatures(this.tenantId)
            .toPromise();
    }

    public async getProductFeatureSettings(): Promise<Array<ProductFeatureSetting>> {
        let now: number = Date.now();
        let lastLoadedAgeSeconds: number = (now - this.lastLoadedTimestamp) / 1000;
        let hasExpired: boolean = lastLoadedAgeSeconds > ProductFeatureSettingService.maximumAgeSeconds;
        if (!this.productFeatureSettings || hasExpired) {
            this.lastLoadedTimestamp = now;
            await this.loadProductFeatureSettings();
        }
        return this.productFeatureSettings;
    }

    public async anyProductHasNewBusinessQuoteFeature(): Promise<boolean> {
        return (await this.getProductFeatureSettings()).filter(
            (q: ProductFeatureSetting) => q.productId && q.areNewBusinessQuotesEnabled).length > 0;
    }

    public async anyProductCanCreateStandaloneClaim(): Promise<boolean> {
        return (await this.getProductFeatureSettings()).filter(
            (q: ProductFeatureSetting) =>
                q.productId && q.isClaimsEnabled && !q.mustCreateClaimAgainstPolicy,
        ).length > 0;
    }

    public async productHasRenewFeature(productId: string): Promise<boolean> {
        return (await this.getProductFeatureSettings()).filter(
            (q: ProductFeatureSetting) => q.productId == productId && q.areRenewalQuotesEnabled).length > 0;
    }

    public async getProductFeatureSetting(productId: string): Promise<ProductFeatureSetting> {
        const productFeatureSettings: Array<ProductFeatureSetting> = (await this.getProductFeatureSettings()).filter(
            (q: ProductFeatureSetting) => q.productId === productId,
        );
        if (productFeatureSettings.length > 0) {
            return productFeatureSettings[0];
        } else {
            this.sharedAlertService.showToast(`There is no product feature for product ${productId}`);
        }
    }

    public async isAllowedForRenewalAfterExpiry(
        productId: string,
        numberOfDaysToExpire: number,
    ): Promise<boolean> {
        const productFeature: ProductFeatureSetting = await this.getProductFeatureSetting(productId);

        let numberOfDaysToExpireIsWithInRenewalPeriodSetting: boolean = numberOfDaysToExpire <= 0 &&
            (productFeature && DateHelper.secondToDays(productFeature.expiredPolicyRenewalPeriodSeconds)
                >= Math.abs(numberOfDaysToExpire));

        return productFeature.areRenewalQuotesEnabled && productFeature.allowRenewalAfterExpiry
        && numberOfDaysToExpireIsWithInRenewalPeriodSetting;
    }

    public async hasNewBusinessFeature(productId: string): Promise<boolean> {
        const productFeatures: Array<ProductFeatureSetting> = (await this.getProductFeatureSettings()).filter(
            (q: ProductFeatureSetting) => q.productId === productId,
        );
        if (productFeatures.length > 0) {
            return productFeatures[0].areNewBusinessQuotesEnabled;
        }
        return false;
    }

    public clearProductFeatureSettings(): void {
        this.productFeatureSettings = null;
    }
}
