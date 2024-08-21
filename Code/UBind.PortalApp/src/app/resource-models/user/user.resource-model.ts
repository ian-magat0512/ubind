import { Entity } from "../../models/entity";
import { AdditionalPropertyValue } from '@app/models/additional-property-item-view.model';
import { UserLinkedIdentity } from "./user-linked-identity.resource-model";
import { PersonResourceModel } from "../person.resource-model";
/**
 * A user resource model
 */
export interface UserResourceModel extends PersonResourceModel, Entity {
    id: string;
    personId: string;
    profilePictureId: string;
    blocked: boolean;
    status: string;
    additionalPropertyValues: Array<AdditionalPropertyValue>;
    createdDateTime: string;
    lastModifiedDateTime: string;
    portalId: string;
    portalName: string;
    linkedIdentities: Array<UserLinkedIdentity>;
}

/**
 * A user signup resource model
 */
export interface UserSignUpResourceModel extends PersonResourceModel {
}

/**
 * A user update resource model
 */
export interface UserUpdateResourceModel extends UserSignUpResourceModel {
}
