import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { ProductFeatureSetting } from '@app/models/product-feature-setting';
import { ApiService } from '../api.service';
import { ApiHelper } from '@app/helpers';
import { ProductFeatureRenewalSetting } from '@app/models/product-feature-renewal-setting';
import { RefundSettingsModel } from '@app/models/refund-settings.model';

/**
 * Export product feature Api Service.
 * This class manage the API services of Product feature.
 */
@Injectable({ providedIn: 'root' })
export class ProductFeatureApiService {

    private productFeatureUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
        public apiService: ApiService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.productFeatureUrl = `${appConfig.portal.api.baseUrl}product-feature-setting`;
        });
    }

    public getProductFeature(tenant: string, product: string): Observable<ProductFeatureSetting> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.get<ProductFeatureSetting>(
            `${this.productFeatureUrl}/${product}`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public updateProductFeatureRenewalSetting(
        tenant: string,
        product: string,
        model: ProductFeatureRenewalSetting,
    ):
        Observable<void> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.patch<void>(
            `${this.productFeatureUrl}/${product}/feature/renewal/additional-settings/`,
            model,
            ApiHelper.toHttpOptions(params),
        );
    }

    public updateProductFeatureRefundSettings(
        tenant: string,
        product: string,
        model: RefundSettingsModel,
    ):
        Observable<void> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.patch<void>(
            `${this.productFeatureUrl}/${product}/refund-settings/`,
            model,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getTenantProductFeatures(tenant: string): Observable<Array<ProductFeatureSetting>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<ProductFeatureSetting>>(
            this.productFeatureUrl,
            ApiHelper.toHttpOptions(params),
        );
    }

    public disable(tenant: string, product: string, productFeatureType: string): Observable<ProductFeatureSetting> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.patch<ProductFeatureSetting>(
            `${this.productFeatureUrl}/${product}/feature/${productFeatureType}/disable/`,
            null,
            ApiHelper.toHttpOptions(params),
        );
    }

    public enable(tenant: string, product: string, productFeatureType: string): Observable<ProductFeatureSetting> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<ProductFeatureSetting>(
            `${this.productFeatureUrl}/${product}/feature/${productFeatureType}/enable/`,
            null,
            ApiHelper.toHttpOptions(params),
        );
    }
}
