import { Injectable } from '@angular/core';
import { HttpParams, HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { map } from 'rxjs/operators';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { UserAuthorisationModel } from '@app/resource-models/authentication-response.resource-model';
import { UserResourceModel } from '@app/resource-models/user/user.resource-model';
import { NavigationEnd, NavigationStart, Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { UserPasswordResourceModel } from '@app/resource-models/user/user-password.resource-model';
import { UrlHelper } from '@app/helpers/url.helper';
import {
    UserPasswordInvitationRequestModel,
} from '@app/resource-models/user/user-password-invitation-request.model';

/**
 * Model an invitation record's status, e.g.password reset invitation status
 */
export interface IInvitationStatus {
    isAvailable: boolean;
    error: string;
}

/**
 * Model for the result of a password reset operation
 */
export interface IPasswordSettingResult {
    succeeded: boolean;
    errorType: string;
    errors: Array<string>;
}

export enum IInvitationStatusResult {
    UserActivated = 'UserActivated'
}

/**
 * Export Invitation API service class.
 * TODO: Write a better class header: invitation API functions.
 */
@Injectable({ providedIn: 'root' })
export class InvitationApiService {

    private baseUrl: string;
    private organisationId: string;
    private environment: string;

    public constructor(
        private httpClient: HttpClient,
        private router: Router,
        private appConfigService: AppConfigService,
    ) {
        this.onNavigationStartExecuteActions();
    }

    public validateActivation(userId: string, invitationId: string, tenant?: string): Observable<IInvitationStatus> {
        const params: HttpParams = new HttpParams()
            .set(ApiHelper.invitationValidateActivation.params.userId, userId)
            .set(ApiHelper.invitationValidateActivation.params.invitationId, invitationId);
        if (tenant) {
            params.set(ApiHelper.invitationValidateActivation.params.tenant, tenant);
        }
        return this.httpClient
            .post<IInvitationStatus>(this.baseUrl + ApiHelper.invitationValidateActivation.route, params);
    }

    public sendActivationForPerson(personId: string, tenant?: string): Observable<UserResourceModel> {
        const requestBody: any = {
            personId: personId,
            environment: this.environment,
        };
        if (tenant) {
            requestBody.tenant = tenant;
        }
        return this.httpClient
            .post<UserResourceModel>(`${this.baseUrl}/invitation/send-person-activation`, requestBody);
    }

    public setPassword(model: UserPasswordResourceModel): Observable<UserAuthorisationModel> {
        return this.httpClient
            .post<UserAuthorisationModel>(this.baseUrl + ApiHelper.invitationSetPassword.route, model);
    }

    public requestResetPassword(
        email: string,
        tenant: string,
        portal: string = null,
        isPasswordExpired: boolean = false,
    ): Observable<any> {
        let model: UserPasswordInvitationRequestModel = {
            email: email,
            tenant: tenant,
            organisation: this.organisationId,
            environment: this.environment,
            isPasswordExpired: isPasswordExpired,
        };
        return this.httpClient
            .post(this.baseUrl + ApiHelper.invitationRequestPassword.route, model);
    }

    public validatePasswordReset(userId: string, invitationId: string, tenant: string): Observable<Response> {
        const requestBody: HttpParams = new HttpParams()
            .set(ApiHelper.invitationValidatePasswordReset.params.userId, userId)
            .set(ApiHelper.invitationValidatePasswordReset.params.invitationId, invitationId)
            .set('tenant', tenant);

        return this.httpClient.post(this.baseUrl + ApiHelper.invitationValidatePasswordReset.route, requestBody)
            .pipe(map((output: Response) => output));
    }

    public resetPassword(model: UserPasswordResourceModel): Observable<UserAuthorisationModel> {
        return this.httpClient
            .post<UserAuthorisationModel>(this.baseUrl + ApiHelper.invitationResetPassword.route, model);
    }

    public sendAccountActivationForPerson(personId: string): Observable<Response> {
        const requestBody: any = {
            personId: personId,
            environment: this.environment,
        };
        const requestUrl: string = `${this.baseUrl}/invitation/send-person-activation`;
        return this.httpClient.post(requestUrl, requestBody).pipe(map((output: Response) => output));
    }

    private onNavigationStartExecuteActions(): void {
        this.router.events
            .pipe(
                filter((event: any) => event instanceof NavigationStart),
                map((event: any) => (<NavigationEnd>event).url),
            )
            .subscribe((url: string) => {
                this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
                    const baseUrl: string = appConfig.portal.api.baseUrl;
                    const tenantAlias: string = appConfig.portal.tenantAlias;
                    this.organisationId = appConfig.portal.organisationId;
                    this.environment = appConfig.portal.environment;
                    const organisationAliasOnURL: string = UrlHelper.getOrganisationAliasFromUrl();
                    if (organisationAliasOnURL) {
                        this.baseUrl = `${baseUrl}tenant/${tenantAlias}/organisation/${this.organisationId}`;
                    } else {
                        this.baseUrl = `${baseUrl}${tenantAlias}`;
                    }
                });
            });
    }
}
