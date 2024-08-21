import { HttpClient } from "@angular/common/http";
import { AppConfigService } from "../app-config.service";
import { AppConfig } from "@app/models/app-config";
import { Observable } from "rxjs";
import { ApiHelper } from "@app/helpers/api.helper";
import { Injectable } from "@angular/core";
import {
    AuthenticationMethodResourceModel,
    AuthenticationMethodUpsertModel,
} from "@app/resource-models/authentication-method.resource-model";

/**
 * Service for fetching and storing authentication method configurations, e.g. SAML SSO
 */
@Injectable({
    providedIn: 'root',
})
export class AuthenticationMethodApiService {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = `${appConfig.portal.api.baseUrl}authentication-method`;
        });
    }

    public createAuthenticationMethod(
        model: AuthenticationMethodUpsertModel,
    ): Observable<AuthenticationMethodResourceModel> {
        return this.httpClient.post<AuthenticationMethodResourceModel>(this.baseUrl, model);
    }

    public getAuthenticationMethod(
        authenticationMethodId: string,
        tenant?: string,
    ): Observable<AuthenticationMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<AuthenticationMethodResourceModel>(
            `${this.baseUrl}/${authenticationMethodId}`,
            ApiHelper.toHttpOptions(params));
    }

    public getAuthenticationMethods(
        organisation: string,
        tenant?: string,
    ): Observable<Array<AuthenticationMethodResourceModel>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('organisation', organisation);
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<AuthenticationMethodResourceModel>>(
            `${this.baseUrl}`,
            ApiHelper.toHttpOptions(params));
    }

    public updateAuthenticationMethod(
        authenticationMethodId: string,
        model: AuthenticationMethodUpsertModel,
    ): Observable<AuthenticationMethodResourceModel> {
        return this.httpClient.put<AuthenticationMethodResourceModel>(
            `${this.baseUrl}/${authenticationMethodId}`,
            model);
    }

    public deleteAuthenticationMethod(authenticationMethodId: string, tenant: string = null): Observable<void> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.delete<void>(
            `${this.baseUrl}/${authenticationMethodId}`,
            ApiHelper.toHttpOptions(params));
    }

    public enableAuthenticationMethod(
        authenticationMethodId: string,
        tenant: string = null,
    ): Observable<AuthenticationMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<AuthenticationMethodResourceModel>(
            `${this.baseUrl}/${authenticationMethodId}/enable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public disableAuthenticationMethod(
        authenticationMethodId: string,
        tenant: string = null,
    ): Observable<AuthenticationMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<AuthenticationMethodResourceModel>(
            `${this.baseUrl}/${authenticationMethodId}/disable`,
            null,
            ApiHelper.toHttpOptions(params));
    }
}
