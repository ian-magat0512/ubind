import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AccountUpdateModel as AccountUpdateModel } from '@app/models';
import { UserResourceModel } from '../../resource-models/user/user.resource-model';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { AccountLoginCredentialsResourceModel } from '@app/resource-models/account-login-credentials.resource-model';
import { UserAuthorisationModel } from '@app/resource-models/authentication-response.resource-model';
import { EntityLoaderSaverService } from '../entity-loader-saver.service';
import { RolePermissionResourceModel, RoleResourceModel } from '@app/resource-models/role.resource-model';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';

/**
 * Export Account API service class.
 * TODO: Write a better class header: account API functions.
 */
@Injectable({ providedIn: 'root' })
export class AccountApiService implements EntityLoaderSaverService<UserResourceModel> {

    private baseUrl: string;
    private environment: DeploymentEnvironment;
    private tenant: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.environment = <DeploymentEnvironment>appConfig.portal.environment;
            this.baseUrl = appConfig.portal.api.baseUrl + 'account';
            this.tenant = appConfig.portal.tenantAlias;
        });
    }

    public create(model: any): Observable<UserResourceModel> {
        throw new Error("Method not implemented.");
    }
    public getList(params?: Map<string, string | Array<string>>): Observable<Array<UserResourceModel>> {
        throw new Error("Method not implemented.");
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<UserResourceModel> {
        return this.get();
    }

    public login(credentials: AccountLoginCredentialsResourceModel): Observable<UserAuthorisationModel> {
        return this.httpClient.post<UserAuthorisationModel>(this.baseUrl + '/login', credentials);
    }

    public getAuthorisationModel(): Observable<UserAuthorisationModel> {
        return this.httpClient.get<UserAuthorisationModel>(this.baseUrl + '/authorisation-model');
    }

    public logout(): Observable<any> {
        return this.httpClient.post(this.baseUrl + '/logout', null);
    }

    public get(): Observable<UserResourceModel> {
        return this.httpClient.get<UserResourceModel>(this.baseUrl);
    }

    public update(id: any, resourceModel: AccountUpdateModel): Observable<UserResourceModel> {
        return this.httpClient.put<UserResourceModel>(this.baseUrl, resourceModel);
    }

    public uploadProfilePicture(file: any): Observable<UserResourceModel> {
        return this.httpClient.post<UserResourceModel>(this.baseUrl + '/profile-picture', file);
    }

    public getUserPermissions(): Observable<Array<RolePermissionResourceModel>> {
        return this.httpClient.get<Array<RolePermissionResourceModel>>(
            this.baseUrl + '/permission',
        );
    }

    public getUserRoles(): Observable<Array<RoleResourceModel>> {
        return this.httpClient.get<Array<RoleResourceModel>>(
            this.baseUrl + '/role',
        );
    }

    public getPortalUrl(): Observable<string> {
        let params: HttpParams = new HttpParams()
            .set('tenant', this.tenant)
            .set('environment', this.environment);
        return this.httpClient.get<string>(this.baseUrl + '/portal-url', { params });
    }
}
