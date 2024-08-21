import { EntityLoaderSaverService } from "../entity-loader-saver.service";
import { PersonResourceModel, PersonCreateModel, PersonUpdateResourceModel } from "@app/models";
import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { AppConfigService } from "../app-config.service";
import { AppConfig } from "@app/models/app-config";
import { ApiHelper } from "@app/helpers";
import { Observable } from "rxjs";
import { CustomerResourceModel } from "@app/resource-models/customer.resource-model";

/**
 * This class handles all person related http api requests.
 */
@Injectable({ providedIn: 'root' })
export class PersonApiService implements EntityLoaderSaverService<PersonResourceModel> {
    private baseUrl: string;
    private tenantAlias: string;
    private environment: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = `${appConfig.portal.api.baseUrl}person`;
            this.tenantAlias = appConfig.portal.tenantAlias;
            this.environment = appConfig.portal.environment;
        });
    }

    public create(model: PersonCreateModel): Observable<PersonResourceModel> {
        return this.httpClient.post<PersonResourceModel>(
            this.baseUrl,
            model,
            { params: { environment: this.appConfigService.getEnvironment() } },
        );
    }

    public update(personId: string, model: PersonUpdateResourceModel): Observable<PersonResourceModel> {
        return this.httpClient.put<PersonResourceModel>(`${this.baseUrl}/${personId}`, model);
    }

    public getList(params?: Map<string, string | Array<string>>): Observable<Array<PersonResourceModel>> {
        params = this.setEnvironment(params);
        return this.httpClient.get<Array<PersonResourceModel>>(this.baseUrl, ApiHelper.toHttpOptions(params));
    }
    public getById(personId: string, params?: Map<string, string | Array<string>>): Observable<PersonResourceModel> {
        params = this.setEnvironment(params);
        return this.httpClient.get<PersonResourceModel>(`${this.baseUrl}/${personId}`, ApiHelper.toHttpOptions(params));
    }

    public delete(personId: string): Observable<boolean> {
        return this.httpClient.delete<boolean>(`${this.baseUrl}/${personId}`);
    }

    public createAccount(personId: string, ...args: Array<any>): Observable<CustomerResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params = this.setEnvironment(params);
        params = params.set("productId", args[0]);
        return this.httpClient.post<CustomerResourceModel>(
            `${this.baseUrl}/${personId}/account`,
            null,
            ApiHelper.toHttpOptions(params),
        );
    }

    public deactivateAccount(personId: string): Observable<CustomerResourceModel> {
        return this.httpClient.post<CustomerResourceModel>(`${this.baseUrl}/${personId}/account/block`, null);
    }

    public activateAccount(personId: string): Observable<CustomerResourceModel> {
        return this.httpClient.post<CustomerResourceModel>(`${this.baseUrl}/${personId}/account/unblock`, null);
    }

    private setEnvironment(params: Map<string, string | Array<string>>): Map<string, string | Array<string>> {
        if (!params) {
            params = new Map<string, string | Array<string>>();
        }
        params.set('environment', this.environment);
        params.set('tenant', this.tenantAlias);
        return params;
    }
}
