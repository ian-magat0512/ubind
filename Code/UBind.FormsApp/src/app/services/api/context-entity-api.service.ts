import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApplicationService } from '../application.service';
import { HttpParams, HttpClient } from '@angular/common/http';
import { QuoteType } from '@app/models/quote-type.enum';

/**
 * Context entity API service class.
 */
@Injectable({
    providedIn: 'root',
})
export class ContextEntityApiService {
    public constructor(
        private httpClient: HttpClient,
        private applicationService: ApplicationService,
    ) {
    }

    public getContextEntities(
        organisation: string,
        formType: string,
        entityId: string,
        quoteType?: QuoteType,
    ): Observable<any> {
        let params: HttpParams = new HttpParams()
            .set('organisation', organisation)
            .set('formType', formType);
        if (quoteType) {
            params = params.set('quoteType', quoteType);
        }
        if (entityId) {
            params = params.set('entityId', entityId);
        }

        const url: string = `${this.getBaseUrl()}/context-entity`;
        return this.httpClient.get<any>(url, { params });
    }

    private getBaseUrl(): string {
        const tenantId: string = this.applicationService.tenantId;
        const productAlias: string = this.applicationService.productAlias;
        const productReleaseId: string = this.applicationService.productReleaseId;
        const environment: string = this.applicationService.environment;
        const baseUrl: string =
            `${this.applicationService.apiOrigin}/api/v1/tenant/${tenantId}` +
            `/product/${productAlias}/product-release/${productReleaseId}/environment/${environment}`;
        return baseUrl;
    }
}
