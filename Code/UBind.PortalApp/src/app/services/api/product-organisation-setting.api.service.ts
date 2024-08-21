import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ProductOrganisationSettingResourceModel }
    from '@app/resource-models/product-organisation-setting.resource-model';
import { HttpClient } from '@angular/common/http';
import { ApiHelper } from '@app/helpers/api.helper';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';
import { EventService } from '../event.service';
import { OrganisationModel } from '@app/models/organisation.model';

/**
 * Product organisation setting API service class.
 */
@Injectable({ providedIn: 'root' })
export class ProductOrganisationSettingApiService {
    private baseUrl: string;
    private performingUserOrganisationId: string;
    private tenantId: string;

    public constructor(
        private httpClient: HttpClient,
        private appConfigService: AppConfigService,
        private eventService: EventService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
            this.tenantId = appConfig.portal.tenantId;
        });
        this.eventService.performingUserOrganisationSubject$.subscribe((organisation: OrganisationModel) => {
            this.performingUserOrganisationId = organisation.id;
        });
    }

    public getList(
        organisation: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<Array<ProductOrganisationSettingResourceModel>> {
        return this.httpClient.get<Array<ProductOrganisationSettingResourceModel>>(
            this.baseUrl + `organisation/${organisation}/product`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getByOrganisation(
        organisation: string,
        tenant?: string,
    ): Observable<Array<ProductOrganisationSettingResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.getList(organisation, params);
    }

    public enable(
        organisation: string,
        product: string,
        tenant?: string,
    ): Observable<ProductOrganisationSettingResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<ProductOrganisationSettingResourceModel>(
            this.baseUrl + `organisation/${organisation}/product/${product}/enable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public disable(
        organisation: string,
        product: string,
        tenant?: string,
    ): Observable<ProductOrganisationSettingResourceModel> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        if (tenant) {
            params.set('tenant', tenant);
        }
        return this.httpClient.patch<ProductOrganisationSettingResourceModel>(
            this.baseUrl + `organisation/${organisation}/product/${product}/disable`,
            null,
            ApiHelper.toHttpOptions(params));
    }

    public async anyProductHasNewQuotesAllowed(productId: string = null): Promise<boolean> {
        let settings: Array<ProductOrganisationSettingResourceModel> = await this.getSettings();
        let filtered: Array<ProductOrganisationSettingResourceModel> =
            settings.filter((q: ProductOrganisationSettingResourceModel) =>
                q.isNewQuotesAllowed && (productId == null || q.productId == productId.toLowerCase()));
        return filtered.length > 0;
    }

    public async getSettings(): Promise<Array<ProductOrganisationSettingResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.tenantId);
        let result: any = this.getList(this.performingUserOrganisationId, params);
        return result.toPromise();
    }
}
