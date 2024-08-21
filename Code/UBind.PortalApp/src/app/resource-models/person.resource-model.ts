import { AdditionalPropertyValue } from "@app/models/additional-property-item-view.model";
import { UserStatus } from "../models";
import { AdditionalPropertyValueUpsertResourceModel } from "./additional-property.resource-model";
import { EmailAddressFieldResourceModel } from "./person/email-address-field.resource-model";
import { MessengerIdFieldResourceModel } from "./person/messenger-id-field.resource-model";
import { PhoneNumberFieldResourceModel } from "./person/phone-number-field.resource-model";
import { SocialMediaIdFieldResourceModel } from "./person/social-field.resource-model";
import { StreetAddressFieldResourceModel } from "./person/street-address-field.resource-model";
import { WebsiteAddressFieldResourceModel } from "./person/website-address-field.resource-model";

/**
 * Resource model for person
 */
export interface PersonResourceModel {
    tenantId: string;
    id: string;
    customerId: string;
    blocked: boolean;
    organisationId: string;
    organisationName: string;
    environment: string;
    preferredName: string;
    namePrefix: string;
    firstName: string;
    middleNames: string;
    lastName: string;
    nameSuffix: string;
    company: string;
    title: string;
    fullName: string;
    displayName: string;
    email: string;
    hasActivePolicies: boolean;
    userStatus: UserStatus;
    picture: string;
    userType: string;
    ownerId: string;
    ownerFullName: string;
    pictureId: string;
    createdDateTime: string;
    lastModifiedDateTime: string;
    emailAddresses: Array<EmailAddressFieldResourceModel>;
    phoneNumbers: Array<PhoneNumberFieldResourceModel>;
    streetAddresses: Array<StreetAddressFieldResourceModel>;
    websiteAddresses: Array<WebsiteAddressFieldResourceModel>;
    messengerIds: Array<MessengerIdFieldResourceModel>;
    socialMediaIds: Array<SocialMediaIdFieldResourceModel>;
    properties?: Array<AdditionalPropertyValueUpsertResourceModel>;
    additionalPropertyValues?: Array<AdditionalPropertyValue>;
}
export interface Person {
    fullName: string;
    namePrefix: string;
    firstName: string;
    middleNames: string;
    lastName: string;
    displayName: string;
    nameSuffix: string;
    company: string;
    title: string;
    preferredName: string;
    email: string;
    alternativeEmail?: string;
    mobilePhone?: string;
    homePhone?: string;
    workPhone?: string;
    emailAddresses: Array<EmailAddressFieldResourceModel>;
    phoneNumbers: Array<PhoneNumberFieldResourceModel>;
    streetAddresses: Array<StreetAddressFieldResourceModel>;
    websiteAddresses: Array<WebsiteAddressFieldResourceModel>;
    messengerIds: Array<MessengerIdFieldResourceModel>;
    socialMediaIds: Array<SocialMediaIdFieldResourceModel>;
    properties?: Array<AdditionalPropertyValueUpsertResourceModel>;
}
/**
 * Model for updating an existing person
 */
export interface PersonUpdateResourceModel extends PersonCreateModel {
    id: string;
}

/**
 * Person create resource model
 */
export interface PersonCreateModel extends Person {
    tenantId: string;
    organisationId: string;
    customerId: string;
    userType: string;
    blocked: boolean;
    environment: string;
    hasActivePolicies: boolean;
    initialRoles?: Array<string>;
    /**
     * The tenant id or alias the user or customer would log into
     */
    tenant?: string;
    /**
     * The organisation id or alias the user or customer would log into
     */
    organisation?: string;
    /*
    * The portal id the user or customer would log into
    */
    portal?: string;

    /* The portal id the user or customer would log into */
    portalId?: string;
}

export interface PersonAccountCreateModel extends Person {
    userType: string;
}
