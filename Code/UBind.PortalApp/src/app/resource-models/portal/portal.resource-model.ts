import { Entity } from '../../models/entity';
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { AdditionalPropertyValueUpsertResourceModel } from '../additional-property.resource-model';
import { PortalUserType } from '@app/models/portal-user-type.enum';

/**
 * Resource model for a portal
 */
export interface PortalResourceModel extends Entity {
    name: string;
    alias: string;
    title: string;
    stylesheetUrl: string;
    styles: string;
    deleted: boolean;
    disabled: boolean;
    isDefault: boolean;
    createdDateTime: string;
    createdTicksSinceEpoch: number;
    lastModifiedDateTime: string;
    lastModifiedTicksSinceEpoch: number;
    tenantId: string;
    tenantName: string;
    userType: PortalUserType;
    organisationId: string;
    productionUrl: string;
    stagingUrl: string;
    developmentUrl: string;
    defaultUrl?: string;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
    properties?: Array<AdditionalPropertyValueUpsertResourceModel>;
}

/**
 * Request model for a portal, usually used to create or edit
 */
export class PortalRequestModel implements PortalResourceModel {
    public constructor(portal: PortalResourceModel) {
        this.tenant = this.tenantId = portal.tenantId;
        this.organisation = this.organisationId = portal.organisationId;
        this.userType = portal.userType;
        this.id = portal.id;
        this.name = portal.name;
        this.alias = portal.alias;
        this.title = portal.title;
        this.stylesheetUrl = portal.stylesheetUrl;
        this.styles = portal.styles;
        this.deleted = portal.deleted;
        this.disabled = portal.disabled;
        this.isDefault = portal.isDefault;
        this.createdDateTime = portal.createdDateTime;
        this.createdTicksSinceEpoch = portal.createdTicksSinceEpoch;
        this.lastModifiedDateTime = portal.lastModifiedDateTime;
        this.lastModifiedTicksSinceEpoch = portal.lastModifiedTicksSinceEpoch;
        this.tenantName = portal.tenantName;
        this.productionUrl = portal.productionUrl;
        this.stagingUrl = portal.stagingUrl;
        this.developmentUrl = portal.developmentUrl;
        this.properties = portal.properties;
        this.additionalPropertyValues = portal.additionalPropertyValues;
    }

    public tenantId: string;
    public organisationId: string;
    public id: string;
    public name: string;
    public alias: string;
    public title: string;
    public stylesheetUrl: string;
    public styles: string;
    public deleted: boolean;
    public disabled: boolean;
    public isDefault: boolean;
    public createdDateTime: string;
    public createdTicksSinceEpoch: number;
    public lastModifiedDateTime: string;
    public lastModifiedTicksSinceEpoch: number;
    public tenant: string;
    public tenantName: string;
    public userType: PortalUserType;
    public organisation: string;
    public productionUrl: string;
    public stagingUrl: string;
    public developmentUrl: string;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;
    public properties?: Array<AdditionalPropertyValueUpsertResourceModel>;
}

/**
 * Resource model for Portal details.
 */
export interface PortalDetailResourceModel extends PortalResourceModel {
    organisationName: string;
}
