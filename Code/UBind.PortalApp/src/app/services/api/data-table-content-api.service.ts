import { HttpClient, HttpParams, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AppConfig } from "@app/models/app-config";
import { Observable } from "rxjs";
import { AppConfigService } from "../app-config.service";

/**
 * Service for handling request for data table content.
 */
@Injectable({
    providedIn: 'root',
})
export class DataTableContentApiService {
    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        appConfigService: AppConfigService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'data-table-content';
        });
    }

    public getDataTableContentCsv(tenant: string, dataTableDefinitionId: string): Observable<string> {
        let params: HttpParams = new HttpParams().set("tenant", tenant);
        return this.httpClient.get(this.baseUrl + `/${dataTableDefinitionId}`, { responseType: 'text', params });
    }

    public downloadDataTableContentCsv(tenant: string, dataTableDefinitionId: string): Observable<HttpResponse<Blob>> {
        let params: HttpParams = new HttpParams().set("tenant", tenant);
        return this.httpClient.get(
            this.baseUrl + `/${dataTableDefinitionId}/download`,
            { observe: 'response', responseType: 'blob' , params },
        );
    }
}
