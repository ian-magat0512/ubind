import { PersonCommonFieldPropertiesResourceModel } from "./person-common-field-properties.resource-model";

/**
 * Resource model for web address field
 */
export interface WebsiteAddressFieldResourceModel extends PersonCommonFieldPropertiesResourceModel {
    websiteAddress: string;
}
