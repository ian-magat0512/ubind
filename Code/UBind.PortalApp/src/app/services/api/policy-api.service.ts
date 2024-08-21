import { Injectable } from '@angular/core';
import { HttpParams, HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiHelper } from '@app/helpers/api.helper';
import { ApiService } from '@app/services/api.service';
import {
    PolicyResourceModel,
    PolicyDetailResourceModel,
    PolicyPremiumDetailResourceModel,
    PolicyQuestionDetailResourceModel,
    PolicyTransactionResourceModel,
    PolicyDocumentsDetailResourceModel,
    PolicyTransactionDetailResourceModel,
    UpdatePolicyNumberResourceModel,
} from '@app/resource-models/policy.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { QuoteCreateResultModel } from '@app/resource-models/quote.resource-model';
import { CustomerResourceModel } from '@app/resource-models/customer.resource-model';

/**
 * Export policy API Service Class.
 * TODO: Write a better class header: policy API functions.
 */
@Injectable({ providedIn: 'root' })
export class PolicyApiService implements EntityLoaderService<PolicyResourceModel> {

    private baseUrl: string;
    private exportUrl: string;
    private environment: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
        private api: ApiService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'policy';
            this.exportUrl = appConfig.portal.api.baseUrl;
            this.environment = appConfig.portal.environment;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<PolicyResourceModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<PolicyResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<PolicyResourceModel> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<PolicyResourceModel>(this.baseUrl + `/${id}`, ApiHelper.toHttpOptions(params));
    }

    public getForRenewalPolicyList(
        params?: Map<string, string | Array<string>>,
    ): Observable<Array<PolicyResourceModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        params.set('includeProductFeatureSetting', 'true');
        return this.httpClient.get<Array<PolicyResourceModel>>(
            this.baseUrl + '/for-renewal',
            ApiHelper.toHttpOptions(params),
        );
    }

    public sendPolicyRenewalInvitation(
        policyId: string,
        productId: string = '',
        shouldHaveAUserAccount: boolean = true,
    ): Observable<Array<PolicyResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.environment);
        if (shouldHaveAUserAccount) {
            params.set('isUserAccountRequired', shouldHaveAUserAccount.toString());
        }
        return this.httpClient.post<Array<PolicyResourceModel>>(
            this.baseUrl + `/${policyId}/send-renewal-invitation`,
            null,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getPolicyHistoryList(policyId: string): Observable<Array<PolicyTransactionResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<PolicyTransactionResourceModel>>(
            this.baseUrl + `/${policyId}/history`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getCustomerOfPolicy(policyId: string): Observable<CustomerResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        return this.httpClient.get<CustomerResourceModel>(
            this.baseUrl + `/${policyId}/customer`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getPolicyTransaction(
        policyId: string,
        transactionId: string,
    ): Observable<PolicyTransactionDetailResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<PolicyTransactionDetailResourceModel>(
            this.baseUrl + `/${policyId}/transaction/${transactionId}`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getPolicyBaseDetails(policyId: string): Observable<PolicyDetailResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<PolicyDetailResourceModel>(
            this.baseUrl + `/${policyId}/detail/base`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getPolicyPremium(policyId: string): Observable<PolicyPremiumDetailResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<PolicyPremiumDetailResourceModel>(
            this.baseUrl + `/${policyId}/detail/premium`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getPolicyQuestions(policyId: string): Observable<PolicyQuestionDetailResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<PolicyQuestionDetailResourceModel>(
            this.baseUrl + `/${policyId}/detail/questions`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getPolicyDocuments(policyId: string): Observable<PolicyDocumentsDetailResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<PolicyDocumentsDetailResourceModel>(
            this.baseUrl + `/${policyId}/detail/documents`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public cancelPolicy(policyId: string): Observable<QuoteCreateResultModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.post<QuoteCreateResultModel>(
            this.baseUrl + `/${policyId}/cancel`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public adjustPolicy(policyId: string): Observable<QuoteCreateResultModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.post<QuoteCreateResultModel>(
            this.baseUrl + `/${policyId}/adjust`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public renewPolicy(policyId: string): Observable<QuoteCreateResultModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.post<QuoteCreateResultModel>(
            this.baseUrl + `/${policyId}/renew`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public exportPolicy(exportType: string, filters?: Map<string, string | Array<string>>): Observable<Blob> {
        let params: HttpParams = new HttpParams();
        params = params.append('format', exportType);
        params = params.append('environment', this.appConfigService.getEnvironment());

        if (filters) {
            filters.forEach((value: any, key: string) => params = params.append(key, value));
        }
        return this.api.export(ApiHelper.exportPolicy.route, params).pipe(
            map((res: Blob) => res),
        );
    }

    public updatePolicyNumber(
        policyId: string,
        policyNumber?: string,
        returnOldPolicyNumberToPool: boolean = false,
    ): Observable<PolicyResourceModel> {
        const model: UpdatePolicyNumberResourceModel = {
            policyNumber: policyNumber,
            returnOldPolicyNumberToPool: returnOldPolicyNumberToPool,
        };
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.patch<PolicyResourceModel>(
            `${this.baseUrl}/${policyId}/update-policy-number`,
            model,
            ApiHelper.toHttpOptions(params));
    }
}
