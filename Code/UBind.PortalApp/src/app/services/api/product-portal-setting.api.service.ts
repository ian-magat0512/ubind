import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ProductPortalSettingResourceModel } from '@app/resource-models/product-portal-setting.resource-model';
import { HttpClient } from '@angular/common/http';
import { ApiHelper } from '@app/helpers/api.helper';
import { AppConfigService } from '../app-config.service';
import { AppConfig } from '@app/models/app-config';

/**
 * Portal product setting API service class.
 */
@Injectable({ providedIn: 'root' })
export class ProductPortalSettingApiService {
    private baseUrl: string;
    private portalId: string;
    private tenantId: string;

    public constructor(private httpClient: HttpClient, private appConfigService: AppConfigService) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            this.baseUrl = appConfig.portal.api.baseUrl;
            this.portalId = appConfig.portal.portalId;
            this.tenantId = appConfig.portal.tenantId;
        });
    }

    public getList(
        portal: string,
        params?: Map<string, string | Array<string>>,
    ): Observable<Array<ProductPortalSettingResourceModel>> {
        return this.httpClient.get<Array<ProductPortalSettingResourceModel>>(
            this.baseUrl + `portal/${portal}/product`,
            ApiHelper.toHttpOptions(params),
        );
    }

    public getByTenant(portal: string, tenant: string): Observable<Array<ProductPortalSettingResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', tenant);
        return this.getList(portal, params);
    }

    public enable(
        portal: string,
        tenant: string,
        product: string,
    ): Observable<ProductPortalSettingResourceModel> {
        return this.httpClient.patch<ProductPortalSettingResourceModel>(
            this.baseUrl + `portal/${portal}/product/${product}/enable?tenant=${tenant}`,
            null,
        );
    }

    public disable(
        portal: string,
        tenant: string,
        product: string,
    ): Observable<ProductPortalSettingResourceModel> {
        return this.httpClient.patch<ProductPortalSettingResourceModel>(
            this.baseUrl + `portal/${portal}/product/${product}/disable?tenant=${tenant}`,
            null,
        );
    }

    public async anyProductHasNewQuotesAllowed(): Promise<boolean> {
        let portals: Array<ProductPortalSettingResourceModel> = await this.getByPortal();
        let filtered: Array<ProductPortalSettingResourceModel> =
            portals.filter((q: ProductPortalSettingResourceModel) => q.isNewQuotesAllowed);
        return filtered.length > 0;
    }

    public async getByPortal(): Promise<Array<ProductPortalSettingResourceModel>> {
        let params: Map<string, string | Array<string>> = new Map<string, string | Array<string>>();
        params.set('tenant', this.tenantId);
        let result: any = this.getList(this.portalId, params);
        return result.toPromise();
    }
}
