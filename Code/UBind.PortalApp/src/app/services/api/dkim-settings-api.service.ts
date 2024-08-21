import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { ApiService } from '../api.service';
import { ApiHelper } from '@app/helpers';
import { DkimSettingsResourceModel, DkimSettingsUpsertModel } from '@app/resource-models/dkim-settings.resource-model';
import { map } from 'rxjs/operators';
import { DkimTestEmailResourceModel } from "@app/resource-models/dkim-test-email.resource-model";

/**
 * Export DKIM setting Api Service.
 * This class manage the API services of DKIM settings
 */
@Injectable({ providedIn: 'root' })
export class DkimSettingsApiService {

    private dkimSettingsUrl: string;
    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
        public apiService: ApiService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.dkimSettingsUrl = `${appConfig.portal.api.baseUrl}dkim-settings`;
        });
    }

    public createDkimSettings(model: DkimSettingsUpsertModel): Observable<DkimSettingsResourceModel> {
        return this.httpClient.post<DkimSettingsResourceModel>(this.dkimSettingsUrl, model).pipe(
            map((res: DkimSettingsResourceModel) => res));
    }

    public getDkimSettingsByOrganisation(
        organisation: string,
        tenant?: string,
    ): Observable<Array<DkimSettingsResourceModel>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('organisation', organisation);
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<Array<DkimSettingsResourceModel>>(
            `${this.dkimSettingsUrl}`,
            ApiHelper.toHttpOptions(params));
    }

    public getDkimSettingsById(
        dkimSettingsId: string,
        organisationId: string,
        tenant?: string,
    ): Observable<DkimSettingsResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.get<DkimSettingsResourceModel>(
            `${this.dkimSettingsUrl}/${dkimSettingsId}/organisation/${organisationId}`,
            ApiHelper.toHttpOptions(params));
    }

    public updateDkimSettings(
        dkimSettingsId: string,
        model: DkimSettingsUpsertModel,
        organisationId: string,
    ): Observable<DkimSettingsResourceModel> {
        return this.httpClient
            .patch<DkimSettingsResourceModel>(
                `${this.dkimSettingsUrl}/${dkimSettingsId}/organisation/${organisationId}`, model)
            .pipe(
                map((res: DkimSettingsResourceModel) => res));
    }

    public sendDkimTestEmail(model: DkimTestEmailResourceModel): Observable<void> {
        return this.httpClient
            .post<void>(`${this.dkimSettingsUrl}/send-dkim-test-email`, model)
            .pipe(
                map((res: void) => res));
    }

    public deleteDkimSettings(dkimSettingsId: string, organisationId: string, tenant?: string): Observable<void> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.delete<void>(
            `${this.dkimSettingsUrl}/${dkimSettingsId}/organisation/${organisationId}`,
            ApiHelper.toHttpOptions(params))
            .pipe(map((res: void) => res));
    }
}
