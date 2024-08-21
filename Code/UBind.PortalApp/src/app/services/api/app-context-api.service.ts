import { Injectable } from "@angular/core";
import { AppConfigService } from "../app-config.service";
import { HttpClient } from "@angular/common/http";
import { AppConfig } from "@app/models/app-config";
import { PortalAppContextModel } from "@app/models/portal-app-context.model";
import { Observable } from "rxjs";
import { ApiHelper } from "@app/helpers/api.helper";

/**
 * API service for making API calls about the portal or forms app context
 */
@Injectable({
    providedIn: 'root',
})
export class AppContextApiService {
    private baseUrl: string;

    public constructor(
        appConfigService: AppConfigService,
        private httpClient: HttpClient,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = `${appConfig.portal.api.baseUrl}app-context`;
        });
    }

    public getPortalAppContext(
        tenant: string,
        organisation?: string,
        portal?: string,
    ): Observable<PortalAppContextModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        if (organisation) {
            params.set('organisation', organisation);
        }
        if (portal) {
            params.set('portal', portal);
        }
        return this.httpClient.get<PortalAppContextModel>(
            `${this.baseUrl}/portal`,
            ApiHelper.toHttpOptions(params));
    }
}
