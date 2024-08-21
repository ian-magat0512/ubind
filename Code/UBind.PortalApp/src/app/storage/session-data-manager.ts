import { Injectable } from '@angular/core';
import { ResilientStorage } from '@app/storage/resilient-storage';

/**
 * This class manages the session data manager of the tenant per organisation.
 */
@Injectable({ providedIn: 'root' })
export class SessionDataManager {
    private storage: ResilientStorage = new ResilientStorage();

    public constructor() {
    }

    public getTenantOrganisationSessionValue<T>(key: string, tenantAlias: string, organisationAlias: string): T {
        if (!this.tenantAndOrganisationSessionExists(tenantAlias, organisationAlias)) {
            return null;
        }

        const keyObject: any = JSON.parse(
            this.storage.getItem(tenantAlias + " - " + organisationAlias)
            || '{}',
        );
        if (Object.keys(keyObject).length) {
            return keyObject[key] as unknown as T;
        }
    }

    public updateTenantOrganisationSessionValue<T>(
        key: string,
        newValue: T,
        tenantAlias: string,
        organisationAlias: string,
    ): void {
        if (this.tenantAndOrganisationSessionExists(tenantAlias, organisationAlias)) {
            const keyObject: any = JSON.parse(
                this.storage.getItem(tenantAlias + " - " + organisationAlias) || '{}',
            );
            if (Object.keys(keyObject).length) {
                keyObject[key] = newValue;
                this.storage.setItem(tenantAlias + " - " + organisationAlias, JSON.stringify(keyObject));
            }
        } else {
            let sessionObject: any = {
                [key]: newValue,
            };
            this.storage.setItem(tenantAlias + " - " + organisationAlias, JSON.stringify(sessionObject));
        }
    }

    public removeTenantOrganisationSessionValue(key: string, tenantAlias: string, organisationAlias: string): void {
        if (this.tenantAndOrganisationSessionExists(tenantAlias, organisationAlias)) {
            const keyObject: any = JSON.parse(
                this.storage.getItem(tenantAlias + " - " + organisationAlias) || '{}',
            );
            if (Object.keys(keyObject).length) {
                delete keyObject[key];
                this.storage.setItem(tenantAlias + " - " + organisationAlias, JSON.stringify(keyObject));
            }
        }
    }

    public tenantAndOrganisationSessionExists(tenantAlias: string, organisationAlias: string): boolean {
        return this.storage.getItem(tenantAlias + " - " + organisationAlias) ? true : false;
    }
}
