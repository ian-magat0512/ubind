import { PersonCommonFieldPropertiesResourceModel } from "./person-common-field-properties.resource-model";

/**
 * Resource model for messenger address field
 */
export interface MessengerIdFieldResourceModel extends PersonCommonFieldPropertiesResourceModel {
    messengerId: string;
}
