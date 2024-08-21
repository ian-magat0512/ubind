import { Injectable } from '@angular/core';
import { ApplicationService } from '../application.service';
import { MessageService } from '../message.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ApiService } from '../api.service';
import { HttpHeadersFactory, MediaType } from '@app/helpers/http-headers-factory';
import { QuoteCreateModel } from '@app/resource-models/quote-create-model';
import { DeploymentEnvironment } from '@app/models/deployment-environment';
import { QuoteType } from '@app/models/quote-type.enum';
import { UnifiedFormModelService } from '../unified-form-model.service';
import { StringHelper } from '@app/helpers/string.helper';

/**
 * Provides functions which call the API to save or retreive data in relation to Quotes.
 */
@Injectable()
export class QuoteApiService {
    public constructor(
        protected messageService: MessageService,
        private httpClient: HttpClient,
        private applicationService: ApplicationService,
        public apiService: ApiService,
        private unifiedFormModelService: UnifiedFormModelService,
    ) {
    }

    /**
     * tries to trigger events if any when opening a quote.
     */
    public triggerEventsWhenQuoteIsOpened(quoteId: string): Promise<Array<string>> {
        let apiUrl: string = `${this.getBaseUrl()}/${quoteId}/event/open`;
        const body: object = { tenant: this.applicationService.tenantAlias };
        return new Promise((resolve: any, reject: any): void => {
            this.httpClient.post(apiUrl, body, {}).toPromise()
                .then((eventIds: Array<string>) => {
                    resolve(eventIds);
                }, (error: any) => {
                    reject(error);
                });
        });
    }

    public getQuoteState(quoteId: string): Promise<string> {
        let apiUrl: string = `${this.getBaseUrl()}/${quoteId}/quoteState`;
        const options: object = { params: { tenantId: this.applicationService.tenantAlias } };
        return new Promise((resolve: any, reject: any): void => {
            this.httpClient.get<string>(apiUrl, options).toPromise()
                .then((quoteState: string) => {
                    resolve(quoteState.toLowerCase());
                },
                (error: any) => {
                    reject(error);
                });
        });
    }

    public createNewBusinessQuote(
        tenantId: string,
        organisationAlias: string,
        portalId: string,
        productId: string,
        environment: DeploymentEnvironment,
        isTestData: boolean,
        productRelease: string,
    ): Promise<void> {
        let quoteCreateModel: QuoteCreateModel = {
            tenant: tenantId,
            portal: portalId,
            organisation: organisationAlias,
            product: productId,
            environment: environment,
            quoteType: QuoteType.NewBusiness,
            isTestData: isTestData,
            customerId: this.applicationService.customerId,
            productRelease: productRelease,
        };
        return this.createQuote(quoteCreateModel);
    }

    public createRenewalQuote(tenantId: string, policyId: string, productRelease: string): Promise<void> {
        let quoteCreateModel: QuoteCreateModel = {
            tenant: tenantId,
            quoteType: QuoteType.Renewal,
            policyId: policyId,
            discardExistingQuote: true,
            productRelease: productRelease,
        };
        return this.createQuote(quoteCreateModel);
    }

    public createAdjustmentQuote(tenantId: string, policyId: string, productRelease: string): Promise<void> {
        let quoteCreateModel: QuoteCreateModel = {
            tenant: tenantId,
            quoteType: QuoteType.Adjustment,
            policyId: policyId,
            discardExistingQuote: true,
            productRelease: productRelease,
        };
        return this.createQuote(quoteCreateModel);
    }

    public createCancellationQuote(tenantId: string, policyId: string, productRelease: string): Promise<void> {
        let quoteCreateModel: QuoteCreateModel = {
            tenant: tenantId,
            quoteType: QuoteType.Cancellation,
            policyId: policyId,
            discardExistingQuote: true,
            productRelease: productRelease,
        };
        return this.createQuote(quoteCreateModel);
    }

    private getBaseUrl(): string {
        return this.applicationService.apiOrigin + '/api/v1/quote';
    }

    private createQuote(quoteCreateModel: QuoteCreateModel): Promise<void> {
        const headers: HttpHeaders = HttpHeadersFactory
            .create()
            .withContentType(MediaType.Json)
            .build();
        let apiUrl: string = `${this.getBaseUrl()}`;
        return this.httpClient.post(apiUrl, quoteCreateModel, { headers }).toPromise()
            .then((response: object) => {
                this.applicationService.quoteId = response['quoteId'];
                if (response['productReleaseId']) {
                    this.applicationService.productReleaseId = response['productReleaseId'];
                }
                if (response['isTestData']) {
                    this.applicationService.isTestData = response['isTestData'];
                }
                if (response['currentUser']) {
                    this.applicationService.currentUser = response['currentUser'];
                }
                if (response['formModel']) {
                    this.unifiedFormModelService.apply(response['formModel']);
                }
                if (response['quoteType']) {
                    this.applicationService.quoteType = <QuoteType>StringHelper.toCamelCase(response['quoteType']);
                }
            });
    }
}
