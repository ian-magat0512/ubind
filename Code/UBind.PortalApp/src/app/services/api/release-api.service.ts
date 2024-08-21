import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiHelper } from '@app/helpers/api.helper';
import { ConfigurationFileResourceModel, ReleaseType } from '@app/models';
import { ReleaseResourceModel } from '@app/resource-models/release.resource-model';
import { ReleaseRequestModel } from '@app/models/release-request.model';
import { EntityLoaderService } from '../entity-loader.service';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import {
    QuotePolicyTransactionCountResourceModel,
} from '@app/resource-models/quote-policy-transaction-count.resource-model';
import { ReleaseUpsertModel } from '@app/resource-models/release-upsert.model';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';

/**
 * Export release API service class.
 * TODO: Write a better class header: release API functions.
 */
@Injectable({ providedIn: 'root' })
export class ReleaseApiService implements EntityLoaderService<ReleaseResourceModel> {
    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<ReleaseResourceModel>> {
        const product: any = params.get('product');
        if (!product) {
            throw new Error("You must ensure the product Id or Alias is set in the params"
                + " when getting a list of releases.");
        }
        return this.httpClient.get<Array<ReleaseResourceModel>>(
            this.baseUrl + `product/${product}/release`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<ReleaseResourceModel> {
        return this.httpClient.get<ReleaseResourceModel>(
            this.baseUrl + `release/${id}`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getSourceFiles(releaseId: string, tenant: string): Observable<Array<ConfigurationFileResourceModel>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<ConfigurationFileResourceModel>>(
            this.baseUrl + `release/${releaseId}/file`,
            ApiHelper.toHttpOptions(params));
    }

    public downloadSourceFile(releaseId: string, formType: string, filePath: string, tenant: string): Observable<any> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get(
            this.baseUrl + `release/${releaseId}/${formType}/file/${filePath}`,
            { responseType: 'blob', params: ApiHelper.toHttpParams(params) });
    }

    public create(
        tenant: string,
        product: string,
        description: string,
        type: ReleaseType,
    ): Observable<ReleaseResourceModel> {
        const model: ReleaseUpsertModel = {
            tenant: tenant,
            product: product,
            description: description,
            type: type,
        };
        return this.httpClient.post<ReleaseResourceModel>(this.baseUrl + 'release', model).pipe(
            map((res: ReleaseResourceModel) => res));
    }

    public update(releaseId: string, model: ReleaseRequestModel): Observable<ReleaseResourceModel> {
        return this.httpClient.put<ReleaseResourceModel>(this.baseUrl + `release/${releaseId}`, model);
    }

    public restoreToDevelopment(releaseId: string, tenant: string): Observable<ReleaseResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.post<ReleaseResourceModel>(
            this.baseUrl + `release/${releaseId}/restore`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public migrateQuotesAndPolicyTransactionsToRelease(
        releaseId: string,
        newReleaseId: string,
        environment: string,
        tenant: string,
    ): Observable<any> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        params.set('environment', environment);
        return this.httpClient.post<any>(
            this.baseUrl + `release/${releaseId}/quote-policy-transaction/migrate/${newReleaseId}`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public migrateUnassociatedEntitiesToRelease(
        newReleaseId: string,
        environment: DeploymentEnvironment,
        tenant: string,
        product: string,
    ): Observable<any> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        if (product) {
            params.set('product', product);
        }
        params.set('environment', environment);
        return this.httpClient.post<any>(
            `${this.baseUrl}release/unassociated/quote-policy-transaction/migrate/${newReleaseId}`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public getQuoteAndPolicyTransactionCount(
        releaseId: string,
        environment: string,
        tenant: string,
    ): Observable<QuotePolicyTransactionCountResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        params.set('environment', environment);
        const url: string = this.baseUrl + `release/${releaseId}/quote-policy-transaction/count`;
        return this.httpClient.get<QuotePolicyTransactionCountResourceModel>(url, ApiHelper.toHttpOptions(params));
    }

    public getUnassociatedQuoteAndPolicyTransactionCount(
        product: string,
        environment: string,
        tenant: string,
    ): Observable<QuotePolicyTransactionCountResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        params.set('product', product);
        params.set('environment', environment);
        const url: string = this.baseUrl + `release/unassociated/quote-policy-transaction/count`;
        return this.httpClient.get<QuotePolicyTransactionCountResourceModel>(url, ApiHelper.toHttpOptions(params));
    }
}
