import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiHelper } from '@app/helpers/api.helper';
import { CustomerResourceModel, CustomerDetailsResourceModel } from '@app/resource-models/customer.resource-model';
import { PersonAccountCreateModel, PersonResourceModel } from '@app/resource-models/person.resource-model';
import { EntityLoaderSaverService } from '../entity-loader-saver.service';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
/**
 * Service for handling request to customer api endpoint.
 */
@Injectable({ providedIn: 'root' })
export class CustomerApiService implements EntityLoaderSaverService<CustomerResourceModel> {
    private baseUrl: string;
    private environment: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = `${appConfig.portal.api.baseUrl}customer`;
            this.environment = appConfig.portal.environment;
        });
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<CustomerResourceModel>> {
        params = this.setEnvironment(params);
        return this.httpClient.get<Array<CustomerResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }

    public getById(id: string, params?: Map<string, string | Array<string>>): Observable<CustomerResourceModel> {
        params = this.setEnvironment(params);
        return this.httpClient.get<CustomerResourceModel>(`${this.baseUrl}/${id}`, ApiHelper.toHttpOptions(params));
    }

    public getCustomersByOwner(
        ownerUserId: string,
        pageParams?: Map<string, string | Array<string>>,
    ): Observable<Array<CustomerResourceModel>> {
        const params: any = this.setEnvironment(new Map<string, string | Array<string>>());
        params.set('ownerUserId', ownerUserId);
        if (pageParams) {
            params.set('page', pageParams.get('page'));
            params.set('pageSize', pageParams.get('pageSize'));
        }
        return this.getList(params);
    }

    public getCustomerDetails(id: string): Observable<CustomerDetailsResourceModel> {
        const params: any = this.setEnvironment(new Map<string, string | Array<string>>());
        return this.httpClient.get<CustomerDetailsResourceModel>(
            `${this.baseUrl}/${id}/detail`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public create(model: PersonResourceModel): Observable<CustomerResourceModel> {
        return this.httpClient.post<CustomerResourceModel>(
            this.baseUrl,
            model,
            { params: { environment: this.appConfigService.getEnvironment() } },
        );
    }

    public createCustomerAccount(model: PersonAccountCreateModel): Observable<CustomerResourceModel> {
        return this.httpClient.post<CustomerResourceModel>(
            `${this.baseUrl}/create-with-account`,
            model,
            { params: { environment: this.appConfigService.getEnvironment() } },
        );
    }

    public update(customerId: string, model: PersonResourceModel): Observable<CustomerResourceModel> {
        return this.httpClient.put<CustomerResourceModel>(`${this.baseUrl}/${customerId}`, model);
    }

    public assignOwner(customerId: string, ownerUserId: string): Observable<void> {
        let params: HttpParams = new HttpParams().set('ownerUserId', ownerUserId);
        return this.httpClient.patch<void>(`${this.baseUrl}/${customerId}/owner`, null, { params });
    }

    public getPrimaryPerson(
        customerId: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<PersonResourceModel> {
        let updatedParams: Map<string, string | Array<string>> = this.setEnvironment(params);
        return this.httpClient.get<PersonResourceModel>(
            `${this.baseUrl}/${customerId}/person/primary`,
            ApiHelper.toHttpOptions(updatedParams),
        );
    }

    public setPersonToPrimaryForCustomer(customerId: string, personId: string): Observable<PersonResourceModel> {
        return this.httpClient.patch<PersonResourceModel>(
            `${this.baseUrl}/${customerId}`,
            { primaryPersonId: `${personId}` },
        );
    }

    public assignPortalToCustomer(customerId: string, portalId: string): Observable<void> {
        return this.httpClient.patch<void>(`${this.baseUrl}/${customerId}/portal/${portalId}`, {});
    }

    public unassignPortalFromCustomer(customerId: string): Observable<void> {
        return this.httpClient.delete<void>(`${this.baseUrl}/${customerId}/portal`, {});
    }

    private setEnvironment(params: Map<string, string | Array<string>>): Map<string, string | Array<string>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.environment);

        return params;
    }
}
