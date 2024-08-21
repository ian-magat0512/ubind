import { PersonCommonFieldPropertiesResourceModel } from "./person-common-field-properties.resource-model";

/**
 * Resource model for phone number field
 */
export interface PhoneNumberFieldResourceModel extends PersonCommonFieldPropertiesResourceModel {
    phoneNumber: string;
}
