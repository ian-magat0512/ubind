import { Entity } from "../models/entity";
/**
 * This class used to manage DKIM settings.
 * DKIM settings are used to sign emails sent from the system.
 */
export interface DkimSettingsResourceModel  extends Entity {
    tenantId: string;
    organisationId: string;
    domainName: string;
    privateKey: string;
    dnsSelector: string;
    agentOrUserIdentifier: string;
    applicableDomainNameList: Array<string>;
    applicableDomainNames: string;
}

/**
 * A model for creating or updating a DKIM setting.
 * DKIM settings are used to sign emails sent from the system.
 */
export interface DkimSettingsUpsertModel {
    tenant: string;
    id?: string;
    organisationId?: string;
    domainName: string;
    privateKey: string;
    dnsSelector: string;
    agentOrUserIdentifier: string;
    applicableDomainNameList: Array<string>;
}
