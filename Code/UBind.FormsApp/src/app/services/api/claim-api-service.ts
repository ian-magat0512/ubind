import { Injectable } from '@angular/core';
import { ApplicationService } from '../application.service';
import { MessageService } from '../message.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ApiService } from '../api.service';
import { HttpHeadersFactory, MediaType } from '@app/helpers/http-headers-factory';

/**
 * Provides functions which call the API to save or retreive data in relation to Claims.
 */
@Injectable()
export class ClaimApiService {
    public constructor(
        protected messageService: MessageService,
        private httpClient: HttpClient,
        private applicationService: ApplicationService,
        public apiService: ApiService,
    ) {
    }

    /**
     * tries to trigger events if any when opening a quote.
     */
    public triggerEventsWhenClaimIsOpened(claimId: string): Promise<Array<string>> {
        let apiUrl: string = `${this.getBaseUrl()}/${claimId}/event/open`;
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

    public getClaimState(claimId: string): Promise<string> {
        let apiUrl: string = `${this.getBaseUrl()}/${claimId}/claimState`;
        const options: object = { params: { tenant: this.applicationService.tenantAlias } };
        return new Promise((resolve: any, reject: any): void => {
            this.httpClient.get<string>(apiUrl, options).toPromise()
                .then((claimState: string) => {
                    resolve(claimState.toLowerCase());
                },
                (error: any) => {
                    reject(error);
                });
        });
    }

    public createNewClaim(
        tenantId: string,
        organisationAlias: string,
        productId: string,
        environment: string,
        isTestData: boolean): Promise<void> {
        const headers: HttpHeaders = HttpHeadersFactory
            .create()
            .withContentType(MediaType.Json)
            .build();

        let apiUrl: string = `${this.getBaseUrl()}`;
        let postData: any = {
            tenant: tenantId,
            organisation: organisationAlias,
            product: productId,
            environment: environment,
            type: 0,
        };
        if (isTestData) {
            postData.isTestData = true;
        }
        if (this.applicationService.customerId) {
            postData.customerId = this.applicationService.customerId;
        }
        return new Promise((resolve: any, reject: any): void => {
            this.httpClient.post(apiUrl, postData, { headers }).toPromise()
                .then((response: object) => {
                    this.applicationService.claimId = response['id'];
                    if (response['isTestData']) {
                        this.applicationService.isTestData = response['isTestData'];
                    }
                    if (response['currentUser']) {
                        this.applicationService.currentUser = response['currentUser'];
                    }
                    resolve();
                });
        });
    }

    private getBaseUrl(): string {
        return this.applicationService.apiOrigin + '/api/v1/claim';
    }
}
