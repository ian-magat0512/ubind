import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ApiHelper } from "@app/helpers/api.helper";
import { ApiService } from "@app/services/api.service";
import { EntityLoaderService } from "../entity-loader.service";
import { EmailResourceModel } from "@app/resource-models/email.resource-model";
import { HttpClient } from "@angular/common/http";
import { AppConfigService } from "../app-config.service";
import { AppConfig } from "@app/models/app-config";
import { EntityType } from "@app/models/entity-type.enum";

/**
 * Email API service handles all API calls related to emails.
 */
@Injectable({ providedIn: 'root' })
export class EmailApiService implements EntityLoaderService<EmailResourceModel> {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
        private api: ApiService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = `${appConfig.portal.api.baseUrl}email`;
        });
    }

    public downloadAttachment(emailId: string, attachmentId: string): Observable<any | Blob> {
        const url: string = `${this.baseUrl}/${emailId}/attachment/${attachmentId}`;
        return this.api.getBlob(url, null, true);
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<EmailResourceModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }

        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<EmailResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<EmailResourceModel> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<EmailResourceModel>(`${this.baseUrl}/${id}`, ApiHelper.toHttpOptions(params));
    }

    public getEntityEmails(
        entityType: EntityType,
        entityId: string,
        sources?: [],
        pageParams?: Map<string, string | Array<string>>,
    ): Observable<Array<EmailResourceModel>> {
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
        return this.httpClient.get<Array<EmailResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }
}
