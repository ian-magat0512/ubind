import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { HttpHeaders, HttpClient } from '@angular/common/http';
import { NumberPoolGetResultModel } from '@app/models/number-pool-result.model';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { NumberPoolAddResultModel } from '@app/models/number-pool-add-result.model';
import { NumberPoolDeleteResultModel } from '@app/models/number-pool-delete-result.model';

/**
 * Export Number pool API service class.
 * TODO: Write a better class header: pool number API functions.
 */
@Injectable({ providedIn: 'root' })
export class NumberPoolApiService {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
        });
    }

    public getNumbers(
        tenantId: string,
        productId: string,
        numberPoolId: string,
        environment: DeploymentEnvironment = DeploymentEnvironment.Production,
    ): Observable<NumberPoolGetResultModel> {
        const url: string = this.baseUrl + 'tenant/' + tenantId + '/product/'
            + productId + '/number-pool/' + numberPoolId;
        return this.httpClient.get<NumberPoolGetResultModel>(url, { params: { environment: environment } });
    }

    public getAvailableNumbers(
        tenantId: string,
        productId: string,
        numberPoolId: string,
        environment: DeploymentEnvironment = DeploymentEnvironment.Production,
    ): Observable<NumberPoolGetResultModel> {
        const url: string = this.baseUrl + 'tenant/' + tenantId + '/product/'
            + productId + '/number-pool/' + numberPoolId + '/available';
        const result: Observable<NumberPoolGetResultModel>
            = this.httpClient.get<NumberPoolGetResultModel>(url, { params: { environment: environment } });
        return result;
    }

    public hasAvailableNumbers(
        tenantId: string,
        productId: string,
        numberPoolId: string,
        environment: DeploymentEnvironment = DeploymentEnvironment.Production,
    ): Observable<boolean> {
        const url: string = `${this.baseUrl}tenant/${tenantId}/product/${productId}
            /number-pool/${numberPoolId}/has-available-numbers`;
        return this.httpClient.get<boolean>(url, { params: { environment: environment } });
    }

    public saveNumbers(
        tenantId: string,
        productId: string,
        numberPoolId: string,
        numbers: Array<string>,
        environment: DeploymentEnvironment = DeploymentEnvironment.Production,
    ): Observable<NumberPoolAddResultModel> {
        const url: string = this.baseUrl + 'tenant/' + tenantId + '/product/'
            + productId + '/number-pool/' + numberPoolId;
        return this.httpClient.post<NumberPoolAddResultModel>(
            url,
            numbers,
            { params: { environment: environment } },
        );
    }

    public deleteNumbers(
        tenantId: string,
        productId: string,
        numberPoolId: string,
        numbers: Array<string>,
        environment: DeploymentEnvironment = DeploymentEnvironment.Production,
    ): Observable<NumberPoolDeleteResultModel> {
        const url: string = this.baseUrl + 'tenant/' + tenantId + '/product/'
            + productId + '/number-pool/' + numberPoolId;
        let options: any = {
            body: JSON.stringify(numbers),
            headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
            params: { environment: environment },
        };
        return this.httpClient.delete<NumberPoolDeleteResultModel>(url, options).pipe(map((res: any) => res));
    }
}
