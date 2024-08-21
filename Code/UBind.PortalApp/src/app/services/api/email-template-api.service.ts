import { Injectable } from '@angular/core';
import { ApiHelper } from '@app/helpers/api.helper';
import { Observable } from 'rxjs';
import { EmailTemplateSetting } from '@app/models';
import { EmailTemplateSettingUpdateModel }
    from '@app/resource-models/email-update-template-setting.resource-model';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';

/**
 * Export email template API service class.
 * TODO: Write a better class header: email template API functions.
 */
@Injectable({ providedIn: 'root' })
export class EmailTemplateApiService {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'email-template';
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<EmailTemplateSetting>> {
        return this.httpClient.get<Array<EmailTemplateSetting>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<EmailTemplateSetting> {
        return this.httpClient.get<EmailTemplateSetting>(this.baseUrl + `/${id}`, ApiHelper.toHttpOptions(params));
    }

    public getEmailTemplatesByTenant(tenant: string): Observable<Array<EmailTemplateSetting>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.getList(params);
    }

    public getEmailTemplatesByProduct(
        product: string,
        tenant?: string,
    ): Observable<Array<EmailTemplateSetting>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('product', product);
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.getList(params);
    }

    public getEmailTemplatesByPortal(
        product: string,
        tenant?: string,
    ): Observable<Array<EmailTemplateSetting>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('portal', product);
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.getList(params);
    }

    public updateEmailTemplateDetails(
        emailTemplateId: string,
        model: EmailTemplateSettingUpdateModel,
    ): Observable<EmailTemplateSetting> {
        return this.httpClient.put<EmailTemplateSetting>(this.baseUrl + `/${emailTemplateId}`, model);
    }

    public disable(emailTemplateId: string, tenant: string = null): Observable<EmailTemplateSetting> {
        let params: HttpParams = new HttpParams();
        if (tenant) {
            params = params.append('tenant', tenant);
        }
        return this.httpClient.patch<EmailTemplateSetting>(this.baseUrl + `/${emailTemplateId}/disable`, params);
    }

    public enable(emailTemplateId: string, tenant: string = null): Observable<EmailTemplateSetting> {
        let params: HttpParams = new HttpParams();
        if (tenant) {
            params = params.append('tenant', tenant);
        }
        return this.httpClient.patch<EmailTemplateSetting>(this.baseUrl + `/${emailTemplateId}/enable`, params);
    }

}
