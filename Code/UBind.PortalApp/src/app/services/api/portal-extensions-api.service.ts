import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { PortalPageTriggerResourceModel } from '@app/resource-models/portal-page-trigger.resource-model';
import { ApiService } from '@app/services/api.service';
import { ApiHelper } from '@app/helpers';
import { SharedAlertService } from "@app/services/shared-alert.service";

/**
 * Portal extensions Api Service.
 * This class manage the API services of Portal extensions.
 */
@Injectable({ providedIn: 'root' })
export class PortalExtensionsApiService {

    private portalExtUrl: string;
    private environment: string;

    public constructor(
        private httpClient: HttpClient,
        protected appConfigService: AppConfigService,
        public apiService: ApiService,
        public sharedAlertService: SharedAlertService,
    ) {
        appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.portalExtUrl = `${appConfig.portal.api.baseUrl}portal-extensions`;
            this.environment = appConfig.portal.environment;
        });
    }

    public getPortalPageTriggers(tenantId: string): Observable<Array<PortalPageTriggerResourceModel>> {
        return this.httpClient.get<Array<PortalPageTriggerResourceModel>>(
            `${this.portalExtUrl}/tenant/${tenantId}/environment/${this.environment}`,
        );
    }

    public executePortalPageTrigger(
        portalPageTrigger: PortalPageTriggerResourceModel,
        entityType: string,
        pageType: string,
        tab: string,
        entityId: string,
        filters: Map<string, string | Array<string>>,
    ): Observable<any> {
        filters.set('tenantId', portalPageTrigger.tenantId);
        filters.set('environment', this.environment);
        filters.set('observe', 'response');
        filters.set('responseType', 'blob' as 'json');
        const options: any = {
            observe: 'response',
            responseType: 'blob' as 'json',
            params: ApiHelper.toHttpParams(filters),
        };
        let url: string =
            `${this.portalExtUrl}/${portalPageTrigger.tenantId}/product/${portalPageTrigger.productId}`
            + `/environment/${this.environment}/automation/${portalPageTrigger.automationAlias}`
            + `/trigger/${portalPageTrigger.triggerAlias}/${entityType}/${pageType}/${tab}/${entityId}`;
        return this.httpClient.get(url, options);
    }
}
