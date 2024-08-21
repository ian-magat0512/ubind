import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { ApiHelper } from '@app/helpers';
import { EntityLoaderSaverService } from '../entity-loader-saver.service';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { RoleResourceModel, RolePermissionResourceModel } from '@app/resource-models/role.resource-model';
import { PersonResourceModel } from '@app/models';
import { OrganisationResourceModel } from '@app/resource-models/organisation/organisation.resource-model';

/**
 * Service for handling request to user api endpoint.
 */
@Injectable({ providedIn: 'root' })
export class UserApiService implements EntityLoaderSaverService<UserResourceModel> {
    private userUrl: string;
    protected baseUrl: string;

    public constructor(
        protected httpClient: HttpClient,
        protected appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
            this.userUrl = `${this.baseUrl}user`;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<UserResourceModel>> {
        return this.httpClient.get<Array<UserResourceModel>>(`${this.userUrl}`, ApiHelper.toHttpOptions(params));
    }

    public getById(userId: string, params?: Map<string, string | Array<string>>): Observable<UserResourceModel> {
        return this.httpClient.get<UserResourceModel>(`${this.userUrl}/${userId}`, ApiHelper.toHttpOptions(params));
    }

    public getUsersWithRole(roleName: string, tenant?: string): Observable<Array<UserResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('roleNames', [roleName]);
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.getList(params);
    }

    public getUsersAssignableAsOwner(organisation: string): Observable<Array<UserResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('organisation', organisation);
        return this.httpClient.get<Array<UserResourceModel>>(
            `${this.userUrl}/assignable-as-owner`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getUserByPersonId(personId: string, tenantId?: string): Observable<UserResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenantId) {
            params.set('tenant', tenantId);
        }
        return this.httpClient.get<UserResourceModel>(
            `${this.userUrl}/person/${personId}`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public create(model: PersonResourceModel): Observable<UserResourceModel> {
        return this.httpClient.post<UserResourceModel>(`${this.userUrl}`, model);
    }

    public update(userId: string, model: PersonResourceModel): Observable<UserResourceModel> {
        return this.httpClient.put<UserResourceModel>(`${this.userUrl}/${userId}`, model);
    }

    public disable(userId: string, tenant: string): Observable<UserResourceModel> {
        let params: HttpParams = new HttpParams();
        params = params.set("tenant", tenant);
        return this.httpClient.patch<UserResourceModel>(`${this.userUrl}/${userId}/disable`, null, { params });
    }

    public enable(userId: string, tenant: string): Observable<UserResourceModel> {
        let params: HttpParams = new HttpParams();
        params = params.set("tenant", tenant);
        return this.httpClient.patch<UserResourceModel>(`${this.userUrl}/${userId}/enable`, null, { params });
    }

    public uploadProfilePicture(userId: string, file: any, tenant?: string): Observable<UserResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.post<UserResourceModel>(
            `${this.userUrl}/${userId}/profile-picture`, file, ApiHelper.toHttpOptions(params));
    }

    public getUserRoles(userId: string, tenant?: string): Observable<Array<RoleResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<RoleResourceModel>>(
            `${this.userUrl}/${userId}/role`,
            ApiHelper.toHttpOptions(params));
    }

    public assignRoleToUser(userId: string, roleId: string, tenant?: string): Observable<RoleResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.post<RoleResourceModel>(
            `${this.userUrl}/${userId}/role/${roleId}`,
            {},
            ApiHelper.toHttpOptions(params));
    }

    public unassignRoleFromUser(userId: string, roleId: string, tenant?: string): Observable<any> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.delete(
            `${this.userUrl}/${userId}/role/${roleId}`,
            ApiHelper.toHttpOptions(params));
    }

    public getAvailableRoles(userId: string, tenant?: string): Observable<Array<RoleResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<RoleResourceModel>>(
            `${this.userUrl}/${userId}/available-roles`,
            ApiHelper.toHttpOptions(params));
    }

    public getOrganisationForUser(
        userId: string,
        organisation: string,
        tenant?: string,
    ): Observable<OrganisationResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<OrganisationResourceModel>(
            `${this.userUrl}/${userId}/organisation/${organisation}`,
            ApiHelper.toHttpOptions(params));
    }

    public getEffectivePermissions(userId: string, tenant?: string): Observable<Array<RolePermissionResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<RolePermissionResourceModel>>(
            `${this.userUrl}/${userId}/permission`,
            ApiHelper.toHttpOptions(params));
    }

    public unlinkIdentity(
        userId: string,
        authenticationMethodId: string,
        tenant?: string,
    ): Observable<UserResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.httpClient.delete<UserResourceModel>(
            `${this.userUrl}/${userId}/linked-identity/${authenticationMethodId}`,
            ApiHelper.toHttpOptions(params));
    }

    public assignPortalToUser(userId: string, portalId: string, tenantId?: string): Observable<void> {
        return this.httpClient.patch<void>(`${this.userUrl}/${userId}/portal/${portalId}?tenant=${tenantId}`, {});
    }

    public unassignPortalFromUser(userId: string, tenantId?: string): Observable<void> {
        return this.httpClient.delete<void>(`${this.userUrl}/${userId}/portal?tenant=${tenantId}`, {});
    }

}
