import { Injectable } from '@angular/core';
import { HttpParams, HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppConfigService } from '@app/services/app-config.service';
import { DeploymentResourceModel } from '@app/models';
import { DeploymentEnvironment } from '@app/models/deployment-environment.enum';
import { AppConfig } from '../../models/app-config';

/**
 * Export deployment API service class.
 * TODO: Write a better class header: deployment API functions.
 */
@Injectable({ providedIn: 'root' })
export class DeploymentApiService {

    private baseUrl: string;

    public constructor(
        private http: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
        });
    }

    public getCurrentDeployments(tenantId: string, productId: string): Observable<Array<DeploymentResourceModel>> {
        const url: string = `${tenantId}/${productId}/deployments/current`;
        return this.http.get(this.baseUrl + url).pipe(map((res: Array<DeploymentResourceModel>) => res));
    }

    public getCurrentDeploymentForRelease(
        tenantId: string,
        releaseId: string,
        productId: string,
    ): Observable<Array<DeploymentResourceModel>> {
        const url: string = `${tenantId}/${productId}/deployments/current/release/${releaseId}`;
        const params: Array<any> = [];
        // return this.api.get(url, this.generateRequestOptions(params)).pipe(
        return this.http.get(this.baseUrl + url, { params: this.generateRequestOptions(params) })
            .pipe(map((res: Array<DeploymentResourceModel>) => res));
    }

    public create(
        tenantId: string,
        productId: string,
        environment: DeploymentEnvironment,
        releaseId?: string,
    ): Observable<Response> {
        const url: string = `${tenantId}/${productId}/deployments/current/${environment}`;
        const body: any = { id: releaseId };
        const options: HttpParams = this.generateRequestOptions([]);
        return this.http.post(this.baseUrl + url, body, { params: options })
            .pipe(map((res: Response) => res));
    }

    private generateRequestOptions(searchParams: Array<any>): HttpParams {
        let params: HttpParams = new HttpParams();
        searchParams.forEach((element: any) => {
            params = params.append(element.label, element.value);
        });
        return params;
    }
}
