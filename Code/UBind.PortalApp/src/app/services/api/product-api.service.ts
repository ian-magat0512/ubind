import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { ConfigurationFileResourceModel } from '@app/resource-models/configuration-file.resource-model';
import {
    ProductResourceModel,
    ProductCreateRequestResourceModel,
    ProductUpdateRequestResourceModel,
} from '@app/resource-models/product.resource-model';
import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { ProductAssetSyncResultModel } from '@app/models/product-asset-sync-result.model';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { SortAndFilterByFieldName } from '@app/models/sort-filter-by.enum';
import { SortDirection } from '@app/viewmodels/sorted-entity.viewmodel';
import { ProductReleaseSettingsResourceModel } from '@app/resource-models/product-release-settings.resource-model';

/**
 * Export product API service class.
 * TODO: Write a better class header: product API functions.
 */
@Injectable({ providedIn: 'root' })
export class ProductApiService implements EntityLoaderService<ProductResourceModel> {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'product';
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<ProductResourceModel>> {
        return this.httpClient.get<Array<ProductResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<ProductResourceModel> {
        return this.httpClient.get<ProductResourceModel>(this.baseUrl + `/${id}`, ApiHelper.toHttpOptions(params));
    }

    public getProductsByTenantId(tenant: string): Observable<Array<ProductResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        params.set('sortBy', SortAndFilterByFieldName.DetailsName);
        params.set('sortOrder', SortDirection.Ascending);
        return this.getList(params);
    }

    public getByAlias(productAlias: string, tenant?: string): Observable<ProductResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<ProductResourceModel>(
            this.baseUrl + `/by-alias/${productAlias}`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getIdByAlias(productAlias: string, tenant?: string): Observable<string> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<string>(
            this.baseUrl + `/by-alias/${productAlias}/id`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getNameByAlias(productAlias: string, tenant?: string): Observable<string> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<string>(
            this.baseUrl + `/by-alias/${productAlias}/name`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getReleasesForProduct(
        product: string,
        tenant: string = null,
        pageParams?: Map<string, string | Array<string>>,
    ): Observable<Array<ReleaseResourceModel>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        if (pageParams) {
            params.set('page', pageParams.get('page'));
            params.set('pageSize', pageParams.get('pageSize'));
        }
        return this.httpClient.get<Array<ReleaseResourceModel>>(
            this.baseUrl + `/${product}/release`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getSourceFiles(
        product: string,
        tenant: string = null,
    ): Observable<Array<ConfigurationFileResourceModel>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<ConfigurationFileResourceModel>>(
            this.baseUrl + `/${product}/file`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public downloadSourceFile(product: string, formType: string, filePath: string, tenant: string): Observable<any> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get(
            this.baseUrl + `/${product}/${formType}/file/${filePath}`,
            { responseType: 'blob', params: ApiHelper.toHttpParams(params) });
    }

    public create(model: ProductCreateRequestResourceModel): Observable<ProductResourceModel> {
        return this.httpClient.post<ProductResourceModel>(this.baseUrl, model);
    }

    public update(product: string, model: ProductUpdateRequestResourceModel): Observable<ProductResourceModel> {
        return this.httpClient.put<ProductResourceModel>(this.baseUrl + `/${product}`, model);
    }

    public syncQuoteAssets(product: string, tenant: string = null): Observable<ProductAssetSyncResultModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.post<ProductAssetSyncResultModel>(
            this.baseUrl + `/${product}/quote/sync`, null, ApiHelper.toHttpOptions(params));
    }

    public syncClaimAssets(product: string, tenant: string = null): Observable<ProductAssetSyncResultModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.post<ProductAssetSyncResultModel>(
            this.baseUrl + `/${product}/claim/sync`, null, ApiHelper.toHttpOptions(params));
    }

    public getLatestSyncResult(
        product: string,
        tenant: string = null,
    ): Observable<ProductAssetSyncResultModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<ProductAssetSyncResultModel>(
            this.baseUrl + `/${product}/sync/result`, ApiHelper.toHttpOptions(params));
    }

    public hasClaimComponent(
        product: string,
        tenant: string = null,
    ): Observable<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<boolean>(this.baseUrl + `/${product}/has-claim`, ApiHelper.toHttpOptions(params));
    }

    public getProductReleaseSettings(
        tenant: string, product: string): Observable<ProductReleaseSettingsResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.get<ProductReleaseSettingsResourceModel>(
            `${this.baseUrl}/${product}/release-settings`,
            ApiHelper.toHttpOptions(params));
    }

    public updateProductReleaseSettings(
        product: string,
        model: ProductReleaseSettingsResourceModel): Observable<Response> {
        return this.httpClient.put<Response>(
            `${this.baseUrl}/${product}/release-settings`, model);
    }
}
