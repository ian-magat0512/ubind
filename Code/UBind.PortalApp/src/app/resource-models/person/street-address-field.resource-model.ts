import { PersonCommonFieldPropertiesResourceModel } from "./person-common-field-properties.resource-model";

/**
 * Resource model for address field
 */
export interface StreetAddressFieldResourceModel extends PersonCommonFieldPropertiesResourceModel {
    address: string;
    suburb: string;
    state: string;
    postcode: string;
}
