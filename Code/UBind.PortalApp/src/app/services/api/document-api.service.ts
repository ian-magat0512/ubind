import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { ApiService } from '@app/services/api.service';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '../../models/app-config';

/**
 * Export Document API service class.
 * TODO: Write a better class header: documents API functions.
 */
@Injectable({ providedIn: 'root' })
export class DocumentApiService {

    private environment: string = '';

    public constructor(private apiService: ApiService, private appConfigService: AppConfigService) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.environment = appConfig.portal.environment;
        });
    }

    public downloadDocument(documentId: string, quoteEmailId: string): Observable<any | Blob> {
        let url: string = ApiHelper.document.route + `${documentId}` + "?quoteEmailId=" + quoteEmailId;
        return this.apiService.getBlob(url);
    }

    public downloadQuoteDocument(documentId: string, quoteId: string): Observable<any | Blob> {
        let url: string = `${ApiHelper.quoteDocument.route}${documentId}`;
        const params: HttpParams = new HttpParams()
            .set(ApiHelper.quoteDocument.params.quoteId, quoteId);
        return this.apiService.getBlob(url, params);
    }

    public downloadQuoteVersionDocument(documentId: string, quoteVersionId: string): Observable<any | Blob> {
        let url: string = `${ApiHelper.quoteVersionDocument.route}${documentId}`;
        const params: HttpParams = new HttpParams()
            .set(ApiHelper.quoteVersionDocument.params.quoteVersionId, quoteVersionId);
        return this.apiService.getBlob(url, params);
    }

    public downloadClaimDocument(documentId: string, claimId: string): Observable<any | Blob> {
        let url: string = `/claim/${claimId}/documents/${documentId}`;
        return this.apiService.getBlob(url);
    }

    public downloadPolicyDocument(policyId: string, transactionId: string, documentId: string): Observable<any | Blob> {
        let url: string = `/policy/${policyId}/transaction/${transactionId}/document/${documentId}`;
        return this.apiService.getBlob(url);
    }
}
