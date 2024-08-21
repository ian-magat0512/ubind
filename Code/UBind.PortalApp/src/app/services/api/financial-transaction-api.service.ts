import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {
    FinancialTransactionCreateModel,
    FinancialTransactionResourceModel,
} from '@app/resource-models/financial-transaction.resource-model';
import { AppConfigService } from '@app/services/app-config.service';
import { Observable } from 'rxjs';
import { AppConfig } from '@app/models/app-config';
import { ApiHelper } from '@app/helpers';

/**
 * Export Financial Transaction API Service
 * TODO: Write a better class header: financial transactions API functions.
 */
@Injectable({ providedIn: 'root' })
export class FinancialTransactionApiService {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'financial-transaction';
        });
    }

    public getFinancialTransactionById(
        transactionId: string,
        transactionType: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<FinancialTransactionResourceModel> {
        params = this.setEnvironment(params);
        return this.httpClient.get<FinancialTransactionResourceModel>(
            this.baseUrl + `/${transactionId}/${transactionType}`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public createFinancialTransaction(
        model: FinancialTransactionCreateModel,
    ): Observable<FinancialTransactionResourceModel> {
        return this.httpClient.post<FinancialTransactionResourceModel>(
            this.baseUrl + `/create`,
            model,
            { params: { environment: this.appConfigService.getEnvironment() } },
        );
    }

    private setEnvironment(params: Map<string, string | Array<string>>): Map<string, string | Array<string>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());

        return params;
    }
}
