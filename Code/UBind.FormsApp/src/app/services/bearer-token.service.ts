import { Injectable } from '@angular/core';
import { SessionDataManager } from '@app/storage/session-data-manager';
import { ApplicationService } from './application.service';
import { storageHelper } from '@app/helpers/storage.helper';

/**
 * This class handles the token management in the local/session storage.
 */
@Injectable({
    providedIn: 'root',
})
export class BearerTokenService {

    private sessionDataManager: SessionDataManager = new SessionDataManager();

    public constructor(
        private applicationService: ApplicationService,
    ) {
    }

    public getToken(): string {
        let accessToken: string = null;
        const uBindToken: string = (this.sessionDataManager.getTenantOrganisationSessionValue(
            storageHelper.user.accessToken,
            this.applicationService.tenantAlias,
            this.applicationService.portalOrganisationAlias)) as string;
        if (uBindToken) {
            accessToken = uBindToken.replace(/\"/g, '');
        }

        return accessToken;
    }
}
