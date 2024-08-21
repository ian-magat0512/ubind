import { Entity } from "@app/models/entity";
import { AdditionalPropertyValue } from "@app/models/additional-property-item-view.model";
import { AdditionalPropertyValueUpsertResourceModel } from "../additional-property.resource-model";
import {
    OrganisationLinkedIdentity,
    OrganisationLinkedIdentityUpsertModel,
} from "./organisation-linked-identity.resource-model";

/**
 * Resource model
 */
export interface BaseOrganisationResourceModel {
    name: string;
    alias: string;
}

/**
 * Resource model
 */
export interface OrganisationResourceModel extends BaseOrganisationResourceModel, Entity {
    tenantId: string;
    createdDateTime: string;
    createdTicksSinceEpoch: number;
    lastModifiedDateTime: string;
    lastModifiedTicksSinceEpoch: number;
    isDefault: boolean;
    status: string;
    isActive: boolean;
    managingOrganisationId?: string;
    managingOrganisationName?: string;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
    linkedIdentities: Array<OrganisationLinkedIdentity>;
}

/**
 * Resource model for adding/updating an organisation
 */
export interface UpsertOrganisationResourceModel extends BaseOrganisationResourceModel {
    tenant: string;
    properties?: Array<AdditionalPropertyValueUpsertResourceModel>;
    managingOrganisation: string;
    linkedIdentities: Array<OrganisationLinkedIdentityUpsertModel>;
}
