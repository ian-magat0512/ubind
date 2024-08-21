
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { Entity } from '@app/models/entity';
import { AdditionalPropertyValueUpsertResourceModel } from './additional-property.resource-model';

/**
 * Product resource model
 */
export interface ProductResourceModel extends Entity {
    name: string;
    status: string;
    deleted: boolean;
    hasClaimComponent: boolean;
    disabled: boolean;
    createdDateTime: string;
    createdTicksSinceEpoch: number;
    lastModifiedDateTime: string;
    lastModifiedTicksSinceEpoch: number;
    tenantId: string;
    tenantName: string;
    alias: string;
    quoteExpirySettings: QuoteExpirySettingsResourceModel;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
}

/**
 * Model for adding a new product
 */
export interface ProductCreateRequestResourceModel {
    alias: string;
    name: string;
    tenant: string;
    properties?: Array<AdditionalPropertyValueUpsertResourceModel>;
}

/**
 * Model for updating a product
 */
export class ProductUpdateRequestResourceModel implements ProductResourceModel {
    public constructor(productResourceModel: ProductResourceModel) {
        this.name = productResourceModel.name;
        this.status = productResourceModel.status;
        this.deleted = productResourceModel.deleted;
        this.hasClaimComponent = productResourceModel.hasClaimComponent;
        this.disabled = productResourceModel.disabled;
        this.createdDateTime = productResourceModel.createdDateTime;
        this.createdTicksSinceEpoch = productResourceModel.createdTicksSinceEpoch;
        this.lastModifiedDateTime = productResourceModel.lastModifiedDateTime;
        this.lastModifiedTicksSinceEpoch = productResourceModel.lastModifiedTicksSinceEpoch;
        this.tenant = this.tenantId = productResourceModel.tenantId;
        this.tenantName = productResourceModel.tenantName;
        this.alias = productResourceModel.alias;
        this.quoteExpirySettings = productResourceModel.quoteExpirySettings;
        this.additionalPropertyValues = productResourceModel.additionalPropertyValues;
        this.id = productResourceModel.id;
    }

    public name: string;
    public status: string;
    public deleted: boolean;
    public hasClaimComponent: boolean;
    public disabled: boolean;
    public createdDateTime: string;
    public createdTicksSinceEpoch: number;
    public lastModifiedDateTime: string;
    public lastModifiedTicksSinceEpoch: number;
    public tenantId: string;
    public tenantName: string;
    public alias: string;
    public quoteExpirySettings: QuoteExpirySettingsResourceModel;
    public additionalPropertyValues: Array<AdditionalPropertyValue>;
    public id: string;
    public tenant: string;
}

/**
 * Model for settings for when quotes should expire
 */
export interface QuoteExpirySettingsResourceModel {
    enabled: boolean;
    expiryDays: number;
    updateType: QuoteExpiryUpdateType;
}

export enum QuoteExpiryUpdateType {
    UpdateNone, //  pdate no quotes
    UpdateAllWithoutExpiryOnly, // update quotes that doesnt have expiry 
    // update quotes that automatically has expiry date because of the quote 
    // expiry setting ( that means exclude to update quotes that has explicitly expiry date ) 
    UpdateAllExceptExplicitSet,
    UpdateAllQuotes, // update all quotes 
}
