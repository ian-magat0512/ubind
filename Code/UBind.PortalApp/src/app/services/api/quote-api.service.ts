import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { AppConfigService } from '@app/services/app-config.service';
import {
    QuoteResourceModel,
    QuoteDetailResourceModel,
    QuoteFormDataResourceModel,
    QuoteCreateResultModel,
    QuoteCreateResourceModel,
    QuotePeriodicSummaryModel,
} from '@app/resource-models/quote.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { AppConfig } from '@app/models/app-config';
import {
    CustomerQuoteAssociationResultModel, CustomerQuoteAssociationVerificationResultModel,
} from '@app/resource-models/customer.resource-model';
import { IssuePolicyResourceModel, PolicyIssuanceResourceModel } from '@app/resource-models/policy.resource-model';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';

/**
 * Export Quote API service class.
 * TODO: Write a better class header: quote API functions.
 */
@Injectable({ providedIn: 'root' })
export class QuoteApiService implements EntityLoaderService<QuoteResourceModel> {
    private baseUrl: string;
    private portalId: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
            this.portalId = appConfig.portal.portalId;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<QuoteResourceModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<QuoteResourceModel>>(`${this.baseUrl}quote`, ApiHelper.toHttpOptions(params));
    }

    public getCount(params?: Map<string, string | Array<string>>): Observable<number> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<number>(`${this.baseUrl}quote/count`, ApiHelper.toHttpOptions(params));
    }

    public getPeriodicSummaries(params?: Map<string, string | Array<string>>):
        Observable<Array<QuotePeriodicSummaryModel> | QuotePeriodicSummaryModel> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        let environment: DeploymentEnvironment = this.appConfigService.getEnvironment();
        if (environment != DeploymentEnvironment.Production) {
            params.set('environment', this.appConfigService.getEnvironment());
        }
        return this.httpClient.get<Array<QuotePeriodicSummaryModel>>(
            `${this.baseUrl}quote/periodic-summary`, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<QuoteResourceModel> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<QuoteResourceModel>(`${this.baseUrl}quote/${id}`, ApiHelper.toHttpOptions(params));
    }

    public createNewBusinessQuote(
        organisation: string,
        product: string,
        customerId?: string,
        isTestData: boolean = false,
        productRelease: string = null,
    ): Observable<any> {
        let createQuoteResourceModel: QuoteCreateResourceModel = {
            organisation: organisation,
            portal: this.portalId,
            product: product,
            customerId: customerId,
            isTestData: isTestData,
            productRelease: productRelease,
            environment: this.appConfigService.getEnvironment(),
        };
        return this.httpClient.post<QuoteCreateResultModel>(`${this.baseUrl}quote`, createQuoteResourceModel);
    }

    public discardQuote(quoteId: string): Observable<any> {
        return this.httpClient.put(`${this.baseUrl}quote/${quoteId}/discard`, null);
    }

    public getQuoteNumber(quoteId: string): Observable<string> {
        return this.httpClient.get(`${this.baseUrl}quote/${quoteId}/quoteNumber`, { responseType: 'text' });
    }

    public getQuoteDetails(quoteId: string): Observable<QuoteDetailResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<QuoteDetailResourceModel>(
            `${this.baseUrl}quote/${quoteId}/detail`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getQuoteFormData(quoteId: any): Observable<QuoteFormDataResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<QuoteFormDataResourceModel>(
            `${this.baseUrl}quote/${quoteId}/form-data`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public associateWithCustomer(
        associationInvitationId: string,
        quoteId: string,
    ): Observable<CustomerQuoteAssociationResultModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('quoteId', quoteId);
        return this.httpClient.post<CustomerQuoteAssociationResultModel>(
            `${this.baseUrl}customer-association-invitation/${associationInvitationId}/associate`,
            null,
            ApiHelper.toHttpOptions(params),
        );
    }

    public verifyCustomerAssociationInvitation(
        associationInvitationId: string,
        quoteId: string,
    ): Observable<CustomerQuoteAssociationVerificationResultModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('quoteId', quoteId);
        return this.httpClient.post<CustomerQuoteAssociationVerificationResultModel>(
            `${this.baseUrl}customer-association-invitation/${associationInvitationId}/verify`,
            null,
            ApiHelper.toHttpOptions(params),
        );
    }

    public expireNow(quoteId: string): Observable<QuoteResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        return this.httpClient.put<QuoteResourceModel>(
            `${this.baseUrl}quote/${quoteId}/expire`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public setExpiryDateTime(quoteId: string, dateTime: Date): Observable<QuoteResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('datetime', dateTime.toISOString());
        return this.httpClient.put<QuoteResourceModel>(
            `${this.baseUrl}quote/${quoteId}/set-expiry`,
            dateTime,
            ApiHelper.toHttpOptions(params),
        );
    }

    public clone(quoteId: string): Observable<QuoteCreateResultModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        return this.httpClient.put<QuoteCreateResultModel>(
            `${this.baseUrl}quote/${quoteId}/clone`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public issuePolicy(quoteId: string, policyNumber?: string): Observable<PolicyIssuanceResourceModel> {
        const model: IssuePolicyResourceModel = {
            policyNumber: policyNumber,
        };

        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.post<PolicyIssuanceResourceModel>(
            `${this.baseUrl}quote/${quoteId}/issue-policy`,
            model,
            ApiHelper.toHttpOptions(params));
    }
}
