import { PersonCommonFieldPropertiesResourceModel } from "./person-common-field-properties.resource-model";

/**
 * Resource model for email address field
 */
export interface EmailAddressFieldResourceModel extends PersonCommonFieldPropertiesResourceModel {
    emailAddress: string;
}
