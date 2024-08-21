import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '@app/services/api.service';
import {
    PolicyTransactionResourceModel,
    PolicyTransactionDetailResourceModel,
    PolicyTransactionPeriodicSummaryModel,
} from '@app/resource-models/policy.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { ApiHelper } from '@app/helpers/api.helper';
import { HttpClient, HttpParams } from '@angular/common/http';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';

/**
 * Export Policy transaction API service class.
 * TODO: Write a better class header: policy transaction API functions.
 */
@Injectable({ providedIn: 'root' })
export class PolicyTransactionApiService implements EntityLoaderService<PolicyTransactionResourceModel> {

    private baseUrl: string;
    private isMutual: boolean;
    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
        private api: ApiService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
            this.isMutual = appConfig.portal.isMutual;
        });
    }

    public getPeriodicSummaries(params?: Map<string, string | Array<string>>):
        Observable<Array<PolicyTransactionPeriodicSummaryModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        let environment: DeploymentEnvironment = this.appConfigService.getEnvironment();
        if (environment != DeploymentEnvironment.Production) {
            params.set('environment', this.appConfigService.getEnvironment());
        }
        return this.httpClient.get<Array<PolicyTransactionPeriodicSummaryModel>>(
            `${this.baseUrl}policy/periodic-summary`, ApiHelper.toHttpOptions(params));
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<PolicyTransactionResourceModel>> {
        const policyId: any = params.get('policyId');
        return this.httpClient.get<Array<PolicyTransactionResourceModel>>(
            this.baseUrl + 'policy/' + policyId + '/history',
            ApiHelper.toHttpOptions(params),
        );
    }

    public getById(
        id: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<PolicyTransactionResourceModel> {
        return this.httpClient.get<PolicyTransactionResourceModel>(
            this.baseUrl + `policy/transaction/${id}`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getPolicyTransactionDetails(
        policyId: string,
        transactionId: string,
    ): Observable<PolicyTransactionDetailResourceModel> {
        let refUrl: string = '/policy/' + policyId + '/transaction/' + transactionId;
        let params: any = null;
        const options: HttpParams = ApiHelper.generateRequestOptions(params);
        return this.api.get(refUrl, options).pipe(map((res: PolicyTransactionDetailResourceModel) => res));
    }
}
