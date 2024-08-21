import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { TenantResourceModel } from '@app/resource-models/tenant.resource-model';
import { TenantSessionSettingResourceModel } from '@app/resource-models/tenant-session-settings.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { TenantPasswordExpirySettingResourceModel }
    from '@app/resource-models/tenant-password-expiry-settings.resource-model';

/**
 * Export tenant API service class.
 * TODO: Write a better class header: tenant API functions.
 */
@Injectable({ providedIn: 'root' })
export class TenantApiService implements EntityLoaderService<TenantResourceModel> {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'tenant';
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<TenantResourceModel>> {
        return this.httpClient.get<Array<TenantResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<TenantResourceModel> {
        return this.httpClient.get<TenantResourceModel>(this.baseUrl + `/${id}`, ApiHelper.toHttpOptions(params));
    }

    public get(tenant: string): Observable<TenantResourceModel> {
        return this.httpClient.get<TenantResourceModel>(this.baseUrl + `/${tenant}`);
    }

    public getTenantId(tenant: string): Observable<string> {
        return this.httpClient.get<string>(this.baseUrl + `/${tenant}/id`);
    }

    public getTenantName(tenant: string): Observable<TenantResourceModel> {
        return this.httpClient.get<TenantResourceModel>(this.baseUrl + `/${tenant}/name`);
    }

    public getTenantAlias(tenant: string): Observable<TenantResourceModel> {
        return this.httpClient.get<TenantResourceModel>(this.baseUrl + `/${tenant}/alias`);
    }

    public create(model: TenantResourceModel): Observable<TenantResourceModel> {
        return this.httpClient.post<TenantResourceModel>(this.baseUrl, model);
    }

    public update(tenant: string, model: TenantResourceModel): Observable<TenantResourceModel> {
        return this.httpClient.put<TenantResourceModel>(this.baseUrl + `/${tenant}`, model);
    }

    public disable(tenant: string): Observable<TenantResourceModel> {
        return this.httpClient.patch<TenantResourceModel>(this.baseUrl + `/${tenant}/disable`, null);
    }

    public enable(tenant: string): Observable<TenantResourceModel> {
        return this.httpClient.patch<TenantResourceModel>(this.baseUrl + `/${tenant}/enable`, null);
    }

    public delete(tenant: string): Observable<Response> {
        return this.httpClient.delete<Response>(this.baseUrl + `/${tenant}`);
    }

    public getSessionSettings(tenant: string): Observable<TenantSessionSettingResourceModel> {
        return this.httpClient.get<TenantSessionSettingResourceModel>(
            this.baseUrl + `/${tenant}/session-settings`,
        );
    }

    public updateSessionSettings(tenant: string, model: TenantSessionSettingResourceModel): Observable<Response> {
        return this.httpClient.put<Response>(this.baseUrl + `/${tenant}/session-settings`, model);
    }

    public getTenantForLoggedInUser(): Observable<TenantResourceModel> {
        return this.httpClient.get<TenantResourceModel>(this.baseUrl + 'for-logged-in-user');
    }

    public getPasswordExpirySettings(tenant: string): Observable<TenantPasswordExpirySettingResourceModel> {
        return this.httpClient.get<TenantPasswordExpirySettingResourceModel>(
            this.baseUrl + `/${tenant}/password-expiry-settings`,
        );
    }

    public updatePasswordExpirySettings(
        tenant: string,
        model: TenantPasswordExpirySettingResourceModel,
    ): Observable<Response> {
        return this.httpClient.put<Response>(this.baseUrl + `/${tenant}/password-expiry-settings`, model);
    }
}
