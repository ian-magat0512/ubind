import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { FeatureSettingResourceModel } from '@app/models';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';

/**
 * Export setting API service class.
 * TODO: Write a better class header: setting API functions.
 */
@Injectable({ providedIn: 'root' })
export class FeatureSettingApiService {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<FeatureSettingResourceModel>> {
        throw Error("The generic getList function on FeatureSettingApiService is not implemented.");
    }

    public getTenantSettings(tenantId: string): Observable<Array<FeatureSettingResourceModel>> {
        return this.httpClient.get<Array<FeatureSettingResourceModel>>(
            `${this.baseUrl}tenant/${tenantId}/feature-setting`,
        );
    }

    public getPortalSettings(tenantId: string, portalId: string): Observable<Array<FeatureSettingResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenantId);
        return this.httpClient.get<Array<FeatureSettingResourceModel>>(
            `${this.baseUrl}portal/${portalId}/feature-setting`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public updateTenantSetting(
        tenantId: string,
        settingId: string,
        model: FeatureSettingResourceModel,
    ): Observable<FeatureSettingResourceModel> {
        return this.httpClient.put<FeatureSettingResourceModel>(
            `${this.baseUrl}tenant/${tenantId}/feature-setting/${settingId}`,
            model,
        );
    }

    public updatePortalSetting(
        portalId: string,
        settingId: string,
        model: FeatureSettingResourceModel,
    ): Observable<FeatureSettingResourceModel> {
        return this.httpClient.put<FeatureSettingResourceModel>(
            `${this.baseUrl}portal/${portalId}/feature-setting/${settingId}`,
            model,
        );
    }
}
