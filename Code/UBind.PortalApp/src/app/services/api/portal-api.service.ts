import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { FeatureSettingResourceModel } from '@app/resource-models/feature-setting.resource-model';
import {
    PortalDetailResourceModel, PortalResourceModel, PortalRequestModel,
} from '@app/resource-models/portal/portal.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { map } from 'rxjs/operators';
import { PortalStyleSettingsResourceModel } from '@app/resource-models/portal/portal-style-settings.resource-model';
import { PortalSignInMethodResourceModel } from '@app/resource-models/portal/portal-sign-in-method.resource-model';
import { PortalLoginMethodResourceModel } from '@app/resource-models/portal/portal-login-method.resource-model';

/**
 * Export portal API service class.
 * TODO: Write a better class header: portal API functions.
 */
@Injectable({ providedIn: 'root' })
export class PortalApiService implements EntityLoaderService<PortalResourceModel> {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'portal';
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<PortalResourceModel>> {
        return this.httpClient.get<Array<PortalResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<PortalDetailResourceModel> {
        return this.httpClient.get<PortalDetailResourceModel>(this.baseUrl + `/${id}`, ApiHelper.toHttpOptions(params));
    }

    public getPortals(tenant: string, organisation: string): Observable<Array<PortalResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        params.set('organisation', organisation);
        return this.getList(params);
    }

    public getActivePortals(
        tenant: string,
        organisation: string,
        userType?: string,
    ): Observable<Array<PortalResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        params.set('organisation', organisation);
        params.set('status', 'Active');
        if (userType) {
            params.set('userType', userType);
        }
        return this.getList(params);
    }

    public create(model: PortalRequestModel): Observable<PortalResourceModel> {
        return this.httpClient.post<PortalResourceModel>(this.baseUrl, model);
    }

    public update(portalId: string, model: PortalRequestModel): Observable<PortalResourceModel> {
        return this.httpClient.put<PortalResourceModel>(this.baseUrl + `/${portalId}`, model);
    }

    public disable(portal: string, tenant?: string): Observable<PortalDetailResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<PortalDetailResourceModel>(
            this.baseUrl + `/${portal}/disable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public enable(portal: string, tenant?: string): Observable<PortalDetailResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<PortalDetailResourceModel>(
            this.baseUrl + `/${portal}/enable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public delete(portal: string, tenant?: string): Observable<Response> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.delete<Response>(
            this.baseUrl + `/${portal}`,
            ApiHelper.toHttpOptions(params));
    }

    public getDefaultPortalFeatures(): Observable<Array<FeatureSettingResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        return this.httpClient.get<Array<FeatureSettingResourceModel>>(
            this.baseUrl + '/default/feature', ApiHelper.toHttpOptions(params));
    }

    public getPortalFeatures(portal: string, tenant: string): Observable<Array<FeatureSettingResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<FeatureSettingResourceModel>>(
            this.baseUrl + `/${portal}/feature`, ApiHelper.toHttpOptions(params));
    }

    public updatePortalLocation(
        portalId: string,
        environment: string,
        url: string,
        tenant?: string,
    ): Observable<PortalResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<PortalResourceModel>(
            this.baseUrl + `/${portalId}/url/${environment}`, url, ApiHelper.toHttpOptions(params));
    }

    public updatePortalStyles(
        portalId: string,
        model: PortalStyleSettingsResourceModel,
    ): Observable<PortalResourceModel> {
        return this.httpClient.patch<PortalResourceModel>(
            this.baseUrl + `/${portalId}/styles`, model);
    }

    public setAsDefault(portal: string, isDefault: boolean, tenant?: string): Observable<PortalResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<PortalResourceModel>(
            this.baseUrl + `/${portal}/default`, isDefault, ApiHelper.toHttpOptions(params));
    }

    public entitySettings(tenant: string, portal: string): Observable<any> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.get<any>(
            `${this.baseUrl}/${portal}/entity-settings`,
            ApiHelper.toHttpOptions(params));
    }

    public canCustomersSelfRegister(tenant: string, portal: string): Observable<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.get<boolean>(
            `${this.baseUrl}/${portal}/entity-settings/customer-self-account-creation`,
            ApiHelper.toHttpOptions(params));
    }

    public enableCustomerSelfAccountCreationSetting(tenant: string, portalId: string): Observable<any> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.patch(
            `${this.baseUrl}/${portalId}/entity-settings/customer-self-account-creation/enable`,
            null,
            ApiHelper.toHttpOptions(params)).pipe(map((res: Response) => res));
    }

    public disableCustomerSelfAccountCreationSetting(tenant: string, portalId: string): Observable<any> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.patch(
            `${this.baseUrl}/${portalId}/entity-settings/customer-self-account-creation/disable`,
            null,
            ApiHelper.toHttpOptions(params)).pipe(
            map((res: Response) => res),
        );
    }

    public getSignInMethods(portal: string, tenant?: string): Observable<Array<PortalSignInMethodResourceModel>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<PortalSignInMethodResourceModel>>(
            `${this.baseUrl}/${portal}/sign-in-method`,
            ApiHelper.toHttpOptions(params));
    }

    public disableSignInMethod(
        portal: string,
        authenticationMethodId: string,
        tenant?: string,
    ): Observable<PortalSignInMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<PortalSignInMethodResourceModel>(
            `${this.baseUrl}/${portal}/sign-in-method/${authenticationMethodId}/disable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public enableSignInMethod(
        portal: string,
        authenticationMethodId: string,
        tenant?: string,
    ): Observable<PortalSignInMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<PortalSignInMethodResourceModel>(
            `${this.baseUrl}/${portal}/sign-in-method/${authenticationMethodId}/enable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public reOrderSignInMethod(
        portal: string,
        authenticationMethodId: string,
        sortOrder: number,
        tenant?: string,
    ): Observable<PortalSignInMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<PortalSignInMethodResourceModel>(
            `${this.baseUrl}/${portal}/sign-in-method/${authenticationMethodId}/sort-order`,
            sortOrder,
            ApiHelper.toHttpOptions(params));
    }

    public getLoginMethods(portal: string, tenant?: string): Observable<Array<PortalLoginMethodResourceModel>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<PortalLoginMethodResourceModel>>(
            `${this.baseUrl}/${portal}/login-method`,
            ApiHelper.toHttpOptions(params));
    }
}
