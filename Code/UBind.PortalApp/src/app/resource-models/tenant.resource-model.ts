import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { Entity } from '@app/models/entity';

/**
 * Tenant resource model
 */
export interface TenantResourceModel extends Entity {
    name: string;
    alias: string;
    customDomain: string;
    deleted: boolean;
    disabled: boolean;
    createdDateTime: string;
    lastModifiedDateTime: string;
    masterTenant: string;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
    organisationId: string;
    defaultOrganisationId: string;
}
