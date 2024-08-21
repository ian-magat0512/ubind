import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import {
    OrganisationResourceModel,
    UpsertOrganisationResourceModel,
} from "@app/resource-models/organisation/organisation.resource-model";
import { ApiHelper } from "@app/helpers";
import { AppConfig } from "@app/models/app-config";
import { Observable } from "rxjs";
import { AppConfigService } from "../app-config.service";
import { EntityLoaderService } from "../entity-loader.service";
import {
    AuthenticationMethodResourceModel,
    AuthenticationMethodUpsertModel,
} from "@app/resource-models/authentication-method.resource-model";
import {
    OrganisationLinkedIdentity,
} from "@app/resource-models/organisation/organisation-linked-identity.resource-model";

/**
 * API service for making API calls about organisations
 */
@Injectable({ providedIn: 'root' })
export class OrganisationApiService implements EntityLoaderService<OrganisationResourceModel> {
    private organisationUrl: string;

    public constructor(
        appConfigService: AppConfigService,
        private httpClient: HttpClient,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.organisationUrl = `${appConfig.portal.api.baseUrl}organisation`;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<OrganisationResourceModel>> {
        return this.httpClient.get<Array<OrganisationResourceModel>>(
            this.organisationUrl,
            ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<OrganisationResourceModel> {
        return this.httpClient.get<OrganisationResourceModel>(
            `${this.organisationUrl}/${id}`,
            ApiHelper.toHttpOptions(params));
    }

    public create(model: UpsertOrganisationResourceModel): Observable<OrganisationResourceModel> {
        return this.httpClient.post<OrganisationResourceModel>(this.organisationUrl, model);
    }

    public update(id: string, model: UpsertOrganisationResourceModel): Observable<OrganisationResourceModel> {
        return this.httpClient.put<OrganisationResourceModel>(`${this.organisationUrl}/${id}`, model);
    }

    public setToDefault(id: string, tenant?: string): Observable<OrganisationResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<OrganisationResourceModel>(
            `${this.organisationUrl}/${id}/default`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public enable(id: string, tenant?: string): Observable<OrganisationResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<OrganisationResourceModel>(
            `${this.organisationUrl}/${id}/enable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public disable(id: string, tenant?: string): Observable<OrganisationResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<OrganisationResourceModel>(
            `${this.organisationUrl}/${id}/disable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public delete(id: string, tenant?: string): Observable<OrganisationResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.delete<OrganisationResourceModel>(
            `${this.organisationUrl}/${id}`,
            ApiHelper.toHttpOptions(params));
    }

    public setManagingOrganisation(
        organisation: string,
        managingOrganisation: string,
        tenant?: string,
    ): Observable<OrganisationResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<OrganisationResourceModel>(
            `${this.organisationUrl}/${organisation}/managing-organisation`,
            managingOrganisation || '', // this must be a string so we don't get an unsupported media type error
            ApiHelper.toHttpOptions(params));
    }

    public getLocalAccountAuthenticationMethod(
        organisation: string,
        tenant?: string,
    ): Observable<AuthenticationMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<AuthenticationMethodResourceModel>(
            `${this.organisationUrl}/${organisation}/authentication-method/local-account`,
            ApiHelper.toHttpOptions(params));
    }

    public updateLocalAccountAuthenticationMethod(
        organisation: string,
        model: AuthenticationMethodUpsertModel,
    ): Observable<AuthenticationMethodResourceModel> {
        return this.httpClient.put<AuthenticationMethodResourceModel>(
            `${this.organisationUrl}/${organisation}/authentication-method/local-account`,
            model);
    }

    public enableLocalAccountAuthenticationMethod(
        organisation: string,
        tenant?: string,
    ): Observable<AuthenticationMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<AuthenticationMethodResourceModel>(
            `${this.organisationUrl}/${organisation}/authentication-method/local-account/enable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public disableLocalAccountAuthenticationMethod(
        organisation: string,
        tenant?: string,
    ): Observable<AuthenticationMethodResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<AuthenticationMethodResourceModel>(
            `${this.organisationUrl}/${organisation}/authentication-method/local-account/disable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public getPotentialLinkedIdentities(
        managingOrganisation: string,
        tenant?: string,
    ): Observable<Array<OrganisationLinkedIdentity>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<OrganisationLinkedIdentity>>(
            `${this.organisationUrl}/${managingOrganisation}/potential-linked-identities`,
            ApiHelper.toHttpOptions(params));
    }

    /**
     * Gets the linked identities for the specified organisation.
     * @param includePotential if set to true, empty/placeholder linked identites for authentication methods that are
     * not yet linked will be included. This is useful for the edit page of an organistion so it can allow you to 
     * manually enter the linked identity unique identifier.
     */
    public getLinkedIdentities(
        organisation: string,
        tenant?: string,
        includePotential?: boolean,
    ): Observable<Array<OrganisationLinkedIdentity>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        if (includePotential != null) {
            params.set('includePotential', includePotential ? 'true' : 'false');
        }
        return this.httpClient.get<Array<OrganisationLinkedIdentity>>(
            `${this.organisationUrl}/${organisation}/linked-identities`,
            ApiHelper.toHttpOptions(params));
    }

    public getEntitySettings(tenantId: string, organisationId: string): Observable<any> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenantId', tenantId);
        return this.httpClient.get<any>(
            `${this.organisationUrl}/${organisationId}/entity-settings`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public isSendingRenewalInvitationsAllowed(tenantId: string, organisationId: string): Observable<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenantId', tenantId);
        return this.httpClient.get<boolean>(
            `${this.organisationUrl}/${organisationId}/entity-settings/renewal-invitation`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public allowSendingRenewalInvitations(tenantId: string, entityId: string): Observable<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenantId', tenantId);
        return this.httpClient.patch<boolean>(
            `${this.organisationUrl}/${entityId}/entity-settings/renewal-invitation/allow`,
            null,
            ApiHelper.toHttpOptions(params),
        );
    }

    public disallowSendingRenewalInvitations(tenantId: string, entityId: string): Observable<boolean> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenantId', tenantId);
        return this.httpClient.patch<boolean>(
            `${this.organisationUrl}/${entityId}/entity-settings/renewal-invitation/disallow`,
            null,
            ApiHelper.toHttpOptions(params),
        );
    }
}
