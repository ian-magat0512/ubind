import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SmsResourceModel } from '@app/resource-models/sms.resource-model';
import { ApiHelper } from '@app/helpers';
import { AppConfig } from '@app/models/app-config';
import { EntityType } from '@app/models/entity-type.enum';
import { Observable } from 'rxjs';
import { AppConfigService } from '../app-config.service';

/**
 * API service class for sms.
 */
@Injectable({ providedIn: 'root' })
export class SmsApiService {
    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<SmsResourceModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params = params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<SmsResourceModel>>(`${this.baseUrl}sms`, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<SmsResourceModel> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params = params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<SmsResourceModel>(`${this.baseUrl}sms/${id}`, ApiHelper.toHttpOptions(params));
    }

    public getEntitySms(
        entityType: EntityType,
        entityId: string,
        sources?: [],
        pageParams?: Map<string, string | Array<string>>,
    ): Observable<Array<SmsResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (sources) {
            params.set('source', sources);
        }
        if (pageParams) {
            params.set('page', pageParams.get('page'));
            params.set('pageSize', pageParams.get('pageSize'));
        }
        params.set('environment', this.appConfigService.getEnvironment());
        params.set('entityType', entityType);
        params.set('entityId', entityId);
        return this.httpClient.get<Array<SmsResourceModel>>(this.baseUrl + 'sms', ApiHelper.toHttpOptions(params));
    }
}
