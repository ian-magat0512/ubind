import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '../../models/app-config';

/**
 * Service for handling startup jobs API functions.
 */
@Injectable({ providedIn: 'root' })
export class StartupJobApiService {

    private baseUrl: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
        });
    }

    public getStartupJobStatus(startupJobAlias: string): Observable<string> {
        return this.httpClient.get<string>(this.baseUrl + `startup-job/${startupJobAlias}/status`,
            { responseType: 'text' as 'json' });
    }
}
