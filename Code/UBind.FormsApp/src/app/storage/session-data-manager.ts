import { Injectable } from '@angular/core';
import { ResilientStorage } from '@app/storage/resilient-storage';

/**
 * This class manage the getting the session of the tenant per organisation.
 */
@Injectable({
    providedIn: 'root',
})
export class SessionDataManager {
    private storage: ResilientStorage = new ResilientStorage();

    public constructor() {
    }

    public getQuoteSessionValue(key: string, keyPrefix: string): string {
        if (this.quoteSessionIsExist(keyPrefix)) {
            const keyObject: any = JSON.parse(
                this.storage.getItem(keyPrefix)
                || '{}');
            if (Object.keys(keyObject).length) {
                return keyObject[key];
            }

            return '';
        }

        return '';
    }

    public removeQuoteSessionValue(key: string, keyPrefix: string): void {
        if (this.quoteSessionIsExist(keyPrefix)) {
            const keyObject: any = JSON.parse(
                this.storage.getItem(keyPrefix) || '{}');
            if (Object.keys(keyObject).length) {
                delete keyObject[key];

                if (Object.keys(keyObject).length > 1) {
                    this.storage.setItem(keyPrefix, JSON.stringify(keyObject));
                } else {
                    this.storage.removeItem(keyPrefix);
                }
            }
        }
    }

    public getTenantOrganisationSessionValue<T>(key: string, tenantAlias: string, organisationAlias: string): T {
        if (!this.tenantAndOrganisationSessionExists(tenantAlias, organisationAlias)) {
            return null;
        }

        const keyObject: any = JSON.parse(
            this.storage.getItem(tenantAlias + " - " + organisationAlias)
            || '{}');
        if (Object.keys(keyObject).length) {
            return keyObject[key] as unknown as T;
        }
    }

    public tenantAndOrganisationSessionExists(tenantAlias: string, organisationAlias: string): boolean {
        return this.storage.getItem(tenantAlias + " - " + organisationAlias) ? true : false;
    }

    public quoteSessionIsExist(key: string): boolean {
        return this.storage.getItem(key) ? true : false;
    }
}
