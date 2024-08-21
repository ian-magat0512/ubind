import { RoleResourceModel, RolePermissionResourceModel } from '@app/resource-models/role.resource-model';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { SubscriptionLike } from 'rxjs/internal/types';
import { AppConfig } from '@app/models/app-config';
import { Observable } from 'rxjs/internal/Observable';
import { EntityLoaderSaverService } from '../entity-loader-saver.service';
import { ApiHelper } from '@app/helpers';
import { RoleType } from '@app/models/role-type.enum';
import { Injectable } from '@angular/core';

/**
 * Export role API service class.
 * TODO: Write a better class header: role API functions.
 */
@Injectable({ providedIn: 'root' })
export class RoleApiService implements EntityLoaderSaverService<RoleResourceModel> {

    private baseURL: string = '';
    private subscriptions: Array<SubscriptionLike> = new Array<SubscriptionLike>();

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.subscriptions.push(this.appConfigService.appConfigSubject
            .subscribe((appConfig: AppConfig) => {
                this.baseURL = `${appConfig.portal.api.baseUrl}role`;
            }));
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<RoleResourceModel>> {
        return this.httpClient.get<Array<RoleResourceModel>>(this.baseURL, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string): Observable<RoleResourceModel> {
        return this.httpClient.get<RoleResourceModel>(`${this.baseURL}/${id}`);
    }

    public create(role: RoleResourceModel): Observable<RoleResourceModel> {
        return this.httpClient.post<RoleResourceModel>(this.baseURL, role);
    }

    public update(roleId: string, role: RoleResourceModel): Observable<RoleResourceModel> {
        return this.httpClient.put<RoleResourceModel>(`${this.baseURL}/${roleId}`, role);
    }

    public delete(roleId: string): Observable<boolean> {
        return this.httpClient.delete<boolean>(`${this.baseURL}/${roleId}`);
    }

    public getAllPermissions(roleType: RoleType): Observable<Array<RolePermissionResourceModel>> {
        const params: any = { label: ApiHelper.rolePermissionsByRoleType.params.roleType, value: roleType };
        return this.httpClient.get<Array<RolePermissionResourceModel>>(
            `${this.baseURL}/permissions`,
            { params: ApiHelper.generateRequestOptions(params) },
        );
    }

    public assignPermission(roleId: string, permission: number): Observable<void> {
        return this.httpClient.post<void>(this.baseURL + `/${roleId}/permission/${permission}`, '');
    }

    public updatePermission(roleId: string, previousPermission: number, newPermission: number): Observable<void> {
        return this.httpClient.put<void>(
            `${this.baseURL}/${roleId}/permission/${previousPermission}/${newPermission}`,
            '',
        );
    }

    public retractPermission(roleId: string, permission: string): Observable<void> {
        return this.httpClient.delete<void>(`${this.baseURL}/${roleId}/permission/${permission}`);
    }
}
