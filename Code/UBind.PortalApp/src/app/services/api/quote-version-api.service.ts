import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import {
    QuoteVersionResourceModel, QuoteVersionDetailResourceModel,
} from '@app/resource-models/quote-version.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';

/**
 * Export quote version API serivce class.
 * TODO: Write a better class header: quote version API functions.
 */
@Injectable({ providedIn: 'root' })
export class QuoteVersionApiService implements EntityLoaderService<QuoteVersionResourceModel> {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<QuoteVersionResourceModel>> {
        const quoteId: any = params.get('quoteId');
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<QuoteVersionResourceModel>>(
            this.baseUrl + `quote/${quoteId}/version`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<QuoteVersionResourceModel> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<QuoteVersionResourceModel>(
            this.baseUrl + `quote-version/${id}`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getQuoteVersions(quoteId: string): Observable<Array<QuoteVersionResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('quoteId', quoteId);
        params.set('environment', this.appConfigService.getEnvironment());
        return this.getList(params);
    }

    public getQuoteVersionDetail(quoteVersionId: string): Observable<QuoteVersionDetailResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<QuoteVersionDetailResourceModel>(
            this.baseUrl + `quote-version/${quoteVersionId}/detail`,
            ApiHelper.toHttpOptions(params),
        );
    }
}
