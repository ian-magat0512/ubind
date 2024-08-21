import { TenantApiService } from "./api/tenant-api.service";
import { TenantResourceModel } from "@app/resource-models/tenant.resource-model";
import { Injectable } from "@angular/core";
import { AppConfigService } from "./app-config.service";
import { AppConfig } from '@app/models/app-config';

/**
 * Export tenant service class.
 * This class manage tenant services functions.
 */
@Injectable({ providedIn: 'root' })
export class TenantService {
    private performingUserTenantId: string;

    public constructor(
        private tenantApiService: TenantApiService,
        private appConfigService: AppConfigService,
    ) {
        this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
            if (!this.appConfigService.isMasterPortal()) {
                this.performingUserTenantId = appConfig.portal.tenantId;
            }
        });
    }

    public async getTenantIdFromAlias(tenantAlias: string): Promise<string> {
        let id: string = await this.tenantApiService.getTenantId(tenantAlias).toPromise();
        return id;
    }

    public async getTenantNameFromAlias(tenantAlias: string): Promise<string> {
        let tenant: TenantResourceModel = await this.getTenantFromAlias(tenantAlias);
        return tenant.name;
    }

    public async getTenantDefaultOrganisationIdFromAlias(tenantAlias: string): Promise<string> {
        let tenant: TenantResourceModel = await this.getTenantFromAlias(tenantAlias);
        return tenant.organisationId;
    }

    public async getTenantFromAlias(tenantAlias: string): Promise<TenantResourceModel> {
        let tenant: TenantResourceModel = await this.tenantApiService.get(tenantAlias).toPromise();
        return tenant;
    }
}
