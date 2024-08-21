import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { AppConfigService } from "../app-config.service";
import { AppConfig } from "@app/models/app-config";
import { Observable } from "rxjs";

/**
 * API service for SAML operations
 */
@Injectable({
    providedIn: 'root',
})
export class SamlApiService {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = `${appConfig.portal.api.baseUrl}`;
        });
    }

    public getMetadataUrl(tenant: string, authenticationMethodId: string): string {
        return `${this.baseUrl}tenant/${tenant}/saml/${authenticationMethodId}/metadata`;
    }

    public getMetadata(tenant: string, authenticationMethodId: string): Observable<string> {
        const url: string = this.getMetadataUrl(tenant, authenticationMethodId);
        return this.httpClient.get<string>(
            url,
            { responseType: 'text' as 'json' });
    }
}
