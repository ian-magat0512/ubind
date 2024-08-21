import { Injectable } from '@angular/core';
import { ApiHelper } from '@app/helpers/api.helper';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
    ReportResourceModel,
    ReportGenerateModel,
    ReportFileResourceModel,
    ReportCreateModel,
} from '@app/resource-models/report.resource-model';
import { EntityLoaderSaverService } from '../entity-loader-saver.service';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';

/**
 * Export Report API service class.
 * TODO: Write a better class header: report API functions.
 */
@Injectable({ providedIn: 'root' })
export class ReportApiService implements EntityLoaderSaverService<ReportResourceModel> {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'report';
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<ReportResourceModel>> {
        return this.httpClient.get<Array<ReportResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<ReportResourceModel> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<ReportResourceModel>(this.baseUrl + `/${id}`, ApiHelper.toHttpOptions(params));
    }

    public getReportsByTenantId(tenantId: string): Observable<Array<ReportResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenantId', tenantId);
        return this.getList(params);
    }

    public generateReportFile(reportId: string, reportGenerateModel: ReportGenerateModel): Observable<any> {
        reportGenerateModel.environment = this.appConfigService.getEnvironment();
        return this.httpClient.post(
            this.baseUrl + `/${reportId}/generate`,
            reportGenerateModel,
            { observe: 'response', responseType: 'blob' as 'json' },
        );
    }

    public getReportFileList(
        reportId: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<Array<ReportFileResourceModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<ReportFileResourceModel>>(
            this.baseUrl + `/${reportId}/file/`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getReportFile(reportId: string, reportFileId: string): Observable<any | Blob> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        let options: any = ApiHelper.toHttpOptions(params);
        options['responseType'] = 'blob';
        return this.httpClient.get(this.baseUrl + `/${reportId}/file/${reportFileId}`, options);
    }

    public create(report: ReportCreateModel): Observable<ReportResourceModel> {
        return this.httpClient.post<ReportResourceModel>(this.baseUrl, report);
    }

    public update(reportId: string, report: ReportCreateModel): Observable<ReportResourceModel> {
        return this.httpClient.put<ReportResourceModel>(this.baseUrl + `/${reportId}`, report);
    }
}
