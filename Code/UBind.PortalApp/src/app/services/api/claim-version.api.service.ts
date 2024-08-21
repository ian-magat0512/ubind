import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { ClaimVersionResourceModel } from '@app/resource-models/claim.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { HttpClient } from '@angular/common/http';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '../app-config.service';

/**
 * Claim version api service.
 */
@Injectable({ providedIn: 'root' })
export class ClaimVersionApiService implements EntityLoaderService<ClaimVersionResourceModel> {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'claim';
        });
    }
    public getList(params?: Map<string, string | Array<string>>): Observable<Array<ClaimVersionResourceModel>> {
        const claimId: any = params.get('claimId');
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<ClaimVersionResourceModel>>(
            this.baseUrl + `/${claimId}/version`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getById(
        claimVersionId: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<ClaimVersionResourceModel> {
        console.log(claimVersionId);
        const claimId: any = params.get('claimId');
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<ClaimVersionResourceModel>(
            this.baseUrl + `/${claimId}/version/${claimVersionId}`,
            ApiHelper.toHttpOptions(params),
        );
    }
}
