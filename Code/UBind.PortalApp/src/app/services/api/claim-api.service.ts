import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiHelper } from '@app/helpers/api.helper';
import {
    ClaimResourceModel, ClaimEmailResourceModel, ClaimVersionListResourceModel,
    ClaimVersionResourceModel,
    ClaimCreateModel,
    ClaimPeriodicSummaryModel,
} from '@app/resource-models/claim.resource-model';
import { EntityLoaderService } from '../entity-loader.service';
import { HttpClient } from '@angular/common/http';
import { AppConfig } from '@app/models/app-config';
import { AppConfigService } from '../app-config.service';
import { EventService } from '../event.service';
import { OrganisationModel } from '@app/models/organisation.model';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';

/**
 * Export Claim API service class.
 * TODO: Write a better class header: claim API functions.
 */
@Injectable({ providedIn: 'root' })
export class ClaimApiService implements EntityLoaderService<ClaimResourceModel> {

    private baseUrl: string;
    private performingUserOrganisationId: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
        private eventService: EventService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl + 'claim';
        });
        this.eventService.performingUserOrganisationSubject$.subscribe((organisation: OrganisationModel) => {
            this.performingUserOrganisationId = organisation.id;
        });
    }

    public getPeriodicSummaries(params?: Map<string, string | Array<string>>):
        Observable<Array<ClaimPeriodicSummaryModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        let environment: DeploymentEnvironment = this.appConfigService.getEnvironment();
        if (environment != DeploymentEnvironment.Production) {
            params.set('environment', this.appConfigService.getEnvironment());
        }
        return this.httpClient.get<Array<ClaimPeriodicSummaryModel>>(
            `${this.baseUrl}/periodic-summary`, ApiHelper.toHttpOptions(params));
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<ClaimResourceModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<Array<ClaimResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getByPolicyId(
        policyId: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<Array<ClaimResourceModel>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('policyId', policyId);
        params.set('environment', this.appConfigService.getEnvironment());
        return this.getList(params);
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<ClaimResourceModel> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get<ClaimResourceModel>(this.baseUrl + `/${id}`, ApiHelper.toHttpOptions(params));
    }

    public createClaimForPolicy(policyId: string): Observable<ClaimResourceModel> {
        let createClaimResourceModel: ClaimCreateModel = {
            policyId: policyId,
        };
        return this.httpClient.post<ClaimResourceModel>(this.baseUrl, createClaimResourceModel);
    }

    public createClaimForProduct(productAlias: string, customerId: string = null): Observable<ClaimResourceModel> {
        let createClaimResourceModel: ClaimCreateModel = {
            product: productAlias,
            customerId: customerId,
            environment: this.appConfigService.getEnvironment(),
            organisation: this.performingUserOrganisationId,
        };
        return this.httpClient.post<ClaimResourceModel>(this.baseUrl, createClaimResourceModel);
    }

    public getClaimsEmails(id: string): Observable<ClaimEmailResourceModel> {
        return Observable.create((o: any) => {
            const claimEmail: ClaimEmailResourceModel = {
                customerEmail: 'Firgy Firgusson',
                createdDateTime: '2020-05-06 10:00',
                emailSubject: 'Your claim has been lodged.',
            };
            o.next(claimEmail);
            o.complete();
        });
    }

    public getClaimVersions(claimId: string): Observable<Array<ClaimVersionListResourceModel>> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get(
            this.baseUrl + `/${claimId}/version`,
            ApiHelper.toHttpOptions(params),
        ).pipe(map((res: Array<ClaimVersionListResourceModel>) => res));
    }

    public getClaimVersionDetail(
        claimId: string,
        claimVersionId: string,
    ): Observable<ClaimVersionResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());
        return this.httpClient.get(
            this.baseUrl + `/${claimId}/version/${claimVersionId}`,
            ApiHelper.toHttpOptions(params),
        ).pipe(map((res: ClaimVersionResourceModel) => res));
    }

    public assignClaimNumber(
        number: string,
        claimId: string,
        isReusePreviousNumber: boolean = false,
    ): Observable<Response> {
        const claimModel: any = {
            claimNumber: number,
            isRestoreToList: isReusePreviousNumber,
        };

        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());

        return this.httpClient.patch(
            this.baseUrl + `/${claimId}/assign-claim-number`,
            claimModel,
            ApiHelper.toHttpOptions(params),
        ).pipe(map((res: Response) => res));
    }

    public unassignClaimNumber(
        number: string,
        claimId: string,
        isReusePreviousNumber: boolean = false,
    ): Observable<Response> {
        const claimModel: any = {
            claimNumber: number,
            isRestoreToList: isReusePreviousNumber,
        };

        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());

        return this.httpClient.patch(
            this.baseUrl + `/${claimId}/unassign-claim-number`,
            claimModel,
            ApiHelper.toHttpOptions(params),
        ).pipe(map((res: Response) => res));
    }

    public withdrawClaim(claimId: string): Observable<ClaimResourceModel> {
        const params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('environment', this.appConfigService.getEnvironment());

        return this.httpClient.patch(this.baseUrl + `/${claimId}/withdraw`, ApiHelper.toHttpOptions(params))
            .pipe(map((res: ClaimResourceModel) => res));
    }
}
