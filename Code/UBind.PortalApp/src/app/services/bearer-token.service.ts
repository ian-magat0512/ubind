import { Injectable } from '@angular/core';
import { AppConfigService } from '@app/services/app-config.service';
import { AppConfig } from '@app/models/app-config';
import { SessionDataManager } from '@app/storage/session-data-manager';
import { storageHelper } from '@app/helpers/storage.helper';

/**
 * This class handles the token management in the local/session storage.
 */
@Injectable({ providedIn: 'root' })
export class BearerTokenService {

    private appConfigTenantAlias: string;
    private portalOrganisationAlias: string;
    private sessionDataManager: SessionDataManager = new SessionDataManager();

    public constructor(
        private appConfigService: AppConfigService,
    ) {
        if (this.appConfigService.appConfigSubject) {
            this.appConfigService.appConfigSubject.subscribe((appConfig: AppConfig) => {
                this.appConfigTenantAlias = appConfig.portal.tenantAlias;
                this.portalOrganisationAlias = appConfig.portal.organisationAlias;
            });
        }
    }

    public getToken(): string {
        let accessToken: string = null;
        const uBindToken: string = this.sessionDataManager.getTenantOrganisationSessionValue(
            storageHelper.user.accessToken,
            this.appConfigTenantAlias,
            this.portalOrganisationAlias,
        );
        if (uBindToken) {
            accessToken = uBindToken.replace(/\"/g, '');
        }

        return accessToken;
    }
}
